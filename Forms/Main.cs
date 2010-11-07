// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.ApplicationServices;

    internal partial class Main : GlassForm
    {
        private struct FindNewViewData
        {
            public Guid ProviderID;
            public object View;
        }

        private Data progData;
        private ViewState view;
        private UpdateCheck checkUpdate;
        private TaskbarNotify tbarNotif;

        private Thread searchThread;
        private object searchThreadLock = new object();

        private Dictionary<int, string> downloadColNames = new Dictionary<int, string>();
        private Dictionary<int, int> downloadColSizes = new Dictionary<int, int>();
        private List<Data.DownloadCols> downloadColOrder = new List<Data.DownloadCols>();

        private bool windowPosLoaded;

        public Main()
        {
            this.InitializeComponent();
        }

        public void UpdateTrayStatus(bool active)
        {
            if (OsUtils.WinSevenOrLater())
            {
                if (this.progData.CountDownloadsErrored() > 0)
                {
                    this.tbarNotif.SetOverlayIcon(this, Properties.Resources.overlay_error, "Error");
                    this.tbarNotif.SetThumbnailTooltip(this, this.Text + ": Error");
                }
                else
                {
                    if (active == true)
                    {
                        this.tbarNotif.SetOverlayIcon(this, Properties.Resources.overlay_downloading, "Downloading");
                        this.tbarNotif.SetThumbnailTooltip(this, this.Text + ": Downloading");
                    }
                    else
                    {
                        this.tbarNotif.SetOverlayIcon(this, null, string.Empty);
                        this.tbarNotif.SetThumbnailTooltip(this, null);
                    }
                }
            }

            if (this.nicTrayIcon.Visible)
            {
                if (this.progData.CountDownloadsErrored() > 0)
                {
                    this.nicTrayIcon.Icon = Properties.Resources.icon_error;
                    this.nicTrayIcon.Text = this.Text + ": Error";
                }
                else
                {
                    if (active == true)
                    {
                        this.nicTrayIcon.Icon = Properties.Resources.icon_working;
                        this.nicTrayIcon.Text = this.Text + ": Downloading";
                    }
                    else
                    {
                        this.nicTrayIcon.Icon = Properties.Resources.icon_main;
                        this.nicTrayIcon.Text = this.Text;
                    }
                }
            }
        }

        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    e.Handled = true;
                    this.mnuHelpShowHelp_Click(sender, e);
                    break;
                case Keys.Delete:
                    if (this.tbtDelete.Visible)
                    {
                        e.Handled = true;
                        this.tbtDelete_Click();
                    }
                    else if (this.tbtCancel.Visible)
                    {
                        e.Handled = true;
                        tbtCancel_Click();
                    }

                    break;
                case Keys.Back:
                    if (!object.ReferenceEquals(this.ActiveControl.GetType(), typeof(TextBox)) & !object.ReferenceEquals(this.ActiveControl.Parent.GetType(), typeof(ExtToolStrip)))
                    {
                        if (e.Shift)
                        {
                            if (this.tbtForward.Enabled)
                            {
                                e.Handled = true;
                                this.tbtForward_Click(sender, e);
                            }
                        }
                        else
                        {
                            if (this.tbtBack.Enabled)
                            {
                                e.Handled = true;
                                this.tbtBack_Click(sender, e);
                            }
                        }
                    }

                    break;
                case Keys.BrowserBack:
                    if (this.tbtBack.Enabled)
                    {
                        e.Handled = true;
                        this.tbtBack_Click(sender, e);
                    }

                    break;
                case Keys.BrowserForward:
                    if (this.tbtForward.Enabled)
                    {
                        e.Handled = true;
                        this.tbtForward_Click(sender, e);
                    }

                    break;
            }
        }

        private void Main_Load(object eventSender, System.EventArgs eventArgs)
        {
            // If this is the first run of a new version of the application, then upgrade the settings from the old version.
            try
            {
                if (Properties.Settings.Default.UpgradeSettings)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeSettings = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch (ConfigurationErrorsException configErrorExp)
            {
                string fileName = null;

                if (configErrorExp.Filename != null)
                {
                    fileName = configErrorExp.Filename;
                }
                else if (configErrorExp.InnerException != null && configErrorExp.InnerException is ConfigurationErrorsException)
                {
                    ConfigurationErrorsException innerExp = (ConfigurationErrorsException)configErrorExp.InnerException;

                    if (innerExp.Filename != null)
                    {
                        fileName = innerExp.Filename;
                    }
                }

                if (fileName != null)
                {
                    File.Delete(fileName);
                    Interaction.MsgBox("Your Radio Downloader configuration file has been reset as it was corrupt.  This only affects your settings and columns, not your subscriptions or downloads." + Environment.NewLine + Environment.NewLine + "You will need to start Radio Downloader again after closing this message.", MsgBoxStyle.Information);
                    this.Close();
                    this.Dispose();
                    return;
                }
                else
                {
                    throw;
                }
            }

            // Make sure that the temp and application data folders exist
            Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"));
            Directory.CreateDirectory(FileUtils.GetAppDataFolder());

            // Make sure that the database exists.  If not, then copy across the empty database from the program's folder.
            System.IO.FileInfo fileExits = new System.IO.FileInfo(Path.Combine(FileUtils.GetAppDataFolder(), "store.db"));

            if (fileExits.Exists == false)
            {
                try
                {
                    System.IO.File.Copy(Path.Combine(new ApplicationBase().Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"));
                }
                catch (FileNotFoundException)
                {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(new ApplicationBase().Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }
            }
            else
            {
                // As the database already exists, copy the specimen database across from the program folder
                // and then make sure that the current db's structure matches it.
                try
                {
                    System.IO.File.Copy(Path.Combine(new ApplicationBase().Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), true);
                }
                catch (FileNotFoundException)
                {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(new ApplicationBase().Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    Interaction.MsgBox("Access was denied when attempting to copy the Radio Downloader template database." + Environment.NewLine + Environment.NewLine + "Check that you have read access to '" + Path.Combine(new ApplicationBase().Info.DirectoryPath, "store.db") + "' and write access to '" + Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db") + "'.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }

                using (UpdateDB doDbUpdate = new UpdateDB(Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db")))
                {
                    doDbUpdate.UpdateStructure();
                }
            }

            this.imlListIcons.Images.Add("downloading", Properties.Resources.list_downloading);
            this.imlListIcons.Images.Add("waiting", Properties.Resources.list_waiting);
            this.imlListIcons.Images.Add("converting", Properties.Resources.list_converting);
            this.imlListIcons.Images.Add("downloaded_new", Properties.Resources.list_downloaded_new);
            this.imlListIcons.Images.Add("downloaded", Properties.Resources.list_downloaded);
            this.imlListIcons.Images.Add("subscribed", Properties.Resources.list_subscribed);
            this.imlListIcons.Images.Add("error", Properties.Resources.list_error);
            this.imlListIcons.Images.Add("favourite", Properties.Resources.list_favourite);

            this.imlProviders.Images.Add("default", Properties.Resources.provider_default);

            this.imlToolbar.Images.Add("choose_programme", Properties.Resources.toolbar_choose_programme);
            this.imlToolbar.Images.Add("clean_up", Properties.Resources.toolbar_clean_up);
            this.imlToolbar.Images.Add("current_episodes", Properties.Resources.toolbar_current_episodes);
            this.imlToolbar.Images.Add("delete", Properties.Resources.toolbar_delete);
            this.imlToolbar.Images.Add("download", Properties.Resources.toolbar_download);
            this.imlToolbar.Images.Add("help", Properties.Resources.toolbar_help);
            this.imlToolbar.Images.Add("options", Properties.Resources.toolbar_options);
            this.imlToolbar.Images.Add("play", Properties.Resources.toolbar_play);
            this.imlToolbar.Images.Add("report_error", Properties.Resources.toolbar_report_error);
            this.imlToolbar.Images.Add("retry", Properties.Resources.toolbar_retry);
            this.imlToolbar.Images.Add("subscribe", Properties.Resources.toolbar_subscribe);
            this.imlToolbar.Images.Add("unsubscribe", Properties.Resources.toolbar_unsubscribe);
            this.imlToolbar.Images.Add("add_favourite", Properties.Resources.toolbar_add_favourite);
            this.imlToolbar.Images.Add("remove_favourite", Properties.Resources.toolbar_remove_favourite);

            this.tbrToolbar.ImageList = this.imlToolbar;
            this.tbrHelp.ImageList = this.imlToolbar;
            this.lstProviders.LargeImageList = this.imlProviders;
            this.lstFavourites.SmallImageList = this.imlListIcons;
            this.lstSubscribed.SmallImageList = this.imlListIcons;
            this.lstDownloads.SmallImageList = this.imlListIcons;

            this.lstEpisodes.Columns.Add("Date", Convert.ToInt32(0.179 * this.lstEpisodes.Width));
            this.lstEpisodes.Columns.Add("Episode Name", Convert.ToInt32(0.786 * this.lstEpisodes.Width));
            this.lstFavourites.Columns.Add("Programme Name", Convert.ToInt32(0.661 * this.lstFavourites.Width));
            this.lstFavourites.Columns.Add("Provider", Convert.ToInt32(0.304 * this.lstFavourites.Width));
            this.lstSubscribed.Columns.Add("Programme Name", Convert.ToInt32(0.482 * this.lstSubscribed.Width));
            this.lstSubscribed.Columns.Add("Last Download", Convert.ToInt32(0.179 * this.lstSubscribed.Width));
            this.lstSubscribed.Columns.Add("Provider", Convert.ToInt32(0.304 * this.lstSubscribed.Width));

            // NB - these are defined in alphabetical order to save sorting later
            this.downloadColNames.Add((int)Data.DownloadCols.EpisodeDate, "Date");
            this.downloadColNames.Add((int)Data.DownloadCols.Duration, "Duration");
            this.downloadColNames.Add((int)Data.DownloadCols.EpisodeName, "Episode Name");
            this.downloadColNames.Add((int)Data.DownloadCols.Progress, "Progress");
            this.downloadColNames.Add((int)Data.DownloadCols.Status, "Status");

            this.view = new ViewState();
            this.view.UpdateNavBtnState += this.view_UpdateNavBtnState;
            this.view.ViewChanged += this.view_ViewChanged;
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);

            this.progData = Data.GetInstance();
            this.progData.ProviderAdded += this.progData_ProviderAdded;
            this.progData.ProgrammeUpdated += this.progData_ProgrammeUpdated;
            this.progData.EpisodeAdded += this.progData_EpisodeAdded;
            this.progData.FavouriteAdded += this.progData_FavouriteAdded;
            this.progData.FavouriteUpdated += this.progData_FavouriteUpdated;
            this.progData.FavouriteRemoved += this.progData_FavouriteRemoved;
            this.progData.SubscriptionAdded += this.progData_SubscriptionAdded;
            this.progData.SubscriptionUpdated += this.progData_SubscriptionUpdated;
            this.progData.SubscriptionRemoved += this.progData_SubscriptionRemoved;
            this.progData.DownloadAdded += this.progData_DownloadAdded;
            this.progData.DownloadProgress += this.progData_DownloadProgress;
            this.progData.DownloadRemoved += this.progData_DownloadRemoved;
            this.progData.DownloadUpdated += this.progData_DownloadUpdated;
            this.progData.DownloadProgressTotal += this.progData_DownloadProgressTotal;
            this.progData.FindNewViewChange += this.progData_FindNewViewChange;
            this.progData.FoundNew += this.progData_FoundNew;

            this.progData.InitProviderList();
            this.InitFavouriteList();
            this.progData.InitSubscriptionList();
            this.InitDownloadList();

            this.lstFavourites.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Favourite);
            this.lstSubscribed.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Subscription);
            this.lstDownloads.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Download);

            if (OsUtils.WinSevenOrLater())
            {
                // New style taskbar - initialise the taskbar notification class
                this.tbarNotif = new TaskbarNotify();
            }

            if (!OsUtils.WinSevenOrLater() | Properties.Settings.Default.CloseToSystray)
            {
                // Show a system tray icon
                this.nicTrayIcon.Visible = true;
            }

            // Set up the initial notification status
            this.UpdateTrayStatus(false);

            this.checkUpdate = new UpdateCheck("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=" + new ApplicationBase().Info.Version.ToString());

            this.picSideBarBorder.Width = 2;

            this.lstProviders.Dock = DockStyle.Fill;
            this.pnlPluginSpace.Dock = DockStyle.Fill;
            this.lstEpisodes.Dock = DockStyle.Fill;
            this.lstFavourites.Dock = DockStyle.Fill;
            this.lstSubscribed.Dock = DockStyle.Fill;
            this.lstDownloads.Dock = DockStyle.Fill;

            this.Font = SystemFonts.MessageBoxFont;
            this.lblSideMainTitle.Font = new Font(this.Font.FontFamily, Convert.ToSingle(this.Font.SizeInPoints * 1.16), this.Font.Style, GraphicsUnit.Point);

            // Scale the max size of the sidebar image for values other than 96 dpi, as it is specified in pixels
            using (Graphics graphicsForDpi = this.CreateGraphics())
            {
                this.picSidebarImg.MaximumSize = new Size(Convert.ToInt32(this.picSidebarImg.MaximumSize.Width * (graphicsForDpi.DpiX / 96)), Convert.ToInt32(this.picSidebarImg.MaximumSize.Height * (graphicsForDpi.DpiY / 96)));
            }

            if (Properties.Settings.Default.MainFormPos != Rectangle.Empty)
            {
                if (OsUtils.VisibleOnScreen(Properties.Settings.Default.MainFormPos))
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.DesktopBounds = Properties.Settings.Default.MainFormPos;
                }
                else
                {
                    this.Size = Properties.Settings.Default.MainFormPos.Size;
                }

                this.WindowState = Properties.Settings.Default.MainFormState;
            }

            this.windowPosLoaded = true;

            this.tblToolbars.Height = this.tbrToolbar.Height;
            this.tbrToolbar.SetWholeDropDown(this.tbtOptionsMenu);
            this.tbrHelp.SetWholeDropDown(this.tbtHelpMenu);
            this.tbrHelp.Width = this.tbtHelpMenu.Rectangle.Width;

            if (this.WindowState != FormWindowState.Minimized)
            {
                this.tblToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, this.tblToolbars.Width - (this.tbtHelpMenu.Rectangle.Width + this.tbrHelp.Margin.Right));
                this.tblToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, this.tbtHelpMenu.Rectangle.Width + this.tbrHelp.Margin.Right);
            }

            if (OsUtils.WinVistaOrLater() & VisualStyleRenderer.IsSupported)
            {
                this.tbrView.Margin = new Padding(0);
            }

            this.SetGlassMargins(0, 0, this.tbrView.Height, 0);
            this.tbrView.Renderer = new TabBarRenderer();

            OsUtils.ApplyRunOnStartup();

            this.progData.StartDownload();
            this.tmrCheckForUpdates.Enabled = true;
        }

        private void Main_FormClosing(object eventSender, System.Windows.Forms.FormClosingEventArgs eventArgs)
        {
            if (eventArgs.CloseReason == CloseReason.UserClosing)
            {
                if (!this.nicTrayIcon.Visible)
                {
                    if (this.WindowState != FormWindowState.Minimized)
                    {
                        this.WindowState = FormWindowState.Minimized;
                        eventArgs.Cancel = true;
                    }
                }
                else
                {
                    OsUtils.TrayAnimate(this, true);
                    this.Visible = false;
                    eventArgs.Cancel = true;

                    if (Properties.Settings.Default.ShownTrayBalloon == false)
                    {
                        this.nicTrayIcon.BalloonTipIcon = ToolTipIcon.Info;
                        this.nicTrayIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + Environment.NewLine + "Click here to hide this message in future.";
                        this.nicTrayIcon.BalloonTipTitle = "Radio Downloader is still running";
                        this.nicTrayIcon.ShowBalloonTip(30000);
                    }
                }
            }
        }

        private void lstProviders_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.lstProviders.SelectedItems.Count > 0)
            {
                Guid pluginId = new Guid(this.lstProviders.SelectedItems[0].Name);
                this.ShowProviderInfo(pluginId);
            }
            else
            {
                this.SetViewDefaults();
            }
        }

        private void ShowProviderInfo(Guid providerId)
        {
            Data.ProviderData info = this.progData.FetchProviderData(providerId);
            this.SetSideBar(info.name, info.description, null);

            if (this.view.CurrentView == ViewState.View.FindNewChooseProvider)
            {
                this.SetToolbarButtons(new ToolBarButton[] { this.tbtChooseProgramme });
            }
        }

        private void lstProviders_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.lstProviders.SelectedItems.Count == 0)
            {
                return;
            }

            this.tbtChooseProgramme_Click();
        }

        private void lstEpisodes_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            this.progData.EpisodeSetAutoDownload(Convert.ToInt32(this.lstEpisodes.Items[e.Index].Name, CultureInfo.InvariantCulture), e.NewValue == CheckState.Checked);
        }

        private void ShowEpisodeInfo(int epid)
        {
            Data.ProgrammeData progInfo = this.progData.FetchProgrammeData((int)this.view.CurrentViewData);
            Data.EpisodeData epInfo = this.progData.FetchEpisodeData(epid);
            string infoText = string.Empty;

            if (epInfo.description != null)
            {
                infoText += epInfo.description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + epInfo.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(epInfo.duration);

            this.SetSideBar(epInfo.name, infoText, this.progData.FetchEpisodeImage(epid));

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.tbtDownload);

            if (progInfo.favourite)
            {
                buttons.Add(this.tbtRemFavourite);
            }
            else
            {
                buttons.Add(this.tbtAddFavourite);
            }

            if (progInfo.subscribed)
            {
                buttons.Add(this.tbtUnsubscribe);
            }
            else if (!progInfo.singleEpisode)
            {
                buttons.Add(this.tbtSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
        }

        private void lstEpisodes_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.lstEpisodes.SelectedItems.Count > 0)
            {
                int epid = Convert.ToInt32(this.lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                this.ShowEpisodeInfo(epid);
            }
            else
            {
                this.SetViewDefaults(); // Revert back to programme info in sidebar
            }
        }

        private void lstFavourites_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.lstFavourites.SelectedItems.Count > 0)
            {
                int progid = Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                this.progData.UpdateProgInfoIfRequired(progid);
                this.ShowFavouriteInfo(progid);
            }
            else
            {
                this.SetViewDefaults(); // Revert back to subscribed items view default sidebar and toolbar
            }
        }

        private void ShowFavouriteInfo(int progid)
        {
            Data.FavouriteData info = this.progData.FetchFavouriteData(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.AddRange(new ToolBarButton[] { this.tbtRemFavourite, this.tbtCurrentEps });

            if (info.subscribed)
            {
                buttons.Add(this.tbtUnsubscribe);
            }
            else if (!info.singleEpisode)
            {
                buttons.Add(this.tbtSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(info.name, info.description, this.progData.FetchProgrammeImage(progid));
        }

        private void lstSubscribed_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.lstSubscribed.SelectedItems.Count > 0)
            {
                int progid = Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                this.progData.UpdateProgInfoIfRequired(progid);
                this.ShowSubscriptionInfo(progid);
            }
            else
            {
                this.SetViewDefaults(); // Revert back to subscribed items view default sidebar and toolbar
            }
        }

        private void ShowSubscriptionInfo(int progid)
        {
            Data.SubscriptionData info = this.progData.FetchSubscriptionData(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.tbtUnsubscribe);
            buttons.Add(this.tbtCurrentEps);

            if (info.favourite)
            {
                buttons.Add(this.tbtRemFavourite);
            }
            else
            {
                buttons.Add(this.tbtAddFavourite);
            }

            this.SetToolbarButtons(buttons.ToArray());

            this.SetSideBar(info.name, info.description, this.progData.FetchProgrammeImage(progid));
        }

        private void lstDownloads_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            Data.DownloadCols clickedCol = this.downloadColOrder[e.Column];

            if (clickedCol == Data.DownloadCols.Progress)
            {
                return;
            }

            if (this.progData.DownloadSortByCol != clickedCol)
            {
                this.progData.DownloadSortByCol = clickedCol;
                this.progData.DownloadSortAscending = true;
            }
            else
            {
                this.progData.DownloadSortAscending = !this.progData.DownloadSortAscending;
            }

            // Set the column header to display the new sort order
            this.lstDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(this.progData.DownloadSortByCol), this.progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Properties.Settings.Default.DownloadColSortBy = (int)this.progData.DownloadSortByCol;
            Properties.Settings.Default.DownloadColSortAsc = this.progData.DownloadSortAscending;

            this.lstDownloads.Sort();
        }

        private void lstDownloads_ColumnReordered(object sender, System.Windows.Forms.ColumnReorderedEventArgs e)
        {
            string[] oldOrder = new string[this.lstDownloads.Columns.Count];

            // Fetch the pre-reorder column order
            foreach (ColumnHeader col in this.lstDownloads.Columns)
            {
                oldOrder[col.DisplayIndex] = ((int)this.downloadColOrder[col.Index]).ToString(CultureInfo.InvariantCulture);
            }

            List<string> newOrder = new List<string>(oldOrder);
            string moveCol = newOrder[e.OldDisplayIndex];

            // Re-order the data to match the new column order
            newOrder.RemoveAt(e.OldDisplayIndex);
            newOrder.Insert(e.NewDisplayIndex, moveCol);

            // Save the new column order to the preference
            Properties.Settings.Default.DownloadCols = Strings.Join(newOrder.ToArray(), ",");

            if (e.OldDisplayIndex == 0 || e.NewDisplayIndex == 0)
            {
                // The reorder involves column 0 which contains the icons, so re-initialise the list
                e.Cancel = true;
                this.InitDownloadList();
            }
        }

        private void lstDownloads_ColumnRightClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            this.mnuListHdrs.Show(this.lstDownloads, this.lstDownloads.PointToClient(Cursor.Position));
        }

        private void lstDownloads_ColumnWidthChanged(object sender, System.Windows.Forms.ColumnWidthChangedEventArgs e)
        {
            // Save the updated column's width
            this.downloadColSizes[(int)this.downloadColOrder[e.ColumnIndex]] = this.lstDownloads.Columns[e.ColumnIndex].Width;

            string saveColSizes = string.Empty;

            // Convert the stored column widths back to a string to save to settings
            foreach (KeyValuePair<int, int> colSize in this.downloadColSizes)
            {
                if (!string.IsNullOrEmpty(saveColSizes))
                {
                    saveColSizes += "|";
                }

                saveColSizes += colSize.Key.ToString(CultureInfo.InvariantCulture) + "," + (colSize.Value / this.CurrentAutoScaleDimensions.Width).ToString(CultureInfo.InvariantCulture);
            }

            Properties.Settings.Default.DownloadColSizes = saveColSizes;
        }

        private void lstDownloads_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.lstDownloads.SelectedItems.Count == 0)
            {
                return;
            }

            this.tbtPlay_Click();
        }

        private void lstDownloads_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.lstDownloads.SelectedItems.Count > 0)
            {
                this.ShowDownloadInfo(Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
            }
            else
            {
                this.SetViewDefaults(); // Revert back to downloads view default sidebar and toolbar
            }
        }

        private void ShowDownloadInfo(int epid)
        {
            Data.DownloadData info = this.progData.FetchDownloadData(epid);

            string infoText = string.Empty;

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.tbtCleanUp);

            if (info.description != null)
            {
                infoText += info.description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + info.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(info.duration);

            switch (info.status)
            {
                case Data.DownloadStatus.Downloaded:
                    if (File.Exists(info.downloadPath))
                    {
                        buttons.Add(this.tbtPlay);
                    }

                    buttons.Add(this.tbtDelete);
                    infoText += Environment.NewLine + "Play count: " + info.playCount.ToString(CultureInfo.CurrentCulture);

                    break;
                case Data.DownloadStatus.Errored:
                    string errorName = string.Empty;
                    string errorDetails = info.errorDetails;

                    buttons.Add(this.tbtRetry);
                    buttons.Add(this.tbtCancel);

                    switch (info.errorType)
                    {
                        case ErrorType.LocalProblem:
                            errorName = "Local problem";
                            break;
                        case ErrorType.ShorterThanExpected:
                            errorName = "Shorter than expected";
                            break;
                        case ErrorType.NotAvailable:
                            errorName = "Not available";
                            break;
                        case ErrorType.NotAvailableInLocation:
                            errorName = "Not available in your location";
                            break;
                        case ErrorType.NetworkProblem:
                            errorName = "Network problem";
                            break;
                        case ErrorType.RemoteProblem:
                            errorName = "Remote problem";
                            break;
                        case ErrorType.UnknownError:
                            errorName = "Unknown error";
                            errorDetails = "An unknown error occurred when trying to download this programme.  Press the 'Report Error' button on the toolbar to send a report of this error back to NerdoftheHerd, so that it can be fixed.";
                            buttons.Add(this.tbtReportError);
                            break;
                    }

                    infoText += Environment.NewLine + Environment.NewLine + "Error: " + errorName;

                    if (!string.IsNullOrEmpty(errorDetails))
                    {
                        infoText += Environment.NewLine + Environment.NewLine + errorDetails;
                    }

                    break;
                default:
                    buttons.Add(this.tbtCancel);
                    break;
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(info.name, infoText, this.progData.FetchEpisodeImage(epid));
        }

        private void SetSideBar(string title, string description, Bitmap picture)
        {
            this.lblSideMainTitle.Text = title;
            this.txtSideDescript.Text = description;

            // Make sure the scrollbars update correctly
            this.txtSideDescript.ScrollBars = RichTextBoxScrollBars.None;
            this.txtSideDescript.ScrollBars = RichTextBoxScrollBars.Both;

            if (picture != null)
            {
                if (picture.Width > this.picSidebarImg.MaximumSize.Width | picture.Height > this.picSidebarImg.MaximumSize.Height)
                {
                    int newWidth = 0;
                    int newHeight = 0;

                    if (picture.Width > picture.Height)
                    {
                        newWidth = this.picSidebarImg.MaximumSize.Width;
                        newHeight = (int)((newWidth / (float)picture.Width) * picture.Height);
                    }
                    else
                    {
                        newHeight = this.picSidebarImg.MaximumSize.Height;
                        newWidth = (int)((newHeight / (float)picture.Height) * picture.Width);
                    }

                    Bitmap origImg = picture;
                    picture = new Bitmap(newWidth, newHeight);
                    Graphics graph = null;

                    graph = Graphics.FromImage(picture);
                    graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    graph.DrawImage(origImg, 0, 0, newWidth, newHeight);

                    origImg.Dispose();
                }

                this.picSidebarImg.Image = picture;
                this.picSidebarImg.Visible = true;
            }
            else
            {
                this.picSidebarImg.Visible = false;
            }
        }

        private void SetToolbarButtons(ToolBarButton[] buttons)
        {
            this.tbtChooseProgramme.Visible = false;
            this.tbtDownload.Visible = false;
            this.tbtAddFavourite.Visible = false;
            this.tbtRemFavourite.Visible = false;
            this.tbtSubscribe.Visible = false;
            this.tbtUnsubscribe.Visible = false;
            this.tbtCurrentEps.Visible = false;
            this.tbtPlay.Visible = false;
            this.tbtCancel.Visible = false;
            this.tbtDelete.Visible = false;
            this.tbtRetry.Visible = false;
            this.tbtReportError.Visible = false;
            this.tbtCleanUp.Visible = false;

            foreach (ToolBarButton button in buttons)
            {
                button.Visible = true;
            }
        }

        public void mnuTrayShow_Click(object sender, System.EventArgs e)
        {
            if (this.Visible == false)
            {
                OsUtils.TrayAnimate(this, false);
                this.Visible = true;
            }

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
        }

        public void mnuTrayExit_Click(object eventSender, System.EventArgs eventArgs)
        {
            this.Close();
            this.Dispose();

            try
            {
                Directory.Delete(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"), true);
            }
            catch (IOException)
            {
                // Ignore an IOException - this just means that a file in the temp folder is still in use.
            }
        }

        private void nicTrayIcon_BalloonTipClicked(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.ShownTrayBalloon = true;
        }

        private void nicTrayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.mnuTrayShow_Click(sender, e);
        }

        private void tbtFindNew_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);
        }

        private void tbtFavourites_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites);
        }

        private void tbtSubscriptions_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions);
        }

        private void tbtDownloads_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
        }

        private void progData_ProviderAdded(System.Guid providerId)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_ProviderAdded(providerId); });
                return;
            }

            Data.ProviderData info = this.progData.FetchProviderData(providerId);

            ListViewItem addItem = new ListViewItem();
            addItem.Name = providerId.ToString();
            addItem.Text = info.name;

            if (info.icon != null)
            {
                this.imlProviders.Images.Add(providerId.ToString(), info.icon);
                addItem.ImageKey = providerId.ToString();
            }
            else
            {
                addItem.ImageKey = "default";
            }

            this.lstProviders.Items.Add(addItem);

            // Hide the 'No providers' provider options menu item
            if (this.mnuOptionsProviderOptsNoProvs.Visible == true)
            {
                this.mnuOptionsProviderOptsNoProvs.Visible = false;
            }

            MenuItem addMenuItem = new MenuItem(info.name + " Provider");

            if (info.showOptionsHandler != null)
            {
                addMenuItem.Click += info.showOptionsHandler;
            }
            else
            {
                addMenuItem.Enabled = false;
            }

            this.mnuOptionsProviderOpts.MenuItems.Add(addMenuItem);

            if (this.view.CurrentView == ViewState.View.FindNewChooseProvider)
            {
                if (this.lstProviders.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_ProgrammeUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_ProgrammeUpdated(progid); });
                return;
            }

            if (this.view.CurrentView == ViewState.View.ProgEpisodes)
            {
                if ((int)this.view.CurrentViewData == progid)
                {
                    if (this.lstEpisodes.SelectedItems.Count == 0)
                    {
                        // Update the displayed programme information
                        this.ShowProgrammeInfo(progid);
                    }
                    else
                    {
                        // Update the displayed episode information (in case the subscription status has changed)
                        int epid = Convert.ToInt32(this.lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                        this.ShowEpisodeInfo(epid);
                    }
                }
            }
        }

        private void ShowProgrammeInfo(int progid)
        {
            Data.ProgrammeData progInfo = this.progData.FetchProgrammeData((int)this.view.CurrentViewData);

            List<ToolBarButton> buttons = new List<ToolBarButton>();

            if (progInfo.favourite)
            {
                buttons.Add(this.tbtRemFavourite);
            }
            else
            {
                buttons.Add(this.tbtAddFavourite);
            }

            if (progInfo.subscribed)
            {
                buttons.Add(this.tbtUnsubscribe);
            }
            else if (!progInfo.singleEpisode)
            {
                buttons.Add(this.tbtSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(progInfo.name, progInfo.description, this.progData.FetchProgrammeImage(progid));
        }

        private void EpisodeListItem(int epid, Data.EpisodeData info, ref ListViewItem item)
        {
            item.Name = epid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.episodeDate.ToShortDateString();
            item.SubItems[1].Text = info.name;
            item.Checked = info.autoDownload;
        }

        private void progData_EpisodeAdded(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_EpisodeAdded(epid); });
                return;
            }

            Data.EpisodeData info = this.progData.FetchEpisodeData(epid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add(string.Empty);

            this.EpisodeListItem(epid, info, ref addItem);

            this.lstEpisodes.ItemCheck -= this.lstEpisodes_ItemCheck;
            this.lstEpisodes.Items.Add(addItem);
            this.lstEpisodes.ItemCheck += this.lstEpisodes_ItemCheck;
        }

        private void progData_FavouriteAdded(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_FavouriteAdded(progid); });
                return;
            }

            Data.FavouriteData info = this.progData.FetchFavouriteData(progid);

            this.lstFavourites.Items.Add(this.FavouriteListItem(info, null));

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.lstFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_FavouriteUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_FavouriteUpdated(progid); });
                return;
            }

            Data.FavouriteData info = this.progData.FetchFavouriteData(progid);
            ListViewItem item = this.lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)];

            item = this.FavouriteListItem(info, item);

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected)
                {
                    this.ShowFavouriteInfo(progid);
                }
                else if (this.lstFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private ListViewItem FavouriteListItem(Data.FavouriteData info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
                item.SubItems.Add(string.Empty);
            }

            item.Name = info.progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.name;

            item.SubItems[1].Text = info.providerName;
            item.ImageKey = "favourite";

            return item;
        }

        private void progData_FavouriteRemoved(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_FavouriteRemoved(progid); });
                return;
            }

            this.lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.lstFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void SubscriptionListItem(int progid, Data.SubscriptionData info, ref ListViewItem item)
        {
            item.Name = progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.name;

            if (info.latestDownload == null)
            {
                item.SubItems[1].Text = "Never";
            }
            else
            {
                item.SubItems[1].Text = info.latestDownload.Value.ToShortDateString();
            }

            item.SubItems[2].Text = info.providerName;
            item.ImageKey = "subscribed";
        }

        private void progData_SubscriptionAdded(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_SubscriptionAdded(progid); });
                return;
            }

            Data.SubscriptionData info = this.progData.FetchSubscriptionData(progid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add(string.Empty);
            addItem.SubItems.Add(string.Empty);

            this.SubscriptionListItem(progid, info, ref addItem);
            this.lstSubscribed.Items.Add(addItem);

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.lstSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_SubscriptionUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_SubscriptionUpdated(progid); });
                return;
            }

            Data.SubscriptionData info = this.progData.FetchSubscriptionData(progid);
            ListViewItem item = this.lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)];

            this.SubscriptionListItem(progid, info, ref item);

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected)
                {
                    this.ShowSubscriptionInfo(progid);
                }
                else if (this.lstSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_SubscriptionRemoved(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_SubscriptionRemoved(progid); });
                return;
            }

            this.lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.lstSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private ListViewItem DownloadListItem(Data.DownloadData info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
            }

            item.Name = info.epid.ToString(CultureInfo.InvariantCulture);

            if (item.SubItems.Count < this.downloadColOrder.Count)
            {
                for (int addCols = item.SubItems.Count; addCols <= this.downloadColOrder.Count - 1; addCols++)
                {
                    item.SubItems.Add(string.Empty);
                }
            }

            for (int column = 0; column <= this.downloadColOrder.Count - 1; column++)
            {
                switch (this.downloadColOrder[column])
                {
                    case Data.DownloadCols.EpisodeName:
                        item.SubItems[column].Text = info.name;
                        break;
                    case Data.DownloadCols.EpisodeDate:
                        item.SubItems[column].Text = info.episodeDate.ToShortDateString();
                        break;
                    case Data.DownloadCols.Status:
                        switch (info.status)
                        {
                            case Data.DownloadStatus.Waiting:
                                item.SubItems[column].Text = "Waiting";
                                break;
                            case Data.DownloadStatus.Downloaded:
                                if (info.playCount == 0)
                                {
                                    item.SubItems[column].Text = "Newly Downloaded";
                                }
                                else
                                {
                                    item.SubItems[column].Text = "Downloaded";
                                }

                                break;
                            case Data.DownloadStatus.Errored:
                                item.SubItems[column].Text = "Error";
                                break;
                            default:
                                throw new InvalidDataException("Unknown status of " + info.status.ToString());
                        }

                        break;
                    case Data.DownloadCols.Progress:
                        item.SubItems[column].Text = string.Empty;
                        break;
                    case Data.DownloadCols.Duration:
                        string durationText = string.Empty;

                        if (info.duration != 0)
                        {
                            int mins = Convert.ToInt32(Math.Round(info.duration / (decimal)60, 0));
                            int hours = mins / 60;
                            mins = mins % 60;

                            durationText = string.Format(CultureInfo.CurrentCulture, "{0}:{1:00}", hours, mins);
                        }

                        item.SubItems[column].Text = durationText;
                        break;
                    default:
                        throw new InvalidDataException("Unknown column type of " + this.downloadColOrder[column].ToString());
                }
            }

            switch (info.status)
            {
                case Data.DownloadStatus.Waiting:
                    item.ImageKey = "waiting";
                    break;
                case Data.DownloadStatus.Downloaded:
                    if (info.playCount == 0)
                    {
                        item.ImageKey = "downloaded_new";
                    }
                    else
                    {
                        item.ImageKey = "downloaded";
                    }

                    break;
                case Data.DownloadStatus.Errored:
                    item.ImageKey = "error";
                    break;
                default:
                    throw new InvalidDataException("Unknown status of " + info.status.ToString());
            }

            return item;
        }

        private void progData_DownloadAdded(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_DownloadAdded(epid); });
                return;
            }

            Data.DownloadData info = this.progData.FetchDownloadData(epid);
            this.lstDownloads.Items.Add(this.DownloadListItem(info, null));

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (this.lstDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_DownloadProgress(int epid, int percent, string statusText, ProgressIcon icon)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_DownloadProgress(epid, percent, statusText, icon); });
                return;
            }

            ListViewItem item = this.lstDownloads.Items[Convert.ToString(epid, CultureInfo.InvariantCulture)];

            if (item == null)
            {
                return;
            }

            if (this.downloadColOrder.Contains(Data.DownloadCols.Status))
            {
                item.SubItems[this.downloadColOrder.IndexOf(Data.DownloadCols.Status)].Text = statusText;
            }

            if (this.downloadColOrder.Contains(Data.DownloadCols.Progress))
            {
                this.prgDldProg.Value = percent;

                if (this.lstDownloads.Controls.Count == 0)
                {
                    this.lstDownloads.AddProgressBar(ref this.prgDldProg, item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress));
                }
            }

            switch (icon)
            {
                case ProgressIcon.Downloading:
                    item.ImageKey = "downloading";
                    break;
                case ProgressIcon.Converting:
                    item.ImageKey = "converting";
                    break;
            }
        }

        private void progData_DownloadRemoved(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_DownloadRemoved(epid); });
                return;
            }

            ListViewItem item = this.lstDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];

            if (item != null)
            {
                if (this.downloadColOrder.Contains(Data.DownloadCols.Progress))
                {
                    if (this.lstDownloads.GetProgressBar(item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null)
                    {
                        this.lstDownloads.RemoveProgressBar(ref this.prgDldProg);
                    }
                }

                item.Remove();
            }

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (this.lstDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_DownloadUpdated(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_DownloadUpdated(epid); });
                return;
            }

            Data.DownloadData info = this.progData.FetchDownloadData(epid);

            ListViewItem item = this.lstDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];
            item = this.DownloadListItem(info, item);

            if (this.downloadColOrder.Contains(Data.DownloadCols.Progress))
            {
                if (this.lstDownloads.GetProgressBar(item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null)
                {
                    this.lstDownloads.RemoveProgressBar(ref this.prgDldProg);
                }
            }

            // Update the downloads list sorting, as the order may now have changed
            this.lstDownloads.Sort();

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (item.Selected)
                {
                    this.ShowDownloadInfo(epid);
                }
                else if (this.lstDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void progData_DownloadProgressTotal(bool downloading, int percent)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.progData_DownloadProgressTotal(downloading, percent); });
                return;
            }

            if (this.IsDisposed == false)
            {
                this.UpdateTrayStatus(downloading);

                if (OsUtils.WinSevenOrLater())
                {
                    if (downloading)
                    {
                        this.tbarNotif.SetProgressValue(this, percent, 100);
                    }
                    else
                    {
                        this.tbarNotif.SetProgressNone(this);
                    }
                }
            }
        }

        private void progData_FindNewViewChange(object viewData)
        {
            FindNewViewData findViewData = (FindNewViewData)this.view.CurrentViewData;
            findViewData.View = viewData;

            this.view.StoreView(findViewData);
        }

        private void progData_FoundNew(int progid)
        {
            this.view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void Main_Shown(object sender, System.EventArgs e)
        {
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                if (commandLineArg.ToUpperInvariant() == "/HIDEMAINWINDOW")
                {
                    if (OsUtils.WinSevenOrLater())
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                    else
                    {
                        OsUtils.TrayAnimate(this, true);
                        this.Visible = false;
                    }
                }
            }
        }

        private void Main_Move_Resize(object sender, System.EventArgs e)
        {
            if (this.windowPosLoaded)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    Properties.Settings.Default.MainFormPos = this.DesktopBounds;
                }

                if (this.WindowState != FormWindowState.Minimized)
                {
                    Properties.Settings.Default.MainFormState = this.WindowState;
                }
            }
        }

        private void tmrCheckForUpdates_Tick(object sender, System.EventArgs e)
        {
            if (this.checkUpdate.IsUpdateAvailable())
            {
                if (Properties.Settings.Default.LastUpdatePrompt.AddDays(7) < DateAndTime.Now)
                {
                    Properties.Settings.Default.LastUpdatePrompt = DateAndTime.Now;
                    Properties.Settings.Default.Save(); // Save the last prompt time in case of unexpected termination

                    using (UpdateNotify showUpdate = new UpdateNotify())
                    {
                        if (this.WindowState == FormWindowState.Minimized | this.Visible == false)
                        {
                            showUpdate.StartPosition = FormStartPosition.CenterScreen;
                        }

                        if (showUpdate.ShowDialog(this) == DialogResult.Yes)
                        {
                            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/update.php?prevver=" + new ApplicationBase().Info.Version.ToString());
                        }
                    }
                }
            }
        }

        private void tbtAddFavourite_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (this.progData.AddFavourite(progid))
            {
                this.view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites, null);
            }
            else
            {
                Interaction.MsgBox("You already have this programme in your list of favourites!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtRemFavourite_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to remove this programme from your list of favourites?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.RemoveFavourite(progid);
            }
        }

        private void tbtSubscribe_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (this.progData.AddSubscription(progid))
            {
                this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions, null);
            }
            else
            {
                Interaction.MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtUnsubscribe_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to stop having new episodes of this programme downloaded automatically?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.RemoveSubscription(progid);
            }
        }

        private void tbtCancel_Click()
        {
            int epid = Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (Interaction.MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.DownloadRemove(epid);
            }
        }

        private void tbtPlay_Click()
        {
            int epid = Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Data.DownloadData info = this.progData.FetchDownloadData(epid);

            if (info.status == Data.DownloadStatus.Downloaded)
            {
                if (File.Exists(info.downloadPath))
                {
                    Process.Start(info.downloadPath);

                    // Bump the play count of this item up by one
                    this.progData.DownloadBumpPlayCount(epid);
                }
            }
        }

        private void tbtDelete_Click()
        {
            int epid = Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Data.DownloadData info = this.progData.FetchDownloadData(epid);

            bool fileExists = File.Exists(info.downloadPath);
            string delQuestion = "Are you sure that you would like to delete this episode";

            if (fileExists)
            {
                delQuestion += " and the associated audio file";
            }

            if (Interaction.MsgBox(delQuestion + "?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                if (fileExists)
                {
                    try
                    {
                        File.Delete(info.downloadPath);
                    }
                    catch (IOException)
                    {
                        if (Interaction.MsgBox("There was a problem deleting the audio file for this episode, as the file is in use by another application." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation | MsgBoxStyle.YesNo) == MsgBoxResult.No)
                        {
                            return;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (Interaction.MsgBox("There was a problem deleting the audio file for this episode, as the file is either read-only or you do not have the permissions required." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation | MsgBoxStyle.YesNo) == MsgBoxResult.No)
                        {
                            return;
                        }
                    }
                }

                this.progData.DownloadRemove(epid);
            }
        }

        private void tbtRetry_Click()
        {
            this.progData.ResetDownload(Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
        }

        private void tbtDownload_Click()
        {
            int epid = Convert.ToInt32(this.lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (this.progData.AddDownload(epid))
            {
                this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
            }
            else
            {
                Interaction.MsgBox("This episode is already in the download list!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtCurrentEps_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            this.view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void tbtReportError_Click()
        {
            int episodeID = Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            this.progData.DownloadReportError(episodeID);
        }

        private void tbtChooseProgramme_Click()
        {
            FindNewViewData viewData = default(FindNewViewData);
            viewData.ProviderID = new Guid(this.lstProviders.SelectedItems[0].Name);
            viewData.View = null;

            this.view.SetView(ViewState.View.FindNewProviderForm, viewData);
        }

        private void mnuOptionsShowOpts_Click(object sender, System.EventArgs e)
        {
            using (Preferences prefs = new Preferences())
            {
                prefs.ShowDialog();
            }
        }

        private void mnuOptionsExit_Click(object sender, System.EventArgs e)
        {
            this.mnuTrayExit_Click(sender, e);
        }

        private void mnuHelpAbout_Click(object sender, System.EventArgs e)
        {
            using (About about = new About())
            {
                about.ShowDialog();
            }
        }

        private void mnuHelpShowHelp_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/help/");
        }

        private void mnuHelpReportBug_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/bug_report.php");
        }

        private void tbtCleanUp_Click()
        {
            using (CleanUp cleanUp = new CleanUp())
            {
                cleanUp.ShowDialog();
            }
        }

        private void view_UpdateNavBtnState(bool enableBack, bool enableFwd)
        {
            this.tbtBack.Enabled = enableBack;
            this.tbtForward.Enabled = enableFwd;
        }

        private void view_ViewChanged(ViewState.View view, ViewState.MainTab tab, object data)
        {
            this.tbtFindNew.Checked = false;
            this.tbtFavourites.Checked = false;
            this.tbtSubscriptions.Checked = false;
            this.tbtDownloads.Checked = false;

            switch (tab)
            {
                case ViewState.MainTab.FindProgramme:
                    this.tbtFindNew.Checked = true;
                    break;
                case ViewState.MainTab.Favourites:
                    this.tbtFavourites.Checked = true;
                    break;
                case ViewState.MainTab.Subscriptions:
                    this.tbtSubscriptions.Checked = true;
                    break;
                case ViewState.MainTab.Downloads:
                    this.tbtDownloads.Checked = true;
                    break;
            }

            this.SetViewDefaults();

            // Set the focus to a control which does not show it, to prevent the toolbar momentarily showing focus
            this.lblSideMainTitle.Focus();

            this.lstProviders.Visible = false;
            this.pnlPluginSpace.Visible = false;
            this.lstEpisodes.Visible = false;
            this.lstFavourites.Visible = false;
            this.lstSubscribed.Visible = false;
            this.lstDownloads.Visible = false;
            this.ttxSearch.Visible = false;

            switch (view)
            {
                case ViewState.View.FindNewChooseProvider:
                    this.lstProviders.Visible = true;
                    this.lstProviders.Focus();

                    if (this.lstProviders.SelectedItems.Count > 0)
                    {
                        this.ShowProviderInfo(new Guid(this.lstProviders.SelectedItems[0].Name));
                    }

                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData FindViewData = (FindNewViewData)data;

                    if (this.pnlPluginSpace.Controls.Count > 0)
                    {
                        this.pnlPluginSpace.Controls[0].Dispose();
                        this.pnlPluginSpace.Controls.Clear();
                    }

                    this.pnlPluginSpace.Visible = true;
                    this.pnlPluginSpace.Controls.Add(this.progData.GetFindNewPanel(FindViewData.ProviderID, FindViewData.View));
                    this.pnlPluginSpace.Controls[0].Dock = DockStyle.Fill;
                    this.pnlPluginSpace.Controls[0].Focus();
                    break;
                case ViewState.View.ProgEpisodes:
                    this.lstEpisodes.Visible = true;
                    this.progData.CancelEpisodeListing();
                    this.lstEpisodes.Items.Clear(); // Clear before DoEvents so that old items don't flash up on screen
                    Application.DoEvents(); // Give any queued BeginInvoke calls a chance to be processed
                    this.lstEpisodes.Items.Clear();
                    this.progData.InitEpisodeList((int)data);
                    break;
                case ViewState.View.Favourites:
                    this.lstFavourites.Visible = true;
                    this.lstFavourites.Focus();

                    if (this.lstFavourites.SelectedItems.Count > 0)
                    {
                        this.ShowFavouriteInfo(Convert.ToInt32(this.lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }

                    break;
                case ViewState.View.Subscriptions:
                    this.lstSubscribed.Visible = true;
                    this.lstSubscribed.Focus();

                    if (this.lstSubscribed.SelectedItems.Count > 0)
                    {
                        this.ShowSubscriptionInfo(Convert.ToInt32(this.lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }

                    break;
                case ViewState.View.Downloads:
                    if (data == null)
                    {
                        this.ttxSearch.Text = string.Empty;
                    }
                    else
                    {
                        this.ttxSearch.Text = (string)data;
                        this.PerformSearch(view, this.ttxSearch.Text);
                    }

                    this.ttxSearch.Visible = true;
                    this.lstDownloads.Visible = true;
                    this.lstDownloads.Focus();

                    if (this.lstDownloads.SelectedItems.Count > 0)
                    {
                        this.ShowDownloadInfo(Convert.ToInt32(this.lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }

                    break;
            }
        }

        private void SetViewDefaults()
        {
            switch (this.view.CurrentView)
            {
                case ViewState.View.FindNewChooseProvider:
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.SetSideBar(Convert.ToString(this.lstProviders.Items.Count, CultureInfo.CurrentCulture) + " provider" + (this.lstProviders.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData FindViewData = (FindNewViewData)this.view.CurrentViewData;
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.ShowProviderInfo(FindViewData.ProviderID);
                    break;
                case ViewState.View.ProgEpisodes:
                    this.ShowProgrammeInfo((int)this.view.CurrentViewData);
                    break;
                case ViewState.View.Favourites:
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.SetSideBar(Convert.ToString(this.lstFavourites.Items.Count, CultureInfo.CurrentCulture) + " favourite" + (this.lstFavourites.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.Subscriptions:
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.SetSideBar(Convert.ToString(this.lstSubscribed.Items.Count, CultureInfo.CurrentCulture) + " subscription" + (this.lstSubscribed.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.Downloads:
                    this.SetToolbarButtons(new ToolBarButton[] { this.tbtCleanUp });

                    if (!string.IsNullOrEmpty(this.progData.DownloadQuery))
                    {
                        this.SetSideBar(Convert.ToString(this.lstDownloads.Items.Count, CultureInfo.CurrentCulture) + " result" + (this.lstDownloads.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    }
                    else
                    {
                        string description = string.Empty;
                        int newCount = this.progData.CountDownloadsNew();
                        int errorCount = this.progData.CountDownloadsErrored();

                        if (newCount > 0)
                        {
                            description += "Newly downloaded: " + Convert.ToString(newCount, CultureInfo.CurrentCulture) + Environment.NewLine;
                        }

                        if (errorCount > 0)
                        {
                            description += "Errored: " + Convert.ToString(errorCount, CultureInfo.CurrentCulture);
                        }

                        this.SetSideBar(Convert.ToString(lstDownloads.Items.Count, CultureInfo.CurrentCulture) + " download" + (lstDownloads.Items.Count == 1 ? string.Empty : "s"), description, null);
                    }

                    break;
            }
        }

        private void tbtBack_Click(object sender, System.EventArgs e)
        {
            this.view.NavBack();
        }

        private void tbtForward_Click(object sender, System.EventArgs e)
        {
            this.view.NavFwd();
        }

        private void tbrToolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.Name)
            {
                case "tbtChooseProgramme":
                    this.tbtChooseProgramme_Click();
                    break;
                case "tbtDownload":
                    this.tbtDownload_Click();
                    break;
                case "tbtAddFavourite":
                    this.tbtAddFavourite_Click();
                    break;
                case "tbtRemFavourite":
                    this.tbtRemFavourite_Click();
                    break;
                case "tbtSubscribe":
                    this.tbtSubscribe_Click();
                    break;
                case "tbtUnsubscribe":
                    this.tbtUnsubscribe_Click();
                    break;
                case "tbtCurrentEps":
                    this.tbtCurrentEps_Click();
                    break;
                case "tbtCancel":
                    this.tbtCancel_Click();
                    break;
                case "tbtPlay":
                    this.tbtPlay_Click();
                    break;
                case "tbtDelete":
                    this.tbtDelete_Click();
                    break;
                case "tbtRetry":
                    this.tbtRetry_Click();
                    break;
                case "tbtReportError":
                    this.tbtReportError_Click();
                    break;
                case "tbtCleanUp":
                    this.tbtCleanUp_Click();
                    break;
            }
        }

        private void tblToolbars_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.tblToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, this.tblToolbars.Width - (this.tbtHelpMenu.Rectangle.Width + this.tbrHelp.Margin.Right));
                this.tblToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, this.tbtHelpMenu.Rectangle.Width + this.tbrHelp.Margin.Right);

                if (VisualStyleRenderer.IsSupported)
                {
                    // Visual styles are enabled, so draw the correct background behind the toolbars
                    Bitmap bmpBackground = new Bitmap(this.tblToolbars.Width, this.tblToolbars.Height);
                    Graphics graGraphics = Graphics.FromImage(bmpBackground);

                    try
                    {
                        VisualStyleRenderer vsrRebar = new VisualStyleRenderer("Rebar", 0, 0);
                        vsrRebar.DrawBackground(graGraphics, new Rectangle(0, 0, this.tblToolbars.Width, this.tblToolbars.Height));
                        this.tblToolbars.BackgroundImage = bmpBackground;
                    }
                    catch (ArgumentException)
                    {
                        // The 'Rebar' background image style did not exist, so don't try to draw it.
                    }
                }
            }
        }

        private void txtSideDescript_GotFocus(object sender, System.EventArgs e)
        {
            this.lblSideMainTitle.Focus();
        }

        private void txtSideDescript_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void txtSideDescript_Resize(object sender, System.EventArgs e)
        {
            this.txtSideDescript.Refresh(); // Make sure the scrollbars update correctly
        }

        private void picSideBarBorder_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(255, 167, 186, 197)), 0, 0, 0, this.picSideBarBorder.Height);
        }

        private void mnuListHdrsColumns_Click(object sender, System.EventArgs e)
        {
            using (ChooseCols chooser = new ChooseCols())
            {
                chooser.Columns = Properties.Settings.Default.DownloadCols;
                chooser.StoreNameList(this.downloadColNames);

                if (chooser.ShowDialog(this) == DialogResult.OK)
                {
                    Properties.Settings.Default.DownloadCols = chooser.Columns;
                    this.InitDownloadList();
                }
            }
        }

        private void mnuListHdrsReset_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.DownloadCols = (string)Properties.Settings.Default.Properties["DownloadCols"].DefaultValue;
            Properties.Settings.Default.DownloadColSizes = (string)Properties.Settings.Default.Properties["DownloadColSizes"].DefaultValue;
            Properties.Settings.Default.DownloadColSortBy = Convert.ToInt32(Properties.Settings.Default.Properties["DownloadColSortBy"].DefaultValue, CultureInfo.InvariantCulture);
            Properties.Settings.Default.DownloadColSortAsc = Convert.ToBoolean(Properties.Settings.Default.Properties["DownloadColSortAsc"].DefaultValue, CultureInfo.InvariantCulture);

            this.InitDownloadList();
        }

        private void InitFavouriteList()
        {
            if (this.lstFavourites.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            // Convert the list of FavouriteData items to an array of ListItems
            List<Data.FavouriteData> initData = this.progData.FetchFavouriteList();
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++)
            {
                initItems[convItems] = this.FavouriteListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.lstFavourites.Items.AddRange(initItems);
        }

        private void InitDownloadList()
        {
            if (this.lstDownloads.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            this.downloadColSizes.Clear();
            this.downloadColOrder.Clear();
            this.lstDownloads.Clear();
            this.lstDownloads.RemoveAllControls();

            string newItems = string.Empty;

            // Find any columns without widths defined in the current setting
            foreach (string sizePair in Strings.Split(Properties.Settings.Default.Properties["DownloadColSizes"].DefaultValue.ToString(), "|"))
            {
                if (!("|" + Properties.Settings.Default.DownloadColSizes).Contains("|" + Strings.Split(sizePair, ",")[0] + ","))
                {
                    newItems += "|" + sizePair;
                }
            }

            // Append the new column sizes to the end of the setting
            if (!string.IsNullOrEmpty(newItems))
            {
                Properties.Settings.Default.DownloadColSizes += newItems;
            }

            // Fetch the column sizes into downloadColSizes for ease of access
            foreach (string sizePair in Strings.Split(Properties.Settings.Default.DownloadColSizes, "|"))
            {
                string[] splitPair = Strings.Split(sizePair, ",");
                int pixelSize = Convert.ToInt32(float.Parse(splitPair[1], CultureInfo.InvariantCulture) * this.CurrentAutoScaleDimensions.Width);

                this.downloadColSizes.Add(int.Parse(splitPair[0], CultureInfo.InvariantCulture), pixelSize);
            }

            // Set up the columns specified in the DownloadCols setting
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DownloadCols))
            {
                string[] columns = Strings.Split(Properties.Settings.Default.DownloadCols, ",");

                foreach (string column in columns)
                {
                    int colVal = int.Parse(column, CultureInfo.InvariantCulture);
                    this.downloadColOrder.Add((Data.DownloadCols)colVal);
                    this.lstDownloads.Columns.Add(this.downloadColNames[colVal], this.downloadColSizes[colVal]);
                }
            }

            // Apply the sort from the current settings
            this.progData.DownloadSortByCol = (Data.DownloadCols)Properties.Settings.Default.DownloadColSortBy;
            this.progData.DownloadSortAscending = Properties.Settings.Default.DownloadColSortAsc;
            this.lstDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(this.progData.DownloadSortByCol), this.progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of DownloadData items to an array of ListItems
            List<Data.DownloadData> initData = this.progData.FetchDownloadList(true);
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++)
            {
                initItems[convItems] = this.DownloadListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.lstDownloads.Items.AddRange(initItems);
        }

        private void ttxSearch_TextChanged(object sender, System.EventArgs e)
        {
            lock (this.searchThreadLock)
            {
                if (string.IsNullOrEmpty(this.ttxSearch.Text))
                {
                    this.searchThread = null;
                    this.PerformSearch(this.view.CurrentView, this.ttxSearch.Text);
                }
                else
                {
                    this.searchThread = new Thread(() => this.SearchWait(this.view.CurrentView, this.ttxSearch.Text));
                    this.searchThread.IsBackground = true;
                    this.searchThread.Start();
                }
            }
        }

        private void SearchWait(ViewState.View origView, string search)
        {
            Thread.Sleep(500);

            lock (this.searchThreadLock)
            {
                if (!object.ReferenceEquals(Thread.CurrentThread, this.searchThread))
                {
                    // A search thread was created more recently, stop this thread
                    return;
                }

                this.Invoke((MethodInvoker)delegate { this.PerformSearch(origView, search); });
            }
        }

        private void PerformSearch(ViewState.View origView, string search)
        {
            if (this.view.CurrentView == origView)
            {
                if (string.IsNullOrEmpty(search))
                {
                    if (this.view.CurrentViewData != null)
                    {
                        this.view.StoreView(null);
                    }
                }
                else
                {
                    if (this.view.CurrentViewData == null)
                    {
                        this.view.StoreView(search);
                    }
                    else
                    {
                        this.view.CurrentViewData = search;
                    }
                }

                this.progData.DownloadQuery = search;
                this.InitDownloadList();
                this.SetViewDefaults();
            }
        }

        public void App_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            foreach (string commandLineArg in e.CommandLine)
            {
                if (commandLineArg.ToUpperInvariant() == "/EXIT")
                {
                    // Close the application
                    this.mnuTrayExit_Click(sender, e);
                    return;
                }
            }

            // Do the same as a double click on the tray icon
            this.mnuTrayShow_Click(sender, e);
        }
    }
}

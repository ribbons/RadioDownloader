/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

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

    internal partial class Main : GlassForm
    {
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

        public void App_StartupNextInstance(object sender, Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs e)
        {
            foreach (string commandLineArg in e.CommandLine)
            {
                if (commandLineArg.ToUpperInvariant() == "/EXIT")
                {
                    // Close the application
                    this.MenuTrayExit_Click(sender, e);
                    return;
                }
            }

            // Do the same as a double click on the tray icon
            this.MenuTrayShow_Click(sender, e);
        }

        private void UpdateTrayStatus(bool active)
        {
            if (OsUtils.WinSevenOrLater())
            {
                if (Model.Download.CountErrored() > 0)
                {
                    this.tbarNotif.SetOverlayIcon(this, Properties.Resources.overlay_error, "Error");
                    this.tbarNotif.SetThumbnailTooltip(this, this.Text + ": Error");
                }
                else
                {
                    if (active)
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

            if (this.NotifyIcon.Visible)
            {
                if (Model.Download.CountErrored() > 0)
                {
                    this.NotifyIcon.Icon = Properties.Resources.icon_error;
                    this.NotifyIcon.Text = this.Text + ": Error";
                }
                else
                {
                    if (active)
                    {
                        this.NotifyIcon.Icon = Properties.Resources.icon_working;
                        this.NotifyIcon.Text = this.Text + ": Downloading";
                    }
                    else
                    {
                        this.NotifyIcon.Icon = Properties.Resources.icon_main;
                        this.NotifyIcon.Text = this.Text;
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
                    this.MenuHelpShowHelp_Click(sender, e);
                    break;
                case Keys.Delete:
                    if (this.ButtonDelete.Visible)
                    {
                        e.Handled = true;
                        this.ButtonDelete_Click();
                    }
                    else if (this.ButtonCancel.Visible)
                    {
                        e.Handled = true;
                        ButtonCancel_Click();
                    }

                    break;
                case Keys.Back:
                    if (!object.ReferenceEquals(this.ActiveControl.GetType(), typeof(TextBox)) && !object.ReferenceEquals(this.ActiveControl.Parent.GetType(), typeof(ExtToolStrip)))
                    {
                        if (e.Shift)
                        {
                            if (this.ButtonForward.Enabled)
                            {
                                e.Handled = true;
                                this.ButtonForward_Click(sender, e);
                            }
                        }
                        else
                        {
                            if (this.ButtonBack.Enabled)
                            {
                                e.Handled = true;
                                this.ButtonBack_Click(sender, e);
                            }
                        }
                    }

                    break;
                case Keys.BrowserBack:
                    if (this.ButtonBack.Enabled)
                    {
                        e.Handled = true;
                        this.ButtonBack_Click(sender, e);
                    }

                    break;
                case Keys.BrowserForward:
                    if (this.ButtonForward.Enabled)
                    {
                        e.Handled = true;
                        this.ButtonForward_Click(sender, e);
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

            if (!fileExits.Exists)
            {
                try
                {
                    System.IO.File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"));
                }
                catch (FileNotFoundException)
                {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
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
                    System.IO.File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), true);
                }
                catch (FileNotFoundException)
                {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    Interaction.MsgBox("Access was denied when attempting to copy the Radio Downloader template database." + Environment.NewLine + Environment.NewLine + "Check that you have read access to '" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "store.db") + "' and write access to '" + Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db") + "'.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }

                using (UpdateDB doDbUpdate = new UpdateDB(Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db")))
                {
                    doDbUpdate.UpdateStructure();
                }
            }

            this.ImagesListIcons.Images.Add("downloading", Properties.Resources.list_downloading);
            this.ImagesListIcons.Images.Add("waiting", Properties.Resources.list_waiting);
            this.ImagesListIcons.Images.Add("converting", Properties.Resources.list_converting);
            this.ImagesListIcons.Images.Add("downloaded_new", Properties.Resources.list_downloaded_new);
            this.ImagesListIcons.Images.Add("downloaded", Properties.Resources.list_downloaded);
            this.ImagesListIcons.Images.Add("subscribed", Properties.Resources.list_subscribed);
            this.ImagesListIcons.Images.Add("subscribed_multi", Properties.Resources.list_subscribed_multi);
            this.ImagesListIcons.Images.Add("error", Properties.Resources.list_error);
            this.ImagesListIcons.Images.Add("favourite", Properties.Resources.list_favourite);

            this.ImagesProviders.Images.Add("default", Properties.Resources.provider_default);

            this.ImagesToolbar.Images.Add("choose_programme", Properties.Resources.toolbar_choose_programme);
            this.ImagesToolbar.Images.Add("clean_up", Properties.Resources.toolbar_clean_up);
            this.ImagesToolbar.Images.Add("current_episodes", Properties.Resources.toolbar_current_episodes);
            this.ImagesToolbar.Images.Add("delete", Properties.Resources.toolbar_delete);
            this.ImagesToolbar.Images.Add("download", Properties.Resources.toolbar_download);
            this.ImagesToolbar.Images.Add("help", Properties.Resources.toolbar_help);
            this.ImagesToolbar.Images.Add("options", Properties.Resources.toolbar_options);
            this.ImagesToolbar.Images.Add("play", Properties.Resources.toolbar_play);
            this.ImagesToolbar.Images.Add("report_error", Properties.Resources.toolbar_report_error);
            this.ImagesToolbar.Images.Add("retry", Properties.Resources.toolbar_retry);
            this.ImagesToolbar.Images.Add("subscribe", Properties.Resources.toolbar_subscribe);
            this.ImagesToolbar.Images.Add("unsubscribe", Properties.Resources.toolbar_unsubscribe);
            this.ImagesToolbar.Images.Add("add_favourite", Properties.Resources.toolbar_add_favourite);
            this.ImagesToolbar.Images.Add("remove_favourite", Properties.Resources.toolbar_remove_favourite);

            this.ToolbarMain.ImageList = this.ImagesToolbar;
            this.ToolbarHelp.ImageList = this.ImagesToolbar;
            this.ListProviders.LargeImageList = this.ImagesProviders;
            this.ListFavourites.SmallImageList = this.ImagesListIcons;
            this.ListSubscribed.SmallImageList = this.ImagesListIcons;
            this.ListDownloads.SmallImageList = this.ImagesListIcons;

            this.ListEpisodes.Columns.Add("Date", Convert.ToInt32(0.179 * this.ListEpisodes.Width));
            this.ListEpisodes.Columns.Add("Episode Name", Convert.ToInt32(0.786 * this.ListEpisodes.Width));
            this.ListFavourites.Columns.Add("Programme Name", Convert.ToInt32(0.661 * this.ListFavourites.Width));
            this.ListFavourites.Columns.Add("Provider", Convert.ToInt32(0.304 * this.ListFavourites.Width));
            this.ListSubscribed.Columns.Add("Programme Name", Convert.ToInt32(0.482 * this.ListSubscribed.Width));
            this.ListSubscribed.Columns.Add("Last Download", Convert.ToInt32(0.179 * this.ListSubscribed.Width));
            this.ListSubscribed.Columns.Add("Provider", Convert.ToInt32(0.304 * this.ListSubscribed.Width));

            // NB - these are defined in alphabetical order to save sorting later
            this.downloadColNames.Add((int)Data.DownloadCols.EpisodeDate, "Date");
            this.downloadColNames.Add((int)Data.DownloadCols.Duration, "Duration");
            this.downloadColNames.Add((int)Data.DownloadCols.EpisodeName, "Episode Name");
            this.downloadColNames.Add((int)Data.DownloadCols.Progress, "Progress");
            this.downloadColNames.Add((int)Data.DownloadCols.Status, "Status");

            this.view = new ViewState();
            this.view.UpdateNavBtnState += this.View_UpdateNavBtnState;
            this.view.ViewChanged += this.View_ViewChanged;
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);

            this.progData = Data.GetInstance();
            this.progData.ProviderAdded += this.ProgData_ProviderAdded;
            this.progData.ProgrammeUpdated += this.ProgData_ProgrammeUpdated;
            this.progData.EpisodeAdded += this.ProgData_EpisodeAdded;
            this.progData.FavouriteAdded += this.ProgData_FavouriteAdded;
            this.progData.FavouriteUpdated += this.ProgData_FavouriteUpdated;
            this.progData.FavouriteRemoved += this.ProgData_FavouriteRemoved;
            this.progData.SubscriptionAdded += this.ProgData_SubscriptionAdded;
            this.progData.SubscriptionUpdated += this.ProgData_SubscriptionUpdated;
            this.progData.SubscriptionRemoved += this.ProgData_SubscriptionRemoved;
            this.progData.DownloadAdded += this.ProgData_DownloadAdded;
            this.progData.DownloadProgress += this.ProgData_DownloadProgress;
            this.progData.DownloadRemoved += this.ProgData_DownloadRemoved;
            this.progData.DownloadUpdated += this.ProgData_DownloadUpdated;
            this.progData.DownloadProgressTotal += this.ProgData_DownloadProgressTotal;
            this.progData.FindNewViewChange += this.ProgData_FindNewViewChange;
            this.progData.FoundNew += this.ProgData_FoundNew;

            this.progData.InitProviderList();
            this.InitFavouriteList();
            this.progData.InitSubscriptionList();
            this.InitDownloadList();

            this.ListFavourites.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Favourite);
            this.ListSubscribed.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Subscription);
            this.ListDownloads.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Download);

            if (OsUtils.WinSevenOrLater())
            {
                // New style taskbar - initialise the taskbar notification class
                this.tbarNotif = new TaskbarNotify();
            }

            if (!OsUtils.WinSevenOrLater() || Properties.Settings.Default.CloseToSystray)
            {
                // Show a system tray icon
                this.NotifyIcon.Visible = true;
            }

            // Set up the initial notification status
            this.UpdateTrayStatus(false);

            this.checkUpdate = new UpdateCheck("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=" + Application.ProductVersion);

            this.ImageSidebarBorder.Width = 2;

            this.ListProviders.Dock = DockStyle.Fill;
            this.PanelPluginSpace.Dock = DockStyle.Fill;
            this.ListEpisodes.Dock = DockStyle.Fill;
            this.ListFavourites.Dock = DockStyle.Fill;
            this.ListSubscribed.Dock = DockStyle.Fill;
            this.ListDownloads.Dock = DockStyle.Fill;

            this.Font = SystemFonts.MessageBoxFont;
            this.LabelSidebarTitle.Font = new Font(this.Font.FontFamily, Convert.ToSingle(this.Font.SizeInPoints * 1.16), this.Font.Style, GraphicsUnit.Point);

            // Scale the max size of the sidebar image for values other than 96 dpi, as it is specified in pixels
            using (Graphics graphicsForDpi = this.CreateGraphics())
            {
                this.ImageSidebar.MaximumSize = new Size(Convert.ToInt32(this.ImageSidebar.MaximumSize.Width * (graphicsForDpi.DpiX / 96)), Convert.ToInt32(this.ImageSidebar.MaximumSize.Height * (graphicsForDpi.DpiY / 96)));
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

            this.TableToolbars.Height = this.ToolbarMain.Height;
            this.ToolbarMain.SetWholeDropDown(this.ButtonOptionsMenu);
            this.ToolbarHelp.SetWholeDropDown(this.ButtonHelpMenu);
            this.ToolbarHelp.Width = this.ButtonHelpMenu.Rectangle.Width;

            if (this.WindowState != FormWindowState.Minimized)
            {
                this.TableToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, this.TableToolbars.Width - (this.ButtonHelpMenu.Rectangle.Width + this.ToolbarHelp.Margin.Right));
                this.TableToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, this.ButtonHelpMenu.Rectangle.Width + this.ToolbarHelp.Margin.Right);
            }

            if (OsUtils.WinVistaOrLater() && VisualStyleRenderer.IsSupported)
            {
                this.ToolbarView.Margin = new Padding(0);
            }

            this.SetGlassMargins(0, 0, this.ToolbarView.Height, 0);
            this.ToolbarView.Renderer = new TabBarRenderer();

            OsUtils.ApplyRunOnStartup();

            this.progData.StartDownload();
            this.TimerCheckForUpdates.Enabled = true;
        }

        private void Main_FormClosing(object eventSender, System.Windows.Forms.FormClosingEventArgs eventArgs)
        {
            if (eventArgs.CloseReason == CloseReason.UserClosing)
            {
                if (!this.NotifyIcon.Visible)
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

                    if (!Properties.Settings.Default.ShownTrayBalloon)
                    {
                        this.NotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                        this.NotifyIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + Environment.NewLine + "Click here to hide this message in future.";
                        this.NotifyIcon.BalloonTipTitle = "Radio Downloader is still running";
                        this.NotifyIcon.ShowBalloonTip(30000);
                    }
                }
            }
        }

        private void ListProviders_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.ListProviders.SelectedItems.Count > 0)
            {
                Guid pluginId = new Guid(this.ListProviders.SelectedItems[0].Name);
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
            this.SetSideBar(info.Name, info.Description, null);

            if (this.view.CurrentView == ViewState.View.FindNewChooseProvider)
            {
                this.SetToolbarButtons(new ToolBarButton[] { this.ButtonChooseProgramme });
            }
        }

        private void ListProviders_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListProviders.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonChooseProgramme_Click();
        }

        private void ListEpisodes_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            this.progData.EpisodeSetAutoDownload(Convert.ToInt32(this.ListEpisodes.Items[e.Index].Name, CultureInfo.InvariantCulture), e.NewValue == CheckState.Checked);
        }

        private void ShowEpisodeInfo(int epid)
        {
            Model.Programme progInfo = new Model.Programme((int)this.view.CurrentViewData);
            Model.Episode epInfo = new Model.Episode(epid);
            string infoText = string.Empty;

            if (epInfo.Description != null)
            {
                infoText += epInfo.Description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + epInfo.EpisodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(epInfo.Duration);

            this.SetSideBar(TextUtils.StripDateFromName(epInfo.Name, epInfo.EpisodeDate), infoText, this.progData.FetchEpisodeImage(epid));

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.ButtonDownload);

            if (Model.Favourite.IsFavourite(progInfo.Progid))
            {
                buttons.Add(this.ButtonRemFavourite);
            }
            else
            {
                buttons.Add(this.ButtonAddFavourite);
            }

            if (Model.Subscription.IsSubscribed(progInfo.Progid))
            {
                buttons.Add(this.ButtonUnsubscribe);
            }
            else
            {
                buttons.Add(this.ButtonSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
        }

        private void ListEpisodes_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.ListEpisodes.SelectedItems.Count > 0)
            {
                int epid = Convert.ToInt32(this.ListEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                this.ShowEpisodeInfo(epid);
            }
            else
            {
                this.SetViewDefaults(); // Revert back to programme info in sidebar
            }
        }

        private void ListFavourites_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.ListFavourites.SelectedItems.Count > 0)
            {
                int progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);

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
            Model.Favourite info = new Model.Favourite(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.AddRange(new ToolBarButton[] { this.ButtonRemFavourite, this.ButtonCurrentEps });

            if (Model.Subscription.IsSubscribed(progid))
            {
                buttons.Add(this.ButtonUnsubscribe);
            }
            else
            {
                buttons.Add(this.ButtonSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(info.Name, info.Description, this.progData.FetchProgrammeImage(progid));
        }

        private void ListSubscribed_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.ListSubscribed.SelectedItems.Count > 0)
            {
                int progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);

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
            Model.Subscription info = new Model.Subscription(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.ButtonUnsubscribe);
            buttons.Add(this.ButtonCurrentEps);

            if (Model.Favourite.IsFavourite(progid))
            {
                buttons.Add(this.ButtonRemFavourite);
            }
            else
            {
                buttons.Add(this.ButtonAddFavourite);
            }

            this.SetToolbarButtons(buttons.ToArray());

            this.SetSideBar(info.Name, info.Description, this.progData.FetchProgrammeImage(progid));
        }

        private void ListDownloads_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
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
            this.ListDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(this.progData.DownloadSortByCol), this.progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Properties.Settings.Default.DownloadColSortBy = (int)this.progData.DownloadSortByCol;
            Properties.Settings.Default.DownloadColSortAsc = this.progData.DownloadSortAscending;

            this.ListDownloads.Sort();
        }

        private void ListDownloads_ColumnReordered(object sender, System.Windows.Forms.ColumnReorderedEventArgs e)
        {
            string[] oldOrder = new string[this.ListDownloads.Columns.Count];

            // Fetch the pre-reorder column order
            foreach (ColumnHeader col in this.ListDownloads.Columns)
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

        private void ListDownloads_ColumnRightClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            this.MenuListHdrs.Show(this.ListDownloads, this.ListDownloads.PointToClient(Cursor.Position));
        }

        private void ListDownloads_ColumnWidthChanged(object sender, System.Windows.Forms.ColumnWidthChangedEventArgs e)
        {
            // Save the updated column's width
            this.downloadColSizes[(int)this.downloadColOrder[e.ColumnIndex]] = this.ListDownloads.Columns[e.ColumnIndex].Width;

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

        private void ListDownloads_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListDownloads.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonPlay_Click();
        }

        private void ListDownloads_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.ListDownloads.SelectedItems.Count > 0)
            {
                this.ShowDownloadInfo(Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
            }
            else
            {
                this.SetViewDefaults(); // Revert back to downloads view default sidebar and toolbar
            }
        }

        private void ShowDownloadInfo(int epid)
        {
            Model.Download info = new Model.Download(epid);

            string infoText = string.Empty;

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.ButtonCleanUp);

            if (info.Description != null)
            {
                infoText += info.Description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + info.EpisodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(info.Duration);

            switch (info.Status)
            {
                case Model.Download.DownloadStatus.Downloaded:
                    if (File.Exists(info.DownloadPath))
                    {
                        buttons.Add(this.ButtonPlay);
                    }

                    buttons.Add(this.ButtonDelete);
                    infoText += Environment.NewLine + "Play count: " + info.PlayCount.ToString(CultureInfo.CurrentCulture);

                    break;
                case Model.Download.DownloadStatus.Errored:
                    string errorName = string.Empty;
                    string errorDetails = info.ErrorDetails;

                    buttons.Add(this.ButtonRetry);
                    buttons.Add(this.ButtonCancel);

                    switch (info.ErrorType)
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
                            buttons.Add(this.ButtonReportError);
                            break;
                    }

                    infoText += Environment.NewLine + Environment.NewLine + "Error: " + errorName;

                    if (!string.IsNullOrEmpty(errorDetails))
                    {
                        infoText += Environment.NewLine + Environment.NewLine + errorDetails;
                    }

                    break;
                default:
                    buttons.Add(this.ButtonCancel);
                    break;
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(TextUtils.StripDateFromName(info.Name, info.EpisodeDate), infoText, this.progData.FetchEpisodeImage(epid));
        }

        private void SetSideBar(string title, string description, Bitmap picture)
        {
            this.LabelSidebarTitle.Text = title;
            this.TextSidebarDescript.Text = description;

            // Make sure the scrollbars update correctly
            this.TextSidebarDescript.ScrollBars = RichTextBoxScrollBars.None;
            this.TextSidebarDescript.ScrollBars = RichTextBoxScrollBars.Both;

            if (picture != null)
            {
                if (picture.Width > this.ImageSidebar.MaximumSize.Width || picture.Height > this.ImageSidebar.MaximumSize.Height)
                {
                    int newWidth = 0;
                    int newHeight = 0;

                    if (picture.Width > picture.Height)
                    {
                        newWidth = this.ImageSidebar.MaximumSize.Width;
                        newHeight = (int)((newWidth / (float)picture.Width) * picture.Height);
                    }
                    else
                    {
                        newHeight = this.ImageSidebar.MaximumSize.Height;
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

                this.ImageSidebar.Image = picture;
                this.ImageSidebar.Visible = true;
            }
            else
            {
                this.ImageSidebar.Visible = false;
            }
        }

        private void SetToolbarButtons(ToolBarButton[] buttons)
        {
            this.ButtonChooseProgramme.Visible = false;
            this.ButtonDownload.Visible = false;
            this.ButtonAddFavourite.Visible = false;
            this.ButtonRemFavourite.Visible = false;
            this.ButtonSubscribe.Visible = false;
            this.ButtonUnsubscribe.Visible = false;
            this.ButtonCurrentEps.Visible = false;
            this.ButtonPlay.Visible = false;
            this.ButtonCancel.Visible = false;
            this.ButtonDelete.Visible = false;
            this.ButtonRetry.Visible = false;
            this.ButtonReportError.Visible = false;
            this.ButtonCleanUp.Visible = false;

            foreach (ToolBarButton button in buttons)
            {
                button.Visible = true;
            }
        }

        private void MenuTrayShow_Click(object sender, System.EventArgs e)
        {
            if (!this.Visible)
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

        private void MenuTrayExit_Click(object eventSender, System.EventArgs eventArgs)
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

        private void NotifyIcon_BalloonTipClicked(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.ShownTrayBalloon = true;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.MenuTrayShow_Click(sender, e);
        }

        private void ButtonFindNew_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);
        }

        private void ButtonFavourites_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites);
        }

        private void ButtonSubscriptions_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions);
        }

        private void ButtonDownloads_Click(object sender, System.EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
        }

        private void ProgData_ProviderAdded(System.Guid providerId)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_ProviderAdded(providerId); });
                return;
            }

            Data.ProviderData info = this.progData.FetchProviderData(providerId);

            ListViewItem addItem = new ListViewItem();
            addItem.Name = providerId.ToString();
            addItem.Text = info.Name;

            if (info.Icon != null)
            {
                this.ImagesProviders.Images.Add(providerId.ToString(), info.Icon);
                addItem.ImageKey = providerId.ToString();
            }
            else
            {
                addItem.ImageKey = "default";
            }

            this.ListProviders.Items.Add(addItem);

            // Hide the 'No providers' provider options menu item
            if (this.MenuOptionsProviderOptsNoProvs.Visible)
            {
                this.MenuOptionsProviderOptsNoProvs.Visible = false;
            }

            MenuItem addMenuItem = new MenuItem(info.Name + " Provider");

            if (info.ShowOptionsHandler != null)
            {
                addMenuItem.Click += info.ShowOptionsHandler;
            }
            else
            {
                addMenuItem.Enabled = false;
            }

            this.MenuOptionsProviderOpts.MenuItems.Add(addMenuItem);

            if (this.view.CurrentView == ViewState.View.FindNewChooseProvider)
            {
                if (this.ListProviders.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_ProgrammeUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_ProgrammeUpdated(progid); });
                return;
            }

            if (this.view.CurrentView == ViewState.View.ProgEpisodes)
            {
                if ((int)this.view.CurrentViewData == progid)
                {
                    if (this.ListEpisodes.SelectedItems.Count == 0)
                    {
                        // Update the displayed programme information
                        this.ShowProgrammeInfo(progid);
                    }
                    else
                    {
                        // Update the displayed episode information (in case the subscription status has changed)
                        int epid = Convert.ToInt32(this.ListEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                        this.ShowEpisodeInfo(epid);
                    }
                }
            }
        }

        private void ShowProgrammeInfo(int progid)
        {
            Model.Programme progInfo = new Model.Programme((int)this.view.CurrentViewData);

            List<ToolBarButton> buttons = new List<ToolBarButton>();

            if (Model.Favourite.IsFavourite(progid))
            {
                buttons.Add(this.ButtonRemFavourite);
            }
            else
            {
                buttons.Add(this.ButtonAddFavourite);
            }

            if (Model.Subscription.IsSubscribed(progid))
            {
                buttons.Add(this.ButtonUnsubscribe);
            }
            else
            {
                buttons.Add(this.ButtonSubscribe);
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(progInfo.Name, progInfo.Description, this.progData.FetchProgrammeImage(progid));
        }

        private void EpisodeListItem(int epid, Model.Episode info, ref ListViewItem item)
        {
            item.Name = epid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.EpisodeDate.ToShortDateString();
            item.SubItems[1].Text = TextUtils.StripDateFromName(info.Name, info.EpisodeDate);
            item.Checked = info.AutoDownload;
        }

        private void ProgData_EpisodeAdded(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_EpisodeAdded(epid); });
                return;
            }

            Model.Episode info = new Model.Episode(epid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add(string.Empty);

            this.EpisodeListItem(epid, info, ref addItem);

            this.ListEpisodes.ItemCheck -= this.ListEpisodes_ItemCheck;
            this.ListEpisodes.Items.Add(addItem);
            this.ListEpisodes.ItemCheck += this.ListEpisodes_ItemCheck;
        }

        private void ProgData_FavouriteAdded(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_FavouriteAdded(progid); });
                return;
            }

            Model.Favourite info = new Model.Favourite(progid);

            this.ListFavourites.Items.Add(this.FavouriteListItem(info, null));

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.ListFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_FavouriteUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_FavouriteUpdated(progid); });
                return;
            }

            Model.Favourite info = new Model.Favourite(progid);
            ListViewItem item = this.ListFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)];

            item = this.FavouriteListItem(info, item);

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.ListFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected)
                {
                    this.ShowFavouriteInfo(progid);
                }
                else if (this.ListFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private ListViewItem FavouriteListItem(Model.Favourite info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
                item.SubItems.Add(string.Empty);
            }

            item.Name = info.Progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.Name;

            item.SubItems[1].Text = info.ProviderName;
            item.ImageKey = "favourite";

            return item;
        }

        private void ProgData_FavouriteRemoved(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_FavouriteRemoved(progid); });
                return;
            }

            this.ListFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.ListFavourites.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void SubscriptionListItem(int progid, Model.Subscription info, ref ListViewItem item)
        {
            item.Name = progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.Name;

            if (info.LatestDownload == null)
            {
                item.SubItems[1].Text = "Never";
            }
            else
            {
                item.SubItems[1].Text = info.LatestDownload.Value.ToShortDateString();
            }

            item.SubItems[2].Text = info.ProviderName;

            if (info.SingleEpisode)
            {
                item.ImageKey = "subscribed";
            }
            else
            {
                item.ImageKey = "subscribed_multi";
            }
        }

        private void ProgData_SubscriptionAdded(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_SubscriptionAdded(progid); });
                return;
            }

            Model.Subscription info = new Model.Subscription(progid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add(string.Empty);
            addItem.SubItems.Add(string.Empty);

            this.SubscriptionListItem(progid, info, ref addItem);
            this.ListSubscribed.Items.Add(addItem);

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.ListSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_SubscriptionUpdated(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_SubscriptionUpdated(progid); });
                return;
            }

            Model.Subscription info = new Model.Subscription(progid);
            ListViewItem item = this.ListSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)];

            this.SubscriptionListItem(progid, info, ref item);

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.ListSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected)
                {
                    this.ShowSubscriptionInfo(progid);
                }
                else if (this.ListSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_SubscriptionRemoved(int progid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_SubscriptionRemoved(progid); });
                return;
            }

            this.ListSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.ListSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private ListViewItem DownloadListItem(Model.Download info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
            }

            item.Name = info.Epid.ToString(CultureInfo.InvariantCulture);

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
                        item.SubItems[column].Text = TextUtils.StripDateFromName(info.Name, info.EpisodeDate);
                        break;
                    case Data.DownloadCols.EpisodeDate:
                        item.SubItems[column].Text = info.EpisodeDate.ToShortDateString();
                        break;
                    case Data.DownloadCols.Status:
                        switch (info.Status)
                        {
                            case Model.Download.DownloadStatus.Waiting:
                                item.SubItems[column].Text = "Waiting";
                                break;
                            case Model.Download.DownloadStatus.Downloaded:
                                if (info.PlayCount == 0)
                                {
                                    item.SubItems[column].Text = "Newly Downloaded";
                                }
                                else
                                {
                                    item.SubItems[column].Text = "Downloaded";
                                }

                                break;
                            case Model.Download.DownloadStatus.Errored:
                                item.SubItems[column].Text = "Error";
                                break;
                            default:
                                throw new InvalidDataException("Unknown status of " + info.Status.ToString());
                        }

                        break;
                    case Data.DownloadCols.Progress:
                        item.SubItems[column].Text = string.Empty;
                        break;
                    case Data.DownloadCols.Duration:
                        string durationText = string.Empty;

                        if (info.Duration != 0)
                        {
                            int mins = Convert.ToInt32(Math.Round(info.Duration / (decimal)60, 0));
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

            switch (info.Status)
            {
                case Model.Download.DownloadStatus.Waiting:
                    item.ImageKey = "waiting";
                    break;
                case Model.Download.DownloadStatus.Downloaded:
                    if (info.PlayCount == 0)
                    {
                        item.ImageKey = "downloaded_new";
                    }
                    else
                    {
                        item.ImageKey = "downloaded";
                    }

                    break;
                case Model.Download.DownloadStatus.Errored:
                    item.ImageKey = "error";
                    break;
                default:
                    throw new InvalidDataException("Unknown status of " + info.Status.ToString());
            }

            return item;
        }

        private void ProgData_DownloadAdded(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_DownloadAdded(epid); });
                return;
            }

            Model.Download info = new Model.Download(epid);
            this.ListDownloads.Items.Add(this.DownloadListItem(info, null));

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (this.ListDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_DownloadProgress(int epid, int percent, string statusText, ProgressIcon icon)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_DownloadProgress(epid, percent, statusText, icon); });
                return;
            }

            ListViewItem item = this.ListDownloads.Items[Convert.ToString(epid, CultureInfo.InvariantCulture)];

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
                this.ProgressDownload.Value = percent;

                if (this.ListDownloads.Controls.Count == 0)
                {
                    this.ListDownloads.AddProgressBar(ref this.ProgressDownload, item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress));
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

        private void ProgData_DownloadRemoved(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_DownloadRemoved(epid); });
                return;
            }

            ListViewItem item = this.ListDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];

            if (item != null)
            {
                if (this.downloadColOrder.Contains(Data.DownloadCols.Progress))
                {
                    if (this.ListDownloads.GetProgressBar(item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null)
                    {
                        this.ListDownloads.RemoveProgressBar(ref this.ProgressDownload);
                    }
                }

                item.Remove();
            }

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (this.ListDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_DownloadUpdated(int epid)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_DownloadUpdated(epid); });
                return;
            }

            Model.Download info = new Model.Download(epid);

            ListViewItem item = this.ListDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];
            item = this.DownloadListItem(info, item);

            if (this.downloadColOrder.Contains(Data.DownloadCols.Progress))
            {
                if (this.ListDownloads.GetProgressBar(item, this.downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null)
                {
                    this.ListDownloads.RemoveProgressBar(ref this.ProgressDownload);
                }
            }

            // Update the downloads list sorting, as the order may now have changed
            this.ListDownloads.Sort();

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (item.Selected)
                {
                    this.ShowDownloadInfo(epid);
                }
                else if (this.ListDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void ProgData_DownloadProgressTotal(bool downloading, int percent)
        {
            if (this.InvokeRequired)
            {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { this.ProgData_DownloadProgressTotal(downloading, percent); });
                return;
            }

            if (!this.IsDisposed)
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

        private void ProgData_FindNewViewChange(object viewData)
        {
            FindNewViewData findViewData = (FindNewViewData)this.view.CurrentViewData;
            findViewData.View = viewData;

            this.view.StoreView(findViewData);
        }

        private void ProgData_FoundNew(int progid)
        {
            this.view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void Main_Shown(object sender, System.EventArgs e)
        {
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                if (commandLineArg.ToUpperInvariant() == "/HIDEMAINWINDOW")
                {
                    if (OsUtils.WinSevenOrLater() && !Properties.Settings.Default.CloseToSystray)
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

        private void TimerCheckForUpdates_Tick(object sender, System.EventArgs e)
        {
            if (this.checkUpdate.IsUpdateAvailable())
            {
                if (Properties.Settings.Default.LastUpdatePrompt.AddDays(7) < DateAndTime.Now)
                {
                    Properties.Settings.Default.LastUpdatePrompt = DateAndTime.Now;
                    Properties.Settings.Default.Save(); // Save the last prompt time in case of unexpected termination

                    using (UpdateNotify showUpdate = new UpdateNotify())
                    {
                        if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                        {
                            showUpdate.StartPosition = FormStartPosition.CenterScreen;
                        }

                        if (showUpdate.ShowDialog(this) == DialogResult.Yes)
                        {
                            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/update.php?prevver=" + Application.ProductVersion);
                        }
                    }
                }
            }
        }

        private void ButtonAddFavourite_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
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

        private void ButtonRemFavourite_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to remove this programme from your list of favourites?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.RemoveFavourite(progid);
            }
        }

        private void ButtonSubscribe_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (this.progData.AddSubscription(progid))
            {
                this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions, null);
            }
            else
            {
                MessageBox.Show("This programme only has one episode, which is already in the download list!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ButtonUnsubscribe_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to stop having new episodes of this programme downloaded automatically?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.RemoveSubscription(progid);
            }
        }

        private void ButtonCancel_Click()
        {
            int epid = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (Interaction.MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                this.progData.DownloadRemove(epid);
            }
        }

        private void ButtonPlay_Click()
        {
            int epid = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Model.Download info = new Model.Download(epid);

            if (info.Status == Model.Download.DownloadStatus.Downloaded)
            {
                if (File.Exists(info.DownloadPath))
                {
                    Process.Start(info.DownloadPath);

                    // Bump the play count of this item up by one
                    this.progData.DownloadBumpPlayCount(epid);
                }
            }
        }

        private void ButtonDelete_Click()
        {
            int epid = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Model.Download info = new Model.Download(epid);

            bool fileExists = File.Exists(info.DownloadPath);
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
                        File.Delete(info.DownloadPath);
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

        private void ButtonRetry_Click()
        {
            this.progData.ResetDownload(Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
        }

        private void ButtonDownload_Click()
        {
            int epid = Convert.ToInt32(this.ListEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (this.progData.AddDownload(epid))
            {
                this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
            }
            else
            {
                Interaction.MsgBox("This episode is already in the download list!", MsgBoxStyle.Exclamation);
            }
        }

        private void ButtonCurrentEps_Click()
        {
            int progid = 0;

            switch (this.view.CurrentView)
            {
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            this.view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void ButtonReportError_Click()
        {
            int episodeID = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            this.progData.DownloadReportError(episodeID);
        }

        private void ButtonChooseProgramme_Click()
        {
            FindNewViewData viewData = default(FindNewViewData);
            viewData.ProviderID = new Guid(this.ListProviders.SelectedItems[0].Name);
            viewData.View = null;

            this.view.SetView(ViewState.View.FindNewProviderForm, viewData);
        }

        private void MenuOptionsShowOpts_Click(object sender, System.EventArgs e)
        {
            using (Preferences prefs = new Preferences())
            {
                prefs.ShowDialog();
            }
        }

        private void MenuOptionsExit_Click(object sender, System.EventArgs e)
        {
            this.MenuTrayExit_Click(sender, e);
        }

        private void MenuHelpAbout_Click(object sender, System.EventArgs e)
        {
            using (About about = new About())
            {
                about.ShowDialog();
            }
        }

        private void MenuHelpShowHelp_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/help/");
        }

        private void MenuHelpReportBug_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/bug_report.php");
        }

        private void ButtonCleanUp_Click()
        {
            using (CleanUp cleanUp = new CleanUp())
            {
                cleanUp.ShowDialog();
            }
        }

        private void View_UpdateNavBtnState(bool enableBack, bool enableFwd)
        {
            this.ButtonBack.Enabled = enableBack;
            this.ButtonForward.Enabled = enableFwd;
        }

        private void View_ViewChanged(ViewState.View view, ViewState.MainTab tab, object data)
        {
            this.ButtonFindNew.Checked = false;
            this.ButtonFavourites.Checked = false;
            this.ButtonSubscriptions.Checked = false;
            this.ButtonDownloads.Checked = false;

            switch (tab)
            {
                case ViewState.MainTab.FindProgramme:
                    this.ButtonFindNew.Checked = true;
                    break;
                case ViewState.MainTab.Favourites:
                    this.ButtonFavourites.Checked = true;
                    break;
                case ViewState.MainTab.Subscriptions:
                    this.ButtonSubscriptions.Checked = true;
                    break;
                case ViewState.MainTab.Downloads:
                    this.ButtonDownloads.Checked = true;
                    break;
            }

            this.SetViewDefaults();

            // Set the focus to a control which does not show it, to prevent the toolbar momentarily showing focus
            this.LabelSidebarTitle.Focus();

            this.ListProviders.Visible = false;
            this.PanelPluginSpace.Visible = false;
            this.ListEpisodes.Visible = false;
            this.ListFavourites.Visible = false;
            this.ListSubscribed.Visible = false;
            this.ListDownloads.Visible = false;
            this.TextSearch.Visible = false;

            switch (view)
            {
                case ViewState.View.FindNewChooseProvider:
                    this.ListProviders.Visible = true;
                    this.ListProviders.Focus();

                    if (this.ListProviders.SelectedItems.Count > 0)
                    {
                        this.ShowProviderInfo(new Guid(this.ListProviders.SelectedItems[0].Name));
                    }

                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData findViewData = (FindNewViewData)data;

                    if (this.PanelPluginSpace.Controls.Count > 0)
                    {
                        this.PanelPluginSpace.Controls[0].Dispose();
                        this.PanelPluginSpace.Controls.Clear();
                    }

                    this.PanelPluginSpace.Visible = true;
                    this.PanelPluginSpace.Controls.Add(this.progData.GetFindNewPanel(findViewData.ProviderID, findViewData.View));
                    this.PanelPluginSpace.Controls[0].Dock = DockStyle.Fill;
                    this.PanelPluginSpace.Controls[0].Focus();
                    break;
                case ViewState.View.ProgEpisodes:
                    this.ListEpisodes.Visible = true;
                    this.progData.CancelEpisodeListing();
                    this.ListEpisodes.Items.Clear(); // Clear before DoEvents so that old items don't flash up on screen
                    Application.DoEvents(); // Give any queued BeginInvoke calls a chance to be processed
                    this.ListEpisodes.Items.Clear();
                    this.progData.InitEpisodeList((int)data);
                    break;
                case ViewState.View.Favourites:
                    this.ListFavourites.Visible = true;
                    this.ListFavourites.Focus();

                    if (this.ListFavourites.SelectedItems.Count > 0)
                    {
                        this.ShowFavouriteInfo(Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }

                    break;
                case ViewState.View.Subscriptions:
                    this.ListSubscribed.Visible = true;
                    this.ListSubscribed.Focus();

                    if (this.ListSubscribed.SelectedItems.Count > 0)
                    {
                        this.ShowSubscriptionInfo(Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }

                    break;
                case ViewState.View.Downloads:
                    if (data == null)
                    {
                        this.TextSearch.Text = string.Empty;
                    }
                    else
                    {
                        this.TextSearch.Text = (string)data;
                        this.PerformSearch(view, this.TextSearch.Text);
                    }

                    this.TextSearch.Visible = true;
                    this.ListDownloads.Visible = true;
                    this.ListDownloads.Focus();

                    if (this.ListDownloads.SelectedItems.Count > 0)
                    {
                        this.ShowDownloadInfo(Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
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
                    this.SetSideBar(Convert.ToString(this.ListProviders.Items.Count, CultureInfo.CurrentCulture) + " provider" + (this.ListProviders.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData findViewData = (FindNewViewData)this.view.CurrentViewData;
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.ShowProviderInfo(findViewData.ProviderID);
                    break;
                case ViewState.View.ProgEpisodes:
                    this.ShowProgrammeInfo((int)this.view.CurrentViewData);
                    break;
                case ViewState.View.Favourites:
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.SetSideBar(Convert.ToString(this.ListFavourites.Items.Count, CultureInfo.CurrentCulture) + " favourite" + (this.ListFavourites.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.Subscriptions:
                    this.SetToolbarButtons(new ToolBarButton[] { });
                    this.SetSideBar(Convert.ToString(this.ListSubscribed.Items.Count, CultureInfo.CurrentCulture) + " subscription" + (this.ListSubscribed.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    break;
                case ViewState.View.Downloads:
                    this.SetToolbarButtons(new ToolBarButton[] { this.ButtonCleanUp });

                    if (!string.IsNullOrEmpty(this.progData.DownloadQuery))
                    {
                        this.SetSideBar(Convert.ToString(this.ListDownloads.Items.Count, CultureInfo.CurrentCulture) + " result" + (this.ListDownloads.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    }
                    else
                    {
                        string description = string.Empty;
                        int newCount = Model.Download.CountNew();
                        int errorCount = Model.Download.CountErrored();

                        if (newCount > 0)
                        {
                            description += "Newly downloaded: " + Convert.ToString(newCount, CultureInfo.CurrentCulture) + Environment.NewLine;
                        }

                        if (errorCount > 0)
                        {
                            description += "Errored: " + Convert.ToString(errorCount, CultureInfo.CurrentCulture);
                        }

                        this.SetSideBar(Convert.ToString(ListDownloads.Items.Count, CultureInfo.CurrentCulture) + " download" + (ListDownloads.Items.Count == 1 ? string.Empty : "s"), description, null);
                    }

                    break;
            }
        }

        private void ButtonBack_Click(object sender, System.EventArgs e)
        {
            this.view.NavBack();
        }

        private void ButtonForward_Click(object sender, System.EventArgs e)
        {
            this.view.NavFwd();
        }

        private void ToolbarMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.Name)
            {
                case "ButtonChooseProgramme":
                    this.ButtonChooseProgramme_Click();
                    break;
                case "ButtonDownload":
                    this.ButtonDownload_Click();
                    break;
                case "ButtonAddFavourite":
                    this.ButtonAddFavourite_Click();
                    break;
                case "ButtonRemFavourite":
                    this.ButtonRemFavourite_Click();
                    break;
                case "ButtonSubscribe":
                    this.ButtonSubscribe_Click();
                    break;
                case "ButtonUnsubscribe":
                    this.ButtonUnsubscribe_Click();
                    break;
                case "ButtonCurrentEps":
                    this.ButtonCurrentEps_Click();
                    break;
                case "ButtonCancel":
                    this.ButtonCancel_Click();
                    break;
                case "ButtonPlay":
                    this.ButtonPlay_Click();
                    break;
                case "ButtonDelete":
                    this.ButtonDelete_Click();
                    break;
                case "ButtonRetry":
                    this.ButtonRetry_Click();
                    break;
                case "ButtonReportError":
                    this.ButtonReportError_Click();
                    break;
                case "ButtonCleanUp":
                    this.ButtonCleanUp_Click();
                    break;
            }
        }

        private void TableToolbars_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.TableToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, this.TableToolbars.Width - (this.ButtonHelpMenu.Rectangle.Width + this.ToolbarHelp.Margin.Right));
                this.TableToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, this.ButtonHelpMenu.Rectangle.Width + this.ToolbarHelp.Margin.Right);

                if (VisualStyleRenderer.IsSupported)
                {
                    // Visual styles are enabled, so draw the correct background behind the toolbars
                    Bitmap bmpBackground = new Bitmap(this.TableToolbars.Width, this.TableToolbars.Height);
                    Graphics graGraphics = Graphics.FromImage(bmpBackground);

                    try
                    {
                        VisualStyleRenderer vsrRebar = new VisualStyleRenderer("Rebar", 0, 0);
                        vsrRebar.DrawBackground(graGraphics, new Rectangle(0, 0, this.TableToolbars.Width, this.TableToolbars.Height));
                        this.TableToolbars.BackgroundImage = bmpBackground;
                    }
                    catch (ArgumentException)
                    {
                        // The 'Rebar' background image style did not exist, so don't try to draw it.
                    }
                }
            }
        }

        private void TextSidebarDescript_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void TextSidebarDescript_Resize(object sender, System.EventArgs e)
        {
            this.TextSidebarDescript.Refresh(); // Make sure the scrollbars update correctly
        }

        private void ImageSidebarBorder_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(255, 167, 186, 197)), 0, 0, 0, this.ImageSidebarBorder.Height);
        }

        private void MenuListHdrsColumns_Click(object sender, System.EventArgs e)
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

        private void MenuListHdrsReset_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.DownloadCols = (string)Properties.Settings.Default.Properties["DownloadCols"].DefaultValue;
            Properties.Settings.Default.DownloadColSizes = (string)Properties.Settings.Default.Properties["DownloadColSizes"].DefaultValue;
            Properties.Settings.Default.DownloadColSortBy = Convert.ToInt32(Properties.Settings.Default.Properties["DownloadColSortBy"].DefaultValue, CultureInfo.InvariantCulture);
            Properties.Settings.Default.DownloadColSortAsc = Convert.ToBoolean(Properties.Settings.Default.Properties["DownloadColSortAsc"].DefaultValue, CultureInfo.InvariantCulture);

            this.InitDownloadList();
        }

        private void InitFavouriteList()
        {
            if (this.ListFavourites.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            // Convert the list of FavouriteData items to an array of ListItems
            List<Model.Favourite> initData = this.progData.FetchFavouriteList();
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++)
            {
                initItems[convItems] = this.FavouriteListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.ListFavourites.Items.AddRange(initItems);
        }

        private void InitDownloadList()
        {
            if (this.ListDownloads.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            this.downloadColSizes.Clear();
            this.downloadColOrder.Clear();
            this.ListDownloads.Clear();
            this.ListDownloads.RemoveAllControls();

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
                    this.ListDownloads.Columns.Add(this.downloadColNames[colVal], this.downloadColSizes[colVal]);
                }
            }

            // Apply the sort from the current settings
            this.progData.DownloadSortByCol = (Data.DownloadCols)Properties.Settings.Default.DownloadColSortBy;
            this.progData.DownloadSortAscending = Properties.Settings.Default.DownloadColSortAsc;
            this.ListDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(this.progData.DownloadSortByCol), this.progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of DownloadData items to an array of ListItems
            List<Model.Download> initData = this.progData.FetchDownloadList(true);
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++)
            {
                initItems[convItems] = this.DownloadListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.ListDownloads.Items.AddRange(initItems);
        }

        private void TextSearch_TextChanged(object sender, System.EventArgs e)
        {
            lock (this.searchThreadLock)
            {
                if (string.IsNullOrEmpty(this.TextSearch.Text))
                {
                    this.searchThread = null;
                    this.PerformSearch(this.view.CurrentView, this.TextSearch.Text);
                }
                else
                {
                    this.searchThread = new Thread(() => this.SearchWait(this.view.CurrentView, this.TextSearch.Text));
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

        private struct FindNewViewData
        {
            public Guid ProviderID;
            public object View;
        }
    }
}

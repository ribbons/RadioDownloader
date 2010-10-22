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

namespace RadioDld
{

    internal partial class Main : GlassForm
    {
        private struct FindNewViewData
        {
            public Guid ProviderID;
            public object View;
        }
        private Data withEventsField_progData;

        private Data progData {
            get { return withEventsField_progData; }
            set {
                if (withEventsField_progData != null) {
                    withEventsField_progData.ProviderAdded -= progData_ProviderAdded;
                    withEventsField_progData.ProgrammeUpdated -= progData_ProgrammeUpdated;
                    withEventsField_progData.EpisodeAdded -= progData_EpisodeAdded;
                    withEventsField_progData.FavouriteAdded -= progData_FavouriteAdded;
                    withEventsField_progData.FavouriteUpdated -= progData_FavouriteUpdated;
                    withEventsField_progData.FavouriteRemoved -= progData_FavouriteRemoved;
                    withEventsField_progData.SubscriptionAdded -= progData_SubscriptionAdded;
                    withEventsField_progData.SubscriptionUpdated -= progData_SubscriptionUpdated;
                    withEventsField_progData.SubscriptionRemoved -= progData_SubscriptionRemoved;
                    withEventsField_progData.DownloadAdded -= progData_DownloadAdded;
                    withEventsField_progData.DownloadProgress -= progData_DownloadProgress;
                    withEventsField_progData.DownloadRemoved -= progData_DownloadRemoved;
                    withEventsField_progData.DownloadUpdated -= progData_DownloadUpdated;
                    withEventsField_progData.DownloadProgressTotal -= progData_DownloadProgressTotal;
                    withEventsField_progData.FindNewViewChange -= progData_FindNewViewChange;
                    withEventsField_progData.FoundNew -= progData_FoundNew;
                }
                withEventsField_progData = value;
                if (withEventsField_progData != null) {
                    withEventsField_progData.ProviderAdded += progData_ProviderAdded;
                    withEventsField_progData.ProgrammeUpdated += progData_ProgrammeUpdated;
                    withEventsField_progData.EpisodeAdded += progData_EpisodeAdded;
                    withEventsField_progData.FavouriteAdded += progData_FavouriteAdded;
                    withEventsField_progData.FavouriteUpdated += progData_FavouriteUpdated;
                    withEventsField_progData.FavouriteRemoved += progData_FavouriteRemoved;
                    withEventsField_progData.SubscriptionAdded += progData_SubscriptionAdded;
                    withEventsField_progData.SubscriptionUpdated += progData_SubscriptionUpdated;
                    withEventsField_progData.SubscriptionRemoved += progData_SubscriptionRemoved;
                    withEventsField_progData.DownloadAdded += progData_DownloadAdded;
                    withEventsField_progData.DownloadProgress += progData_DownloadProgress;
                    withEventsField_progData.DownloadRemoved += progData_DownloadRemoved;
                    withEventsField_progData.DownloadUpdated += progData_DownloadUpdated;
                    withEventsField_progData.DownloadProgressTotal += progData_DownloadProgressTotal;
                    withEventsField_progData.FindNewViewChange += progData_FindNewViewChange;
                    withEventsField_progData.FoundNew += progData_FoundNew;
                }
            }
        }
        private ViewState withEventsField_view;
        private ViewState view {
            get { return withEventsField_view; }
            set {
                if (withEventsField_view != null) {
                    withEventsField_view.UpdateNavBtnState -= view_UpdateNavBtnState;
                    withEventsField_view.ViewChanged -= view_ViewChanged;
                }
                withEventsField_view = value;
                if (withEventsField_view != null) {
                    withEventsField_view.UpdateNavBtnState += view_UpdateNavBtnState;
                    withEventsField_view.ViewChanged += view_ViewChanged;
                }
            }

        }
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
            InitializeComponent();
        }

        public void UpdateTrayStatus(bool active)
        {
            if (OsUtils.WinSevenOrLater()) {
                if (progData.CountDownloadsErrored() > 0) {
                    tbarNotif.SetOverlayIcon(this, Properties.Resources.overlay_error, "Error");
                    tbarNotif.SetThumbnailTooltip(this, this.Text + ": Error");
                } else {
                    if (active == true) {
                        tbarNotif.SetOverlayIcon(this, Properties.Resources.overlay_downloading, "Downloading");
                        tbarNotif.SetThumbnailTooltip(this, this.Text + ": Downloading");
                    } else {
                        tbarNotif.SetOverlayIcon(this, null, string.Empty);
                        tbarNotif.SetThumbnailTooltip(this, null);
                    }
                }
            }

            if (nicTrayIcon.Visible) {
                if (progData.CountDownloadsErrored() > 0) {
                    nicTrayIcon.Icon = Properties.Resources.icon_error;
                    nicTrayIcon.Text = this.Text + ": Error";
                } else {
                    if (active == true) {
                        nicTrayIcon.Icon = Properties.Resources.icon_working;
                        nicTrayIcon.Text = this.Text + ": Downloading";
                    } else {
                        nicTrayIcon.Icon = Properties.Resources.icon_main;
                        nicTrayIcon.Text = this.Text;
                    }
                }
            }
        }

        private void Main_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode) {
                case Keys.F1:
                    e.Handled = true;
                    mnuHelpShowHelp_Click(sender, e);
                    break;
                case Keys.Delete:
                    if (tbtDelete.Visible) {
                        e.Handled = true;
                        tbtDelete_Click();
                    } else if (tbtCancel.Visible) {
                        e.Handled = true;
                        tbtCancel_Click();
                    }
                    break;
                case Keys.Back:
                    if (!object.ReferenceEquals(this.ActiveControl.GetType(), typeof(TextBox)) & !object.ReferenceEquals(this.ActiveControl.Parent.GetType(), typeof(ExtToolStrip))) {
                        if (e.Shift) {
                            if (tbtForward.Enabled) {
                                e.Handled = true;
                                tbtForward_Click(sender, e);
                            }
                        } else {
                            if (tbtBack.Enabled) {
                                e.Handled = true;
                                tbtBack_Click(sender, e);
                            }
                        }
                    }
                    break;
                case Keys.BrowserBack:
                    if (tbtBack.Enabled) {
                        e.Handled = true;
                        tbtBack_Click(sender, e);
                    }
                    break;
                case Keys.BrowserForward:
                    if (tbtForward.Enabled) {
                        e.Handled = true;
                        tbtForward_Click(sender, e);
                    }
                    break;
            }
        }

        private void Main_Load(System.Object eventSender, System.EventArgs eventArgs)
        {
            // If this is the first run of a new version of the application, then upgrade the settings from the old version.
            try {
                if (Properties.Settings.Default.UpgradeSettings) {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeSettings = false;
                    Properties.Settings.Default.Save();
                }
            } catch (ConfigurationErrorsException configErrorExp) {
                string fileName = null;

                if (configErrorExp.Filename != null) {
                    fileName = configErrorExp.Filename;
                } else if (configErrorExp.InnerException != null && configErrorExp.InnerException is ConfigurationErrorsException) {
                    ConfigurationErrorsException innerExp = (ConfigurationErrorsException)configErrorExp.InnerException;

                    if (innerExp.Filename != null) {
                        fileName = innerExp.Filename;
                    }
                }

                if (fileName != null) {
                    File.Delete(fileName);
                    Interaction.MsgBox("Your Radio Downloader configuration file has been reset as it was corrupt.  This only affects your settings and columns, not your subscriptions or downloads." + Environment.NewLine + Environment.NewLine + "You will need to start Radio Downloader again after closing this message.", MsgBoxStyle.Information);
                    this.Close();
                    this.Dispose();
                    return;
                } else {
                    throw;
                }
            }

            // Make sure that the temp and application data folders exist
            Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"));
            Directory.CreateDirectory(FileUtils.GetAppDataFolder());

            // Make sure that the database exists.  If not, then copy across the empty database from the program's folder.
            System.IO.FileInfo fileExits = new System.IO.FileInfo(Path.Combine(FileUtils.GetAppDataFolder(), "store.db"));

            if (fileExits.Exists == false) {
                try {
                    System.IO.File.Copy(Path.Combine(RadioDld.My.MyProject.Application.Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"));
                } catch (FileNotFoundException) {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(RadioDld.My.MyProject.Application.Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }
            } else {
                // As the database already exists, copy the specimen database across from the program folder
                // and then make sure that the current db's structure matches it.
                try {
                    System.IO.File.Copy(Path.Combine(RadioDld.My.MyProject.Application.Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), true);
                } catch (FileNotFoundException) {
                    Interaction.MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(RadioDld.My.MyProject.Application.Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                } catch (UnauthorizedAccessException) {
                    Interaction.MsgBox("Access was denied when attempting to copy the Radio Downloader template database." + Environment.NewLine + Environment.NewLine + "Check that you have read access to '" + Path.Combine(RadioDld.My.MyProject.Application.Info.DirectoryPath, "store.db") + "' and write access to '" + Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db") + "'.", MsgBoxStyle.Critical);
                    this.Close();
                    this.Dispose();
                    return;
                }

                using (UpdateDB doDbUpdate = new UpdateDB(Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"))) {
                    doDbUpdate.UpdateStructure();
                }
            }

            imlListIcons.Images.Add("downloading", Properties.Resources.list_downloading);
            imlListIcons.Images.Add("waiting", Properties.Resources.list_waiting);
            imlListIcons.Images.Add("converting", Properties.Resources.list_converting);
            imlListIcons.Images.Add("downloaded_new", Properties.Resources.list_downloaded_new);
            imlListIcons.Images.Add("downloaded", Properties.Resources.list_downloaded);
            imlListIcons.Images.Add("subscribed", Properties.Resources.list_subscribed);
            imlListIcons.Images.Add("error", Properties.Resources.list_error);
            imlListIcons.Images.Add("favourite", Properties.Resources.list_favourite);

            imlProviders.Images.Add("default", Properties.Resources.provider_default);

            imlToolbar.Images.Add("choose_programme", Properties.Resources.toolbar_choose_programme);
            imlToolbar.Images.Add("clean_up", Properties.Resources.toolbar_clean_up);
            imlToolbar.Images.Add("current_episodes", Properties.Resources.toolbar_current_episodes);
            imlToolbar.Images.Add("delete", Properties.Resources.toolbar_delete);
            imlToolbar.Images.Add("download", Properties.Resources.toolbar_download);
            imlToolbar.Images.Add("help", Properties.Resources.toolbar_help);
            imlToolbar.Images.Add("options", Properties.Resources.toolbar_options);
            imlToolbar.Images.Add("play", Properties.Resources.toolbar_play);
            imlToolbar.Images.Add("report_error", Properties.Resources.toolbar_report_error);
            imlToolbar.Images.Add("retry", Properties.Resources.toolbar_retry);
            imlToolbar.Images.Add("subscribe", Properties.Resources.toolbar_subscribe);
            imlToolbar.Images.Add("unsubscribe", Properties.Resources.toolbar_unsubscribe);
            imlToolbar.Images.Add("add_favourite", Properties.Resources.toolbar_add_favourite);
            imlToolbar.Images.Add("remove_favourite", Properties.Resources.toolbar_remove_favourite);

            tbrToolbar.ImageList = imlToolbar;
            tbrHelp.ImageList = imlToolbar;
            lstProviders.LargeImageList = imlProviders;
            lstFavourites.SmallImageList = imlListIcons;
            lstSubscribed.SmallImageList = imlListIcons;
            lstDownloads.SmallImageList = imlListIcons;

            lstEpisodes.Columns.Add("Date", Convert.ToInt32(0.179 * lstEpisodes.Width));
            lstEpisodes.Columns.Add("Episode Name", Convert.ToInt32(0.786 * lstEpisodes.Width));
            lstFavourites.Columns.Add("Programme Name", Convert.ToInt32(0.661 * lstFavourites.Width));
            lstFavourites.Columns.Add("Provider", Convert.ToInt32(0.304 * lstFavourites.Width));
            lstSubscribed.Columns.Add("Programme Name", Convert.ToInt32(0.482 * lstSubscribed.Width));
            lstSubscribed.Columns.Add("Last Download", Convert.ToInt32(0.179 * lstSubscribed.Width));
            lstSubscribed.Columns.Add("Provider", Convert.ToInt32(0.304 * lstSubscribed.Width));

            // NB - these are defined in alphabetical order to save sorting later
            downloadColNames.Add((int)Data.DownloadCols.EpisodeDate, "Date");
            downloadColNames.Add((int)Data.DownloadCols.Duration, "Duration");
            downloadColNames.Add((int)Data.DownloadCols.EpisodeName, "Episode Name");
            downloadColNames.Add((int)Data.DownloadCols.Progress, "Progress");
            downloadColNames.Add((int)Data.DownloadCols.Status, "Status");

            view = new ViewState();
            view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);

            progData = Data.GetInstance();

            progData.InitProviderList();
            InitFavouriteList();
            progData.InitSubscriptionList();
            InitDownloadList();

            lstFavourites.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Favourite);
            lstSubscribed.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Subscription);
            lstDownloads.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Download);

            if (OsUtils.WinSevenOrLater()) {
                // New style taskbar - initialise the taskbar notification class
                tbarNotif = new TaskbarNotify();
            }

            if (!OsUtils.WinSevenOrLater() | Properties.Settings.Default.CloseToSystray) {
                // Show a system tray icon
                nicTrayIcon.Visible = true;
            }

            // Set up the initial notification status
            UpdateTrayStatus(false);

            checkUpdate = new UpdateCheck("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=" + RadioDld.My.MyProject.Application.Info.Version.ToString());

            picSideBarBorder.Width = 2;

            lstProviders.Dock = DockStyle.Fill;
            pnlPluginSpace.Dock = DockStyle.Fill;
            lstEpisodes.Dock = DockStyle.Fill;
            lstFavourites.Dock = DockStyle.Fill;
            lstSubscribed.Dock = DockStyle.Fill;
            lstDownloads.Dock = DockStyle.Fill;

            this.Font = SystemFonts.MessageBoxFont;
            lblSideMainTitle.Font = new Font(this.Font.FontFamily, Convert.ToSingle(this.Font.SizeInPoints * 1.16), this.Font.Style, GraphicsUnit.Point);

            // Scale the max size of the sidebar image for values other than 96 dpi, as it is specified in pixels
            using (Graphics graphicsForDpi = this.CreateGraphics()) {
                picSidebarImg.MaximumSize = new Size(Convert.ToInt32(picSidebarImg.MaximumSize.Width * (graphicsForDpi.DpiX / 96)), Convert.ToInt32(picSidebarImg.MaximumSize.Height * (graphicsForDpi.DpiY / 96)));
            }

            if (Properties.Settings.Default.MainFormPos != Rectangle.Empty) {
                if (OsUtils.VisibleOnScreen(Properties.Settings.Default.MainFormPos)) {
                    this.StartPosition = FormStartPosition.Manual;
                    this.DesktopBounds = Properties.Settings.Default.MainFormPos;
                } else {
                    this.Size = Properties.Settings.Default.MainFormPos.Size;
                }

                this.WindowState = Properties.Settings.Default.MainFormState;
            }

            windowPosLoaded = true;

            tblToolbars.Height = tbrToolbar.Height;
            tbrToolbar.SetWholeDropDown(tbtOptionsMenu);
            tbrHelp.SetWholeDropDown(tbtHelpMenu);
            tbrHelp.Width = tbtHelpMenu.Rectangle.Width;

            if (this.WindowState != FormWindowState.Minimized) {
                tblToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, tblToolbars.Width - (tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right));
                tblToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right);
            }

            if (OsUtils.WinVistaOrLater() & VisualStyleRenderer.IsSupported) {
                tbrView.Margin = new Padding(0);
            }

            this.SetGlassMargins(0, 0, tbrView.Height, 0);
            tbrView.Renderer = new TabBarRenderer();

            OsUtils.ApplyRunOnStartup();

            progData.StartDownload();
            tmrCheckForUpdates.Enabled = true;
        }

        private void Main_FormClosing(System.Object eventSender, System.Windows.Forms.FormClosingEventArgs eventArgs)
        {
            if (eventArgs.CloseReason == CloseReason.UserClosing) {
                if (!nicTrayIcon.Visible) {
                    if (this.WindowState != FormWindowState.Minimized) {
                        this.WindowState = FormWindowState.Minimized;
                        eventArgs.Cancel = true;
                    }
                } else {
                    OsUtils.TrayAnimate(this, true);
                    this.Visible = false;
                    eventArgs.Cancel = true;

                    if (Properties.Settings.Default.ShownTrayBalloon == false) {
                        nicTrayIcon.BalloonTipIcon = ToolTipIcon.Info;
                        nicTrayIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + Environment.NewLine + "Click here to hide this message in future.";
                        nicTrayIcon.BalloonTipTitle = "Radio Downloader is still running";
                        nicTrayIcon.ShowBalloonTip(30000);
                    }
                }
            }
        }

        private void lstProviders_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            if (lstProviders.SelectedItems.Count > 0) {
                Guid pluginId = new Guid(lstProviders.SelectedItems[0].Name);
                ShowProviderInfo(pluginId);
            } else {
                SetViewDefaults();
            }
        }

        private void ShowProviderInfo(Guid providerId)
        {
            Data.ProviderData info = progData.FetchProviderData(providerId);
            SetSideBar(info.name, info.description, null);

            if (view.CurrentView == ViewState.View.FindNewChooseProvider) {
                SetToolbarButtons(new ToolBarButton[] { tbtChooseProgramme });
            }
        }

        private void lstProviders_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (lstProviders.SelectedItems.Count == 0) {
                return;
            }

            tbtChooseProgramme_Click();
        }

        private void lstEpisodes_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            progData.EpisodeSetAutoDownload(Convert.ToInt32(lstEpisodes.Items[e.Index].Name, CultureInfo.InvariantCulture), e.NewValue == CheckState.Checked);
        }

        private void ShowEpisodeInfo(int epid)
        {
            Data.ProgrammeData progInfo = progData.FetchProgrammeData((int)view.CurrentViewData);
            Data.EpisodeData epInfo = progData.FetchEpisodeData(epid);
            string infoText = "";

            if (epInfo.description != null) {
                infoText += epInfo.description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + epInfo.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(epInfo.duration);

            SetSideBar(epInfo.name, infoText, progData.FetchEpisodeImage(epid));

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(tbtDownload);

            if (progInfo.favourite) {
                buttons.Add(tbtRemFavourite);
            } else {
                buttons.Add(tbtAddFavourite);
            }

            if (progInfo.subscribed) {
                buttons.Add(tbtUnsubscribe);
            } else if (!progInfo.singleEpisode) {
                buttons.Add(tbtSubscribe);
            }

            SetToolbarButtons(buttons.ToArray());
        }

        private void lstEpisodes_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (lstEpisodes.SelectedItems.Count > 0) {
                int epid = Convert.ToInt32(lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                ShowEpisodeInfo(epid);
            } else {
                SetViewDefaults();
                // Revert back to programme info in sidebar
            }
        }

        private void lstFavourites_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (lstFavourites.SelectedItems.Count > 0) {
                int progid = Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                progData.UpdateProgInfoIfRequired(progid);
                ShowFavouriteInfo(progid);
            } else {
                SetViewDefaults();
                // Revert back to subscribed items view default sidebar and toolbar
            }
        }

        private void ShowFavouriteInfo(int progid)
        {
            Data.FavouriteData info = progData.FetchFavouriteData(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.AddRange(new ToolBarButton[] {tbtRemFavourite, tbtCurrentEps});

            if (info.subscribed) {
                buttons.Add(tbtUnsubscribe);
            } else if (!info.singleEpisode) {
                buttons.Add(tbtSubscribe);
            }

            SetToolbarButtons(buttons.ToArray());
            SetSideBar(info.name, info.description, progData.FetchProgrammeImage(progid));
        }

        private void lstSubscribed_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            if (lstSubscribed.SelectedItems.Count > 0) {
                int progid = Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                progData.UpdateProgInfoIfRequired(progid);
                ShowSubscriptionInfo(progid);
            } else {
                SetViewDefaults();
                // Revert back to subscribed items view default sidebar and toolbar
            }
        }

        private void ShowSubscriptionInfo(int progid)
        {
            Data.SubscriptionData info = progData.FetchSubscriptionData(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(tbtUnsubscribe);
            buttons.Add(tbtCurrentEps);

            if (info.favourite) {
                buttons.Add(tbtRemFavourite);
            } else {
                buttons.Add(tbtAddFavourite);
            }

            SetToolbarButtons(buttons.ToArray());

            SetSideBar(info.name, info.description, progData.FetchProgrammeImage(progid));
        }

        private void lstDownloads_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            Data.DownloadCols clickedCol = downloadColOrder[e.Column];

            if (clickedCol == Data.DownloadCols.Progress) {
                return;
            }

            if (progData.DownloadSortByCol != clickedCol) {
                progData.DownloadSortByCol = clickedCol;
                progData.DownloadSortAscending = true;
            } else {
                progData.DownloadSortAscending = !progData.DownloadSortAscending;
            }

            // Set the column header to display the new sort order
            lstDownloads.ShowSortOnHeader(downloadColOrder.IndexOf(progData.DownloadSortByCol), progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Properties.Settings.Default.DownloadColSortBy = (int)progData.DownloadSortByCol;
            Properties.Settings.Default.DownloadColSortAsc = progData.DownloadSortAscending;

            lstDownloads.Sort();
        }

        private void lstDownloads_ColumnReordered(object sender, System.Windows.Forms.ColumnReorderedEventArgs e)
        {
            string[] oldOrder = new string[lstDownloads.Columns.Count];

            // Fetch the pre-reorder column order
            foreach (ColumnHeader col in lstDownloads.Columns) {
                oldOrder[col.DisplayIndex] = ((int)downloadColOrder[col.Index]).ToString(CultureInfo.InvariantCulture);
            }

            List<string> newOrder = new List<string>(oldOrder);
            string moveCol = newOrder[e.OldDisplayIndex];

            // Re-order the data to match the new column order
            newOrder.RemoveAt(e.OldDisplayIndex);
            newOrder.Insert(e.NewDisplayIndex, moveCol);

            // Save the new column order to the preference
            Properties.Settings.Default.DownloadCols = Strings.Join(newOrder.ToArray(), ",");

            if (e.OldDisplayIndex == 0 || e.NewDisplayIndex == 0) {
                // The reorder involves column 0 which contains the icons, so re-initialise the list
                e.Cancel = true;
                InitDownloadList();
            }
        }

        private void lstDownloads_ColumnRightClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            mnuListHdrs.Show(lstDownloads, lstDownloads.PointToClient(Cursor.Position));
        }

        private void lstDownloads_ColumnWidthChanged(object sender, System.Windows.Forms.ColumnWidthChangedEventArgs e)
        {
            // Save the updated column's width
            downloadColSizes[(int)downloadColOrder[e.ColumnIndex]] = lstDownloads.Columns[e.ColumnIndex].Width;

            string saveColSizes = string.Empty;

            // Convert the stored column widths back to a string to save to settings
            foreach (KeyValuePair<int, int> colSize in downloadColSizes) {
                if (!string.IsNullOrEmpty(saveColSizes)) {
                    saveColSizes += "|";
                }

                saveColSizes += colSize.Key.ToString(CultureInfo.InvariantCulture) + "," + (colSize.Value / this.CurrentAutoScaleDimensions.Width).ToString(CultureInfo.InvariantCulture);
            }

            Properties.Settings.Default.DownloadColSizes = saveColSizes;
        }

        private void lstDownloads_ItemActivate(object sender, System.EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (lstDownloads.SelectedItems.Count == 0) {
                return;
            }

            tbtPlay_Click();
        }

        private void lstDownloads_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            if (lstDownloads.SelectedItems.Count > 0) {
                ShowDownloadInfo(Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
            } else {
                SetViewDefaults();
                // Revert back to downloads view default sidebar and toolbar
            }
        }

        private void ShowDownloadInfo(int epid)
        {
            Data.DownloadData info = progData.FetchDownloadData(epid);

            string infoText = string.Empty;

            if (info.description != null) {
                infoText += info.description + Environment.NewLine + Environment.NewLine;
            }

            infoText += "Date: " + info.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
            infoText += TextUtils.DescDuration(info.duration);

            switch (info.status) {
                case Data.DownloadStatus.Downloaded:
                    if (File.Exists(info.downloadPath)) {
                        SetToolbarButtons(new ToolBarButton[] {tbtPlay, tbtDelete});
                    } else {
                        SetToolbarButtons(new ToolBarButton[] {tbtDelete});
                    }

                    infoText += Environment.NewLine + "Play count: " + info.playCount.ToString(CultureInfo.CurrentCulture);
                    break;
                case Data.DownloadStatus.Errored:
                    string errorName = "";
                    string errorDetails = info.errorDetails;

                    ToolBarButton[] toolbarButtons = {
                        tbtRetry,
                        tbtCancel
                    };

                    switch (info.errorType) {
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
                            toolbarButtons = new ToolBarButton[] {
                                tbtRetry,
                                tbtCancel,
                                tbtReportError
                            };
                            break;
                    }

                    infoText += Environment.NewLine + Environment.NewLine + "Error: " + errorName;

                    if (!string.IsNullOrEmpty(errorDetails)) {
                        infoText += Environment.NewLine + Environment.NewLine + errorDetails;
                    }

                    SetToolbarButtons(toolbarButtons);
                    break;
                default:
                    SetToolbarButtons(new ToolBarButton[] { tbtCancel });
                    break;
            }

            SetSideBar(info.name, infoText, progData.FetchEpisodeImage(epid));
        }

        private void SetSideBar(string title, string description, Bitmap picture)
        {
            lblSideMainTitle.Text = title;

            txtSideDescript.Text = description;

            // Make sure the scrollbars update correctly
            txtSideDescript.ScrollBars = RichTextBoxScrollBars.None;
            txtSideDescript.ScrollBars = RichTextBoxScrollBars.Both;

            if (picture != null) {
                if (picture.Width > picSidebarImg.MaximumSize.Width | picture.Height > picSidebarImg.MaximumSize.Height) {
                    int newWidth = 0;
                    int newHeight = 0;

                    if (picture.Width > picture.Height) {
                        newWidth = picSidebarImg.MaximumSize.Width;
                        newHeight = (int)((newWidth / (float)picture.Width) * picture.Height);
                    } else {
                        newHeight = picSidebarImg.MaximumSize.Height;
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

                picSidebarImg.Image = picture;
                picSidebarImg.Visible = true;
            } else {
                picSidebarImg.Visible = false;
            }
        }

        private void SetToolbarButtons(ToolBarButton[] buttons)
        {
            tbtChooseProgramme.Visible = false;
            tbtDownload.Visible = false;
            tbtAddFavourite.Visible = false;
            tbtRemFavourite.Visible = false;
            tbtSubscribe.Visible = false;
            tbtUnsubscribe.Visible = false;
            tbtCurrentEps.Visible = false;
            tbtPlay.Visible = false;
            tbtCancel.Visible = false;
            tbtDelete.Visible = false;
            tbtRetry.Visible = false;
            tbtReportError.Visible = false;
            tbtCleanUp.Visible = false;

            foreach (ToolBarButton button in buttons) {
                button.Visible = true;
            }
        }

        public void mnuTrayShow_Click(System.Object sender, System.EventArgs e)
        {
            if (this.Visible == false) {
                OsUtils.TrayAnimate(this, false);
                this.Visible = true;
            }

            if (this.WindowState == FormWindowState.Minimized) {
                this.WindowState = FormWindowState.Normal;
            }

            this.Activate();
        }

        public void mnuTrayExit_Click(System.Object eventSender, System.EventArgs eventArgs)
        {
            this.Close();
            this.Dispose();

            try {
                Directory.Delete(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"), true);
            } catch (IOException) {
                // Ignore an IOException - this just means that a file in the temp folder is still in use.
            }
        }

        private void nicTrayIcon_BalloonTipClicked(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.ShownTrayBalloon = true;
        }

        private void nicTrayIcon_MouseDoubleClick(System.Object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mnuTrayShow_Click(sender, e);
        }

        private void tbtFindNew_Click(System.Object sender, System.EventArgs e)
        {
            view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);
        }

        private void tbtFavourites_Click(System.Object sender, System.EventArgs e)
        {
            view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites);
        }

        private void tbtSubscriptions_Click(System.Object sender, System.EventArgs e)
        {
            view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions);
        }

        private void tbtDownloads_Click(System.Object sender, System.EventArgs e)
        {
            view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
        }

        private void progData_ProviderAdded(System.Guid providerId)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_ProviderAdded(providerId); });
                return;
            }

            Data.ProviderData info = progData.FetchProviderData(providerId);

            ListViewItem addItem = new ListViewItem();
            addItem.Name = providerId.ToString();
            addItem.Text = info.name;

            if (info.icon != null) {
                imlProviders.Images.Add(providerId.ToString(), info.icon);
                addItem.ImageKey = providerId.ToString();
            } else {
                addItem.ImageKey = "default";
            }

            lstProviders.Items.Add(addItem);

            // Hide the 'No providers' provider options menu item
            if (mnuOptionsProviderOptsNoProvs.Visible == true) {
                mnuOptionsProviderOptsNoProvs.Visible = false;
            }

            MenuItem addMenuItem = new MenuItem(info.name + " Provider");

            if (info.showOptionsHandler != null) {
                addMenuItem.Click += info.showOptionsHandler;
            } else {
                addMenuItem.Enabled = false;
            }

            mnuOptionsProviderOpts.MenuItems.Add(addMenuItem);

            if (view.CurrentView == ViewState.View.FindNewChooseProvider) {
                if (lstProviders.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_ProgrammeUpdated(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_ProgrammeUpdated(progid); });
                return;
            }

            if (view.CurrentView == ViewState.View.ProgEpisodes) {
                if ((int)view.CurrentViewData == progid) {
                    if (lstEpisodes.SelectedItems.Count == 0) {
                        // Update the displayed programme information
                        ShowProgrammeInfo(progid);
                    } else {
                        // Update the displayed episode information (in case the subscription status has changed)
                        int epid = Convert.ToInt32(lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                        ShowEpisodeInfo(epid);
                    }
                }
            }
        }

        private void ShowProgrammeInfo(int progid)
        {
            Data.ProgrammeData progInfo = progData.FetchProgrammeData((int)view.CurrentViewData);

            List<ToolBarButton> buttons = new List<ToolBarButton>();

            if (progInfo.favourite) {
                buttons.Add(tbtRemFavourite);
            } else {
                buttons.Add(tbtAddFavourite);
            }

            if (progInfo.subscribed) {
                buttons.Add(tbtUnsubscribe);
            } else if (!progInfo.singleEpisode) {
                buttons.Add(tbtSubscribe);
            }

            SetToolbarButtons(buttons.ToArray());
            SetSideBar(progInfo.name, progInfo.description, progData.FetchProgrammeImage(progid));
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
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_EpisodeAdded(epid); });
                return;
            }

            Data.EpisodeData info = progData.FetchEpisodeData(epid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add("");

            EpisodeListItem(epid, info, ref addItem);

            lstEpisodes.ItemCheck -= lstEpisodes_ItemCheck;
            lstEpisodes.Items.Add(addItem);
            lstEpisodes.ItemCheck += lstEpisodes_ItemCheck;
        }

        private void progData_FavouriteAdded(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_FavouriteAdded(progid); });
                return;
            }

            Data.FavouriteData info = progData.FetchFavouriteData(progid);

            lstFavourites.Items.Add(FavouriteListItem(info, null));

            if (view.CurrentView == ViewState.View.Favourites) {
                if (lstFavourites.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_FavouriteUpdated(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_FavouriteUpdated(progid); });
                return;
            }

            Data.FavouriteData info = progData.FetchFavouriteData(progid);
            ListViewItem item = lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)];

            item = FavouriteListItem(info, item);

            if (view.CurrentView == ViewState.View.Favourites) {
                if (lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected) {
                    ShowFavouriteInfo(progid);
                } else if (lstFavourites.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private ListViewItem FavouriteListItem(Data.FavouriteData info, ListViewItem item)
        {
            if (item == null) {
                item = new ListViewItem();
                item.SubItems.Add("");
            }

            item.Name = info.progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.name;

            item.SubItems[1].Text = info.providerName;
            item.ImageKey = "favourite";

            return item;
        }

        private void progData_FavouriteRemoved(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_FavouriteRemoved(progid); });
                return;
            }

            lstFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (view.CurrentView == ViewState.View.Favourites) {
                if (lstFavourites.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void SubscriptionListItem(int progid, Data.SubscriptionData info, ref ListViewItem item)
        {
            item.Name = progid.ToString(CultureInfo.InvariantCulture);
            item.Text = info.name;

            if (info.latestDownload == null) {
                item.SubItems[1].Text = "Never";
            } else {
                item.SubItems[1].Text = info.latestDownload.Value.ToShortDateString();
            }

            item.SubItems[2].Text = info.providerName;
            item.ImageKey = "subscribed";
        }

        private void progData_SubscriptionAdded(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_SubscriptionAdded(progid); });
                return;
            }

            Data.SubscriptionData info = progData.FetchSubscriptionData(progid);

            ListViewItem addItem = new ListViewItem();
            addItem.SubItems.Add("");
            addItem.SubItems.Add("");

            SubscriptionListItem(progid, info, ref addItem);
            lstSubscribed.Items.Add(addItem);

            if (view.CurrentView == ViewState.View.Subscriptions) {
                if (lstSubscribed.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_SubscriptionUpdated(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_SubscriptionUpdated(progid); });
                return;
            }

            Data.SubscriptionData info = progData.FetchSubscriptionData(progid);
            ListViewItem item = lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)];

            SubscriptionListItem(progid, info, ref item);

            if (view.CurrentView == ViewState.View.Subscriptions) {
                if (lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Selected) {
                    ShowSubscriptionInfo(progid);
                } else if (lstSubscribed.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_SubscriptionRemoved(int progid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_SubscriptionRemoved(progid); });
                return;
            }

            lstSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)].Remove();

            if (view.CurrentView == ViewState.View.Subscriptions) {
                if (lstSubscribed.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private ListViewItem DownloadListItem(Data.DownloadData info, ListViewItem item)
        {
            if (item == null) {
                item = new ListViewItem();
            }

            item.Name = info.epid.ToString(CultureInfo.InvariantCulture);

            if (item.SubItems.Count < downloadColOrder.Count) {
                for (int addCols = item.SubItems.Count; addCols <= downloadColOrder.Count - 1; addCols++) {
                    item.SubItems.Add("");
                }
            }

            for (int column = 0; column <= downloadColOrder.Count - 1; column++) {
                switch (downloadColOrder[column]) {
                    case Data.DownloadCols.EpisodeName:
                        item.SubItems[column].Text = info.name;
                        break;
                    case Data.DownloadCols.EpisodeDate:
                        item.SubItems[column].Text = info.episodeDate.ToShortDateString();
                        break;
                    case Data.DownloadCols.Status:
                        switch (info.status) {
                            case Data.DownloadStatus.Waiting:
                                item.SubItems[column].Text = "Waiting";
                                break;
                            case Data.DownloadStatus.Downloaded:
                                if (info.playCount == 0) {
                                    item.SubItems[column].Text = "Newly Downloaded";
                                } else {
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
                        item.SubItems[column].Text = "";
                        break;
                    case Data.DownloadCols.Duration:
                        string durationText = string.Empty;

                        if (info.duration != 0) {
                            int mins = Convert.ToInt32(Math.Round(info.duration / (decimal)60, 0));
                            int hours = mins / 60;
                            mins = mins % 60;

                            durationText = string.Format(CultureInfo.CurrentCulture, "{0}:{1:00}", hours, mins);
                        }

                        item.SubItems[column].Text = durationText;
                        break;
                    default:
                        throw new InvalidDataException("Unknown column type of " + downloadColOrder[column].ToString());
                }
            }

            switch (info.status) {
                case Data.DownloadStatus.Waiting:
                    item.ImageKey = "waiting";
                    break;
                case Data.DownloadStatus.Downloaded:
                    if (info.playCount == 0) {
                        item.ImageKey = "downloaded_new";
                    } else {
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
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_DownloadAdded(epid); });
                return;
            }

            Data.DownloadData info = progData.FetchDownloadData(epid);
            lstDownloads.Items.Add(DownloadListItem(info, null));

            if (view.CurrentView == ViewState.View.Downloads) {
                if (lstDownloads.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_DownloadProgress(int epid, int percent, string statusText, ProgressIcon icon)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_DownloadProgress(epid, percent, statusText, icon); });
                return;
            }

            ListViewItem item = lstDownloads.Items[Convert.ToString(epid, CultureInfo.InvariantCulture)];

            if (item == null) {
                return;
            }

            if (downloadColOrder.Contains(Data.DownloadCols.Status)) {
                item.SubItems[downloadColOrder.IndexOf(Data.DownloadCols.Status)].Text = statusText;
            }

            if (downloadColOrder.Contains(Data.DownloadCols.Progress)) {
                prgDldProg.Value = percent;

                if (lstDownloads.Controls.Count == 0) {
                    lstDownloads.AddProgressBar(ref prgDldProg, item, downloadColOrder.IndexOf(Data.DownloadCols.Progress));
                }
            }

            switch (icon) {
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
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_DownloadRemoved(epid); });
                return;
            }

            ListViewItem item = lstDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];

            if (item != null) {
                if (downloadColOrder.Contains(Data.DownloadCols.Progress)) {
                    if (lstDownloads.GetProgressBar(item, downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null) {
                        lstDownloads.RemoveProgressBar(ref prgDldProg);
                    }
                }

                item.Remove();
            }

            if (view.CurrentView == ViewState.View.Downloads) {
                if (lstDownloads.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_DownloadUpdated(int epid)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_DownloadUpdated(epid); });
                return;
            }

            Data.DownloadData info = progData.FetchDownloadData(epid);

            ListViewItem item = lstDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];
            item = DownloadListItem(info, item);

            if (downloadColOrder.Contains(Data.DownloadCols.Progress)) {
                if (lstDownloads.GetProgressBar(item, downloadColOrder.IndexOf(Data.DownloadCols.Progress)) != null) {
                    lstDownloads.RemoveProgressBar(ref prgDldProg);
                }
            }

            // Update the downloads list sorting, as the order may now have changed
            lstDownloads.Sort();

            if (view.CurrentView == ViewState.View.Downloads) {
                if (item.Selected) {
                    ShowDownloadInfo(epid);
                } else if (lstDownloads.SelectedItems.Count == 0) {
                    // Update the displayed statistics
                    SetViewDefaults();
                }
            }
        }

        private void progData_DownloadProgressTotal(bool downloading, int percent)
        {
            if (this.InvokeRequired) {
                // Events will sometimes be fired on a different thread to the ui
                this.BeginInvoke((MethodInvoker)delegate { progData_DownloadProgressTotal(downloading, percent); });
                return;
            }

            if (this.IsDisposed == false) {
                UpdateTrayStatus(downloading);

                if (OsUtils.WinSevenOrLater()) {
                    if (downloading) {
                        tbarNotif.SetProgressValue(this, percent, 100);
                    } else {
                        tbarNotif.SetProgressNone(this);
                    }
                }
            }
        }

        private void progData_FindNewViewChange(object viewData)
        {
            FindNewViewData findViewData = (FindNewViewData)view.CurrentViewData;
            findViewData.View = viewData;

            view.StoreView(findViewData);
        }

        private void progData_FoundNew(int progid)
        {
            view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void Main_Shown(object sender, System.EventArgs e)
        {
            foreach (string commandLineArg in Environment.GetCommandLineArgs()) {
                if (commandLineArg.ToUpperInvariant() == "/HIDEMAINWINDOW") {
                    if (OsUtils.WinSevenOrLater()) {
                        this.WindowState = FormWindowState.Minimized;
                    } else {
                        OsUtils.TrayAnimate(this, true);
                        this.Visible = false;
                    }
                }
            }
        }

        private void Main_Move_Resize(object sender, System.EventArgs e)
        {
            if (windowPosLoaded) {
                if (this.WindowState == FormWindowState.Normal) {
                    Properties.Settings.Default.MainFormPos = this.DesktopBounds;
                }

                if (this.WindowState != FormWindowState.Minimized) {
                    Properties.Settings.Default.MainFormState = this.WindowState;
                }
            }
        }

        private void tmrCheckForUpdates_Tick(System.Object sender, System.EventArgs e)
        {
            if (checkUpdate.IsUpdateAvailable()) {
                if (Properties.Settings.Default.LastUpdatePrompt.AddDays(7) < DateAndTime.Now) {
                    Properties.Settings.Default.LastUpdatePrompt = DateAndTime.Now;
                    Properties.Settings.Default.Save();
                    // Save the last prompt time in case of unexpected termination

                    UpdateNotify showUpdate = new UpdateNotify();

                    if (this.WindowState == FormWindowState.Minimized | this.Visible == false) {
                        showUpdate.StartPosition = FormStartPosition.CenterScreen;
                    }

                    if (showUpdate.ShowDialog(this) == DialogResult.Yes) {
                        Process.Start("http://www.nerdoftheherd.com/tools/radiodld/update.php?prevver=" + RadioDld.My.MyProject.Application.Info.Version.ToString());
                    }
                }
            }
        }

        private void tbtAddFavourite_Click()
        {
            int progid = 0;

            switch (view.CurrentView) {
                case ViewState.View.ProgEpisodes:
                    progid = (int)view.CurrentViewData;
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (progData.AddFavourite(progid)) {
                view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites, null);
            } else {
                Interaction.MsgBox("You already have this programme in your list of favourites!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtRemFavourite_Click()
        {
            int progid = 0;

            switch (view.CurrentView) {
                case ViewState.View.ProgEpisodes:
                    progid = (int)view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to remove this programme from your list of favourites?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes) {
                progData.RemoveFavourite(progid);
            }
        }

        private void tbtSubscribe_Click()
        {
            int progid = 0;

            switch (view.CurrentView) {
                case ViewState.View.ProgEpisodes:
                    progid = (int)view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (progData.AddSubscription(progid)) {
                view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions, null);
            } else {
                Interaction.MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtUnsubscribe_Click()
        {
            int progid = 0;

            switch (view.CurrentView) {
                case ViewState.View.ProgEpisodes:
                    progid = (int)view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            if (Interaction.MsgBox("Are you sure you would like to stop having new episodes of this programme downloaded automatically?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes) {
                progData.RemoveSubscription(progid);
            }
        }

        private void tbtCancel_Click()
        {
            int epid = Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (Interaction.MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes) {
                progData.DownloadRemove(epid);
            }
        }

        private void tbtPlay_Click()
        {
            int epid = Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Data.DownloadData info = progData.FetchDownloadData(epid);

            if (info.status == Data.DownloadStatus.Downloaded) {
                if (File.Exists(info.downloadPath)) {
                    Process.Start(info.downloadPath);

                    // Bump the play count of this item up by one
                    progData.DownloadBumpPlayCount(epid);
                }
            }
        }

        private void tbtDelete_Click()
        {
            int epid = Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Data.DownloadData info = progData.FetchDownloadData(epid);

            bool fileExists = File.Exists(info.downloadPath);
            string delQuestion = "Are you sure that you would like to delete this episode";

            if (fileExists) {
                delQuestion += " and the associated audio file";
            }

            if (Interaction.MsgBox(delQuestion + "?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes) {
                if (fileExists) {
                    try {
                        File.Delete(info.downloadPath);
                    } catch (IOException) {
                        if (Interaction.MsgBox("There was a problem deleting the audio file for this episode, as the file is in use by another application." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation | MsgBoxStyle.YesNo) == MsgBoxResult.No) {
                            return;
                        }
                    } catch (UnauthorizedAccessException) {
                        if (Interaction.MsgBox("There was a problem deleting the audio file for this episode, as the file is either read-only or you do not have the permissions required." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation | MsgBoxStyle.YesNo) == MsgBoxResult.No) {
                            return;
                        }
                    }
                }

                progData.DownloadRemove(epid);
            }
        }

        private void tbtRetry_Click()
        {
            progData.ResetDownload(Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
        }

        private void tbtDownload_Click()
        {
            int epid = Convert.ToInt32(lstEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (progData.AddDownload(epid)) {
                view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
            } else {
                Interaction.MsgBox("This episode is already in the download list!", MsgBoxStyle.Exclamation);
            }
        }

        private void tbtCurrentEps_Click()
        {
            int progid = 0;

            switch (view.CurrentView) {
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
            }

            view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void tbtReportError_Click()
        {
            int episodeID = Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            progData.DownloadReportError(episodeID);
        }

        private void tbtChooseProgramme_Click()
        {
            FindNewViewData viewData = default(FindNewViewData);
            viewData.ProviderID = new Guid(lstProviders.SelectedItems[0].Name);
            viewData.View = null;

            view.SetView(ViewState.View.FindNewProviderForm, viewData);
        }

        private void mnuOptionsShowOpts_Click(System.Object sender, System.EventArgs e)
        {
            My.MyProject.Forms.Preferences.ShowDialog();
        }

        private void mnuOptionsExit_Click(System.Object sender, System.EventArgs e)
        {
            mnuTrayExit_Click(mnuTrayExit, e);
        }

        private void mnuHelpAbout_Click(System.Object sender, System.EventArgs e)
        {
            My.MyProject.Forms.About.ShowDialog();
        }

        private void mnuHelpShowHelp_Click(System.Object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/help/");
        }

        private void mnuHelpReportBug_Click(System.Object sender, System.EventArgs e)
        {
            Process.Start("http://www.nerdoftheherd.com/tools/radiodld/bug_report.php");
        }

        private void tbtCleanUp_Click()
        {
            My.MyProject.Forms.CleanUp.ShowDialog();
        }

        private void view_UpdateNavBtnState(bool enableBack, bool enableFwd)
        {
            tbtBack.Enabled = enableBack;
            tbtForward.Enabled = enableFwd;
        }

        private void view_ViewChanged(ViewState.View view, ViewState.MainTab tab, object data)
        {
            tbtFindNew.Checked = false;
            tbtFavourites.Checked = false;
            tbtSubscriptions.Checked = false;
            tbtDownloads.Checked = false;

            switch (tab) {
                case ViewState.MainTab.FindProgramme:
                    tbtFindNew.Checked = true;
                    break;
                case ViewState.MainTab.Favourites:
                    tbtFavourites.Checked = true;
                    break;
                case ViewState.MainTab.Subscriptions:
                    tbtSubscriptions.Checked = true;
                    break;
                case ViewState.MainTab.Downloads:
                    tbtDownloads.Checked = true;
                    break;
            }

            SetViewDefaults();

            // Set the focus to a control which does not show it, to prevent the toolbar momentarily showing focus
            lblSideMainTitle.Focus();

            lstProviders.Visible = false;
            pnlPluginSpace.Visible = false;
            lstEpisodes.Visible = false;
            lstFavourites.Visible = false;
            lstSubscribed.Visible = false;
            lstDownloads.Visible = false;
            ttxSearch.Visible = false;

            switch (view) {
                case ViewState.View.FindNewChooseProvider:
                    lstProviders.Visible = true;
                    lstProviders.Focus();

                    if (lstProviders.SelectedItems.Count > 0) {
                        ShowProviderInfo(new Guid(lstProviders.SelectedItems[0].Name));
                    }
                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData FindViewData = (FindNewViewData)data;

                    if (pnlPluginSpace.Controls.Count > 0) {
                        pnlPluginSpace.Controls[0].Dispose();
                        pnlPluginSpace.Controls.Clear();
                    }

                    pnlPluginSpace.Visible = true;
                    pnlPluginSpace.Controls.Add(progData.GetFindNewPanel(FindViewData.ProviderID, FindViewData.View));
                    pnlPluginSpace.Controls[0].Dock = DockStyle.Fill;
                    pnlPluginSpace.Controls[0].Focus();
                    break;
                case ViewState.View.ProgEpisodes:
                    lstEpisodes.Visible = true;
                    progData.CancelEpisodeListing();
                    lstEpisodes.Items.Clear();
                    // Clear before DoEvents so that old items don't flash up on screen
                    Application.DoEvents();
                    // Give any queued BeginInvoke calls a chance to be processed
                    lstEpisodes.Items.Clear();
                    progData.InitEpisodeList((int)data);
                    break;
                case ViewState.View.Favourites:
                    lstFavourites.Visible = true;
                    lstFavourites.Focus();

                    if (lstFavourites.SelectedItems.Count > 0) {
                        ShowFavouriteInfo(Convert.ToInt32(lstFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }
                    break;
                case ViewState.View.Subscriptions:
                    lstSubscribed.Visible = true;
                    lstSubscribed.Focus();

                    if (lstSubscribed.SelectedItems.Count > 0) {
                        ShowSubscriptionInfo(Convert.ToInt32(lstSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }
                    break;
                case ViewState.View.Downloads:
                    if (data == null) {
                        ttxSearch.Text = string.Empty;
                    } else {
                        ttxSearch.Text = (string)data;
                        PerformSearch(view, ttxSearch.Text);
                    }

                    ttxSearch.Visible = true;
                    lstDownloads.Visible = true;
                    lstDownloads.Focus();

                    if (lstDownloads.SelectedItems.Count > 0) {
                        ShowDownloadInfo(Convert.ToInt32(lstDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
                    }
                    break;
            }
        }

        private void SetViewDefaults()
        {
            switch (view.CurrentView) {
                case ViewState.View.FindNewChooseProvider:
                    SetToolbarButtons(new ToolBarButton[] {});
                    SetSideBar(Convert.ToString(lstProviders.Items.Count, CultureInfo.CurrentCulture) + " provider" + (lstProviders.Items.Count == 1 ? "" : "s"), "", null);
                    break;
                case ViewState.View.FindNewProviderForm:
                    FindNewViewData FindViewData = (FindNewViewData)view.CurrentViewData;
                    SetToolbarButtons(new ToolBarButton[] {});
                    ShowProviderInfo(FindViewData.ProviderID);
                    break;
                case ViewState.View.ProgEpisodes:
                    ShowProgrammeInfo((int)view.CurrentViewData);
                    break;
                case ViewState.View.Favourites:
                    SetToolbarButtons(new ToolBarButton[] {});
                    SetSideBar(Convert.ToString(lstFavourites.Items.Count, CultureInfo.CurrentCulture) + " favourite" + (lstFavourites.Items.Count == 1 ? "" : "s"), "", null);
                    break;
                case ViewState.View.Subscriptions:
                    SetToolbarButtons(new ToolBarButton[] {});
                    SetSideBar(Convert.ToString(lstSubscribed.Items.Count, CultureInfo.CurrentCulture) + " subscription" + (lstSubscribed.Items.Count == 1 ? "" : "s"), "", null);
                    break;
                case ViewState.View.Downloads:
                    SetToolbarButtons(new ToolBarButton[] {tbtCleanUp});

                    if (!string.IsNullOrEmpty(progData.DownloadQuery)) {
                        SetSideBar(Convert.ToString(lstDownloads.Items.Count, CultureInfo.CurrentCulture) + " result" + (lstDownloads.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    } else {
                        string description = string.Empty;
                        int newCount = progData.CountDownloadsNew();
                        int errorCount = progData.CountDownloadsErrored();

                        if (newCount > 0) {
                            description += "Newly downloaded: " + Convert.ToString(newCount, CultureInfo.CurrentCulture) + Environment.NewLine;
                        }

                        if (errorCount > 0) {
                            description += "Errored: " + Convert.ToString(errorCount, CultureInfo.CurrentCulture);
                        }

                        SetSideBar(Convert.ToString(lstDownloads.Items.Count, CultureInfo.CurrentCulture) + " download" + (lstDownloads.Items.Count == 1 ? string.Empty : "s"), description, null);
                    }
                    break;
            }
        }

        private void tbtBack_Click(System.Object sender, System.EventArgs e)
        {
            view.NavBack();
        }

        private void tbtForward_Click(System.Object sender, System.EventArgs e)
        {
            view.NavFwd();
        }

        private void tbrToolbar_ButtonClick(System.Object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.Name) {
                case "tbtChooseProgramme":
                    tbtChooseProgramme_Click();
                    break;
                case "tbtDownload":
                    tbtDownload_Click();
                    break;
                case "tbtAddFavourite":
                    tbtAddFavourite_Click();
                    break;
                case "tbtRemFavourite":
                    tbtRemFavourite_Click();
                    break;
                case "tbtSubscribe":
                    tbtSubscribe_Click();
                    break;
                case "tbtUnsubscribe":
                    tbtUnsubscribe_Click();
                    break;
                case "tbtCurrentEps":
                    tbtCurrentEps_Click();
                    break;
                case "tbtCancel":
                    tbtCancel_Click();
                    break;
                case "tbtPlay":
                    tbtPlay_Click();
                    break;
                case "tbtDelete":
                    tbtDelete_Click();
                    break;
                case "tbtRetry":
                    tbtRetry_Click();
                    break;
                case "tbtReportError":
                    tbtReportError_Click();
                    break;
                case "tbtCleanUp":
                    tbtCleanUp_Click();
                    break;
            }
        }

        private void tblToolbars_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized) {
                tblToolbars.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, tblToolbars.Width - (tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right));
                tblToolbars.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right);

                if (VisualStyleRenderer.IsSupported) {
                    // Visual styles are enabled, so draw the correct background behind the toolbars

                    Bitmap bmpBackground = new Bitmap(tblToolbars.Width, tblToolbars.Height);
                    Graphics graGraphics = Graphics.FromImage(bmpBackground);

                    try {
                        VisualStyleRenderer vsrRebar = new VisualStyleRenderer("Rebar", 0, 0);
                        vsrRebar.DrawBackground(graGraphics, new Rectangle(0, 0, tblToolbars.Width, tblToolbars.Height));
                        tblToolbars.BackgroundImage = bmpBackground;
                    } catch (ArgumentException) {
                        // The 'Rebar' background image style did not exist, so don't try to draw it.
                    }
                }
            }
        }

        private void txtSideDescript_GotFocus(object sender, System.EventArgs e)
        {
            lblSideMainTitle.Focus();
        }

        private void txtSideDescript_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }

        private void txtSideDescript_Resize(object sender, System.EventArgs e)
        {
            txtSideDescript.Refresh();
            // Make sure the scrollbars update correctly
        }

        private void picSideBarBorder_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(255, 167, 186, 197)), 0, 0, 0, picSideBarBorder.Height);
        }

        private void mnuListHdrsColumns_Click(System.Object sender, System.EventArgs e)
        {
            ChooseCols chooser = new ChooseCols();
            chooser.Columns = Properties.Settings.Default.DownloadCols;
            chooser.StoreNameList(downloadColNames);

            if (chooser.ShowDialog(this) == DialogResult.OK) {
                Properties.Settings.Default.DownloadCols = chooser.Columns;
                InitDownloadList();
            }
        }

        private void mnuListHdrsReset_Click(System.Object sender, System.EventArgs e)
        {
            Properties.Settings.Default.DownloadCols = (string)Properties.Settings.Default.Properties["DownloadCols"].DefaultValue;
            Properties.Settings.Default.DownloadColSizes = (string)Properties.Settings.Default.Properties["DownloadColSizes"].DefaultValue;
            Properties.Settings.Default.DownloadColSortBy = Convert.ToInt32(Properties.Settings.Default.Properties["DownloadColSortBy"].DefaultValue, CultureInfo.InvariantCulture);
            Properties.Settings.Default.DownloadColSortAsc = Convert.ToBoolean(Properties.Settings.Default.Properties["DownloadColSortAsc"].DefaultValue, CultureInfo.InvariantCulture);

            InitDownloadList();
        }

        private void InitFavouriteList()
        {
            if (lstFavourites.SelectedItems.Count > 0) {
                SetViewDefaults();
                // Revert back to default sidebar and toolbar
            }

            // Convert the list of FavouriteData items to an array of ListItems
            List<Data.FavouriteData> initData = progData.FetchFavouriteList();
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++) {
                initItems[convItems] = FavouriteListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            lstFavourites.Items.AddRange(initItems);
        }

        private void InitDownloadList()
        {
            if (lstDownloads.SelectedItems.Count > 0) {
                SetViewDefaults();
                // Revert back to default sidebar and toolbar
            }

            downloadColSizes.Clear();
            downloadColOrder.Clear();
            lstDownloads.Clear();
            lstDownloads.RemoveAllControls();

            string newItems = string.Empty;

            // Find any columns without widths defined in the current setting
            foreach (string sizePair in Strings.Split(Properties.Settings.Default.Properties["DownloadColSizes"].DefaultValue.ToString(), "|")) {
                if (!("|" + Properties.Settings.Default.DownloadColSizes).Contains("|" + Strings.Split(sizePair, ",")[0] + ",")) {
                    newItems += "|" + sizePair;
                }
            }

            // Append the new column sizes to the end of the setting
            if (!string.IsNullOrEmpty(newItems)) {
                Properties.Settings.Default.DownloadColSizes += newItems;
            }

            // Fetch the column sizes into downloadColSizes for ease of access
            foreach (string sizePair in Strings.Split(Properties.Settings.Default.DownloadColSizes, "|")) {
                string[] splitPair = Strings.Split(sizePair, ",");
                int pixelSize = Convert.ToInt32(float.Parse(splitPair[1], CultureInfo.InvariantCulture) * this.CurrentAutoScaleDimensions.Width);

                downloadColSizes.Add(int.Parse(splitPair[0], CultureInfo.InvariantCulture), pixelSize);
            }

            // Set up the columns specified in the DownloadCols setting
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DownloadCols)) {
                string[] columns = Strings.Split(Properties.Settings.Default.DownloadCols, ",");

                foreach (string column in columns) {
                    int colVal = int.Parse(column, CultureInfo.InvariantCulture);
                    downloadColOrder.Add((Data.DownloadCols)colVal);
                    lstDownloads.Columns.Add(downloadColNames[colVal], downloadColSizes[colVal]);
                }
            }

            // Apply the sort from the current settings
            progData.DownloadSortByCol = (Data.DownloadCols)Properties.Settings.Default.DownloadColSortBy;
            progData.DownloadSortAscending = Properties.Settings.Default.DownloadColSortAsc;
            lstDownloads.ShowSortOnHeader(downloadColOrder.IndexOf(progData.DownloadSortByCol), progData.DownloadSortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of DownloadData items to an array of ListItems
            List<Data.DownloadData> initData = progData.FetchDownloadList(true);
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems <= initData.Count - 1; convItems++) {
                initItems[convItems] = DownloadListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            lstDownloads.Items.AddRange(initItems);
        }

        private void ttxSearch_TextChanged(object sender, System.EventArgs e)
        {
            lock (searchThreadLock) {
                if (string.IsNullOrEmpty(ttxSearch.Text)) {
                    searchThread = null;
                    PerformSearch(view.CurrentView, ttxSearch.Text);
                } else {
                    searchThread = new Thread(() => SearchWait(view.CurrentView, ttxSearch.Text));
                    searchThread.IsBackground = true;
                    searchThread.Start();
                }
            }
        }

        private void SearchWait(ViewState.View origView, string search)
        {
            Thread.Sleep(500);

            lock (searchThreadLock) {
                if (!object.ReferenceEquals(Thread.CurrentThread, searchThread)) {
                    // A search thread was created more recently, stop this thread
                    return;
                }

                this.Invoke((MethodInvoker)delegate { PerformSearch(origView, search); });
            }
        }

        private void PerformSearch(ViewState.View origView, string search)
        {
            if (view.CurrentView == origView) {
                if (string.IsNullOrEmpty(search)) {
                    if (view.CurrentViewData != null) {
                        view.StoreView(null);
                    }
                } else {
                    if (view.CurrentViewData == null) {
                        view.StoreView(search);
                    } else {
                        view.CurrentViewData = search;
                    }
                }

                progData.DownloadQuery = search;
                InitDownloadList();
                SetViewDefaults();
            }
        }
    }
}

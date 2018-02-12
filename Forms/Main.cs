/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2018 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        private DataSearch dataSearch;
        private ViewState view = new ViewState();
        private TaskbarNotify tbarNotif;

        private Thread searchThread;
        private object searchThreadLock = new object();

        private Dictionary<int, string> downloadColNames = new Dictionary<int, string>();
        private Dictionary<int, int> downloadColSizes = new Dictionary<int, int>();
        private List<Model.Download.DownloadCols> downloadColOrder = new List<Model.Download.DownloadCols>();

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

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    e.Handled = true;
                    this.MenuHelpContext_Click(sender, e);
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
                        this.ButtonCancel_Click();
                    }

                    break;
                case Keys.Back:
                    if (!ReferenceEquals(this.ActiveControl.GetType(), typeof(TextBox)) && !ReferenceEquals(this.ActiveControl.Parent.GetType(), typeof(ExtToolStrip)))
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

        private void Main_Load(object eventSender, EventArgs eventArgs)
        {
            this.ImagesListIcons.Images.Add("downloading", Properties.Resources.list_downloading);
            this.ImagesListIcons.Images.Add("waiting", Properties.Resources.list_waiting);
            this.ImagesListIcons.Images.Add("processing", Properties.Resources.list_processing);
            this.ImagesListIcons.Images.Add("downloaded_new", Properties.Resources.list_downloaded_new);
            this.ImagesListIcons.Images.Add("downloaded", Properties.Resources.list_downloaded);
            this.ImagesListIcons.Images.Add("subscribed", Properties.Resources.list_subscribed);
            this.ImagesListIcons.Images.Add("subscribed_multi", Properties.Resources.list_subscribed_multi);
            this.ImagesListIcons.Images.Add("error", Properties.Resources.list_error);
            this.ImagesListIcons.Images.Add("favourite", Properties.Resources.list_favourite);
            this.ImagesListIcons.Images.Add("episode_auto", Properties.Resources.list_episode_auto);
            this.ImagesListIcons.Images.Add("episode_noauto", Properties.Resources.list_episode_noauto);

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
            this.ImagesToolbar.Images.Add("set_auto", Properties.Resources.toolbar_set_auto);
            this.ImagesToolbar.Images.Add("more_info", Properties.Resources.toolbar_more_info);

            this.ToolbarMain.ImageList = this.ImagesToolbar;
            this.ToolbarHelp.ImageList = this.ImagesToolbar;
            this.ListProviders.LargeImageList = this.ImagesProviders;
            this.ListEpisodes.SmallImageList = this.ImagesListIcons;
            this.ListFavourites.SmallImageList = this.ImagesListIcons;
            this.ListSubscribed.SmallImageList = this.ImagesListIcons;
            this.ListDownloads.SmallImageList = this.ImagesListIcons;

            this.ListEpisodes.Columns.Add("Date", (int)(0.179 * this.ListEpisodes.Width));
            this.ListEpisodes.Columns.Add("Episode Name", (int)(0.786 * this.ListEpisodes.Width));
            this.ListFavourites.Columns.Add("Programme Name", (int)(0.661 * this.ListFavourites.Width));
            this.ListFavourites.Columns.Add("Provider", (int)(0.304 * this.ListFavourites.Width));
            this.ListSubscribed.Columns.Add("Programme Name", (int)(0.482 * this.ListSubscribed.Width));
            this.ListSubscribed.Columns.Add("Last Download", (int)(0.179 * this.ListSubscribed.Width));
            this.ListSubscribed.Columns.Add("Provider", (int)(0.304 * this.ListSubscribed.Width));

            // NB - these are defined in alphabetical order to save sorting later
            this.downloadColNames.Add((int)Model.Download.DownloadCols.EpisodeDate, "Date");
            this.downloadColNames.Add((int)Model.Download.DownloadCols.Duration, "Duration");
            this.downloadColNames.Add((int)Model.Download.DownloadCols.EpisodeName, "Episode Name");
            this.downloadColNames.Add((int)Model.Download.DownloadCols.Progress, "Progress");
            this.downloadColNames.Add((int)Model.Download.DownloadCols.Status, "Status");
            this.downloadColNames.Add((int)Model.Download.DownloadCols.ProgrammeName, "Programme Name");

            FindNew.EpisodeAdded += this.ProgData_EpisodeAdded;
            FindNew.FindNewViewChange += this.ProgData_FindNewViewChange;
            FindNew.FindNewFailed += this.ProgData_FindNewFailed;
            FindNew.FoundNew += this.ProgData_FoundNew;

            Model.Programme.Updated += this.Programme_Updated;
            Model.Favourite.Added += this.Favourite_Added;
            Model.Favourite.Updated += this.Favourite_Updated;
            Model.Favourite.Removed += this.Favourite_Removed;
            Model.Subscription.Added += this.Subscription_Added;
            Model.Subscription.Updated += this.Subscription_Updated;
            Model.Subscription.Removed += this.Subscription_Removed;
            Model.Episode.Updated += this.Episode_Updated;
            DownloadManager.ProgressTotal += this.DownloadManager_ProgressTotal;

            this.dataSearch = DataSearch.GetInstance();
            this.dataSearch.DownloadAdded += this.DataSearch_DownloadAdded;
            this.dataSearch.DownloadUpdated += this.DataSearch_DownloadUpdated;
            this.dataSearch.DownloadRemoved += this.DataSearch_DownloadRemoved;
            this.dataSearch.DownloadProgress += this.DataSearch_DownloadProgress;

            this.InitProviderList();
            this.InitFavouriteList();
            this.InitSubscriptionList();
            this.InitDownloadList();

            this.view.UpdateNavBtnState += this.View_UpdateNavBtnState;
            this.view.ViewChanged += this.View_ViewChanged;
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);

            this.ListFavourites.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Favourite);
            this.ListSubscribed.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Subscription);
            this.ListDownloads.ListViewItemSorter = new ListItemComparer(ListItemComparer.ListType.Download);

            if (OsUtils.WinSevenOrLater())
            {
                // New style taskbar - initialise the taskbar notification class
                this.tbarNotif = new TaskbarNotify();
            }

            if (!OsUtils.WinSevenOrLater() || Settings.CloseToSystray)
            {
                // Show a system tray icon
                this.NotifyIcon.Visible = true;
            }

            // Set up the initial notification status
            this.UpdateTrayStatus(false);

            this.ImageSidebarBorder.Width = 2;

            this.ListProviders.Dock = DockStyle.Fill;
            this.PanelPluginSpace.Dock = DockStyle.Fill;
            this.ListEpisodes.Dock = DockStyle.Fill;
            this.ListFavourites.Dock = DockStyle.Fill;
            this.ListSubscribed.Dock = DockStyle.Fill;
            this.ListDownloads.Dock = DockStyle.Fill;

            this.Font = SystemFonts.MessageBoxFont;
            this.LabelSidebarTitle.Font = new Font(this.Font.FontFamily, (int)(this.Font.SizeInPoints * 1.2), this.Font.Style, GraphicsUnit.Point);

            // Scale the max size of the sidebar image for values other than 96 dpi, as it is specified in pixels
            using (Graphics graphicsForDpi = this.CreateGraphics())
            {
                this.ImageSidebar.MaximumSize = new Size((int)(this.ImageSidebar.MaximumSize.Width * (graphicsForDpi.DpiX / 96)), (int)(this.ImageSidebar.MaximumSize.Height * (graphicsForDpi.DpiY / 96)));
            }

            if (Settings.MainFormPos != Rectangle.Empty)
            {
                if (OsUtils.VisibleOnScreen(Settings.MainFormPos))
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.DesktopBounds = Settings.MainFormPos;
                }
                else
                {
                    this.Size = Settings.MainFormPos.Size;
                }

                this.WindowState = Settings.MainFormState;
            }

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

            if (Settings.RssServer)
            {
                RssServer listener = new RssServer(Settings.RssServerPort);
                listener.Start();
            }

            DownloadManager.ResumeDownloads();
            this.TimerCheckForUpdates.Enabled = true;
        }

        private void Main_FormClosing(object eventSender, FormClosingEventArgs eventArgs)
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

                    if (!Settings.ShownTrayBalloon)
                    {
                        this.NotifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                        this.NotifyIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + Environment.NewLine + "Click here to hide this message in future.";
                        this.NotifyIcon.BalloonTipTitle = "Radio Downloader is still running";
                        this.NotifyIcon.ShowBalloonTip(30000);
                    }
                }
            }
        }

        private void ListProviders_SelectedIndexChanged(object sender, EventArgs e)
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
            Provider.Handler provider = Provider.Handler.GetFromId(providerId);
            this.SetSideBar(provider.Name, provider.Description, null);

            if (this.view.CurrentView == ViewState.View.FindNewChooseProvider)
            {
                this.SetToolbarButtons(new ToolBarButton[] { this.ButtonChooseProgramme });
            }
        }

        private void ListProviders_ItemActivate(object sender, EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListProviders.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonChooseProgramme_Click();
        }

        private void ShowEpisodeInfo()
        {
            int progid = (int)this.view.CurrentViewData;

            if (this.ListEpisodes.SelectedItems.Count == 1)
            {
                int epid = Convert.ToInt32(this.ListEpisodes.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                Model.Episode epInfo = new Model.Episode(epid);
                string infoText = string.Empty;

                if (epInfo.Description != null)
                {
                    infoText += epInfo.Description + Environment.NewLine + Environment.NewLine;
                }

                infoText += "Date: " + epInfo.Date.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
                infoText += TextUtils.DescDuration(epInfo.Duration) + Environment.NewLine;
                infoText += "Auto download: " + (epInfo.AutoDownload ? "Yes" : "No");

                this.SetSideBar(TextUtils.StripDateFromName(epInfo.Name, epInfo.Date), infoText, Model.Episode.GetImage(epid));
            }
            else
            {
                this.SetSideBar(Convert.ToString(this.ListEpisodes.SelectedItems.Count, CultureInfo.CurrentCulture) + " episodes selected", string.Empty, null);
            }

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.ButtonDownload);
            buttons.Add(this.ButtonSetAuto);

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
        }

        private void ListEpisodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.view.CurrentView == ViewState.View.ProgEpisodes)
            {
                if (this.ListEpisodes.SelectedItems.Count > 0)
                {
                    this.ShowEpisodeInfo();
                }
                else
                {
                    this.SetViewDefaults(); // Revert back to programme info in sidebar
                }
            }
        }

        private void ListEpisodes_ItemActivate(object sender, EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListEpisodes.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonDownload_Click();
        }

        private void ListFavourites_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Event is fired when un-favouriting hidden selected item
            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (this.ListFavourites.SelectedItems.Count > 0)
                {
                    int progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                    Model.Programme.UpdateInfoIfRequired(progid);
                    this.ShowFavouriteInfo(progid);
                }
                else
                {
                    this.SetViewDefaults(); // Revert back to subscribed items view default sidebar and toolbar
                }
            }
        }

        private void ShowFavouriteInfo(int progid)
        {
            Model.Favourite info = new Model.Favourite(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.AddRange(new ToolBarButton[] { this.ButtonRemFavourite, this.ButtonCurrentEps });

            if (info.HasMoreInfo)
            {
                buttons.Add(this.ButtonMoreInfo);
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
            this.SetSideBar(info.Name, info.Description, Model.Programme.GetImage(progid));
        }

        private void ListFavourites_ItemActivate(object sender, EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListFavourites.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonCurrentEps_Click();
        }

        private void ListSubscribed_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Event is fired when unsubscribing hidden selected item
            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.ListSubscribed.SelectedItems.Count > 0)
                {
                    int progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);

                    Model.Programme.UpdateInfoIfRequired(progid);
                    this.ShowSubscriptionInfo(progid);
                }
                else
                {
                    this.SetViewDefaults(); // Revert back to subscribed items view default sidebar and toolbar
                }
            }
        }

        private void ListSubscribed_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Model.Subscription.SubscriptionCols clickedCol = (Model.Subscription.SubscriptionCols)e.Column;

            if (Model.Subscription.SortByColumn != clickedCol)
            {
                Model.Subscription.SortByColumn = clickedCol;
                Model.Subscription.SortAscending = true;
            }
            else
            {
                Model.Subscription.SortAscending = !Model.Subscription.SortAscending;
            }

            // Set the column header to display the new sort order
            this.ListSubscribed.ShowSortOnHeader((int)Model.Subscription.SortByColumn, Model.Subscription.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Settings.SubscriptionColSortBy = Model.Subscription.SortByColumn;
            Settings.SubscriptionColSortAsc = Model.Subscription.SortAscending;

            this.ListSubscribed.Sort();
        }

        private void ShowSubscriptionInfo(int progid)
        {
            Model.Subscription info = new Model.Subscription(progid);

            List<ToolBarButton> buttons = new List<ToolBarButton>();
            buttons.Add(this.ButtonUnsubscribe);
            buttons.Add(this.ButtonCurrentEps);

            if (info.HasMoreInfo)
            {
                buttons.Add(this.ButtonMoreInfo);
            }

            if (Model.Favourite.IsFavourite(progid))
            {
                buttons.Add(this.ButtonRemFavourite);
            }
            else
            {
                buttons.Add(this.ButtonAddFavourite);
            }

            this.SetToolbarButtons(buttons.ToArray());
            this.SetSideBar(info.Name, info.Description, Model.Programme.GetImage(progid));
        }

        private void ListSubscribed_ItemActivate(object sender, EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListSubscribed.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonCurrentEps_Click();
        }

        private void ListDownloads_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Model.Download.DownloadCols clickedCol = this.downloadColOrder[e.Column];

            if (clickedCol == Model.Download.DownloadCols.Progress)
            {
                return;
            }

            if (Model.Download.SortByColumn != clickedCol)
            {
                Model.Download.SortByColumn = clickedCol;
                Model.Download.SortAscending = true;
            }
            else
            {
                Model.Download.SortAscending = !Model.Download.SortAscending;
            }

            // Set the column header to display the new sort order
            this.ListDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(Model.Download.SortByColumn), Model.Download.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Settings.DownloadColSortBy = Model.Download.SortByColumn;
            Settings.DownloadColSortAsc = Model.Download.SortAscending;

            this.ListDownloads.Sort();
        }

        private void ListFavourites_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Model.Favourite.FavouriteCols clickedCol = (Model.Favourite.FavouriteCols)e.Column;

            if (Model.Favourite.SortByColumn != clickedCol)
            {
                Model.Favourite.SortByColumn = clickedCol;
                Model.Favourite.SortAscending = true;
            }
            else
            {
                Model.Favourite.SortAscending = !Model.Favourite.SortAscending;
            }

            // Set the column header to display the new sort order
            this.ListFavourites.ShowSortOnHeader((int)Model.Favourite.SortByColumn, Model.Favourite.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Save the current sort
            Settings.FavouriteColSortBy = Model.Favourite.SortByColumn;
            Settings.FavouriteColSortAsc = Model.Favourite.SortAscending;

            this.ListFavourites.Sort();
        }

        private void ListDownloads_ColumnReordered(object sender, ColumnReorderedEventArgs e)
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
            Settings.DownloadCols = string.Join(",", newOrder.ToArray());

            if (e.OldDisplayIndex == 0 || e.NewDisplayIndex == 0)
            {
                // The reorder involves column 0 which contains the icons, so re-initialise the list
                e.Cancel = true;
                this.InitDownloadList();
            }
        }

        private void ListDownloads_ColumnRightClick(object sender, ColumnClickEventArgs e)
        {
            this.MenuListHdrs.Show(this.ListDownloads, this.ListDownloads.PointToClient(Cursor.Position));
        }

        private void ListDownloads_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
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

            Settings.DownloadColSizes = saveColSizes;
        }

        private void ListDownloads_ItemActivate(object sender, EventArgs e)
        {
            // Occasionally the event gets fired when there isn't an item selected
            if (this.ListDownloads.SelectedItems.Count == 0)
            {
                return;
            }

            this.ButtonPlay_Click();
        }

        private void ListDownloads_SelectedIndexChanged(object sender, EventArgs e)
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

            infoText += "Date: " + info.Date.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture);
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
                        case Provider.ErrorType.LocalProblem:
                            errorName = "Local problem";
                            break;
                        case Provider.ErrorType.ShorterThanExpected:
                            errorName = "Shorter than expected";
                            break;
                        case Provider.ErrorType.NotAvailable:
                            errorName = "Not available";
                            break;
                        case Provider.ErrorType.NotAvailableInLocation:
                            errorName = "Not available in your location";
                            break;
                        case Provider.ErrorType.NetworkProblem:
                            errorName = "Network problem";
                            break;
                        case Provider.ErrorType.RemoteProblem:
                            errorName = "Remote problem";
                            break;
                        case Provider.ErrorType.UnknownError:
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
            this.SetSideBar(TextUtils.StripDateFromName(info.Name, info.Date), infoText, Model.Episode.GetImage(epid));
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
            this.ButtonSetAuto.Visible = false;
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
            this.ButtonMoreInfo.Visible = false;

            foreach (ToolBarButton button in buttons)
            {
                button.Visible = true;
            }
        }

        private void MenuTrayShow_Click(object sender, EventArgs e)
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

        private void MenuTrayExit_Click(object eventSender, EventArgs eventArgs)
        {
            this.Close();
            this.Dispose();
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            Settings.ShownTrayBalloon = true;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.MenuTrayShow_Click(sender, e);
        }

        private void ButtonFindNew_Click(object sender, EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider);
        }

        private void ButtonFavourites_Click(object sender, EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites);
        }

        private void ButtonSubscriptions_Click(object sender, EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions);
        }

        private void ButtonDownloads_Click(object sender, EventArgs e)
        {
            this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
        }

        private ListViewItem ProviderListItem(Provider.Handler provider)
        {
            ListViewItem addItem = new ListViewItem();
            addItem.Name = provider.Id.ToString();
            addItem.Text = provider.Name;

            if (provider.Icon != null)
            {
                this.ImagesProviders.Images.Add(provider.Id.ToString(), provider.Icon);
                addItem.ImageKey = provider.Id.ToString();
            }
            else
            {
                addItem.ImageKey = "default";
            }

            // Hide the 'No providers' provider options menu item
            if (this.MenuOptionsProviderOptsNoProvs.Visible)
            {
                this.MenuOptionsProviderOptsNoProvs.Visible = false;
            }

            MenuItem addMenuItem = new MenuItem(provider.Name + " Provider");

            if (provider.ShowOptionsHandler != null)
            {
                addMenuItem.Click += provider.ShowOptionsHandler;
            }
            else
            {
                addMenuItem.Enabled = false;
            }

            this.MenuOptionsProviderOpts.MenuItems.Add(addMenuItem);

            return addItem;
        }

        private void Programme_Updated(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Programme_Updated(progid); }));
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
                        // Update the episode information (for subscription / favourite toolbar buttons etc)
                        this.ShowEpisodeInfo();
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
            this.SetSideBar(progInfo.Name, progInfo.Description, Model.Programme.GetImage(progid));
        }

        private ListViewItem EpisodeListItem(Model.Episode info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
                item.Name = info.Epid.ToString(CultureInfo.InvariantCulture);
                item.SubItems.Add(string.Empty);
            }

            item.Text = info.Date.ToShortDateString();
            item.SubItems[1].Text = TextUtils.StripDateFromName(info.Name, info.Date);
            item.ImageKey = info.AutoDownload ? "episode_auto" : "episode_noauto";

            return item;
        }

        private void ProgData_EpisodeAdded(int epid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.ProgData_EpisodeAdded(epid); }));
                return;
            }

            Model.Episode info = new Model.Episode(epid);

            ListViewItem addItem = this.EpisodeListItem(info, null);
            this.ListEpisodes.Items.Add(addItem);
        }

        private void Episode_Updated(int epid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Episode_Updated(epid); }));
                return;
            }

            if (this.view.CurrentView == ViewState.View.ProgEpisodes)
            {
                Model.Episode info = new Model.Episode(epid);

                if ((int)this.view.CurrentViewData == info.Progid)
                {
                    ListViewItem item = this.ListEpisodes.Items[epid.ToString(CultureInfo.InvariantCulture)];
                    item = this.EpisodeListItem(info, item);

                    if (item.Selected)
                    {
                        this.ShowEpisodeInfo();
                    }
                }
            }
        }

        private void Favourite_Added(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Favourite_Added(progid); }));
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

        private void Favourite_Updated(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Favourite_Updated(progid); }));
                return;
            }

            Model.Favourite info = new Model.Favourite(progid);
            ListViewItem item = this.ListFavourites.Items[progid.ToString(CultureInfo.InvariantCulture)];

            item = this.FavouriteListItem(info, item);

            if (this.view.CurrentView == ViewState.View.Favourites)
            {
                if (item.Selected)
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

        private void Favourite_Removed(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Favourite_Removed(progid); }));
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

        private ListViewItem SubscriptionListItem(Model.Subscription info, ListViewItem item)
        {
            if (item == null)
            {
                item = new ListViewItem();
                item.SubItems.Add(string.Empty);
                item.SubItems.Add(string.Empty);
            }

            item.Name = info.Progid.ToString(CultureInfo.InvariantCulture);
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

            return item;
        }

        private void Subscription_Added(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Subscription_Added(progid); }));
                return;
            }

            Model.Subscription info = new Model.Subscription(progid);
            this.ListSubscribed.Items.Add(this.SubscriptionListItem(info, null));

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (this.ListSubscribed.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void Subscription_Updated(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Subscription_Updated(progid); }));
                return;
            }

            Model.Subscription info = new Model.Subscription(progid);
            ListViewItem item = this.ListSubscribed.Items[progid.ToString(CultureInfo.InvariantCulture)];

            item = this.SubscriptionListItem(info, item);

            if (this.view.CurrentView == ViewState.View.Subscriptions)
            {
                if (item.Selected)
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

        private void Subscription_Removed(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.Subscription_Removed(progid); }));
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
                item.Name = info.Epid.ToString(CultureInfo.InvariantCulture);

                if (item.SubItems.Count < this.downloadColOrder.Count)
                {
                    for (int addCols = item.SubItems.Count; addCols <= this.downloadColOrder.Count - 1; addCols++)
                    {
                        item.SubItems.Add(string.Empty);
                    }
                }
            }

            for (int column = 0; column <= this.downloadColOrder.Count - 1; column++)
            {
                switch (this.downloadColOrder[column])
                {
                    case Model.Download.DownloadCols.EpisodeName:
                        item.SubItems[column].Text = TextUtils.StripDateFromName(info.Name, info.Date);
                        break;
                    case Model.Download.DownloadCols.EpisodeDate:
                        item.SubItems[column].Text = info.Date.ToShortDateString();
                        break;
                    case Model.Download.DownloadCols.Status:
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
                    case Model.Download.DownloadCols.Progress:
                        item.SubItems[column].Text = string.Empty;
                        break;
                    case Model.Download.DownloadCols.Duration:
                        string durationText = string.Empty;

                        if (info.Duration != 0)
                        {
                            int mins = (int)Math.Round(info.Duration / (decimal)60, 0);
                            int hours = mins / 60;
                            mins = mins % 60;

                            durationText = string.Format(CultureInfo.CurrentCulture, "{0}:{1:00}", hours, mins);
                        }

                        item.SubItems[column].Text = durationText;
                        break;
                    case Model.Download.DownloadCols.ProgrammeName:
                        Model.Programme progInfo = new Model.Programme(info.Progid);
                        item.SubItems[column].Text = progInfo.Name;
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

        private void DataSearch_DownloadAdded(int epid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.DataSearch_DownloadAdded(epid); }));
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

        private void DataSearch_DownloadProgress(int epid, int percent, Provider.ProgressType type)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.DataSearch_DownloadProgress(epid, percent, type); }));
                return;
            }

            ListViewItem item = this.ListDownloads.Items[Convert.ToString(epid, CultureInfo.InvariantCulture)];

            if (item == null)
            {
                return;
            }

            if (this.downloadColOrder.Contains(Model.Download.DownloadCols.Status))
            {
                string statusText = "Downloading...";

                if (type == Provider.ProgressType.Processing)
                {
                    statusText = "Processing...";
                }

                item.SubItems[this.downloadColOrder.IndexOf(Model.Download.DownloadCols.Status)].Text = statusText;
            }

            if (this.downloadColOrder.Contains(Model.Download.DownloadCols.Progress))
            {
                this.ListDownloads.ShowProgress(item, this.downloadColOrder.IndexOf(Model.Download.DownloadCols.Progress), percent);
            }

            switch (type)
            {
                case Provider.ProgressType.Downloading:
                    item.ImageKey = "downloading";
                    break;
                case Provider.ProgressType.Processing:
                    item.ImageKey = "processing";
                    break;
            }
        }

        private void DataSearch_DownloadRemoved(int epid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.DataSearch_DownloadRemoved(epid); }));
                return;
            }

            ListViewItem item = this.ListDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];

            if (this.downloadColOrder.Contains(Model.Download.DownloadCols.Progress))
            {
                this.ListDownloads.HideProgress(item);
            }

            item.Remove();

            if (this.view.CurrentView == ViewState.View.Downloads)
            {
                if (this.ListDownloads.SelectedItems.Count == 0)
                {
                    // Update the displayed statistics
                    this.SetViewDefaults();
                }
            }
        }

        private void DataSearch_DownloadUpdated(int epid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.DataSearch_DownloadUpdated(epid); }));
                return;
            }

            Model.Download info = new Model.Download(epid);

            ListViewItem item = this.ListDownloads.Items[epid.ToString(CultureInfo.InvariantCulture)];
            item = this.DownloadListItem(info, item);

            if (this.downloadColOrder.Contains(Model.Download.DownloadCols.Progress))
            {
                this.ListDownloads.HideProgress(item);
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

        private void DownloadManager_ProgressTotal(bool downloading, int percent)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.DownloadManager_ProgressTotal(downloading, percent); }));
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

        private void ProgData_FindNewFailed()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.ProgData_FindNewFailed(); }));
                return;
            }

            // Re-load the last good provider find new page
            this.View_ViewChanged(this.view.CurrentView, ViewState.MainTab.FindProgramme, this.view.CurrentViewData);
        }

        private void ProgData_FoundNew(int progid)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.ProgData_FoundNew(progid); }));
                return;
            }

            this.view.SetView(ViewState.View.ProgEpisodes, progid);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            foreach (string commandLineArg in Environment.GetCommandLineArgs())
            {
                if (commandLineArg.ToUpperInvariant() == "/HIDEMAINWINDOW")
                {
                    if (OsUtils.WinSevenOrLater() && !Settings.CloseToSystray)
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

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.MainFormPos = this.DesktopBounds;
            }

            if (this.WindowState != FormWindowState.Minimized)
            {
                Settings.MainFormState = this.WindowState;
            }
        }

        private void TimerCheckForUpdates_Tick(object sender, EventArgs e)
        {
            if (Settings.LastCheckForUpdates.AddDays(1) < DateTime.Now)
            {
                UpdateCheck.CheckAvailable(this.UpdateAvailable);
            }
        }

        private void UpdateAvailable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() => { this.UpdateAvailable(); }));
                return;
            }

            if (Settings.LastUpdatePrompt.AddDays(7) < DateTime.Now)
            {
                Settings.LastUpdatePrompt = DateTime.Now;

                using (UpdateNotify showUpdate = new UpdateNotify())
                {
                    if (this.WindowState == FormWindowState.Minimized || !this.Visible)
                    {
                        showUpdate.StartPosition = FormStartPosition.CenterScreen;
                    }

                    if (showUpdate.ShowDialog(this) == DialogResult.Yes)
                    {
                        OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/"), "Download Update (Auto)");
                    }
                }
            }
        }

        private void ButtonAddFavourite_Click()
        {
            int progid;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Subscriptions:
                    progid = Convert.ToInt32(this.ListSubscribed.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new InvalidOperationException("Add favourite not valid in " + this.view.CurrentView.ToString() + " view");
            }

            Model.Favourite.Add(progid);
            this.view.SetView(ViewState.MainTab.Favourites, ViewState.View.Favourites, null);
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

            Model.Favourite.Remove(progid);
        }

        private void ButtonSubscribe_Click()
        {
            int progid;

            switch (this.view.CurrentView)
            {
                case ViewState.View.ProgEpisodes:
                    progid = (int)this.view.CurrentViewData;
                    break;
                case ViewState.View.Favourites:
                    progid = Convert.ToInt32(this.ListFavourites.SelectedItems[0].Name, CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new InvalidOperationException("Subscribe not valid in " + this.view.CurrentView.ToString() + " view");
            }

            if (Model.Subscription.Add(progid))
            {
                this.view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions, null);
            }
            else
            {
                MessageBox.Show("This programme only has one episode, which is already in the download list.", Application.ProductName, MessageBoxButtons.OK);
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
                Model.Subscription.Remove(progid);
            }
        }

        private void ButtonCancel_Click()
        {
            int epid = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);

            if (Interaction.MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question | MsgBoxStyle.YesNo) == MsgBoxResult.Yes)
            {
                Model.Download.Remove(epid);
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
                    try
                    {
                        Process.Start(info.DownloadPath);
                    }
                    catch (System.ComponentModel.Win32Exception playExp)
                    {
                        MessageBox.Show("Failed to play the download: " + playExp.Message + ".  Check your file associations for " + Path.GetExtension(info.DownloadPath) + " files and try again.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    // Bump the play count of this item up by one
                    Model.Download.BumpPlayCount(epid);
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

                Model.Download.Remove(epid);
            }
        }

        private void ButtonRetry_Click()
        {
            Model.Download.Reset(Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture));
        }

        private void ButtonDownload_Click()
        {
            List<int> epids = new List<int>();

            foreach (ListViewItem item in this.ListEpisodes.SelectedItems)
            {
                epids.Add(Convert.ToInt32(item.Name, CultureInfo.InvariantCulture));
            }

            // Add the items in date order, oldest first
            epids.Reverse();

            if (Model.Download.Add(epids.ToArray()))
            {
                this.view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads);
            }
            else
            {
                string episodes;

                if (epids.Count == 1)
                {
                    episodes = "The selected episode is";
                }
                else
                {
                    episodes = "All of the selected episodes are";
                }

                MessageBox.Show(episodes + " already in your downloads list.", Application.ProductName);
            }
        }

        private void ButtonAutoDownload_Click()
        {
            List<int> epids = new List<int>();

            foreach (ListViewItem item in this.ListEpisodes.SelectedItems)
            {
                epids.Add(Convert.ToInt32(item.Name, CultureInfo.InvariantCulture));
            }

            Model.Episode info = new Model.Episode(epids[0]);
            Model.Episode.SetAutoDownload(epids.ToArray(), !info.AutoDownload);
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

        private void ButtonMoreInfo_Click()
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

            new Model.Programme(progid).ShowMoreInfo();
        }

        private void ButtonReportError_Click()
        {
            int epid = Convert.ToInt32(this.ListDownloads.SelectedItems[0].Name, CultureInfo.InvariantCulture);
            Model.Download.ReportError(epid);
        }

        private void ButtonChooseProgramme_Click()
        {
            FindNewViewData viewData = default(FindNewViewData);
            viewData.ProviderID = new Guid(this.ListProviders.SelectedItems[0].Name);
            viewData.View = null;

            this.view.SetView(ViewState.View.FindNewProviderForm, viewData);
        }

        private void MenuOptionsShowOpts_Click(object sender, EventArgs e)
        {
            using (Preferences prefs = new Preferences())
            {
                prefs.ShowDialog();
            }
        }

        private void MenuOptionsExit_Click(object sender, EventArgs e)
        {
            this.MenuTrayExit_Click(sender, e);
        }

        private void MenuHelpAbout_Click(object sender, EventArgs e)
        {
            using (About about = new About())
            {
                about.ShowDialog();
            }
        }

        private void MenuHelpContext_Click(object sender, EventArgs e)
        {
            Uri helpUri;

            switch (this.view.CurrentView)
            {
                case ViewState.View.FindNewChooseProvider:
                case ViewState.View.FindNewProviderForm:
                    helpUri = new Uri("https://nerdoftheherd.com/tools/radiodld/help/views.find-programme");
                    break;
                case ViewState.View.ProgEpisodes:
                    helpUri = new Uri("https://nerdoftheherd.com/tools/radiodld/help/views.available-episodes/");
                    break;
                case ViewState.View.Favourites:
                    helpUri = new Uri("https://nerdoftheherd.com/tools/radiodld/help/views.favourites/");
                    break;
                case ViewState.View.Subscriptions:
                    helpUri = new Uri("https://nerdoftheherd.com/tools/radiodld/help/views.subscriptions/");
                    break;
                case ViewState.View.Downloads:
                    helpUri = new Uri("https://nerdoftheherd.com/tools/radiodld/help/views.downloads/");
                    break;
                default:
                    throw new InvalidOperationException("Context sensitive help is not defined for the \"" + this.view.CurrentView.ToString() + "\" view.");
            }

            OsUtils.LaunchUrl(helpUri, "Context Help");
        }

        private void MenuHelpContents_Click(object sender, EventArgs e)
        {
            OsUtils.LaunchUrl(new Uri("https://nerdoftheherd.com/tools/radiodld/help/"), "Help Menu");
        }

        private void MenuHelpReportBug_Click(object sender, EventArgs e)
        {
            OsUtils.LaunchUrl(new Uri("https://github.com/ribbons/RadioDownloader/issues"), "Help Menu");
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
                    this.PanelPluginSpace.Controls.Add(FindNew.GetFindNewPanel(findViewData.ProviderID, findViewData.View));
                    this.PanelPluginSpace.Controls[0].Dock = DockStyle.Fill;
                    this.PanelPluginSpace.Controls[0].Focus();
                    break;
                case ViewState.View.ProgEpisodes:
                    this.ListEpisodes.Visible = true;
                    FindNew.CancelEpisodeListing();
                    this.ListEpisodes.Items.Clear(); // Clear before DoEvents so that old items don't flash up on screen
                    Application.DoEvents(); // Give any queued Invoke calls a chance to be processed
                    this.ListEpisodes.Items.Clear();
                    FindNew.InitEpisodeList((int)data);
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

                    if (!string.IsNullOrEmpty(this.dataSearch.DownloadQuery))
                    {
                        this.SetSideBar(Convert.ToString(this.ListDownloads.Items.Count, CultureInfo.CurrentCulture) + " result" + (this.ListDownloads.Items.Count == 1 ? string.Empty : "s"), string.Empty, null);
                    }
                    else
                    {
                        string description = string.Empty;
                        long newCount = Model.Download.CountNew();
                        long errorCount = Model.Download.CountErrored();

                        if (newCount > 0)
                        {
                            description += "Newly downloaded: " + Convert.ToString(newCount, CultureInfo.CurrentCulture) + Environment.NewLine;
                        }

                        if (errorCount > 0)
                        {
                            description += "Errored: " + Convert.ToString(errorCount, CultureInfo.CurrentCulture);
                        }

                        this.SetSideBar(Convert.ToString(this.ListDownloads.Items.Count, CultureInfo.CurrentCulture) + " download" + (this.ListDownloads.Items.Count == 1 ? string.Empty : "s"), description, null);
                    }

                    break;
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            this.view.NavBack();
        }

        private void ButtonForward_Click(object sender, EventArgs e)
        {
            this.view.NavFwd();
        }

        private void ToolbarMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.Name)
            {
                case "ButtonChooseProgramme":
                    this.ButtonChooseProgramme_Click();
                    break;
                case "ButtonDownload":
                    this.ButtonDownload_Click();
                    break;
                case "ButtonSetAuto":
                    this.ButtonAutoDownload_Click();
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
                case "ButtonMoreInfo":
                    this.ButtonMoreInfo_Click();
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

        private void TableToolbars_Resize(object sender, EventArgs e)
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

        private void TextSidebarDescript_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // Prefix the url with http:// if a protocol isn't specified
            string launch = (e.LinkText.Contains("://") ? string.Empty : "http://") + e.LinkText;

            OsUtils.LaunchUrl(new Uri(launch), "Sidebar Link");
        }

        private void TextSidebarDescript_Resize(object sender, EventArgs e)
        {
            this.TextSidebarDescript.Refresh(); // Make sure the scrollbars update correctly
        }

        private void ImageSidebarBorder_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(255, 167, 186, 197)), 0, 0, 0, this.ImageSidebarBorder.Height);
        }

        private void MenuListHdrsColumns_Click(object sender, EventArgs e)
        {
            using (ChooseCols chooser = new ChooseCols())
            {
                chooser.Columns = Settings.DownloadCols;
                chooser.StoreNameList(this.downloadColNames);

                if (chooser.ShowDialog(this) == DialogResult.OK)
                {
                    Settings.DownloadCols = chooser.Columns;
                    this.InitDownloadList();
                }
            }
        }

        private void MenuListHdrsReset_Click(object sender, EventArgs e)
        {
            Settings.ResetDownloadCols();
            this.InitDownloadList();
        }

        private void InitProviderList()
        {
            foreach (Provider.Handler provider in Provider.Handler.GetAll())
            {
                this.ListProviders.Items.Add(this.ProviderListItem(provider));
            }
        }

        private void InitFavouriteList()
        {
            if (this.ListFavourites.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            // Apply the sort from the current settings
            Model.Favourite.SortByColumn = Settings.FavouriteColSortBy;
            Model.Favourite.SortAscending = Settings.FavouriteColSortAsc;
            this.ListFavourites.ShowSortOnHeader((int)Model.Favourite.SortByColumn, Model.Favourite.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of Favourite items to an array of ListItems
            List<Model.Favourite> initData = Model.Favourite.FetchAll();
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems < initData.Count; convItems++)
            {
                initItems[convItems] = this.FavouriteListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.ListFavourites.Items.AddRange(initItems);
        }

        private void InitSubscriptionList()
        {
            if (this.ListSubscribed.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            // Apply the sort from the current settings
            Model.Subscription.SortByColumn = Settings.SubscriptionColSortBy;
            Model.Subscription.SortAscending = Settings.SubscriptionColSortAsc;
            this.ListSubscribed.ShowSortOnHeader((int)Model.Subscription.SortByColumn, Model.Subscription.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of Subscription items to an array of ListItems
            List<Model.Subscription> initData = Model.Subscription.FetchAll();
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems < initData.Count; convItems++)
            {
                initItems[convItems] = this.SubscriptionListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.ListSubscribed.Items.AddRange(initItems);
        }

        private void InitDownloadList()
        {
            if (this.ListDownloads.SelectedItems.Count > 0)
            {
                this.SetViewDefaults(); // Revert back to default sidebar and toolbar
            }

            this.downloadColSizes.Clear();
            this.downloadColOrder.Clear();
            Application.DoEvents(); // Give any queued Invoke calls a chance to be processed
            this.ListDownloads.Clear();
            this.ListDownloads.HideAllProgress();

            const string DefaultColSizes = "0,2.49|1,0.81|2,1.28|3,1.04|4,0.6|5,1.4";

            if (string.IsNullOrEmpty(Settings.DownloadColSizes))
            {
                Settings.DownloadColSizes = DefaultColSizes;
            }
            else
            {
                string newItems = string.Empty;

                // Find any columns without widths defined in the current setting
                foreach (string sizePair in DefaultColSizes.Split('|'))
                {
                    if (!("|" + Settings.DownloadColSizes).Contains("|" + sizePair.Split(',')[0] + ","))
                    {
                        newItems += "|" + sizePair;
                    }
                }

                // Append the new column sizes to the end of the setting
                if (!string.IsNullOrEmpty(newItems))
                {
                    Settings.DownloadColSizes += newItems;
                }
            }

            // Fetch the column sizes into downloadColSizes for ease of access
            foreach (string sizePair in Settings.DownloadColSizes.Split('|'))
            {
                string[] splitPair = sizePair.Split(',');
                int pixelSize = (int)(float.Parse(splitPair[1], CultureInfo.InvariantCulture) * this.CurrentAutoScaleDimensions.Width);

                this.downloadColSizes.Add(int.Parse(splitPair[0], CultureInfo.InvariantCulture), pixelSize);
            }

            // Set up the columns specified in the DownloadCols setting
            if (!string.IsNullOrEmpty(Settings.DownloadCols))
            {
                string[] columns = Settings.DownloadCols.Split(',');

                foreach (string column in columns)
                {
                    int colVal = int.Parse(column, CultureInfo.InvariantCulture);
                    this.downloadColOrder.Add((Model.Download.DownloadCols)colVal);
                    this.ListDownloads.Columns.Add(this.downloadColNames[colVal], this.downloadColSizes[colVal]);
                }
            }

            // Apply the sort from the current settings
            Model.Download.SortByColumn = Settings.DownloadColSortBy;
            Model.Download.SortAscending = Settings.DownloadColSortAsc;
            this.ListDownloads.ShowSortOnHeader(this.downloadColOrder.IndexOf(Model.Download.SortByColumn), Model.Download.SortAscending ? SortOrder.Ascending : SortOrder.Descending);

            // Convert the list of Download items to an array of ListItems
            List<Model.Download> initData = Model.Download.FetchVisible(this.dataSearch);
            ListViewItem[] initItems = new ListViewItem[initData.Count];

            for (int convItems = 0; convItems < initData.Count; convItems++)
            {
                initItems[convItems] = this.DownloadListItem(initData[convItems], null);
            }

            // Add the whole array of ListItems at once
            this.ListDownloads.Items.AddRange(initItems);
        }

        private void TextSearch_TextChanged(object sender, EventArgs e)
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
                if (!ReferenceEquals(Thread.CurrentThread, this.searchThread))
                {
                    // A search thread was created more recently, stop this thread
                    return;
                }

                if (!this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)(() => { this.PerformSearch(origView, search); }));
                }
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

                this.dataSearch.DownloadQuery = search;
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

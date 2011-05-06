namespace RadioDld
{
    internal partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TableLayoutPanel TableSidebar;
            System.Windows.Forms.MenuItem MenuTraySep;
            System.Windows.Forms.MenuItem MenuOptionsSep;
            System.Windows.Forms.MenuItem MenuHelpSep;
            System.Windows.Forms.MenuItem MenuListHdrsSep;
            this.TextSidebarDescript = new System.Windows.Forms.RichTextBox();
            this.LabelSidebarTitle = new System.Windows.Forms.Label();
            this.ImageSidebar = new System.Windows.Forms.PictureBox();
            this.ToolbarView = new RadioDld.ExtToolStrip();
            this.ButtonBack = new System.Windows.Forms.ToolStripButton();
            this.ButtonForward = new System.Windows.Forms.ToolStripButton();
            this.ButtonFindNew = new System.Windows.Forms.ToolStripButton();
            this.ButtonFavourites = new System.Windows.Forms.ToolStripButton();
            this.ButtonSubscriptions = new System.Windows.Forms.ToolStripButton();
            this.ButtonDownloads = new System.Windows.Forms.ToolStripButton();
            this.TextSearch = new RadioDld.ToolStripSearchBox();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuTray = new System.Windows.Forms.ContextMenu();
            this.MenuTrayShow = new System.Windows.Forms.MenuItem();
            this.MenuTrayExit = new System.Windows.Forms.MenuItem();
            this.ImagesListIcons = new System.Windows.Forms.ImageList(this.components);
            this.ImagesProviders = new System.Windows.Forms.ImageList(this.components);
            this.ProgressDownload = new System.Windows.Forms.ProgressBar();
            this.TimerCheckForUpdates = new System.Windows.Forms.Timer(this.components);
            this.MenuOptions = new System.Windows.Forms.ContextMenu();
            this.MenuOptionsShowOpts = new System.Windows.Forms.MenuItem();
            this.MenuOptionsProviderOpts = new System.Windows.Forms.MenuItem();
            this.MenuOptionsProviderOptsNoProvs = new System.Windows.Forms.MenuItem();
            this.MenuOptionsExit = new System.Windows.Forms.MenuItem();
            this.MenuHelp = new System.Windows.Forms.ContextMenu();
            this.MenuHelpShowHelp = new System.Windows.Forms.MenuItem();
            this.MenuHelpReportBug = new System.Windows.Forms.MenuItem();
            this.MenuHelpAbout = new System.Windows.Forms.MenuItem();
            this.ImagesToolbar = new System.Windows.Forms.ImageList(this.components);
            this.PanelPluginSpace = new System.Windows.Forms.Panel();
            this.TableToolbars = new System.Windows.Forms.TableLayoutPanel();
            this.ToolbarMain = new RadioDld.ExtToolBar();
            this.ButtonOptionsMenu = new System.Windows.Forms.ToolBarButton();
            this.ButtonChooseProgramme = new System.Windows.Forms.ToolBarButton();
            this.ButtonDownload = new System.Windows.Forms.ToolBarButton();
            this.ButtonAddFavourite = new System.Windows.Forms.ToolBarButton();
            this.ButtonRemFavourite = new System.Windows.Forms.ToolBarButton();
            this.ButtonSubscribe = new System.Windows.Forms.ToolBarButton();
            this.ButtonUnsubscribe = new System.Windows.Forms.ToolBarButton();
            this.ButtonCurrentEps = new System.Windows.Forms.ToolBarButton();
            this.ButtonCleanUp = new System.Windows.Forms.ToolBarButton();
            this.ButtonCancel = new System.Windows.Forms.ToolBarButton();
            this.ButtonPlay = new System.Windows.Forms.ToolBarButton();
            this.ButtonDelete = new System.Windows.Forms.ToolBarButton();
            this.ButtonRetry = new System.Windows.Forms.ToolBarButton();
            this.ButtonReportError = new System.Windows.Forms.ToolBarButton();
            this.ToolbarHelp = new RadioDld.ExtToolBar();
            this.ButtonHelpMenu = new System.Windows.Forms.ToolBarButton();
            this.ImageSidebarBorder = new System.Windows.Forms.PictureBox();
            this.MenuListHdrs = new System.Windows.Forms.ContextMenu();
            this.MenuListHdrsColumns = new System.Windows.Forms.MenuItem();
            this.MenuListHdrsReset = new System.Windows.Forms.MenuItem();
            this.ListDownloads = new RadioDld.ExtListView();
            this.ListSubscribed = new RadioDld.ExtListView();
            this.ListEpisodes = new RadioDld.ExtListView();
            this.ListProviders = new RadioDld.ExtListView();
            this.ListFavourites = new RadioDld.ExtListView();
            TableSidebar = new System.Windows.Forms.TableLayoutPanel();
            MenuTraySep = new System.Windows.Forms.MenuItem();
            MenuOptionsSep = new System.Windows.Forms.MenuItem();
            MenuHelpSep = new System.Windows.Forms.MenuItem();
            MenuListHdrsSep = new System.Windows.Forms.MenuItem();
            TableSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSidebar)).BeginInit();
            this.ToolbarView.SuspendLayout();
            this.TableToolbars.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSidebarBorder)).BeginInit();
            this.SuspendLayout();
            // 
            // TableSidebar
            // 
            TableSidebar.BackColor = System.Drawing.SystemColors.Window;
            TableSidebar.ColumnCount = 1;
            TableSidebar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            TableSidebar.Controls.Add(this.TextSidebarDescript, 0, 2);
            TableSidebar.Controls.Add(this.LabelSidebarTitle, 0, 0);
            TableSidebar.Controls.Add(this.ImageSidebar, 0, 1);
            TableSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            TableSidebar.Location = new System.Drawing.Point(0, 65);
            TableSidebar.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            TableSidebar.Name = "TableSidebar";
            TableSidebar.RowCount = 4;
            TableSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            TableSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            TableSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            TableSidebar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            TableSidebar.Size = new System.Drawing.Size(187, 385);
            TableSidebar.TabIndex = 9;
            // 
            // TextSidebarDescript
            // 
            this.TextSidebarDescript.BackColor = System.Drawing.SystemColors.Window;
            this.TextSidebarDescript.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextSidebarDescript.Cursor = System.Windows.Forms.Cursors.Default;
            this.TextSidebarDescript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextSidebarDescript.Location = new System.Drawing.Point(8, 120);
            this.TextSidebarDescript.Margin = new System.Windows.Forms.Padding(8, 5, 8, 6);
            this.TextSidebarDescript.Name = "TextSidebarDescript";
            this.TextSidebarDescript.ReadOnly = true;
            this.TextSidebarDescript.ShortcutsEnabled = false;
            this.TextSidebarDescript.Size = new System.Drawing.Size(171, 259);
            this.TextSidebarDescript.TabIndex = 1;
            this.TextSidebarDescript.TabStop = false;
            this.TextSidebarDescript.Text = "Description";
            this.TextSidebarDescript.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.TextSidebarDescript_LinkClicked);
            // 
            // LabelSidebarTitle
            // 
            this.LabelSidebarTitle.AutoSize = true;
            this.LabelSidebarTitle.BackColor = System.Drawing.Color.Transparent;
            this.LabelSidebarTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelSidebarTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSidebarTitle.ForeColor = System.Drawing.SystemColors.WindowText;
            this.LabelSidebarTitle.Location = new System.Drawing.Point(8, 10);
            this.LabelSidebarTitle.Margin = new System.Windows.Forms.Padding(8, 10, 8, 8);
            this.LabelSidebarTitle.Name = "LabelSidebarTitle";
            this.LabelSidebarTitle.Size = new System.Drawing.Size(171, 19);
            this.LabelSidebarTitle.TabIndex = 0;
            this.LabelSidebarTitle.Text = "Title";
            this.LabelSidebarTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LabelSidebarTitle.UseMnemonic = false;
            // 
            // ImageSidebar
            // 
            this.ImageSidebar.ErrorImage = null;
            this.ImageSidebar.InitialImage = null;
            this.ImageSidebar.Location = new System.Drawing.Point(12, 40);
            this.ImageSidebar.Margin = new System.Windows.Forms.Padding(12, 3, 12, 5);
            this.ImageSidebar.MaximumSize = new System.Drawing.Size(160, 100);
            this.ImageSidebar.Name = "ImageSidebar";
            this.ImageSidebar.Size = new System.Drawing.Size(70, 70);
            this.ImageSidebar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ImageSidebar.TabIndex = 1;
            this.ImageSidebar.TabStop = false;
            // 
            // MenuTraySep
            // 
            MenuTraySep.Index = 1;
            MenuTraySep.Text = "-";
            // 
            // MenuOptionsSep
            // 
            MenuOptionsSep.Index = 2;
            MenuOptionsSep.Text = "-";
            // 
            // MenuHelpSep
            // 
            MenuHelpSep.Index = 2;
            MenuHelpSep.Text = "-";
            // 
            // MenuListHdrsSep
            // 
            MenuListHdrsSep.Index = 1;
            MenuListHdrsSep.Text = "-";
            // 
            // ToolbarView
            // 
            this.ToolbarView.CanOverflow = false;
            this.ToolbarView.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolbarView.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ToolbarView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ButtonBack,
            this.ButtonForward,
            this.ButtonFindNew,
            this.ButtonFavourites,
            this.ButtonSubscriptions,
            this.ButtonDownloads,
            this.TextSearch});
            this.ToolbarView.Location = new System.Drawing.Point(0, 0);
            this.ToolbarView.Name = "ToolbarView";
            this.ToolbarView.Padding = new System.Windows.Forms.Padding(3, 0, 1, 0);
            this.ToolbarView.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.ToolbarView.Size = new System.Drawing.Size(757, 31);
            this.ToolbarView.TabIndex = 7;
            this.ToolbarView.TabStop = true;
            // 
            // ButtonBack
            // 
            this.ButtonBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ButtonBack.Image = global::RadioDld.Properties.Resources.views_back;
            this.ButtonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonBack.Name = "ButtonBack";
            this.ButtonBack.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ButtonBack.Size = new System.Drawing.Size(30, 28);
            this.ButtonBack.Text = "Back";
            this.ButtonBack.Click += new System.EventHandler(this.ButtonBack_Click);
            // 
            // ButtonForward
            // 
            this.ButtonForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ButtonForward.Image = global::RadioDld.Properties.Resources.views_forward;
            this.ButtonForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonForward.Margin = new System.Windows.Forms.Padding(0, 1, 8, 2);
            this.ButtonForward.Name = "ButtonForward";
            this.ButtonForward.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ButtonForward.Size = new System.Drawing.Size(30, 28);
            this.ButtonForward.Text = "Forward";
            this.ButtonForward.Click += new System.EventHandler(this.ButtonForward_Click);
            // 
            // ButtonFindNew
            // 
            this.ButtonFindNew.AutoToolTip = false;
            this.ButtonFindNew.Checked = true;
            this.ButtonFindNew.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ButtonFindNew.Image = global::RadioDld.Properties.Resources.views_find_new;
            this.ButtonFindNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonFindNew.Name = "ButtonFindNew";
            this.ButtonFindNew.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ButtonFindNew.Size = new System.Drawing.Size(128, 28);
            this.ButtonFindNew.Text = "&Find Programme";
            this.ButtonFindNew.Click += new System.EventHandler(this.ButtonFindNew_Click);
            // 
            // ButtonFavourites
            // 
            this.ButtonFavourites.AutoToolTip = false;
            this.ButtonFavourites.Image = global::RadioDld.Properties.Resources.views_favourites;
            this.ButtonFavourites.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonFavourites.Name = "ButtonFavourites";
            this.ButtonFavourites.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ButtonFavourites.Size = new System.Drawing.Size(93, 28);
            this.ButtonFavourites.Text = "F&avourites";
            this.ButtonFavourites.Click += new System.EventHandler(this.ButtonFavourites_Click);
            // 
            // ButtonSubscriptions
            // 
            this.ButtonSubscriptions.AutoToolTip = false;
            this.ButtonSubscriptions.Image = global::RadioDld.Properties.Resources.views_subscriptions;
            this.ButtonSubscriptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonSubscriptions.Name = "ButtonSubscriptions";
            this.ButtonSubscriptions.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ButtonSubscriptions.Size = new System.Drawing.Size(110, 28);
            this.ButtonSubscriptions.Text = "&Subscriptions";
            this.ButtonSubscriptions.Click += new System.EventHandler(this.ButtonSubscriptions_Click);
            // 
            // ButtonDownloads
            // 
            this.ButtonDownloads.AutoToolTip = false;
            this.ButtonDownloads.Image = global::RadioDld.Properties.Resources.views_downloads;
            this.ButtonDownloads.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ButtonDownloads.Name = "ButtonDownloads";
            this.ButtonDownloads.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ButtonDownloads.Size = new System.Drawing.Size(98, 28);
            this.ButtonDownloads.Text = "&Downloads";
            this.ButtonDownloads.Click += new System.EventHandler(this.ButtonDownloads_Click);
            // 
            // TextSearch
            // 
            this.TextSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.TextSearch.BackColor = System.Drawing.Color.Transparent;
            this.TextSearch.ControlAlign = System.Drawing.ContentAlignment.TopRight;
            this.TextSearch.CueBanner = "Search downloads";
            this.TextSearch.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
            this.TextSearch.Name = "TextSearch";
            this.TextSearch.Size = new System.Drawing.Size(160, 30);
            this.TextSearch.TextChanged += new System.EventHandler(this.TextSearch_TextChanged);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenu = this.MenuTray;
            this.NotifyIcon.BalloonTipClicked += new System.EventHandler(this.NotifyIcon_BalloonTipClicked);
            this.NotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // MenuTray
            // 
            this.MenuTray.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuTrayShow,
            MenuTraySep,
            this.MenuTrayExit});
            // 
            // MenuTrayShow
            // 
            this.MenuTrayShow.Index = 0;
            this.MenuTrayShow.Text = "&Show Radio Downloader";
            this.MenuTrayShow.Click += new System.EventHandler(this.MenuTrayShow_Click);
            // 
            // MenuTrayExit
            // 
            this.MenuTrayExit.Index = 2;
            this.MenuTrayExit.Text = "E&xit";
            this.MenuTrayExit.Click += new System.EventHandler(this.MenuTrayExit_Click);
            // 
            // ImagesListIcons
            // 
            this.ImagesListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ImagesListIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.ImagesListIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ImagesProviders
            // 
            this.ImagesProviders.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ImagesProviders.ImageSize = new System.Drawing.Size(64, 64);
            this.ImagesProviders.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ProgressDownload
            // 
            this.ProgressDownload.Location = new System.Drawing.Point(438, 356);
            this.ProgressDownload.Name = "ProgressDownload";
            this.ProgressDownload.Size = new System.Drawing.Size(100, 23);
            this.ProgressDownload.TabIndex = 6;
            this.ProgressDownload.Visible = false;
            // 
            // TimerCheckForUpdates
            // 
            this.TimerCheckForUpdates.Interval = 3600000;
            this.TimerCheckForUpdates.Tick += new System.EventHandler(this.TimerCheckForUpdates_Tick);
            // 
            // MenuOptions
            // 
            this.MenuOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuOptionsShowOpts,
            this.MenuOptionsProviderOpts,
            MenuOptionsSep,
            this.MenuOptionsExit});
            // 
            // MenuOptionsShowOpts
            // 
            this.MenuOptionsShowOpts.Index = 0;
            this.MenuOptionsShowOpts.Text = "Main &Options";
            this.MenuOptionsShowOpts.Click += new System.EventHandler(this.MenuOptionsShowOpts_Click);
            // 
            // MenuOptionsProviderOpts
            // 
            this.MenuOptionsProviderOpts.Index = 1;
            this.MenuOptionsProviderOpts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuOptionsProviderOptsNoProvs});
            this.MenuOptionsProviderOpts.Text = "&Provider Options";
            // 
            // MenuOptionsProviderOptsNoProvs
            // 
            this.MenuOptionsProviderOptsNoProvs.Enabled = false;
            this.MenuOptionsProviderOptsNoProvs.Index = 0;
            this.MenuOptionsProviderOptsNoProvs.Text = "No providers";
            // 
            // MenuOptionsExit
            // 
            this.MenuOptionsExit.Index = 3;
            this.MenuOptionsExit.Text = "E&xit";
            this.MenuOptionsExit.Click += new System.EventHandler(this.MenuOptionsExit_Click);
            // 
            // MenuHelp
            // 
            this.MenuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuHelpShowHelp,
            this.MenuHelpReportBug,
            MenuHelpSep,
            this.MenuHelpAbout});
            // 
            // MenuHelpShowHelp
            // 
            this.MenuHelpShowHelp.Index = 0;
            this.MenuHelpShowHelp.Shortcut = System.Windows.Forms.Shortcut.F1;
            this.MenuHelpShowHelp.Text = "&Help Contents";
            this.MenuHelpShowHelp.Click += new System.EventHandler(this.MenuHelpShowHelp_Click);
            // 
            // MenuHelpReportBug
            // 
            this.MenuHelpReportBug.Index = 1;
            this.MenuHelpReportBug.Text = "Report a &Bug";
            this.MenuHelpReportBug.Click += new System.EventHandler(this.MenuHelpReportBug_Click);
            // 
            // MenuHelpAbout
            // 
            this.MenuHelpAbout.Index = 3;
            this.MenuHelpAbout.Text = "&About";
            this.MenuHelpAbout.Click += new System.EventHandler(this.MenuHelpAbout_Click);
            // 
            // ImagesToolbar
            // 
            this.ImagesToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ImagesToolbar.ImageSize = new System.Drawing.Size(16, 16);
            this.ImagesToolbar.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // PanelPluginSpace
            // 
            this.PanelPluginSpace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelPluginSpace.Location = new System.Drawing.Point(197, 118);
            this.PanelPluginSpace.Name = "PanelPluginSpace";
            this.PanelPluginSpace.Size = new System.Drawing.Size(560, 41);
            this.PanelPluginSpace.TabIndex = 1;
            // 
            // TableToolbars
            // 
            this.TableToolbars.ColumnCount = 2;
            this.TableToolbars.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.TableToolbars.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.TableToolbars.Controls.Add(this.ToolbarMain, 0, 0);
            this.TableToolbars.Controls.Add(this.ToolbarHelp, 1, 0);
            this.TableToolbars.Dock = System.Windows.Forms.DockStyle.Top;
            this.TableToolbars.Location = new System.Drawing.Point(0, 31);
            this.TableToolbars.Name = "TableToolbars";
            this.TableToolbars.RowCount = 1;
            this.TableToolbars.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableToolbars.Size = new System.Drawing.Size(757, 34);
            this.TableToolbars.TabIndex = 8;
            this.TableToolbars.Resize += new System.EventHandler(this.TableToolbars_Resize);
            // 
            // ToolbarMain
            // 
            this.ToolbarMain.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.ToolbarMain.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.ButtonOptionsMenu,
            this.ButtonChooseProgramme,
            this.ButtonDownload,
            this.ButtonAddFavourite,
            this.ButtonRemFavourite,
            this.ButtonSubscribe,
            this.ButtonUnsubscribe,
            this.ButtonCurrentEps,
            this.ButtonCleanUp,
            this.ButtonCancel,
            this.ButtonPlay,
            this.ButtonDelete,
            this.ButtonRetry,
            this.ButtonReportError});
            this.ToolbarMain.ButtonSize = new System.Drawing.Size(135, 22);
            this.ToolbarMain.Divider = false;
            this.ToolbarMain.DropDownArrows = true;
            this.ToolbarMain.Location = new System.Drawing.Point(3, 2);
            this.ToolbarMain.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
            this.ToolbarMain.Name = "ToolbarMain";
            this.ToolbarMain.ShowToolTips = true;
            this.ToolbarMain.Size = new System.Drawing.Size(678, 26);
            this.ToolbarMain.TabIndex = 0;
            this.ToolbarMain.TabStop = true;
            this.ToolbarMain.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
            this.ToolbarMain.Wrappable = false;
            this.ToolbarMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.ToolbarMain_ButtonClick);
            // 
            // ButtonOptionsMenu
            // 
            this.ButtonOptionsMenu.DropDownMenu = this.MenuOptions;
            this.ButtonOptionsMenu.ImageKey = "options";
            this.ButtonOptionsMenu.Name = "ButtonOptionsMenu";
            this.ButtonOptionsMenu.Text = "&Options";
            // 
            // ButtonChooseProgramme
            // 
            this.ButtonChooseProgramme.ImageKey = "choose_programme";
            this.ButtonChooseProgramme.Name = "ButtonChooseProgramme";
            this.ButtonChooseProgramme.Text = "&Choose Programme";
            // 
            // ButtonDownload
            // 
            this.ButtonDownload.ImageKey = "download";
            this.ButtonDownload.Name = "ButtonDownload";
            this.ButtonDownload.Text = "Do&wnload";
            // 
            // ButtonAddFavourite
            // 
            this.ButtonAddFavourite.ImageKey = "add_favourite";
            this.ButtonAddFavourite.Name = "ButtonAddFavourite";
            this.ButtonAddFavourite.Text = "Add &Favourite";
            // 
            // ButtonRemFavourite
            // 
            this.ButtonRemFavourite.ImageKey = "remove_favourite";
            this.ButtonRemFavourite.Name = "ButtonRemFavourite";
            this.ButtonRemFavourite.Text = "Remove &Favourite";
            // 
            // ButtonSubscribe
            // 
            this.ButtonSubscribe.ImageKey = "subscribe";
            this.ButtonSubscribe.Name = "ButtonSubscribe";
            this.ButtonSubscribe.Text = "S&ubscribe";
            // 
            // ButtonUnsubscribe
            // 
            this.ButtonUnsubscribe.ImageKey = "unsubscribe";
            this.ButtonUnsubscribe.Name = "ButtonUnsubscribe";
            this.ButtonUnsubscribe.Text = "&Unsubscribe";
            // 
            // ButtonCurrentEps
            // 
            this.ButtonCurrentEps.ImageKey = "current_episodes";
            this.ButtonCurrentEps.Name = "ButtonCurrentEps";
            this.ButtonCurrentEps.Text = "&Current Episodes";
            // 
            // ButtonCleanUp
            // 
            this.ButtonCleanUp.ImageKey = "clean_up";
            this.ButtonCleanUp.Name = "ButtonCleanUp";
            this.ButtonCleanUp.Text = "Clean &Up";
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.ImageKey = "delete";
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Text = "&Cancel";
            // 
            // ButtonPlay
            // 
            this.ButtonPlay.ImageKey = "play";
            this.ButtonPlay.Name = "ButtonPlay";
            this.ButtonPlay.Text = "&Play";
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.ImageKey = "delete";
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Text = "D&elete";
            // 
            // ButtonRetry
            // 
            this.ButtonRetry.ImageKey = "retry";
            this.ButtonRetry.Name = "ButtonRetry";
            this.ButtonRetry.Text = "&Retry";
            // 
            // ButtonReportError
            // 
            this.ButtonReportError.ImageKey = "report_error";
            this.ButtonReportError.Name = "ButtonReportError";
            this.ButtonReportError.Text = "Report &Error";
            // 
            // ToolbarHelp
            // 
            this.ToolbarHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolbarHelp.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.ToolbarHelp.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.ButtonHelpMenu});
            this.ToolbarHelp.Divider = false;
            this.ToolbarHelp.Dock = System.Windows.Forms.DockStyle.None;
            this.ToolbarHelp.DropDownArrows = true;
            this.ToolbarHelp.Location = new System.Drawing.Point(681, 2);
            this.ToolbarHelp.Margin = new System.Windows.Forms.Padding(0, 2, 3, 0);
            this.ToolbarHelp.Name = "ToolbarHelp";
            this.ToolbarHelp.ShowToolTips = true;
            this.ToolbarHelp.Size = new System.Drawing.Size(73, 26);
            this.ToolbarHelp.TabIndex = 1;
            this.ToolbarHelp.TabStop = true;
            this.ToolbarHelp.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
            this.ToolbarHelp.Wrappable = false;
            // 
            // ButtonHelpMenu
            // 
            this.ButtonHelpMenu.DropDownMenu = this.MenuHelp;
            this.ButtonHelpMenu.ImageKey = "help";
            this.ButtonHelpMenu.Name = "ButtonHelpMenu";
            this.ButtonHelpMenu.Text = "&Help";
            // 
            // ImageSidebarBorder
            // 
            this.ImageSidebarBorder.BackColor = System.Drawing.SystemColors.Window;
            this.ImageSidebarBorder.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImageSidebarBorder.Location = new System.Drawing.Point(187, 65);
            this.ImageSidebarBorder.Margin = new System.Windows.Forms.Padding(0);
            this.ImageSidebarBorder.Name = "ImageSidebarBorder";
            this.ImageSidebarBorder.Size = new System.Drawing.Size(10, 385);
            this.ImageSidebarBorder.TabIndex = 22;
            this.ImageSidebarBorder.TabStop = false;
            this.ImageSidebarBorder.Paint += new System.Windows.Forms.PaintEventHandler(this.ImageSidebarBorder_Paint);
            // 
            // MenuListHdrs
            // 
            this.MenuListHdrs.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuListHdrsColumns,
            MenuListHdrsSep,
            this.MenuListHdrsReset});
            // 
            // MenuListHdrsColumns
            // 
            this.MenuListHdrsColumns.Index = 0;
            this.MenuListHdrsColumns.Text = "Choose Columns...";
            this.MenuListHdrsColumns.Click += new System.EventHandler(this.MenuListHdrsColumns_Click);
            // 
            // MenuListHdrsReset
            // 
            this.MenuListHdrsReset.Index = 2;
            this.MenuListHdrsReset.Text = "Reset";
            this.MenuListHdrsReset.Click += new System.EventHandler(this.MenuListHdrsReset_Click);
            // 
            // ListDownloads
            // 
            this.ListDownloads.AllowColumnReorder = true;
            this.ListDownloads.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListDownloads.FullRowSelect = true;
            this.ListDownloads.HideSelection = false;
            this.ListDownloads.Location = new System.Drawing.Point(197, 396);
            this.ListDownloads.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
            this.ListDownloads.MultiSelect = false;
            this.ListDownloads.Name = "ListDownloads";
            this.ListDownloads.Size = new System.Drawing.Size(560, 54);
            this.ListDownloads.TabIndex = 5;
            this.ListDownloads.UseCompatibleStateImageBehavior = false;
            this.ListDownloads.View = System.Windows.Forms.View.Details;
            this.ListDownloads.ColumnRightClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListDownloads_ColumnRightClick);
            this.ListDownloads.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListDownloads_ColumnClick);
            this.ListDownloads.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(this.ListDownloads_ColumnReordered);
            this.ListDownloads.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.ListDownloads_ColumnWidthChanged);
            this.ListDownloads.ItemActivate += new System.EventHandler(this.ListDownloads_ItemActivate);
            this.ListDownloads.SelectedIndexChanged += new System.EventHandler(this.ListDownloads_SelectedIndexChanged);
            // 
            // ListSubscribed
            // 
            this.ListSubscribed.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListSubscribed.FullRowSelect = true;
            this.ListSubscribed.HideSelection = false;
            this.ListSubscribed.Location = new System.Drawing.Point(197, 301);
            this.ListSubscribed.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.ListSubscribed.MultiSelect = false;
            this.ListSubscribed.Name = "ListSubscribed";
            this.ListSubscribed.Size = new System.Drawing.Size(560, 49);
            this.ListSubscribed.TabIndex = 4;
            this.ListSubscribed.UseCompatibleStateImageBehavior = false;
            this.ListSubscribed.View = System.Windows.Forms.View.Details;
            this.ListSubscribed.SelectedIndexChanged += new System.EventHandler(this.ListSubscribed_SelectedIndexChanged);
            // 
            // ListEpisodes
            // 
            this.ListEpisodes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListEpisodes.CheckBoxes = true;
            this.ListEpisodes.FullRowSelect = true;
            this.ListEpisodes.HideSelection = false;
            this.ListEpisodes.Location = new System.Drawing.Point(197, 171);
            this.ListEpisodes.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.ListEpisodes.MultiSelect = false;
            this.ListEpisodes.Name = "ListEpisodes";
            this.ListEpisodes.Size = new System.Drawing.Size(560, 50);
            this.ListEpisodes.TabIndex = 2;
            this.ListEpisodes.UseCompatibleStateImageBehavior = false;
            this.ListEpisodes.View = System.Windows.Forms.View.Details;
            this.ListEpisodes.SelectedIndexChanged += new System.EventHandler(this.ListEpisodes_SelectedIndexChanged);
            // 
            // ListProviders
            // 
            this.ListProviders.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListProviders.HideSelection = false;
            this.ListProviders.Location = new System.Drawing.Point(197, 63);
            this.ListProviders.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.ListProviders.MultiSelect = false;
            this.ListProviders.Name = "ListProviders";
            this.ListProviders.Size = new System.Drawing.Size(560, 49);
            this.ListProviders.TabIndex = 0;
            this.ListProviders.UseCompatibleStateImageBehavior = false;
            this.ListProviders.ItemActivate += new System.EventHandler(this.ListProviders_ItemActivate);
            this.ListProviders.SelectedIndexChanged += new System.EventHandler(this.ListProviders_SelectedIndexChanged);
            // 
            // ListFavourites
            // 
            this.ListFavourites.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListFavourites.FullRowSelect = true;
            this.ListFavourites.HideSelection = false;
            this.ListFavourites.Location = new System.Drawing.Point(197, 236);
            this.ListFavourites.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.ListFavourites.MultiSelect = false;
            this.ListFavourites.Name = "ListFavourites";
            this.ListFavourites.Size = new System.Drawing.Size(560, 50);
            this.ListFavourites.TabIndex = 3;
            this.ListFavourites.UseCompatibleStateImageBehavior = false;
            this.ListFavourites.View = System.Windows.Forms.View.Details;
            this.ListFavourites.SelectedIndexChanged += new System.EventHandler(this.ListFavourites_SelectedIndexChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(757, 450);
            this.Controls.Add(this.ListFavourites);
            this.Controls.Add(this.ListDownloads);
            this.Controls.Add(this.ListSubscribed);
            this.Controls.Add(this.ListEpisodes);
            this.Controls.Add(this.PanelPluginSpace);
            this.Controls.Add(this.ListProviders);
            this.Controls.Add(this.ImageSidebarBorder);
            this.Controls.Add(this.ProgressDownload);
            this.Controls.Add(TableSidebar);
            this.Controls.Add(this.TableToolbars);
            this.Controls.Add(this.ToolbarView);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = global::RadioDld.Properties.Resources.icon_main;
            this.KeyPreview = true;
            this.Location = new System.Drawing.Point(11, 37);
            this.MinimumSize = new System.Drawing.Size(550, 300);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Radio Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.Move += new System.EventHandler(this.Main_Move_Resize);
            this.Resize += new System.EventHandler(this.Main_Move_Resize);
            TableSidebar.ResumeLayout(false);
            TableSidebar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSidebar)).EndInit();
            this.ToolbarView.ResumeLayout(false);
            this.ToolbarView.PerformLayout();
            this.TableToolbars.ResumeLayout(false);
            this.TableToolbars.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageSidebarBorder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.ContextMenu MenuHelp;
        private System.Windows.Forms.ToolStripButton ButtonFindNew;
        private System.Windows.Forms.ToolStripButton ButtonSubscriptions;
        private System.Windows.Forms.ToolStripButton ButtonDownloads;
        private ToolStripSearchBox TextSearch;
        private ExtListView ListProviders;
        private ExtListView ListSubscribed;
        private System.Windows.Forms.ProgressBar ProgressDownload;
        private ExtListView ListDownloads;
        private ExtToolBar ToolbarMain;
        private System.Windows.Forms.PictureBox ImageSidebar;
        private ExtListView ListEpisodes;
        private System.Windows.Forms.Panel PanelPluginSpace;
        private System.Windows.Forms.ToolStripButton ButtonFavourites;
        private System.Windows.Forms.ToolStripButton ButtonBack;
        private System.Windows.Forms.ToolStripButton ButtonForward;
        private ExtToolBar ToolbarHelp;
        private System.Windows.Forms.Label LabelSidebarTitle;
        private System.Windows.Forms.RichTextBox TextSidebarDescript;
        private ExtListView ListFavourites;
        private System.Windows.Forms.PictureBox ImageSidebarBorder;
        private System.Windows.Forms.ToolBarButton ButtonAddFavourite;
        private System.Windows.Forms.ToolBarButton ButtonRemFavourite;
        private System.Windows.Forms.ToolBarButton ButtonSubscribe;
        private System.Windows.Forms.ToolBarButton ButtonUnsubscribe;
        private System.Windows.Forms.ToolBarButton ButtonCurrentEps;
        private System.Windows.Forms.ToolBarButton ButtonCleanUp;
        private System.Windows.Forms.ToolBarButton ButtonCancel;
        private System.Windows.Forms.ToolBarButton ButtonPlay;
        private System.Windows.Forms.ToolBarButton ButtonDelete;
        private System.Windows.Forms.ToolBarButton ButtonRetry;
        private System.Windows.Forms.ToolBarButton ButtonReportError;
        private System.Windows.Forms.ToolBarButton ButtonHelpMenu;
        private ExtToolStrip ToolbarView;
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenu MenuTray;
        private System.Windows.Forms.MenuItem MenuTrayShow;
        private System.Windows.Forms.MenuItem MenuTrayExit;
        private System.Windows.Forms.ImageList ImagesListIcons;
        private System.Windows.Forms.ImageList ImagesProviders;
        private System.Windows.Forms.Timer TimerCheckForUpdates;
        private System.Windows.Forms.MenuItem MenuOptionsShowOpts;
        private System.Windows.Forms.MenuItem MenuOptionsExit;
        private System.Windows.Forms.MenuItem MenuHelpShowHelp;
        private System.Windows.Forms.MenuItem MenuHelpAbout;
        private System.Windows.Forms.MenuItem MenuHelpReportBug;
        private System.Windows.Forms.ImageList ImagesToolbar;
        private System.Windows.Forms.ContextMenu MenuOptions;
        private System.Windows.Forms.MenuItem MenuOptionsProviderOpts;
        private System.Windows.Forms.MenuItem MenuOptionsProviderOptsNoProvs;
        private System.Windows.Forms.ContextMenu MenuListHdrs;
        private System.Windows.Forms.MenuItem MenuListHdrsColumns;
        private System.Windows.Forms.MenuItem MenuListHdrsReset;
        private System.Windows.Forms.TableLayoutPanel TableToolbars;
        private System.Windows.Forms.ToolBarButton ButtonOptionsMenu;
        private System.Windows.Forms.ToolBarButton ButtonChooseProgramme;
        private System.Windows.Forms.ToolBarButton ButtonDownload;

        #endregion
    }
}

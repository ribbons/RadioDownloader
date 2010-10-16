using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
namespace RadioDld
{
	[Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
	partial class Main
	{
		#region "Windows Form Designer generated code "
		[System.Diagnostics.DebuggerNonUserCode()]
		public Main() : base()
		{
			Resize += Main_Move_Resize;
			Move += Main_Move_Resize;
			Shown += Main_Shown;
			FormClosing += Main_FormClosing;
			Load += Main_Load;
			KeyDown += Main_KeyDown;
			//This call is required by the Windows Form Designer.
			InitializeComponent();
		}
//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool Disposing)
		{
			if (Disposing) {
				if ((components != null)) {
					components.Dispose();
				}
			}
			base.Dispose(Disposing);
		}
//Required by the Windows Form Designer
		private System.ComponentModel.IContainer components;
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tbrView = new RadioDld.ExtToolStrip();
			this.tbtBack = new System.Windows.Forms.ToolStripButton();
			this.tbtForward = new System.Windows.Forms.ToolStripButton();
			this.tbtFindNew = new System.Windows.Forms.ToolStripButton();
			this.tbtFavourites = new System.Windows.Forms.ToolStripButton();
			this.tbtSubscriptions = new System.Windows.Forms.ToolStripButton();
			this.tbtDownloads = new System.Windows.Forms.ToolStripButton();
			this.ttxSearch = new RadioDld.ToolStripSearchBox();
			this.nicTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.mnuTray = new System.Windows.Forms.ContextMenu();
			this.mnuTrayShow = new System.Windows.Forms.MenuItem();
			this.mnuTraySep = new System.Windows.Forms.MenuItem();
			this.mnuTrayExit = new System.Windows.Forms.MenuItem();
			this.imlListIcons = new System.Windows.Forms.ImageList(this.components);
			this.imlProviders = new System.Windows.Forms.ImageList(this.components);
			this.prgDldProg = new System.Windows.Forms.ProgressBar();
			this.tmrCheckForUpdates = new System.Windows.Forms.Timer(this.components);
			this.mnuOptions = new System.Windows.Forms.ContextMenu();
			this.mnuOptionsShowOpts = new System.Windows.Forms.MenuItem();
			this.mnuOptionsProviderOpts = new System.Windows.Forms.MenuItem();
			this.mnuOptionsProviderOptsNoProvs = new System.Windows.Forms.MenuItem();
			this.mnuOptionsSep = new System.Windows.Forms.MenuItem();
			this.mnuOptionsExit = new System.Windows.Forms.MenuItem();
			this.mnuHelp = new System.Windows.Forms.ContextMenu();
			this.mnuHelpShowHelp = new System.Windows.Forms.MenuItem();
			this.mnuHelpReportBug = new System.Windows.Forms.MenuItem();
			this.mnuHelpSep = new System.Windows.Forms.MenuItem();
			this.mnuHelpAbout = new System.Windows.Forms.MenuItem();
			this.imlToolbar = new System.Windows.Forms.ImageList(this.components);
			this.tblInfo = new System.Windows.Forms.TableLayoutPanel();
			this.txtSideDescript = new System.Windows.Forms.RichTextBox();
			this.lblSideMainTitle = new System.Windows.Forms.Label();
			this.picSidebarImg = new System.Windows.Forms.PictureBox();
			this.pnlPluginSpace = new System.Windows.Forms.Panel();
			this.tblToolbars = new System.Windows.Forms.TableLayoutPanel();
			this.tbrToolbar = new RadioDld.ExtToolBar();
			this.tbtOptionsMenu = new System.Windows.Forms.ToolBarButton();
			this.tbtChooseProgramme = new System.Windows.Forms.ToolBarButton();
			this.tbtDownload = new System.Windows.Forms.ToolBarButton();
			this.tbtAddFavourite = new System.Windows.Forms.ToolBarButton();
			this.tbtRemFavourite = new System.Windows.Forms.ToolBarButton();
			this.tbtSubscribe = new System.Windows.Forms.ToolBarButton();
			this.tbtUnsubscribe = new System.Windows.Forms.ToolBarButton();
			this.tbtCurrentEps = new System.Windows.Forms.ToolBarButton();
			this.tbtCancel = new System.Windows.Forms.ToolBarButton();
			this.tbtPlay = new System.Windows.Forms.ToolBarButton();
			this.tbtDelete = new System.Windows.Forms.ToolBarButton();
			this.tbtRetry = new System.Windows.Forms.ToolBarButton();
			this.tbtReportError = new System.Windows.Forms.ToolBarButton();
			this.tbtCleanUp = new System.Windows.Forms.ToolBarButton();
			this.tbrHelp = new RadioDld.ExtToolBar();
			this.tbtHelpMenu = new System.Windows.Forms.ToolBarButton();
			this.picSideBarBorder = new System.Windows.Forms.PictureBox();
			this.mnuListHdrs = new System.Windows.Forms.ContextMenu();
			this.mnuListHdrsColumns = new System.Windows.Forms.MenuItem();
			this.mnuListHdrsSep = new System.Windows.Forms.MenuItem();
			this.mnuListHdrsReset = new System.Windows.Forms.MenuItem();
			this.lstDownloads = new RadioDld.ExtListView();
			this.lstSubscribed = new RadioDld.ExtListView();
			this.lstEpisodes = new RadioDld.ExtListView();
			this.lstProviders = new RadioDld.ExtListView();
			this.lstFavourites = new RadioDld.ExtListView();
			this.tbrView.SuspendLayout();
			this.tblInfo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.picSidebarImg).BeginInit();
			this.tblToolbars.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.picSideBarBorder).BeginInit();
			this.SuspendLayout();
			//
			//tbrView
			//
			this.tbrView.CanOverflow = false;
			this.tbrView.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tbrView.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.tbrView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.tbtBack,
				this.tbtForward,
				this.tbtFindNew,
				this.tbtFavourites,
				this.tbtSubscriptions,
				this.tbtDownloads,
				this.ttxSearch
			});
			this.tbrView.Location = new System.Drawing.Point(0, 0);
			this.tbrView.Name = "tbrView";
			this.tbrView.Padding = new System.Windows.Forms.Padding(3, 0, 1, 0);
			this.tbrView.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tbrView.Size = new System.Drawing.Size(757, 31);
			this.tbrView.TabIndex = 7;
			this.tbrView.TabStop = true;
			//
			//tbtBack
			//
			this.tbtBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbtBack.Image = Properties.Resources.views_back;
			this.tbtBack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtBack.Name = "tbtBack";
			this.tbtBack.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.tbtBack.Size = new System.Drawing.Size(30, 28);
			this.tbtBack.Text = "Back";
			//
			//tbtForward
			//
			this.tbtForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tbtForward.Image = Properties.Resources.views_forward;
			this.tbtForward.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtForward.Margin = new System.Windows.Forms.Padding(0, 1, 8, 2);
			this.tbtForward.Name = "tbtForward";
			this.tbtForward.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.tbtForward.Size = new System.Drawing.Size(30, 28);
			this.tbtForward.Text = "Forward";
			//
			//tbtFindNew
			//
			this.tbtFindNew.AutoToolTip = false;
			this.tbtFindNew.Checked = true;
			this.tbtFindNew.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tbtFindNew.Image = Properties.Resources.views_find_new;
			this.tbtFindNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtFindNew.Name = "tbtFindNew";
			this.tbtFindNew.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.tbtFindNew.Size = new System.Drawing.Size(128, 28);
			this.tbtFindNew.Text = "&Find Programme";
			//
			//tbtFavourites
			//
			this.tbtFavourites.AutoToolTip = false;
			this.tbtFavourites.Image = Properties.Resources.views_favourites;
			this.tbtFavourites.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtFavourites.Name = "tbtFavourites";
			this.tbtFavourites.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.tbtFavourites.Size = new System.Drawing.Size(93, 28);
			this.tbtFavourites.Text = "F&avourites";
			//
			//tbtSubscriptions
			//
			this.tbtSubscriptions.AutoToolTip = false;
			this.tbtSubscriptions.Image = Properties.Resources.views_subscriptions;
			this.tbtSubscriptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtSubscriptions.Name = "tbtSubscriptions";
			this.tbtSubscriptions.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.tbtSubscriptions.Size = new System.Drawing.Size(110, 28);
			this.tbtSubscriptions.Text = "&Subscriptions";
			//
			//tbtDownloads
			//
			this.tbtDownloads.AutoToolTip = false;
			this.tbtDownloads.Image = Properties.Resources.views_downloads;
			this.tbtDownloads.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tbtDownloads.Name = "tbtDownloads";
			this.tbtDownloads.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.tbtDownloads.Size = new System.Drawing.Size(98, 28);
			this.tbtDownloads.Text = "&Downloads";
			//
			//ttxSearch
			//
			this.ttxSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ttxSearch.BackColor = System.Drawing.Color.Transparent;
			this.ttxSearch.ControlAlign = System.Drawing.ContentAlignment.TopRight;
			this.ttxSearch.CueBanner = "Search downloads";
			this.ttxSearch.Margin = new System.Windows.Forms.Padding(1, 1, 0, 0);
			this.ttxSearch.Name = "ttxSearch";
			this.ttxSearch.Size = new System.Drawing.Size(160, 30);
			//
			//nicTrayIcon
			//
			this.nicTrayIcon.ContextMenu = this.mnuTray;
			//
			//mnuTray
			//
			this.mnuTray.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuTrayShow,
				this.mnuTraySep,
				this.mnuTrayExit
			});
			//
			//mnuTrayShow
			//
			this.mnuTrayShow.Index = 0;
			this.mnuTrayShow.Text = "&Show Radio Downloader";
			//
			//mnuTraySep
			//
			this.mnuTraySep.Index = 1;
			this.mnuTraySep.Text = "-";
			//
			//mnuTrayExit
			//
			this.mnuTrayExit.Index = 2;
			this.mnuTrayExit.Text = "E&xit";
			//
			//imlListIcons
			//
			this.imlListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imlListIcons.ImageSize = new System.Drawing.Size(16, 16);
			this.imlListIcons.TransparentColor = System.Drawing.Color.Transparent;
			//
			//imlProviders
			//
			this.imlProviders.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imlProviders.ImageSize = new System.Drawing.Size(64, 64);
			this.imlProviders.TransparentColor = System.Drawing.Color.Transparent;
			//
			//prgDldProg
			//
			this.prgDldProg.Location = new System.Drawing.Point(438, 356);
			this.prgDldProg.Name = "prgDldProg";
			this.prgDldProg.Size = new System.Drawing.Size(100, 23);
			this.prgDldProg.TabIndex = 6;
			this.prgDldProg.Visible = false;
			//
			//tmrCheckForUpdates
			//
			this.tmrCheckForUpdates.Interval = 3600000;
			//
			//mnuOptions
			//
			this.mnuOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuOptionsShowOpts,
				this.mnuOptionsProviderOpts,
				this.mnuOptionsSep,
				this.mnuOptionsExit
			});
			//
			//mnuOptionsShowOpts
			//
			this.mnuOptionsShowOpts.Index = 0;
			this.mnuOptionsShowOpts.Text = "Main &Options";
			//
			//mnuOptionsProviderOpts
			//
			this.mnuOptionsProviderOpts.Index = 1;
			this.mnuOptionsProviderOpts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this.mnuOptionsProviderOptsNoProvs });
			this.mnuOptionsProviderOpts.Text = "&Provider Options";
			//
			//mnuOptionsProviderOptsNoProvs
			//
			this.mnuOptionsProviderOptsNoProvs.Enabled = false;
			this.mnuOptionsProviderOptsNoProvs.Index = 0;
			this.mnuOptionsProviderOptsNoProvs.Text = "No providers";
			//
			//mnuOptionsSep
			//
			this.mnuOptionsSep.Index = 2;
			this.mnuOptionsSep.Text = "-";
			//
			//mnuOptionsExit
			//
			this.mnuOptionsExit.Index = 3;
			this.mnuOptionsExit.Text = "E&xit";
			//
			//mnuHelp
			//
			this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuHelpShowHelp,
				this.mnuHelpReportBug,
				this.mnuHelpSep,
				this.mnuHelpAbout
			});
			//
			//mnuHelpShowHelp
			//
			this.mnuHelpShowHelp.Index = 0;
			this.mnuHelpShowHelp.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.mnuHelpShowHelp.Text = "&Help Contents";
			//
			//mnuHelpReportBug
			//
			this.mnuHelpReportBug.Index = 1;
			this.mnuHelpReportBug.Text = "Report a &Bug";
			//
			//mnuHelpSep
			//
			this.mnuHelpSep.Index = 2;
			this.mnuHelpSep.Text = "-";
			//
			//mnuHelpAbout
			//
			this.mnuHelpAbout.Index = 3;
			this.mnuHelpAbout.Text = "&About";
			//
			//imlToolbar
			//
			this.imlToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imlToolbar.ImageSize = new System.Drawing.Size(16, 16);
			this.imlToolbar.TransparentColor = System.Drawing.Color.Transparent;
			//
			//tblInfo
			//
			this.tblInfo.BackColor = System.Drawing.SystemColors.Window;
			this.tblInfo.ColumnCount = 1;
			this.tblInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this.tblInfo.Controls.Add(this.txtSideDescript, 0, 2);
			this.tblInfo.Controls.Add(this.lblSideMainTitle, 0, 0);
			this.tblInfo.Controls.Add(this.picSidebarImg, 0, 1);
			this.tblInfo.Dock = System.Windows.Forms.DockStyle.Left;
			this.tblInfo.Location = new System.Drawing.Point(0, 65);
			this.tblInfo.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.tblInfo.Name = "tblInfo";
			this.tblInfo.RowCount = 4;
			this.tblInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this.tblInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tblInfo.Size = new System.Drawing.Size(187, 385);
			this.tblInfo.TabIndex = 9;
			//
			//txtSideDescript
			//
			this.txtSideDescript.BackColor = System.Drawing.SystemColors.Window;
			this.txtSideDescript.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtSideDescript.Cursor = System.Windows.Forms.Cursors.Default;
			this.txtSideDescript.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSideDescript.Location = new System.Drawing.Point(8, 120);
			this.txtSideDescript.Margin = new System.Windows.Forms.Padding(8, 5, 8, 6);
			this.txtSideDescript.Name = "txtSideDescript";
			this.txtSideDescript.ReadOnly = true;
			this.txtSideDescript.ShortcutsEnabled = false;
			this.txtSideDescript.Size = new System.Drawing.Size(171, 259);
			this.txtSideDescript.TabIndex = 1;
			this.txtSideDescript.TabStop = false;
			this.txtSideDescript.Text = "Description";
			//
			//lblSideMainTitle
			//
			this.lblSideMainTitle.AutoSize = true;
			this.lblSideMainTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblSideMainTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblSideMainTitle.Font = new System.Drawing.Font("Tahoma", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			this.lblSideMainTitle.ForeColor = System.Drawing.SystemColors.WindowText;
			this.lblSideMainTitle.Location = new System.Drawing.Point(8, 10);
			this.lblSideMainTitle.Margin = new System.Windows.Forms.Padding(8, 10, 8, 8);
			this.lblSideMainTitle.Name = "lblSideMainTitle";
			this.lblSideMainTitle.Size = new System.Drawing.Size(171, 19);
			this.lblSideMainTitle.TabIndex = 0;
			this.lblSideMainTitle.Text = "Title";
			this.lblSideMainTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblSideMainTitle.UseMnemonic = false;
			//
			//picSidebarImg
			//
			this.picSidebarImg.ErrorImage = null;
			this.picSidebarImg.InitialImage = null;
			this.picSidebarImg.Location = new System.Drawing.Point(12, 40);
			this.picSidebarImg.Margin = new System.Windows.Forms.Padding(12, 3, 12, 5);
			this.picSidebarImg.MaximumSize = new System.Drawing.Size(160, 100);
			this.picSidebarImg.Name = "picSidebarImg";
			this.picSidebarImg.Size = new System.Drawing.Size(70, 70);
			this.picSidebarImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picSidebarImg.TabIndex = 1;
			this.picSidebarImg.TabStop = false;
			//
			//pnlPluginSpace
			//
			this.pnlPluginSpace.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.pnlPluginSpace.Location = new System.Drawing.Point(197, 118);
			this.pnlPluginSpace.Name = "pnlPluginSpace";
			this.pnlPluginSpace.Size = new System.Drawing.Size(560, 41);
			this.pnlPluginSpace.TabIndex = 1;
			//
			//tblToolbars
			//
			this.tblToolbars.ColumnCount = 2;
			this.tblToolbars.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90f));
			this.tblToolbars.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10f));
			this.tblToolbars.Controls.Add(this.tbrToolbar, 0, 0);
			this.tblToolbars.Controls.Add(this.tbrHelp, 1, 0);
			this.tblToolbars.Dock = System.Windows.Forms.DockStyle.Top;
			this.tblToolbars.Location = new System.Drawing.Point(0, 31);
			this.tblToolbars.Name = "tblToolbars";
			this.tblToolbars.RowCount = 1;
			this.tblToolbars.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
			this.tblToolbars.Size = new System.Drawing.Size(757, 34);
			this.tblToolbars.TabIndex = 8;
			//
			//tbrToolbar
			//
			this.tbrToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tbrToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
				this.tbtOptionsMenu,
				this.tbtChooseProgramme,
				this.tbtDownload,
				this.tbtAddFavourite,
				this.tbtRemFavourite,
				this.tbtSubscribe,
				this.tbtUnsubscribe,
				this.tbtCurrentEps,
				this.tbtCancel,
				this.tbtPlay,
				this.tbtDelete,
				this.tbtRetry,
				this.tbtReportError,
				this.tbtCleanUp
			});
			this.tbrToolbar.ButtonSize = new System.Drawing.Size(135, 22);
			this.tbrToolbar.Divider = false;
			this.tbrToolbar.DropDownArrows = true;
			this.tbrToolbar.Location = new System.Drawing.Point(3, 2);
			this.tbrToolbar.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.tbrToolbar.Name = "tbrToolbar";
			this.tbrToolbar.ShowToolTips = true;
			this.tbrToolbar.Size = new System.Drawing.Size(678, 26);
			this.tbrToolbar.TabIndex = 0;
			this.tbrToolbar.TabStop = true;
			this.tbrToolbar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.tbrToolbar.Wrappable = false;
			//
			//tbtOptionsMenu
			//
			this.tbtOptionsMenu.DropDownMenu = this.mnuOptions;
			this.tbtOptionsMenu.ImageKey = "options";
			this.tbtOptionsMenu.Name = "tbtOptionsMenu";
			this.tbtOptionsMenu.Text = "&Options";
			//
			//tbtChooseProgramme
			//
			this.tbtChooseProgramme.ImageKey = "choose_programme";
			this.tbtChooseProgramme.Name = "tbtChooseProgramme";
			this.tbtChooseProgramme.Text = "&Choose Programme";
			//
			//tbtDownload
			//
			this.tbtDownload.ImageKey = "download";
			this.tbtDownload.Name = "tbtDownload";
			this.tbtDownload.Text = "Do&wnload";
			//
			//tbtAddFavourite
			//
			this.tbtAddFavourite.ImageKey = "add_favourite";
			this.tbtAddFavourite.Name = "tbtAddFavourite";
			this.tbtAddFavourite.Text = "Add &Favourite";
			//
			//tbtRemFavourite
			//
			this.tbtRemFavourite.ImageKey = "remove_favourite";
			this.tbtRemFavourite.Name = "tbtRemFavourite";
			this.tbtRemFavourite.Text = "Remove &Favourite";
			//
			//tbtSubscribe
			//
			this.tbtSubscribe.ImageKey = "subscribe";
			this.tbtSubscribe.Name = "tbtSubscribe";
			this.tbtSubscribe.Text = "S&ubscribe";
			//
			//tbtUnsubscribe
			//
			this.tbtUnsubscribe.ImageKey = "unsubscribe";
			this.tbtUnsubscribe.Name = "tbtUnsubscribe";
			this.tbtUnsubscribe.Text = "&Unsubscribe";
			//
			//tbtCurrentEps
			//
			this.tbtCurrentEps.ImageKey = "current_episodes";
			this.tbtCurrentEps.Name = "tbtCurrentEps";
			this.tbtCurrentEps.Text = "&Current Episodes";
			//
			//tbtCancel
			//
			this.tbtCancel.ImageKey = "delete";
			this.tbtCancel.Name = "tbtCancel";
			this.tbtCancel.Text = "&Cancel";
			//
			//tbtPlay
			//
			this.tbtPlay.ImageKey = "play";
			this.tbtPlay.Name = "tbtPlay";
			this.tbtPlay.Text = "&Play";
			//
			//tbtDelete
			//
			this.tbtDelete.ImageKey = "delete";
			this.tbtDelete.Name = "tbtDelete";
			this.tbtDelete.Text = "D&elete";
			//
			//tbtRetry
			//
			this.tbtRetry.ImageKey = "retry";
			this.tbtRetry.Name = "tbtRetry";
			this.tbtRetry.Text = "&Retry";
			//
			//tbtReportError
			//
			this.tbtReportError.ImageKey = "report_error";
			this.tbtReportError.Name = "tbtReportError";
			this.tbtReportError.Text = "Report &Error";
			//
			//tbtCleanUp
			//
			this.tbtCleanUp.ImageKey = "clean_up";
			this.tbtCleanUp.Name = "tbtCleanUp";
			this.tbtCleanUp.Text = "Clean &Up";
			//
			//tbrHelp
			//
			this.tbrHelp.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.tbrHelp.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tbrHelp.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] { this.tbtHelpMenu });
			this.tbrHelp.Divider = false;
			this.tbrHelp.Dock = System.Windows.Forms.DockStyle.None;
			this.tbrHelp.DropDownArrows = true;
			this.tbrHelp.Location = new System.Drawing.Point(681, 2);
			this.tbrHelp.Margin = new System.Windows.Forms.Padding(0, 2, 3, 0);
			this.tbrHelp.Name = "tbrHelp";
			this.tbrHelp.ShowToolTips = true;
			this.tbrHelp.Size = new System.Drawing.Size(73, 26);
			this.tbrHelp.TabIndex = 1;
			this.tbrHelp.TabStop = true;
			this.tbrHelp.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.tbrHelp.Wrappable = false;
			//
			//tbtHelpMenu
			//
			this.tbtHelpMenu.DropDownMenu = this.mnuHelp;
			this.tbtHelpMenu.ImageKey = "help";
			this.tbtHelpMenu.Name = "tbtHelpMenu";
			this.tbtHelpMenu.Text = "&Help";
			//
			//picSideBarBorder
			//
			this.picSideBarBorder.BackColor = System.Drawing.SystemColors.Window;
			this.picSideBarBorder.Dock = System.Windows.Forms.DockStyle.Left;
			this.picSideBarBorder.Location = new System.Drawing.Point(187, 65);
			this.picSideBarBorder.Margin = new System.Windows.Forms.Padding(0);
			this.picSideBarBorder.Name = "picSideBarBorder";
			this.picSideBarBorder.Size = new System.Drawing.Size(10, 385);
			this.picSideBarBorder.TabIndex = 22;
			this.picSideBarBorder.TabStop = false;
			//
			//mnuListHdrs
			//
			this.mnuListHdrs.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuListHdrsColumns,
				this.mnuListHdrsSep,
				this.mnuListHdrsReset
			});
			//
			//mnuListHdrsColumns
			//
			this.mnuListHdrsColumns.Index = 0;
			this.mnuListHdrsColumns.Text = "Choose Columns...";
			//
			//mnuListHdrsSep
			//
			this.mnuListHdrsSep.Index = 1;
			this.mnuListHdrsSep.Text = "-";
			//
			//mnuListHdrsReset
			//
			this.mnuListHdrsReset.Index = 2;
			this.mnuListHdrsReset.Text = "Reset";
			//
			//lstDownloads
			//
			this.lstDownloads.AllowColumnReorder = true;
			this.lstDownloads.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstDownloads.FullRowSelect = true;
			this.lstDownloads.HideSelection = false;
			this.lstDownloads.Location = new System.Drawing.Point(197, 396);
			this.lstDownloads.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
			this.lstDownloads.MultiSelect = false;
			this.lstDownloads.Name = "lstDownloads";
			this.lstDownloads.Size = new System.Drawing.Size(560, 54);
			this.lstDownloads.TabIndex = 5;
			this.lstDownloads.UseCompatibleStateImageBehavior = false;
			this.lstDownloads.View = System.Windows.Forms.View.Details;
			//
			//lstSubscribed
			//
			this.lstSubscribed.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstSubscribed.FullRowSelect = true;
			this.lstSubscribed.HideSelection = false;
			this.lstSubscribed.Location = new System.Drawing.Point(197, 301);
			this.lstSubscribed.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.lstSubscribed.MultiSelect = false;
			this.lstSubscribed.Name = "lstSubscribed";
			this.lstSubscribed.Size = new System.Drawing.Size(560, 49);
			this.lstSubscribed.TabIndex = 4;
			this.lstSubscribed.UseCompatibleStateImageBehavior = false;
			this.lstSubscribed.View = System.Windows.Forms.View.Details;
			//
			//lstEpisodes
			//
			this.lstEpisodes.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstEpisodes.CheckBoxes = true;
			this.lstEpisodes.FullRowSelect = true;
			this.lstEpisodes.HideSelection = false;
			this.lstEpisodes.Location = new System.Drawing.Point(197, 171);
			this.lstEpisodes.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
			this.lstEpisodes.MultiSelect = false;
			this.lstEpisodes.Name = "lstEpisodes";
			this.lstEpisodes.Size = new System.Drawing.Size(560, 50);
			this.lstEpisodes.TabIndex = 2;
			this.lstEpisodes.UseCompatibleStateImageBehavior = false;
			this.lstEpisodes.View = System.Windows.Forms.View.Details;
			//
			//lstProviders
			//
			this.lstProviders.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstProviders.HideSelection = false;
			this.lstProviders.Location = new System.Drawing.Point(197, 63);
			this.lstProviders.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
			this.lstProviders.MultiSelect = false;
			this.lstProviders.Name = "lstProviders";
			this.lstProviders.Size = new System.Drawing.Size(560, 49);
			this.lstProviders.TabIndex = 0;
			this.lstProviders.UseCompatibleStateImageBehavior = false;
			//
			//lstFavourites
			//
			this.lstFavourites.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lstFavourites.FullRowSelect = true;
			this.lstFavourites.HideSelection = false;
			this.lstFavourites.Location = new System.Drawing.Point(197, 236);
			this.lstFavourites.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
			this.lstFavourites.MultiSelect = false;
			this.lstFavourites.Name = "lstFavourites";
			this.lstFavourites.Size = new System.Drawing.Size(560, 50);
			this.lstFavourites.TabIndex = 3;
			this.lstFavourites.UseCompatibleStateImageBehavior = false;
			this.lstFavourites.View = System.Windows.Forms.View.Details;
			//
			//Main
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(757, 450);
			this.Controls.Add(this.lstFavourites);
			this.Controls.Add(this.lstDownloads);
			this.Controls.Add(this.lstSubscribed);
			this.Controls.Add(this.lstEpisodes);
			this.Controls.Add(this.pnlPluginSpace);
			this.Controls.Add(this.lstProviders);
			this.Controls.Add(this.picSideBarBorder);
			this.Controls.Add(this.prgDldProg);
			this.Controls.Add(this.tblInfo);
			this.Controls.Add(this.tblToolbars);
			this.Controls.Add(this.tbrView);
			this.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			this.Icon = Properties.Resources.icon_main;
			this.KeyPreview = true;
			this.Location = new System.Drawing.Point(11, 37);
			this.MinimumSize = new System.Drawing.Size(550, 300);
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Radio Downloader";
			this.tbrView.ResumeLayout(false);
			this.tbrView.PerformLayout();
			this.tblInfo.ResumeLayout(false);
			this.tblInfo.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.picSidebarImg).EndInit();
			this.tblToolbars.ResumeLayout(false);
			this.tblToolbars.PerformLayout();
			((System.ComponentModel.ISupportInitialize)this.picSideBarBorder).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ExtToolStrip tbrView;
		private System.Windows.Forms.ToolStripButton withEventsField_tbtFindNew;
		internal System.Windows.Forms.ToolStripButton tbtFindNew {
			get { return withEventsField_tbtFindNew; }
			set {
				if (withEventsField_tbtFindNew != null) {
					withEventsField_tbtFindNew.Click -= tbtFindNew_Click;
				}
				withEventsField_tbtFindNew = value;
				if (withEventsField_tbtFindNew != null) {
					withEventsField_tbtFindNew.Click += tbtFindNew_Click;
				}
			}
		}
		private System.Windows.Forms.ToolStripButton withEventsField_tbtSubscriptions;
		internal System.Windows.Forms.ToolStripButton tbtSubscriptions {
			get { return withEventsField_tbtSubscriptions; }
			set {
				if (withEventsField_tbtSubscriptions != null) {
					withEventsField_tbtSubscriptions.Click -= tbtSubscriptions_Click;
				}
				withEventsField_tbtSubscriptions = value;
				if (withEventsField_tbtSubscriptions != null) {
					withEventsField_tbtSubscriptions.Click += tbtSubscriptions_Click;
				}
			}
		}
		private System.Windows.Forms.ToolStripButton withEventsField_tbtDownloads;
		internal System.Windows.Forms.ToolStripButton tbtDownloads {
			get { return withEventsField_tbtDownloads; }
			set {
				if (withEventsField_tbtDownloads != null) {
					withEventsField_tbtDownloads.Click -= tbtDownloads_Click;
				}
				withEventsField_tbtDownloads = value;
				if (withEventsField_tbtDownloads != null) {
					withEventsField_tbtDownloads.Click += tbtDownloads_Click;
				}
			}
		}
		internal System.Windows.Forms.ToolBarButton tbtCleanUp;
		private System.Windows.Forms.NotifyIcon withEventsField_nicTrayIcon;
		internal System.Windows.Forms.NotifyIcon nicTrayIcon {
			get { return withEventsField_nicTrayIcon; }
			set {
				if (withEventsField_nicTrayIcon != null) {
					withEventsField_nicTrayIcon.BalloonTipClicked -= nicTrayIcon_BalloonTipClicked;
					withEventsField_nicTrayIcon.MouseDoubleClick -= nicTrayIcon_MouseDoubleClick;
				}
				withEventsField_nicTrayIcon = value;
				if (withEventsField_nicTrayIcon != null) {
					withEventsField_nicTrayIcon.BalloonTipClicked += nicTrayIcon_BalloonTipClicked;
					withEventsField_nicTrayIcon.MouseDoubleClick += nicTrayIcon_MouseDoubleClick;
				}
			}
		}
		internal System.Windows.Forms.ContextMenu mnuTray;
		private System.Windows.Forms.MenuItem withEventsField_mnuTrayShow;
		internal System.Windows.Forms.MenuItem mnuTrayShow {
			get { return withEventsField_mnuTrayShow; }
			set {
				if (withEventsField_mnuTrayShow != null) {
					withEventsField_mnuTrayShow.Click -= mnuTrayShow_Click;
				}
				withEventsField_mnuTrayShow = value;
				if (withEventsField_mnuTrayShow != null) {
					withEventsField_mnuTrayShow.Click += mnuTrayShow_Click;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuTraySep;
		private System.Windows.Forms.MenuItem withEventsField_mnuTrayExit;
		internal System.Windows.Forms.MenuItem mnuTrayExit {
			get { return withEventsField_mnuTrayExit; }
			set {
				if (withEventsField_mnuTrayExit != null) {
					withEventsField_mnuTrayExit.Click -= mnuTrayExit_Click;
				}
				withEventsField_mnuTrayExit = value;
				if (withEventsField_mnuTrayExit != null) {
					withEventsField_mnuTrayExit.Click += mnuTrayExit_Click;
				}
			}
		}
		private ToolStripSearchBox withEventsField_ttxSearch;
		internal ToolStripSearchBox ttxSearch {
			get { return withEventsField_ttxSearch; }
			set {
				if (withEventsField_ttxSearch != null) {
					withEventsField_ttxSearch.TextChanged -= ttxSearch_TextChanged;
				}
				withEventsField_ttxSearch = value;
				if (withEventsField_ttxSearch != null) {
					withEventsField_ttxSearch.TextChanged += ttxSearch_TextChanged;
				}
			}
		}
		private RadioDld.ExtListView withEventsField_lstProviders;
		internal RadioDld.ExtListView lstProviders {
			get { return withEventsField_lstProviders; }
			set {
				if (withEventsField_lstProviders != null) {
					withEventsField_lstProviders.SelectedIndexChanged -= lstProviders_SelectedIndexChanged;
					withEventsField_lstProviders.ItemActivate -= lstProviders_ItemActivate;
				}
				withEventsField_lstProviders = value;
				if (withEventsField_lstProviders != null) {
					withEventsField_lstProviders.SelectedIndexChanged += lstProviders_SelectedIndexChanged;
					withEventsField_lstProviders.ItemActivate += lstProviders_ItemActivate;
				}
			}
		}
		internal System.Windows.Forms.ImageList imlListIcons;
		internal System.Windows.Forms.ImageList imlProviders;
		private RadioDld.ExtListView withEventsField_lstSubscribed;
		internal RadioDld.ExtListView lstSubscribed {
			get { return withEventsField_lstSubscribed; }
			set {
				if (withEventsField_lstSubscribed != null) {
					withEventsField_lstSubscribed.SelectedIndexChanged -= lstSubscribed_SelectedIndexChanged;
				}
				withEventsField_lstSubscribed = value;
				if (withEventsField_lstSubscribed != null) {
					withEventsField_lstSubscribed.SelectedIndexChanged += lstSubscribed_SelectedIndexChanged;
				}
			}
		}
		internal System.Windows.Forms.ProgressBar prgDldProg;
		private RadioDld.ExtListView withEventsField_lstDownloads;
		internal RadioDld.ExtListView lstDownloads {
			get { return withEventsField_lstDownloads; }
			set {
				if (withEventsField_lstDownloads != null) {
					withEventsField_lstDownloads.ColumnClick -= lstDownloads_ColumnClick;
					withEventsField_lstDownloads.ColumnReordered -= lstDownloads_ColumnReordered;
					withEventsField_lstDownloads.ColumnRightClick -= lstDownloads_ColumnRightClick;
					withEventsField_lstDownloads.ColumnWidthChanged -= lstDownloads_ColumnWidthChanged;
					withEventsField_lstDownloads.ItemActivate -= lstDownloads_ItemActivate;
					withEventsField_lstDownloads.SelectedIndexChanged -= lstDownloads_SelectedIndexChanged;
				}
				withEventsField_lstDownloads = value;
				if (withEventsField_lstDownloads != null) {
					withEventsField_lstDownloads.ColumnClick += lstDownloads_ColumnClick;
					withEventsField_lstDownloads.ColumnReordered += lstDownloads_ColumnReordered;
					withEventsField_lstDownloads.ColumnRightClick += lstDownloads_ColumnRightClick;
					withEventsField_lstDownloads.ColumnWidthChanged += lstDownloads_ColumnWidthChanged;
					withEventsField_lstDownloads.ItemActivate += lstDownloads_ItemActivate;
					withEventsField_lstDownloads.SelectedIndexChanged += lstDownloads_SelectedIndexChanged;
				}
			}
		}
		private System.Windows.Forms.Timer withEventsField_tmrCheckForUpdates;
		internal System.Windows.Forms.Timer tmrCheckForUpdates {
			get { return withEventsField_tmrCheckForUpdates; }
			set {
				if (withEventsField_tmrCheckForUpdates != null) {
					withEventsField_tmrCheckForUpdates.Tick -= tmrCheckForUpdates_Tick;
				}
				withEventsField_tmrCheckForUpdates = value;
				if (withEventsField_tmrCheckForUpdates != null) {
					withEventsField_tmrCheckForUpdates.Tick += tmrCheckForUpdates_Tick;
				}
			}
		}
		private ExtToolBar withEventsField_tbrToolbar;
		internal ExtToolBar tbrToolbar {
			get { return withEventsField_tbrToolbar; }
			set {
				if (withEventsField_tbrToolbar != null) {
					withEventsField_tbrToolbar.ButtonClick -= tbrToolbar_ButtonClick;
				}
				withEventsField_tbrToolbar = value;
				if (withEventsField_tbrToolbar != null) {
					withEventsField_tbrToolbar.ButtonClick += tbrToolbar_ButtonClick;
				}
			}
		}
		internal System.Windows.Forms.TableLayoutPanel tblInfo;
		internal System.Windows.Forms.PictureBox picSidebarImg;
		internal System.Windows.Forms.ToolBarButton tbtPlay;
		internal System.Windows.Forms.ToolBarButton tbtCancel;
		internal System.Windows.Forms.ToolBarButton tbtSubscribe;
		internal System.Windows.Forms.ToolBarButton tbtUnsubscribe;
		internal System.Windows.Forms.ToolBarButton tbtDownload;
		internal System.Windows.Forms.ToolBarButton tbtDelete;
		internal System.Windows.Forms.ToolBarButton tbtRetry;
		private RadioDld.ExtListView withEventsField_lstEpisodes;
		internal RadioDld.ExtListView lstEpisodes {
			get { return withEventsField_lstEpisodes; }
			set {
				if (withEventsField_lstEpisodes != null) {
					withEventsField_lstEpisodes.ItemCheck -= lstEpisodes_ItemCheck;
					withEventsField_lstEpisodes.SelectedIndexChanged -= lstEpisodes_SelectedIndexChanged;
				}
				withEventsField_lstEpisodes = value;
				if (withEventsField_lstEpisodes != null) {
					withEventsField_lstEpisodes.ItemCheck += lstEpisodes_ItemCheck;
					withEventsField_lstEpisodes.SelectedIndexChanged += lstEpisodes_SelectedIndexChanged;
				}
			}
		}
		internal System.Windows.Forms.ToolBarButton tbtMisc;
		private System.Windows.Forms.MenuItem withEventsField_mnuOptionsShowOpts;
		internal System.Windows.Forms.MenuItem mnuOptionsShowOpts {
			get { return withEventsField_mnuOptionsShowOpts; }
			set {
				if (withEventsField_mnuOptionsShowOpts != null) {
					withEventsField_mnuOptionsShowOpts.Click -= mnuOptionsShowOpts_Click;
				}
				withEventsField_mnuOptionsShowOpts = value;
				if (withEventsField_mnuOptionsShowOpts != null) {
					withEventsField_mnuOptionsShowOpts.Click += mnuOptionsShowOpts_Click;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuOptionsSep;
		private System.Windows.Forms.MenuItem withEventsField_mnuOptionsExit;
		internal System.Windows.Forms.MenuItem mnuOptionsExit {
			get { return withEventsField_mnuOptionsExit; }
			set {
				if (withEventsField_mnuOptionsExit != null) {
					withEventsField_mnuOptionsExit.Click -= mnuOptionsExit_Click;
				}
				withEventsField_mnuOptionsExit = value;
				if (withEventsField_mnuOptionsExit != null) {
					withEventsField_mnuOptionsExit.Click += mnuOptionsExit_Click;
				}
			}
		}
		private System.Windows.Forms.MenuItem withEventsField_mnuHelpShowHelp;
		internal System.Windows.Forms.MenuItem mnuHelpShowHelp {
			get { return withEventsField_mnuHelpShowHelp; }
			set {
				if (withEventsField_mnuHelpShowHelp != null) {
					withEventsField_mnuHelpShowHelp.Click -= mnuHelpShowHelp_Click;
				}
				withEventsField_mnuHelpShowHelp = value;
				if (withEventsField_mnuHelpShowHelp != null) {
					withEventsField_mnuHelpShowHelp.Click += mnuHelpShowHelp_Click;
				}
			}
		}
		private System.Windows.Forms.MenuItem withEventsField_mnuHelpAbout;
		internal System.Windows.Forms.MenuItem mnuHelpAbout {
			get { return withEventsField_mnuHelpAbout; }
			set {
				if (withEventsField_mnuHelpAbout != null) {
					withEventsField_mnuHelpAbout.Click -= mnuHelpAbout_Click;
				}
				withEventsField_mnuHelpAbout = value;
				if (withEventsField_mnuHelpAbout != null) {
					withEventsField_mnuHelpAbout.Click += mnuHelpAbout_Click;
				}
			}
		}
		private System.Windows.Forms.MenuItem withEventsField_mnuHelpReportBug;
		internal System.Windows.Forms.MenuItem mnuHelpReportBug {
			get { return withEventsField_mnuHelpReportBug; }
			set {
				if (withEventsField_mnuHelpReportBug != null) {
					withEventsField_mnuHelpReportBug.Click -= mnuHelpReportBug_Click;
				}
				withEventsField_mnuHelpReportBug = value;
				if (withEventsField_mnuHelpReportBug != null) {
					withEventsField_mnuHelpReportBug.Click += mnuHelpReportBug_Click;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuHelpSep;
		internal System.Windows.Forms.ToolBarButton tbtReportError;
		internal System.Windows.Forms.Panel pnlPluginSpace;
		private System.Windows.Forms.ToolStripButton withEventsField_tbtFavourites;
		internal System.Windows.Forms.ToolStripButton tbtFavourites {
			get { return withEventsField_tbtFavourites; }
			set {
				if (withEventsField_tbtFavourites != null) {
					withEventsField_tbtFavourites.Click -= tbtFavourites_Click;
				}
				withEventsField_tbtFavourites = value;
				if (withEventsField_tbtFavourites != null) {
					withEventsField_tbtFavourites.Click += tbtFavourites_Click;
				}
			}
		}
		private System.Windows.Forms.ToolStripButton withEventsField_tbtBack;
		internal System.Windows.Forms.ToolStripButton tbtBack {
			get { return withEventsField_tbtBack; }
			set {
				if (withEventsField_tbtBack != null) {
					withEventsField_tbtBack.Click -= tbtBack_Click;
				}
				withEventsField_tbtBack = value;
				if (withEventsField_tbtBack != null) {
					withEventsField_tbtBack.Click += tbtBack_Click;
				}
			}
		}
		private System.Windows.Forms.ToolStripButton withEventsField_tbtForward;
		internal System.Windows.Forms.ToolStripButton tbtForward {
			get { return withEventsField_tbtForward; }
			set {
				if (withEventsField_tbtForward != null) {
					withEventsField_tbtForward.Click -= tbtForward_Click;
				}
				withEventsField_tbtForward = value;
				if (withEventsField_tbtForward != null) {
					withEventsField_tbtForward.Click += tbtForward_Click;
				}
			}
		}
		internal System.Windows.Forms.ToolBarButton tbtCurrentEps;
		internal System.Windows.Forms.ImageList imlToolbar;
		internal System.Windows.Forms.ContextMenu mnuOptions;
		internal System.Windows.Forms.ContextMenu mnuHelp;
		internal System.Windows.Forms.ToolBarButton tbtOptionsMenu;
		internal System.Windows.Forms.ToolBarButton tbtHelpMenu;
		private System.Windows.Forms.TableLayoutPanel withEventsField_tblToolbars;
		internal System.Windows.Forms.TableLayoutPanel tblToolbars {
			get { return withEventsField_tblToolbars; }
			set {
				if (withEventsField_tblToolbars != null) {
					withEventsField_tblToolbars.Resize -= tblToolbars_Resize;
				}
				withEventsField_tblToolbars = value;
				if (withEventsField_tblToolbars != null) {
					withEventsField_tblToolbars.Resize += tblToolbars_Resize;
				}
			}
		}
		internal ExtToolBar tbrHelp;
		internal System.Windows.Forms.Label lblSideMainTitle;
		private System.Windows.Forms.RichTextBox withEventsField_txtSideDescript;
		internal System.Windows.Forms.RichTextBox txtSideDescript {
			get { return withEventsField_txtSideDescript; }
			set {
				if (withEventsField_txtSideDescript != null) {
					withEventsField_txtSideDescript.GotFocus -= txtSideDescript_GotFocus;
					withEventsField_txtSideDescript.LinkClicked -= txtSideDescript_LinkClicked;
					withEventsField_txtSideDescript.Resize -= txtSideDescript_Resize;
				}
				withEventsField_txtSideDescript = value;
				if (withEventsField_txtSideDescript != null) {
					withEventsField_txtSideDescript.GotFocus += txtSideDescript_GotFocus;
					withEventsField_txtSideDescript.LinkClicked += txtSideDescript_LinkClicked;
					withEventsField_txtSideDescript.Resize += txtSideDescript_Resize;
				}
			}
		}
		private System.Windows.Forms.PictureBox withEventsField_picSideBarBorder;
		internal System.Windows.Forms.PictureBox picSideBarBorder {
			get { return withEventsField_picSideBarBorder; }
			set {
				if (withEventsField_picSideBarBorder != null) {
					withEventsField_picSideBarBorder.Paint -= picSideBarBorder_Paint;
				}
				withEventsField_picSideBarBorder = value;
				if (withEventsField_picSideBarBorder != null) {
					withEventsField_picSideBarBorder.Paint += picSideBarBorder_Paint;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuOptionsProviderOpts;
		internal System.Windows.Forms.ToolBarButton tbtChooseProgramme;
		internal System.Windows.Forms.MenuItem mnuOptionsProviderOptsNoProvs;
		internal System.Windows.Forms.ContextMenu mnuListHdrs;
		private System.Windows.Forms.MenuItem withEventsField_mnuListHdrsColumns;
		internal System.Windows.Forms.MenuItem mnuListHdrsColumns {
			get { return withEventsField_mnuListHdrsColumns; }
			set {
				if (withEventsField_mnuListHdrsColumns != null) {
					withEventsField_mnuListHdrsColumns.Click -= mnuListHdrsColumns_Click;
				}
				withEventsField_mnuListHdrsColumns = value;
				if (withEventsField_mnuListHdrsColumns != null) {
					withEventsField_mnuListHdrsColumns.Click += mnuListHdrsColumns_Click;
				}
			}
		}
		private System.Windows.Forms.MenuItem withEventsField_mnuListHdrsReset;
		internal System.Windows.Forms.MenuItem mnuListHdrsReset {
			get { return withEventsField_mnuListHdrsReset; }
			set {
				if (withEventsField_mnuListHdrsReset != null) {
					withEventsField_mnuListHdrsReset.Click -= mnuListHdrsReset_Click;
				}
				withEventsField_mnuListHdrsReset = value;
				if (withEventsField_mnuListHdrsReset != null) {
					withEventsField_mnuListHdrsReset.Click += mnuListHdrsReset_Click;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuListHdrsSep;
		private RadioDld.ExtListView withEventsField_lstFavourites;
		internal RadioDld.ExtListView lstFavourites {
			get { return withEventsField_lstFavourites; }
			set {
				if (withEventsField_lstFavourites != null) {
					withEventsField_lstFavourites.SelectedIndexChanged -= lstFavourites_SelectedIndexChanged;
				}
				withEventsField_lstFavourites = value;
				if (withEventsField_lstFavourites != null) {
					withEventsField_lstFavourites.SelectedIndexChanged += lstFavourites_SelectedIndexChanged;
				}
			}
		}
		internal System.Windows.Forms.ToolBarButton tbtAddFavourite;
			#endregion
		internal System.Windows.Forms.ToolBarButton tbtRemFavourite;
	}
}

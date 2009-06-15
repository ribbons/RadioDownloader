<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class Main
#Region "Windows Form Designer generated code "
	<System.Diagnostics.DebuggerNonUserCode()> Public Sub New()
		MyBase.New()
		'This call is required by the Windows Form Designer.
		InitializeComponent()
	End Sub
	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
		If Disposing Then
			If Not components Is Nothing Then
				components.Dispose()
			End If
		End If
		MyBase.Dispose(Disposing)
	End Sub
	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer
    Public WithEvents tmrCheckSub As System.Windows.Forms.Timer
    Public WithEvents tmrStartProcess As System.Windows.Forms.Timer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Main))
        Me.tmrCheckSub = New System.Windows.Forms.Timer(Me.components)
        Me.tmrStartProcess = New System.Windows.Forms.Timer(Me.components)
        Me.tbrView = New System.Windows.Forms.ToolStrip
        Me.tbtBack = New System.Windows.Forms.ToolStripButton
        Me.tbtForward = New System.Windows.Forms.ToolStripButton
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.tbtFindNew = New System.Windows.Forms.ToolStripButton
        Me.tbtFavourites = New System.Windows.Forms.ToolStripButton
        Me.tbtSubscriptions = New System.Windows.Forms.ToolStripButton
        Me.tbtDownloads = New System.Windows.Forms.ToolStripButton
        Me.ttxSearch = New System.Windows.Forms.ToolStripTextBox
        Me.nicTrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.mnuTray = New System.Windows.Forms.ContextMenu
        Me.mnuTrayShow = New System.Windows.Forms.MenuItem
        Me.mnuTraySep = New System.Windows.Forms.MenuItem
        Me.mnuTrayExit = New System.Windows.Forms.MenuItem
        Me.imlListIcons = New System.Windows.Forms.ImageList(Me.components)
        Me.imlProviders = New System.Windows.Forms.ImageList(Me.components)
        Me.prgDldProg = New System.Windows.Forms.ProgressBar
        Me.tmrCheckForUpdates = New System.Windows.Forms.Timer(Me.components)
        Me.mnuOptions = New System.Windows.Forms.ContextMenu
        Me.mnuOptionsShowOpts = New System.Windows.Forms.MenuItem
        Me.mnuOptionsSep = New System.Windows.Forms.MenuItem
        Me.mnuOptionsExit = New System.Windows.Forms.MenuItem
        Me.mnuHelp = New System.Windows.Forms.ContextMenu
        Me.mnuHelpShowHelp = New System.Windows.Forms.MenuItem
        Me.mnuHelpReportBug = New System.Windows.Forms.MenuItem
        Me.mnuHelpSep = New System.Windows.Forms.MenuItem
        Me.mnuHelpAbout = New System.Windows.Forms.MenuItem
        Me.imlToolbar = New System.Windows.Forms.ImageList(Me.components)
        Me.tblInfo = New System.Windows.Forms.TableLayoutPanel
        Me.txtSideDescript = New System.Windows.Forms.TextBox
        Me.lblSideMainTitle = New System.Windows.Forms.Label
        Me.picSidebarImg = New System.Windows.Forms.PictureBox
        Me.pnlPluginSpace = New System.Windows.Forms.Panel
        Me.tblToolbars = New System.Windows.Forms.TableLayoutPanel
        Me.picSideBarBorder = New System.Windows.Forms.PictureBox
        Me.lstDownloads = New RadioDld.ExtListView
        Me.lstSubscribed = New RadioDld.ExtListView
        Me.lstEpisodes = New RadioDld.ExtListView
        Me.lstProviders = New RadioDld.ExtListView
        Me.tbrToolbar = New RadioDld.ExtToolBar
        Me.tbtOptionsMenu = New System.Windows.Forms.ToolBarButton
        Me.tbtChooseProgramme = New System.Windows.Forms.ToolBarButton
        Me.tbtDownload = New System.Windows.Forms.ToolBarButton
        Me.tbtSubscribe = New System.Windows.Forms.ToolBarButton
        Me.tbtUnsubscribe = New System.Windows.Forms.ToolBarButton
        Me.tbtCurrentEps = New System.Windows.Forms.ToolBarButton
        Me.tbtCancel = New System.Windows.Forms.ToolBarButton
        Me.tbtPlay = New System.Windows.Forms.ToolBarButton
        Me.tbtDelete = New System.Windows.Forms.ToolBarButton
        Me.tbtRetry = New System.Windows.Forms.ToolBarButton
        Me.tbtReportError = New System.Windows.Forms.ToolBarButton
        Me.tbtCleanUp = New System.Windows.Forms.ToolBarButton
        Me.tbrHelp = New RadioDld.ExtToolBar
        Me.tbtHelpMenu = New System.Windows.Forms.ToolBarButton
        Me.mnuOptionsProviderOpts = New System.Windows.Forms.MenuItem
        Me.tbrView.SuspendLayout()
        Me.tblInfo.SuspendLayout()
        CType(Me.picSidebarImg, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tblToolbars.SuspendLayout()
        CType(Me.picSideBarBorder, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tmrCheckSub
        '
        Me.tmrCheckSub.Interval = 60000
        '
        'tmrStartProcess
        '
        Me.tmrStartProcess.Interval = 2000
        '
        'tbrView
        '
        Me.tbrView.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.tbrView.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.tbrView.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tbtBack, Me.tbtForward, Me.ToolStripSeparator1, Me.tbtFindNew, Me.tbtFavourites, Me.tbtSubscriptions, Me.tbtDownloads, Me.ttxSearch})
        Me.tbrView.Location = New System.Drawing.Point(0, 0)
        Me.tbrView.Name = "tbrView"
        Me.tbrView.Padding = New System.Windows.Forms.Padding(3, 0, 1, 0)
        Me.tbrView.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.tbrView.Size = New System.Drawing.Size(757, 31)
        Me.tbrView.TabIndex = 6
        Me.tbrView.TabStop = True
        '
        'tbtBack
        '
        Me.tbtBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tbtBack.Image = Global.RadioDld.My.Resources.Resources.views_back
        Me.tbtBack.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtBack.Name = "tbtBack"
        Me.tbtBack.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtBack.Size = New System.Drawing.Size(32, 28)
        Me.tbtBack.Text = "Back"
        '
        'tbtForward
        '
        Me.tbtForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tbtForward.Image = Global.RadioDld.My.Resources.Resources.views_forward
        Me.tbtForward.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtForward.Name = "tbtForward"
        Me.tbtForward.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtForward.Size = New System.Drawing.Size(32, 28)
        Me.tbtForward.Text = "Forward"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 31)
        '
        'tbtFindNew
        '
        Me.tbtFindNew.Checked = True
        Me.tbtFindNew.CheckState = System.Windows.Forms.CheckState.Checked
        Me.tbtFindNew.Image = Global.RadioDld.My.Resources.Resources.views_find_new
        Me.tbtFindNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtFindNew.Name = "tbtFindNew"
        Me.tbtFindNew.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtFindNew.Size = New System.Drawing.Size(128, 28)
        Me.tbtFindNew.Text = "&Find Programme"
        '
        'tbtFavourites
        '
        Me.tbtFavourites.Image = CType(resources.GetObject("tbtFavourites.Image"), System.Drawing.Image)
        Me.tbtFavourites.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtFavourites.Name = "tbtFavourites"
        Me.tbtFavourites.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtFavourites.Size = New System.Drawing.Size(93, 28)
        Me.tbtFavourites.Text = "F&avourites"
        Me.tbtFavourites.Visible = False
        '
        'tbtSubscriptions
        '
        Me.tbtSubscriptions.Image = Global.RadioDld.My.Resources.Resources.views_subscriptions
        Me.tbtSubscriptions.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtSubscriptions.Name = "tbtSubscriptions"
        Me.tbtSubscriptions.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtSubscriptions.Size = New System.Drawing.Size(110, 28)
        Me.tbtSubscriptions.Text = "&Subscriptions"
        '
        'tbtDownloads
        '
        Me.tbtDownloads.Image = Global.RadioDld.My.Resources.Resources.views_downloads
        Me.tbtDownloads.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtDownloads.Name = "tbtDownloads"
        Me.tbtDownloads.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtDownloads.Size = New System.Drawing.Size(98, 28)
        Me.tbtDownloads.Text = "&Downloads"
        '
        'ttxSearch
        '
        Me.ttxSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ttxSearch.Name = "ttxSearch"
        Me.ttxSearch.Size = New System.Drawing.Size(130, 31)
        Me.ttxSearch.Text = "Search..."
        '
        'nicTrayIcon
        '
        Me.nicTrayIcon.ContextMenu = Me.mnuTray
        Me.nicTrayIcon.Visible = True
        '
        'mnuTray
        '
        Me.mnuTray.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuTrayShow, Me.mnuTraySep, Me.mnuTrayExit})
        '
        'mnuTrayShow
        '
        Me.mnuTrayShow.Index = 0
        Me.mnuTrayShow.Text = "&Show Radio Downloader"
        '
        'mnuTraySep
        '
        Me.mnuTraySep.Index = 1
        Me.mnuTraySep.Text = "-"
        '
        'mnuTrayExit
        '
        Me.mnuTrayExit.Index = 2
        Me.mnuTrayExit.Text = "E&xit"
        '
        'imlListIcons
        '
        Me.imlListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlListIcons.ImageSize = New System.Drawing.Size(16, 16)
        Me.imlListIcons.TransparentColor = System.Drawing.Color.Transparent
        '
        'imlProviders
        '
        Me.imlProviders.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlProviders.ImageSize = New System.Drawing.Size(64, 64)
        Me.imlProviders.TransparentColor = System.Drawing.Color.Transparent
        '
        'prgDldProg
        '
        Me.prgDldProg.Location = New System.Drawing.Point(430, 427)
        Me.prgDldProg.Name = "prgDldProg"
        Me.prgDldProg.Size = New System.Drawing.Size(100, 23)
        Me.prgDldProg.TabIndex = 5
        Me.prgDldProg.Visible = False
        '
        'tmrCheckForUpdates
        '
        Me.tmrCheckForUpdates.Enabled = True
        Me.tmrCheckForUpdates.Interval = 3600000
        '
        'mnuOptions
        '
        Me.mnuOptions.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuOptionsShowOpts, Me.mnuOptionsProviderOpts, Me.mnuOptionsSep, Me.mnuOptionsExit})
        '
        'mnuOptionsShowOpts
        '
        Me.mnuOptionsShowOpts.Index = 0
        Me.mnuOptionsShowOpts.Text = "Main &Options"
        '
        'mnuOptionsSep
        '
        Me.mnuOptionsSep.Index = 2
        Me.mnuOptionsSep.Text = "-"
        '
        'mnuOptionsExit
        '
        Me.mnuOptionsExit.Index = 3
        Me.mnuOptionsExit.Text = "E&xit"
        '
        'mnuHelp
        '
        Me.mnuHelp.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuHelpShowHelp, Me.mnuHelpReportBug, Me.mnuHelpSep, Me.mnuHelpAbout})
        '
        'mnuHelpShowHelp
        '
        Me.mnuHelpShowHelp.Index = 0
        Me.mnuHelpShowHelp.Shortcut = System.Windows.Forms.Shortcut.F1
        Me.mnuHelpShowHelp.Text = "&Help Contents"
        '
        'mnuHelpReportBug
        '
        Me.mnuHelpReportBug.Index = 1
        Me.mnuHelpReportBug.Text = "Report a &Bug"
        '
        'mnuHelpSep
        '
        Me.mnuHelpSep.Index = 2
        Me.mnuHelpSep.Text = "-"
        '
        'mnuHelpAbout
        '
        Me.mnuHelpAbout.Index = 3
        Me.mnuHelpAbout.Text = "&About"
        '
        'imlToolbar
        '
        Me.imlToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imlToolbar.ImageSize = New System.Drawing.Size(16, 16)
        Me.imlToolbar.TransparentColor = System.Drawing.Color.Transparent
        '
        'tblInfo
        '
        Me.tblInfo.BackColor = System.Drawing.SystemColors.Window
        Me.tblInfo.ColumnCount = 1
        Me.tblInfo.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tblInfo.Controls.Add(Me.txtSideDescript, 0, 2)
        Me.tblInfo.Controls.Add(Me.lblSideMainTitle, 0, 0)
        Me.tblInfo.Controls.Add(Me.picSidebarImg, 0, 1)
        Me.tblInfo.Dock = System.Windows.Forms.DockStyle.Left
        Me.tblInfo.Location = New System.Drawing.Point(0, 65)
        Me.tblInfo.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.tblInfo.Name = "tblInfo"
        Me.tblInfo.RowCount = 4
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.Size = New System.Drawing.Size(187, 406)
        Me.tblInfo.TabIndex = 8
        '
        'txtSideDescript
        '
        Me.txtSideDescript.BackColor = System.Drawing.SystemColors.Window
        Me.txtSideDescript.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtSideDescript.Cursor = System.Windows.Forms.Cursors.Default
        Me.txtSideDescript.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtSideDescript.Location = New System.Drawing.Point(8, 120)
        Me.txtSideDescript.Margin = New System.Windows.Forms.Padding(8, 5, 8, 6)
        Me.txtSideDescript.Multiline = True
        Me.txtSideDescript.Name = "txtSideDescript"
        Me.txtSideDescript.ReadOnly = True
        Me.txtSideDescript.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtSideDescript.ShortcutsEnabled = False
        Me.txtSideDescript.Size = New System.Drawing.Size(171, 280)
        Me.txtSideDescript.TabIndex = 1
        Me.txtSideDescript.TabStop = False
        Me.txtSideDescript.Text = "Description"
        '
        'lblSideMainTitle
        '
        Me.lblSideMainTitle.AutoSize = True
        Me.lblSideMainTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblSideMainTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblSideMainTitle.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSideMainTitle.ForeColor = System.Drawing.SystemColors.WindowText
        Me.lblSideMainTitle.Location = New System.Drawing.Point(8, 10)
        Me.lblSideMainTitle.Margin = New System.Windows.Forms.Padding(8, 10, 8, 8)
        Me.lblSideMainTitle.Name = "lblSideMainTitle"
        Me.lblSideMainTitle.Size = New System.Drawing.Size(171, 19)
        Me.lblSideMainTitle.TabIndex = 0
        Me.lblSideMainTitle.Text = "Title"
        Me.lblSideMainTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblSideMainTitle.UseMnemonic = False
        '
        'picSidebarImg
        '
        Me.picSidebarImg.ErrorImage = Nothing
        Me.picSidebarImg.InitialImage = Nothing
        Me.picSidebarImg.Location = New System.Drawing.Point(12, 40)
        Me.picSidebarImg.Margin = New System.Windows.Forms.Padding(12, 3, 12, 5)
        Me.picSidebarImg.MaximumSize = New System.Drawing.Size(160, 100)
        Me.picSidebarImg.Name = "picSidebarImg"
        Me.picSidebarImg.Size = New System.Drawing.Size(70, 70)
        Me.picSidebarImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picSidebarImg.TabIndex = 1
        Me.picSidebarImg.TabStop = False
        '
        'pnlPluginSpace
        '
        Me.pnlPluginSpace.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlPluginSpace.Location = New System.Drawing.Point(197, 149)
        Me.pnlPluginSpace.Name = "pnlPluginSpace"
        Me.pnlPluginSpace.Size = New System.Drawing.Size(560, 62)
        Me.pnlPluginSpace.TabIndex = 1
        '
        'tblToolbars
        '
        Me.tblToolbars.ColumnCount = 2
        Me.tblToolbars.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90.0!))
        Me.tblToolbars.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0!))
        Me.tblToolbars.Controls.Add(Me.tbrToolbar, 0, 0)
        Me.tblToolbars.Controls.Add(Me.tbrHelp, 1, 0)
        Me.tblToolbars.Dock = System.Windows.Forms.DockStyle.Top
        Me.tblToolbars.Location = New System.Drawing.Point(0, 31)
        Me.tblToolbars.Name = "tblToolbars"
        Me.tblToolbars.RowCount = 1
        Me.tblToolbars.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tblToolbars.Size = New System.Drawing.Size(757, 34)
        Me.tblToolbars.TabIndex = 7
        '
        'picSideBarBorder
        '
        Me.picSideBarBorder.BackColor = System.Drawing.SystemColors.Window
        Me.picSideBarBorder.Dock = System.Windows.Forms.DockStyle.Left
        Me.picSideBarBorder.Location = New System.Drawing.Point(187, 65)
        Me.picSideBarBorder.Margin = New System.Windows.Forms.Padding(0)
        Me.picSideBarBorder.Name = "picSideBarBorder"
        Me.picSideBarBorder.Size = New System.Drawing.Size(10, 406)
        Me.picSideBarBorder.TabIndex = 22
        Me.picSideBarBorder.TabStop = False
        '
        'lstDownloads
        '
        Me.lstDownloads.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstDownloads.FullRowSelect = True
        Me.lstDownloads.HideSelection = False
        Me.lstDownloads.Location = New System.Drawing.Point(197, 418)
        Me.lstDownloads.Margin = New System.Windows.Forms.Padding(0, 3, 3, 0)
        Me.lstDownloads.MultiSelect = False
        Me.lstDownloads.Name = "lstDownloads"
        Me.lstDownloads.Size = New System.Drawing.Size(560, 54)
        Me.lstDownloads.TabIndex = 4
        Me.lstDownloads.UseCompatibleStateImageBehavior = False
        Me.lstDownloads.View = System.Windows.Forms.View.Details
        '
        'lstSubscribed
        '
        Me.lstSubscribed.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstSubscribed.FullRowSelect = True
        Me.lstSubscribed.HideSelection = False
        Me.lstSubscribed.Location = New System.Drawing.Point(197, 348)
        Me.lstSubscribed.Margin = New System.Windows.Forms.Padding(0, 3, 3, 3)
        Me.lstSubscribed.MultiSelect = False
        Me.lstSubscribed.Name = "lstSubscribed"
        Me.lstSubscribed.Size = New System.Drawing.Size(560, 49)
        Me.lstSubscribed.TabIndex = 3
        Me.lstSubscribed.UseCompatibleStateImageBehavior = False
        Me.lstSubscribed.View = System.Windows.Forms.View.Details
        '
        'lstEpisodes
        '
        Me.lstEpisodes.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstEpisodes.CheckBoxes = True
        Me.lstEpisodes.FullRowSelect = True
        Me.lstEpisodes.HideSelection = False
        Me.lstEpisodes.Location = New System.Drawing.Point(197, 247)
        Me.lstEpisodes.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        Me.lstEpisodes.MultiSelect = False
        Me.lstEpisodes.Name = "lstEpisodes"
        Me.lstEpisodes.Size = New System.Drawing.Size(560, 50)
        Me.lstEpisodes.TabIndex = 2
        Me.lstEpisodes.UseCompatibleStateImageBehavior = False
        Me.lstEpisodes.View = System.Windows.Forms.View.Details
        '
        'lstProviders
        '
        Me.lstProviders.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstProviders.HideSelection = False
        Me.lstProviders.Location = New System.Drawing.Point(197, 63)
        Me.lstProviders.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        Me.lstProviders.MultiSelect = False
        Me.lstProviders.Name = "lstProviders"
        Me.lstProviders.Size = New System.Drawing.Size(560, 62)
        Me.lstProviders.TabIndex = 0
        Me.lstProviders.UseCompatibleStateImageBehavior = False
        '
        'tbrToolbar
        '
        Me.tbrToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat
        Me.tbrToolbar.Buttons.AddRange(New System.Windows.Forms.ToolBarButton() {Me.tbtOptionsMenu, Me.tbtChooseProgramme, Me.tbtDownload, Me.tbtSubscribe, Me.tbtUnsubscribe, Me.tbtCurrentEps, Me.tbtCancel, Me.tbtPlay, Me.tbtDelete, Me.tbtRetry, Me.tbtReportError, Me.tbtCleanUp})
        Me.tbrToolbar.ButtonSize = New System.Drawing.Size(135, 22)
        Me.tbrToolbar.Divider = False
        Me.tbrToolbar.DropDownArrows = True
        Me.tbrToolbar.Location = New System.Drawing.Point(3, 2)
        Me.tbrToolbar.Margin = New System.Windows.Forms.Padding(3, 2, 0, 0)
        Me.tbrToolbar.Name = "tbrToolbar"
        Me.tbrToolbar.ShowToolTips = True
        Me.tbrToolbar.Size = New System.Drawing.Size(678, 26)
        Me.tbrToolbar.TabIndex = 0
        Me.tbrToolbar.TabStop = True
        Me.tbrToolbar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right
        Me.tbrToolbar.Wrappable = False
        '
        'tbtOptionsMenu
        '
        Me.tbtOptionsMenu.DropDownMenu = Me.mnuOptions
        Me.tbtOptionsMenu.ImageKey = "options"
        Me.tbtOptionsMenu.Name = "tbtOptionsMenu"
        Me.tbtOptionsMenu.Text = "&Options"
        '
        'tbtChooseProgramme
        '
        Me.tbtChooseProgramme.ImageKey = "choose_programme"
        Me.tbtChooseProgramme.Name = "tbtChooseProgramme"
        Me.tbtChooseProgramme.Text = "&Choose Programme"
        '
        'tbtDownload
        '
        Me.tbtDownload.ImageKey = "download"
        Me.tbtDownload.Name = "tbtDownload"
        Me.tbtDownload.Text = "Do&wnload"
        '
        'tbtSubscribe
        '
        Me.tbtSubscribe.ImageKey = "subscribe"
        Me.tbtSubscribe.Name = "tbtSubscribe"
        Me.tbtSubscribe.Text = "S&ubscribe"
        '
        'tbtUnsubscribe
        '
        Me.tbtUnsubscribe.ImageKey = "unsubscribe"
        Me.tbtUnsubscribe.Name = "tbtUnsubscribe"
        Me.tbtUnsubscribe.Text = "&Unsubscribe"
        '
        'tbtCurrentEps
        '
        Me.tbtCurrentEps.ImageKey = "current_episodes"
        Me.tbtCurrentEps.Name = "tbtCurrentEps"
        Me.tbtCurrentEps.Text = "&Current Episodes"
        '
        'tbtCancel
        '
        Me.tbtCancel.ImageKey = "delete"
        Me.tbtCancel.Name = "tbtCancel"
        Me.tbtCancel.Text = "&Cancel"
        '
        'tbtPlay
        '
        Me.tbtPlay.ImageKey = "play"
        Me.tbtPlay.Name = "tbtPlay"
        Me.tbtPlay.Text = "&Play"
        '
        'tbtDelete
        '
        Me.tbtDelete.ImageKey = "delete"
        Me.tbtDelete.Name = "tbtDelete"
        Me.tbtDelete.Text = "D&elete"
        '
        'tbtRetry
        '
        Me.tbtRetry.ImageKey = "retry"
        Me.tbtRetry.Name = "tbtRetry"
        Me.tbtRetry.Text = "&Retry"
        '
        'tbtReportError
        '
        Me.tbtReportError.ImageKey = "report_error"
        Me.tbtReportError.Name = "tbtReportError"
        Me.tbtReportError.Text = "Report &Error"
        '
        'tbtCleanUp
        '
        Me.tbtCleanUp.ImageKey = "clean_up"
        Me.tbtCleanUp.Name = "tbtCleanUp"
        Me.tbtCleanUp.Text = "Clean &Up"
        '
        'tbrHelp
        '
        Me.tbrHelp.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbrHelp.Appearance = System.Windows.Forms.ToolBarAppearance.Flat
        Me.tbrHelp.Buttons.AddRange(New System.Windows.Forms.ToolBarButton() {Me.tbtHelpMenu})
        Me.tbrHelp.Divider = False
        Me.tbrHelp.Dock = System.Windows.Forms.DockStyle.None
        Me.tbrHelp.DropDownArrows = True
        Me.tbrHelp.Location = New System.Drawing.Point(681, 2)
        Me.tbrHelp.Margin = New System.Windows.Forms.Padding(0, 2, 3, 0)
        Me.tbrHelp.Name = "tbrHelp"
        Me.tbrHelp.ShowToolTips = True
        Me.tbrHelp.Size = New System.Drawing.Size(73, 26)
        Me.tbrHelp.TabIndex = 1
        Me.tbrHelp.TabStop = True
        Me.tbrHelp.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right
        Me.tbrHelp.Wrappable = False
        '
        'tbtHelpMenu
        '
        Me.tbtHelpMenu.DropDownMenu = Me.mnuHelp
        Me.tbtHelpMenu.ImageKey = "help"
        Me.tbtHelpMenu.Name = "tbtHelpMenu"
        Me.tbtHelpMenu.Text = "&Help"
        '
        'mnuOptionsProviderOpts
        '
        Me.mnuOptionsProviderOpts.Index = 1
        Me.mnuOptionsProviderOpts.Text = "&Provider Options"
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(757, 471)
        Me.Controls.Add(Me.lstDownloads)
        Me.Controls.Add(Me.lstSubscribed)
        Me.Controls.Add(Me.lstEpisodes)
        Me.Controls.Add(Me.pnlPluginSpace)
        Me.Controls.Add(Me.lstProviders)
        Me.Controls.Add(Me.picSideBarBorder)
        Me.Controls.Add(Me.prgDldProg)
        Me.Controls.Add(Me.tblInfo)
        Me.Controls.Add(Me.tblToolbars)
        Me.Controls.Add(Me.tbrView)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = Global.RadioDld.My.Resources.Resources.icon_main
        Me.KeyPreview = True
        Me.Location = New System.Drawing.Point(11, 37)
        Me.MinimumSize = New System.Drawing.Size(500, 300)
        Me.Name = "Main"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        Me.tbrView.ResumeLayout(False)
        Me.tbrView.PerformLayout()
        Me.tblInfo.ResumeLayout(False)
        Me.tblInfo.PerformLayout()
        CType(Me.picSidebarImg, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tblToolbars.ResumeLayout(False)
        Me.tblToolbars.PerformLayout()
        CType(Me.picSideBarBorder, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tbrView As System.Windows.Forms.ToolStrip
    Friend WithEvents tbtFindNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtSubscriptions As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtDownloads As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtCleanUp As System.Windows.Forms.ToolBarButton
    Friend WithEvents nicTrayIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents mnuTray As System.Windows.Forms.ContextMenu
    Friend WithEvents mnuTrayShow As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTraySep As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTrayExit As System.Windows.Forms.MenuItem
    Friend WithEvents ttxSearch As System.Windows.Forms.ToolStripTextBox
    Friend WithEvents lstProviders As RadioDld.ExtListView
    Friend WithEvents imlListIcons As System.Windows.Forms.ImageList
    Friend WithEvents imlProviders As System.Windows.Forms.ImageList
    Friend WithEvents lstSubscribed As RadioDld.ExtListView
    Friend WithEvents prgDldProg As System.Windows.Forms.ProgressBar
    Friend WithEvents lstDownloads As RadioDld.ExtListView
    Friend WithEvents tmrCheckForUpdates As System.Windows.Forms.Timer
    Friend WithEvents tbrToolbar As ExtToolBar
    Friend WithEvents tblInfo As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents picSidebarImg As System.Windows.Forms.PictureBox
    Friend WithEvents tbtPlay As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtCancel As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtSubscribe As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtUnsubscribe As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtDownload As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtDelete As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtRetry As System.Windows.Forms.ToolBarButton
    Friend WithEvents lstEpisodes As RadioDld.ExtListView
    Friend WithEvents tbtMisc As System.Windows.Forms.ToolBarButton
    Friend WithEvents mnuOptionsShowOpts As System.Windows.Forms.MenuItem
    Friend WithEvents mnuOptionsSep As System.Windows.Forms.MenuItem
    Friend WithEvents mnuOptionsExit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpShowHelp As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpAbout As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpReportBug As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpSep As System.Windows.Forms.MenuItem
    Friend WithEvents tbtReportError As System.Windows.Forms.ToolBarButton
    Friend WithEvents pnlPluginSpace As System.Windows.Forms.Panel
    Friend WithEvents tbtFavourites As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtBack As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtForward As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tbtCurrentEps As System.Windows.Forms.ToolBarButton
    Friend WithEvents imlToolbar As System.Windows.Forms.ImageList
    Friend WithEvents mnuOptions As System.Windows.Forms.ContextMenu
    Friend WithEvents mnuHelp As System.Windows.Forms.ContextMenu
    Friend WithEvents tbtOptionsMenu As System.Windows.Forms.ToolBarButton
    Friend WithEvents tbtHelpMenu As System.Windows.Forms.ToolBarButton
    Friend WithEvents tblToolbars As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents tbrHelp As ExtToolBar
    Friend WithEvents lblSideMainTitle As System.Windows.Forms.Label
    Friend WithEvents txtSideDescript As System.Windows.Forms.TextBox
    Friend WithEvents picSideBarBorder As System.Windows.Forms.PictureBox
    Friend WithEvents mnuOptionsProviderOpts As System.Windows.Forms.MenuItem
    Friend WithEvents tbtChooseProgramme As System.Windows.Forms.ToolBarButton
#End Region
End Class

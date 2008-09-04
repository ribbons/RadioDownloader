<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class frmMain
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
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
        Me.tbtCleanUp = New System.Windows.Forms.ToolStripButton
        Me.nicTrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.mnuTray = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuTrayShow = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator
        Me.mnuTrayExit = New System.Windows.Forms.ToolStripMenuItem
        Me.imlListIcons = New System.Windows.Forms.ImageList(Me.components)
        Me.imlProviders = New System.Windows.Forms.ImageList(Me.components)
        Me.prgDldProg = New System.Windows.Forms.ProgressBar
        Me.tmrCheckForUpdates = New System.Windows.Forms.Timer(Me.components)
        Me.tbrToolbar = New System.Windows.Forms.ToolStrip
        Me.tbmMisc = New System.Windows.Forms.ToolStripDropDownButton
        Me.tbmOptions = New System.Windows.Forms.ToolStripMenuItem
        Me.tbmSep1 = New System.Windows.Forms.ToolStripSeparator
        Me.tbmExit = New System.Windows.Forms.ToolStripMenuItem
        Me.tbtDownload = New System.Windows.Forms.ToolStripButton
        Me.tbtSubscribe = New System.Windows.Forms.ToolStripButton
        Me.tbtUnsubscribe = New System.Windows.Forms.ToolStripButton
        Me.tbtCurrentEps = New System.Windows.Forms.ToolStripButton
        Me.tbtCancel = New System.Windows.Forms.ToolStripButton
        Me.tbtPlay = New System.Windows.Forms.ToolStripButton
        Me.tbtDelete = New System.Windows.Forms.ToolStripButton
        Me.tbtRetry = New System.Windows.Forms.ToolStripButton
        Me.tbtReportError = New System.Windows.Forms.ToolStripButton
        Me.tbmHelp = New System.Windows.Forms.ToolStripDropDownButton
        Me.tbmShowHelp = New System.Windows.Forms.ToolStripMenuItem
        Me.tbmReportABug = New System.Windows.Forms.ToolStripMenuItem
        Me.tbmSep2 = New System.Windows.Forms.ToolStripSeparator
        Me.tbmAbout = New System.Windows.Forms.ToolStripMenuItem
        Me.tblInfo = New System.Windows.Forms.TableLayoutPanel
        Me.lblSideMainTitle = New System.Windows.Forms.Label
        Me.picSidebarImg = New System.Windows.Forms.PictureBox
        Me.lblSideDescript = New System.Windows.Forms.Label
        Me.pnlPluginSpace = New System.Windows.Forms.Panel
        Me.lstDownloads = New RadioDld.ExtListView
        Me.lstSubscribed = New RadioDld.ExtListView
        Me.lstEpisodes = New RadioDld.ExtListView
        Me.lstProviders = New RadioDld.ExtListView
        Me.tbrView.SuspendLayout()
        Me.mnuTray.SuspendLayout()
        Me.tbrToolbar.SuspendLayout()
        Me.tblInfo.SuspendLayout()
        CType(Me.picSidebarImg, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tmrCheckSub
        '
        Me.tmrCheckSub.Enabled = True
        Me.tmrCheckSub.Interval = 60000
        '
        'tmrStartProcess
        '
        Me.tmrStartProcess.Interval = 2000
        '
        'tbrView
        '
        Me.tbrView.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.tbrView.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tbtBack, Me.tbtForward, Me.ToolStripSeparator1, Me.tbtFindNew, Me.tbtFavourites, Me.tbtSubscriptions, Me.tbtDownloads, Me.ttxSearch})
        Me.tbrView.Location = New System.Drawing.Point(0, 0)
        Me.tbrView.Name = "tbrView"
        Me.tbrView.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.tbrView.Size = New System.Drawing.Size(757, 31)
        Me.tbrView.TabIndex = 11
        '
        'tbtBack
        '
        Me.tbtBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tbtBack.Image = CType(resources.GetObject("tbtBack.Image"), System.Drawing.Image)
        Me.tbtBack.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtBack.Name = "tbtBack"
        Me.tbtBack.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtBack.Size = New System.Drawing.Size(32, 28)
        Me.tbtBack.Text = "Back"
        '
        'tbtForward
        '
        Me.tbtForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tbtForward.Image = CType(resources.GetObject("tbtForward.Image"), System.Drawing.Image)
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
        Me.tbtFindNew.Image = CType(resources.GetObject("tbtFindNew.Image"), System.Drawing.Image)
        Me.tbtFindNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtFindNew.Name = "tbtFindNew"
        Me.tbtFindNew.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtFindNew.Size = New System.Drawing.Size(128, 28)
        Me.tbtFindNew.Text = "Find Programme"
        '
        'tbtFavourites
        '
        Me.tbtFavourites.Image = CType(resources.GetObject("tbtFavourites.Image"), System.Drawing.Image)
        Me.tbtFavourites.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtFavourites.Name = "tbtFavourites"
        Me.tbtFavourites.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtFavourites.Size = New System.Drawing.Size(93, 28)
        Me.tbtFavourites.Text = "Favourites"
        Me.tbtFavourites.Visible = False
        '
        'tbtSubscriptions
        '
        Me.tbtSubscriptions.Image = CType(resources.GetObject("tbtSubscriptions.Image"), System.Drawing.Image)
        Me.tbtSubscriptions.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtSubscriptions.Name = "tbtSubscriptions"
        Me.tbtSubscriptions.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtSubscriptions.Size = New System.Drawing.Size(110, 28)
        Me.tbtSubscriptions.Text = "Subscriptions"
        '
        'tbtDownloads
        '
        Me.tbtDownloads.Image = CType(resources.GetObject("tbtDownloads.Image"), System.Drawing.Image)
        Me.tbtDownloads.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtDownloads.Name = "tbtDownloads"
        Me.tbtDownloads.Padding = New System.Windows.Forms.Padding(2, 0, 2, 0)
        Me.tbtDownloads.Size = New System.Drawing.Size(98, 28)
        Me.tbtDownloads.Text = "Downloads"
        '
        'ttxSearch
        '
        Me.ttxSearch.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.ttxSearch.Name = "ttxSearch"
        Me.ttxSearch.Size = New System.Drawing.Size(130, 31)
        Me.ttxSearch.Text = "Search..."
        '
        'tbtCleanUp
        '
        Me.tbtCleanUp.Image = CType(resources.GetObject("tbtCleanUp.Image"), System.Drawing.Image)
        Me.tbtCleanUp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtCleanUp.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtCleanUp.Name = "tbtCleanUp"
        Me.tbtCleanUp.Size = New System.Drawing.Size(75, 20)
        Me.tbtCleanUp.Text = "Clean Up"
        '
        'nicTrayIcon
        '
        Me.nicTrayIcon.ContextMenuStrip = Me.mnuTray
        Me.nicTrayIcon.Visible = True
        '
        'mnuTray
        '
        Me.mnuTray.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTrayShow, Me.ToolStripSeparator3, Me.mnuTrayExit})
        Me.mnuTray.Name = "ContextMenuStrip1"
        Me.mnuTray.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.mnuTray.Size = New System.Drawing.Size(204, 54)
        '
        'mnuTrayShow
        '
        Me.mnuTrayShow.Name = "mnuTrayShow"
        Me.mnuTrayShow.Size = New System.Drawing.Size(203, 22)
        Me.mnuTrayShow.Text = "&Show Radio Downloader"
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(200, 6)
        '
        'mnuTrayExit
        '
        Me.mnuTrayExit.Name = "mnuTrayExit"
        Me.mnuTrayExit.Size = New System.Drawing.Size(203, 22)
        Me.mnuTrayExit.Text = "E&xit"
        '
        'imlListIcons
        '
        Me.imlListIcons.ImageStream = CType(resources.GetObject("imlListIcons.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.imlListIcons.TransparentColor = System.Drawing.Color.Transparent
        Me.imlListIcons.Images.SetKeyName(0, "downloading")
        Me.imlListIcons.Images.SetKeyName(1, "waiting")
        Me.imlListIcons.Images.SetKeyName(2, "converting")
        Me.imlListIcons.Images.SetKeyName(3, "downloaded_new")
        Me.imlListIcons.Images.SetKeyName(4, "downloaded")
        Me.imlListIcons.Images.SetKeyName(5, "subscribed")
        Me.imlListIcons.Images.SetKeyName(6, "error")
        '
        'imlProviders
        '
        Me.imlProviders.ImageStream = CType(resources.GetObject("imlProviders.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.imlProviders.TransparentColor = System.Drawing.Color.Transparent
        Me.imlProviders.Images.SetKeyName(0, "default")
        '
        'prgDldProg
        '
        Me.prgDldProg.Location = New System.Drawing.Point(430, 427)
        Me.prgDldProg.Name = "prgDldProg"
        Me.prgDldProg.Size = New System.Drawing.Size(100, 23)
        Me.prgDldProg.TabIndex = 16
        Me.prgDldProg.Visible = False
        '
        'tmrCheckForUpdates
        '
        Me.tmrCheckForUpdates.Enabled = True
        Me.tmrCheckForUpdates.Interval = 3600000
        '
        'tbrToolbar
        '
        Me.tbrToolbar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tbmMisc, Me.tbtDownload, Me.tbtSubscribe, Me.tbtUnsubscribe, Me.tbtCurrentEps, Me.tbtCancel, Me.tbtPlay, Me.tbtDelete, Me.tbtRetry, Me.tbtReportError, Me.tbtCleanUp, Me.tbmHelp})
        Me.tbrToolbar.Location = New System.Drawing.Point(0, 31)
        Me.tbrToolbar.Name = "tbrToolbar"
        Me.tbrToolbar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System
        Me.tbrToolbar.Size = New System.Drawing.Size(757, 25)
        Me.tbrToolbar.TabIndex = 17
        '
        'tbmMisc
        '
        Me.tbmMisc.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tbmOptions, Me.tbmSep1, Me.tbmExit})
        Me.tbmMisc.Image = CType(resources.GetObject("tbmMisc.Image"), System.Drawing.Image)
        Me.tbmMisc.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbmMisc.Margin = New System.Windows.Forms.Padding(0, 1, 2, 2)
        Me.tbmMisc.Name = "tbmMisc"
        Me.tbmMisc.Size = New System.Drawing.Size(78, 22)
        Me.tbmMisc.Text = "Options"
        '
        'tbmOptions
        '
        Me.tbmOptions.Name = "tbmOptions"
        Me.tbmOptions.Size = New System.Drawing.Size(148, 22)
        Me.tbmOptions.Text = "Show &Options"
        '
        'tbmSep1
        '
        Me.tbmSep1.Name = "tbmSep1"
        Me.tbmSep1.Size = New System.Drawing.Size(145, 6)
        '
        'tbmExit
        '
        Me.tbmExit.Name = "tbmExit"
        Me.tbmExit.Size = New System.Drawing.Size(148, 22)
        Me.tbmExit.Text = "E&xit"
        '
        'tbtDownload
        '
        Me.tbtDownload.Image = CType(resources.GetObject("tbtDownload.Image"), System.Drawing.Image)
        Me.tbtDownload.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtDownload.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtDownload.Name = "tbtDownload"
        Me.tbtDownload.Size = New System.Drawing.Size(81, 22)
        Me.tbtDownload.Text = "Download"
        '
        'tbtSubscribe
        '
        Me.tbtSubscribe.Image = CType(resources.GetObject("tbtSubscribe.Image"), System.Drawing.Image)
        Me.tbtSubscribe.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtSubscribe.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtSubscribe.Name = "tbtSubscribe"
        Me.tbtSubscribe.Size = New System.Drawing.Size(78, 22)
        Me.tbtSubscribe.Text = "Subscribe"
        '
        'tbtUnsubscribe
        '
        Me.tbtUnsubscribe.Image = CType(resources.GetObject("tbtUnsubscribe.Image"), System.Drawing.Image)
        Me.tbtUnsubscribe.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtUnsubscribe.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtUnsubscribe.Name = "tbtUnsubscribe"
        Me.tbtUnsubscribe.Size = New System.Drawing.Size(92, 22)
        Me.tbtUnsubscribe.Text = "Unsubscribe"
        '
        'tbtCurrentEps
        '
        Me.tbtCurrentEps.Image = CType(resources.GetObject("tbtCurrentEps.Image"), System.Drawing.Image)
        Me.tbtCurrentEps.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtCurrentEps.Name = "tbtCurrentEps"
        Me.tbtCurrentEps.Size = New System.Drawing.Size(116, 22)
        Me.tbtCurrentEps.Text = "Current Episodes"
        '
        'tbtCancel
        '
        Me.tbtCancel.Image = CType(resources.GetObject("tbtCancel.Image"), System.Drawing.Image)
        Me.tbtCancel.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtCancel.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtCancel.Name = "tbtCancel"
        Me.tbtCancel.Size = New System.Drawing.Size(63, 22)
        Me.tbtCancel.Text = "Cancel"
        '
        'tbtPlay
        '
        Me.tbtPlay.Image = CType(resources.GetObject("tbtPlay.Image"), System.Drawing.Image)
        Me.tbtPlay.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtPlay.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtPlay.Name = "tbtPlay"
        Me.tbtPlay.Size = New System.Drawing.Size(49, 22)
        Me.tbtPlay.Text = "Play"
        '
        'tbtDelete
        '
        Me.tbtDelete.Image = CType(resources.GetObject("tbtDelete.Image"), System.Drawing.Image)
        Me.tbtDelete.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtDelete.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtDelete.Name = "tbtDelete"
        Me.tbtDelete.Size = New System.Drawing.Size(60, 22)
        Me.tbtDelete.Text = "Delete"
        '
        'tbtRetry
        '
        Me.tbtRetry.Image = CType(resources.GetObject("tbtRetry.Image"), System.Drawing.Image)
        Me.tbtRetry.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtRetry.Margin = New System.Windows.Forms.Padding(2, 1, 2, 2)
        Me.tbtRetry.Name = "tbtRetry"
        Me.tbtRetry.Size = New System.Drawing.Size(54, 22)
        Me.tbtRetry.Text = "Retry"
        '
        'tbtReportError
        '
        Me.tbtReportError.Image = CType(resources.GetObject("tbtReportError.Image"), System.Drawing.Image)
        Me.tbtReportError.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbtReportError.Name = "tbtReportError"
        Me.tbtReportError.Size = New System.Drawing.Size(90, 20)
        Me.tbtReportError.Text = "Report Error"
        '
        'tbmHelp
        '
        Me.tbmHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.tbmHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tbmHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tbmShowHelp, Me.tbmReportABug, Me.tbmSep2, Me.tbmAbout})
        Me.tbmHelp.Image = CType(resources.GetObject("tbmHelp.Image"), System.Drawing.Image)
        Me.tbmHelp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tbmHelp.Name = "tbmHelp"
        Me.tbmHelp.Size = New System.Drawing.Size(29, 20)
        Me.tbmHelp.Text = "Help"
        '
        'tbmShowHelp
        '
        Me.tbmShowHelp.Name = "tbmShowHelp"
        Me.tbmShowHelp.Size = New System.Drawing.Size(142, 22)
        Me.tbmShowHelp.Text = "&Help"
        Me.tbmShowHelp.Visible = False
        '
        'tbmReportABug
        '
        Me.tbmReportABug.Name = "tbmReportABug"
        Me.tbmReportABug.Size = New System.Drawing.Size(142, 22)
        Me.tbmReportABug.Text = "Report a &Bug"
        '
        'tbmSep2
        '
        Me.tbmSep2.Name = "tbmSep2"
        Me.tbmSep2.Size = New System.Drawing.Size(139, 6)
        '
        'tbmAbout
        '
        Me.tbmAbout.Name = "tbmAbout"
        Me.tbmAbout.Size = New System.Drawing.Size(142, 22)
        Me.tbmAbout.Text = "&About"
        '
        'tblInfo
        '
        Me.tblInfo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.tblInfo.BackColor = System.Drawing.Color.Transparent
        Me.tblInfo.BackgroundImage = CType(resources.GetObject("tblInfo.BackgroundImage"), System.Drawing.Image)
        Me.tblInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.tblInfo.ColumnCount = 1
        Me.tblInfo.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tblInfo.Controls.Add(Me.lblSideMainTitle, 0, 0)
        Me.tblInfo.Controls.Add(Me.picSidebarImg, 0, 1)
        Me.tblInfo.Controls.Add(Me.lblSideDescript, 0, 2)
        Me.tblInfo.Location = New System.Drawing.Point(0, 56)
        Me.tblInfo.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.tblInfo.Name = "tblInfo"
        Me.tblInfo.RowCount = 4
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tblInfo.RowStyles.Add(New System.Windows.Forms.RowStyle)
        Me.tblInfo.Size = New System.Drawing.Size(187, 415)
        Me.tblInfo.TabIndex = 18
        '
        'lblSideMainTitle
        '
        Me.lblSideMainTitle.AutoSize = True
        Me.lblSideMainTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblSideMainTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblSideMainTitle.Font = New System.Drawing.Font("Tahoma", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSideMainTitle.ForeColor = System.Drawing.Color.White
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
        Me.picSidebarImg.MaximumSize = New System.Drawing.Size(100, 100)
        Me.picSidebarImg.Name = "picSidebarImg"
        Me.picSidebarImg.Size = New System.Drawing.Size(70, 70)
        Me.picSidebarImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picSidebarImg.TabIndex = 1
        Me.picSidebarImg.TabStop = False
        '
        'lblSideDescript
        '
        Me.lblSideDescript.AutoSize = True
        Me.lblSideDescript.BackColor = System.Drawing.Color.Transparent
        Me.lblSideDescript.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblSideDescript.ForeColor = System.Drawing.Color.White
        Me.lblSideDescript.Location = New System.Drawing.Point(8, 120)
        Me.lblSideDescript.Margin = New System.Windows.Forms.Padding(8, 5, 8, 6)
        Me.lblSideDescript.Name = "lblSideDescript"
        Me.lblSideDescript.Size = New System.Drawing.Size(171, 289)
        Me.lblSideDescript.TabIndex = 2
        Me.lblSideDescript.Text = "Description"
        Me.lblSideDescript.UseMnemonic = False
        '
        'pnlPluginSpace
        '
        Me.pnlPluginSpace.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlPluginSpace.Location = New System.Drawing.Point(187, 142)
        Me.pnlPluginSpace.Name = "pnlPluginSpace"
        Me.pnlPluginSpace.Size = New System.Drawing.Size(570, 62)
        Me.pnlPluginSpace.TabIndex = 20
        '
        'lstDownloads
        '
        Me.lstDownloads.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstDownloads.FullRowSelect = True
        Me.lstDownloads.HideSelection = False
        Me.lstDownloads.Location = New System.Drawing.Point(187, 410)
        Me.lstDownloads.Margin = New System.Windows.Forms.Padding(0, 3, 3, 0)
        Me.lstDownloads.MultiSelect = False
        Me.lstDownloads.Name = "lstDownloads"
        Me.lstDownloads.Size = New System.Drawing.Size(570, 62)
        Me.lstDownloads.TabIndex = 15
        Me.lstDownloads.UseCompatibleStateImageBehavior = False
        Me.lstDownloads.View = System.Windows.Forms.View.Details
        '
        'lstSubscribed
        '
        Me.lstSubscribed.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstSubscribed.FullRowSelect = True
        Me.lstSubscribed.HideSelection = False
        Me.lstSubscribed.Location = New System.Drawing.Point(187, 321)
        Me.lstSubscribed.Margin = New System.Windows.Forms.Padding(0, 3, 3, 3)
        Me.lstSubscribed.MultiSelect = False
        Me.lstSubscribed.Name = "lstSubscribed"
        Me.lstSubscribed.Size = New System.Drawing.Size(570, 62)
        Me.lstSubscribed.TabIndex = 14
        Me.lstSubscribed.UseCompatibleStateImageBehavior = False
        Me.lstSubscribed.View = System.Windows.Forms.View.Details
        '
        'lstEpisodes
        '
        Me.lstEpisodes.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstEpisodes.CheckBoxes = True
        Me.lstEpisodes.FullRowSelect = True
        Me.lstEpisodes.HideSelection = False
        Me.lstEpisodes.Location = New System.Drawing.Point(187, 231)
        Me.lstEpisodes.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        Me.lstEpisodes.MultiSelect = False
        Me.lstEpisodes.Name = "lstEpisodes"
        Me.lstEpisodes.Size = New System.Drawing.Size(570, 62)
        Me.lstEpisodes.TabIndex = 19
        Me.lstEpisodes.UseCompatibleStateImageBehavior = False
        Me.lstEpisodes.View = System.Windows.Forms.View.Details
        '
        'lstProviders
        '
        Me.lstProviders.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lstProviders.HideSelection = False
        Me.lstProviders.Location = New System.Drawing.Point(187, 56)
        Me.lstProviders.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        Me.lstProviders.MultiSelect = False
        Me.lstProviders.Name = "lstProviders"
        Me.lstProviders.Size = New System.Drawing.Size(570, 62)
        Me.lstProviders.TabIndex = 12
        Me.lstProviders.UseCompatibleStateImageBehavior = False
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(757, 471)
        Me.Controls.Add(Me.prgDldProg)
        Me.Controls.Add(Me.pnlPluginSpace)
        Me.Controls.Add(Me.lstDownloads)
        Me.Controls.Add(Me.lstSubscribed)
        Me.Controls.Add(Me.lstEpisodes)
        Me.Controls.Add(Me.lstProviders)
        Me.Controls.Add(Me.tblInfo)
        Me.Controls.Add(Me.tbrToolbar)
        Me.Controls.Add(Me.tbrView)
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(11, 37)
        Me.MinimumSize = New System.Drawing.Size(400, 300)
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        Me.tbrView.ResumeLayout(False)
        Me.tbrView.PerformLayout()
        Me.mnuTray.ResumeLayout(False)
        Me.tbrToolbar.ResumeLayout(False)
        Me.tbrToolbar.PerformLayout()
        Me.tblInfo.ResumeLayout(False)
        Me.tblInfo.PerformLayout()
        CType(Me.picSidebarImg, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tbrView As System.Windows.Forms.ToolStrip
    Friend WithEvents tbtFindNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtSubscriptions As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtDownloads As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtCleanUp As System.Windows.Forms.ToolStripButton
    Friend WithEvents nicTrayIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents mnuTray As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuTrayShow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuTrayExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ttxSearch As System.Windows.Forms.ToolStripTextBox
    Friend WithEvents lstProviders As RadioDld.ExtListView
    Friend WithEvents imlListIcons As System.Windows.Forms.ImageList
    Friend WithEvents imlProviders As System.Windows.Forms.ImageList
    Friend WithEvents lstSubscribed As RadioDld.ExtListView
    Friend WithEvents prgDldProg As System.Windows.Forms.ProgressBar
    Friend WithEvents lstDownloads As RadioDld.ExtListView
    Friend WithEvents tmrCheckForUpdates As System.Windows.Forms.Timer
    Friend WithEvents tbrToolbar As System.Windows.Forms.ToolStrip
    Friend WithEvents tblInfo As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblSideMainTitle As System.Windows.Forms.Label
    Friend WithEvents picSidebarImg As System.Windows.Forms.PictureBox
    Friend WithEvents lblSideDescript As System.Windows.Forms.Label
    Friend WithEvents tbtPlay As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtCancel As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtSubscribe As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtUnsubscribe As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtDownload As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtDelete As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtRetry As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbmHelp As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents lstEpisodes As RadioDld.ExtListView
    Friend WithEvents tbmMisc As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents tbmOptions As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbmSep1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tbmExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbmShowHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbmAbout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbmReportABug As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tbmSep2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tbtReportError As System.Windows.Forms.ToolStripButton
    Friend WithEvents pnlPluginSpace As System.Windows.Forms.Panel
    Friend WithEvents tbtFavourites As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtBack As System.Windows.Forms.ToolStripButton
    Friend WithEvents tbtForward As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tbtCurrentEps As System.Windows.Forms.ToolStripButton
#End Region
End Class
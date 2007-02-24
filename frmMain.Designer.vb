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
	Public ToolTip1 As System.Windows.Forms.ToolTip
    Public WithEvents tmrResizeHack As System.Windows.Forms.Timer
    Public WithEvents lstDownloads As AxComctlLib.AxListView
    Public WithEvents lstSubscribed As AxComctlLib.AxListView
    Public WithEvents lstNew As AxComctlLib.AxListView
    Public WithEvents tmrCheckSub As System.Windows.Forms.Timer
    Public WithEvents tmrStartProcess As System.Windows.Forms.Timer
    Public WithEvents staStatus As AxComctlLib.AxStatusBar
    Public WithEvents prgItemProgress As AxComctlLib.AxProgressBar
    Public WithEvents webDetails As System.Windows.Forms.WebBrowser
    Public WithEvents imlToolbar As AxComctlLib.AxImageList
    Public WithEvents imlStations As AxComctlLib.AxImageList
    Public WithEvents imlListIcons As AxComctlLib.AxImageList
    Public WithEvents mnuFileExit As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents File As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents mnuToolsPrefs As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents mnuTools As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents mnuHelpAbout As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents mnuHelp As System.Windows.Forms.ToolStripMenuItem
    Public WithEvents mnuMainMenu As System.Windows.Forms.MenuStrip
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.tmrResizeHack = New System.Windows.Forms.Timer(Me.components)
        Me.lstDownloads = New AxComctlLib.AxListView
        Me.lstSubscribed = New AxComctlLib.AxListView
        Me.lstNew = New AxComctlLib.AxListView
        Me.tmrCheckSub = New System.Windows.Forms.Timer(Me.components)
        Me.tmrStartProcess = New System.Windows.Forms.Timer(Me.components)
        Me.staStatus = New AxComctlLib.AxStatusBar
        Me.prgItemProgress = New AxComctlLib.AxProgressBar
        Me.webDetails = New System.Windows.Forms.WebBrowser
        Me.imlToolbar = New AxComctlLib.AxImageList
        Me.imlStations = New AxComctlLib.AxImageList
        Me.imlListIcons = New AxComctlLib.AxImageList
        Me.mnuMainMenu = New System.Windows.Forms.MenuStrip
        Me.File = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuFileExit = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuTools = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuToolsPrefs = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuHelp = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuHelpAbout = New System.Windows.Forms.ToolStripMenuItem
        Me.tbrToolbar = New System.Windows.Forms.ToolStrip
        Me.tsbFindNew = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton3 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.ToolStripButton4 = New System.Windows.Forms.ToolStripButton
        Me.ToolStripButton5 = New System.Windows.Forms.ToolStripButton
        Me.txtSearch = New System.Windows.Forms.TextBox
        Me.nicTrayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.mnuTray = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuTrayShow = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator
        Me.mnuTrayExit = New System.Windows.Forms.ToolStripMenuItem
        CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lstNew, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.staStatus, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlStations, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mnuMainMenu.SuspendLayout()
        Me.tbrToolbar.SuspendLayout()
        Me.mnuTray.SuspendLayout()
        Me.SuspendLayout()
        '
        'tmrResizeHack
        '
        Me.tmrResizeHack.Enabled = True
        Me.tmrResizeHack.Interval = 500
        '
        'lstDownloads
        '
        Me.lstDownloads.Location = New System.Drawing.Point(210, 312)
        Me.lstDownloads.Name = "lstDownloads"
        Me.lstDownloads.OcxState = CType(resources.GetObject("lstDownloads.OcxState"), System.Windows.Forms.AxHost.State)
        Me.lstDownloads.Size = New System.Drawing.Size(545, 129)
        Me.lstDownloads.TabIndex = 0
        '
        'lstSubscribed
        '
        Me.lstSubscribed.Location = New System.Drawing.Point(210, 180)
        Me.lstSubscribed.Name = "lstSubscribed"
        Me.lstSubscribed.OcxState = CType(resources.GetObject("lstSubscribed.OcxState"), System.Windows.Forms.AxHost.State)
        Me.lstSubscribed.Size = New System.Drawing.Size(545, 125)
        Me.lstSubscribed.TabIndex = 5
        '
        'lstNew
        '
        Me.lstNew.Location = New System.Drawing.Point(212, 94)
        Me.lstNew.Name = "lstNew"
        Me.lstNew.OcxState = CType(resources.GetObject("lstNew.OcxState"), System.Windows.Forms.AxHost.State)
        Me.lstNew.Size = New System.Drawing.Size(545, 40)
        Me.lstNew.TabIndex = 4
        '
        'tmrCheckSub
        '
        Me.tmrCheckSub.Enabled = True
        Me.tmrCheckSub.Interval = 60000
        '
        'tmrStartProcess
        '
        Me.tmrStartProcess.Enabled = True
        Me.tmrStartProcess.Interval = 2000
        '
        'staStatus
        '
        Me.staStatus.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.staStatus.Location = New System.Drawing.Point(0, 441)
        Me.staStatus.Name = "staStatus"
        Me.staStatus.OcxState = CType(resources.GetObject("staStatus.OcxState"), System.Windows.Forms.AxHost.State)
        Me.staStatus.Size = New System.Drawing.Size(757, 21)
        Me.staStatus.TabIndex = 3
        '
        'prgItemProgress
        '
        Me.prgItemProgress.Location = New System.Drawing.Point(408, 140)
        Me.prgItemProgress.Name = "prgItemProgress"
        Me.prgItemProgress.OcxState = CType(resources.GetObject("prgItemProgress.OcxState"), System.Windows.Forms.AxHost.State)
        Me.prgItemProgress.Size = New System.Drawing.Size(129, 21)
        Me.prgItemProgress.TabIndex = 1
        Me.prgItemProgress.Visible = False
        '
        'webDetails
        '
        Me.webDetails.Location = New System.Drawing.Point(0, 52)
        Me.webDetails.Name = "webDetails"
        Me.webDetails.Size = New System.Drawing.Size(210, 389)
        Me.webDetails.TabIndex = 2
        '
        'imlToolbar
        '
        Me.imlToolbar.Enabled = True
        Me.imlToolbar.Location = New System.Drawing.Point(292, 140)
        Me.imlToolbar.Name = "imlToolbar"
        Me.imlToolbar.OcxState = CType(resources.GetObject("imlToolbar.OcxState"), System.Windows.Forms.AxHost.State)
        Me.imlToolbar.Size = New System.Drawing.Size(38, 38)
        Me.imlToolbar.TabIndex = 7
        '
        'imlStations
        '
        Me.imlStations.Enabled = True
        Me.imlStations.Location = New System.Drawing.Point(252, 140)
        Me.imlStations.Name = "imlStations"
        Me.imlStations.OcxState = CType(resources.GetObject("imlStations.OcxState"), System.Windows.Forms.AxHost.State)
        Me.imlStations.Size = New System.Drawing.Size(38, 38)
        Me.imlStations.TabIndex = 8
        '
        'imlListIcons
        '
        Me.imlListIcons.Enabled = True
        Me.imlListIcons.Location = New System.Drawing.Point(212, 140)
        Me.imlListIcons.Name = "imlListIcons"
        Me.imlListIcons.OcxState = CType(resources.GetObject("imlListIcons.OcxState"), System.Windows.Forms.AxHost.State)
        Me.imlListIcons.Size = New System.Drawing.Size(38, 38)
        Me.imlListIcons.TabIndex = 9
        '
        'mnuMainMenu
        '
        Me.mnuMainMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.File, Me.mnuTools, Me.mnuHelp})
        Me.mnuMainMenu.Location = New System.Drawing.Point(0, 0)
        Me.mnuMainMenu.Name = "mnuMainMenu"
        Me.mnuMainMenu.Size = New System.Drawing.Size(757, 24)
        Me.mnuMainMenu.TabIndex = 10
        '
        'File
        '
        Me.File.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFileExit})
        Me.File.Name = "File"
        Me.File.Size = New System.Drawing.Size(37, 20)
        Me.File.Text = "&File"
        '
        'mnuFileExit
        '
        Me.mnuFileExit.Name = "mnuFileExit"
        Me.mnuFileExit.Size = New System.Drawing.Size(92, 22)
        Me.mnuFileExit.Text = "E&xit"
        '
        'mnuTools
        '
        Me.mnuTools.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuToolsPrefs})
        Me.mnuTools.Name = "mnuTools"
        Me.mnuTools.Size = New System.Drawing.Size(48, 20)
        Me.mnuTools.Text = "&Tools"
        '
        'mnuToolsPrefs
        '
        Me.mnuToolsPrefs.Name = "mnuToolsPrefs"
        Me.mnuToolsPrefs.Size = New System.Drawing.Size(135, 22)
        Me.mnuToolsPrefs.Text = "&Preferences"
        '
        'mnuHelp
        '
        Me.mnuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuHelpAbout})
        Me.mnuHelp.Name = "mnuHelp"
        Me.mnuHelp.Size = New System.Drawing.Size(44, 20)
        Me.mnuHelp.Text = "&Help"
        '
        'mnuHelpAbout
        '
        Me.mnuHelpAbout.Name = "mnuHelpAbout"
        Me.mnuHelpAbout.Size = New System.Drawing.Size(107, 22)
        Me.mnuHelpAbout.Text = "&About"
        '
        'tbrToolbar
        '
        Me.tbrToolbar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbFindNew, Me.ToolStripButton2, Me.ToolStripButton3, Me.ToolStripSeparator1, Me.ToolStripButton1, Me.ToolStripSeparator2, Me.ToolStripButton4, Me.ToolStripButton5})
        Me.tbrToolbar.Location = New System.Drawing.Point(0, 24)
        Me.tbrToolbar.Name = "tbrToolbar"
        Me.tbrToolbar.Size = New System.Drawing.Size(757, 25)
        Me.tbrToolbar.TabIndex = 11
        '
        'tsbFindNew
        '
        Me.tsbFindNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbFindNew.Image = CType(resources.GetObject("tsbFindNew.Image"), System.Drawing.Image)
        Me.tsbFindNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbFindNew.Name = "tsbFindNew"
        Me.tsbFindNew.Size = New System.Drawing.Size(23, 22)
        Me.tsbFindNew.Text = "ToolStripButton1"
        '
        'ToolStripButton2
        '
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), System.Drawing.Image)
        Me.ToolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton2.Name = "ToolStripButton2"
        Me.ToolStripButton2.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton2.Text = "ToolStripButton2"
        '
        'ToolStripButton3
        '
        Me.ToolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton3.Image = CType(resources.GetObject("ToolStripButton3.Image"), System.Drawing.Image)
        Me.ToolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton3.Name = "ToolStripButton3"
        Me.ToolStripButton3.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton3.Text = "ToolStripButton3"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton1.Text = "ToolStripButton1"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripButton4
        '
        Me.ToolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton4.Image = CType(resources.GetObject("ToolStripButton4.Image"), System.Drawing.Image)
        Me.ToolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton4.Name = "ToolStripButton4"
        Me.ToolStripButton4.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton4.Text = "ToolStripButton4"
        '
        'ToolStripButton5
        '
        Me.ToolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton5.Image = CType(resources.GetObject("ToolStripButton5.Image"), System.Drawing.Image)
        Me.ToolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton5.Name = "ToolStripButton5"
        Me.ToolStripButton5.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton5.Text = "ToolStripButton5"
        '
        'txtSearch
        '
        Me.txtSearch.AcceptsReturn = True
        Me.txtSearch.BackColor = System.Drawing.SystemColors.Window
        Me.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtSearch.Enabled = False
        Me.txtSearch.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSearch.Location = New System.Drawing.Point(304, 221)
        Me.txtSearch.MaxLength = 0
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtSearch.Size = New System.Drawing.Size(149, 21)
        Me.txtSearch.TabIndex = 12
        Me.txtSearch.Text = "Search..."
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
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(757, 462)
        Me.Controls.Add(Me.txtSearch)
        Me.Controls.Add(Me.tbrToolbar)
        Me.Controls.Add(Me.lstDownloads)
        Me.Controls.Add(Me.lstSubscribed)
        Me.Controls.Add(Me.lstNew)
        Me.Controls.Add(Me.staStatus)
        Me.Controls.Add(Me.prgItemProgress)
        Me.Controls.Add(Me.webDetails)
        Me.Controls.Add(Me.imlToolbar)
        Me.Controls.Add(Me.imlStations)
        Me.Controls.Add(Me.imlListIcons)
        Me.Controls.Add(Me.mnuMainMenu)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(11, 37)
        Me.Name = "frmMain"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lstNew, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.staStatus, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlStations, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mnuMainMenu.ResumeLayout(False)
        Me.mnuMainMenu.PerformLayout()
        Me.tbrToolbar.ResumeLayout(False)
        Me.tbrToolbar.PerformLayout()
        Me.mnuTray.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tbrToolbar As System.Windows.Forms.ToolStrip
    Friend WithEvents tsbFindNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton3 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButton4 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton5 As System.Windows.Forms.ToolStripButton
    Public WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents nicTrayIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents mnuTray As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuTrayShow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator3 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuTrayExit As System.Windows.Forms.ToolStripMenuItem
#End Region 
End Class
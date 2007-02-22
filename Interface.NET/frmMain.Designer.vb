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
	Public WithEvents imgShadow As System.Windows.Forms.PictureBox
	Public WithEvents picShadow As System.Windows.Forms.Panel
	Public WithEvents _picSeperator_0 As System.Windows.Forms.PictureBox
	Public WithEvents txtSearch As System.Windows.Forms.TextBox
	Public WithEvents _picSeperator_1 As System.Windows.Forms.PictureBox
	Public WithEvents tbrToolbar As AxComctlLib.AxToolbar
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
	Public WithEvents picSeperator As Microsoft.VisualBasic.Compatibility.VB6.PictureBoxArray
	Public WithEvents mnuFileExit As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents File As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuToolsPrefs As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuTools As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuHelpAbout As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuHelp As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuTrayShow As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuTraySpacer As System.Windows.Forms.ToolStripSeparator
	Public WithEvents mnuTrayExit As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents mnuTray As System.Windows.Forms.ToolStripMenuItem
	Public WithEvents MainMenu1 As System.Windows.Forms.MenuStrip
	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.tbrToolbar = New AxComctlLib.AxToolbar
        Me.picShadow = New System.Windows.Forms.Panel
        Me.imgShadow = New System.Windows.Forms.PictureBox
        Me._picSeperator_0 = New System.Windows.Forms.PictureBox
        Me.txtSearch = New System.Windows.Forms.TextBox
        Me._picSeperator_1 = New System.Windows.Forms.PictureBox
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
        Me.picSeperator = New Microsoft.VisualBasic.Compatibility.VB6.PictureBoxArray(Me.components)
        Me.MainMenu1 = New System.Windows.Forms.MenuStrip
        Me.File = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuFileExit = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuTools = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuToolsPrefs = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuHelp = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuHelpAbout = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuTray = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuTrayShow = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuTraySpacer = New System.Windows.Forms.ToolStripSeparator
        Me.mnuTrayExit = New System.Windows.Forms.ToolStripMenuItem
        CType(Me.tbrToolbar, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbrToolbar.SuspendLayout()
        Me.picShadow.SuspendLayout()
        CType(Me.imgShadow, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me._picSeperator_0, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me._picSeperator_1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lstNew, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.staStatus, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlStations, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picSeperator, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MainMenu1.SuspendLayout()
        Me.SuspendLayout()
        '
        'tbrToolbar
        '
        Me.tbrToolbar.Controls.Add(Me.picShadow)
        Me.tbrToolbar.Controls.Add(Me._picSeperator_0)
        Me.tbrToolbar.Controls.Add(Me.txtSearch)
        Me.tbrToolbar.Controls.Add(Me._picSeperator_1)
        Me.tbrToolbar.Dock = System.Windows.Forms.DockStyle.Top
        Me.tbrToolbar.Location = New System.Drawing.Point(0, 24)
        Me.tbrToolbar.Name = "tbrToolbar"
        Me.tbrToolbar.OcxState = CType(resources.GetObject("tbrToolbar.OcxState"), System.Windows.Forms.AxHost.State)
        Me.tbrToolbar.Size = New System.Drawing.Size(757, 42)
        Me.tbrToolbar.TabIndex = 6
        '
        'picShadow
        '
        Me.picShadow.BackColor = System.Drawing.SystemColors.Control
        Me.picShadow.Controls.Add(Me.imgShadow)
        Me.picShadow.Cursor = System.Windows.Forms.Cursors.Default
        Me.picShadow.ForeColor = System.Drawing.SystemColors.ControlText
        Me.picShadow.Location = New System.Drawing.Point(0, 38)
        Me.picShadow.Name = "picShadow"
        Me.picShadow.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.picShadow.Size = New System.Drawing.Size(293, 4)
        Me.picShadow.TabIndex = 9
        Me.picShadow.TabStop = True
        '
        'imgShadow
        '
        Me.imgShadow.Cursor = System.Windows.Forms.Cursors.Default
        Me.imgShadow.Image = CType(resources.GetObject("imgShadow.Image"), System.Drawing.Image)
        Me.imgShadow.Location = New System.Drawing.Point(0, 0)
        Me.imgShadow.Name = "imgShadow"
        Me.imgShadow.Size = New System.Drawing.Size(666, 3)
        Me.imgShadow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.imgShadow.TabIndex = 0
        Me.imgShadow.TabStop = False
        '
        '_picSeperator_0
        '
        Me._picSeperator_0.BackColor = System.Drawing.Color.FromArgb(CType(CType(202, Byte), Integer), CType(CType(198, Byte), Integer), CType(CType(175, Byte), Integer))
        Me._picSeperator_0.Cursor = System.Windows.Forms.Cursors.Default
        Me._picSeperator_0.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picSeperator.SetIndex(Me._picSeperator_0, CType(0, Short))
        Me._picSeperator_0.Location = New System.Drawing.Point(216, 4)
        Me._picSeperator_0.Name = "_picSeperator_0"
        Me._picSeperator_0.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._picSeperator_0.Size = New System.Drawing.Size(2, 36)
        Me._picSeperator_0.TabIndex = 8
        Me._picSeperator_0.TabStop = False
        '
        'txtSearch
        '
        Me.txtSearch.AcceptsReturn = True
        Me.txtSearch.BackColor = System.Drawing.SystemColors.Window
        Me.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtSearch.Enabled = False
        Me.txtSearch.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSearch.Location = New System.Drawing.Point(604, 8)
        Me.txtSearch.MaxLength = 0
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtSearch.Size = New System.Drawing.Size(149, 21)
        Me.txtSearch.TabIndex = 7
        Me.txtSearch.Text = "Search..."
        '
        '_picSeperator_1
        '
        Me._picSeperator_1.BackColor = System.Drawing.Color.FromArgb(CType(CType(202, Byte), Integer), CType(CType(198, Byte), Integer), CType(CType(175, Byte), Integer))
        Me._picSeperator_1.Cursor = System.Windows.Forms.Cursors.Default
        Me._picSeperator_1.ForeColor = System.Drawing.SystemColors.WindowText
        Me.picSeperator.SetIndex(Me._picSeperator_1, CType(1, Short))
        Me._picSeperator_1.Location = New System.Drawing.Point(428, 4)
        Me._picSeperator_1.Name = "_picSeperator_1"
        Me._picSeperator_1.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me._picSeperator_1.Size = New System.Drawing.Size(2, 36)
        Me._picSeperator_1.TabIndex = 10
        Me._picSeperator_1.TabStop = False
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
        Me.lstNew.Location = New System.Drawing.Point(210, 40)
        Me.lstNew.Name = "lstNew"
        Me.lstNew.OcxState = CType(resources.GetObject("lstNew.OcxState"), System.Windows.Forms.AxHost.State)
        Me.lstNew.Size = New System.Drawing.Size(545, 89)
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
        Me.webDetails.Location = New System.Drawing.Point(0, 40)
        Me.webDetails.Name = "webDetails"
        Me.webDetails.Size = New System.Drawing.Size(210, 401)
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
        'MainMenu1
        '
        Me.MainMenu1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.File, Me.mnuTools, Me.mnuHelp, Me.mnuTray})
        Me.MainMenu1.Location = New System.Drawing.Point(0, 0)
        Me.MainMenu1.Name = "MainMenu1"
        Me.MainMenu1.Size = New System.Drawing.Size(757, 24)
        Me.MainMenu1.TabIndex = 10
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
        'mnuTray
        '
        Me.mnuTray.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuTrayShow, Me.mnuTraySpacer, Me.mnuTrayExit})
        Me.mnuTray.Name = "mnuTray"
        Me.mnuTray.Size = New System.Drawing.Size(42, 20)
        Me.mnuTray.Text = "Tray"
        Me.mnuTray.Visible = False
        '
        'mnuTrayShow
        '
        Me.mnuTrayShow.Name = "mnuTrayShow"
        Me.mnuTrayShow.Size = New System.Drawing.Size(203, 22)
        Me.mnuTrayShow.Text = "&Show Radio Downloader"
        '
        'mnuTraySpacer
        '
        Me.mnuTraySpacer.Name = "mnuTraySpacer"
        Me.mnuTraySpacer.Size = New System.Drawing.Size(200, 6)
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
        Me.Controls.Add(Me.MainMenu1)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Location = New System.Drawing.Point(11, 37)
        Me.Name = "frmMain"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        CType(Me.tbrToolbar, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbrToolbar.ResumeLayout(False)
        Me.tbrToolbar.PerformLayout()
        Me.picShadow.ResumeLayout(False)
        CType(Me.imgShadow, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me._picSeperator_0, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me._picSeperator_1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lstNew, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.staStatus, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlStations, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picSeperator, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MainMenu1.ResumeLayout(False)
        Me.MainMenu1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region 
End Class
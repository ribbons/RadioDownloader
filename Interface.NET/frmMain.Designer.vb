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
		Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(frmMain))
		Me.components = New System.ComponentModel.Container()
		Me.ToolTip1 = New System.Windows.Forms.ToolTip(components)
		Me.tbrToolbar = New AxComctlLib.AxToolbar
		Me.picShadow = New System.Windows.Forms.Panel
		Me.imgShadow = New System.Windows.Forms.PictureBox
		Me._picSeperator_0 = New System.Windows.Forms.PictureBox
		Me.txtSearch = New System.Windows.Forms.TextBox
		Me._picSeperator_1 = New System.Windows.Forms.PictureBox
		Me.tmrResizeHack = New System.Windows.Forms.Timer(components)
		Me.lstDownloads = New AxComctlLib.AxListView
		Me.lstSubscribed = New AxComctlLib.AxListView
		Me.lstNew = New AxComctlLib.AxListView
		Me.tmrCheckSub = New System.Windows.Forms.Timer(components)
		Me.tmrStartProcess = New System.Windows.Forms.Timer(components)
		Me.staStatus = New AxComctlLib.AxStatusBar
		Me.prgItemProgress = New AxComctlLib.AxProgressBar
		Me.webDetails = New System.Windows.Forms.WebBrowser
		Me.imlToolbar = New AxComctlLib.AxImageList
		Me.imlStations = New AxComctlLib.AxImageList
		Me.imlListIcons = New AxComctlLib.AxImageList
		Me.picSeperator = New Microsoft.VisualBasic.Compatibility.VB6.PictureBoxArray(components)
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
		Me.tbrToolbar.SuspendLayout()
		Me.picShadow.SuspendLayout()
		Me.MainMenu1.SuspendLayout()
		Me.SuspendLayout()
		Me.ToolTip1.Active = True
		CType(Me.tbrToolbar, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.lstNew, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.staStatus, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.imlStations, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).BeginInit()
		CType(Me.picSeperator, System.ComponentModel.ISupportInitialize).BeginInit()
		Me.Text = "Radio Downloader"
		Me.ClientSize = New System.Drawing.Size(757, 462)
		Me.Location = New System.Drawing.Point(11, 37)
		Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.BackColor = System.Drawing.SystemColors.Control
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
		Me.ControlBox = True
		Me.Enabled = True
		Me.KeyPreview = False
		Me.MaximizeBox = True
		Me.MinimizeBox = True
		Me.Cursor = System.Windows.Forms.Cursors.Default
		Me.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.ShowInTaskbar = True
		Me.HelpButton = False
		Me.WindowState = System.Windows.Forms.FormWindowState.Normal
		Me.Name = "frmMain"
		tbrToolbar.OcxState = CType(resources.GetObject("tbrToolbar.OcxState"), System.Windows.Forms.AxHost.State)
		Me.tbrToolbar.Dock = System.Windows.Forms.DockStyle.Top
		Me.tbrToolbar.Size = New System.Drawing.Size(757, 40)
		Me.tbrToolbar.Location = New System.Drawing.Point(0, 0)
		Me.tbrToolbar.TabIndex = 6
		Me.tbrToolbar.Name = "tbrToolbar"
		Me.picShadow.Size = New System.Drawing.Size(293, 4)
		Me.picShadow.Location = New System.Drawing.Point(0, 38)
		Me.picShadow.TabIndex = 9
		Me.picShadow.Dock = System.Windows.Forms.DockStyle.None
		Me.picShadow.BackColor = System.Drawing.SystemColors.Control
		Me.picShadow.CausesValidation = True
		Me.picShadow.Enabled = True
		Me.picShadow.ForeColor = System.Drawing.SystemColors.ControlText
		Me.picShadow.Cursor = System.Windows.Forms.Cursors.Default
		Me.picShadow.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.picShadow.TabStop = True
		Me.picShadow.Visible = True
		Me.picShadow.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me.picShadow.Name = "picShadow"
		Me.imgShadow.Size = New System.Drawing.Size(666, 3)
		Me.imgShadow.Location = New System.Drawing.Point(0, 0)
		Me.imgShadow.Image = CType(resources.GetObject("imgShadow.Image"), System.Drawing.Image)
		Me.imgShadow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
		Me.imgShadow.Enabled = True
		Me.imgShadow.Cursor = System.Windows.Forms.Cursors.Default
		Me.imgShadow.Visible = True
		Me.imgShadow.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me.imgShadow.Name = "imgShadow"
		Me._picSeperator_0.BackColor = System.Drawing.Color.FromARGB(202, 198, 175)
		Me._picSeperator_0.ForeColor = System.Drawing.SystemColors.WindowText
		Me._picSeperator_0.Size = New System.Drawing.Size(2, 36)
		Me._picSeperator_0.Location = New System.Drawing.Point(216, 4)
		Me._picSeperator_0.TabIndex = 8
		Me._picSeperator_0.Dock = System.Windows.Forms.DockStyle.None
		Me._picSeperator_0.CausesValidation = True
		Me._picSeperator_0.Enabled = True
		Me._picSeperator_0.Cursor = System.Windows.Forms.Cursors.Default
		Me._picSeperator_0.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me._picSeperator_0.TabStop = True
		Me._picSeperator_0.Visible = True
		Me._picSeperator_0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal
		Me._picSeperator_0.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me._picSeperator_0.Name = "_picSeperator_0"
		Me.txtSearch.AutoSize = False
		Me.txtSearch.Enabled = False
		Me.txtSearch.Size = New System.Drawing.Size(149, 21)
		Me.txtSearch.Location = New System.Drawing.Point(604, 8)
		Me.txtSearch.TabIndex = 7
		Me.txtSearch.Text = "Search..."
		Me.txtSearch.AcceptsReturn = True
		Me.txtSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Left
		Me.txtSearch.BackColor = System.Drawing.SystemColors.Window
		Me.txtSearch.CausesValidation = True
		Me.txtSearch.ForeColor = System.Drawing.SystemColors.WindowText
		Me.txtSearch.HideSelection = True
		Me.txtSearch.ReadOnly = False
		Me.txtSearch.Maxlength = 0
		Me.txtSearch.Cursor = System.Windows.Forms.Cursors.IBeam
		Me.txtSearch.MultiLine = False
		Me.txtSearch.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.txtSearch.ScrollBars = System.Windows.Forms.ScrollBars.None
		Me.txtSearch.TabStop = True
		Me.txtSearch.Visible = True
		Me.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.txtSearch.Name = "txtSearch"
		Me._picSeperator_1.BackColor = System.Drawing.Color.FromARGB(202, 198, 175)
		Me._picSeperator_1.ForeColor = System.Drawing.SystemColors.WindowText
		Me._picSeperator_1.Size = New System.Drawing.Size(2, 36)
		Me._picSeperator_1.Location = New System.Drawing.Point(428, 4)
		Me._picSeperator_1.TabIndex = 10
		Me._picSeperator_1.Dock = System.Windows.Forms.DockStyle.None
		Me._picSeperator_1.CausesValidation = True
		Me._picSeperator_1.Enabled = True
		Me._picSeperator_1.Cursor = System.Windows.Forms.Cursors.Default
		Me._picSeperator_1.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me._picSeperator_1.TabStop = True
		Me._picSeperator_1.Visible = True
		Me._picSeperator_1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Normal
		Me._picSeperator_1.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me._picSeperator_1.Name = "_picSeperator_1"
		Me.tmrResizeHack.Interval = 500
		Me.tmrResizeHack.Enabled = True
		lstDownloads.OcxState = CType(resources.GetObject("lstDownloads.OcxState"), System.Windows.Forms.AxHost.State)
		Me.lstDownloads.Size = New System.Drawing.Size(545, 129)
		Me.lstDownloads.Location = New System.Drawing.Point(210, 312)
		Me.lstDownloads.TabIndex = 0
		Me.lstDownloads.Name = "lstDownloads"
		lstSubscribed.OcxState = CType(resources.GetObject("lstSubscribed.OcxState"), System.Windows.Forms.AxHost.State)
		Me.lstSubscribed.Size = New System.Drawing.Size(545, 125)
		Me.lstSubscribed.Location = New System.Drawing.Point(210, 180)
		Me.lstSubscribed.TabIndex = 5
		Me.lstSubscribed.Name = "lstSubscribed"
		lstNew.OcxState = CType(resources.GetObject("lstNew.OcxState"), System.Windows.Forms.AxHost.State)
		Me.lstNew.Size = New System.Drawing.Size(545, 89)
		Me.lstNew.Location = New System.Drawing.Point(210, 40)
		Me.lstNew.TabIndex = 4
		Me.lstNew.Name = "lstNew"
		Me.tmrCheckSub.Interval = 60000
		Me.tmrCheckSub.Enabled = True
		Me.tmrStartProcess.Interval = 2000
		Me.tmrStartProcess.Enabled = True
		staStatus.OcxState = CType(resources.GetObject("staStatus.OcxState"), System.Windows.Forms.AxHost.State)
		Me.staStatus.Dock = System.Windows.Forms.DockStyle.Bottom
		Me.staStatus.Size = New System.Drawing.Size(757, 21)
		Me.staStatus.Location = New System.Drawing.Point(0, 441)
		Me.staStatus.TabIndex = 3
		Me.staStatus.Name = "staStatus"
		prgItemProgress.OcxState = CType(resources.GetObject("prgItemProgress.OcxState"), System.Windows.Forms.AxHost.State)
		Me.prgItemProgress.Size = New System.Drawing.Size(129, 21)
		Me.prgItemProgress.Location = New System.Drawing.Point(408, 140)
		Me.prgItemProgress.TabIndex = 1
		Me.prgItemProgress.Visible = False
		Me.prgItemProgress.Name = "prgItemProgress"
		Me.webDetails.Size = New System.Drawing.Size(210, 401)
		Me.webDetails.Location = New System.Drawing.Point(0, 40)
		Me.webDetails.TabIndex = 2
		Me.webDetails.AllowWebBrowserDrop = True
		Me.webDetails.Name = "webDetails"
		imlToolbar.OcxState = CType(resources.GetObject("imlToolbar.OcxState"), System.Windows.Forms.AxHost.State)
		Me.imlToolbar.Location = New System.Drawing.Point(292, 140)
		Me.imlToolbar.Name = "imlToolbar"
		imlStations.OcxState = CType(resources.GetObject("imlStations.OcxState"), System.Windows.Forms.AxHost.State)
		Me.imlStations.Location = New System.Drawing.Point(252, 140)
		Me.imlStations.Name = "imlStations"
		imlListIcons.OcxState = CType(resources.GetObject("imlListIcons.OcxState"), System.Windows.Forms.AxHost.State)
		Me.imlListIcons.Location = New System.Drawing.Point(212, 140)
		Me.imlListIcons.Name = "imlListIcons"
		Me.File.Name = "File"
		Me.File.Text = "&File"
		Me.File.Checked = False
		Me.File.Enabled = True
		Me.File.Visible = True
		Me.mnuFileExit.Name = "mnuFileExit"
		Me.mnuFileExit.Text = "E&xit"
		Me.mnuFileExit.Checked = False
		Me.mnuFileExit.Enabled = True
		Me.mnuFileExit.Visible = True
		Me.mnuTools.Name = "mnuTools"
		Me.mnuTools.Text = "&Tools"
		Me.mnuTools.Checked = False
		Me.mnuTools.Enabled = True
		Me.mnuTools.Visible = True
		Me.mnuToolsPrefs.Name = "mnuToolsPrefs"
		Me.mnuToolsPrefs.Text = "&Preferences"
		Me.mnuToolsPrefs.Checked = False
		Me.mnuToolsPrefs.Enabled = True
		Me.mnuToolsPrefs.Visible = True
		Me.mnuHelp.Name = "mnuHelp"
		Me.mnuHelp.Text = "&Help"
		Me.mnuHelp.Checked = False
		Me.mnuHelp.Enabled = True
		Me.mnuHelp.Visible = True
		Me.mnuHelpAbout.Name = "mnuHelpAbout"
		Me.mnuHelpAbout.Text = "&About"
		Me.mnuHelpAbout.Checked = False
		Me.mnuHelpAbout.Enabled = True
		Me.mnuHelpAbout.Visible = True
		Me.mnuTray.Name = "mnuTray"
		Me.mnuTray.Text = "Tray"
		Me.mnuTray.Visible = False
		Me.mnuTray.Checked = False
		Me.mnuTray.Enabled = True
		Me.mnuTrayShow.Name = "mnuTrayShow"
		Me.mnuTrayShow.Text = "&Show Radio Downloader"
		Me.mnuTrayShow.Checked = False
		Me.mnuTrayShow.Enabled = True
		Me.mnuTrayShow.Visible = True
		Me.mnuTraySpacer.Enabled = True
		Me.mnuTraySpacer.Visible = True
		Me.mnuTraySpacer.Name = "mnuTraySpacer"
		Me.mnuTrayExit.Name = "mnuTrayExit"
		Me.mnuTrayExit.Text = "E&xit"
		Me.mnuTrayExit.Checked = False
		Me.mnuTrayExit.Enabled = True
		Me.mnuTrayExit.Visible = True
		Me.Controls.Add(tbrToolbar)
		Me.Controls.Add(lstDownloads)
		Me.Controls.Add(lstSubscribed)
		Me.Controls.Add(lstNew)
		Me.Controls.Add(staStatus)
		Me.Controls.Add(prgItemProgress)
		Me.Controls.Add(webDetails)
		Me.Controls.Add(imlToolbar)
		Me.Controls.Add(imlStations)
		Me.Controls.Add(imlListIcons)
		Me.tbrToolbar.Controls.Add(picShadow)
		Me.tbrToolbar.Controls.Add(_picSeperator_0)
		Me.tbrToolbar.Controls.Add(txtSearch)
		Me.tbrToolbar.Controls.Add(_picSeperator_1)
		Me.picShadow.Controls.Add(imgShadow)
		Me.picSeperator.SetIndex(_picSeperator_0, CType(0, Short))
		Me.picSeperator.SetIndex(_picSeperator_1, CType(1, Short))
		CType(Me.picSeperator, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.imlListIcons, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.imlStations, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.imlToolbar, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.prgItemProgress, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.staStatus, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.lstNew, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.lstSubscribed, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.lstDownloads, System.ComponentModel.ISupportInitialize).EndInit()
		CType(Me.tbrToolbar, System.ComponentModel.ISupportInitialize).EndInit()
		MainMenu1.Items.AddRange(New System.Windows.Forms.ToolStripItem(){Me.File, Me.mnuTools, Me.mnuHelp, Me.mnuTray})
		File.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem(){Me.mnuFileExit})
		mnuTools.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem(){Me.mnuToolsPrefs})
		mnuHelp.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem(){Me.mnuHelpAbout})
		mnuTray.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem(){Me.mnuTrayShow, Me.mnuTraySpacer, Me.mnuTrayExit})
		Me.Controls.Add(MainMenu1)
		Me.tbrToolbar.ResumeLayout(False)
		Me.picShadow.ResumeLayout(False)
		Me.MainMenu1.ResumeLayout(False)
		Me.ResumeLayout(False)
		Me.PerformLayout()
	End Sub
#End Region 
End Class
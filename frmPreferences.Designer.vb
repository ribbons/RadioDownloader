<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class frmPreferences
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
	Public WithEvents cmdCancel As System.Windows.Forms.Button
	Public WithEvents cmdOK As System.Windows.Forms.Button
	Public WithEvents cmdChangeFolder As System.Windows.Forms.Button
	Public WithEvents txtSaveIn As System.Windows.Forms.TextBox
	Public WithEvents lblSaveIn As System.Windows.Forms.Label
	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
		Dim resources As System.Resources.ResourceManager = New System.Resources.ResourceManager(GetType(frmPreferences))
		Me.components = New System.ComponentModel.Container()
		Me.ToolTip1 = New System.Windows.Forms.ToolTip(components)
		Me.cmdCancel = New System.Windows.Forms.Button
		Me.cmdOK = New System.Windows.Forms.Button
		Me.cmdChangeFolder = New System.Windows.Forms.Button
		Me.txtSaveIn = New System.Windows.Forms.TextBox
		Me.lblSaveIn = New System.Windows.Forms.Label
		Me.SuspendLayout()
		Me.ToolTip1.Active = True
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.Text = "Options"
		Me.ClientSize = New System.Drawing.Size(413, 242)
		Me.Location = New System.Drawing.Point(3, 29)
		Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
		Me.MaximizeBox = False
		Me.MinimizeBox = False
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.BackColor = System.Drawing.SystemColors.Control
		Me.ControlBox = True
		Me.Enabled = True
		Me.KeyPreview = False
		Me.Cursor = System.Windows.Forms.Cursors.Default
		Me.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.ShowInTaskbar = True
		Me.HelpButton = False
		Me.WindowState = System.Windows.Forms.FormWindowState.Normal
		Me.Name = "frmPreferences"
		Me.cmdCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.cmdCancel.Text = "Cancel"
		Me.cmdCancel.Size = New System.Drawing.Size(77, 25)
		Me.cmdCancel.Location = New System.Drawing.Point(320, 204)
		Me.cmdCancel.TabIndex = 4
		Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
		Me.cmdCancel.CausesValidation = True
		Me.cmdCancel.Enabled = True
		Me.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText
		Me.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default
		Me.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.cmdCancel.TabStop = True
		Me.cmdCancel.Name = "cmdCancel"
		Me.cmdOK.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.cmdOK.Text = "OK"
		Me.cmdOK.Size = New System.Drawing.Size(77, 25)
		Me.cmdOK.Location = New System.Drawing.Point(236, 204)
		Me.cmdOK.TabIndex = 3
		Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
		Me.cmdOK.CausesValidation = True
		Me.cmdOK.Enabled = True
		Me.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText
		Me.cmdOK.Cursor = System.Windows.Forms.Cursors.Default
		Me.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.cmdOK.TabStop = True
		Me.cmdOK.Name = "cmdOK"
		Me.cmdChangeFolder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		Me.cmdChangeFolder.Text = "Change"
		Me.cmdChangeFolder.Size = New System.Drawing.Size(73, 25)
		Me.cmdChangeFolder.Location = New System.Drawing.Point(324, 56)
		Me.cmdChangeFolder.TabIndex = 2
		Me.cmdChangeFolder.BackColor = System.Drawing.SystemColors.Control
		Me.cmdChangeFolder.CausesValidation = True
		Me.cmdChangeFolder.Enabled = True
		Me.cmdChangeFolder.ForeColor = System.Drawing.SystemColors.ControlText
		Me.cmdChangeFolder.Cursor = System.Windows.Forms.Cursors.Default
		Me.cmdChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.cmdChangeFolder.TabStop = True
		Me.cmdChangeFolder.Name = "cmdChangeFolder"
		Me.txtSaveIn.AutoSize = False
		Me.txtSaveIn.Size = New System.Drawing.Size(305, 25)
		Me.txtSaveIn.Location = New System.Drawing.Point(12, 56)
		Me.txtSaveIn.ReadOnly = True
		Me.txtSaveIn.TabIndex = 0
		Me.txtSaveIn.AcceptsReturn = True
		Me.txtSaveIn.TextAlign = System.Windows.Forms.HorizontalAlignment.Left
		Me.txtSaveIn.BackColor = System.Drawing.SystemColors.Window
		Me.txtSaveIn.CausesValidation = True
		Me.txtSaveIn.Enabled = True
		Me.txtSaveIn.ForeColor = System.Drawing.SystemColors.WindowText
		Me.txtSaveIn.HideSelection = True
		Me.txtSaveIn.Maxlength = 0
		Me.txtSaveIn.Cursor = System.Windows.Forms.Cursors.IBeam
		Me.txtSaveIn.MultiLine = False
		Me.txtSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.txtSaveIn.ScrollBars = System.Windows.Forms.ScrollBars.None
		Me.txtSaveIn.TabStop = True
		Me.txtSaveIn.Visible = True
		Me.txtSaveIn.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
		Me.txtSaveIn.Name = "txtSaveIn"
		Me.lblSaveIn.Text = "Save downloaded programs in:"
		Me.lblSaveIn.Size = New System.Drawing.Size(385, 21)
		Me.lblSaveIn.Location = New System.Drawing.Point(14, 40)
		Me.lblSaveIn.TabIndex = 1
		Me.lblSaveIn.TextAlign = System.Drawing.ContentAlignment.TopLeft
		Me.lblSaveIn.BackColor = System.Drawing.SystemColors.Control
		Me.lblSaveIn.Enabled = True
		Me.lblSaveIn.ForeColor = System.Drawing.SystemColors.ControlText
		Me.lblSaveIn.Cursor = System.Windows.Forms.Cursors.Default
		Me.lblSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
		Me.lblSaveIn.UseMnemonic = True
		Me.lblSaveIn.Visible = True
		Me.lblSaveIn.AutoSize = False
		Me.lblSaveIn.BorderStyle = System.Windows.Forms.BorderStyle.None
		Me.lblSaveIn.Name = "lblSaveIn"
		Me.Controls.Add(cmdCancel)
		Me.Controls.Add(cmdOK)
		Me.Controls.Add(cmdChangeFolder)
		Me.Controls.Add(txtSaveIn)
		Me.Controls.Add(lblSaveIn)
		Me.ResumeLayout(False)
		Me.PerformLayout()
	End Sub
#End Region 
End Class
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class Preferences
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
    Public WithEvents cmdCancel As System.Windows.Forms.Button
	Public WithEvents cmdOK As System.Windows.Forms.Button
	Public WithEvents cmdChangeFolder As System.Windows.Forms.Button
	Public WithEvents txtSaveIn As System.Windows.Forms.TextBox
	Public WithEvents lblSaveIn As System.Windows.Forms.Label
	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.cmdChangeFolder = New System.Windows.Forms.Button()
        Me.txtSaveIn = New System.Windows.Forms.TextBox()
        Me.lblSaveIn = New System.Windows.Forms.Label()
        Me.lblFileNameFormat = New System.Windows.Forms.Label()
        Me.txtFileNameFormat = New System.Windows.Forms.TextBox()
        Me.lblFilenameFormatResult = New System.Windows.Forms.Label()
        Me.cmdReset = New System.Windows.Forms.Button()
        Me.lblRunAfter = New System.Windows.Forms.Label()
        Me.txtRunAfter = New System.Windows.Forms.TextBox()
        Me.lblRunAfterFileDef = New System.Windows.Forms.Label()
        Me.uxRunOnStartup = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
        Me.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdCancel.Location = New System.Drawing.Point(297, 277)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdCancel.Size = New System.Drawing.Size(77, 25)
        Me.cmdCancel.TabIndex = 1
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
        Me.cmdOK.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdOK.Location = New System.Drawing.Point(214, 277)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdOK.Size = New System.Drawing.Size(77, 25)
        Me.cmdOK.TabIndex = 0
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'cmdChangeFolder
        '
        Me.cmdChangeFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdChangeFolder.BackColor = System.Drawing.SystemColors.Control
        Me.cmdChangeFolder.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdChangeFolder.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdChangeFolder.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdChangeFolder.Location = New System.Drawing.Point(384, 74)
        Me.cmdChangeFolder.Name = "cmdChangeFolder"
        Me.cmdChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdChangeFolder.Size = New System.Drawing.Size(73, 25)
        Me.cmdChangeFolder.TabIndex = 6
        Me.cmdChangeFolder.Text = "&Change"
        Me.cmdChangeFolder.UseVisualStyleBackColor = False
        '
        'txtSaveIn
        '
        Me.txtSaveIn.AcceptsReturn = True
        Me.txtSaveIn.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSaveIn.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtSaveIn.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSaveIn.Location = New System.Drawing.Point(37, 77)
        Me.txtSaveIn.Margin = New System.Windows.Forms.Padding(3, 3, 3, 20)
        Me.txtSaveIn.MaxLength = 0
        Me.txtSaveIn.Name = "txtSaveIn"
        Me.txtSaveIn.ReadOnly = True
        Me.txtSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtSaveIn.Size = New System.Drawing.Size(341, 20)
        Me.txtSaveIn.TabIndex = 5
        '
        'lblSaveIn
        '
        Me.lblSaveIn.AutoSize = True
        Me.lblSaveIn.BackColor = System.Drawing.SystemColors.Control
        Me.lblSaveIn.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblSaveIn.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblSaveIn.Location = New System.Drawing.Point(12, 56)
        Me.lblSaveIn.Margin = New System.Windows.Forms.Padding(3, 0, 3, 5)
        Me.lblSaveIn.Name = "lblSaveIn"
        Me.lblSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblSaveIn.Size = New System.Drawing.Size(167, 13)
        Me.lblSaveIn.TabIndex = 4
        Me.lblSaveIn.Text = "Save &downloaded programmes in:"
        '
        'lblFileNameFormat
        '
        Me.lblFileNameFormat.AutoSize = True
        Me.lblFileNameFormat.Location = New System.Drawing.Point(12, 113)
        Me.lblFileNameFormat.Margin = New System.Windows.Forms.Padding(3, 0, 3, 5)
        Me.lblFileNameFormat.Name = "lblFileNameFormat"
        Me.lblFileNameFormat.Size = New System.Drawing.Size(202, 13)
        Me.lblFileNameFormat.TabIndex = 7
        Me.lblFileNameFormat.Text = "Downloaded programme file name &format:"
        '
        'txtFileNameFormat
        '
        Me.txtFileNameFormat.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFileNameFormat.Location = New System.Drawing.Point(37, 134)
        Me.txtFileNameFormat.Name = "txtFileNameFormat"
        Me.txtFileNameFormat.Size = New System.Drawing.Size(420, 20)
        Me.txtFileNameFormat.TabIndex = 8
        '
        'lblFilenameFormatResult
        '
        Me.lblFilenameFormatResult.AutoSize = True
        Me.lblFilenameFormatResult.Location = New System.Drawing.Point(34, 161)
        Me.lblFilenameFormatResult.Margin = New System.Windows.Forms.Padding(3, 3, 3, 17)
        Me.lblFilenameFormatResult.Name = "lblFilenameFormatResult"
        Me.lblFilenameFormatResult.Size = New System.Drawing.Size(40, 13)
        Me.lblFilenameFormatResult.TabIndex = 9
        Me.lblFilenameFormatResult.Text = "Result:"
        '
        'cmdReset
        '
        Me.cmdReset.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdReset.BackColor = System.Drawing.SystemColors.Control
        Me.cmdReset.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdReset.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdReset.Location = New System.Drawing.Point(380, 277)
        Me.cmdReset.Name = "cmdReset"
        Me.cmdReset.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdReset.Size = New System.Drawing.Size(77, 25)
        Me.cmdReset.TabIndex = 2
        Me.cmdReset.Text = "&Reset"
        Me.cmdReset.UseVisualStyleBackColor = False
        '
        'lblRunAfter
        '
        Me.lblRunAfter.AutoSize = True
        Me.lblRunAfter.Location = New System.Drawing.Point(12, 191)
        Me.lblRunAfter.Margin = New System.Windows.Forms.Padding(3, 0, 3, 5)
        Me.lblRunAfter.Name = "lblRunAfter"
        Me.lblRunAfter.Size = New System.Drawing.Size(229, 13)
        Me.lblRunAfter.TabIndex = 10
        Me.lblRunAfter.Text = "Run command &after download (blank for none):"
        '
        'txtRunAfter
        '
        Me.txtRunAfter.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtRunAfter.Location = New System.Drawing.Point(37, 212)
        Me.txtRunAfter.Name = "txtRunAfter"
        Me.txtRunAfter.Size = New System.Drawing.Size(420, 20)
        Me.txtRunAfter.TabIndex = 11
        '
        'lblRunAfterFileDef
        '
        Me.lblRunAfterFileDef.AutoSize = True
        Me.lblRunAfterFileDef.Location = New System.Drawing.Point(34, 239)
        Me.lblRunAfterFileDef.Margin = New System.Windows.Forms.Padding(3, 3, 3, 17)
        Me.lblRunAfterFileDef.Name = "lblRunAfterFileDef"
        Me.lblRunAfterFileDef.Size = New System.Drawing.Size(192, 13)
        Me.lblRunAfterFileDef.TabIndex = 12
        Me.lblRunAfterFileDef.Text = "%file% = full path to the downloaded file"
        '
        'uxRunOnStartup
        '
        Me.uxRunOnStartup.AutoSize = True
        Me.uxRunOnStartup.Location = New System.Drawing.Point(15, 19)
        Me.uxRunOnStartup.Margin = New System.Windows.Forms.Padding(6, 10, 3, 20)
        Me.uxRunOnStartup.Name = "uxRunOnStartup"
        Me.uxRunOnStartup.Size = New System.Drawing.Size(240, 17)
        Me.uxRunOnStartup.TabIndex = 3
        Me.uxRunOnStartup.Text = "Run Radio Downloader on computer &startup?"
        Me.uxRunOnStartup.UseVisualStyleBackColor = True
        '
        'Preferences
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(469, 314)
        Me.Controls.Add(Me.uxRunOnStartup)
        Me.Controls.Add(Me.lblRunAfterFileDef)
        Me.Controls.Add(Me.txtRunAfter)
        Me.Controls.Add(Me.lblRunAfter)
        Me.Controls.Add(Me.cmdReset)
        Me.Controls.Add(Me.lblFilenameFormatResult)
        Me.Controls.Add(Me.txtFileNameFormat)
        Me.Controls.Add(Me.lblFileNameFormat)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdChangeFolder)
        Me.Controls.Add(Me.txtSaveIn)
        Me.Controls.Add(Me.lblSaveIn)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Location = New System.Drawing.Point(3, 29)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Preferences"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Main Options"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblFileNameFormat As System.Windows.Forms.Label
    Friend WithEvents txtFileNameFormat As System.Windows.Forms.TextBox
    Friend WithEvents lblFilenameFormatResult As System.Windows.Forms.Label
    Public WithEvents cmdReset As System.Windows.Forms.Button
    Friend WithEvents lblRunAfter As System.Windows.Forms.Label
    Friend WithEvents txtRunAfter As System.Windows.Forms.TextBox
    Friend WithEvents lblRunAfterFileDef As System.Windows.Forms.Label
    Friend WithEvents uxRunOnStartup As System.Windows.Forms.CheckBox
#End Region
End Class

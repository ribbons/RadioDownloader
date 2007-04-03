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
    Public WithEvents cmdCancel As System.Windows.Forms.Button
	Public WithEvents cmdOK As System.Windows.Forms.Button
	Public WithEvents cmdChangeFolder As System.Windows.Forms.Button
	Public WithEvents txtSaveIn As System.Windows.Forms.TextBox
	Public WithEvents lblSaveIn As System.Windows.Forms.Label
	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPreferences))
        Me.cmdCancel = New System.Windows.Forms.Button
        Me.cmdOK = New System.Windows.Forms.Button
        Me.cmdChangeFolder = New System.Windows.Forms.Button
        Me.txtSaveIn = New System.Windows.Forms.TextBox
        Me.lblSaveIn = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
        Me.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdCancel.Location = New System.Drawing.Point(313, 136)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdCancel.Size = New System.Drawing.Size(77, 25)
        Me.cmdCancel.TabIndex = 2
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
        Me.cmdOK.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdOK.Location = New System.Drawing.Point(229, 136)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdOK.Size = New System.Drawing.Size(77, 25)
        Me.cmdOK.TabIndex = 1
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
        Me.cmdChangeFolder.Location = New System.Drawing.Point(317, 56)
        Me.cmdChangeFolder.Name = "cmdChangeFolder"
        Me.cmdChangeFolder.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdChangeFolder.Size = New System.Drawing.Size(73, 25)
        Me.cmdChangeFolder.TabIndex = 4
        Me.cmdChangeFolder.Text = "Change"
        Me.cmdChangeFolder.UseVisualStyleBackColor = False
        '
        'txtSaveIn
        '
        Me.txtSaveIn.AcceptsReturn = True
        Me.txtSaveIn.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSaveIn.BackColor = System.Drawing.SystemColors.Window
        Me.txtSaveIn.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.txtSaveIn.ForeColor = System.Drawing.SystemColors.WindowText
        Me.txtSaveIn.Location = New System.Drawing.Point(12, 56)
        Me.txtSaveIn.MaxLength = 0
        Me.txtSaveIn.Name = "txtSaveIn"
        Me.txtSaveIn.ReadOnly = True
        Me.txtSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.txtSaveIn.Size = New System.Drawing.Size(298, 21)
        Me.txtSaveIn.TabIndex = 3
        '
        'lblSaveIn
        '
        Me.lblSaveIn.BackColor = System.Drawing.SystemColors.Control
        Me.lblSaveIn.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblSaveIn.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblSaveIn.Location = New System.Drawing.Point(14, 40)
        Me.lblSaveIn.Name = "lblSaveIn"
        Me.lblSaveIn.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblSaveIn.Size = New System.Drawing.Size(385, 21)
        Me.lblSaveIn.TabIndex = 0
        Me.lblSaveIn.Text = "Save downloaded programs in:"
        '
        'frmPreferences
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(406, 174)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdChangeFolder)
        Me.Controls.Add(Me.txtSaveIn)
        Me.Controls.Add(Me.lblSaveIn)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(3, 29)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmPreferences"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Options"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region 
End Class
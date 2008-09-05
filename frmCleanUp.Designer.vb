<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> Partial Class frmCleanUp
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
    Public WithEvents cmdOK As System.Windows.Forms.Button
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCleanUp))
        Me.cmdOK = New System.Windows.Forms.Button
        Me.cmdCancel = New System.Windows.Forms.Button
        Me.lblExplainOrphan = New System.Windows.Forms.Label
        Me.radType = New System.Windows.Forms.RadioButton
        Me.SuspendLayout()
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
        Me.cmdOK.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdOK.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdOK.Location = New System.Drawing.Point(187, 109)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdOK.Size = New System.Drawing.Size(77, 25)
        Me.cmdOK.TabIndex = 2
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
        Me.cmdCancel.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdCancel.ForeColor = System.Drawing.SystemColors.ControlText
        Me.cmdCancel.Location = New System.Drawing.Point(270, 109)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.cmdCancel.Size = New System.Drawing.Size(77, 25)
        Me.cmdCancel.TabIndex = 9
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'lblExplainOrphan
        '
        Me.lblExplainOrphan.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblExplainOrphan.BackColor = System.Drawing.SystemColors.Control
        Me.lblExplainOrphan.Cursor = System.Windows.Forms.Cursors.Default
        Me.lblExplainOrphan.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblExplainOrphan.Location = New System.Drawing.Point(36, 36)
        Me.lblExplainOrphan.Name = "lblExplainOrphan"
        Me.lblExplainOrphan.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.lblExplainOrphan.Size = New System.Drawing.Size(311, 44)
        Me.lblExplainOrphan.TabIndex = 0
        Me.lblExplainOrphan.Text = "This will remove the programmes in your download list for which the audio file ha" & _
            "s been moved or deleted."
        '
        'radType
        '
        Me.radType.AutoSize = True
        Me.radType.Checked = True
        Me.radType.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.radType.Location = New System.Drawing.Point(12, 15)
        Me.radType.Name = "radType"
        Me.radType.Size = New System.Drawing.Size(143, 18)
        Me.radType.TabIndex = 10
        Me.radType.TabStop = True
        Me.radType.Text = "Remove orphan entries"
        Me.radType.UseVisualStyleBackColor = True
        '
        'frmCleanUp
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(359, 146)
        Me.Controls.Add(Me.radType)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.lblExplainOrphan)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(3, 29)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmCleanUp"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Clean Up"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents cmdCancel As System.Windows.Forms.Button
    Public WithEvents lblExplainOrphan As System.Windows.Forms.Label
    Friend WithEvents radType As System.Windows.Forms.RadioButton
#End Region
End Class
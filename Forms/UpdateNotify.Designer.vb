<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UpdateNotify
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UpdateNotify))
        Me.uxMessage = New System.Windows.Forms.Label()
        Me.uxYes = New System.Windows.Forms.Button()
        Me.uxNo = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'uxMessage
        '
        Me.uxMessage.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.uxMessage.Location = New System.Drawing.Point(12, 12)
        Me.uxMessage.Margin = New System.Windows.Forms.Padding(3)
        Me.uxMessage.Name = "uxMessage"
        Me.uxMessage.Size = New System.Drawing.Size(355, 108)
        Me.uxMessage.TabIndex = 2
        Me.uxMessage.Text = resources.GetString("uxMessage.Text")
        '
        'uxYes
        '
        Me.uxYes.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.uxYes.DialogResult = System.Windows.Forms.DialogResult.Yes
        Me.uxYes.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.uxYes.Location = New System.Drawing.Point(207, 126)
        Me.uxYes.Name = "uxYes"
        Me.uxYes.Size = New System.Drawing.Size(77, 25)
        Me.uxYes.TabIndex = 0
        Me.uxYes.Text = "Yes"
        Me.uxYes.UseVisualStyleBackColor = True
        '
        'uxNo
        '
        Me.uxNo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.uxNo.DialogResult = System.Windows.Forms.DialogResult.No
        Me.uxNo.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.uxNo.Location = New System.Drawing.Point(290, 126)
        Me.uxNo.Name = "uxNo"
        Me.uxNo.Size = New System.Drawing.Size(77, 25)
        Me.uxNo.TabIndex = 1
        Me.uxNo.Text = "No"
        Me.uxNo.UseVisualStyleBackColor = True
        '
        'Update
        '
        Me.AcceptButton = Me.uxYes
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.uxNo
        Me.ClientSize = New System.Drawing.Size(379, 163)
        Me.Controls.Add(Me.uxNo)
        Me.Controls.Add(Me.uxYes)
        Me.Controls.Add(Me.uxMessage)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Update"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Radio Downloader"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents uxMessage As System.Windows.Forms.Label
    Friend WithEvents uxYes As System.Windows.Forms.Button
    Friend WithEvents uxNo As System.Windows.Forms.Button
End Class

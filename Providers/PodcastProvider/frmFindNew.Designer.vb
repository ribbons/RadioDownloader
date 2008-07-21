<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFindNew
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
        Me.pnlFindNew = New System.Windows.Forms.Panel
        Me.lblResult = New System.Windows.Forms.Label
        Me.lblInstructions = New System.Windows.Forms.Label
        Me.cmdViewEps = New System.Windows.Forms.Button
        Me.txtFeedURL = New System.Windows.Forms.TextBox
        Me.pnlFindNew.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlFindNew
        '
        Me.pnlFindNew.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlFindNew.Controls.Add(Me.lblResult)
        Me.pnlFindNew.Controls.Add(Me.lblInstructions)
        Me.pnlFindNew.Controls.Add(Me.cmdViewEps)
        Me.pnlFindNew.Controls.Add(Me.txtFeedURL)
        Me.pnlFindNew.Location = New System.Drawing.Point(0, 0)
        Me.pnlFindNew.Name = "pnlFindNew"
        Me.pnlFindNew.Size = New System.Drawing.Size(570, 416)
        Me.pnlFindNew.TabIndex = 0
        '
        'lblResult
        '
        Me.lblResult.AutoSize = True
        Me.lblResult.Location = New System.Drawing.Point(36, 108)
        Me.lblResult.Margin = New System.Windows.Forms.Padding(4, 20, 3, 0)
        Me.lblResult.Name = "lblResult"
        Me.lblResult.Size = New System.Drawing.Size(0, 13)
        Me.lblResult.TabIndex = 3
        '
        'lblInstructions
        '
        Me.lblInstructions.AutoSize = True
        Me.lblInstructions.Location = New System.Drawing.Point(36, 39)
        Me.lblInstructions.Margin = New System.Windows.Forms.Padding(4, 0, 3, 10)
        Me.lblInstructions.Name = "lblInstructions"
        Me.lblInstructions.Size = New System.Drawing.Size(189, 13)
        Me.lblInstructions.TabIndex = 2
        Me.lblInstructions.Text = "Enter the URL of a podcast RSS feed:"
        '
        'cmdViewEps
        '
        Me.cmdViewEps.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdViewEps.Location = New System.Drawing.Point(461, 63)
        Me.cmdViewEps.Name = "cmdViewEps"
        Me.cmdViewEps.Size = New System.Drawing.Size(73, 23)
        Me.cmdViewEps.TabIndex = 1
        Me.cmdViewEps.Text = "View"
        Me.cmdViewEps.UseVisualStyleBackColor = True
        '
        'txtFeedURL
        '
        Me.txtFeedURL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFeedURL.Location = New System.Drawing.Point(39, 65)
        Me.txtFeedURL.Name = "txtFeedURL"
        Me.txtFeedURL.Size = New System.Drawing.Size(416, 20)
        Me.txtFeedURL.TabIndex = 0
        '
        'frmFindNew
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(570, 416)
        Me.Controls.Add(Me.pnlFindNew)
        Me.Name = "frmFindNew"
        Me.Text = "Find New"
        Me.pnlFindNew.ResumeLayout(False)
        Me.pnlFindNew.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlFindNew As System.Windows.Forms.Panel
    Friend WithEvents txtFeedURL As System.Windows.Forms.TextBox
    Friend WithEvents cmdViewEps As System.Windows.Forms.Button
    Friend WithEvents lblInstructions As System.Windows.Forms.Label
    Friend WithEvents lblResult As System.Windows.Forms.Label
End Class

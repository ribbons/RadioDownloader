<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAbout
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAbout))
        Me.LogoPictureBox = New System.Windows.Forms.PictureBox
        Me.LabelNameAndVer = New System.Windows.Forms.Label
        Me.TextboxLicense = New System.Windows.Forms.TextBox
        Me.LabelCopyright = New System.Windows.Forms.Label
        Me.HomepageLink = New System.Windows.Forms.LinkLabel
        Me.LabelLicense = New System.Windows.Forms.Label
        Me.ButtonOK = New System.Windows.Forms.Button
        CType(Me.LogoPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LogoPictureBox
        '
        Me.LogoPictureBox.Image = CType(resources.GetObject("LogoPictureBox.Image"), System.Drawing.Image)
        Me.LogoPictureBox.Location = New System.Drawing.Point(12, 12)
        Me.LogoPictureBox.Name = "LogoPictureBox"
        Me.LogoPictureBox.Size = New System.Drawing.Size(64, 64)
        Me.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.LogoPictureBox.TabIndex = 1
        Me.LogoPictureBox.TabStop = False
        '
        'LabelNameAndVer
        '
        Me.LabelNameAndVer.AutoSize = True
        Me.LabelNameAndVer.Location = New System.Drawing.Point(82, 16)
        Me.LabelNameAndVer.Name = "LabelNameAndVer"
        Me.LabelNameAndVer.Size = New System.Drawing.Size(131, 13)
        Me.LabelNameAndVer.TabIndex = 1
        Me.LabelNameAndVer.Text = "Radio Downloader ?.?.?.?"
        '
        'TextboxLicense
        '
        Me.TextboxLicense.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextboxLicense.Location = New System.Drawing.Point(12, 108)
        Me.TextboxLicense.Multiline = True
        Me.TextboxLicense.Name = "TextboxLicense"
        Me.TextboxLicense.ReadOnly = True
        Me.TextboxLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.TextboxLicense.Size = New System.Drawing.Size(489, 140)
        Me.TextboxLicense.TabIndex = 2
        Me.TextboxLicense.Text = resources.GetString("TextboxLicense.Text")
        '
        'LabelCopyright
        '
        Me.LabelCopyright.AutoSize = True
        Me.LabelCopyright.Location = New System.Drawing.Point(82, 37)
        Me.LabelCopyright.Name = "LabelCopyright"
        Me.LabelCopyright.Size = New System.Drawing.Size(90, 13)
        Me.LabelCopyright.TabIndex = 2
        Me.LabelCopyright.Text = "Copyright © 20??"
        '
        'HomepageLink
        '
        Me.HomepageLink.AutoSize = True
        Me.HomepageLink.Location = New System.Drawing.Point(82, 56)
        Me.HomepageLink.Name = "HomepageLink"
        Me.HomepageLink.Size = New System.Drawing.Size(228, 13)
        Me.HomepageLink.TabIndex = 1
        Me.HomepageLink.TabStop = True
        Me.HomepageLink.Text = "http://www.nerdoftheherd.com/tools/radiodld/"
        '
        'LabelLicense
        '
        Me.LabelLicense.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.LabelLicense.AutoSize = True
        Me.LabelLicense.Location = New System.Drawing.Point(12, 92)
        Me.LabelLicense.Name = "LabelLicense"
        Me.LabelLicense.Size = New System.Drawing.Size(47, 13)
        Me.LabelLicense.TabIndex = 4
        Me.LabelLicense.Text = "License:"
        '
        'ButtonOK
        '
        Me.ButtonOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.ButtonOK.Location = New System.Drawing.Point(419, 254)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(82, 26)
        Me.ButtonOK.TabIndex = 0
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'frmAbout
        '
        Me.AcceptButton = Me.ButtonOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(513, 292)
        Me.Controls.Add(Me.ButtonOK)
        Me.Controls.Add(Me.LabelLicense)
        Me.Controls.Add(Me.HomepageLink)
        Me.Controls.Add(Me.LabelCopyright)
        Me.Controls.Add(Me.TextboxLicense)
        Me.Controls.Add(Me.LabelNameAndVer)
        Me.Controls.Add(Me.LogoPictureBox)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAbout"
        Me.Padding = New System.Windows.Forms.Padding(9)
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "About"
        CType(Me.LogoPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents LogoPictureBox As System.Windows.Forms.PictureBox
    Friend WithEvents LabelNameAndVer As System.Windows.Forms.Label
    Friend WithEvents TextboxLicense As System.Windows.Forms.TextBox
    Friend WithEvents LabelCopyright As System.Windows.Forms.Label
    Friend WithEvents HomepageLink As System.Windows.Forms.LinkLabel
    Friend WithEvents LabelLicense As System.Windows.Forms.Label
    Friend WithEvents ButtonOK As System.Windows.Forms.Button

End Class

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmError
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmError))
        Me.cmdSend = New System.Windows.Forms.Button
        Me.cmdDontSend = New System.Windows.Forms.Button
        Me.lblWhiteBar = New System.Windows.Forms.Label
        Me.lblTopMessage = New System.Windows.Forms.Label
        Me.lblTopMessage2 = New System.Windows.Forms.Label
        Me.lblTopBorder = New System.Windows.Forms.Label
        Me.lblIfYouWere = New System.Windows.Forms.Label
        Me.lblPleaseTell = New System.Windows.Forms.Label
        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        Me.lblWeHaveCreated = New System.Windows.Forms.Label
        Me.lblWeWill = New System.Windows.Forms.Label
        Me.lnkWhatData = New System.Windows.Forms.LinkLabel
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cmdSend
        '
        Me.cmdSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdSend.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.cmdSend.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdSend.Location = New System.Drawing.Point(277, 188)
        Me.cmdSend.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdSend.Name = "cmdSend"
        Me.cmdSend.Size = New System.Drawing.Size(113, 26)
        Me.cmdSend.TabIndex = 0
        Me.cmdSend.Text = "&Send Error Report"
        '
        'cmdDontSend
        '
        Me.cmdDontSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdDontSend.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdDontSend.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdDontSend.Location = New System.Drawing.Point(397, 188)
        Me.cmdDontSend.Name = "cmdDontSend"
        Me.cmdDontSend.Size = New System.Drawing.Size(84, 26)
        Me.cmdDontSend.TabIndex = 1
        Me.cmdDontSend.Text = "&Don't Send"
        '
        'lblWhiteBar
        '
        Me.lblWhiteBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblWhiteBar.BackColor = System.Drawing.Color.White
        Me.lblWhiteBar.Location = New System.Drawing.Point(0, -1)
        Me.lblWhiteBar.Name = "lblWhiteBar"
        Me.lblWhiteBar.Size = New System.Drawing.Size(493, 58)
        Me.lblWhiteBar.TabIndex = 3
        '
        'lblTopMessage
        '
        Me.lblTopMessage.AutoSize = True
        Me.lblTopMessage.BackColor = System.Drawing.Color.White
        Me.lblTopMessage.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTopMessage.Location = New System.Drawing.Point(21, 15)
        Me.lblTopMessage.Name = "lblTopMessage"
        Me.lblTopMessage.Size = New System.Drawing.Size(385, 13)
        Me.lblTopMessage.TabIndex = 4
        Me.lblTopMessage.Text = "Radio Downloader has encountered a problem and needs to close."
        '
        'lblTopMessage2
        '
        Me.lblTopMessage2.AutoSize = True
        Me.lblTopMessage2.BackColor = System.Drawing.Color.White
        Me.lblTopMessage2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTopMessage2.Location = New System.Drawing.Point(21, 30)
        Me.lblTopMessage2.Name = "lblTopMessage2"
        Me.lblTopMessage2.Size = New System.Drawing.Size(211, 13)
        Me.lblTopMessage2.TabIndex = 5
        Me.lblTopMessage2.Text = "We are sorry for the inconvenience."
        '
        'lblTopBorder
        '
        Me.lblTopBorder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblTopBorder.BackColor = System.Drawing.Color.Silver
        Me.lblTopBorder.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblTopBorder.Location = New System.Drawing.Point(0, 56)
        Me.lblTopBorder.Name = "lblTopBorder"
        Me.lblTopBorder.Size = New System.Drawing.Size(493, 1)
        Me.lblTopBorder.TabIndex = 6
        '
        'lblIfYouWere
        '
        Me.lblIfYouWere.AutoSize = True
        Me.lblIfYouWere.Location = New System.Drawing.Point(30, 69)
        Me.lblIfYouWere.Name = "lblIfYouWere"
        Me.lblIfYouWere.Size = New System.Drawing.Size(425, 13)
        Me.lblIfYouWere.TabIndex = 7
        Me.lblIfYouWere.Text = "If you were in the middle of something, the information you were working on might" & _
            " be lost."
        '
        'lblPleaseTell
        '
        Me.lblPleaseTell.AutoSize = True
        Me.lblPleaseTell.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPleaseTell.Location = New System.Drawing.Point(30, 91)
        Me.lblPleaseTell.Name = "lblPleaseTell"
        Me.lblPleaseTell.Size = New System.Drawing.Size(292, 13)
        Me.lblPleaseTell.TabIndex = 8
        Me.lblPleaseTell.Text = "Please tell NerdoftheHerd.com about this problem."
        '
        'PictureBox1
        '
        Me.PictureBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.PictureBox1.BackColor = System.Drawing.Color.White
        Me.PictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(449, 13)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(32, 32)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox1.TabIndex = 5
        Me.PictureBox1.TabStop = False
        '
        'lblWeHaveCreated
        '
        Me.lblWeHaveCreated.AutoSize = True
        Me.lblWeHaveCreated.Location = New System.Drawing.Point(30, 113)
        Me.lblWeHaveCreated.Name = "lblWeHaveCreated"
        Me.lblWeHaveCreated.Size = New System.Drawing.Size(430, 13)
        Me.lblWeHaveCreated.TabIndex = 9
        Me.lblWeHaveCreated.Text = "We have created an error report that you can send to help us improve Radio Downlo" & _
            "ader."
        '
        'lblWeWill
        '
        Me.lblWeWill.AutoSize = True
        Me.lblWeWill.Location = New System.Drawing.Point(30, 135)
        Me.lblWeWill.Name = "lblWeWill"
        Me.lblWeWill.Size = New System.Drawing.Size(266, 13)
        Me.lblWeWill.TabIndex = 10
        Me.lblWeWill.Text = "We will treat this report as confidential and anonymous."
        '
        'lnkWhatData
        '
        Me.lnkWhatData.AutoSize = True
        Me.lnkWhatData.Location = New System.Drawing.Point(30, 158)
        Me.lnkWhatData.Name = "lnkWhatData"
        Me.lnkWhatData.Size = New System.Drawing.Size(200, 13)
        Me.lnkWhatData.TabIndex = 2
        Me.lnkWhatData.TabStop = True
        Me.lnkWhatData.Text = "What data does this error report contain?"
        '
        'frmError
        '
        Me.AcceptButton = Me.cmdSend
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.cmdDontSend
        Me.ClientSize = New System.Drawing.Size(493, 226)
        Me.Controls.Add(Me.lnkWhatData)
        Me.Controls.Add(Me.lblWeWill)
        Me.Controls.Add(Me.lblWeHaveCreated)
        Me.Controls.Add(Me.lblPleaseTell)
        Me.Controls.Add(Me.lblIfYouWere)
        Me.Controls.Add(Me.cmdDontSend)
        Me.Controls.Add(Me.cmdSend)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.lblTopBorder)
        Me.Controls.Add(Me.lblTopMessage2)
        Me.Controls.Add(Me.lblTopMessage)
        Me.Controls.Add(Me.lblWhiteBar)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmError"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmdSend As System.Windows.Forms.Button
    Friend WithEvents cmdDontSend As System.Windows.Forms.Button
    Friend WithEvents lblWhiteBar As System.Windows.Forms.Label
    Friend WithEvents lblTopMessage As System.Windows.Forms.Label
    Friend WithEvents lblTopMessage2 As System.Windows.Forms.Label
    Friend WithEvents lblTopBorder As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents lblIfYouWere As System.Windows.Forms.Label
    Friend WithEvents lblPleaseTell As System.Windows.Forms.Label
    Friend WithEvents lblWeHaveCreated As System.Windows.Forms.Label
    Friend WithEvents lblWeWill As System.Windows.Forms.Label
    Friend WithEvents lnkWhatData As System.Windows.Forms.LinkLabel

End Class

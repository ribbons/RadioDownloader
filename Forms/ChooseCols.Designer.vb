<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ChooseCols
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
        Me.HideButton = New System.Windows.Forms.Button
        Me.ShowButton = New System.Windows.Forms.Button
        Me.MoveDown = New System.Windows.Forms.Button
        Me.MoveUp = New System.Windows.Forms.Button
        Me.Okay = New System.Windows.Forms.Button
        Me.Cancel = New System.Windows.Forms.Button
        Me.ColumnsList = New System.Windows.Forms.ListView
        Me.InfoLabel = New System.Windows.Forms.Label
        Me.ColumnsLabel = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'HideButton
        '
        Me.HideButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.HideButton.Enabled = False
        Me.HideButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.HideButton.Location = New System.Drawing.Point(251, 157)
        Me.HideButton.Name = "HideButton"
        Me.HideButton.Size = New System.Drawing.Size(77, 25)
        Me.HideButton.TabIndex = 8
        Me.HideButton.Text = "&Hide"
        Me.HideButton.UseVisualStyleBackColor = True
        '
        'ShowButton
        '
        Me.ShowButton.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ShowButton.Enabled = False
        Me.ShowButton.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.ShowButton.Location = New System.Drawing.Point(251, 126)
        Me.ShowButton.Name = "ShowButton"
        Me.ShowButton.Size = New System.Drawing.Size(77, 25)
        Me.ShowButton.TabIndex = 7
        Me.ShowButton.Text = "&Show"
        Me.ShowButton.UseVisualStyleBackColor = True
        '
        'MoveDown
        '
        Me.MoveDown.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.MoveDown.Enabled = False
        Me.MoveDown.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.MoveDown.Location = New System.Drawing.Point(251, 95)
        Me.MoveDown.Name = "MoveDown"
        Me.MoveDown.Size = New System.Drawing.Size(77, 25)
        Me.MoveDown.TabIndex = 6
        Me.MoveDown.Text = "Move &Down"
        Me.MoveDown.UseVisualStyleBackColor = True
        '
        'MoveUp
        '
        Me.MoveUp.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.MoveUp.Enabled = False
        Me.MoveUp.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.MoveUp.Location = New System.Drawing.Point(251, 64)
        Me.MoveUp.Name = "MoveUp"
        Me.MoveUp.Size = New System.Drawing.Size(77, 25)
        Me.MoveUp.TabIndex = 5
        Me.MoveUp.Text = "Move &Up"
        Me.MoveUp.UseVisualStyleBackColor = True
        '
        'Okay
        '
        Me.Okay.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Okay.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Okay.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.Okay.Location = New System.Drawing.Point(168, 286)
        Me.Okay.Name = "Okay"
        Me.Okay.Size = New System.Drawing.Size(77, 25)
        Me.Okay.TabIndex = 0
        Me.Okay.Text = "OK"
        Me.Okay.UseVisualStyleBackColor = True
        '
        'Cancel
        '
        Me.Cancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.Cancel.Location = New System.Drawing.Point(251, 286)
        Me.Cancel.Name = "Cancel"
        Me.Cancel.Size = New System.Drawing.Size(77, 25)
        Me.Cancel.TabIndex = 1
        Me.Cancel.Text = "Cancel"
        Me.Cancel.UseVisualStyleBackColor = True
        '
        'ColumnsList
        '
        Me.ColumnsList.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ColumnsList.CheckBoxes = True
        Me.ColumnsList.HideSelection = False
        Me.ColumnsList.Location = New System.Drawing.Point(12, 64)
        Me.ColumnsList.Margin = New System.Windows.Forms.Padding(3, 3, 6, 3)
        Me.ColumnsList.Name = "ColumnsList"
        Me.ColumnsList.Size = New System.Drawing.Size(230, 199)
        Me.ColumnsList.TabIndex = 4
        Me.ColumnsList.UseCompatibleStateImageBehavior = False
        Me.ColumnsList.View = System.Windows.Forms.View.List
        '
        'InfoLabel
        '
        Me.InfoLabel.Location = New System.Drawing.Point(9, 11)
        Me.InfoLabel.Margin = New System.Windows.Forms.Padding(3, 2, 3, 0)
        Me.InfoLabel.Name = "InfoLabel"
        Me.InfoLabel.Size = New System.Drawing.Size(315, 35)
        Me.InfoLabel.TabIndex = 2
        Me.InfoLabel.Text = "Select the columns you want to display for this list."
        '
        'ColumnsLabel
        '
        Me.ColumnsLabel.AutoSize = True
        Me.ColumnsLabel.Location = New System.Drawing.Point(9, 46)
        Me.ColumnsLabel.Margin = New System.Windows.Forms.Padding(3, 0, 3, 2)
        Me.ColumnsLabel.Name = "ColumnsLabel"
        Me.ColumnsLabel.Size = New System.Drawing.Size(50, 13)
        Me.ColumnsLabel.TabIndex = 3
        Me.ColumnsLabel.Text = "&Columns:"
        '
        'ChooseCols
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.CancelButton = Me.Cancel
        Me.ClientSize = New System.Drawing.Size(340, 321)
        Me.Controls.Add(Me.ColumnsLabel)
        Me.Controls.Add(Me.InfoLabel)
        Me.Controls.Add(Me.ColumnsList)
        Me.Controls.Add(Me.Cancel)
        Me.Controls.Add(Me.Okay)
        Me.Controls.Add(Me.MoveUp)
        Me.Controls.Add(Me.MoveDown)
        Me.Controls.Add(Me.ShowButton)
        Me.Controls.Add(Me.HideButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ChooseCols"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Choose Columns"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents HideButton As System.Windows.Forms.Button
    Friend WithEvents ShowButton As System.Windows.Forms.Button
    Friend WithEvents MoveDown As System.Windows.Forms.Button
    Friend WithEvents MoveUp As System.Windows.Forms.Button
    Friend WithEvents Okay As System.Windows.Forms.Button
    Friend WithEvents Cancel As System.Windows.Forms.Button
    Friend WithEvents ColumnsList As System.Windows.Forms.ListView
    Friend WithEvents InfoLabel As System.Windows.Forms.Label
    Friend WithEvents ColumnsLabel As System.Windows.Forms.Label
End Class

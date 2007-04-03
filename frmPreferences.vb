Option Strict On
Option Explicit On

Imports System.IO

Friend Class frmPreferences
	Inherits System.Windows.Forms.Form
	
	Private Sub cmdCancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
    End Sub
	
	Private Sub cmdChangeFolder_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdChangeFolder.Click
        Dim BrowseDialog As New FolderBrowserDialog
        BrowseDialog.SelectedPath = txtSaveIn.Text
        BrowseDialog.Description = "Choose the folder to save downloaded programmes in:"

        If BrowseDialog.ShowDialog = Windows.Forms.DialogResult.OK Then
            txtSaveIn.Text = BrowseDialog.SelectedPath
        End If
	End Sub
	
	Private Sub cmdOK_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdOK.Click
        My.Settings.SaveFolder = txtSaveIn.Text
		Me.Close()
	End Sub
	
	Private Sub frmPreferences_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        txtSaveIn.Text = GetSaveFolder()
    End Sub
End Class
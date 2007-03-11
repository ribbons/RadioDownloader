Option Strict Off
Option Explicit On
Friend Class frmPreferences
	Inherits System.Windows.Forms.Form
	
	Private Sub cmdCancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCancel.Click
		Me.Close()
	End Sub
	
	Private Sub cmdChangeFolder_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdChangeFolder.Click
		Dim strReturned As String
		strReturned = BrowseForFolder(Me.Handle.ToInt32, "Choose the folder to save downloaded programs in:", txtSaveIn.Text)
		
		If strReturned <> "" Then
			txtSaveIn.Text = strReturned
		End If
	End Sub
	
	Private Sub cmdOK_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdOK.Click
		Call SaveSetting("Radio Downloader", "Interface", "SaveFolder", txtSaveIn.Text)
		Me.Close()
	End Sub
	
	Private Sub frmPreferences_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
		txtSaveIn.Text = GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(My.Application.Info.DirectoryPath) & "Downloads")
		
		If PathIsDirectory(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(My.Application.Info.DirectoryPath) & "Downloads")) = False Then
			lblSaveIn.ForeColor = System.Drawing.Color.Red
			lblSaveIn.Text = "Path not found - choose a location to store your downloaded programs:"
		End If
	End Sub
End Class
' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007  www.nerdoftheherd.com
'
' This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
' Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
' implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
' License for more details.
'
' You should have received a copy of the GNU General Public License along with this program; if not, write
' to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

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
        My.Settings.FileNameFormat = txtFileNameFormat.Text
        My.Settings.RunAfterCommand = txtRunAfter.Text
        My.Settings.BandwidthLimit = CInt(updBandwidthLimit.Value)
        Me.Close()
    End Sub

    Private Sub frmPreferences_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        txtSaveIn.Text = GetSaveFolder()
        txtFileNameFormat.Text = My.Settings.FileNameFormat
        txtRunAfter.Text = My.Settings.RunAfterCommand
        updBandwidthLimit.Value = My.Settings.BandwidthLimit
    End Sub

    Private Sub txtFileNameFormat_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFileNameFormat.TextChanged
        lblFilenameFormatResult.Text = "Result: " + FindFreeSaveFileName(txtFileNameFormat.Text, "Example Program", "mp3", Now, "")
    End Sub

    Private Sub cmdReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReset.Click
        If MsgBox("Are you sure that you would like to reset all of your settings?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question) = MsgBoxResult.Yes Then
            My.Settings.Reset()
            Me.Close()
        End If
    End Sub
End Class
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

Imports System.Web
Imports System.Net
Imports System.Text.Encoding
Imports System.Windows.Forms

Public Class frmError
    Public strErrorInfo As String

    Private Sub cmdSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSend.Click
        Me.Visible = False

        Dim webSend As New WebClient()
        webSend.Headers.Add("Content-Type", "application/x-www-form-urlencoded")

        Dim PostData As Byte() = ASCII.GetBytes("stacktrace=" + HttpUtility.UrlEncode(strErrorInfo))
        Dim Result As Byte() = webSend.UploadData("http://www.nerdoftheherd.com/tools/radiodld/error_report.php", "POST", PostData)

        If ASCII.GetString(Result).Substring(0, 7) = "success" Then
            MsgBox("Your error report was sent successfully.")
        End If

        Me.Close()
    End Sub

    Private Sub lnkWhatData_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lnkWhatData.LinkClicked
        Call MsgBox(strErrorInfo)
    End Sub

    Private Sub cmdDontSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDontSend.Click
        Me.Close()
    End Sub

    Private Sub frmError_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        ' As there has been an error, call 'end' to blow away the rest of the app reasonably tidily
        End
    End Sub
End Class

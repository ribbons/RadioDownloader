' Plugin for Radio Downloader to download general podcasts.
' Copyright © 2007-2010 Matt Robinson
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

Imports System.Net

Imports Microsoft.Win32

Friend Class DownloadWrapper
    Public Event DownloadProgress(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs)

    Private WithEvents downloadClient As WebClient

    Private downloadUrl As Uri
    Private destPath As String
    Private downloadComplete As Boolean
    Private downloadError As Exception

    Public Sub New(ByVal downloadUrl As Uri, ByVal destPath As String)
        Me.downloadUrl = downloadUrl
        Me.destPath = destPath
    End Sub

    Public Sub Download()
        AddHandler SystemEvents.PowerModeChanged, AddressOf PowerModeChange

        downloadClient = New WebClient
        downloadClient.Headers.Add("user-agent", My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString)
        downloadClient.DownloadFileAsync(downloadUrl, destPath)
    End Sub

    Public ReadOnly Property Complete() As Boolean
        Get
            Return downloadComplete
        End Get
    End Property

    Public ReadOnly Property [Error]() As Exception
        Get
            Return downloadError
        End Get
    End Property

    Private Sub downloadClient_DownloadProgressChanged(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles downloadClient.DownloadProgressChanged
        RaiseEvent DownloadProgress(Me, e)
    End Sub

    Private Sub downloadClient_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles downloadClient.DownloadFileCompleted
        If e.Cancelled = False Then
            RemoveHandler SystemEvents.PowerModeChanged, AddressOf PowerModeChange

            If e.Error IsNot Nothing Then
                downloadError = e.Error
            Else
                downloadComplete = True
            End If
        End If
    End Sub

    Private Sub PowerModeChange(ByVal sender As Object, ByVal e As PowerModeChangedEventArgs)
        If e.Mode = PowerModes.Resume Then
            ' Restart the download, as it is quite likely to have hung during the suspend / hibernate
            If downloadClient.IsBusy Then
                downloadClient.CancelAsync()

                ' Pause for 30 seconds to be give the pc a chance to settle down after the suspend. 
                Threading.Thread.Sleep(30000)

                ' Restart the download
                downloadClient.DownloadFileAsync(downloadUrl, destPath)
            End If
        End If
    End Sub
End Class

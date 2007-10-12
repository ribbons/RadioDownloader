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

Option Explicit On
Option Strict On

Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Diagnostics

Public Class clsAutoUpdate
    Private strVersionInfoURL As String
    Private strNewVersionURL As String
    Private strUrlPrefix As String
    Private strSaveFileName As String
    Private strRunCommand As String
    Private strRunArguments As String
    Private thrDownloadThread As New Thread(AddressOf DownloadUpdate)

    Public Sub New(ByVal strVersionInfoURL As String, ByVal strUrlPrefix As String, ByVal strSaveFileName As String, ByVal strRunCommand As String, ByVal strRunArguments As String)
        Me.strVersionInfoURL = strVersionInfoURL
        Me.strUrlPrefix = strUrlPrefix
        Me.strSaveFileName = strSaveFileName
        Me.strRunCommand = strRunCommand
        Me.strRunArguments = strRunArguments
    End Sub

    Public ReadOnly Property UpdateDownloaded() As Boolean
        Get
            Return My.Settings.UpdateDownloaded
        End Get
    End Property

    Public Sub CheckForUpdates()
        If My.Settings.UpdateDownloaded = False Then
            If thrDownloadThread.IsAlive = False Then
                Dim webUpdate As New WebClient
                Dim strVersionInfo As String
                Dim strSplitInfo() As String

                Try
                    Dim stmReader As New System.IO.StreamReader(webUpdate.OpenRead(strVersionInfoURL))
                    strVersionInfo = stmReader.ReadToEnd()
                    stmReader.Close()

                    If strVersionInfo.Length > 0 Then
                        strSplitInfo = Split(strVersionInfo, vbCrLf)

                        If strSplitInfo.GetUpperBound(0) > 0 Then ' Make sure that we have at least two items in the array
                            'If strSplitInfo(0) > My.Application.Info.Version.ToString Then ' There is a new version available
                            strNewVersionURL = strUrlPrefix + strSplitInfo(1)
                            thrDownloadThread = New Thread(AddressOf DownloadUpdate)
                            thrDownloadThread.Start()
                            'End If
                        End If
                    End If
                Catch expWeb As WebException
                ' Temporary problem downloading the information, we will try again later
            End Try
            End If
        End If
    End Sub

    Public Sub InstallUpdate()
        If My.Settings.UpdateDownloaded Then
            If File.Exists(strSaveFileName) Then
                Process.Start(strRunCommand, strRunArguments)
            End If

            My.Settings.UpdateDownloaded = False
        End If
    End Sub

    Private Sub DownloadUpdate()
        Dim webUpdate As New WebClient
        Try
            webUpdate.DownloadFile(strNewVersionURL, strSaveFileName)
        Catch expWeb As WebException
            ' Temporary problem downloading the file, we will try again later
            Exit Sub
        End Try

        My.Settings.UpdateDownloaded = True
    End Sub
End Class

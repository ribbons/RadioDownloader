' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2009  www.nerdoftheherd.com
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
Imports System.Security
Imports System.Threading
Imports Microsoft.Win32

Public Class AutoUpdate
    Private strVersionInfoURL As String
    Private strDownloadUrl As String
    Private installerSavePath As String
    Private thrDownloadThread As New Thread(AddressOf DownloadUpdate)

    Public Sub New(ByVal strVersionInfoURL As String, ByVal strDownloadUrl As String, ByVal installerProductId As Guid, ByVal saveBasePath As String, ByVal defaultInstallerFileName As String)
        Me.strVersionInfoURL = strVersionInfoURL
        Me.strDownloadUrl = strDownloadUrl
        Me.installerSavePath = saveBasePath + "\" + installerName(installerProductId, defaultInstallerFileName)
    End Sub

    Public ReadOnly Property UpdateDownloaded() As Boolean
        Get
            Return My.Settings.UpdateDownloaded
        End Get
    End Property

    Public Sub CheckForUpdates()
        If My.Settings.LastCheckForUpdates.AddDays(1) < Now Then
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
                                If strSplitInfo(0) > My.Application.Info.Version.ToString Then ' There is a new version available
                                    thrDownloadThread = New Thread(AddressOf DownloadUpdate)
                                    thrDownloadThread.Start()
                                End If
                            End If
                        End If

                        My.Settings.LastCheckForUpdates = Now
                    Catch expWeb As WebException
                        ' Temporary problem downloading the information, we will try again later
                    End Try
                End If
            End If
        End If
    End Sub

    Public Sub InstallUpdate()
        If My.Settings.UpdateDownloaded Then
            If File.Exists(installerSavePath) Then
                Process.Start("msiexec", "/i """ + installerSavePath + """ REINSTALL=ALL REINSTALLMODE=vamus")
            End If

            My.Settings.UpdateDownloaded = False
        End If
    End Sub

    Private Sub DownloadUpdate()
        Dim webUpdate As New WebClient
        Try
            webUpdate.DownloadFile(strDownloadUrl, installerSavePath)
        Catch expWeb As WebException
            ' Temporary problem downloading the file, we will try again later
            Exit Sub
        End Try

        My.Settings.UpdateDownloaded = True
    End Sub

    Private Function installerName(ByVal productId As Guid, ByVal defaultInstallerFileName As String) As String
        Dim prodIdCharArray() As Char = productId.ToString("N").ToUpper.ToCharArray()

        ' ProductID is stored in the registry in an encoded form, where the following blocks are reversed
        Dim blocks() As Integer = {8, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2}
        Dim currentPosition As Integer = 0

        ' Perform the block reversal
        For reverseBlock As Integer = 0 To 10
            Array.Reverse(prodIdCharArray, currentPosition, blocks(reverseBlock))
            currentPosition += blocks(reverseBlock)
        Next

        Dim sourceList As RegistryKey = Nothing

        Try
            sourceList = Registry.ClassesRoot.OpenSubKey("Installer\Products\" + prodIdCharArray + "\SourceList")
        Catch securityExp As SecurityException
            ' Unable to read the registry key, so just return the default
            Return defaultInstallerFileName
        End Try

        If sourceList Is Nothing Then
            ' The key could not be found, so just return the default
            Return defaultInstallerFileName
        End If

        Dim packageName As String = CStr(sourceList.GetValue("PackageName"))

        If packageName Is Nothing Then
            ' The value could not be found, so just return the default
            Return defaultInstallerFileName
        End If

        If packageName.ToLower.EndsWith(".msi") = False Then
            ' The file name has the wrong extension, so just return the default
            Return defaultInstallerFileName
        End If

        Return packageName
    End Function
End Class

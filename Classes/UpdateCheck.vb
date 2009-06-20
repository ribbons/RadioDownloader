' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2009 Matt Robinson
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

Imports System.Net

Friend Class UpdateCheck
    Private versionInfoURL As String

    Public Sub New(ByVal versionInfoURL As String)
        Me.versionInfoURL = versionInfoURL
    End Sub

    Public Function IsUpdateAvailable() As Boolean
        If My.Settings.LastCheckForUpdates.AddDays(1) > Now Then
            Return False
        End If

        Dim checkUpdate As New WebClient
        checkUpdate.Headers.Add("user-agent", My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString)

        Dim versionInfo As String

        Try
            versionInfo = checkUpdate.DownloadString(versionInfoURL)
        Catch webExp As WebException
            ' Temporary problem downloading the information, try again later
            Return False
        End Try

        My.Settings.LastCheckForUpdates = Now
        My.Settings.Save() ' Save the last check time in case of unexpected termination

        If versionInfo <> String.Empty Then
            versionInfo = Split(versionInfo, vbCrLf)(0)

            If versionInfo > My.Application.Info.Version.ToString Then ' There is a new version available
                Return True
            End If
        End If

        Return False
    End Function
End Class

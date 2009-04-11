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

Option Strict On
Option Explicit On

Imports System.Net
Imports System.Text

Public Class CachedWebClient
    Private dataInst As Data

    Public Sub New()
        Me.dataInst = Data.GetInstance
    End Sub

    Public Function DownloadData(ByVal uri As String, ByVal fetchIntervalHrs As Integer) As Byte()
        If fetchIntervalHrs = 0 Then
            Throw New ArgumentException("fetchIntervalHrs cannot be zero.", "fetchIntervalHrs")
        End If

        Dim lastFetch As Date = dataInst.GetHTTPCacheLastUpdate(uri)

        If lastFetch <> Nothing Then
            If lastFetch.AddHours(fetchIntervalHrs) > Now Then
                Dim bteCacheData As Byte() = dataInst.GetHTTPCacheContent(uri)

                If bteCacheData IsNot Nothing Then
                    Return bteCacheData
                End If
            End If
        End If

        Debug.Print("Cached WebClient: Fetching " + uri)
        Dim webClient As New WebClient
        Dim data As Byte() = webClient.DownloadData(uri)

        dataInst.AddToHTTPCache(uri, data)
        Return data
    End Function

    Public Function DownloadString(ByVal uri As String, ByVal fetchIntervalHrs As Integer) As String
        Return Encoding.UTF8.GetString(DownloadData(uri, fetchIntervalHrs))
    End Function
End Class

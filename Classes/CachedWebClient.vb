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
    Private clsDataInst As Data

    Public Sub New(ByVal clsDataInst As Data)
        Me.clsDataInst = clsDataInst
    End Sub

    Public Function DownloadData(ByVal strURI As String, ByVal intFetchIntervalHrs As Integer) As Byte()
        If intFetchIntervalHrs = 0 Then
            Throw New ArgumentException("intFetchIntervalHrs cannot be zero.")
        End If

        Dim dteLastFetch As Date = clsDataInst.GetHTTPCacheLastUpdate(strURI)

        If dteLastFetch <> Nothing Then
            If dteLastFetch.AddHours(intFetchIntervalHrs) > Now Then
                Dim bteCacheData As Byte() = clsDataInst.GetHTTPCacheContent(strURI)

                If bteCacheData IsNot Nothing Then
                    Return bteCacheData
                End If
            End If
        End If

        Debug.Print("Cached WebClient: Fetching " + strURI)
        Dim webClient As New WebClient
        Dim bteData As Byte() = webClient.DownloadData(strURI)

        clsDataInst.AddToHTTPCache(strURI, bteData)
        Return bteData
    End Function

    Public Function DownloadString(ByVal strURI As String, ByVal intFetchIntervalHrs As Integer) As String
        Return Encoding.UTF8.GetString(DownloadData(strURI, intFetchIntervalHrs))
    End Function
End Class

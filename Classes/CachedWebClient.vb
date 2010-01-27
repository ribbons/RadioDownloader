' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
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

Imports System.Data.SQLite
Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text

Public Class CachedWebClient
    <Serializable()> _
    Public Structure CacheWebException
        Dim Message As String
        Dim InnerException As Exception
        Dim Status As WebExceptionStatus
        Dim Response As WebResponse
    End Structure

    Private Shared instance As New CachedWebClient

    <ThreadStatic()> _
    Private Shared dbConn As SQLiteConnection

    Public Shared Function GetInstance() As CachedWebClient
        Return instance
    End Function

    Private Function FetchDbConn() As SQLiteConnection
        If dbConn Is Nothing Then
            dbConn = New SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "httpcache.db") + ";Version=3")
            dbConn.Open()
        End If

        Return dbConn
    End Function

    Private Sub New()
        ' Create the table (and implicitly the database) for caching HTTP requests if it does not exist
        Using command As New SQLiteCommand("create table if not exists httpcache (uri varchar (1000) primary key, lastfetch datetime, success int, data blob)", FetchDbConn)
            command.ExecuteNonQuery()
        End Using

        ' Empty the table if it does exist
        Using command As New SQLiteCommand("delete from httpcache", FetchDbConn)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub AddToHTTPCache(ByVal uri As String, ByVal requestSuccess As Boolean, ByVal data As Byte())
        Using command As New SQLiteCommand("insert or replace into httpcache (uri, lastfetch, success, data) values(@uri, @lastfetch, @success, @data)", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@uri", uri))
            command.Parameters.Add(New SQLiteParameter("@lastfetch", Now))
            command.Parameters.Add(New SQLiteParameter("@success", requestSuccess))
            command.Parameters.Add(New SQLiteParameter("@data", data))
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Function GetHTTPCacheLastUpdate(ByVal uri As String) As Date
        Using command As New SQLiteCommand("select lastfetch from httpcache where uri=@uri", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@uri", uri))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Return reader.GetDateTime(reader.GetOrdinal("lastfetch"))
                Else
                    Return Nothing
                End If
            End Using
        End Using
    End Function

    Private Function GetHTTPCacheContent(ByVal uri As String, ByRef requestSuccess As Boolean) As Byte()
        Using command As New SQLiteCommand("select success, data from httpcache where uri=@uri", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@uri", uri))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    requestSuccess = reader.GetBoolean(reader.GetOrdinal("success"))

                    ' Get the length of the content by passing nothing to getbytes
                    Dim contentLength As Integer = CInt(reader.GetBytes(reader.GetOrdinal("data"), 0, Nothing, 0, 0))

                    Dim content(contentLength - 1) As Byte
                    reader.GetBytes(reader.GetOrdinal("data"), 0, content, 0, contentLength)

                    Return content
                Else
                    Return Nothing
                End If
            End Using
        End Using
    End Function

    Public Function DownloadData(ByVal uri As String, ByVal fetchIntervalHrs As Integer) As Byte()
        If fetchIntervalHrs = 0 Then
            Throw New ArgumentException("fetchIntervalHrs cannot be zero.", "fetchIntervalHrs")
        End If

        Dim lastFetch As Date = GetHTTPCacheLastUpdate(uri)

        If lastFetch <> Nothing Then
            If lastFetch.AddHours(fetchIntervalHrs) > Now Then
                Dim requestSuccess As Boolean
                Dim cacheData As Byte() = GetHTTPCacheContent(uri, requestSuccess)

                If cacheData IsNot Nothing Then
                    If requestSuccess Then
                        Return cacheData
                    Else
                        Dim memoryStream As New MemoryStream(cacheData)
                        Dim binaryFormatter As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()

                        ' Deserialise the CacheWebException structure
                        Dim cachedException As CacheWebException
                        cachedException = DirectCast(binaryFormatter.Deserialize(memoryStream), CacheWebException)

                        ' Crete a new WebException with the cached data and throw it
                        Throw New WebException(cachedException.Message, cachedException.InnerException, cachedException.Status, cachedException.Response)
                    End If
                End If
            End If
        End If

        Debug.Print("Cached WebClient: Fetching " + uri)
        Dim webClient As New WebClient
        webClient.Headers.Add("user-agent", My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString)

        Dim data As Byte()

        Try
            data = webClient.DownloadData(uri)
        Catch webExp As WebException
            If webExp.Status <> WebExceptionStatus.NameResolutionFailure And webExp.Status <> WebExceptionStatus.Timeout Then
                ' A WebException doesn't serialise well, as Response and Status get lost,
                ' so store the information in a structure and then recreate it later
                Dim cacheException As New CacheWebException
                cacheException.Message = webExp.Message
                cacheException.InnerException = webExp.InnerException
                cacheException.Status = webExp.Status
                cacheException.Response = webExp.Response

                Dim stream As New MemoryStream()
                Dim formatter As New BinaryFormatter()

                ' Serialise the CacheWebException and store it in the cache
                formatter.Serialize(stream, cacheException)
                AddToHTTPCache(uri, False, stream.ToArray())
            End If

            ' Re-throw the WebException
            Throw
        End Try

        AddToHTTPCache(uri, True, data)

        Return data
    End Function

    Public Function DownloadString(ByVal uri As String, ByVal fetchIntervalHrs As Integer) As String
        Return Encoding.UTF8.GetString(DownloadData(uri, fetchIntervalHrs))
    End Function
End Class

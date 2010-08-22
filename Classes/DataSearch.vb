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
Imports System.Collections.Generic
Imports System.IO

Friend Class DataSearch
    <ThreadStatic()> _
    Private Shared dbConn As SQLiteConnection

    Private Shared searchInstance As DataSearch
    Private Shared searchInstanceLock As New Object

    Private dataInstance As Data

    Public Shared Function GetInstance(ByVal instance As Data) As DataSearch
        ' Need to use a lock instead of declaring the instance variable as New,
        ' as otherwise New gets called before the Data class is ready
        SyncLock searchInstanceLock
            If searchInstance Is Nothing Then
                searchInstance = New DataSearch(instance)
            End If

            Return searchInstance
        End SyncLock
    End Function

    Private Sub New(ByVal instance As Data)
        dataInstance = instance

        Dim tableCols As New Dictionary(Of String, String())

        tableCols.Add("downloads", {"name"})

        If CheckIndex(tableCols) = False Then
            ' Close & clean up the connection used for testing
            dbConn.Close()
            dbConn.Dispose()
            dbConn = Nothing

            ' Clean up the old index
            File.Delete(DatabasePath())

            Status.StatusText = "Building search index..."
            Status.ProgressBarMarquee = False
            Status.ProgressBarValue = 0
            Status.ProgressBarMax = 100 * tableCols.Count
            Status.Show()

            Using trans As SQLiteTransaction = FetchDbConn.BeginTransaction
                ' Create the index tables
                For Each table As KeyValuePair(Of String, String()) In tableCols
                    Using createTable As New SQLiteCommand(TableSql(table.Key, table.Value), FetchDbConn, trans)
                        createTable.ExecuteNonQuery()
                    End Using
                Next

                Status.StatusText = "Building search index for downloads..."

                Using insertCmd As New SQLiteCommand("insert into downloads (docid, name) values (@epid, @name)", FetchDbConn, trans)
                    Dim epidParam As New SQLiteParameter("@epid")
                    Dim nameParam As New SQLiteParameter("@name")

                    insertCmd.Parameters.Add(epidParam)
                    insertCmd.Parameters.Add(nameParam)

                    Dim progress As Integer = 1
                    Dim downloadItems As List(Of Data.DownloadData) = dataInstance.FetchDownloadList(False)

                    For Each downloadItem As Data.DownloadData In downloadItems
                        epidParam.Value = downloadItem.epid
                        nameParam.Value = downloadItem.name

                        insertCmd.ExecuteNonQuery()

                        Status.ProgressBarValue = CInt((progress / downloadItems.Count) * 100)
                        progress += 1
                    Next
                End Using

                Status.ProgressBarValue = 100

                trans.Commit()
            End Using

            Status.Hide()
        End If
    End Sub

    Private Function DatabasePath() As String
        Return Path.Combine(FileUtils.GetAppDataFolder(), "searchindex.db")
    End Function

    Private Function FetchDbConn() As SQLiteConnection
        If dbConn Is Nothing Then
            dbConn = New SQLiteConnection("Data Source=" + DatabasePath() + ";Version=3")
            dbConn.Open()
        End If

        Return dbConn
    End Function

    Private Function CheckIndex(ByVal tableCols As Dictionary(Of String, String())) As Boolean
        Using checkCmd As New SQLiteCommand("select count(*) from sqlite_master where type='table' and name=@name and sql=@sql", FetchDbConn)
            Dim nameParam As New SQLiteParameter("@name")
            Dim sqlParam As New SQLiteParameter("@sql")

            checkCmd.Parameters.Add(nameParam)
            checkCmd.Parameters.Add(sqlParam)

            For Each table As KeyValuePair(Of String, String()) In tableCols
                nameParam.Value = table.Key
                sqlParam.Value = TableSql(table.Key, table.Value)

                If CInt(checkCmd.ExecuteScalar()) <> 1 Then
                    Return False
                End If
            Next
        End Using

        Return True
    End Function

    Private Function TableSql(ByVal tableName As String, ByVal columns As String()) As String
        Return "CREATE VIRTUAL TABLE " + tableName + " USING fts3(" + Join(columns, ", ") + ")"
    End Function
End Class

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

Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Friend Class UpdateDB
    Implements IDisposable

    Private isDisposed As Boolean

    Private specConn As SQLiteConnection
    Private updateConn As SQLiteConnection

    Private Enum UpdateType
        None
        Create
        Update
    End Enum

    Public Sub New(ByVal specDbPath As String, ByVal updateDbPath As String)
        MyBase.New()

        specConn = New SQLiteConnection("Data Source=" + specDbPath + ";Version=3;New=False")
        specConn.Open()

        updateConn = New SQLiteConnection("Data Source=" + updateDbPath + ";Version=3;New=False")
        updateConn.Open()
    End Sub

    Public Function UpdateStructure() As Boolean
        Dim updateReqd As UpdateType

        Using specCommand As New SQLiteCommand("select name, sql from sqlite_master where type='table'", specConn), _
              checkUpdateCmd As New SQLiteCommand("select sql from sqlite_master where type='table' and name=@name", updateConn)

            Dim nameParam As New SQLiteParameter("@name")
            checkUpdateCmd.Parameters.Add(nameParam)

            Using specReader As SQLiteDataReader = specCommand.ExecuteReader
                Dim nameOrd As Integer = specReader.GetOrdinal("name")
                Dim sqlOrd As Integer = specReader.GetOrdinal("sql")

                While specReader.Read
                    Dim specName As String = specReader.GetString(nameOrd)
                    Dim specSql As String = specReader.GetString(sqlOrd)

                    nameParam.Value = specName

                    Using checkUpdateRdr As SQLiteDataReader = checkUpdateCmd.ExecuteReader
                        If checkUpdateRdr.Read = False Then
                            ' The table doesn't exist
                            updateReqd = UpdateType.Create
                        Else
                            If specSql = checkUpdateRdr.GetString(checkUpdateRdr.GetOrdinal("sql")) Then
                                ' The table does not require an update
                                updateReqd = UpdateType.None
                            Else
                                ' The structure of the table doesn't match, so update it
                                updateReqd = UpdateType.Update
                            End If
                        End If
                    End Using

                    If updateReqd = UpdateType.Create Then
                        ' Create the table
                        Using updateCommand As New SQLiteCommand(specSql, updateConn)
                            updateCommand.ExecuteNonQuery()
                        End Using
                    ElseIf updateReqd = UpdateType.Update Then
                        ' Fetch a list of common column names for transferring the data
                        Dim columnNames As String = ColNames(specName)

                        ' Start a transaction, so we can roll back if there is an error
                        Using trans As SQLiteTransaction = updateConn.BeginTransaction
                            Try
                                ' Rename the existing table to table_name_old
                                Using updateCommand As New SQLiteCommand("alter table [" + specName + "] rename to [" + specName + "_old]", updateConn, trans)
                                    updateCommand.ExecuteNonQuery()
                                End Using

                                ' Create the new table with the correct structure
                                Using updateCommand As New SQLiteCommand(specSql, updateConn, trans)
                                    updateCommand.ExecuteNonQuery()
                                End Using

                                ' Copy across the data (discarding rows which violate any new constraints)
                                If columnNames <> "" Then
                                    Using updateCommand As New SQLiteCommand("insert or ignore into [" + specName + "] (" + columnNames + ") select " + columnNames + " from [" + specName + "_old]", updateConn, trans)
                                        updateCommand.ExecuteNonQuery()
                                    End Using
                                End If

                                ' Delete the old table
                                Using updateCommand As New SQLiteCommand("drop table [" + specName + "_old]", updateConn, trans)
                                    updateCommand.ExecuteNonQuery()
                                End Using
                            Catch sqliteExp As SQLiteException
                                trans.Rollback()
                                Throw
                            End Try

                            trans.Commit()
                        End Using
                    End If
                End While
            End Using
        End Using

        Return True
    End Function

    Private Function ColNames(ByVal tableName As String) As String
        Dim fromCols As List(Of String) = ListTableColumns(updateConn, tableName)
        Dim toCols As List(Of String) = ListTableColumns(specConn, tableName)
        Dim resultCols As New List(Of String)

        ' Store an intersect of the from and to columns in resultCols
        For Each fromCol As String In fromCols
            If toCols.Contains(fromCol) Then
                resultCols.Add(fromCol)
            End If
        Next

        If resultCols.Count > 0 Then
            Return "[" + Join(resultCols.ToArray, "], [") + "]"
        Else
            Return ""
        End If
    End Function

    Private Function ListTableColumns(ByVal connection As SQLiteConnection, ByVal tableName As String) As List(Of String)
        Dim returnList As New List(Of String)

        Dim restrictionValues() As String = {Nothing, Nothing, tableName, Nothing}
        Dim columns As DataTable = connection.GetSchema(SQLite.SQLiteMetaDataCollectionNames.Columns, restrictionValues)

        For Each columnRow As DataRow In columns.Rows
            returnList.Add(columnRow.Item("COLUMN_NAME").ToString)
        Next

        Return returnList
    End Function

    Private Sub Dispose(ByVal disposing As Boolean)
        If Not Me.isDisposed Then
            If disposing Then
                specConn.Dispose()
                updateConn.Dispose()
            End If

            Me.isDisposed = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
End Class

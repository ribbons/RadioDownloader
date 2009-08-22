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

Imports System.Collections.Generic
Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Friend Class UpdateDB
    Private sqlSpecConn As SQLiteConnection
    Private sqlUpdConn As SQLiteConnection

    Public Sub New(ByVal strSpecDBPath As String, ByVal strUpdateDBPath As String)
        MyBase.New()

        sqlSpecConn = New SQLiteConnection("Data Source=" + strSpecDBPath + ";Version=3;New=False")
        sqlSpecConn.Open()

        sqlUpdConn = New SQLiteConnection("Data Source=" + strUpdateDBPath + ";Version=3;New=False")
        sqlUpdConn.Open()
    End Sub

    Public Function UpdateStructure() As Boolean
        Dim sqlSpecCommand As New SQLiteCommand("SELECT name, sql FROM sqlite_master WHERE type='table'", sqlSpecConn)
        Dim sqlSpecReader As SQLiteDataReader = sqlSpecCommand.ExecuteReader

        Dim sqlUpdCommand As SQLiteCommand
        Dim sqlUpdReader As SQLiteDataReader

        With sqlSpecReader
            While .Read()
                sqlUpdCommand = New SQLiteCommand("SELECT sql FROM sqlite_master WHERE type='table' AND name='" + .GetString(.GetOrdinal("name")).Replace("'", "''") + "'", sqlUpdConn)
                sqlUpdReader = sqlUpdCommand.ExecuteReader

                If sqlUpdReader.Read Then
                    ' The table exists in the database already, so check that it has the correct structure.
                    If .GetString(.GetOrdinal("sql")) <> sqlUpdReader.GetString(sqlUpdReader.GetOrdinal("sql")) Then
                        ' The structure of the table doesn't match, so update it

                        ' Store a list of common column names for transferring the data
                        Dim strColNames As String = ColNames(.GetString(.GetOrdinal("name")))

                        sqlUpdReader.Close()

                        ' Start a transaction, so we can roll back if there is an error
                        sqlUpdCommand = New SQLiteCommand("BEGIN TRANSACTION", sqlUpdConn)
                        sqlUpdCommand.ExecuteNonQuery()

                        Try
                            ' Rename the existing table to table_name_old
                            sqlUpdCommand = New SQLiteCommand("ALTER TABLE [" + .GetString(.GetOrdinal("name")) + "] RENAME TO [" + .GetString(.GetOrdinal("name")) + "_old]", sqlUpdConn)
                            sqlUpdCommand.ExecuteNonQuery()

                            ' Create the new table with the correct structure
                            sqlUpdCommand = New SQLiteCommand(.GetString(.GetOrdinal("sql")), sqlUpdConn)
                            sqlUpdCommand.ExecuteNonQuery()

                            ' Copy across the data
                            If strColNames <> "" Then
                                sqlUpdCommand = New SQLiteCommand("INSERT INTO [" + .GetString(.GetOrdinal("name")) + "] (" + strColNames + ") SELECT " + strColNames + " FROM [" + .GetString(.GetOrdinal("name")) + "_old]", sqlUpdConn)
                                sqlUpdCommand.ExecuteNonQuery()
                            End If
                            
                            ' Delete the old table
                            sqlUpdCommand = New SQLiteCommand("DROP TABLE [" + .GetString(.GetOrdinal("name")) + "_old]", sqlUpdConn)
                            sqlUpdCommand.ExecuteNonQuery()

                            ' Commit the transaction
                            sqlUpdCommand = New SQLiteCommand("COMMIT TRANSACTION", sqlUpdConn)
                            sqlUpdCommand.ExecuteNonQuery()

                        Catch expException As SQLiteException
                            ' Roll back the transaction, to try and stop the database being corrupted
                            sqlUpdCommand = New SQLiteCommand("ROLLBACK TRANSACTION", sqlUpdConn)
                            sqlUpdCommand.ExecuteNonQuery()

                            Throw
                        End Try
                    End If
                Else
                    ' Create the table
                    sqlUpdCommand = New SQLiteCommand(.GetString(.GetOrdinal("sql")), sqlUpdConn)
                    sqlUpdCommand.ExecuteNonQuery()
                End If

                sqlUpdReader.Close()
            End While
        End With

        sqlSpecReader.Close()

        Return True
    End Function

    Private Function ColNames(ByVal tableName As String) As String
        Dim fromCols As List(Of String) = ListTableColumns(sqlUpdConn, tableName)
        Dim toCols As List(Of String) = ListTableColumns(sqlSpecConn, tableName)
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

    Protected Overrides Sub Finalize()
        sqlSpecConn.Close()
        sqlUpdConn.Close()

        MyBase.Finalize()
    End Sub
End Class

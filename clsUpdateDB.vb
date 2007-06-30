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

Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Public Class clsUpdateDB
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
                        Dim strColNames As String = ColNames(sqlUpdReader.GetString(sqlUpdReader.GetOrdinal("sql")), .GetString(.GetOrdinal("sql")))

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

                            Throw expException
                        End Try
                    End If
                Else
                    ' Create the database
                    sqlUpdCommand = New SQLiteCommand(.GetString(.GetOrdinal("sql")), sqlUpdConn)
                    sqlUpdCommand.ExecuteNonQuery()
                End If

                sqlUpdReader.Close()
            End While
        End With

        sqlSpecReader.Close()

        Return True
    End Function

    Private Function ColNames(ByVal strSqlFrom As String, ByVal strSqlTo As String) As String
        Dim strFromCols() As String = ListColsFromSql(strSqlFrom)
        Dim strToCols() As String = ListColsFromSql(strSqlTo)
        Dim strResultCols(-1) As String

        ' Need to sort strToCols so that BinarySearch works effectively.
        Array.Sort(strToCols)

        ' Store an intersect of the from and to columns in strResultCols
        For Each strFromCol As String In strFromCols
            If Array.BinarySearch(strToCols, strFromCol) >= 0 Then
                ReDim Preserve strResultCols(strResultCols.GetUpperBound(0) + 1)
                strResultCols(strResultCols.GetUpperBound(0)) = strFromCol
            End If
        Next

        If strResultCols.GetUpperBound(0) > -1 Then
            Return "[" + Join(strResultCols, "], [") + "]"
        Else
            Return ""
        End If
    End Function

    Private Function ListColsFromSql(ByVal strSql As String) As String()
        ' Work through the sql definition of a table, pick out the row names, and store them in an array

        Dim strReturnList(-1) As String
        Dim strSqlLines() As String = Split(strSql, vbLf)
        Dim booInBrackets As Boolean

        Dim ColNameLine As New Regex(vbTab + "\[(?<row>.*?)\].*")

        For Each strSqlLine As String In strSqlLines
            If booInBrackets = False Then
                If strSqlLine = "(" Then
                    booInBrackets = True
                End If
            Else
                If strSqlLine = ")" Then
                    booInBrackets = False
                Else
                    If ColNameLine.IsMatch(strSqlLine) Then
                        ReDim Preserve strReturnList(strReturnList.GetUpperBound(0) + 1)
                        strReturnList(strReturnList.GetUpperBound(0)) = ColNameLine.Match(strSqlLine).Groups("row").Value
                    End If
                End If
            End If
        Next

        Return strReturnList
    End Function

    Protected Overrides Sub Finalize()
        sqlSpecConn.Close()
        sqlUpdConn.Close()

        MyBase.Finalize()
    End Sub
End Class

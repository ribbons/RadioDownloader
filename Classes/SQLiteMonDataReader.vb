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

Imports System.Collections.Generic
Imports System.Data.SQLite

Public Class SQLiteMonDataReader
    Implements IDisposable

    Private Shared readerInfo As New Dictionary(Of SQLiteDataReader, String)
    Private Shared readerInfoLock As New Object

    Private isDisposed As Boolean
    Private wrappedReader As SQLiteDataReader

    Public Sub New(ByVal reader As SQLiteDataReader)
        wrappedReader = reader

        Dim trace As New StackTrace(True)

        SyncLock readerInfoLock
            readerInfo.Add(wrappedReader, trace.ToString)
        End SyncLock
    End Sub

    Public Shared Function AddReadersInfo(ByVal exp As Exception) As Exception
        Dim info As String = String.Empty

        SyncLock readerInfoLock
            For Each entry As String In readerInfo.Values
                info += entry + Environment.NewLine
            Next
        End SyncLock

        If info <> String.Empty Then
            exp.Data.Add("readers", info)
        End If

        Return exp
    End Function

    Public Function GetOrdinal(ByVal name As String) As Integer
        Return wrappedReader.GetOrdinal(name)
    End Function

    Public Function GetBoolean(ByVal i As Integer) As Boolean
        Return wrappedReader.GetBoolean(i)
    End Function

    Public Function GetBytes(ByVal i As Integer, ByVal fieldOffset As Long, ByVal buffer() As Byte, ByVal bufferOffset As Integer, ByVal length As Integer) As Long
        Return wrappedReader.GetBytes(i, fieldOffset, buffer, bufferOffset, length)
    End Function

    Public Function GetDateTime(ByVal i As Integer) As Date
        Return wrappedReader.GetDateTime(i)
    End Function

    Public Function GetInt32(ByVal i As Integer) As Integer
        Return wrappedReader.GetInt32(i)
    End Function

    Public Function GetString(ByVal i As Integer) As String
        Return wrappedReader.GetString(i)
    End Function

    Public Function GetValue(ByVal i As Integer) As Object
        Return wrappedReader.GetValue(i)
    End Function

    Public Function IsDBNull(ByVal i As Integer) As Boolean
        Return wrappedReader.IsDBNull(i)
    End Function

    Public Function Read() As Boolean
        Return wrappedReader.Read
    End Function

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.isDisposed Then
            If disposing Then
                SyncLock readerInfoLock
                    readerInfo.Remove(wrappedReader)
                End SyncLock

                wrappedReader.Dispose()
            End If
        End If

        Me.isDisposed = True
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

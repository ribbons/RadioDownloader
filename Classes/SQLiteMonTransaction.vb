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

Public Class SQLiteMonTransaction
    Implements IDisposable

    Private Shared readerInfo As New Dictionary(Of SQLiteTransaction, String)
    Private Shared readerInfoLock As New Object

    Private isDisposed As Boolean
    Private wrappedTrans As SQLiteTransaction

    Public Sub New(ByVal transaction As SQLiteTransaction)
        wrappedTrans = transaction

        Dim trace As New StackTrace(True)

        SyncLock readerInfoLock
            readerInfo.Add(wrappedTrans, trace.ToString)
        End SyncLock
    End Sub

    Public Shared Function AddTransactionsInfo(ByVal exp As Exception) As Exception
        Dim info As String = String.Empty

        SyncLock readerInfoLock
            For Each entry As String In readerInfo.Values
                info += entry + Environment.NewLine
            Next
        End SyncLock

        If info <> String.Empty Then
            exp.Data.Add("transactions", info)
        End If

        Return exp
    End Function

    Public ReadOnly Property trans() As SQLiteTransaction
        Get
            Return wrappedTrans
        End Get
    End Property

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.isDisposed Then
            If disposing Then
                SyncLock readerInfoLock
                    readerInfo.Remove(wrappedTrans)
                End SyncLock

                wrappedTrans.Dispose()
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

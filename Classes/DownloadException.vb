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
Imports System.Runtime.Serialization
Imports System.Security.Permissions

<Serializable()> _
Public Class DldErrorDataItem
    Private itemName As String
    Private itemData As String

    Public Property Name() As String
        Get
            Return itemName
        End Get
        Set(ByVal value As String)
            itemName = value
        End Set
    End Property

    Public Property Data() As String
        Get
            Return itemData
        End Get
        Set(ByVal value As String)
            itemData = value
        End Set
    End Property

    Protected Sub New()
        ' Do nothing, just needed for deserialisation
    End Sub

    Public Sub New(ByVal name As String, ByVal data As String)
        itemName = name
        itemData = data
    End Sub
End Class

Public Enum ErrorType
    UnknownError = 0
    LocalProblem = 1
    ShorterThanExpected = 2
    NotAvailable = 3
    RemoveFromList = 4
    NotAvailableInLocation = 5
    NetworkProblem = 6
End Enum

<Serializable()> _
Public Class DownloadException : Inherits Exception
    Private ReadOnly type As ErrorType
    Private ReadOnly extraDetails As List(Of DldErrorDataItem)

    Public Sub New()
        MyBase.New()

        Me.type = ErrorType.UnknownError
    End Sub

    Public Sub New(ByVal message As String)
        MyBase.New(message)

        Me.type = ErrorType.UnknownError
    End Sub

    Public Sub New(ByVal message As String, ByVal innerException As Exception)
        MyBase.New(message, innerException)

        Me.type = ErrorType.UnknownError
    End Sub

    Public Sub New(ByVal type As ErrorType)
        Me.type = type
    End Sub

    Public Sub New(ByVal type As ErrorType, ByVal message As String)
        MyBase.New(message)

        Me.type = type
    End Sub

    Public Sub New(ByVal type As ErrorType, ByVal message As String, ByVal extraDetails As List(Of DldErrorDataItem))
        MyBase.New(message)

        Me.type = type
        Me.extraDetails = extraDetails
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)

        Me.type = CType(info.GetValue("type", GetType(ErrorType)), ErrorType)
        Me.extraDetails = CType(info.GetValue("extraDetails", GetType(List(Of DldErrorDataItem))), List(Of DldErrorDataItem))
    End Sub

    <SecurityPermission(SecurityAction.Demand, SerializationFormatter:=True)> _
    Public Overrides Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.GetObjectData(info, context)

        info.AddValue("type", type)
        info.AddValue("extraDetails", extraDetails)
    End Sub

    Public ReadOnly Property TypeOfError() As ErrorType
        Get
            Return type
        End Get
    End Property

    Public ReadOnly Property ErrorExtraDetails() As List(Of DldErrorDataItem)
        Get
            Return extraDetails
        End Get
    End Property
End Class

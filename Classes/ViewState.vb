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

Friend Class ViewState
    Public Enum MainTab
        FindProgramme
        Favourites
        Subscriptions
        Downloads
    End Enum

    Public Enum View
        FindNewChooseProvider
        FindNewProviderForm
        ProgEpisodes
        Favourites
        Subscriptions
        Downloads
    End Enum

    Private Structure ViewData
        Dim Tab As MainTab
        Dim View As View
        Dim Data As Object
    End Structure

    Private backData As New Stack(Of ViewData)
    Private fwdData As New Stack(Of ViewData)

    Public Event UpdateNavBtnState(ByVal enableBack As Boolean, ByVal enableFwd As Boolean)
    Public Event ViewChanged(ByVal view As View, ByVal tab As MainTab, ByVal data As Object)

    Public ReadOnly Property CurrentView As View
        Get
            Return backData.Peek.View
        End Get
    End Property

    Public Property CurrentViewData As Object
        Get
            Return backData.Peek.Data
        End Get
        Set(ByVal value As Object)
            Dim curView As ViewData = backData.Peek
            curView.Data = value
        End Set
    End Property

    Public Sub SetView(ByVal tab As MainTab, ByVal view As View)
        Call SetView(tab, view, Nothing)
    End Sub

    Public Sub SetView(ByVal tab As MainTab, ByVal view As View, ByVal viewData As Object)
        Call StoreView(tab, view, viewData)
        RaiseEvent ViewChanged(view, tab, viewData)
    End Sub

    Public Sub SetView(ByVal view As View, ByVal viewData As Object)
        Dim currentView As ViewData = backData.Peek
        SetView(currentView.Tab, view, viewData)
    End Sub

    Public Sub StoreView(ByVal tab As MainTab, ByVal view As View, ByVal viewData As Object)
        Dim storeView As ViewData

        storeView.Tab = tab
        storeView.View = view
        storeView.Data = viewData

        backData.Push(storeView)

        If fwdData.Count > 0 Then
            fwdData.Clear()
        End If

        RaiseEvent UpdateNavBtnState(backData.Count > 1, False)
    End Sub

    Public Sub StoreView(ByVal view As View, ByVal viewData As Object)
        Dim currentView As ViewData = backData.Peek
        StoreView(currentView.Tab, view, viewData)
    End Sub

    Public Sub StoreView(ByVal viewData As Object)
        Dim currentView As ViewData = backData.Peek
        StoreView(currentView.Tab, currentView.View, viewData)
    End Sub

    Public Sub NavBack()
        fwdData.Push(backData.Pop)

        Dim curView As ViewData = backData.Peek

        RaiseEvent UpdateNavBtnState(backData.Count > 1, fwdData.Count > 0)
        RaiseEvent ViewChanged(curView.View, curView.Tab, curView.Data)
    End Sub

    Public Sub NavFwd()
        backData.Push(fwdData.Pop)

        Dim curView As ViewData = backData.Peek

        RaiseEvent UpdateNavBtnState(backData.Count > 1, fwdData.Count > 0)
        RaiseEvent ViewChanged(curView.View, curView.Tab, curView.Data)
    End Sub
End Class

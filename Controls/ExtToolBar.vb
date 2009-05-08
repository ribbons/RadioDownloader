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

Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Runtime.InteropServices

Friend Class ExtToolBar : Inherits ToolBar
    ' Window Messages
    Private Const WM_USER As Integer = &H400
    Private Const TB_SETBUTTONINFO As Integer = WM_USER + 64

    ' TBBUTTONINFO Mask Flags
    Private Const TBIF_STYLE As Integer = &H8&
    Private Const TBIF_SIZE As Integer = &H40
    Private Const TBIF_BYINDEX As Long = &H80000000

    ' TBBUTTONINFO Style Flags
    Private Const BTNS_AUTOSIZE As Integer = &H10
    Private Const BTNS_WHOLEDROPDOWN As Integer = &H80

    'API Structs
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
    Private Structure TBBUTTONINFO
        Dim cbSize As Integer
        Dim dwMask As Integer
        Dim idCommand As Integer
        Dim iImage As Integer
        Dim fsState As Byte
        Dim fsStyle As Byte
        Dim cx As Short
        Dim lParam As IntPtr
        Dim pszText As IntPtr
        Dim cchText As Integer
    End Structure

    ' API Declarations
    Private Declare Auto Function SendMessage Lib "user32" (ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByRef lParam As TBBUTTONINFO) As IntPtr

    ' Variables
    Private wholeDropDownButtons As New List(Of ToolBarButton)

    Public Sub SetWholeDropDown(ByVal button As ToolBarButton)
        If button Is Nothing Then
            Throw New ArgumentNullException("button")
        End If

        If wholeDropDownButtons.Contains(button) = False Then
            wholeDropDownButtons.Add(button)
        End If

        Dim buttonInfo As TBBUTTONINFO

        With buttonInfo
            .cbSize = Marshal.SizeOf(buttonInfo)
            .dwMask = TBIF_STYLE Or TBIF_BYINDEX
            .fsStyle = BTNS_WHOLEDROPDOWN Or BTNS_AUTOSIZE
        End With

        SendMessage(Me.Handle, TB_SETBUTTONINFO, CType(Me.Buttons.IndexOf(button), IntPtr), buttonInfo)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case TB_SETBUTTONINFO
                Dim tbrInfo As TBBUTTONINFO
                tbrInfo = CType(Marshal.PtrToStructure(m.LParam, GetType(TBBUTTONINFO)), TBBUTTONINFO)

                If (tbrInfo.dwMask And TBIF_SIZE) = TBIF_SIZE Then
                    ' If the .net wrapper is trying to set the size, then prevent this
                    tbrInfo.dwMask = tbrInfo.dwMask Xor TBIF_SIZE
                End If

                If (tbrInfo.dwMask And TBIF_STYLE) = TBIF_STYLE Then
                    If (tbrInfo.fsStyle And BTNS_AUTOSIZE) <> BTNS_AUTOSIZE Then
                        ' Make sure that the autosize style is set for all buttons, and doesn't
                        ' get inadvertantly unset at any point by the .net wrapper
                        tbrInfo.fsStyle = CByte(tbrInfo.fsStyle Or BTNS_AUTOSIZE)
                    End If
                End If

                Marshal.StructureToPtr(tbrInfo, m.LParam, True)
        End Select

        MyBase.WndProc(m)
    End Sub

    Protected Overrides Function ProcessMnemonic(ByVal inputChar As Char) As Boolean
        For Each checkButton As ToolBarButton In Me.Buttons
            If checkButton.Visible AndAlso IsMnemonic(inputChar, checkButton.Text) Then
                If wholeDropDownButtons.Contains(checkButton) Then
                    ' Give the toolbar button a pressed appearance
                    checkButton.Pushed = True
                    ' Set the whole dropdown flag again as setting pushed will have cleared it
                    SetWholeDropDown(checkButton)

                    ' Calculate where the menu should be shown
                    Dim menuLocation As New Point(checkButton.Rectangle.Left, checkButton.Rectangle.Bottom)
                    ' Show the menu (modally)
                    CType(checkButton.DropDownMenu, ContextMenu).Show(Me, menuLocation)

                    ' Remove the pressed appearance
                    checkButton.Pushed = False
                    SetWholeDropDown(checkButton)
                Else
                    ' Just fire the click code for the button
                    Me.OnButtonClick(New ToolBarButtonClickEventArgs(checkButton))
                End If

                ' Let the calling function know that we found a matching mnemonic
                Return True
            End If
        Next

        Return False
    End Function
End Class

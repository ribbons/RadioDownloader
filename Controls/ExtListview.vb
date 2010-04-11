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
Imports System.Globalization
Imports System.Runtime.InteropServices

' Parts of the code in this class are based on c# code from http://www.codeproject.com/cs/miscctrl/ListViewEmbeddedControls.asp

Friend Class ExtListView : Inherits ListView
    ' Window Messages
    Private Const WM_CREATE As Integer = &H1
    Private Const WM_SETFOCUS As Integer = &H7
    Private Const WM_PAINT As Integer = &HF
    Private Const WM_CHANGEUISTATE As Integer = &H127

    ' WM_CHANGEUISTATE Parameters
    Protected Const UIS_INITIALIZE As Integer = &H3
    Protected Const UISF_HIDEFOCUS As Integer = &H1

    ' ListView messages
    Private Const LVM_FIRST As Integer = &H1000
    Private Const LVM_SETEXTENDEDLISTVIEWSTYLE As Integer = (LVM_FIRST + 54)

    ' Extended ListView Styles
    Private Const LVS_EX_DOUBLEBUFFER As Integer = &H10000

    ' API Declarations
    Private Declare Auto Function SendMessage Lib "user32" (ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    Private Declare Unicode Function SetWindowTheme Lib "uxtheme" (ByVal hWnd As IntPtr, ByVal pszSubAppName As String, ByVal pszSubIdList As String) As IntPtr

    ' Data structure to store information about the controls
    Private Structure EmbeddedProgress
        Dim prgProgress As ProgressBar
        Dim intColumn As Integer
        Dim dstDock As DockStyle
        Dim lstItem As ListViewItem
    End Structure

    ' List to store the EmbeddedProgress structures
    Private embeddedControls As New List(Of EmbeddedProgress)

    Private Function GetColumnOrder() As Integer()
        Dim intOrder(Me.Columns.Count) As Integer

        For intLoop As Integer = 0 To Me.Columns.Count - 1
            intOrder(intLoop) = Me.Columns(intLoop).DisplayIndex
        Next

        Return intOrder
    End Function

    Private Function GetSubItemBounds(ByVal listItem As ListViewItem, ByVal intSubItem As Integer) As Rectangle
        Dim subItemRect As Rectangle = Rectangle.Empty

        If listItem Is Nothing Then
            Throw New ArgumentNullException("listItem")
        End If

        Dim intOrder() As Integer = GetColumnOrder()

        If intOrder Is Nothing Then
            ' No Columns
            Return subItemRect
        End If

        If intSubItem >= intOrder.Length Then
            Throw New ArgumentOutOfRangeException("SubItem " + intSubItem.ToString(CultureInfo.InvariantCulture) + " out of range")
        End If

        ' Retrieve the bounds of the entire ListViewItem (all subitems)
        Dim lviBounds As Rectangle = listItem.GetBounds(ItemBoundsPortion.Entire)

        Dim subItemX As Integer = lviBounds.Left

        ' Calculate the X position of the SubItem.
        ' Because the columns can be reordered we have to use Columns[order[i]] instead of Columns[i] !

        Dim colHdr As ColumnHeader
        Dim intLoop As Integer

        For intLoop = 0 To intOrder.Length - 1
            colHdr = Me.Columns(intOrder(intLoop))

            If colHdr.Index = intSubItem Then
                Exit For
            End If

            subItemX += colHdr.Width
        Next

        subItemRect = New Rectangle(subItemX, lviBounds.Top, Me.Columns(intOrder(intLoop)).Width, lviBounds.Height)

        Return subItemRect
    End Function

    Public Sub AddProgressBar(ByRef prgProgress As ProgressBar, ByVal lstParentItem As ListViewItem, ByVal intCol As Integer)
        Call AddProgressBar(prgProgress, lstParentItem, intCol, DockStyle.Fill)
    End Sub

    Public Sub AddProgressBar(ByRef progressBar As ProgressBar, ByVal lstParentItem As ListViewItem, ByVal column As Integer, ByVal dstDock As DockStyle)
        If progressBar Is Nothing Then
            Throw New ArgumentNullException("progressBar")
        End If

        If column >= Columns.Count Then
            Throw New ArgumentOutOfRangeException("column")
        End If

        Dim emcControl As EmbeddedProgress

        emcControl.prgProgress = progressBar
        emcControl.intColumn = column
        emcControl.dstDock = dstDock
        emcControl.lstItem = lstParentItem

        embeddedControls.Add(emcControl)

        ' Add a Click event handler to select the ListView row when an embedded control is clicked
        AddHandler progressBar.Click, AddressOf embeddedControl_Click

        Me.Controls.Add(progressBar)
    End Sub

    Public Sub RemoveProgressBar(ByRef progressBar As ProgressBar)
        If progressBar Is Nothing Then
            Throw New ArgumentNullException("progressBar")
        End If

        For intLoop As Integer = 0 To embeddedControls.Count - 1
            If embeddedControls(intLoop).prgProgress.Equals(progressBar) Then
                RemoveHandler progressBar.Click, AddressOf embeddedControl_Click
                Me.Controls.Remove(progressBar)
                embeddedControls.RemoveAt(intLoop)
                Exit Sub
            End If
        Next

        Throw New ArgumentException("Progress bar not found!")
    End Sub

    Public Function GetProgressBar(ByVal lstParentItem As ListViewItem, ByVal intCol As Integer) As ProgressBar
        For Each emcControl As EmbeddedProgress In embeddedControls
            If emcControl.lstItem.Equals(lstParentItem) And emcControl.intColumn = intCol Then
                Return emcControl.prgProgress
            End If
        Next

        Return Nothing
    End Function

    Public Sub RemoveAllControls()
        For intLoop As Integer = 0 To embeddedControls.Count - 1
            Dim emcControl As EmbeddedProgress = embeddedControls(intLoop)

            emcControl.prgProgress.Visible = False
            RemoveHandler emcControl.prgProgress.Click, AddressOf embeddedControl_Click
            Me.Controls.Remove(emcControl.prgProgress)
        Next

        embeddedControls.Clear()
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_CREATE
                If OsUtils.WinXpOrLater() Then
                    ' Set the theme of the control to "explorer", to give the 
                    ' correct styling under Vista.  This has no effect under XP.
                    SetWindowTheme(Me.Handle, "explorer", Nothing)
                End If

                ' Remove the focus rectangle from the control (and as a side effect, all other controls on the
                ' form) if the last input event came from the mouse, or add them if it came from the keyboard.
                SendMessage(Me.Handle, WM_CHANGEUISTATE, MakeLParam(UIS_INITIALIZE, UISF_HIDEFOCUS), New IntPtr(0))
            Case LVM_SETEXTENDEDLISTVIEWSTYLE
                If OsUtils.WinXpOrLater() Then
                    Dim intStyles As Integer = CInt(m.LParam)

                    If (intStyles And LVS_EX_DOUBLEBUFFER) <> LVS_EX_DOUBLEBUFFER Then
                        intStyles = intStyles Or LVS_EX_DOUBLEBUFFER
                        m.LParam = CType(intStyles, IntPtr)
                    End If
                End If
            Case WM_SETFOCUS
                ' Remove the focus rectangle from the control (and as a side effect, all other controls on the
                ' form) if the last input event came from the mouse, or add them if it came from the keyboard.
                SendMessage(Me.Handle, WM_CHANGEUISTATE, MakeLParam(UIS_INITIALIZE, UISF_HIDEFOCUS), New IntPtr(0))
            Case WM_PAINT
                If View <> View.Details Then
                    Exit Select
                End If

                ' Calculate the position of all embedded controls
                For Each emcControl As EmbeddedProgress In embeddedControls
                    Dim rect As Rectangle = Me.GetSubItemBounds(emcControl.lstItem, emcControl.intColumn)

                    If ((Me.HeaderStyle <> ColumnHeaderStyle.None) And (rect.Top < Me.Font.Height)) Or (rect.Top + rect.Height) <= 0 Or (rect.Top > Me.ClientRectangle.Height) Then
                        ' Control overlaps ColumnHeader, is off the top, or is off the bottom of the listview
                        emcControl.prgProgress.Visible = False
                        Continue For
                    Else
                        emcControl.prgProgress.Visible = True
                    End If

                    Select Case emcControl.dstDock
                        Case DockStyle.Fill
                        Case DockStyle.Top
                            rect.Height = emcControl.prgProgress.Height
                        Case DockStyle.Left
                            rect.Width = emcControl.prgProgress.Width
                        Case DockStyle.Bottom
                            rect.Offset(0, rect.Height - emcControl.prgProgress.Height)
                            rect.Height = emcControl.prgProgress.Height
                        Case DockStyle.Right
                            rect.Offset(rect.Width - emcControl.prgProgress.Width, 0)
                            rect.Width = emcControl.prgProgress.Width
                        Case DockStyle.None
                            rect.Size = emcControl.prgProgress.Size
                    End Select

                    ' Set embedded control's bounds
                    emcControl.prgProgress.Bounds = rect
                Next
        End Select

        Call MyBase.WndProc(m)
    End Sub

    Private Sub embeddedControl_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' When a progress bar is clicked the ListViewItem holding it is selected
        For Each emcControl As EmbeddedProgress In embeddedControls
            If emcControl.prgProgress.Equals(DirectCast(sender, ProgressBar)) Then
                Me.SelectedItems.Clear()
                emcControl.lstItem.Selected = True
            End If
        Next
    End Sub

    Private Function MakeLParam(ByVal LoWord As Integer, ByVal HiWord As Integer) As IntPtr
        Dim IntPtrHiWord As New IntPtr(HiWord << 16)
        Dim IntPtrLoWord As New IntPtr(LoWord And &HFFFF)

        Return New IntPtr(IntPtrHiWord.ToInt32() Or IntPtrLoWord.ToInt32())
    End Function
End Class

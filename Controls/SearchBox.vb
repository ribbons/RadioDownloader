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

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.VisualStyles

Public Class SearchBox
    Inherits Control

    <DllImport("uxtheme.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
    Private Shared Function SetWindowTheme(ByVal hWnd As IntPtr, ByVal pszSubAppName As String, ByVal pszSubIdList As String) As Integer
    End Function

    ' Search box theme class, part and states
    Private Const SEARCHBOX As String = "SearchBox"
    Private Const SBBACKGROUND As Integer = 1

    Private Const SBB_NORMAL As Integer = 1
    Private Const SBB_HOT As Integer = 2
    Private Const SBB_DISABLED As Integer = 3
    Private Const SBB_FOCUSED As Integer = 4

    Private boxState As Integer = 1
    Private themeHeight As Integer

    Private WithEvents textBox As TextBox

    Public Sub New()
        MyBase.New()

        ' Create the child textbox control for the user to type in, without a border
        textBox = New TextBox
        textBox.BorderStyle = BorderStyle.None
        Me.Controls.Add(textBox)

        ' Work out the height that the search box should be displayed with
        Dim sizeStyle As New VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, SBB_NORMAL)

        Using sizeGraphics As Graphics = Me.CreateGraphics
            Me.themeHeight = sizeStyle.GetPartSize(sizeGraphics, ThemeSizeType.True).Height
        End Using
    End Sub

    Private Sub SearchBox_HandleCreated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.HandleCreated
        If OsUtils.WinXpOrLater() Then
            ' Set the theme of this parent control and the edit control, so they are rendered correctly
            If SetWindowTheme(Me.Handle, "SearchBoxComposited", Nothing) <> 0 Then
                Throw New Win32Exception
            End If

            If SetWindowTheme(textBox.Handle, "SearchBoxEditComposited", Nothing) <> 0 Then
                Throw New Win32Exception
            End If
        End If
    End Sub

    Private Sub SearchBox_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        ' Clear the background of the control to make sure it is transparent when on glass
        e.Graphics.Clear(Color.Transparent)

        If VisualStyleRenderer.IsSupported Then
            ' Paint the correct background for the control based on the current state
            Dim searchBoxStyle As New VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, boxState)
            searchBoxStyle.DrawBackground(e.Graphics, New Rectangle(0, 0, Me.Width, Me.Height))
        End If
    End Sub

    Private Sub textBox_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.MouseEnter
        If boxState = SBB_NORMAL Then
            boxState = SBB_HOT
        End If

        ' Repaint the control and child textbox
        Me.Invalidate()
        textBox.Invalidate()
    End Sub

    Private Sub textBox_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.MouseLeave
        If boxState = SBB_HOT Then
            boxState = SBB_NORMAL
        End If

        ' Repaint the control and child textbox
        Me.Invalidate()
        textBox.Invalidate()
    End Sub

    Private Sub textBox_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.GotFocus
        boxState = SBB_FOCUSED
        Me.Invalidate() ' Repaint the control
    End Sub

    Private Sub textBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.LostFocus
        boxState = SBB_NORMAL
        Me.Invalidate() ' Repaint the control
    End Sub

    Private Sub SearchBox_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Me.Height <> Me.themeHeight Then
            ' Force the height to always be set to that specified from the theme
            Me.Height = Me.themeHeight
        End If

        textBox.Top = 4
        textBox.Left = 2
        textBox.Width = Me.Width - (textBox.Left + 2)
    End Sub

    Private Sub textBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.TextChanged
        ' Hook up changes to the child textbox through this control
        Me.Text = textBox.Text
    End Sub
End Class


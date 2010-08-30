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

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As String) As IntPtr
    End Function

    ' Window Messages
    Private Const EM_SETCUEBANNER As Integer = &H1501

    ' Search box theme class, part and states
    Private Const SEARCHBOX As String = "SearchBox"
    Private Const SBBACKGROUND As Integer = &H1

    Private Const SBB_NORMAL As Integer = &H1
    Private Const SBB_HOT As Integer = &H2
    Private Const SBB_DISABLED As Integer = &H3
    Private Const SBB_FOCUSED As Integer = &H4

    Private boxState As Integer = SBB_NORMAL
    Private themeHeight As Integer
    Private buttonHover As Boolean

    Private _cueBanner As String

    Private WithEvents textBox As TextBox
    Private WithEvents button As PictureBox

    Public Sub New()
        MyBase.New()

        SetStyle(ControlStyles.SupportsTransparentBackColor, True)

        ' Create the child textbox control for the user to type in, without a border
        textBox = New TextBox
        Me.themeHeight = textBox.Height + 2
        textBox.BorderStyle = BorderStyle.None
        Me.Controls.Add(textBox)

        ' Create a picturebox to display the search icon and cancel 'button'
        button = New PictureBox
        button.BackColor = Color.Transparent
        button.BackgroundImage = My.Resources.search_icon
        button.Size = My.Resources.search_icon.Size
        Me.Controls.Add(button)

        ' Work out the height that the search box should be displayed
        If VisualStyleRenderer.IsSupported Then
            Dim sizeStyle As New VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, SBB_NORMAL)

            Using sizeGraphics As Graphics = Me.CreateGraphics
                Me.themeHeight = sizeStyle.GetPartSize(sizeGraphics, ThemeSizeType.True).Height
            End Using
        End If
    End Sub

    Public Property CueBanner As String
        Get
            Return _cueBanner
        End Get
        Set(ByVal value As String)
            _cueBanner = value
            SendMessage(textBox.Handle, EM_SETCUEBANNER, IntPtr.Zero, _cueBanner)
        End Set
    End Property

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
        If VisualStyleRenderer.IsSupported Then
            ' Paint the correct background for the control based on the current state
            Dim searchBoxStyle As New VisualStyleRenderer(SEARCHBOX, SBBACKGROUND, boxState)
            searchBoxStyle.DrawBackground(e.Graphics, New Rectangle(0, 0, Me.Width, Me.Height))
        Else
            e.Graphics.Clear(SystemColors.Window)

            ' Paint a 'classic textbox' border for the control
            Using controlDark As New Pen(SystemColors.ControlDark), controlDarkDark As New Pen(SystemColors.ControlDarkDark), controlLightLight As New Pen(SystemColors.ControlLightLight), controlLight As New Pen(SystemColors.ControlLight)
                e.Graphics.DrawLine(controlDark, 0, Me.Height, 0, 0)
                e.Graphics.DrawLine(controlDark, 0, 0, Me.Width, 0)
                e.Graphics.DrawLine(controlDarkDark, 1, Me.Height - 1, 1, 1)
                e.Graphics.DrawLine(controlDarkDark, 1, 1, Me.Width - 1, 1)

                e.Graphics.DrawLine(controlLight, Me.Width - 2, 1, Me.Width - 2, Me.Height - 2)
                e.Graphics.DrawLine(controlLight, Me.Width - 2, Me.Height - 2, 1, Me.Height - 2)
                e.Graphics.DrawLine(controlLightLight, Me.Width - 1, 0, Me.Width - 1, Me.Height - 1)
                e.Graphics.DrawLine(controlLightLight, Me.Width - 1, Me.Height - 1, 0, Me.Height - 1)
            End Using
        End If
    End Sub

    Private Sub textBox_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.MouseEnter
        If boxState = SBB_NORMAL Then
            boxState = SBB_HOT
        End If

        If VisualStyleRenderer.IsSupported Then
            ' Repaint the control and child textbox
            Me.Invalidate()
            textBox.Invalidate()
        End If
    End Sub

    Private Sub textBox_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.MouseLeave
        If boxState = SBB_HOT Then
            boxState = SBB_NORMAL
        End If

        If VisualStyleRenderer.IsSupported Then
            ' Repaint the control and child textbox
            Me.Invalidate()
            textBox.Invalidate()
        End If
    End Sub

    Private Sub textBox_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.GotFocus
        boxState = SBB_FOCUSED
        Me.Invalidate() ' Repaint the control
    End Sub

    Private Sub textBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.LostFocus
        boxState = SBB_NORMAL
        Me.Invalidate() ' Repaint the control
    End Sub

    Private Sub button_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles button.MouseEnter
        buttonHover = True

        If Me.Text <> String.Empty Then
            button.BackgroundImage = My.Resources.search_close_hover
        End If

        textBox_MouseEnter(sender, e)
    End Sub

    Private Sub button_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles button.MouseLeave
        buttonHover = False

        If Me.Text <> String.Empty Then
            button.BackgroundImage = My.Resources.search_close
        End If

        textBox_MouseLeave(sender, e)
    End Sub

    Private Sub button_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles button.MouseDown
        If Me.Text <> String.Empty Then
            button.BackgroundImage = My.Resources.search_close_pressed
        End If
    End Sub

    Private Sub button_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles button.MouseUp
        If Me.Text <> String.Empty Then
            button.BackgroundImage = My.Resources.search_close_hover
        End If
    End Sub

    Private Sub button_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles button.MouseClick
        If textBox.Text <> String.Empty Then
            textBox.Text = String.Empty
        End If
    End Sub

    Private Sub SearchBox_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        If Me.Height <> Me.themeHeight Then
            ' Force the height to always be set to that specified from the theme
            Me.Height = Me.themeHeight
        End If

        ' Vertically center the search / cancel button on the right hand side
        button.Left = Me.Width - (button.Width + 6)
        button.Top = CInt((Me.Height - button.Height) / 2) + 1

        ' Use the rest of the space for the textbox
        textBox.Top = 4
        textBox.Width = button.Left - (textBox.Left + 4)

        If OsUtils.WinVistaOrLater And VisualStyleRenderer.IsSupported Then
            ' The textbox is given extra padding as part of the visual style
            textBox.Left = 2
        Else
            textBox.Left = 8
        End If
    End Sub

    Private Sub textBox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles textBox.TextChanged
        ' Hook up changes to the child textbox through this control
        Me.Text = textBox.Text

        ' Update the displayed icon
        If Me.Text = String.Empty Then
            button.BackgroundImage = My.Resources.search_icon
        Else
            If buttonHover Then
                button.BackgroundImage = My.Resources.search_close_hover
            Else
                button.BackgroundImage = My.Resources.search_close
            End If
        End If
    End Sub

    Private Sub textBox_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles textBox.KeyDown
        If e.KeyCode = Keys.Escape Then
            textBox.Text = String.Empty
        End If
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not Me.IsDisposed Then
            If disposing Then
                textBox.Dispose()
                button.Dispose()
            End If
        End If

        MyBase.Dispose(disposing)
    End Sub
End Class


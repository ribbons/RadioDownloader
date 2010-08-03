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

Imports System.Threading

Friend Class Status
    Private showThread As Thread
    Private tbarNotif As TaskbarNotify

    Public Shadows Sub Show()
        showThread = New Thread(AddressOf ShowFormThread)
        showThread.Start()
    End Sub

    Private Sub ShowFormThread()
        If OsUtils.WinSevenOrLater Then
            tbarNotif = New TaskbarNotify
        End If

        MyBase.ShowDialog()
    End Sub

    Public Property StatusText() As String
        Get
            Return lblStatus.Text
        End Get
        Set(ByVal text As String)
            If Me.IsHandleCreated Then
                Call Me.Invoke(Sub() SetStatusText_FormThread(text))
            Else
                Call SetStatusText_FormThread(text)
            End If
        End Set
    End Property

    Private Sub SetStatusText_FormThread(ByVal text As String)
        lblStatus.Text = text
    End Sub

    Public Property ProgressBarMarquee() As Boolean
        Get
            Return prgProgress.Style = ProgressBarStyle.Marquee
        End Get
        Set(ByVal marquee As Boolean)
            If Me.IsHandleCreated Then
                Call Me.Invoke(Sub() SetProgressBarMarquee_FormThread(marquee))
            Else
                Call SetProgressBarMarquee_FormThread(marquee)
            End If
        End Set
    End Property

    Private Sub SetProgressBarMarquee_FormThread(ByVal marquee As Boolean)
        prgProgress.Style = If(marquee, ProgressBarStyle.Marquee, ProgressBarStyle.Blocks)

        If OsUtils.WinSevenOrLater And Me.IsHandleCreated Then
            If marquee Then
                tbarNotif.SetProgressMarquee(Me)
            Else
                tbarNotif.SetProgressNone(Me)
            End If
        End If
    End Sub

    Public Property ProgressBarMax() As Integer
        Get
            Return prgProgress.Maximum
        End Get
        Set(ByVal value As Integer)
            If Me.IsHandleCreated Then
                Call Me.Invoke(Sub() SetProgressBarMax_FormThread(value))
            Else
                SetProgressBarMax_FormThread(value)
            End If
        End Set
    End Property

    Private Sub SetProgressBarMax_FormThread(ByVal value As Integer)
        prgProgress.Maximum = value
    End Sub

    Public Property ProgressBarValue() As Integer
        Get
            Return prgProgress.Value
        End Get
        Set(ByVal value As Integer)
            If Me.IsHandleCreated Then
                Call Me.Invoke(Sub() SetProgressBarValue_FormThread(value))
            Else
                SetProgressBarValue_FormThread(value)
            End If
        End Set
    End Property

    Private Sub SetProgressBarValue_FormThread(ByVal value As Integer)
        prgProgress.Value = value

        If OsUtils.WinSevenOrLater And Me.IsHandleCreated Then
            tbarNotif.SetProgressValue(Me, value, prgProgress.Maximum)
        End If
    End Sub

    Public Shadows Sub Hide()
        If Me.IsHandleCreated Then
            Call Me.Invoke(Sub() HideForm_FormThread())
        Else
            HideForm_FormThread()
        End If
    End Sub

    Private Sub HideForm_FormThread()
        MyBase.Hide()
    End Sub

    Private Sub Status_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        e.Cancel = True
    End Sub

    Private Sub Status_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Font = SystemFonts.MessageBoxFont
    End Sub

    Private Sub Status_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        If OsUtils.WinSevenOrLater Then
            If prgProgress.Style = ProgressBarStyle.Marquee Then
                tbarNotif.SetProgressMarquee(Me)
            Else
                If prgProgress.Value <> 0 Then
                    tbarNotif.SetProgressValue(Me, prgProgress.Value, prgProgress.Maximum)
                End If
            End If
        End If
    End Sub
End Class
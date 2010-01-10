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
    Private Delegate Sub SetStatusText_Delegate(ByVal text As String)
    Private Delegate Sub SetProgressBarMarquee_Delegate(ByVal marquee As Boolean)
    Private Delegate Sub SetProgressBarMax_Delegate(ByVal value As Integer)
    Private Delegate Sub SetProgressBarValue_Delegate(ByVal value As Integer)
    Private Delegate Sub HideForm_Delegate()

    Private showThread As Thread

    Public Shadows Sub Show()
        showThread = New Thread(AddressOf ShowFormThread)
        showThread.Start()
    End Sub

    Private Sub ShowFormThread()
        MyBase.ShowDialog()
    End Sub

    Public Property StatusText() As String
        Get
            Return lblStatus.Text
        End Get
        Set(ByVal text As String)
            If Me.IsHandleCreated Then
                Call Me.Invoke(New SetStatusText_Delegate(AddressOf SetStatusText_FormThread), New Object() {text})
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
                Call Me.Invoke(New SetProgressBarMarquee_Delegate(AddressOf SetProgressBarMarquee_FormThread), New Object() {marquee})
            Else
                Call SetProgressBarMarquee_FormThread(marquee)
            End If
        End Set
    End Property

    Private Sub SetProgressBarMarquee_FormThread(ByVal marquee As Boolean)
        If marquee Then
            prgProgress.Style = ProgressBarStyle.Marquee
        Else
            prgProgress.Style = ProgressBarStyle.Blocks
        End If
    End Sub

    Public Property ProgressBarMax() As Integer
        Get
            Return prgProgress.Maximum
        End Get
        Set(ByVal value As Integer)
            If Me.IsHandleCreated Then
                Call Me.Invoke(New SetProgressBarMax_Delegate(AddressOf SetProgressBarMax_FormThread), New Object() {value})
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
                Call Me.Invoke(New SetProgressBarValue_Delegate(AddressOf SetProgressBarValue_FormThread), New Object() {value})
            Else
                SetProgressBarValue_FormThread(value)
            End If
        End Set
    End Property

    Private Sub SetProgressBarValue_FormThread(ByVal value As Integer)
        prgProgress.Value = value
    End Sub

    Public Shadows Sub Hide()
        If Me.IsHandleCreated Then
            Call Me.Invoke(New HideForm_Delegate(AddressOf HideForm_FormThread))
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
End Class
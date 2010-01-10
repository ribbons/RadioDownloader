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

Imports System.Windows.Forms

Friend Class ReportError
    Private clsReport As ErrorReporting

    Public Sub AssignReport(ByVal clsReport As ErrorReporting)
        Me.clsReport = clsReport
    End Sub

    Private Sub cmdSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSend.Click
        Try
            Me.Visible = False
            clsReport.SendReport(My.Settings.ErrorReportURL)
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try

        Me.Close()
    End Sub

    Private Sub lnkWhatData_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lnkWhatData.LinkClicked
        Call MsgBox(clsReport.ToString)
    End Sub

    Private Sub cmdDontSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDontSend.Click
        Me.Close()
    End Sub

    Private Sub Error_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        ' As there has been an error, call 'end' to blow away the rest of the app reasonably tidily
        End
    End Sub
End Class

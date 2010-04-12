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

Friend Class CleanUp 
    Inherits System.Windows.Forms.Form

    Private progData As Data

    Private Sub CleanUp_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        progData = Data.GetInstance
        Me.Font = SystemFonts.MessageBoxFont
    End Sub

    Private Sub cmdCancel_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdCancel.Click
        Me.Close()
        Me.Dispose()
    End Sub

    Private Sub cmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOK.Click
        cmdOK.Enabled = False
        cmdCancel.Enabled = False
        radType.Enabled = False
        lblExplainOrphan.Enabled = False

        progData.PerformCleanup()

        Me.Close()
        Me.Dispose()
    End Sub
End Class
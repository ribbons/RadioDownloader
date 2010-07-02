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

Imports System.Security.Permissions

Public Class ExtToolStrip
    Inherits ToolStrip

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTTRANSPARENT As Integer = -&H1

    Public Sub New()
        MyBase.New()
    End Sub

    <SecurityPermission(SecurityAction.LinkDemand, Flags:=SecurityPermissionFlag.UnmanagedCode)> _
    Protected Overrides Sub WndProc(ByRef m As Message)
        Select Case m.Msg
            Case WM_NCHITTEST
                Dim xPos As Integer = (CInt(m.LParam) << 16) >> 16
                Dim yPos As Integer = CInt(m.LParam) >> 16

                Dim clientPos As Point = Me.PointToClient(New Point(xPos, yPos))
                Dim onBackground As Boolean = True

                ' Test to see if the mouse is over any of the toolstrip controls
                For Each child As ToolStripItem In Me.Items
                    If child.Bounds.Contains(clientPos) Then
                        onBackground = False
                        Exit For
                    End If
                Next

                If onBackground Then
                    ' Make the strip transparent to mouse actions in this area
                    m.Result = New IntPtr(HTTRANSPARENT)
                    Return
                End If
        End Select

        Call MyBase.WndProc(m)
    End Sub
End Class

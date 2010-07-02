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
Imports System.Security.Permissions
Imports System.Windows.Forms.VisualStyles

Public MustInherit Class GlassForm
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const WM_DWMCOMPOSITIONCHANGED As Integer = &H31E

    Private Const HTCLIENT As Integer = &H1
    Private Const HTCAPTION As Integer = &H2

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure MARGINS
        Public cxLeftWidth As Integer
        Public cxRightWidth As Integer
        Public cyTopHeight As Integer
        Public cyButtomheight As Integer
    End Structure

    <DllImport("dwmapi.dll", SetLastError:=True)> _
    Private Shared Function DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef pMarInset As MARGINS) As Integer
    End Function

    Private glassSet As Boolean
    Private glassMargins As MARGINS

    Protected Sub New()
        MyBase.New()
    End Sub

    Public Sub SetGlassMargins(ByVal leftMargin As Integer, ByVal rightMargin As Integer, ByVal topMargin As Integer, ByVal bottomMargin As Integer)
        glassMargins = New MARGINS()

        glassMargins.cxLeftWidth = leftMargin
        glassMargins.cxRightWidth = rightMargin
        glassMargins.cyTopHeight = topMargin
        glassMargins.cyButtomheight = bottomMargin

        glassSet = True
        ExtendFrameIntoClientArea()
    End Sub

    <SecurityPermission(SecurityAction.LinkDemand, Flags:=SecurityPermissionFlag.UnmanagedCode)> _
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Select Case m.Msg
            Case WM_DWMCOMPOSITIONCHANGED
                If glassSet Then
                    ExtendFrameIntoClientArea()
                End If
            Case WM_NCHITTEST
                DefWndProc(m)

                If OsUtils.WinVistaOrLater AndAlso VisualStyleRenderer.IsSupported AndAlso glassSet Then
                    If CInt(m.Result) = HTCLIENT Then
                        ' Pretend that the mouse was over the title bar, making the form draggable
                        m.Result = New IntPtr(HTCAPTION)
                        Return
                    End If
                End If
        End Select

        MyBase.WndProc(m)
    End Sub

    Private Sub ExtendFrameIntoClientArea()
        If Not OsUtils.CompositionEnabled Then
            Return
        End If

        If DwmExtendFrameIntoClientArea(Me.Handle, glassMargins) <> 0 Then
            Throw New Win32Exception()
        End If
    End Sub
End Class

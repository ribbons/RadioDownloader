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

Friend Module modMain
    Private Const GW_CHILD As Short = 5
    Private Const GW_HWNDNEXT As Short = 2
    Private Const IDANI_OPEN As Short = &H1S
    Private Const IDANI_CLOSE As Short = &H2S
    Private Const IDANI_CAPTION As Short = &H3S

    Private Declare Function DrawAnimatedRects Lib "user32" (ByVal hWnd As IntPtr, ByVal idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    Private Declare Function GetWindow Lib "user32" (ByVal hWnd As IntPtr, ByVal wCmd As Integer) As IntPtr
    Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As IntPtr, ByVal lpClassName As String, ByVal nMaxCount As Integer) As Integer
    Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As IntPtr, ByRef lpRect As RECT) As Integer

    Public Sub TrayAnimate(ByRef form As System.Windows.Forms.Form, ByRef down As Boolean)
        Dim window As RECT
        Dim systemTray As RECT

        systemTray = GetSysTrayPos()
        Call GetWindowRect(form.Handle, window)

        If down = True Then
            DrawAnimatedRects(form.Handle, IDANI_CLOSE Or IDANI_CAPTION, window, systemTray)
        Else
            DrawAnimatedRects(form.Handle, IDANI_OPEN Or IDANI_CAPTION, systemTray, window)
        End If
    End Sub

    Private Function GetSysTrayPos() As RECT
        Dim taskbarHwnd As IntPtr
        Dim trayHwnd As IntPtr
        Dim className As String = Space(250)

        'Get taskbar handle
        taskbarHwnd = FindWindow("Shell_traywnd", vbNullString)

        'Get system tray handle
        trayHwnd = GetWindow(taskbarHwnd, GW_CHILD)
        Do
            GetClassName(trayHwnd, className, 250)
            If TrimNull(className) = "TrayNotifyWnd" Then Exit Do
            trayHwnd = GetWindow(trayHwnd, GW_HWNDNEXT)
        Loop

        Call GetWindowRect(trayHwnd, GetSysTrayPos)
    End Function

    Private Function TrimNull(ByVal trimStr As String) As String
        Dim pos As Integer
        pos = InStr(trimStr, Chr(0))

        If pos > 0 Then trimStr = Left(trimStr, pos - 1)
        TrimNull = trimStr
    End Function
End Module
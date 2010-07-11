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
Imports System.Text

Imports Microsoft.Win32

<StructLayout(LayoutKind.Sequential, Pack:=4)> _
Friend Structure RECT
    Public left As Integer
    Public top As Integer
    Public right As Integer
    Public bottom As Integer
End Structure

Friend Class OsUtils
    Private Const GW_CHILD As Short = 5
    Private Const GW_HWNDNEXT As Short = 2
    Private Const IDANI_OPEN As Short = &H1S
    Private Const IDANI_CLOSE As Short = &H2S
    Private Const IDANI_CAPTION As Short = &H3S

    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function DrawAnimatedRects(ByVal hWnd As IntPtr, ByVal idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Ansi, BestFitMapping:=False, ThrowOnUnmappableChar:=True)> _
    Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function GetWindow(ByVal hWnd As IntPtr, ByVal wCmd As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto, BestFitMapping:=False, ThrowOnUnmappableChar:=True)> _
    Private Shared Function GetClassName(ByVal hWnd As System.IntPtr, ByVal lpClassName As StringBuilder, ByVal nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)> _
    Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("dwmapi.dll", SetLastError:=True)> _
    Private Shared Function DwmIsCompositionEnabled(<MarshalAs(UnmanagedType.Bool)> ByRef pfEnabled As Boolean) As Integer
    End Function

    Public Shared Function WinSevenOrLater() As Boolean
        Dim curOs As OperatingSystem = System.Environment.OSVersion

        If curOs.Platform = PlatformID.Win32NT AndAlso (((curOs.Version.Major = 6) AndAlso (curOs.Version.Minor >= 1)) OrElse (curOs.Version.Major > 6)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function WinVistaOrLater() As Boolean
        Dim curOs As OperatingSystem = System.Environment.OSVersion

        If curOs.Platform = PlatformID.Win32NT AndAlso (((curOs.Version.Major = 6) AndAlso (curOs.Version.Minor >= 0)) OrElse (curOs.Version.Major > 6)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function WinXpOrLater() As Boolean
        Dim curOs As OperatingSystem = System.Environment.OSVersion

        If curOs.Platform = PlatformID.Win32NT AndAlso (((curOs.Version.Major = 5) AndAlso (curOs.Version.Minor >= 1)) OrElse (curOs.Version.Major > 5)) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Sub TrayAnimate(ByRef form As Form, ByRef down As Boolean)
        Dim className As New StringBuilder(255)
        Dim taskbarHwnd As IntPtr
        Dim trayHwnd As IntPtr

        'Get taskbar handle
        taskbarHwnd = FindWindow("Shell_traywnd", Nothing)

        If taskbarHwnd = IntPtr.Zero Then
            Throw New Win32Exception
        End If

        'Get system tray handle
        trayHwnd = GetWindow(taskbarHwnd, GW_CHILD)

        Do
            If trayHwnd = IntPtr.Zero Then
                Throw New Win32Exception
            End If

            If GetClassName(trayHwnd, className, className.Capacity) = 0 Then
                Throw New Win32Exception
            End If

            If className.ToString = "TrayNotifyWnd" Then
                Exit Do
            End If

            trayHwnd = GetWindow(trayHwnd, GW_HWNDNEXT)
        Loop

        Dim systray As RECT
        Dim window As RECT

        ' Fetch the location of the systray from its window handle
        If GetWindowRect(trayHwnd, systray) = False Then
            Throw New Win32Exception
        End If

        ' Fetch the location of the window from its window handle
        If GetWindowRect(form.Handle, window) = False Then
            Throw New Win32Exception
        End If

        ' Perform the animation
        If down = True Then
            If DrawAnimatedRects(form.Handle, IDANI_CLOSE Or IDANI_CAPTION, window, systray) = False Then
                Throw New Win32Exception
            End If
        Else
            If DrawAnimatedRects(form.Handle, IDANI_OPEN Or IDANI_CAPTION, systray, window) = False Then
                Throw New Win32Exception
            End If
        End If
    End Sub

    Public Shared Function CompositionEnabled() As Boolean
        If Not WinVistaOrLater() Then
            Return False
        End If

        Dim enabled As Boolean = False

        If DwmIsCompositionEnabled(enabled) <> 0 Then
            Throw New Win32Exception
        End If

        Return enabled
    End Function

    Public Shared Sub ApplyRunOnStartup()
        Dim runKey As RegistryKey = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)

        If My.Settings.RunOnStartup Then
            runKey.SetValue(My.Application.Info.Title, """" + Application.ExecutablePath + """ /hidemainwindow")
        Else
            If runKey.GetValue(My.Application.Info.Title) IsNot Nothing Then
                runKey.DeleteValue(My.Application.Info.Title)
            End If
        End If
    End Sub
End Class

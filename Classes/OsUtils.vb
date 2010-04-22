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

Imports System.Runtime.InteropServices

<StructLayout(LayoutKind.Sequential, Pack:=4)> _
Public Structure RECT
    Public left As Integer
    Public top As Integer
    Public right As Integer
    Public bottom As Integer
End Structure

Friend Class OsUtils
    Public Shared Function WinSevenOrLater() As Boolean
        Dim curOs As OperatingSystem = System.Environment.OSVersion

        If curOs.Platform = PlatformID.Win32NT AndAlso (((curOs.Version.Major = 6) AndAlso (curOs.Version.Minor >= 1)) OrElse (curOs.Version.Major > 6)) Then
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
End Class

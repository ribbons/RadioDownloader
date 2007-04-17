' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007  www.nerdoftheherd.com
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

Imports System.IO

Module modMain
    Public Structure RECT
        Dim Left_Renamed As Integer
        Dim Top As Integer
        Dim Right_Renamed As Integer
        Dim Bottom As Integer
    End Structure

    Private Const GW_CHILD As Short = 5
    Private Const GW_HWNDNEXT As Short = 2
    Private Const IDANI_OPEN As Short = &H1S
    Private Const IDANI_CLOSE As Short = &H2S
    Private Const IDANI_CAPTION As Short = &H3S
    
    Public Declare Function DrawAnimatedRects Lib "user32" (ByVal hWnd As Integer, ByVal idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Integer, ByVal wCmd As Integer) As Integer
    Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As Integer, ByVal lpClassName As String, ByVal nMaxCount As Integer) As Integer
    Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As Integer, ByRef lpRect As RECT) As Integer

    Public Function GetSaveFolder() As String
        If My.Settings.SaveFolder <> "" Then
            If New DirectoryInfo(My.Settings.SaveFolder).exists = False Then
                My.Settings.SaveFolder = ""
            Else
                Return My.Settings.SaveFolder
            End If
        End If

        GetSaveFolder = My.Application.Info.DirectoryPath + "\Downloads"

        If New DirectoryInfo(GetSaveFolder).Exists = False Then
            Call New DirectoryInfo(GetSaveFolder).Create()
        End If
    End Function

    Public Function CreateSaveFileName(ByVal strFormatString As String, ByVal strProgTitle As String, ByVal strFileExtension As String) As String
        Dim strName As String = strFormatString

        strName = strName.Replace("%title%", strProgTitle)
        strName = strName.Replace("%day%", Now.ToString("dd"))
        strName = strName.Replace("%month%", Now.ToString("MM"))
        strName = strName.Replace("%shortmonthname%", Now.ToString("MMM"))
        strName = strName.Replace("%monthname%", Now.ToString("MMMM"))
        strName = strName.Replace("%year%", Now.ToString("yy"))
        strName = strName.Replace("%longyear%", Now.ToString("yyyy"))
        strName = strName.Replace("%ext%", strFileExtension)

        Dim strCleanedName As String
        Dim strTrimmedName As String = ""

        strCleanedName = Replace(strName, "\", " ")
        strCleanedName = Replace(strCleanedName, "/", " ")
        strCleanedName = Replace(strCleanedName, ":", " ")
        strCleanedName = Replace(strCleanedName, "*", " ")
        strCleanedName = Replace(strCleanedName, "?", " ")
        strCleanedName = Replace(strCleanedName, """", " ")
        strCleanedName = Replace(strCleanedName, ">", " ")
        strCleanedName = Replace(strCleanedName, "<", " ")
        strCleanedName = Replace(strCleanedName, "|", " ")

        Do While strTrimmedName <> strCleanedName
            strTrimmedName = strCleanedName
            strCleanedName = Replace(strCleanedName, "  ", " ")
        Loop

        Return Trim(strCleanedName)
    End Function

    Public Sub TrayAnimate(ByRef frmForm As System.Windows.Forms.Form, ByRef booDown As Boolean)
        Dim rctWindow As RECT
        Dim rctSystemTray As RECT

        rctSystemTray = GetSysTrayPos()
        Call GetWindowRect(frmForm.Handle.ToInt32, rctWindow)

        If booDown = True Then
            DrawAnimatedRects(frmForm.Handle.ToInt32, IDANI_CLOSE Or IDANI_CAPTION, rctWindow, rctSystemTray)
        Else
            DrawAnimatedRects(frmForm.Handle.ToInt32, IDANI_OPEN Or IDANI_CAPTION, rctSystemTray, rctWindow)
        End If
    End Sub

    Private Function GetSysTrayPos() As RECT
        Dim lngTaskbarHwnd As Integer
        Dim lngTrayHwnd As Integer
        Dim strClassName As String = Space(250)

        'Get taskbar handle
        lngTaskbarHwnd = FindWindow("Shell_traywnd", vbNullString)

        'Get system tray handle
        lngTrayHwnd = GetWindow(lngTaskbarHwnd, GW_CHILD)
        Do
            GetClassName(lngTrayHwnd, strClassName, 250)
            If TrimNull(strClassName) = "TrayNotifyWnd" Then Exit Do
            lngTrayHwnd = GetWindow(lngTrayHwnd, GW_HWNDNEXT)
        Loop

        Call GetWindowRect(lngTrayHwnd, GetSysTrayPos)
    End Function

    Public Function TrimNull(ByVal strString As String) As String
        Dim lngPos As Integer
        lngPos = InStr(strString, Chr(0))

        If lngPos > 0 Then strString = Left(strString, lngPos - 1)
        TrimNull = strString
    End Function
End Module
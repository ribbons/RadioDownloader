' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2009  www.nerdoftheherd.com
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

Friend Module modMain
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

    Private Declare Function DrawAnimatedRects Lib "user32" (ByVal hWnd As Integer, ByVal idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Integer, ByVal wCmd As Integer) As Integer
    Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As Integer, ByVal lpClassName As String, ByVal nMaxCount As Integer) As Integer
    Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As Integer, ByRef lpRect As RECT) As Integer

    Public Function GetSaveFolder() As String
        If My.Settings.SaveFolder <> "" Then
            If New DirectoryInfo(My.Settings.SaveFolder).Exists Then
                Return My.Settings.SaveFolder
            End If

            My.Settings.SaveFolder = ""
        End If

        Dim strMyDocs As String = My.Computer.FileSystem.SpecialDirectories.MyDocuments

        If strMyDocs.Substring(strMyDocs.Length - 1) = "\" Then
            strMyDocs = strMyDocs.Substring(0, strMyDocs.Length - 1)
        End If

        GetSaveFolder = strMyDocs + "\Downloaded Radio"

        If New DirectoryInfo(GetSaveFolder).Exists = False Then
            Call New DirectoryInfo(GetSaveFolder).Create()
        End If
    End Function

    Public Function GetAppDataFolder() As String
        Dim lngLastSlash As Integer
        lngLastSlash = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData.LastIndexOf("\")

        Return My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData.Substring(0, lngLastSlash)
    End Function

    Public Function FindFreeSaveFileName(ByVal strFormatString As String, ByVal strProgrammeName As String, ByVal strEpisodeName As String, ByVal dteEpisodeDate As Date, ByVal strSavePath As String) As String
        If strSavePath <> "" Then
            strSavePath += "\"
        End If

        Dim strSaveName As String = CreateSaveFileName(strFormatString, strProgrammeName, strEpisodeName, dteEpisodeDate)
        Dim intDiffNum As Integer = 1

        If strSavePath = "" Then
            ' This is only for the example on the prefs form, so don't check if the file already exists.
            Return strSaveName
        End If

        While Directory.GetFiles(strSavePath, strSaveName + ".*").Length > 0
            strSaveName = CreateSaveFileName(strFormatString + " (" + CStr(intDiffNum) + ")", strProgrammeName, strEpisodeName, dteEpisodeDate)
            intDiffNum += 1
        End While

        Return strSavePath + strSaveName
    End Function

    Private Function CreateSaveFileName(ByVal strFormatString As String, ByVal strProgrammeName As String, ByVal strEpisodeName As String, ByVal dteEpisodeDate As Date) As String
        Dim strName As String = strFormatString

        ' Convert %title% -> %epname% for backwards compatability
        strName = strName.Replace("%title%", "%epname%")

        ' Make variable substitutions
        strName = strName.Replace("%progname%", strProgrammeName)
        strName = strName.Replace("%epname%", strEpisodeName)
        strName = strName.Replace("%day%", dteEpisodeDate.ToString("dd"))
        strName = strName.Replace("%month%", dteEpisodeDate.ToString("MM"))
        strName = strName.Replace("%shortmonthname%", dteEpisodeDate.ToString("MMM"))
        strName = strName.Replace("%monthname%", dteEpisodeDate.ToString("MMMM"))
        strName = strName.Replace("%year%", dteEpisodeDate.ToString("yy"))
        strName = strName.Replace("%longyear%", dteEpisodeDate.ToString("yyyy"))

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

    Public Sub ExceptionHandler(ByVal sender As Object, ByVal e As UnhandledExceptionEventArgs)
        Dim expException As Exception
        expException = DirectCast(e.ExceptionObject, Exception)

        If ReportError.Visible = False Then
            Dim clsReport As New ErrorReporting(expException)
            ReportError.AssignReport(clsReport)
            ReportError.ShowDialog()
        End If
    End Sub

    Public Sub ThreadExceptionHandler(ByVal sender As Object, ByVal e As Threading.ThreadExceptionEventArgs)
        If ReportError.Visible = False Then
            Dim clsReport As New ErrorReporting(e.Exception)
            ReportError.AssignReport(clsReport)
            ReportError.ShowDialog()
        End If
    End Sub

    Public Sub StartupNextInstanceHandler(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs)
        ' This is called when another instance is started, so do the same as a double click on the tray icon
        Call Main.mnuTrayShow_Click(New Object, New System.EventArgs)
    End Sub
End Module
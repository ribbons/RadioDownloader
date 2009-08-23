' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2009 Matt Robinson
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

Imports System.IO

Friend Class FileUtils
    Public Shared Function GetSaveFolder() As String
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

    Public Shared Function GetAppDataFolder() As String
        Dim lngLastSlash As Integer
        lngLastSlash = My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData.LastIndexOf("\")

        Return My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData.Substring(0, lngLastSlash)
    End Function

    Public Shared Function FindFreeSaveFileName(ByVal strFormatString As String, ByVal strProgrammeName As String, ByVal strEpisodeName As String, ByVal dteEpisodeDate As Date, ByVal strSavePath As String) As String
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

    Private Shared Function CreateSaveFileName(ByVal strFormatString As String, ByVal strProgrammeName As String, ByVal strEpisodeName As String, ByVal dteEpisodeDate As Date) As String
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
End Class

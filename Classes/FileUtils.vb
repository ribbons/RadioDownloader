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

Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions

Friend Class FileUtils
    Public Shared Function GetSaveFolder() As String
        Const defaultFolder As String = "Downloaded Radio"

        If My.Settings.SaveFolder <> "" Then
            If New DirectoryInfo(My.Settings.SaveFolder).Exists = False Then
                Throw New DirectoryNotFoundException()
            End If

            Return My.Settings.SaveFolder
        End If

        Try
            GetSaveFolder = Path.Combine(My.Computer.FileSystem.SpecialDirectories.MyDocuments, defaultFolder)
        Catch dirNotFoundExp As DirectoryNotFoundException
            ' The user's Documents folder could not be found, so fall back to a folder under the system drive
            GetSaveFolder = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), defaultFolder)
        End Try

        Directory.CreateDirectory(GetSaveFolder)
    End Function

    Public Shared Function GetAppDataFolder() As String
        Return New DirectoryInfo(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData).Parent.FullName
    End Function

    Public Shared Function FindFreeSaveFileName(ByVal formatString As String, ByVal programmeName As String, ByVal episodeName As String, ByVal episodeDate As Date, ByVal baseSavePath As String) As String
        Dim saveName As String = CreateSaveFileName(formatString, programmeName, episodeName, episodeDate)
        Dim savePath As String = Path.Combine(baseSavePath, saveName)
        Dim diffNum As Integer = 1

        'Make sure the save folder exists (to support subfolders in the save file name template)
        Directory.CreateDirectory(Path.GetDirectoryName(savePath))

        While Directory.GetFiles(Path.GetDirectoryName(savePath), Path.GetFileName(savePath) + ".*").Length > 0
            savePath = Path.Combine(baseSavePath, saveName + " (" + CStr(diffNum) + ")")
            diffNum += 1
        End While

        Return savePath
    End Function

    Public Shared Function CreateSaveFileName(ByVal formatString As String, ByVal programmeName As String, ByVal episodeName As String, ByVal episodeDate As Date) As String
        Dim fileName As String = formatString

        ' Convert %title% -> %epname% for backwards compatability
        fileName = fileName.Replace("%title%", "%epname%")

        ' Make variable substitutions
        fileName = fileName.Replace("%progname%", programmeName)
        fileName = fileName.Replace("%epname%", episodeName)
        fileName = fileName.Replace("%day%", episodeDate.ToString("dd", CultureInfo.CurrentCulture))
        fileName = fileName.Replace("%month%", episodeDate.ToString("MM", CultureInfo.CurrentCulture))
        fileName = fileName.Replace("%shortmonthname%", episodeDate.ToString("MMM", CultureInfo.CurrentCulture))
        fileName = fileName.Replace("%monthname%", episodeDate.ToString("MMMM", CultureInfo.CurrentCulture))
        fileName = fileName.Replace("%year%", episodeDate.ToString("yy", CultureInfo.CurrentCulture))
        fileName = fileName.Replace("%longyear%", episodeDate.ToString("yyyy", CultureInfo.CurrentCulture))

        ' Replace invalid file name characters with spaces (except for directory separators
        ' as this then allows the flexibility of storing the downloads in subdirectories)
        For Each removeChar As Char In Path.GetInvalidFileNameChars
            If removeChar <> Path.DirectorySeparatorChar Then
                fileName = Replace(fileName, removeChar, " ")
            End If
        Next

        ' Replace runs of spaces with a single space
        fileName = Regex.Replace(fileName, " {2,}", " ")

        Return Trim(fileName)
    End Function
End Class

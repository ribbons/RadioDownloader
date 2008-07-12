' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2008  www.nerdoftheherd.com
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

Imports System.Web
Imports System.Net
Imports System.Reflection
Imports System.Text.Encoding
Imports System.Diagnostics.Process

Public Class clsErrorReporting
    Dim htbFields As New Hashtable

    Public Sub New(ByVal strError As String, ByVal strDetails As String)
        Try
            htbFields.Add("version", My.Application.Info.Version.ToString)
            htbFields.Add("errortext", strError)
            htbFields.Add("errordetails", strDetails)

            Dim strLoadedAssemblies As String = ""

            For Each LoadedAssembly As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                strLoadedAssemblies += LoadedAssembly.GetName.Name + vbCrLf
                strLoadedAssemblies += "Assembly Version: " + LoadedAssembly.GetName.Version.ToString + vbCrLf
                strLoadedAssemblies += "CodeBase: " + LoadedAssembly.CodeBase + vbCrLf + vbCrLf
            Next

            htbFields.Add("loadedassemblies", strLoadedAssemblies)
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Sub

    Public Overrides Function ToString() As String
        ToString = ""

        Try
            For Each htbItem As DictionaryEntry In htbFields
                ToString += CStr(htbItem.Key) + ": " + CStr(htbItem.Value) + vbCrLf
            Next
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Function

    Public Sub SendReport(ByVal strSendUrl As String)
        Try
            Dim webSend As New WebClient()
            webSend.Headers.Add("Content-Type", "application/x-www-form-urlencoded")

            Dim strPostData As String = ""

            For Each htbItem As DictionaryEntry In htbFields
                strPostData += "&" + CStr(htbItem.Key) + "=" + HttpUtility.UrlEncode(CStr(htbItem.Value))
            Next

            strPostData = strPostData.Substring(1)

            Dim Result As Byte() = webSend.UploadData(strSendUrl, "POST", ASCII.GetBytes(strPostData))
            Dim strReturnLines() As String = Split(ASCII.GetString(Result), vbLf)

            If strReturnLines(0) = "success" Then
                MsgBox("Your error report was sent successfully.", MsgBoxStyle.Information)

                If strReturnLines(1).Substring(0, 7) = "http://" Then
                    Start(strReturnLines(1))
                End If
            End If
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Sub
End Class

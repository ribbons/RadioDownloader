' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2009  Matt Robinson
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

Imports System.Collections.Generic
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text.Encoding
Imports System.Web
Imports System.Xml.Serialization

Friend Class ErrorReporting
    Dim fields As New Dictionary(Of String, String)

    Public Sub New(ByVal strError As String, ByVal strDetails As String)
        Try
            fields.Add("version", My.Application.Info.Version.ToString)
            fields.Add("errortext", strError)
            fields.Add("errordetails", strDetails)

            Dim loadedAssemblies As String = ""

            For Each loadedAssembly As Assembly In AppDomain.CurrentDomain.GetAssemblies()
                loadedAssemblies += loadedAssembly.GetName.Name + vbCrLf
                loadedAssemblies += "Assembly Version: " + loadedAssembly.GetName.Version.ToString + vbCrLf
                loadedAssemblies += "CodeBase: " + loadedAssembly.CodeBase + vbCrLf + vbCrLf
            Next

            fields.Add("loadedassemblies", loadedAssemblies)
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Sub

    Public Sub New(ByVal uncaughtException As Exception)
        Me.New(uncaughtException.GetType.ToString + ": " + uncaughtException.Message, uncaughtException.GetType.ToString + vbCrLf + uncaughtException.StackTrace)

        Try
            fields.Add("exceptiontostring", uncaughtException.ToString())

            ' Set up a list of types which do not need to be serialized
            Dim notSerialize As New List(Of Type)
            notSerialize.AddRange(New Type() {GetType(String), GetType(Integer), GetType(Single), GetType(Double), GetType(Boolean)})

            ' Store the type of the exception and get a list of its properties to loop through
            Dim exceptionType As Type = uncaughtException.GetType()
            Dim baseExceptionProperties() As PropertyInfo = GetType(Exception).GetProperties

            Dim extraProperty As Boolean

            For Each thisExpProperty As PropertyInfo In exceptionType.GetProperties
                extraProperty = True

                ' Check if this property exists in the base exception class: if not then add it to the report
                For Each baseProperty As PropertyInfo In baseExceptionProperties
                    If thisExpProperty.Name = baseProperty.Name Then
                        extraProperty = False
                        Exit For
                    End If
                Next

                If extraProperty Then
                    Dim propertyValue As Object = thisExpProperty.GetValue(uncaughtException, Nothing)

                    If propertyValue IsNot Nothing AndAlso propertyValue.ToString <> "" Then
                        If notSerialize.Contains(propertyValue.GetType) = False Then
                            If propertyValue.GetType.IsSerializable Then
                                ' Attempt to serialize the object as an XML string
                                Try
                                    Dim valueStringWriter As New StringWriter()
                                    Dim valueSerializer As New XmlSerializer(propertyValue.GetType)

                                    valueSerializer.Serialize(valueStringWriter, propertyValue)
                                    fields.Add("expdata:" + thisExpProperty.Name, valueStringWriter.ToString)

                                    Continue For
                                Catch notSupported As NotSupportedException
                                    ' Not possible to serialize - do nothing & fall through to the ToString code
                                Catch invalidOperation As InvalidOperationException
                                    ' Problem serializing the object - do nothing & fall through to the ToString code
                                End Try
                            End If
                        End If

                        fields.Add("expdata:" + thisExpProperty.Name, propertyValue.ToString)
                    End If
                End If
            Next
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Sub

    Public Overrides Function ToString() As String
        ToString = ""

        Try
            For Each reportField As KeyValuePair(Of String, String) In fields
                ToString += reportField.Key + ": " + reportField.Value + vbCrLf
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

            For Each reportField As KeyValuePair(Of String, String) In fields
                strPostData += "&" + HttpUtility.UrlEncode(reportField.Key) + "=" + HttpUtility.UrlEncode(reportField.Value)
            Next

            strPostData = strPostData.Substring(1)

            Dim Result As Byte() = webSend.UploadData(strSendUrl, "POST", ASCII.GetBytes(strPostData))
            Dim strReturnLines() As String = Split(ASCII.GetString(Result), vbLf)

            If strReturnLines(0) = "success" Then
                MsgBox("Your error report was sent successfully.", MsgBoxStyle.Information)

                If strReturnLines(1).Substring(0, 7) = "http://" Then
                    Process.Start(strReturnLines(1))
                End If
            End If
        Catch
            ' No way of reporting errors that have happened here, so just give up
        End Try
    End Sub
End Class

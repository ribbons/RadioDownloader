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

Imports System.IO
Imports System.Reflection
Imports System.Collections.Generic

Public Interface IRadioProvider
    Structure ProgrammeInfo
        Dim Name As String
        Dim Description As String
        Dim Image As Bitmap
        Dim Success As Boolean
    End Structure

    Structure EpisodeInfo
        Dim Name As String
        Dim Description As String
        Dim DurationSecs As Integer
        Dim [Date] As DateTime
        Dim Image As Bitmap
        Dim ExtInfo As Dictionary(Of String, String)
        Dim Success As Boolean
    End Structure

    Enum ProgressIcon
        Downloading
        Converting
    End Enum

    Enum ErrorType
        UnknownError
        MissingDependency
        ShorterThanExpected
    End Enum

    ReadOnly Property ProviderID() As Guid
    ReadOnly Property ProviderName() As String
    ReadOnly Property ProviderDescription() As String
    ReadOnly Property ProgInfoUpdateFreqDays() As Integer

    Function GetFindNewPanel(ByVal clsCachedHTTP As clsCachedWebClient) As Panel
    Function GetProgrammeInfo(ByVal clsCachedHTTP As clsCachedWebClient, ByVal strProgExtID As String) As ProgrammeInfo
    Function GetAvailableEpisodeIDs(ByVal clsCachedHTTP As clsCachedWebClient, ByVal strProgExtID As String) As String()
    Function GetEpisodeInfo(ByVal clsCachedHTTP As clsCachedWebClient, ByVal strProgExtID As String, ByVal strEpisodeExtID As String) As EpisodeInfo
    Function IsStillAvailable(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booIsLatestProg As Boolean) As Boolean

    Event FoundNew(ByVal strProgExtID As String)
    Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As ProgressIcon)
    Event DldError(ByVal errType As ErrorType, ByVal strErrorDetails As String)
    Event Finished()

    Sub DownloadProgram(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strProgDldUrl As String, ByVal strFinalName As String, ByVal intBandwidthLimitKBytes As Integer, ByVal intAttemptNumber As Integer)
End Interface

Public Class clsPlugins
    Private Const strInterfaceName As String = "IRadioProvider"
    Private htbPlugins As New Hashtable

    Private Structure AvailablePlugin
        Dim AssemblyPath As String
        Dim ClassName As String
    End Structure

    Public Function PluginExists(ByVal gidPluginID As Guid) As Boolean
        Return htbPlugins.ContainsKey(gidPluginID)
    End Function

    Public Function GetPluginInstance(ByVal gidPluginID As Guid) As IRadioProvider
        If PluginExists(gidPluginID) Then
            Return CreateInstance(DirectCast(htbPlugins.Item(gidPluginID), AvailablePlugin))
        Else
            Return Nothing
        End If
    End Function

    Public Function GetPluginIdList() As Guid()
        ReDim GetPluginIdList(htbPlugins.Keys.Count - 1)
        htbPlugins.Keys.CopyTo(GetPluginIdList, 0)
    End Function

    ' Next three functions are based on code from http://www.developerfusion.co.uk/show/4371/3/

    Public Sub New(ByVal strPath As String)
        Dim strDLLs() As String, intIndex As Integer
        Dim objDLL As Assembly

        'Go through all DLLs in the directory, attempting to load them
        strDLLs = Directory.GetFileSystemEntries(strPath, "*.dll")
        For intIndex = 0 To strDLLs.Length - 1
            Try
                objDLL = [Assembly].LoadFrom(strDLLs(intIndex))
                ExamineAssembly(objDLL)
            Catch
                'Error loading DLL, we don't need to do anything special
            End Try
        Next
    End Sub

    Private Sub ExamineAssembly(ByVal objDLL As Assembly)
        Dim objType As Type
        Dim objInterface As Type
        Dim Plugin As AvailablePlugin
        'Loop through each type in the DLL
        For Each objType In objDLL.GetTypes
            'Only look at public types
            If objType.IsPublic = True Then
                'Ignore abstract classes
                If Not ((objType.Attributes And TypeAttributes.Abstract) = TypeAttributes.Abstract) Then
                    'See if this type implements our interface
                    objInterface = objType.GetInterface(strInterfaceName, True)
                    If objInterface IsNot Nothing Then
                        Plugin = New AvailablePlugin()
                        Plugin.AssemblyPath = objDLL.Location
                        Plugin.ClassName = objType.FullName

                        Try
                            Dim objPlugin As IRadioProvider
                            objPlugin = CreateInstance(Plugin)
                            htbPlugins.Add(objPlugin.ProviderID, Plugin)
                        Catch
                            Continue For
                        End Try
                    End If
                End If
            End If
        Next
    End Sub

    Private Function CreateInstance(ByVal Plugin As AvailablePlugin) As IRadioProvider
        Dim objDLL As Assembly
        Dim objPlugin As Object

        Try
            'Load dll
            objDLL = Assembly.LoadFrom(Plugin.AssemblyPath)
            'Create and return class instance
            objPlugin = objDLL.CreateInstance(Plugin.ClassName)
            ' Cast to IRadioProvider and return
            Return DirectCast(objPlugin, IRadioProvider)
        Catch
            Return Nothing
        End Try
    End Function
End Class

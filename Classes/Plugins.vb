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

Imports System.IO
Imports System.Reflection
Imports System.Collections.Generic

<Serializable()> _
Public Class DldErrorDataItem
    Private itemName As String
    Private itemData As String

    Public Property Name() As String
        Get
            Return itemName
        End Get
        Set(ByVal value As String)
            itemName = value
        End Set
    End Property

    Public Property Data() As String
        Get
            Return itemData
        End Get
        Set(ByVal value As String)
            itemData = value
        End Set
    End Property

    Protected Sub New()
        ' Do nothing, just needed for deserialisation
    End Sub

    Public Sub New(ByVal name As String, ByVal data As String)
        itemName = name
        itemData = data
    End Sub
End Class

Public Interface IRadioProvider
    Structure ProgrammeInfo
        Dim Name As String
        Dim Description As String
        Dim Image As Bitmap
        Dim SingleEpisode As Boolean
    End Structure

    Structure GetProgrammeInfoReturn
        Dim ProgrammeInfo As ProgrammeInfo
        Dim Success As Boolean
        Dim Exception As Exception
    End Structure

    Structure EpisodeInfo
        Dim Name As String
        Dim Description As String
        Dim DurationSecs As Integer
        Dim [Date] As DateTime
        Dim Image As Bitmap
        Dim ExtInfo As Dictionary(Of String, String)
    End Structure

    Structure GetEpisodeInfoReturn
        Dim EpisodeInfo As EpisodeInfo
        Dim Success As Boolean
    End Structure

    Enum ProgressIcon
        Downloading
        Converting
    End Enum

    Enum ErrorType
        UnknownError = 0
        LocalProblem = 1
        ShorterThanExpected = 2
        NotAvailable = 3
        RemoveFromList = 4
        NotAvailableInLocation = 5
        NetworkProblem = 6
    End Enum

    ReadOnly Property ProviderID() As Guid
    ReadOnly Property ProviderName() As String
    ReadOnly Property ProviderIcon() As Bitmap
    ReadOnly Property ProviderDescription() As String
    ReadOnly Property ProgInfoUpdateFreqDays() As Integer

    Function GetShowOptionsHandler() As EventHandler
    Function GetFindNewPanel(ByVal view As Object) As Panel
    Function GetProgrammeInfo(ByVal progExtID As String) As GetProgrammeInfoReturn
    Function GetAvailableEpisodeIDs(ByVal progExtID As String) As String()
    Function GetEpisodeInfo(ByVal progExtID As String, ByVal episodeExtID As String) As GetEpisodeInfoReturn

    Event FindNewViewChange(ByVal view As Object)
    Event FindNewException(ByVal findExp As Exception, ByVal unhandled As Boolean)
    Event FoundNew(ByVal progExtID As String)
    Event Progress(ByVal percent As Integer, ByVal statusText As String, ByVal icon As ProgressIcon)
    Event DldError(ByVal type As ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem))
    Event Finished(ByVal fileExtension As String)

    Sub DownloadProgramme(ByVal progExtID As String, ByVal episodeExtID As String, ByVal progInfo As ProgrammeInfo, ByVal epInfo As EpisodeInfo, ByVal finalName As String, ByVal attempt As Integer)
End Interface

Friend Class Plugins
    Private Const interfaceName As String = "IRadioProvider"
    Private availablePlugins As New Dictionary(Of Guid, AvailablePlugin)

    Private Structure AvailablePlugin
        Dim AssemblyPath As String
        Dim ClassName As String
    End Structure

    Public Function PluginExists(ByVal pluginID As Guid) As Boolean
        Return availablePlugins.ContainsKey(pluginID)
    End Function

    Public Function GetPluginInstance(ByVal pluginID As Guid) As IRadioProvider
        If PluginExists(pluginID) Then
            Return CreateInstance(availablePlugins.Item(pluginID))
        Else
            Return Nothing
        End If
    End Function

    Public Function GetPluginIdList() As Guid()
        ReDim GetPluginIdList(availablePlugins.Keys.Count - 1)
        availablePlugins.Keys.CopyTo(GetPluginIdList, 0)
    End Function

    ' Next three functions are based on code from http://www.developerfusion.co.uk/show/4371/3/

    Public Sub New(ByVal path As String)
        Dim dlls() As String
        Dim thisDll As Assembly

        ' Go through the DLLs in the specified path and try to load them
        dlls = Directory.GetFileSystemEntries(path, "*.dll")

        For Each dll As String In dlls
            Try
                thisDll = [Assembly].LoadFrom(dll)
                ExamineAssembly(thisDll)
            Catch
                ' Error loading DLL, we don't need to do anything special
            End Try
        Next
    End Sub

    Private Sub ExamineAssembly(ByVal dll As Assembly)
        Dim thisType As Type
        Dim implInterface As Type
        Dim pluginInfo As AvailablePlugin

        ' Loop through each type in the assembly
        For Each thisType In dll.GetTypes

            ' Only look at public types
            If thisType.IsPublic = True Then

                ' Ignore abstract classes
                If Not ((thisType.Attributes And TypeAttributes.Abstract) = TypeAttributes.Abstract) Then

                    ' See if this type implements our interface
                    implInterface = thisType.GetInterface(interfaceName, True)

                    If implInterface IsNot Nothing Then
                        pluginInfo = New AvailablePlugin()
                        pluginInfo.AssemblyPath = dll.Location
                        pluginInfo.ClassName = thisType.FullName

                        Try
                            Dim pluginInst As IRadioProvider
                            pluginInst = CreateInstance(pluginInfo)
                            availablePlugins.Add(pluginInst.ProviderID, pluginInfo)
                        Catch
                            Continue For
                        End Try
                    End If
                End If
            End If
        Next
    End Sub

    Private Function CreateInstance(ByVal plugin As AvailablePlugin) As IRadioProvider
        Dim dll As Assembly
        Dim pluginInst As Object

        Try
            ' Load the assembly
            dll = Assembly.LoadFrom(plugin.AssemblyPath)

            ' Create and return class instance
            pluginInst = dll.CreateInstance(plugin.ClassName)

            ' Cast to an IRadioProvider and return
            Return DirectCast(pluginInst, IRadioProvider)
        Catch
            Return Nothing
        End Try
    End Function
End Class

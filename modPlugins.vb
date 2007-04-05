Option Strict On
Option Explicit On

Imports System.Reflection
Imports System.IO

Public Interface IRadioProvider
    Structure StationInfo
        Dim StationUniqueID As String
        Dim StationName As String
        Dim StationIcon As Icon
    End Structure

    Structure ProgramInfo
        Dim Success As Boolean
        Dim ProgramName As String
        Dim ProgramDescription As String
        Dim ProgramDuration As Long
        Dim ProgramDate As Date
        Dim ImageUrl As String
    End Structure

    Structure ProgramListItem
        Dim ProgramID As String
        Dim StationID As String
        Dim ProgramName As String
    End Structure

    Enum ProgressIcon
        Downloading
        Converting
    End Enum

    ReadOnly Property ProviderUniqueID() As String
    ReadOnly Property ProviderName() As String
    ReadOnly Property ProviderDescription() As String

    Function ReturnStations(ByRef clsCommon As clsCommon) As StationInfo()
    Function ListProgramIDs(ByRef clsCommon As clsCommon, ByVal strStationID As String) As ProgramListItem()
    Function GetLatestProgramInfo(ByRef clsCommon As clsCommon, ByVal strStationID As String, ByVal strProgramID As String) As ProgramInfo
    Function IsLatestProgram(ByRef clsCommon As clsCommon, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean
    Function IsStillAvailable(ByRef clsCommon As clsCommon, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean

    Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As ProgressIcon)
    Event DldError(ByVal strError As String)
    Event Finished()

    Sub DownloadProgram(ByRef clsCommon As clsCommon, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strFinalName As String)
End Interface

Module modPlugins
    Private Const strInterfaceName As String = "IRadioProvider"

    Public Structure AvailablePlugin
        Dim AssemblyPath As String
        Dim ClassName As String
    End Structure

    ' Next three functions are from http://www.developerfusion.co.uk/show/4371/3/

    Public Function FindPlugins(ByVal strPath As String) As AvailablePlugin()
        Dim Plugins As ArrayList = New ArrayList()
        Dim strDLLs() As String, intIndex As Integer
        Dim objDLL As [Assembly]
        'Go through all DLLs in the directory, attempting to load them
        strDLLs = Directory.GetFileSystemEntries(strPath, "*.dll")
        For intIndex = 0 To strDLLs.Length - 1
            Try
                objDLL = [Assembly].LoadFrom(strDLLs(intIndex))
                ExamineAssembly(objDLL, Plugins)
            Catch e As Exception
                'Error loading DLL, we don't need to do anything special
            End Try
        Next
        'Return all plugins found
        Dim Results(Plugins.Count - 1) As AvailablePlugin
        If Plugins.Count <> 0 Then
            Plugins.CopyTo(Results)
            Return Results
        Else
            Return Nothing
        End If
    End Function

    Private Sub ExamineAssembly(ByVal objDLL As [Assembly], ByVal Plugins As ArrayList)
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
                    If Not (objInterface Is Nothing) Then
                        'It does
                        Plugin = New AvailablePlugin()
                        Plugin.AssemblyPath = objDLL.Location
                        Plugin.ClassName = objType.FullName
                        Plugins.Add(Plugin)
                    End If
                End If
            End If
        Next
    End Sub

    Public Function CreateInstance(ByVal Plugin As AvailablePlugin) As Object
        Dim objDLL As [Assembly]
        Dim objPlugin As Object
        Try
            'Load dll
            objDLL = [Assembly].LoadFrom(Plugin.AssemblyPath)
            'Create and return class instance
            objPlugin = objDLL.CreateInstance(Plugin.ClassName)
        Catch e As Exception
            Return Nothing
        End Try
        Return objPlugin
    End Function
End Module

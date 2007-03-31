Option Strict Off
Option Explicit On

Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions

Friend Class clsBackground
    Private Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (ByVal lpExistingFileName As String, ByVal lpNewFileName As String) As Integer

    Private WithEvents ThisInstance As IRadioProvider

    Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    Event DldError(ByVal strError As String)
    Event Finished()

    ' Private variables to store information about the current task
    Private strProgType As String
    Private strProgID As String
    Private intDuration As Integer
    Private dteProgDate As Date
    Private strProgTitle As String
    Private strStationID As String
    Private strFinalName As String
    Private AvailablePlugins As AvailablePlugin()

    Public Property ProgramType() As String
        Get
            ProgramType = strProgType
        End Get
        Set(ByVal strProgramType As String)
            strProgType = strProgramType
        End Set
    End Property

    Public Property StationID() As String
        Get
            Return strStationID
        End Get
        Set(ByVal strInStationID As String)
            strStationID = strInStationID
        End Set
    End Property

    Public Property ProgramID() As String
        Get
            ProgramID = strProgID
        End Get
        Set(ByVal strProgramID As String)
            strProgID = strProgramID
        End Set
    End Property

    Public Property ProgramDuration() As Integer
        Get
            Return intDuration
        End Get
        Set(ByVal intInDuration As Integer)
            intDuration = intInDuration
        End Set
    End Property

    Public Property ProgramDate() As Date
        Get
            ProgramDate = dteProgDate
        End Get
        Set(ByVal dteProgramDate As Date)
            dteProgDate = dteProgramDate
        End Set
    End Property

    Public ReadOnly Property FinalName() As String
        Get
            FinalName = strFinalName
        End Get
    End Property

    Public Property PluginsList() As AvailablePlugin()
        Get
            Return AvailablePlugins
        End Get
        Set(ByVal InPluginsList As AvailablePlugin())
            AvailablePlugins = InPluginsList
        End Set
    End Property

    Public Sub DownloadProgram()
        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = CreateInstance(SinglePlugin)

            If ThisInstance.ProviderUniqueID = strProgType Then
                Exit For
            End If
        Next SinglePlugin

        strFinalName = CreateFinalName()

        ThisInstance.DownloadProgram(New clsCommon, strStationID, strProgID, intDuration, strFinalName)
    End Sub

    Private Function CreateFinalName() As String
        Dim strCleanedTitle As String
        Dim strTrimmedTitle As String = ""

        strCleanedTitle = Replace(strProgTitle, "\", " ")
        strCleanedTitle = Replace(strCleanedTitle, "/", " ")
        strCleanedTitle = Replace(strCleanedTitle, ":", " ")
        strCleanedTitle = Replace(strCleanedTitle, "*", " ")
        strCleanedTitle = Replace(strCleanedTitle, "?", " ")
        strCleanedTitle = Replace(strCleanedTitle, """", " ")
        strCleanedTitle = Replace(strCleanedTitle, ">", " ")
        strCleanedTitle = Replace(strCleanedTitle, "<", " ")
        strCleanedTitle = Replace(strCleanedTitle, "|", " ")

        Do While strTrimmedTitle <> strCleanedTitle
            strTrimmedTitle = strCleanedTitle
            strCleanedTitle = Replace(strCleanedTitle, "  ", " ")
        Loop

        Dim strFolderPath As String

        If My.Settings.SaveFolder <> "" Then
            strFolderPath = My.Settings.SaveFolder
        Else
            strFolderPath = My.Application.Info.DirectoryPath + "\Downloads"
        End If

        CreateFinalName = "\" + Trim(strCleanedTitle) + " " + dteProgDate.ToString("dd-mm-yy") + ".mp3"
    End Function

    Private Sub ThisInstance_DldError(ByVal strError As String) Handles ThisInstance.DldError
        RaiseEvent DldError(strError)
    End Sub

    Private Sub ThisInstance_Finished() Handles ThisInstance.Finished
        RaiseEvent Finished()
    End Sub

    Private Sub ThisInstance_Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles ThisInstance.Progress
        RaiseEvent Progress(intPercent, strStatusText, Icon)
    End Sub
End Class
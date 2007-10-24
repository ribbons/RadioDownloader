' Plugin for Radio Downloader to download programs made from the BBC as Podcasts.
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

Imports RadioDld

Public Class clsBBCPodcasts
    Implements IRadioProvider

    Public Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Implements IRadioProvider.Progress
    Public Event DldError(ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Implements IRadioProvider.DldError
    Public Event Finished() Implements IRadioProvider.Finished

    ReadOnly Property ProviderUniqueID() As String Implements IRadioProvider.ProviderUniqueID
        Get
            Return "BBCPODCAST"
        End Get
    End Property

    ReadOnly Property ProviderName() As String Implements IRadioProvider.ProviderName
        Get
            Return "BBC Podcasts"
        End Get
    End Property

    ReadOnly Property ProviderDescription() As String Implements IRadioProvider.ProviderDescription
        Get
            Return "Allows you to download programs made available by the BBC as Podcasts."
        End Get
    End Property

    Function ReturnStations() As IRadioProvider.StationTable Implements IRadioProvider.ReturnStations
        Dim Stations As New IRadioProvider.StationTable

        Return Stations
    End Function

    Function ListProgramIDs(ByVal strStationID As String) As IRadioProvider.ProgramListItem() Implements IRadioProvider.ListProgramIDs
        Dim ProgramIDList() As IRadioProvider.ProgramListItem
        ReDim ProgramIDList(0)

        Return ProgramIDList
    End Function

    Function GetLatestProgramInfo(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteLastInfoFor As Date, ByVal dteLastAttempt As Date) As IRadioProvider.ProgramInfo Implements IRadioProvider.GetLatestProgramInfo
        Dim ProgInfo As IRadioProvider.ProgramInfo = Nothing

        Return ProgInfo
    End Function

    Function CouldBeNewEpisode(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean Implements IRadioProvider.CouldBeNewEpisode

    End Function

    Function IsStillAvailable(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booIsLatestProg As Boolean) As Boolean Implements IRadioProvider.IsStillAvailable

    End Function

    Sub DownloadProgram(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strFinalName As String, ByVal intBandwidthLimitKBytes As Integer) Implements IRadioProvider.DownloadProgram

    End Sub
End Class

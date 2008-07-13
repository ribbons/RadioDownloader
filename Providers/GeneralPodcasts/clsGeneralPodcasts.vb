' Plugin for Radio Downloader to download general podcasts.
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

Imports RadioDld
Imports System.Xml
Imports System.Net
Imports System.Windows.Forms

Public Class clsGeneralPodcasts
    Implements IRadioProvider

    Public Event FoundNew(ByVal gidPluginID As Guid, ByVal strProgExtID As String) Implements IRadioProvider.FoundNew
    Public Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Implements IRadioProvider.Progress
    Public Event DldError(ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Implements IRadioProvider.DldError
    Public Event Finished() Implements IRadioProvider.Finished

    Friend clsCachedHTTP As RadioDld.clsCachedWebClient
    Friend Const intCacheHTTPHours As Integer = 2

    Public ReadOnly Property ProviderID() As Guid Implements IRadioProvider.ProviderID
        Get
            Return New Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921")
        End Get
    End Property

    Public ReadOnly Property ProviderName() As String Implements IRadioProvider.ProviderName
        Get
            Return "General Podcasts"
        End Get
    End Property

    Public ReadOnly Property ProviderDescription() As String Implements IRadioProvider.ProviderDescription
        Get
            Return "Allows you to download programmes made available as Podcasts."
        End Get
    End Property

    ReadOnly Property ProgInfoUpdateFreqDays() As Integer Implements IRadioProvider.ProgInfoUpdateFreqDays
        Get
            ' Updating the programme info every week should be a reasonable trade-off
            Return 7
        End Get
    End Property

    ReadOnly Property DynamicSubscriptionName() As Boolean Implements IRadioProvider.DynamicSubscriptionName
        Get
            Return False
        End Get
    End Property

    Public Function GetFindNewPanel(ByVal clsCachedHTTP As clsCachedWebClient) As Panel Implements IRadioProvider.GetFindNewPanel
        Me.clsCachedHTTP = clsCachedHTTP

        Dim frmFindNewInst As New frmFindNew
        frmFindNewInst.clsPluginInst = Me
        Return frmFindNewInst.pnlFindNew
    End Function

    Public Function GetProgrammeInfo(ByVal clsCachedHTTP As clsCachedWebClient, ByVal strProgExtID As String) As IRadioProvider.ProgrammeInfo Implements IRadioProvider.GetProgrammeInfo
        Dim ProgInfo As New IRadioProvider.ProgrammeInfo
        ProgInfo.Success = False

        Dim strRSS As String
        Dim xmlRSS As New XmlDocument

        Try
            strRSS = clsCachedHTTP.DownloadString(strProgExtID, intCacheHTTPHours)
        Catch expWeb As WebException
            Return ProgInfo
        End Try

        Try
            xmlRSS.LoadXml(strRSS)
        Catch expXML As XmlException
            Return ProgInfo
        End Try

        Dim xmlTitle As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/title")
        Dim xmlDescription As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/description")

        If xmlTitle Is Nothing Or xmlDescription Is Nothing Then
            Return ProgInfo
        End If

        ProgInfo.Name = xmlTitle.InnerText
        ProgInfo.Description = xmlDescription.InnerText

        If ProgInfo.Name = "" Then
            ProgInfo.Description = ""
            Return ProgInfo
        End If

        Try
            Dim strImageUrl As String = xmlRSS.SelectSingleNode("./rss/channel/image/url").InnerText
            Dim bteImageData As Byte() = clsCachedHTTP.DownloadData(strImageUrl, intCacheHTTPHours)
            ProgInfo.Image = New System.Drawing.Bitmap(New IO.MemoryStream(bteImageData))
        Catch
            ProgInfo.Image = Nothing
        End Try

        ProgInfo.Success = True
        Return ProgInfo
    End Function

    Public Function CouldBeNewEpisode(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean Implements IRadioProvider.CouldBeNewEpisode

    End Function

    Public Function IsStillAvailable(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booIsLatestProg As Boolean) As Boolean Implements IRadioProvider.IsStillAvailable

    End Function

    Public Sub DownloadProgram(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strProgDldUrl As String, ByVal strFinalName As String, ByVal intBandwidthLimitKBytes As Integer, ByVal intAttemptNumber As Integer) Implements IRadioProvider.DownloadProgram

    End Sub

    Friend Sub RaiseFoundNew(ByVal strExtID As String)
        RaiseEvent FoundNew(Me.ProviderID, strExtID)
    End Sub
End Class

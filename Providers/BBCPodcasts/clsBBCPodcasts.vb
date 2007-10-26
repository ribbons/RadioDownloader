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
Imports System.IO
Imports System.Net
Imports System.Web
Imports System.Xml
Imports HtmlAgilityPack

Public Class clsBBCPodcasts
    Implements IRadioProvider

    Public Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Implements IRadioProvider.Progress
    Public Event DldError(ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Implements IRadioProvider.DldError
    Public Event Finished() Implements IRadioProvider.Finished

    Private WithEvents webDownload As WebClient
    Private strDownloadFileName As String
    Private strFinalName As String

    Public ReadOnly Property ProviderUniqueID() As String Implements IRadioProvider.ProviderUniqueID
        Get
            Return "BBCPODCAST"
        End Get
    End Property

    Public ReadOnly Property ProviderName() As String Implements IRadioProvider.ProviderName
        Get
            Return "BBC Podcasts"
        End Get
    End Property

    Public ReadOnly Property ProviderDescription() As String Implements IRadioProvider.ProviderDescription
        Get
            Return "Allows you to download programs made available by the BBC as Podcasts."
        End Get
    End Property

    Public Function ReturnStations() As IRadioProvider.StationTable Implements IRadioProvider.ReturnStations
        Dim Stations As New IRadioProvider.StationTable

        Stations.Add("radio1", "Radio 1 Podcasts")
        Stations.Add("1xtra", "1Xtra Podcasts")
        Stations.Add("radio2", "Radio 2 Podcasts")
        Stations.Add("radio3", "Radio 3 Podcasts")
        Stations.Add("radio4", "Radio 4 Podcasts")
        Stations.Add("fivelive", "Five Live Podcasts")
        Stations.Add("6music", "Six Music Podcasts")
        Stations.Add("bbc7", "BBC 7 Podcasts")
        Stations.Add("asiannet", "BBC Asian Network Podcasts")
        Stations.Add("wservice", "BBC World Service Podcasts")
        Stations.Add("scotland", "Radio Scotland / Nan Gaidheal Podcasts")
        Stations.Add("ulster", "Radio Ulster / Foyle Podcasts")
        Stations.Add("wales", "Radio Wales / Cymru Podcasts")
        Stations.Add("local", "English Local Radio Podcasts")

        Return Stations
    End Function

    Public Function ListProgramIDs(ByVal strStationID As String) As IRadioProvider.ProgramListItem() Implements IRadioProvider.ListProgramIDs
        Dim ProgramIDList() As IRadioProvider.ProgramListItem
        ReDim ProgramIDList(-1)

        Try
            Dim webClient As New WebClient
            Dim strReturned As String = webClient.DownloadString("http://www.bbc.co.uk/radio/podcasts/directory/station/" + strStationID + "/")

            Dim htmDocument As New HtmlDocument
            htmDocument.LoadHtml(strReturned)

            Dim divResults As HtmlNode = htmDocument.GetElementbyId("results_cells")

            For Each divProgram As HtmlNode In (divResults.SelectNodes("./div"))
                ReDim Preserve ProgramIDList(ProgramIDList.GetUpperBound(0) + 1)

                Try
                    With ProgramIDList(ProgramIDList.GetUpperBound(0))
                        Dim TitleLink As HtmlNode
                        TitleLink = divProgram.SelectSingleNode("./div/div/div/div/div[1]/h3/a")

                        .StationID = strStationID
                        .ProgramID = strStationID + "/" + TitleLink.Attributes("href").Value.Substring("/radio/podcasts/".Length).TrimEnd("/".ToCharArray)
                        .ProgramName = HttpUtility.HtmlDecode(TitleLink.InnerText)
                    End With
                Catch
                    ' Remove this item, as it is incomplete
                    ReDim Preserve ProgramIDList(ProgramIDList.GetUpperBound(0) - 1)
                End Try
            Next
        Catch
            ' Empty the program list, as it might contain bad data
            ReDim ProgramIDList(-1)
        End Try

        Return ProgramIDList
    End Function

    Public Function GetLatestProgramInfo(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteLastInfoFor As Date, ByVal dteLastAttempt As Date) As IRadioProvider.ProgramInfo Implements IRadioProvider.GetLatestProgramInfo
        Dim ProgInfo As IRadioProvider.ProgramInfo = Nothing

        ' Important check to make sure we don't hammer the server with too frequent requests.
        If dteLastAttempt <> Nothing Then
            If dteLastAttempt.AddHours(2) > Now Then
                ' Last tried to get information less than an hour ago, so don't try again.
                ProgInfo.Result = IRadioProvider.ProgInfoResult.Skipped
                Return ProgInfo
            End If
        End If

        Debug.WriteLine("BBCPODCAST: Downloading info for: " + strProgramID)

        Dim strFeed As String
        Dim webClient As New WebClient

        Try
            strFeed = webClient.DownloadString("http://downloads.bbc.co.uk/podcasts/" + strProgramID + "/rss.xml")
        Catch exWebExp As System.Net.WebException
            ' Pass back a temp error as its most likely no net connection
            ProgInfo.Result = IRadioProvider.ProgInfoResult.TempError
            Return ProgInfo
        End Try

        Try
            Dim xmlFeed As New XmlDocument
            xmlFeed.LoadXml(strFeed)

            Dim xmlItems As XmlNodeList
            xmlItems = xmlFeed.SelectNodes("./rss/channel/item")

            Dim strTitle As String = ""
            Dim strDescription As String = ""
            Dim intDuration As Integer
            Dim dteDate As Date = New Date(0)
            Dim strDldUrl As String = ""

            For Each xmlItem As XmlNode In xmlItems
                If CDate(xmlItem.SelectSingleNode("./pubDate").InnerText) > dteDate Then
                    strTitle = xmlItem.SelectSingleNode("./title").InnerText
                    strDescription = xmlItem.SelectSingleNode("./description").InnerText
                    intDuration = CInt(CInt(xmlItem.SelectSingleNode("./media:content", CreateNamespaceMgr(xmlFeed)).Attributes("duration").Value) / 60)
                    dteDate = CDate(xmlItem.SelectSingleNode("./pubDate").InnerText)
                    strDldUrl = xmlItem.SelectSingleNode("./enclosure").Attributes("url").Value
                End If
            Next

            If strTitle = "" Then
                ProgInfo.Result = IRadioProvider.ProgInfoResult.OtherError
                Return ProgInfo
            End If

            With ProgInfo
                .ProgramName = strTitle
                .ProgramDescription = strDescription
                .ProgramDuration = intDuration
                .ProgramDate = dteDate
                .ProgramDldUrl = strDldUrl

                Try
                    .Image = New System.Drawing.Bitmap(New IO.MemoryStream(New System.Net.WebClient().DownloadData(xmlFeed.SelectSingleNode(".//rss/channel/image/url").InnerText)))
                Catch
                    .Image = Nothing
                End Try
            End With


        Catch expException As Exception
            ProgInfo.Result = IRadioProvider.ProgInfoResult.OtherError
            Return ProgInfo
        End Try

        ProgInfo.Result = IRadioProvider.ProgInfoResult.Success
        Return ProgInfo
    End Function

    Public Function CouldBeNewEpisode(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean Implements IRadioProvider.CouldBeNewEpisode
        Return dteProgramDate.AddDays(1) > Now
    End Function

    Public Function IsStillAvailable(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booIsLatestProg As Boolean) As Boolean Implements IRadioProvider.IsStillAvailable
        Debug.WriteLine("BBCPODCAST: Downloading info for: " + strProgramID)

        Dim strFeed As String
        Dim webClient As New WebClient

        Try
            strFeed = webClient.DownloadString("http://downloads.bbc.co.uk/podcasts/" + strProgramID + "/rss.xml")
        Catch exWebExp As System.Net.WebException
            ' So that programs don't just disappear out of the download list because of a dropped net connection, then
            ' guess that it is still available.
            Return True
        End Try

        Try
            Dim xmlFeed As New XmlDocument
            xmlFeed.LoadXml(strFeed)

            Dim xmlItems As XmlNodeList
            xmlItems = xmlFeed.SelectNodes("./rss/channel/item")

            For Each xmlItem As XmlNode In xmlItems
                If dteProgramDate = CDate(xmlItem.SelectSingleNode("./pubDate").InnerText) Then
                    Return True
                End If
            Next
        Catch expException As Exception
            ' So that programs don't just disappear out of the download list because of an error, then guess that it
            ' is still available.
            Return True
        End Try

        Return False
    End Function

    Public Sub DownloadProgram(ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal intProgLength As Integer, ByVal strProgDldUrl As String, ByVal strFinalName As String, ByVal intBandwidthLimitKBytes As Integer) Implements IRadioProvider.DownloadProgram
        Dim lngTrimPos As Integer = InStr(1, strProgramID, "/")
        strDownloadFileName = System.IO.Path.GetTempPath + "\RadioDownloader\" + Mid(strProgramID, lngTrimPos + 1) + Right(strProgDldUrl, 4)
        Me.strFinalName = strFinalName

        webDownload = New WebClient
        webDownload.DownloadFileAsync(New Uri(strProgDldUrl), strDownloadFileName)
    End Sub

    Private Function CreateNamespaceMgr(ByVal xmlDocument As XmlDocument) As XmlNamespaceManager
        Dim nsManager As New XmlNamespaceManager(xmlDocument.NameTable)

        For Each xmlAttrib As XmlAttribute In xmlDocument.SelectSingleNode("/*").Attributes
            If xmlAttrib.Prefix = "xmlns" Then
                nsManager.AddNamespace(xmlAttrib.LocalName, xmlAttrib.Value)
            End If
        Next

        Return nsManager
    End Function

    Private Sub webDownload_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles webDownload.DownloadFileCompleted
        If e.Error Is Nothing = False Then
            RaiseEvent DldError(IRadioProvider.ErrorType.UnknownError, e.Error.Message + vbCrLf + e.Error.StackTrace)
        Else
            RaiseEvent Progress(100, "Downloading...", IRadioProvider.ProgressIcon.Downloading)

            Try
                Call File.Move(strDownloadFileName, strFinalName)
            Catch
                RaiseEvent DldError(IRadioProvider.ErrorType.UnknownError, e.Error.StackTrace)
                Exit Sub
            End Try

            RaiseEvent Finished()
        End If
    End Sub

    Private Sub webDownload_DownloadProgressChanged(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles webDownload.DownloadProgressChanged
        Dim intPercent As Integer = e.ProgressPercentage

        If intPercent > 99 Then
            intPercent = 99
        End If

        RaiseEvent Progress(intPercent, "Downloading...", IRadioProvider.ProgressIcon.Downloading)
    End Sub
End Class

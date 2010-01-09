' Plugin for Radio Downloader to download general podcasts.
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

Option Strict On
Option Explicit On

Imports RadioDld
Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Web
Imports System.Drawing
Imports Microsoft.Win32
Imports System.Globalization
Imports System.Windows.Forms
Imports System.Text.RegularExpressions

Public Class PodcastProvider
    Implements IRadioProvider

    Public Event FindNewViewChange(ByVal objView As Object) Implements IRadioProvider.FindNewViewChange
    Public Event FindNewException(ByVal exception As Exception, ByVal unhandled As Boolean) Implements IRadioProvider.FindNewException
    Public Event FoundNew(ByVal strProgExtID As String) Implements IRadioProvider.FoundNew
    Public Event Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Implements IRadioProvider.Progress
    Public Event DldError(ByVal errorType As IRadioProvider.ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem)) Implements IRadioProvider.DldError
    Public Event Finished(ByVal strFileExtension As String) Implements IRadioProvider.Finished

    Friend Const intCacheHTTPHours As Integer = 2

    Private WithEvents webDownload As WebClient

    Private strDownloadFileName As String
    Private strProgDldUrl As String
    Private strFinalName As String
    Private strExtension As String = ""

    Public ReadOnly Property ProviderID() As Guid Implements IRadioProvider.ProviderID
        Get
            Return New Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921")
        End Get
    End Property

    Public ReadOnly Property ProviderName() As String Implements IRadioProvider.ProviderName
        Get
            Return "Podcast"
        End Get
    End Property

    Public ReadOnly Property ProviderIcon() As Bitmap Implements IRadioProvider.ProviderIcon
        Get
            Return My.Resources.provider_icon
        End Get
    End Property

    Public ReadOnly Property ProviderDescription() As String Implements IRadioProvider.ProviderDescription
        Get
            Return "Audio files made available as enclosures on an RSS feed."
        End Get
    End Property

    ReadOnly Property ProgInfoUpdateFreqDays() As Integer Implements IRadioProvider.ProgInfoUpdateFreqDays
        Get
            ' Updating the programme info every week should be a reasonable trade-off
            Return 7
        End Get
    End Property

    Public Function GetShowOptionsHandler() As EventHandler Implements IRadioProvider.GetShowOptionsHandler
        Return Nothing
    End Function

    Public Function GetFindNewPanel(ByVal objView As Object) As Panel Implements IRadioProvider.GetFindNewPanel
        Dim FindNewInst As New FindNew
        FindNewInst.clsPluginInst = Me
        Return FindNewInst.pnlFindNew
    End Function

    Public Function GetProgrammeInfo(ByVal strProgExtID As String) As IRadioProvider.GetProgrammeInfoReturn Implements IRadioProvider.GetProgrammeInfo
        Dim getProgInfo As New IRadioProvider.GetProgrammeInfoReturn
        getProgInfo.Success = False

        Dim cachedWeb As New CachedWebClient
        Dim strRSS As String
        Dim xmlRSS As New XmlDocument
        Dim xmlNamespaceMgr As XmlNamespaceManager

        Try
            strRSS = cachedWeb.DownloadString(strProgExtID, intCacheHTTPHours)
        Catch expWeb As WebException
            Return getProgInfo
        End Try

        Try
            xmlRSS.LoadXml(strRSS)
        Catch expXML As XmlException
            Return getProgInfo
        End Try

        Try
            xmlNamespaceMgr = CreateNamespaceMgr(xmlRSS)
        Catch
            xmlNamespaceMgr = Nothing
        End Try

        Dim xmlTitle As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/title")
        Dim xmlDescription As XmlNode = xmlRSS.SelectSingleNode("./rss/channel/description")

        If xmlTitle Is Nothing Or xmlDescription Is Nothing Then
            Return getProgInfo
        End If

        getProgInfo.ProgrammeInfo.Name = xmlTitle.InnerText

        If getProgInfo.ProgrammeInfo.Name = "" Then
            Return getProgInfo
        End If

        getProgInfo.ProgrammeInfo.Description = xmlDescription.InnerText
        getProgInfo.ProgrammeInfo.Image = RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr)

        getProgInfo.Success = True
        Return getProgInfo
    End Function

    Public Function GetAvailableEpisodeIDs(ByVal strProgExtID As String) As String() Implements IRadioProvider.GetAvailableEpisodeIDs
        Dim strEpisodeIDs(-1) As String
        GetAvailableEpisodeIDs = strEpisodeIDs

        Dim cachedWeb As New CachedWebClient
        Dim strRSS As String
        Dim xmlRSS As New XmlDocument

        Try
            strRSS = cachedWeb.DownloadString(strProgExtID, intCacheHTTPHours)
        Catch expWeb As WebException
            Exit Function
        End Try

        Try
            xmlRSS.LoadXml(strRSS)
        Catch expXML As XmlException
            Exit Function
        End Try

        Dim xmlItems As XmlNodeList
        xmlItems = xmlRSS.SelectNodes("./rss/channel/item")

        If xmlItems Is Nothing Then
            Exit Function
        End If

        Dim strItemID As String

        For Each xmlItem As XmlNode In xmlItems
            strItemID = ItemNodeToEpisodeID(xmlItem)

            If strItemID <> "" Then
                ReDim Preserve strEpisodeIDs(strEpisodeIDs.GetUpperBound(0) + 1)
                strEpisodeIDs(strEpisodeIDs.GetUpperBound(0)) = strItemID
            End If
        Next

        Return strEpisodeIDs
    End Function

    Function GetEpisodeInfo(ByVal strProgExtID As String, ByVal strEpisodeExtID As String) As IRadioProvider.GetEpisodeInfoReturn Implements IRadioProvider.GetEpisodeInfo
        Dim episodeInfoReturn As New IRadioProvider.GetEpisodeInfoReturn
        episodeInfoReturn.Success = False

        Dim cachedWeb As New CachedWebClient
        Dim strRSS As String
        Dim xmlRSS As New XmlDocument
        Dim xmlNamespaceMgr As XmlNamespaceManager

        Try
            strRSS = cachedWeb.DownloadString(strProgExtID, intCacheHTTPHours)
        Catch expWeb As WebException
            Return episodeInfoReturn
        End Try

        Try
            xmlRSS.LoadXml(strRSS)
        Catch expXML As XmlException
            Return episodeInfoReturn
        End Try

        Try
            xmlNamespaceMgr = CreateNamespaceMgr(xmlRSS)
        Catch
            xmlNamespaceMgr = Nothing
        End Try

        Dim xmlItems As XmlNodeList
        xmlItems = xmlRSS.SelectNodes("./rss/channel/item")

        If xmlItems Is Nothing Then
            Return episodeInfoReturn
        End If

        Dim strItemID As String

        For Each xmlItem As XmlNode In xmlItems
            strItemID = ItemNodeToEpisodeID(xmlItem)

            If strItemID = strEpisodeExtID Then
                Dim xmlTitle As XmlNode = xmlItem.SelectSingleNode("./title")
                Dim xmlDescription As XmlNode = xmlItem.SelectSingleNode("./description")
                Dim xmlPubDate As XmlNode = xmlItem.SelectSingleNode("./pubDate")
                Dim xmlEnclosure As XmlNode = xmlItem.SelectSingleNode("./enclosure")

                If xmlEnclosure Is Nothing Then
                    Return episodeInfoReturn
                End If

                Dim xmlUrl As XmlAttribute = xmlEnclosure.Attributes("url")

                If xmlUrl Is Nothing Then
                    Return episodeInfoReturn
                End If

                Try
                    Dim uriTestValid As New Uri(xmlUrl.Value)
                Catch expUriFormat As UriFormatException
                    ' The enclosure url is empty or malformed, so return false for success
                    Return episodeInfoReturn
                End Try

                Dim dicExtInfo As New Dictionary(Of String, String)
                dicExtInfo.Add("EnclosureURL", xmlUrl.Value)

                If xmlTitle IsNot Nothing Then
                    episodeInfoReturn.EpisodeInfo.Name = xmlTitle.InnerText
                End If

                If episodeInfoReturn.EpisodeInfo.Name = "" Then
                    Return episodeInfoReturn
                End If

                If xmlDescription IsNot Nothing Then
                    Dim description As String = xmlDescription.InnerText

                    ' Replace common block level tags with newlines
                    description = description.Replace("<br", vbCrLf + "<br")
                    description = description.Replace("<p", vbCrLf + "<p")
                    description = description.Replace("<div", vbCrLf + "<div")

                    ' Replace HTML entities with their character counterparts
                    description = HttpUtility.HtmlDecode(description)

                    ' Strip out any HTML tags
                    Dim RegExpression As New Regex("<(.|\n)+?>")
                    episodeInfoReturn.EpisodeInfo.Description = RegExpression.Replace(description, "")
                End If

                Try
                    Dim xmlDuration As XmlNode = xmlItem.SelectSingleNode("./itunes:duration", xmlNamespaceMgr)

                    If xmlDuration IsNot Nothing Then
                        Dim strSplitDuration() As String = Split(xmlDuration.InnerText.Replace(".", ":"), ":")

                        If strSplitDuration.GetUpperBound(0) = 0 Then
                            episodeInfoReturn.EpisodeInfo.DurationSecs = CInt(strSplitDuration(0))
                        ElseIf strSplitDuration.GetUpperBound(0) = 1 Then
                            episodeInfoReturn.EpisodeInfo.DurationSecs = (CInt(strSplitDuration(0)) * 60) + CInt(strSplitDuration(1))
                        Else
                            episodeInfoReturn.EpisodeInfo.DurationSecs = ((CInt(strSplitDuration(0)) * 60) + CInt(strSplitDuration(1))) * 60 + CInt(strSplitDuration(2))
                        End If
                    Else
                        episodeInfoReturn.EpisodeInfo.DurationSecs = Nothing
                    End If
                Catch
                    episodeInfoReturn.EpisodeInfo.DurationSecs = Nothing
                End Try

                If xmlPubDate IsNot Nothing Then
                    Dim strPubDate As String = xmlPubDate.InnerText.Trim
                    Dim intZonePos As Integer = strPubDate.LastIndexOf(" ", StringComparison.Ordinal)
                    Dim tspOffset As TimeSpan = New TimeSpan(0)

                    If intZonePos > 0 Then
                        Dim strZone As String = strPubDate.Substring(intZonePos + 1)
                        Dim strZoneFree As String = strPubDate.Substring(0, intZonePos)

                        Select Case strZone
                            Case "GMT"
                                ' No need to do anything
                            Case "UT"
                                tspOffset = New TimeSpan(0)
                                strPubDate = strZoneFree
                            Case "EDT"
                                tspOffset = New TimeSpan(-4, 0, 0)
                                strPubDate = strZoneFree
                            Case "EST", "CDT"
                                tspOffset = New TimeSpan(-5, 0, 0)
                                strPubDate = strZoneFree
                            Case "CST", "MDT"
                                tspOffset = New TimeSpan(-6, 0, 0)
                                strPubDate = strZoneFree
                            Case "MST", "PDT"
                                tspOffset = New TimeSpan(-7, 0, 0)
                                strPubDate = strZoneFree
                            Case "PST"
                                tspOffset = New TimeSpan(-8, 0, 0)
                                strPubDate = strZoneFree
                            Case Else
                                If strZone.Length >= 4 And IsNumeric(strZone) Or IsNumeric(strZone.Substring(1)) Then
                                    Try
                                        Dim intValue As Integer = Integer.Parse(strZone, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture)
                                        tspOffset = New TimeSpan(intValue \ 100, intValue Mod 100, 0)
                                        strPubDate = strZoneFree
                                    Catch expFormat As FormatException
                                        ' The last part of the date was not a time offset
                                    End Try
                                End If
                        End Select
                    End If

                    ' Strip the day of the week from the beginning of the date string if it is there,
                    ' as it can contradict the date itself.
                    Dim strDays() As String = {"mon,", "tue,", "wed,", "thu,", "fri,", "sat,", "sun,"}

                    For Each strDay As String In strDays
                        If strPubDate.StartsWith(strDay, StringComparison.OrdinalIgnoreCase) Then
                            strPubDate = strPubDate.Substring(strDay.Length).Trim
                            Exit For
                        End If
                    Next

                    Try
                        episodeInfoReturn.EpisodeInfo.Date = Date.Parse(strPubDate, Nothing, DateTimeStyles.AssumeUniversal)
                    Catch expFormat As FormatException
                        episodeInfoReturn.EpisodeInfo.Date = Now
                        tspOffset = New TimeSpan(0)
                    End Try

                    episodeInfoReturn.EpisodeInfo.Date = episodeInfoReturn.EpisodeInfo.Date.Subtract(tspOffset)
                Else
                    episodeInfoReturn.EpisodeInfo.Date = Now
                End If

                episodeInfoReturn.EpisodeInfo.Image = RSSNodeImage(xmlItem, xmlNamespaceMgr)

                If episodeInfoReturn.EpisodeInfo.Image Is Nothing Then
                    episodeInfoReturn.EpisodeInfo.Image = RSSNodeImage(xmlRSS.SelectSingleNode("./rss/channel"), xmlNamespaceMgr)
                End If

                episodeInfoReturn.EpisodeInfo.ExtInfo = dicExtInfo
                episodeInfoReturn.Success = True

                Return episodeInfoReturn
            End If
        Next

        Return episodeInfoReturn
    End Function

    Public Sub DownloadProgramme(ByVal strProgExtID As String, ByVal strEpisodeExtID As String, ByVal ProgInfo As IRadioProvider.ProgrammeInfo, ByVal EpInfo As IRadioProvider.EpisodeInfo, ByVal strFinalName As String, ByVal intAttempt As Integer) Implements IRadioProvider.DownloadProgramme
        strProgDldUrl = EpInfo.ExtInfo("EnclosureURL")

        Dim intFileNamePos As Integer = strFinalName.LastIndexOf("\", StringComparison.Ordinal)
        Dim intExtensionPos As Integer = strProgDldUrl.LastIndexOf(".", StringComparison.Ordinal)

        If intExtensionPos > -1 Then
            strExtension = strProgDldUrl.Substring(intExtensionPos + 1)
        End If

        strDownloadFileName = Path.Combine(System.IO.Path.GetTempPath, Path.Combine("RadioDownloader", strFinalName.Substring(intFileNamePos + 1) + "." + strExtension))
        Me.strFinalName = strFinalName + "." + strExtension

        AddHandler SystemEvents.PowerModeChanged, AddressOf PowerModeChange

        webDownload = New WebClient
        webDownload.Headers.Add("user-agent", My.Application.Info.AssemblyName + " " + My.Application.Info.Version.ToString)
        webDownload.DownloadFileAsync(New Uri(strProgDldUrl), strDownloadFileName)
    End Sub

    Friend Sub RaiseFindNewException(ByVal expException As Exception)
        RaiseEvent FindNewException(expException, True)
    End Sub

    Friend Sub RaiseFoundNew(ByVal strExtID As String)
        RaiseEvent FoundNew(strExtID)
    End Sub

    Private Function ItemNodeToEpisodeID(ByVal xmlItem As XmlNode) As String
        Dim strItemID As String = ""
        Dim xmlItemID As XmlNode = xmlItem.SelectSingleNode("./guid")

        If xmlItemID IsNot Nothing Then
            strItemID = xmlItemID.InnerText
        End If

        If strItemID = "" Then
            xmlItemID = xmlItem.SelectSingleNode("./enclosure")

            If xmlItemID IsNot Nothing Then
                Dim xmlUrl As XmlAttribute = xmlItemID.Attributes("url")

                If xmlUrl IsNot Nothing Then
                    strItemID = xmlUrl.Value
                End If
            End If
        End If

        Return strItemID
    End Function

    Private Function RSSNodeImage(ByVal xmlNode As XmlNode, ByVal xmlNamespaceMgr As XmlNamespaceManager) As Bitmap
        Dim cachedWeb As New CachedWebClient

        Try
            Dim xmlImageNode As XmlNode = xmlNode.SelectSingleNode("itunes:image", xmlNamespaceMgr)

            If xmlImageNode IsNot Nothing Then
                Dim strImageUrl As String = xmlImageNode.Attributes("href").Value
                Dim bteImageData As Byte() = cachedWeb.DownloadData(strImageUrl, intCacheHTTPHours)
                RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
            Else
                RSSNodeImage = Nothing
            End If
        Catch
            RSSNodeImage = Nothing
        End Try

        If RSSNodeImage Is Nothing Then
            Try
                Dim xmlImageUrlNode As XmlNode = xmlNode.SelectSingleNode("image/url")

                If xmlImageUrlNode IsNot Nothing Then
                    Dim strImageUrl As String = xmlImageUrlNode.InnerText
                    Dim bteImageData As Byte() = cachedWeb.DownloadData(strImageUrl, intCacheHTTPHours)
                    RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
                Else
                    RSSNodeImage = Nothing
                End If
            Catch
                RSSNodeImage = Nothing
            End Try

            If RSSNodeImage Is Nothing Then
                Try
                    Dim xmlImageNode As XmlNode = xmlNode.SelectSingleNode("media:thumbnail", xmlNamespaceMgr)

                    If xmlImageNode IsNot Nothing Then
                        Dim strImageUrl As String = xmlImageNode.Attributes("url").Value
                        Dim bteImageData As Byte() = cachedWeb.DownloadData(strImageUrl, intCacheHTTPHours)
                        RSSNodeImage = New Bitmap(New IO.MemoryStream(bteImageData))
                    Else
                        RSSNodeImage = Nothing
                    End If
                Catch
                    RSSNodeImage = Nothing
                End Try
            End If
        End If
    End Function

    Private Function CreateNamespaceMgr(ByVal xmlDocument As XmlDocument) As XmlNamespaceManager
        Dim nsManager As New XmlNamespaceManager(xmlDocument.NameTable)

        For Each xmlAttrib As XmlAttribute In xmlDocument.SelectSingleNode("/*").Attributes
            If xmlAttrib.Prefix = "xmlns" Then
                nsManager.AddNamespace(xmlAttrib.LocalName, xmlAttrib.Value)
            End If
        Next

        Return nsManager
    End Function

    Private Sub PowerModeChange(ByVal sender As Object, ByVal e As PowerModeChangedEventArgs)
        If e.Mode = PowerModes.Resume Then
            ' Restart the download, as it is quite likely to have hung during the suspend / hibernate
            If webDownload.IsBusy Then
                webDownload.CancelAsync()

                ' Pause for 30 seconds to be give the pc a chance to settle down after the suspend. 
                Threading.Thread.Sleep(30000)

                webDownload.DownloadFileAsync(New Uri(strProgDldUrl), strDownloadFileName)
            End If
        End If
    End Sub

    Private Sub webDownload_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles webDownload.DownloadFileCompleted
        If e.Cancelled = False Then
            RemoveHandler SystemEvents.PowerModeChanged, AddressOf PowerModeChange

            If e.Error IsNot Nothing Then
                If TypeOf e.Error Is WebException Then
                    Dim webExp As WebException = CType(e.Error, WebException)

                    If webExp.Status = WebExceptionStatus.NameResolutionFailure Then
                        RaiseEvent DldError(IRadioProvider.ErrorType.NetworkProblem, "Unable to resolve the domain to download this episode from.  Check your internet connection, or try again later.", New List(Of DldErrorDataItem))
                        Exit Sub
                    ElseIf TypeOf webExp.Response Is HttpWebResponse Then
                        Dim webErrorResponse As HttpWebResponse = CType(webExp.Response, HttpWebResponse)

                        If webErrorResponse.StatusCode = HttpStatusCode.NotFound Then
                            RaiseEvent DldError(IRadioProvider.ErrorType.NotAvailable, "This episode appears to be no longer available.  You can either try again later, or cancel the download to remove it from the list and clear the error.", New List(Of DldErrorDataItem))
                            Exit Sub
                        End If
                    End If
                End If

                Dim extraDetails As New List(Of DldErrorDataItem)
                extraDetails.Add(New DldErrorDataItem("error", e.Error.GetType.ToString + ": " + e.Error.Message))
                extraDetails.Add(New DldErrorDataItem("exceptiontostring", e.Error.ToString))

                RaiseEvent DldError(IRadioProvider.ErrorType.UnknownError, e.Error.GetType.ToString + vbCrLf + e.Error.StackTrace, extraDetails)
            Else
                RaiseEvent Progress(100, "Downloading...", IRadioProvider.ProgressIcon.Downloading)
                Call File.Move(strDownloadFileName, strFinalName)
                RaiseEvent Finished(strExtension)
            End If
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

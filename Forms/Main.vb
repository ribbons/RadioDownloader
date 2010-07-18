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

Imports System.Collections.Generic
Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms.VisualStyles

Friend Class Main
    Inherits GlassForm

    Private Structure FindNewViewData
        Dim ProviderID As Guid
        Dim View As Object
    End Structure

    Private Class ListComparer
        Implements IComparer

        Public Enum ListType
            Subscription
            Download
        End Enum

        Private dataInstance As Data
        Private compareType As ListType

        Public Sub New(ByVal compareType As ListType)
            dataInstance = Data.GetInstance
            Me.compareType = compareType
        End Sub

        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
            Dim itemXId As Integer = CInt(CType(x, ListViewItem).Name)
            Dim itemYId As Integer = CInt(CType(y, ListViewItem).Name)

            Select Case compareType
                Case ListType.Subscription
                    Return dataInstance.CompareSubscriptions(itemXId, itemYId)
                Case ListType.Download
                    Return dataInstance.CompareDownloads(itemXId, itemYId)
                Case Else
                    Throw New InvalidOperationException("Unknown compare type of " + compareType.ToString)
            End Select
        End Function
    End Class

    Private WithEvents progData As Data
    Private WithEvents view As ViewState

    Private checkUpdate As UpdateCheck
    Private tbarNotif As TaskbarNotify

    Private downloadColNames As New Dictionary(Of Integer, String)
    Private downloadColSizes As New Dictionary(Of Integer, Integer)
    Private downloadColOrder As New List(Of Data.DownloadCols)

    Public Sub UpdateTrayStatus(ByVal active As Boolean)
        If OsUtils.WinSevenOrLater Then
            If progData.CountDownloadsErrored > 0 Then
                tbarNotif.SetOverlayIcon(Me, Icon.FromHandle(My.Resources.list_error.GetHicon), "Error")
                tbarNotif.SetThumbnailTooltip(Me, Me.Text + ": Error")
            Else
                If active = True Then
                    tbarNotif.SetOverlayIcon(Me, Icon.FromHandle(My.Resources.list_downloading.GetHicon), "Downloading")
                    tbarNotif.SetThumbnailTooltip(Me, Me.Text + ": Downloading")
                Else
                    tbarNotif.SetOverlayIcon(Me, Nothing, String.Empty)
                    tbarNotif.SetThumbnailTooltip(Me, Nothing)
                End If
            End If
        End If

        If nicTrayIcon.Visible Then
            If progData.CountDownloadsErrored > 0 Then
                nicTrayIcon.Icon = My.Resources.icon_error
                nicTrayIcon.Text = Me.Text + ": Error"
            Else
                If active = True Then
                    nicTrayIcon.Icon = My.Resources.icon_working
                    nicTrayIcon.Text = Me.Text + ": Downloading"
                Else
                    nicTrayIcon.Icon = My.Resources.icon_main
                    nicTrayIcon.Text = Me.Text
                End If
            End If
        End If
    End Sub

    Private Sub Main_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.F1
                e.Handled = True
                Call mnuHelpShowHelp_Click(sender, e)
            Case Keys.Delete
                If tbtDelete.Visible Then
                    e.Handled = True
                    Call tbtDelete_Click()
                ElseIf tbtCancel.Visible Then
                    e.Handled = True
                    Call tbtCancel_Click()
                End If
            Case Keys.Back
                If Me.ActiveControl.GetType IsNot GetType(TextBox) Then
                    If e.Shift Then
                        If tbtForward.Enabled Then
                            e.Handled = True
                            Call tbtForward_Click(sender, e)
                        End If
                    Else
                        If tbtBack.Enabled Then
                            e.Handled = True
                            Call tbtBack_Click(sender, e)
                        End If
                    End If
                End If
            Case Keys.BrowserBack
                If tbtBack.Enabled Then
                    e.Handled = True
                    Call tbtBack_Click(sender, e)
                End If
            Case Keys.BrowserForward
                If tbtForward.Enabled Then
                    e.Handled = True
                    Call tbtForward_Click(sender, e)
                End If
        End Select
    End Sub

    Private Sub Main_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        ' If this is the first run of a new version of the application, then upgrade the settings from the old version.
        Try
            If My.Settings.UpgradeSettings Then
                My.Settings.Upgrade()
                My.Settings.UpgradeSettings = False
                My.Settings.Save()
            End If
        Catch configErrorExp As ConfigurationErrorsException
            Dim fileName As String = Nothing

            If configErrorExp.Filename IsNot Nothing Then
                fileName = configErrorExp.Filename
            ElseIf configErrorExp.InnerException IsNot Nothing AndAlso TypeOf configErrorExp.InnerException Is ConfigurationErrorsException Then
                Dim innerExp As ConfigurationErrorsException = CType(configErrorExp.InnerException, ConfigurationErrorsException)

                If innerExp.Filename IsNot Nothing Then
                    fileName = innerExp.Filename
                End If
            End If

            If fileName IsNot Nothing Then
                File.Delete(fileName)
                MsgBox("Your Radio Downloader configuration file has been reset as it was corrupt.  This only affects your settings and columns, not your subscriptions or downloads." + Environment.NewLine + Environment.NewLine + "You will need to start Radio Downloader again after closing this message.", MsgBoxStyle.Information)
                Me.Close()
                Me.Dispose()
                Exit Sub
            Else
                Throw
            End If
        End Try

        ' Make sure that the temp and application data folders exist
        Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath, "RadioDownloader"))
        Directory.CreateDirectory(FileUtils.GetAppDataFolder)

        ' Make sure that the database exists.  If not, then copy across the empty database from the program's folder.
        Dim fileExits As New IO.FileInfo(Path.Combine(FileUtils.GetAppDataFolder(), "store.db"))

        If fileExits.Exists = False Then
            Try
                IO.File.Copy(Path.Combine(My.Application.Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"))
            Catch fileNotFoundExp As FileNotFoundException
                Call MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(My.Application.Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical)
                Me.Close()
                Me.Dispose()
                Exit Sub
            End Try
        Else
            ' As the database already exists, copy the specimen database across from the program folder
            ' and then make sure that the current db's structure matches it.
            Try
                IO.File.Copy(Path.Combine(My.Application.Info.DirectoryPath, "store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), True)
            Catch fileNotFoundExp As FileNotFoundException
                Call MsgBox("The Radio Downloader template database was not found at '" + Path.Combine(My.Application.Info.DirectoryPath, "store.db") + "'." + Environment.NewLine + Environment.NewLine + "Try repairing the Radio Downloader installation, or uninstalling Radio Downloader and then installing the latest version from the NerdoftheHerd website.", MsgBoxStyle.Critical)
                Me.Close()
                Me.Dispose()
                Exit Sub
            Catch unauthorizedAccessExp As UnauthorizedAccessException
                Call MsgBox("Access was denied when attempting to copy the Radio Downloader template database." + Environment.NewLine + Environment.NewLine + "Check that you have read access to '" + Path.Combine(My.Application.Info.DirectoryPath, "store.db") + "' and write access to '" + Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db") + "'.", MsgBoxStyle.Critical)
                Me.Close()
                Me.Dispose()
                Exit Sub
            End Try

            Using doDbUpdate As New UpdateDB(Path.Combine(FileUtils.GetAppDataFolder(), "spec-store.db"), Path.Combine(FileUtils.GetAppDataFolder(), "store.db"))
                Call doDbUpdate.UpdateStructure()
            End Using
        End If

        imlListIcons.Images.Add("downloading", My.Resources.list_downloading)
        imlListIcons.Images.Add("waiting", My.Resources.list_waiting)
        imlListIcons.Images.Add("converting", My.Resources.list_converting)
        imlListIcons.Images.Add("downloaded_new", My.Resources.list_downloaded_new)
        imlListIcons.Images.Add("downloaded", My.Resources.list_downloaded)
        imlListIcons.Images.Add("subscribed", My.Resources.list_subscribed)
        imlListIcons.Images.Add("error", My.Resources.list_error)

        imlProviders.Images.Add("default", My.Resources.provider_default)

        imlToolbar.Images.Add("choose_programme", My.Resources.toolbar_choose_programme)
        imlToolbar.Images.Add("clean_up", My.Resources.toolbar_clean_up)
        imlToolbar.Images.Add("current_episodes", My.Resources.toolbar_current_episodes)
        imlToolbar.Images.Add("delete", My.Resources.toolbar_delete)
        imlToolbar.Images.Add("download", My.Resources.toolbar_download)
        imlToolbar.Images.Add("help", My.Resources.toolbar_help)
        imlToolbar.Images.Add("options", My.Resources.toolbar_options)
        imlToolbar.Images.Add("play", My.Resources.toolbar_play)
        imlToolbar.Images.Add("report_error", My.Resources.toolbar_report_error)
        imlToolbar.Images.Add("retry", My.Resources.toolbar_retry)
        imlToolbar.Images.Add("subscribe", My.Resources.toolbar_subscribe)
        imlToolbar.Images.Add("unsubscribe", My.Resources.toolbar_unsubscribe)

        tbrToolbar.ImageList = imlToolbar
        tbrHelp.ImageList = imlToolbar
        lstProviders.LargeImageList = imlProviders
        lstSubscribed.SmallImageList = imlListIcons
        lstDownloads.SmallImageList = imlListIcons

        Call lstEpisodes.Columns.Add("Date", CInt(0.179 * lstEpisodes.Width))
        Call lstEpisodes.Columns.Add("Episode Name", CInt(0.786 * lstEpisodes.Width))
        Call lstSubscribed.Columns.Add("Programme Name", CInt(0.482 * lstSubscribed.Width))
        Call lstSubscribed.Columns.Add("Last Download", CInt(0.179 * lstSubscribed.Width))
        Call lstSubscribed.Columns.Add("Provider", CInt(0.304 * lstSubscribed.Width))

        ' NB - these are defined in alphabetical order to save sorting later
        downloadColNames.Add(Data.DownloadCols.EpisodeDate, "Date")
        downloadColNames.Add(Data.DownloadCols.Duration, "Duration")
        downloadColNames.Add(Data.DownloadCols.EpisodeName, "Episode Name")
        downloadColNames.Add(Data.DownloadCols.Progress, "Progress")
        downloadColNames.Add(Data.DownloadCols.Status, "Status")

        view = New ViewState
        Call view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider)

        progData = Data.GetInstance
        Call progData.InitProviderList()
        Call progData.InitSubscriptionList()
        lstSubscribed.ListViewItemSorter = New ListComparer(ListComparer.ListType.Subscription)

        Call InitDownloadList()
        lstDownloads.ListViewItemSorter = New ListComparer(ListComparer.ListType.Download)

        If OsUtils.WinSevenOrLater Then
            ' New style taskbar - initialise the taskbar notification class
            tbarNotif = New TaskbarNotify
        End If

        If Not OsUtils.WinSevenOrLater Or My.Settings.CloseToSystray Then
            ' Show a system tray icon
            nicTrayIcon.Visible = True
        End If

        ' Set up the initial notification status
        Call UpdateTrayStatus(False)

        checkUpdate = New UpdateCheck("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=" + My.Application.Info.Version.ToString)

        picSideBarBorder.Width = 2

        lstProviders.Dock = DockStyle.Fill
        pnlPluginSpace.Dock = DockStyle.Fill
        lstEpisodes.Dock = DockStyle.Fill
        lstSubscribed.Dock = DockStyle.Fill
        lstDownloads.Dock = DockStyle.Fill

        tbrView.Items.Remove(ttxSearch)

        Me.Font = SystemFonts.MessageBoxFont
        lblSideMainTitle.Font = New Font(Me.Font.FontFamily, CSng(Me.Font.SizeInPoints * 1.16), Me.Font.Style, GraphicsUnit.Point)

        ' Scale the max size of the sidebar image for values other than 96 dpi, as it is specified in pixels
        Dim graphicsForDpi As Graphics = Me.CreateGraphics()
        picSidebarImg.MaximumSize = New Size(CInt(picSidebarImg.MaximumSize.Width * (graphicsForDpi.DpiX / 96)), CInt(picSidebarImg.MaximumSize.Height * (graphicsForDpi.DpiY / 96)))

        tblToolbars.Height = tbrToolbar.Height
        tbrToolbar.SetWholeDropDown(tbtOptionsMenu)
        tbrHelp.SetWholeDropDown(tbtHelpMenu)
        tbrHelp.Width = tbtHelpMenu.Rectangle.Width

        If Me.WindowState <> FormWindowState.Minimized Then
            tblToolbars.ColumnStyles(0) = New ColumnStyle(SizeType.Absolute, tblToolbars.Width - (tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right))
            tblToolbars.ColumnStyles(1) = New ColumnStyle(SizeType.Absolute, tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right)
        End If

        If OsUtils.WinVistaOrLater And VisualStyleRenderer.IsSupported Then
            tbrView.Margin = New Padding(0)
        End If

        Me.SetGlassMargins(0, 0, tbrView.Height, 0)
        tbrView.Renderer = New TabBarRenderer

        OsUtils.ApplyRunOnStartup()

        progData.StartDownload()
        tmrCheckForUpdates.Enabled = True
    End Sub

    Private Sub Main_FormClosing(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If eventArgs.CloseReason = CloseReason.UserClosing Then
            If Not nicTrayIcon.Visible Then
                If Me.WindowState <> FormWindowState.Minimized Then
                    Me.WindowState = FormWindowState.Minimized
                    eventArgs.Cancel = True
                End If
            Else
                Call OsUtils.TrayAnimate(Me, True)
                Me.Visible = False
                eventArgs.Cancel = True

                If My.Settings.ShownTrayBalloon = False Then
                    nicTrayIcon.BalloonTipIcon = ToolTipIcon.Info
                    nicTrayIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + Environment.NewLine + "Click here to hide this message in future."
                    nicTrayIcon.BalloonTipTitle = "Radio Downloader is still running"
                    nicTrayIcon.ShowBalloonTip(30000)
                End If
            End If
        End If
    End Sub

    Private Sub lstProviders_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstProviders.SelectedIndexChanged
        If lstProviders.SelectedItems.Count > 0 Then
            Dim pluginId As Guid = New Guid(lstProviders.SelectedItems(0).Name)
            ShowProviderInfo(pluginId)
        Else
            Call SetViewDefaults()
        End If
    End Sub

    Private Sub ShowProviderInfo(ByVal providerId As Guid)
        Dim info As Data.ProviderData = progData.FetchProviderData(providerId)
        Call SetSideBar(info.name, info.description, Nothing)

        If view.CurrentView = ViewState.View.FindNewChooseProvider Then
            SetToolbarButtons("ChooseProgramme")
        End If
    End Sub

    Private Sub lstProviders_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstProviders.ItemActivate
        ' Occasionally the event gets fired when there isn't an item selected
        If lstProviders.SelectedItems.Count = 0 Then
            Exit Sub
        End If

        Call tbtChooseProgramme_Click()
    End Sub

    Private Sub lstEpisodes_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstEpisodes.ItemCheck
        progData.EpisodeSetAutoDownload(CInt(lstEpisodes.Items(e.Index).Name), e.NewValue = CheckState.Checked)
    End Sub

    Private Sub ShowEpisodeInfo(ByVal epid As Integer)
        Dim progInfo As Data.ProgrammeData = progData.FetchProgrammeData(CInt(view.CurrentViewData))
        Dim epInfo As Data.EpisodeData = progData.FetchEpisodeData(epid)
        Dim infoText As String = ""

        If epInfo.description IsNot Nothing Then
            infoText += epInfo.description + Environment.NewLine + Environment.NewLine
        End If

        infoText += "Date: " + epInfo.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture)
        infoText += DescDuration(epInfo.duration)

        Call SetSideBar(epInfo.name, infoText, progData.FetchEpisodeImage(epid))

        If progInfo.subscribed Then
            Call SetToolbarButtons("Download,Unsubscribe")
        Else
            If progInfo.singleEpisode Then
                Call SetToolbarButtons("Download")
            Else
                Call SetToolbarButtons("Download,Subscribe")
            End If
        End If
    End Sub

    Private Sub lstEpisodes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstEpisodes.SelectedIndexChanged
        If lstEpisodes.SelectedItems.Count > 0 Then
            Dim epid As Integer = CInt(lstEpisodes.SelectedItems(0).Name)
            Call ShowEpisodeInfo(epid)
        Else
            Call SetViewDefaults() ' Revert back to programme info in sidebar
        End If
    End Sub

    Private Sub lstSubscribed_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSubscribed.SelectedIndexChanged
        If lstSubscribed.SelectedItems.Count > 0 Then
            Dim progid As Integer = CInt(lstSubscribed.SelectedItems(0).Name)

            progData.UpdateProgInfoIfRequired(progid)
            Call ShowSubscriptionInfo(progid)
        Else
            Call SetViewDefaults() ' Revert back to subscribed items view default sidebar and toolbar
        End If
    End Sub

    Private Sub ShowSubscriptionInfo(ByVal progid As Integer)
        Dim info As Data.SubscriptionData = progData.FetchSubscriptionData(progid)

        Call SetSideBar(info.name, info.description, progData.FetchProgrammeImage(progid))
        Call SetToolbarButtons("Unsubscribe,CurrentEps")
    End Sub

    Private Sub lstDownloads_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstDownloads.ColumnClick
        Dim clickedCol As Data.DownloadCols = downloadColOrder(e.Column)

        If clickedCol = Data.DownloadCols.Progress Then
            Return
        End If

        If progData.DownloadSortByCol <> clickedCol Then
            progData.DownloadSortByCol = clickedCol
            progData.DownloadSortAscending = True
        Else
            progData.DownloadSortAscending = Not progData.DownloadSortAscending
        End If

        ' Set the column header to display the new sort order
        lstDownloads.ShowSortOnHeader(downloadColOrder.IndexOf(progData.DownloadSortByCol), If(progData.DownloadSortAscending, SortOrder.Ascending, SortOrder.Descending))

        ' Save the current sort
        My.Settings.DownloadColSortBy = progData.DownloadSortByCol
        My.Settings.DownloadColSortAsc = progData.DownloadSortAscending

        lstDownloads.Sort()
    End Sub

    Private Sub lstDownloads_ColumnReordered(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnReorderedEventArgs) Handles lstDownloads.ColumnReordered
        Dim oldOrder(lstDownloads.Columns.Count - 1) As String

        ' Fetch the pre-reorder column order
        For Each col As ColumnHeader In lstDownloads.Columns
            oldOrder(col.DisplayIndex) = CInt(downloadColOrder(col.Index)).ToString(CultureInfo.InvariantCulture)
        Next

        Dim newOrder As New List(Of String)(oldOrder)
        Dim moveCol As String = newOrder(e.OldDisplayIndex)

        ' Re-order the data to match the new column order
        newOrder.RemoveAt(e.OldDisplayIndex)
        newOrder.Insert(e.NewDisplayIndex, moveCol)

        ' Save the new column order to the preference
        My.Settings.DownloadCols = Join(newOrder.ToArray, ",")

        If e.OldDisplayIndex = 0 OrElse e.NewDisplayIndex = 0 Then
            ' The reorder involves column 0 which contains the icons, so re-initialise the list
            e.Cancel = True
            Call InitDownloadList()
        End If
    End Sub

    Private Sub lstDownloads_ColumnRightClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles lstDownloads.ColumnRightClick
        mnuListHdrs.Show(lstDownloads, lstDownloads.PointToClient(Cursor.Position))
    End Sub

    Private Sub lstDownloads_ColumnWidthChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnWidthChangedEventArgs) Handles lstDownloads.ColumnWidthChanged
        ' Save the updated column's width
        downloadColSizes(downloadColOrder(e.ColumnIndex)) = lstDownloads.Columns(e.ColumnIndex).Width

        Dim saveColSizes As String = String.Empty

        ' Convert the stored column widths back to a string to save to settings
        For Each colSize As KeyValuePair(Of Integer, Integer) In downloadColSizes
            If saveColSizes <> String.Empty Then
                saveColSizes += "|"
            End If

            saveColSizes += colSize.Key.ToString(CultureInfo.InvariantCulture) + "," + (colSize.Value / Me.CurrentAutoScaleDimensions.Width).ToString(CultureInfo.InvariantCulture)
        Next

        My.Settings.DownloadColSizes = saveColSizes
    End Sub

    Private Sub lstDownloads_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDownloads.ItemActivate
        ' Occasionally the event gets fired when there isn't an item selected
        If lstDownloads.SelectedItems.Count = 0 Then
            Exit Sub
        End If

        Call tbtPlay_Click()
    End Sub

    Private Sub lstDownloads_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstDownloads.SelectedIndexChanged
        If lstDownloads.SelectedItems.Count > 0 Then
            Call ShowDownloadInfo(CInt(lstDownloads.SelectedItems(0).Name))
        Else
            Call SetViewDefaults() ' Revert back to downloads view default sidebar and toolbar
        End If
    End Sub

    Private Sub ShowDownloadInfo(ByVal epid As Integer)
        Dim info As Data.DownloadData = progData.FetchDownloadData(epid)

        Dim actionString As String
        Dim infoText As String = ""

        If info.description IsNot Nothing Then
            infoText += info.description + Environment.NewLine + Environment.NewLine
        End If

        infoText += "Date: " + info.episodeDate.ToString("ddd dd/MMM/yy HH:mm", CultureInfo.CurrentCulture)
        infoText += DescDuration(info.duration)

        Select Case info.status
            Case Data.DownloadStatus.Downloaded
                If File.Exists(info.downloadPath) Then
                    actionString = "Play,Delete"
                Else
                    actionString = "Delete"
                End If

                infoText += Environment.NewLine + "Play count: " + info.playCount.ToString(CultureInfo.CurrentCulture)
            Case Data.DownloadStatus.Errored
                Dim errorName As String = ""
                Dim errorDetails As String = info.errorDetails

                actionString = "Retry,Cancel"

                Select Case info.errorType
                    Case ErrorType.LocalProblem
                        errorName = "Local problem"
                    Case ErrorType.ShorterThanExpected
                        errorName = "Shorter than expected"
                    Case ErrorType.NotAvailable
                        errorName = "Not available"
                    Case ErrorType.NotAvailableInLocation
                        errorName = "Not available in your location"
                    Case ErrorType.NetworkProblem
                        errorName = "Network problem"
                    Case ErrorType.RemoteProblem
                        errorName = "Remote problem"
                    Case ErrorType.UnknownError
                        errorName = "Unknown error"
                        errorDetails = "An unknown error occurred when trying to download this programme.  Press the 'Report Error' button on the toolbar to send a report of this error back to NerdoftheHerd, so that it can be fixed."
                        actionString = "Retry,Cancel,ReportError"
                End Select

                infoText += Environment.NewLine + Environment.NewLine + "Error: " + errorName

                If errorDetails <> "" Then
                    infoText += Environment.NewLine + Environment.NewLine + errorDetails
                End If
            Case Else
                actionString = "Cancel"
        End Select

        Call SetSideBar(info.name, infoText, progData.FetchEpisodeImage(epid))
        Call SetToolbarButtons(actionString)
    End Sub

    Private Function DescDuration(ByVal duration As Integer) As String
        Dim readable As String = String.Empty

        If duration <> 0 Then
            readable += Environment.NewLine + "Duration: "

            Dim mins As Integer = CInt(Math.Round(duration / 60, 0))
            Dim hours As Integer = mins \ 60
            mins = mins Mod 60

            If hours > 0 Then
                readable += CStr(hours) + "hr" + If(hours = 1, "", "s")
            End If

            If hours > 0 And mins > 0 Then
                readable += " "
            End If

            If mins > 0 Then
                readable += CStr(mins) + "min"
            End If
        End If

        Return readable
    End Function

    Private Sub SetSideBar(ByVal title As String, ByVal description As String, ByVal picture As Bitmap)
        lblSideMainTitle.Text = title

        txtSideDescript.Text = description

        ' Make sure the scrollbars update correctly
        txtSideDescript.ScrollBars = RichTextBoxScrollBars.None
        txtSideDescript.ScrollBars = RichTextBoxScrollBars.Both

        If picture IsNot Nothing Then
            If picture.Width > picSidebarImg.MaximumSize.Width Or picture.Height > picSidebarImg.MaximumSize.Height Then
                Dim newWidth As Integer
                Dim newHeight As Integer

                If picture.Width > picture.Height Then
                    newWidth = picSidebarImg.MaximumSize.Width
                    newHeight = CInt((newWidth / picture.Width) * picture.Height)
                Else
                    newHeight = picSidebarImg.MaximumSize.Height
                    newWidth = CInt((newHeight / picture.Height) * picture.Width)
                End If

                Dim origImg As Bitmap = picture
                picture = New Bitmap(newWidth, newHeight)
                Dim graph As Graphics

                graph = Graphics.FromImage(picture)
                graph.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

                graph.DrawImage(origImg, 0, 0, newWidth, newHeight)

                origImg.Dispose()
            End If

            picSidebarImg.Image = picture
            picSidebarImg.Visible = True
        Else
            picSidebarImg.Visible = False
        End If
    End Sub

    Private Sub SetToolbarButtons(ByVal buttons As String)
        Dim splitButtons() As String = Split(buttons, ",")

        tbtChooseProgramme.Visible = False
        tbtDownload.Visible = False
        tbtSubscribe.Visible = False
        tbtUnsubscribe.Visible = False
        tbtCurrentEps.Visible = False
        tbtPlay.Visible = False
        tbtCancel.Visible = False
        tbtDelete.Visible = False
        tbtRetry.Visible = False
        tbtReportError.Visible = False
        tbtCleanUp.Visible = False

        For Each loopButtons As String In splitButtons
            Select Case loopButtons
                Case "ChooseProgramme"
                    tbtChooseProgramme.Visible = True
                Case "Download"
                    tbtDownload.Visible = True
                Case "Subscribe"
                    tbtSubscribe.Visible = True
                Case "Unsubscribe"
                    tbtUnsubscribe.Visible = True
                Case "CurrentEps"
                    tbtCurrentEps.Visible = True
                Case "Play"
                    tbtPlay.Visible = True
                Case "Delete"
                    tbtDelete.Visible = True
                Case "Cancel"
                    tbtCancel.Visible = True
                Case "Retry"
                    tbtRetry.Visible = True
                Case "ReportError"
                    tbtReportError.Visible = True
                Case "CleanUp"
                    tbtCleanUp.Visible = True
            End Select
        Next
    End Sub

    Public Sub mnuTrayShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTrayShow.Click
        If Me.Visible = False Then
            Call OsUtils.TrayAnimate(Me, False)
            Me.Visible = True
        End If

        If Me.WindowState = FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Normal
        End If

        Me.Activate()
    End Sub

    Public Sub mnuTrayExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuTrayExit.Click
        Me.Close()
        Me.Dispose()

        Try
            Directory.Delete(Path.Combine(System.IO.Path.GetTempPath, "RadioDownloader"), True)
        Catch exp As IOException
            ' Ignore an IOException - this just means that a file in the temp folder is still in use.
        End Try
    End Sub

    Private Sub nicTrayIcon_BalloonTipClicked(ByVal sender As Object, ByVal e As System.EventArgs) Handles nicTrayIcon.BalloonTipClicked
        My.Settings.ShownTrayBalloon = True
    End Sub

    Private Sub nicTrayIcon_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles nicTrayIcon.MouseDoubleClick
        Call mnuTrayShow_Click(sender, e)
    End Sub

    Private Sub tbtFindNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtFindNew.Click
        Call view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewChooseProvider)
    End Sub

    Private Sub tbtSubscriptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtSubscriptions.Click
        Call view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions)
    End Sub

    Private Sub tbtDownloads_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownloads.Click
        Call view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads)
    End Sub

    Private Sub progData_ProviderAdded(ByVal providerId As System.Guid) Handles progData.ProviderAdded
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_ProviderAdded(providerId))
            Return
        End If

        Dim info As Data.ProviderData = progData.FetchProviderData(providerId)

        Dim addItem As New ListViewItem
        addItem.Name = providerId.ToString
        addItem.Text = info.name

        If info.icon IsNot Nothing Then
            imlProviders.Images.Add(providerId.ToString, info.icon)
            addItem.ImageKey = providerId.ToString
        Else
            addItem.ImageKey = "default"
        End If

        lstProviders.Items.Add(addItem)

        ' Hide the 'No providers' provider options menu item
        If mnuOptionsProviderOptsNoProvs.Visible = True Then
            mnuOptionsProviderOptsNoProvs.Visible = False
        End If

        Dim addMenuItem As New MenuItem(info.name + " Provider")

        If info.showOptionsHandler IsNot Nothing Then
            AddHandler addMenuItem.Click, info.showOptionsHandler
        Else
            addMenuItem.Enabled = False
        End If

        mnuOptionsProviderOpts.MenuItems.Add(addMenuItem)

        If view.CurrentView = ViewState.View.FindNewChooseProvider Then
            If lstProviders.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If
    End Sub

    Private Sub progData_ProgrammeUpdated(ByVal progid As Integer) Handles progData.ProgrammeUpdated
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_ProgrammeUpdated(progid))
            Return
        End If

        If view.CurrentView = ViewState.View.ProgEpisodes Then
            If CInt(view.CurrentViewData) = progid Then
                If lstEpisodes.SelectedItems.Count = 0 Then
                    ' Update the displayed programme information
                    Call ShowProgrammeInfo(progid)
                Else
                    ' Update the displayed episode information (in case the subscription status has changed)
                    Dim epid As Integer = CInt(lstEpisodes.SelectedItems(0).Name)
                    Call ShowEpisodeInfo(epid)
                End If
            End If
        End If
    End Sub

    Private Sub ShowProgrammeInfo(ByVal progid As Integer)
        Dim progInfo As Data.ProgrammeData = progData.FetchProgrammeData(CInt(view.CurrentViewData))

        If progInfo.subscribed Then
            Call SetToolbarButtons("Unsubscribe")
        Else
            If progInfo.singleEpisode Then
                Call SetToolbarButtons("")
            Else
                Call SetToolbarButtons("Subscribe")
            End If
        End If

        Call SetSideBar(progInfo.name, progInfo.description, progData.FetchProgrammeImage(progid))
    End Sub

    Private Sub EpisodeListItem(ByVal epid As Integer, ByVal info As Data.EpisodeData, ByRef item As ListViewItem)
        item.Name = epid.ToString(CultureInfo.InvariantCulture)
        item.Text = info.episodeDate.ToShortDateString
        item.SubItems(1).Text = info.name
        item.Checked = info.autoDownload
    End Sub

    Private Sub progData_EpisodeAdded(ByVal epid As Integer) Handles progData.EpisodeAdded
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_EpisodeAdded(epid))
            Return
        End If

        Dim info As Data.EpisodeData = progData.FetchEpisodeData(epid)

        Dim addItem As New ListViewItem
        addItem.SubItems.Add("")

        EpisodeListItem(epid, info, addItem)

        RemoveHandler lstEpisodes.ItemCheck, AddressOf lstEpisodes_ItemCheck
        lstEpisodes.Items.Add(addItem)
        AddHandler lstEpisodes.ItemCheck, AddressOf lstEpisodes_ItemCheck
    End Sub

    Private Sub SubscriptionListItem(ByVal progid As Integer, ByVal info As Data.SubscriptionData, ByRef item As ListViewItem)
        item.Name = progid.ToString(CultureInfo.InvariantCulture)
        item.Text = info.name

        If info.latestDownload = Nothing Then
            item.SubItems(1).Text = "Never"
        Else
            item.SubItems(1).Text = info.latestDownload.ToShortDateString
        End If

        item.SubItems(2).Text = info.providerName
        item.ImageKey = "subscribed"
    End Sub

    Private Sub progData_SubscriptionAdded(ByVal progid As Integer) Handles progData.SubscriptionAdded
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_SubscriptionAdded(progid))
            Return
        End If

        Dim info As Data.SubscriptionData = progData.FetchSubscriptionData(progid)

        Dim addItem As New ListViewItem
        addItem.SubItems.Add("")
        addItem.SubItems.Add("")

        SubscriptionListItem(progid, info, addItem)
        lstSubscribed.Items.Add(addItem)

        If view.CurrentView = ViewState.View.Subscriptions Then
            If lstSubscribed.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If
    End Sub

    Private Sub progData_SubscriptionUpdated(ByVal progid As Integer) Handles progData.SubscriptionUpdated
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_SubscriptionUpdated(progid))
            Return
        End If

        Dim info As Data.SubscriptionData = progData.FetchSubscriptionData(progid)
        Dim item As ListViewItem = lstSubscribed.Items(progid.ToString(CultureInfo.InvariantCulture))

        SubscriptionListItem(progid, info, item)

        If view.CurrentView = ViewState.View.Subscriptions Then
            If lstSubscribed.Items(progid.ToString(CultureInfo.InvariantCulture)).Selected Then
                ShowSubscriptionInfo(progid)
            ElseIf lstSubscribed.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If
    End Sub

    Private Sub progData_SubscriptionRemoved(ByVal progid As Integer) Handles progData.SubscriptionRemoved
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_SubscriptionRemoved(progid))
            Return
        End If

        If view.CurrentView = ViewState.View.Subscriptions Then
            If lstSubscribed.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If

        lstSubscribed.Items(progid.ToString(CultureInfo.InvariantCulture)).Remove()
    End Sub

    Private Function DownloadListItem(ByVal info As Data.DownloadData, Optional ByRef item As ListViewItem = Nothing) As ListViewItem
        If item Is Nothing Then
            item = New ListViewItem
        End If

        item.Name = info.epid.ToString(CultureInfo.InvariantCulture)

        If item.SubItems.Count < downloadColOrder.Count Then
            For addCols As Integer = item.SubItems.Count To downloadColOrder.Count - 1
                item.SubItems.Add("")
            Next
        End If

        For column As Integer = 0 To downloadColOrder.Count - 1
            Select Case downloadColOrder(column)
                Case Data.DownloadCols.EpisodeName
                    item.SubItems(column).Text = info.name
                Case Data.DownloadCols.EpisodeDate
                    item.SubItems(column).Text = info.episodeDate.ToShortDateString
                Case Data.DownloadCols.Status
                    Select Case info.status
                        Case Data.DownloadStatus.Waiting
                            item.SubItems(column).Text = "Waiting"
                        Case Data.DownloadStatus.Downloaded
                            If info.playCount = 0 Then
                                item.SubItems(column).Text = "Newly Downloaded"
                            Else
                                item.SubItems(column).Text = "Downloaded"
                            End If
                        Case Data.DownloadStatus.Errored
                            item.SubItems(column).Text = "Error"
                        Case Else
                            Throw New InvalidDataException("Unknown status of " + info.status.ToString)
                    End Select
                Case Data.DownloadCols.Progress
                    item.SubItems(column).Text = ""
                Case Data.DownloadCols.Duration
                    Dim durationText As String = String.Empty

                    If info.duration <> 0 Then
                        Dim mins As Integer = CInt(Math.Round(info.duration / 60, 0))
                        Dim hours As Integer = mins \ 60
                        mins = mins Mod 60

                        durationText = String.Format(CultureInfo.CurrentCulture, "{0}:{1:00}", hours, mins)
                    End If

                    item.SubItems(column).Text = durationText
                Case Else
                    Throw New InvalidDataException("Unknown column type of " + downloadColOrder(column).ToString)
            End Select
        Next

        Select Case info.status
            Case Data.DownloadStatus.Waiting
                item.ImageKey = "waiting"
            Case Data.DownloadStatus.Downloaded
                If info.playCount = 0 Then
                    item.ImageKey = "downloaded_new"
                Else
                    item.ImageKey = "downloaded"
                End If
            Case Data.DownloadStatus.Errored
                item.ImageKey = "error"
            Case Else
                Throw New InvalidDataException("Unknown status of " + info.status.ToString)
        End Select

        Return item
    End Function

    Private Sub progData_DownloadAdded(ByVal epid As Integer) Handles progData.DownloadAdded
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_DownloadAdded(epid))
            Return
        End If

        Dim info As Data.DownloadData = progData.FetchDownloadData(epid)
        lstDownloads.Items.Add(DownloadListItem(info))

        If view.CurrentView = ViewState.View.Downloads Then
            If lstDownloads.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If
    End Sub

    Private Sub progData_DownloadProgress(ByVal epid As Integer, ByVal percent As Integer, ByVal statusText As String, ByVal icon As ProgressIcon) Handles progData.DownloadProgress
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_DownloadProgress(epid, percent, statusText, icon))
            Return
        End If

        Dim item As ListViewItem = lstDownloads.Items(CStr(epid))

        If item Is Nothing Then
            Return
        End If

        If downloadColOrder.Contains(Data.DownloadCols.Status) Then
            item.SubItems(downloadColOrder.IndexOf(Data.DownloadCols.Status)).Text = statusText
        End If

        If downloadColOrder.Contains(Data.DownloadCols.Progress) Then
            prgDldProg.Value = percent

            If lstDownloads.Controls.Count = 0 Then
                lstDownloads.AddProgressBar(prgDldProg, item, downloadColOrder.IndexOf(Data.DownloadCols.Progress))
            End If
        End If

        Select Case icon
            Case ProgressIcon.Downloading
                item.ImageKey = "downloading"
            Case ProgressIcon.Converting
                item.ImageKey = "converting"
        End Select

        Call UpdateTrayStatus(True)

        If OsUtils.WinSevenOrLater Then
            tbarNotif.SetProgressValue(Me, percent, 100)
        End If
    End Sub

    Private Sub progData_DownloadRemoved(ByVal epid As Integer) Handles progData.DownloadRemoved
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_DownloadRemoved(epid))
            Return
        End If

        Dim item As ListViewItem = lstDownloads.Items(epid.ToString(CultureInfo.InvariantCulture))

        If item IsNot Nothing Then
            If downloadColOrder.Contains(Data.DownloadCols.Progress) Then
                If lstDownloads.GetProgressBar(item, downloadColOrder.IndexOf(Data.DownloadCols.Progress)) IsNot Nothing Then
                    lstDownloads.RemoveProgressBar(prgDldProg)
                End If
            End If

            item.Remove()
        End If

        If view.CurrentView = ViewState.View.Downloads Then
            If lstDownloads.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If

        Call UpdateTrayStatus(False)

        If OsUtils.WinSevenOrLater Then
            tbarNotif.SetProgressNone(Me)
        End If
    End Sub

    Private Sub progData_DownloadUpdated(ByVal epid As Integer) Handles progData.DownloadUpdated
        If Me.InvokeRequired Then
            ' Events will sometimes be fired on a different thread to the ui
            Me.BeginInvoke(Sub() progData_DownloadUpdated(epid))
            Return
        End If

        Dim info As Data.DownloadData = progData.FetchDownloadData(epid)

        Dim item As ListViewItem = lstDownloads.Items(epid.ToString(CultureInfo.InvariantCulture))
        DownloadListItem(info, item)

        If downloadColOrder.Contains(Data.DownloadCols.Progress) Then
            If lstDownloads.GetProgressBar(item, downloadColOrder.IndexOf(Data.DownloadCols.Progress)) IsNot Nothing Then
                lstDownloads.RemoveProgressBar(prgDldProg)
            End If
        End If

        ' Update the downloads list sorting, as the order may now have changed
        lstDownloads.Sort()

        If view.CurrentView = ViewState.View.Downloads Then
            If item.Selected Then
                ShowDownloadInfo(epid)
            ElseIf lstDownloads.SelectedItems.Count = 0 Then
                ' Update the displayed statistics
                SetViewDefaults()
            End If
        End If

        Call UpdateTrayStatus(False)

        If OsUtils.WinSevenOrLater Then
            tbarNotif.SetProgressNone(Me)
        End If
    End Sub

    Private Sub progData_FindNewViewChange(ByVal viewData As Object) Handles progData.FindNewViewChange
        Dim findViewData As FindNewViewData = DirectCast(view.CurrentViewData, FindNewViewData)
        findViewData.View = viewData

        view.StoreView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewProviderForm, findViewData)
    End Sub

    Private Sub progData_FoundNew(ByVal progid As Integer) Handles progData.FoundNew
        Call view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.ProgEpisodes, progid)
    End Sub

    Private Sub Main_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        For Each commandLineArg As String In Environment.GetCommandLineArgs
            If commandLineArg.ToUpperInvariant = "/HIDEMAINWINDOW" Then
                If OsUtils.WinSevenOrLater Then
                    Me.WindowState = FormWindowState.Minimized
                Else
                    Call OsUtils.TrayAnimate(Me, True)
                    Me.Visible = False
                End If
            End If
        Next
    End Sub

    Private Sub tmrCheckForUpdates_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrCheckForUpdates.Tick
        If checkUpdate.IsUpdateAvailable Then
            If My.Settings.LastUpdatePrompt.AddDays(7) < Now Then
                My.Settings.LastUpdatePrompt = Now
                My.Settings.Save() ' Save the last prompt time in case of unexpected termination

                Dim showUpdate As New UpdateNotify

                If Me.WindowState = FormWindowState.Minimized Or Me.Visible = False Then
                    showUpdate.StartPosition = FormStartPosition.CenterScreen
                End If

                If showUpdate.ShowDialog(Me) = DialogResult.Yes Then
                    Process.Start("http://www.nerdoftheherd.com/tools/radiodld/update.php?prevver=" + My.Application.Info.Version.ToString)
                End If
            End If
        End If
    End Sub

    Private Sub tbtSubscribe_Click()
        Dim progid As Integer = CInt(view.CurrentViewData)

        If progData.AddSubscription(progid) Then
            Call view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.Subscriptions, Nothing)
        Else
            Call MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub tbtUnsubscribe_Click()
        Dim progid As Integer

        Select Case view.CurrentView
            Case ViewState.View.ProgEpisodes
                progid = CInt(view.CurrentViewData)
            Case ViewState.View.Subscriptions
                progid = CInt(lstSubscribed.SelectedItems(0).Name)
        End Select

        If MsgBox("Are you sure you would like to stop having new episodes of this programme downloaded automatically?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Call progData.RemoveSubscription(progid)
        End If
    End Sub

    Private Sub tbtCancel_Click()
        Dim epid As Integer = CInt(lstDownloads.SelectedItems(0).Name)

        If MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            progData.DownloadRemove(epid)
        End If
    End Sub

    Private Sub tbtPlay_Click()
        Dim epid As Integer = CInt(lstDownloads.SelectedItems(0).Name)
        Dim info As Data.DownloadData = progData.FetchDownloadData(epid)

        If info.status = Data.DownloadStatus.Downloaded Then
            If File.Exists(info.downloadPath) Then
                Process.Start(info.downloadPath)

                ' Bump the play count of this item up by one
                progData.DownloadBumpPlayCount(epid)
            End If
        End If
    End Sub

    Private Sub tbtDelete_Click()
        Dim epid As Integer = CInt(lstDownloads.SelectedItems(0).Name)
        Dim info As Data.DownloadData = progData.FetchDownloadData(epid)

        Dim fileExists As Boolean = File.Exists(info.downloadPath)
        Dim delQuestion As String = "Are you sure that you would like to delete this episode"

        If fileExists Then
            delQuestion += " and the associated audio file"
        End If

        If MsgBox(delQuestion + "?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If fileExists Then
                Try
                    File.Delete(info.downloadPath)
                Catch ioExp As IOException
                    If MsgBox("There was a problem deleting the audio file for this episode, as the file is in use by another application." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                        Exit Sub
                    End If
                Catch unauthAccessExp As UnauthorizedAccessException
                    If MsgBox("There was a problem deleting the audio file for this episode, as the file is either read-only or you do not have the permissions required." + Environment.NewLine + Environment.NewLine + "Would you like to delete the episode from the list anyway?", MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                        Exit Sub
                    End If
                End Try
            End If

            progData.DownloadRemove(epid)
        End If
    End Sub

    Private Sub tbtRetry_Click()
        Call progData.ResetDownload(CInt(lstDownloads.SelectedItems(0).Name))
    End Sub

    Private Sub tbtDownload_Click()
        Dim epid As Integer = CInt(lstEpisodes.SelectedItems(0).Name)

        If progData.AddDownload(epid) Then
            Call view.SetView(ViewState.MainTab.Downloads, ViewState.View.Downloads)
        Else
            Call MsgBox("This episode is already in the download list!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub tbtCurrentEps_Click()
        Dim progid As Integer = CInt(lstSubscribed.SelectedItems(0).Name)
        Call view.SetView(ViewState.MainTab.Subscriptions, ViewState.View.ProgEpisodes, progid)
    End Sub

    Private Sub tbtReportError_Click()
        Dim episodeID As Integer = CInt(lstDownloads.SelectedItems(0).Name)
        progData.DownloadReportError(episodeID)
    End Sub

    Private Sub tbtChooseProgramme_Click()
        Dim viewData As FindNewViewData
        viewData.ProviderID = New Guid(lstProviders.SelectedItems(0).Name)
        viewData.View = Nothing

        Call view.SetView(ViewState.MainTab.FindProgramme, ViewState.View.FindNewProviderForm, viewData)
    End Sub

    Private Sub mnuOptionsShowOpts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsShowOpts.Click
        Call Preferences.ShowDialog()
    End Sub

    Private Sub mnuOptionsExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsExit.Click
        Call mnuTrayExit_Click(mnuTrayExit, e)
    End Sub

    Private Sub mnuHelpAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpAbout.Click
        Call About.ShowDialog()
    End Sub

    Private Sub mnuHelpShowHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpShowHelp.Click
        Process.Start("http://www.nerdoftheherd.com/tools/radiodld/help/")
    End Sub

    Private Sub mnuHelpReportBug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpReportBug.Click
        Process.Start("http://www.nerdoftheherd.com/tools/radiodld/bug_report.php")
    End Sub

    Private Sub tbtCleanUp_Click()
        Call CleanUp.ShowDialog()
    End Sub

    Private Sub view_UpdateNavBtnState(ByVal enableBack As Boolean, ByVal enableFwd As Boolean) Handles view.UpdateNavBtnState
        tbtBack.Enabled = enableBack
        tbtForward.Enabled = enableFwd
    End Sub

    Private Sub view_ViewChanged(ByVal view As ViewState.View, ByVal tab As ViewState.MainTab, ByVal data As Object) Handles view.ViewChanged
        tbtFindNew.Checked = False
        tbtFavourites.Checked = False
        tbtSubscriptions.Checked = False
        tbtDownloads.Checked = False

        Select Case tab
            Case ViewState.MainTab.FindProgramme
                tbtFindNew.Checked = True
            Case ViewState.MainTab.Favourites
                tbtFavourites.Checked = True
            Case ViewState.MainTab.Subscriptions
                tbtSubscriptions.Checked = True
            Case ViewState.MainTab.Downloads
                tbtDownloads.Checked = True
        End Select

        Call SetViewDefaults()

        ' Set the focus to a control which does not show it, to prevent the toolbar momentarily showing focus
        lblSideMainTitle.Focus()

        lstProviders.Visible = False
        pnlPluginSpace.Visible = False
        lstEpisodes.Visible = False
        lstSubscribed.Visible = False
        lstDownloads.Visible = False

        Select Case view
            Case view.FindNewChooseProvider
                lstProviders.Visible = True
                lstProviders.Focus()

                If lstProviders.SelectedItems.Count > 0 Then
                    ShowProviderInfo(New Guid(lstProviders.SelectedItems(0).Name))
                End If
            Case view.FindNewProviderForm
                Dim FindViewData As FindNewViewData = DirectCast(data, FindNewViewData)

                pnlPluginSpace.Visible = True
                pnlPluginSpace.Controls.Clear()
                pnlPluginSpace.Controls.Add(progData.GetFindNewPanel(FindViewData.ProviderID, FindViewData.View))
                pnlPluginSpace.Controls(0).Dock = DockStyle.Fill
                pnlPluginSpace.Controls(0).Focus()
            Case view.ProgEpisodes
                lstEpisodes.Visible = True
                progData.CancelEpisodeListing()
                lstEpisodes.Items.Clear() ' Clear before DoEvents so that old items don't flash up on screen
                Application.DoEvents() ' Give any queued BeginInvoke calls a chance to be processed
                lstEpisodes.Items.Clear()
                progData.InitEpisodeList(CInt(data))
            Case view.Subscriptions
                lstSubscribed.Visible = True
                lstSubscribed.Focus()

                If lstSubscribed.SelectedItems.Count > 0 Then
                    ShowSubscriptionInfo(CInt(lstSubscribed.SelectedItems(0).Name))
                End If
            Case view.Downloads
                lstDownloads.Visible = True
                lstDownloads.Focus()

                If lstDownloads.SelectedItems.Count > 0 Then
                    ShowDownloadInfo(CInt(lstDownloads.SelectedItems(0).Name))
                End If
        End Select
    End Sub

    Private Sub SetViewDefaults()
        Select Case view.CurrentView
            Case ViewState.View.FindNewChooseProvider
                Call SetToolbarButtons("")
                Call SetSideBar(CStr(lstProviders.Items.Count) + " provider" + If(lstProviders.Items.Count = 1, "", "s"), "", Nothing)
            Case ViewState.View.FindNewProviderForm
                Dim FindViewData As FindNewViewData = DirectCast(view.CurrentViewData, FindNewViewData)
                Call SetToolbarButtons("")
                Call ShowProviderInfo(FindViewData.ProviderID)
            Case ViewState.View.ProgEpisodes
                Dim progid As Integer = CInt(view.CurrentViewData)
                Call ShowProgrammeInfo(progid)
            Case ViewState.View.Subscriptions
                Call SetToolbarButtons("")
                Call SetSideBar(CStr(lstSubscribed.Items.Count) + " subscription" + If(lstSubscribed.Items.Count = 1, "", "s"), "", Nothing)
            Case ViewState.View.Downloads
                Call SetToolbarButtons("CleanUp")

                Dim description As String = ""
                Dim newCount As Integer = progData.CountDownloadsNew
                Dim errorCount As Integer = progData.CountDownloadsErrored

                If newCount > 0 Then
                    description += "Newly downloaded: " + CStr(newCount) + Environment.NewLine
                End If

                If errorCount > 0 Then
                    description += "Errored: " + CStr(errorCount)
                End If

                Call SetSideBar(CStr(lstDownloads.Items.Count) + " download" + If(lstDownloads.Items.Count = 1, "", "s"), description, Nothing)
        End Select
    End Sub

    Private Sub tbtBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtBack.Click
        view.NavBack()
    End Sub

    Private Sub tbtForward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtForward.Click
        view.NavFwd()
    End Sub

    Private Sub tbrToolbar_ButtonClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolBarButtonClickEventArgs) Handles tbrToolbar.ButtonClick
        Select Case e.Button.Name
            Case "tbtChooseProgramme"
                Call tbtChooseProgramme_Click()
            Case "tbtDownload"
                Call tbtDownload_Click()
            Case "tbtSubscribe"
                Call tbtSubscribe_Click()
            Case "tbtUnsubscribe"
                Call tbtUnsubscribe_Click()
            Case "tbtCurrentEps"
                Call tbtCurrentEps_Click()
            Case "tbtCancel"
                Call tbtCancel_Click()
            Case "tbtPlay"
                Call tbtPlay_Click()
            Case "tbtDelete"
                Call tbtDelete_Click()
            Case "tbtRetry"
                Call tbtRetry_Click()
            Case "tbtReportError"
                Call tbtReportError_Click()
            Case "tbtCleanUp"
                Call tbtCleanUp_Click()
        End Select
    End Sub

    Private Sub tblToolbars_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles tblToolbars.Resize
        If Me.WindowState <> FormWindowState.Minimized Then
            tblToolbars.ColumnStyles(0) = New ColumnStyle(SizeType.Absolute, tblToolbars.Width - (tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right))
            tblToolbars.ColumnStyles(1) = New ColumnStyle(SizeType.Absolute, tbtHelpMenu.Rectangle.Width + tbrHelp.Margin.Right)

            If VisualStyleRenderer.IsSupported Then
                ' Visual styles are enabled, so draw the correct background behind the toolbars

                Dim bmpBackground As New Bitmap(tblToolbars.Width, tblToolbars.Height)
                Dim graGraphics As Graphics = Graphics.FromImage(bmpBackground)

                Try
                    Dim vsrRebar As New VisualStyleRenderer("Rebar", 0, 0)
                    vsrRebar.DrawBackground(graGraphics, New Rectangle(0, 0, tblToolbars.Width, tblToolbars.Height))
                    tblToolbars.BackgroundImage = bmpBackground
                Catch expArgument As ArgumentException
                    ' The 'Rebar' background image style did not exist, so don't try to draw it.
                End Try
            End If
        End If
    End Sub

    Private Sub txtSideDescript_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSideDescript.GotFocus
        lblSideMainTitle.Focus()
    End Sub

    Private Sub txtSideDescript_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkClickedEventArgs) Handles txtSideDescript.LinkClicked
        Process.Start(e.LinkText)
    End Sub

    Private Sub txtSideDescript_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSideDescript.Resize
        txtSideDescript.Refresh() ' Make sure the scrollbars update correctly
    End Sub

    Private Sub picSideBarBorder_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles picSideBarBorder.Paint
        e.Graphics.DrawLine(New Pen(Color.FromArgb(255, 167, 186, 197)), 0, 0, 0, picSideBarBorder.Height)
    End Sub

    Private Sub mnuListHdrsColumns_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuListHdrsColumns.Click
        Dim chooser As New ChooseCols
        chooser.Columns = My.Settings.DownloadCols
        chooser.StoreNameList(downloadColNames)

        If chooser.ShowDialog(Me) = DialogResult.OK Then
            My.Settings.DownloadCols = chooser.Columns
            Call InitDownloadList()
        End If
    End Sub

    Private Sub mnuListHdrsReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuListHdrsReset.Click
        My.Settings.DownloadCols = My.Settings.Properties.Item("DownloadCols").DefaultValue.ToString
        My.Settings.DownloadColSizes = My.Settings.Properties.Item("DownloadColSizes").DefaultValue.ToString
        My.Settings.DownloadColSortBy = Integer.Parse(My.Settings.Properties.Item("DownloadColSortBy").DefaultValue.ToString, CultureInfo.InvariantCulture)
        My.Settings.DownloadColSortAsc = CBool(My.Settings.Properties.Item("DownloadColSortAsc").DefaultValue)

        Call InitDownloadList()
    End Sub

    Private Sub InitDownloadList()
        If lstDownloads.SelectedItems.Count > 0 Then
            Call SetViewDefaults() ' Revert back to default sidebar and toolbar
        End If

        downloadColSizes.Clear()
        downloadColOrder.Clear()
        lstDownloads.Clear()
        lstDownloads.RemoveAllControls()

        Dim newItems As String = String.Empty

        ' Find any columns without widths defined in the current setting
        For Each sizePair As String In Split(My.Settings.Properties.Item("DownloadColSizes").DefaultValue.ToString, "|")
            If Not ("|" + My.Settings.DownloadColSizes).Contains("|" + Split(sizePair, ",")(0) + ",") Then
                newItems += "|" + sizePair
            End If
        Next

        ' Append the new column sizes to the end of the setting
        If newItems <> String.Empty Then
            My.Settings.DownloadColSizes += newItems
        End If

        ' Fetch the column sizes into downloadColSizes for ease of access
        For Each sizePair As String In Split(My.Settings.DownloadColSizes, "|")
            Dim splitPair() As String = Split(sizePair, ",")
            Dim pixelSize As Integer = CInt(Single.Parse(splitPair(1), CultureInfo.InvariantCulture) * Me.CurrentAutoScaleDimensions.Width)

            downloadColSizes.Add(Integer.Parse(splitPair(0), CultureInfo.InvariantCulture), pixelSize)
        Next

        ' Set up the columns specified in the DownloadCols setting
        If My.Settings.DownloadCols <> String.Empty Then
            Dim columns As String() = Split(My.Settings.DownloadCols, ",")

            For Each column As String In columns
                Dim colVal As Data.DownloadCols = CType(column, Data.DownloadCols)
                downloadColOrder.Add(colVal)
                lstDownloads.Columns.Add(downloadColNames(colVal), downloadColSizes(colVal))
            Next
        End If

        ' Apply the sort from the current settings
        progData.DownloadSortByCol = CType(My.Settings.DownloadColSortBy, Data.DownloadCols)
        progData.DownloadSortAscending = My.Settings.DownloadColSortAsc
        lstDownloads.ShowSortOnHeader(downloadColOrder.IndexOf(progData.DownloadSortByCol), If(progData.DownloadSortAscending, SortOrder.Ascending, SortOrder.Descending))

        ' Convert the list of DownloadData items to an array of ListItems
        Dim initData As List(Of Data.DownloadData) = progData.FetchDownloadList
        Dim initItems(initData.Count - 1) As ListViewItem

        For convItems As Integer = 0 To initData.Count - 1
            initItems(convItems) = DownloadListItem(initData(convItems))
        Next

        ' Add the whole array of ListItems at once
        lstDownloads.Items.AddRange(initItems)
    End Sub
End Class

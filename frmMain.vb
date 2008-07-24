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
Imports System.IO.File
Imports System.Reflection
Imports System.Text.ASCIIEncoding
Imports System.Diagnostics.Process

Public Class frmMain
    Inherits System.Windows.Forms.Form

    Public Enum ErrorStatus
        NoChange
        Normal
        [Error]
    End Enum

    Private Enum MainTab
        FindProgramme
        Favourites
        Subscriptions
        Downloads
    End Enum

    Private Enum View
        FindNewChooseProvider
        FindNewProviderForm
        ProgEpisodes
        Favourites
        Subscriptions
        Downloads
    End Enum

    Private Structure ViewStore
        Dim Tab As MainTab
        Dim View As View
        Dim ViewData As Object
    End Structure

    Private viwBackData(-1) As ViewStore
    Private viwFwdData(-1) As ViewStore

    Private WithEvents clsProgData As clsData
    Private clsDoDBUpdate As clsUpdateDB
    Private clsUpdate As clsAutoUpdate

    Private Delegate Sub clsProgData_Progress_Delegate(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    Private Delegate Sub clsProgData_DldError_Delegate(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
    Private Delegate Sub clsProgData_Finished_Delegate(ByVal clsCurDldProgData As clsDldProgData)

    Public Sub SetTrayStatus(ByVal booActive As Boolean, Optional ByVal ErrorStatus As ErrorStatus = ErrorStatus.NoChange)
        Dim booErrorStatus As Boolean

        If ErrorStatus = frmMain.ErrorStatus.Error Then
            booErrorStatus = True
        ElseIf ErrorStatus = frmMain.ErrorStatus.Normal Then
            booErrorStatus = False
        End If

        If booErrorStatus = True Then
            nicTrayIcon.Icon = New Icon([Assembly].GetExecutingAssembly().GetManifestResourceStream("RadioDld.Error.ico"))
            nicTrayIcon.Text = Me.Text + ": Error"
        Else
            If booActive = True Then
                nicTrayIcon.Icon = New Icon([Assembly].GetExecutingAssembly().GetManifestResourceStream("RadioDld.Working.ico"))
                nicTrayIcon.Text = Me.Text + ": Downloading"
            Else
                nicTrayIcon.Icon = New Icon([Assembly].GetExecutingAssembly().GetManifestResourceStream("RadioDld.Icon.ico"))
                nicTrayIcon.Text = Me.Text
            End If
        End If
    End Sub

    Private Sub frmMain_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        ' Add a handler to catch otherwise unhandled exceptions
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf ExceptionHandler
        ' Add a handler for thread exceptions
        AddHandler Application.ThreadException, AddressOf ThreadExceptionHandler
        ' Add a handler for when a second instance is loaded
        AddHandler My.Application.StartupNextInstance, AddressOf StartupNextInstanceHandler

        ' If this is the first run of a new version of the application, then upgrade the settings from the old version.
        If My.Settings.UpgradeSettings Then
            My.Settings.Upgrade()
            My.Settings.UpgradeSettings = False
        End If

        ' Make sure that the temp and application data folders exist
        Directory.CreateDirectory(System.IO.Path.GetTempPath + "\RadioDownloader")
        Directory.CreateDirectory(GetAppDataFolder)

        ' Make sure that the database exists.  If not, then copy across the empty database from the program's folder.
        Dim fileExits As New IO.FileInfo(GetAppDataFolder() + "\store.db")
        If fileExits.Exists = False Then
            IO.File.Copy(My.Application.Info.DirectoryPath + "\store.db", GetAppDataFolder() + "\store.db")
        Else
            ' As the database already exists, copy the specimen database across from the program folder
            ' and then make sure that the current db's structure matches it.
            IO.File.Copy(My.Application.Info.DirectoryPath + "\store.db", GetAppDataFolder() + "\spec-store.db", True)

            clsDoDBUpdate = New clsUpdateDB(GetAppDataFolder() + "\spec-store.db", GetAppDataFolder() + "\store.db")
            Call clsDoDBUpdate.UpdateStructure()
        End If

        Call lstEpisodes.Columns.Add("Date", 100)
        Call lstEpisodes.Columns.Add("Episode Name", 440)
        Call lstSubscribed.Columns.Add("Programme Name", 270)
        Call lstSubscribed.Columns.Add("Last Download", 100)
        Call lstSubscribed.Columns.Add("Provider", 170)
        Call lstDownloads.Columns.Add("Name", 225)
        Call lstDownloads.Columns.Add("Date", 85)
        Call lstDownloads.Columns.Add("Status", 130)
        Call lstDownloads.Columns.Add("Progress", 100)

        lstProviders.LargeImageList = imlProviders
        lstSubscribed.SmallImageList = imlListIcons
        lstDownloads.SmallImageList = imlListIcons

        clsProgData = New clsData()
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        Call clsProgData.UpdateSubscrList(lstSubscribed)

        clsProgData.UpdateProviderList(lstProviders)
        Call SetView(MainTab.FindProgramme, View.FindNewChooseProvider, Nothing)
        nicTrayIcon.Icon = New Icon([Assembly].GetExecutingAssembly().GetManifestResourceStream("RadioDld.Icon.ico"))
        nicTrayIcon.Text = Me.Text
        nicTrayIcon.Visible = True

        clsUpdate = New clsAutoUpdate("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt?reqver=" + My.Application.Info.Version.ToString, "http://www.nerdoftheherd.com/tools/radiodld/downloads/Radio Downloader.msi", GetAppDataFolder() + "\Radio Downloader.msi", "msiexec", "/i """ + GetAppDataFolder() + "\Radio Downloader.msi"" REINSTALL=ALL REINSTALLMODE=vamus")
        If My.Settings.UpdateDownloaded Then
            Call InstallUpdate()
        End If

        tblInfo.Dock = DockStyle.Left

        lstProviders.Dock = DockStyle.Fill
        pnlPluginSpace.Dock = DockStyle.Fill
        lstEpisodes.Dock = DockStyle.Fill
        lstSubscribed.Dock = DockStyle.Fill
        lstDownloads.Dock = DockStyle.Fill

        ttxSearch.Visible = False

        tmrStartProcess.Enabled = True
    End Sub

    Private Sub frmMain_FormClosing(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If eventArgs.CloseReason = CloseReason.UserClosing Then
            Call TrayAnimate(Me, True)
            Me.Visible = False
            eventArgs.Cancel = True

            If My.Settings.ShownTrayBalloon = False Then
                nicTrayIcon.BalloonTipIcon = ToolTipIcon.Info
                nicTrayIcon.BalloonTipText = "Radio Downloader will continue to run in the background, so that it can download your subscriptions as soon as they become available." + vbCrLf + "Click here to hide this message in future."
                nicTrayIcon.BalloonTipTitle = "Radio Downloader is Still Running"
                nicTrayIcon.ShowBalloonTip(30000)
            End If
        End If
    End Sub

    Private Sub lstProviders_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstProviders.ItemActivate
        Call SetView(MainTab.FindProgramme, View.FindNewProviderForm, lstProviders.SelectedItems(0).Tag)
    End Sub

    Private Sub lstEpisodes_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstEpisodes.ItemCheck
        clsProgData.EpisodeSetAutoDownload(CInt(lstEpisodes.Items(e.Index).Tag), e.NewValue = CheckState.Checked)
    End Sub

    Private Sub lstEpisodes_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstEpisodes.SelectedIndexChanged
        If lstEpisodes.SelectedItems.Count > 0 Then
            Dim intEpID As Integer = CInt(lstEpisodes.SelectedItems(0).Tag)

            With clsProgData
                Call SetSideBar(.EpisodeName(intEpID), .EpisodeDetails(intEpID), .EpisodeImage(intEpID))
                Call SetToolbarButtons("Download,Subscribe")
            End With
        Else
            Call SetViewDefaults() ' Revert back to programme info in sidebar
        End If
    End Sub

    Private Sub lstSubscribed_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSubscribed.SelectedIndexChanged
        Call SetContextForSelectedSubscription()
    End Sub

    Private Sub SetContextForSelectedSubscription()
        If lstSubscribed.SelectedItems.Count > 0 Then
            Dim intProgID As Integer = CInt(lstSubscribed.SelectedItems(0).Tag)

            Call SetSideBar(clsProgData.ProgrammeName(intProgID), clsProgData.ProgrammeDescription(intProgID), clsProgData.ProgrammeImage(intProgID))
            Call SetToolbarButtons("Unsubscribe")
        Else
            Call SetViewDefaults() ' Revert back to subscribed items view default sidebar and toolbar
        End If
    End Sub

    Private Sub lstDownloads_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstDownloads.ItemActivate
        Call tbtPlay_Click(sender, e)
    End Sub

    Private Sub lstDownloads_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstDownloads.SelectedIndexChanged
        Call SetContextForSelectedDownload()
    End Sub

    Private Sub SetContextForSelectedDownload()
        If lstDownloads.SelectedItems.Count > 0 Then
            Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)
            
            With clsProgData
                Dim staDownloadStatus As clsData.Statuses = .DownloadStatus(intEpID)
                Dim strInfoBox As String = ""
                Dim strActionString As String

                If staDownloadStatus = clsData.Statuses.Downloaded Then
                    If Exists(.DownloadPath(intEpID)) Then
                        strActionString = "Play,Delete"
                    Else
                        strActionString = "Delete"
                    End If

                    strInfoBox = vbCrLf + vbCrLf + "Play count: " + CStr(.DownloadPlayCount(intEpID))
                ElseIf staDownloadStatus = clsData.Statuses.Errored Then
                    Dim strErrorName As String = ""
                    Dim strErrorDetails As String = .DownloadErrorDetails(intEpID)

                    strActionString = "Retry,Cancel"

                    Select Case .DownloadErrorType(intEpID)
                        Case IRadioProvider.ErrorType.MissingDependency
                            strErrorName = "Missing Dependency"
                        Case IRadioProvider.ErrorType.ShorterThanExpected
                            strErrorName = "Shorter Than Expected"
                        Case IRadioProvider.ErrorType.NotAvailable
                            strErrorName = "Not Available"
                        Case IRadioProvider.ErrorType.UnknownError
                            strErrorName = "Unknown Error"
                            strErrorDetails = "An unknown error occurred when trying to download this programme.  Press the 'Report Error' button on the toolbar to send a report of this error back to NerdoftheHerd, so that it can be fixed."
                            strActionString = "Retry,Cancel,ReportError"
                    End Select

                    strInfoBox = vbCrLf + vbCrLf + "Error: " + strErrorName
                    If strErrorDetails <> "" Then
                        strInfoBox += vbCrLf + vbCrLf + strErrorDetails
                    End If
                Else
                    strActionString = "Cancel"
                End If

                Call SetSideBar(.EpisodeName(intEpID), .EpisodeDetails(intEpID) + strInfoBox, .EpisodeImage(intEpID))
                Call SetToolbarButtons(strActionString)
            End With
        Else
            Call SetViewDefaults() ' Revert back to downloads view default sidebar and toolbar
        End If
    End Sub

    Private Sub SetSideBar(ByVal strTitle As String, ByVal strDescription As String, ByVal bmpPicture As Bitmap)
        lblSideMainTitle.Text = strTitle
        lblSideDescript.Text = strDescription

        If bmpPicture IsNot Nothing Then
            If bmpPicture.Width > picSidebarImg.MaximumSize.Width Or bmpPicture.Height > picSidebarImg.MaximumSize.Height Then
                Dim intNewWidth As Integer
                Dim intNewHeight As Integer

                If bmpPicture.Width > bmpPicture.Height Then
                    intNewWidth = picSidebarImg.MaximumSize.Width
                    intNewHeight = CInt((intNewWidth / bmpPicture.Width) * bmpPicture.Height)
                Else
                    intNewHeight = picSidebarImg.MaximumSize.Height
                    intNewWidth = CInt((intNewHeight / bmpPicture.Height) * bmpPicture.Width)
                End If

                Dim bmpOrigImg As Bitmap = bmpPicture
                bmpPicture = New Bitmap(intNewWidth, intNewHeight)
                Dim graGraphics As Graphics

                graGraphics = Graphics.FromImage(bmpPicture)
                graGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

                graGraphics.DrawImage(bmpOrigImg, 0, 0, intNewWidth, intNewHeight)

                bmpOrigImg.Dispose()
            End If

            picSidebarImg.Image = bmpPicture
            picSidebarImg.Visible = True
        Else
            picSidebarImg.Visible = False
        End If
    End Sub

    Private Sub SetToolbarButtons(ByVal strButtons As String)
        Dim strSplitButtons() As String = Split(strButtons, ",")

        tbtDownload.Visible = False
        tbtSubscribe.Visible = False
        tbtUnsubscribe.Visible = False
        tbtPlay.Visible = False
        tbtCancel.Visible = False
        tbtDelete.Visible = False
        tbtRetry.Visible = False
        tbtReportError.Visible = False
        tbtCleanUp.Visible = False

        For Each strLoopButtons As String In strSplitButtons
            Select Case strLoopButtons
                Case "Download"
                    tbtDownload.Visible = True
                Case "Subscribe"
                    tbtSubscribe.Visible = True
                Case "Unsubscribe"
                    tbtUnsubscribe.Visible = True
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
            Call TrayAnimate(Me, False)
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
            Directory.Delete(System.IO.Path.GetTempPath + "\RadioDownloader", True)
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

    Private Sub tmrCheckSub_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrCheckSub.Tick
        Call clsProgData.CheckSubscriptions(lstDownloads, prgDldProg)
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub tmrStartProcess_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrStartProcess.Tick
        If clsProgData.FindAndDownload Then
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

            If viwBackData(viwBackData.GetUpperBound(0)).View = View.Downloads Then
                Call SetContextForSelectedDownload()
            End If
        End If

        tmrStartProcess.Enabled = False
    End Sub

    Private Sub tbtFindNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtFindNew.Click
        Call SetView(MainTab.FindProgramme, View.FindNewChooseProvider, Nothing)
    End Sub

    Private Sub tbtSubscriptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtSubscriptions.Click
        Call SetView(MainTab.Subscriptions, View.Subscriptions, Nothing)
    End Sub

    Private Sub tbtDownloads_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownloads.Click
        Call SetView(MainTab.Downloads, View.Downloads, Nothing)
    End Sub

    Private Sub clsProgData_DldError(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Handles clsProgData.DldError
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated And Me.IsDisposed = False Then
            Dim DelegateInst As New clsProgData_DldError_Delegate(AddressOf clsProgData_DldError_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData, errType, strErrorDetails})
        End If
    End Sub

    Private Sub clsProgData_DldError_FormThread(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
        Try
            If clsProgData.EpisodeExists(clsCurDldProgData.EpID) Then
                Call clsProgData.DownloadSetErrored(clsCurDldProgData.EpID, errType, strErrorDetails)

                ' If the item that has just errored is selected then update the information
                If lstDownloads.Items.ContainsKey(CStr(clsCurDldProgData.EpID)) Then
                    If viwBackData(viwBackData.GetUpperBound(0)).View = View.Downloads And lstDownloads.Items(CStr(clsCurDldProgData.EpID)).Selected Then
                        Call SetContextForSelectedDownload()
                    End If
                End If
            End If

            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

            tmrStartProcess.Enabled = True
        Catch expException As Exception
            ' Errors in a sub called via a delegate are not caught in the right place
            If frmError.Visible = False Then
                Dim clsReport As New clsErrorReporting(expException.GetType.ToString + ": " + expException.Message, expException.GetType.ToString + vbCrLf + expException.StackTrace)
                frmError.AssignReport(clsReport)
                frmError.ShowDialog()
            End If
        End Try
    End Sub

    Private Sub clsProgData_Finished(ByVal clsCurDldProgData As clsDldProgData) Handles clsProgData.Finished
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated And Me.IsDisposed = False Then
            Dim DelegateInst As New clsProgData_Finished_Delegate(AddressOf clsProgData_Finished_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData})
        End If
    End Sub

    Private Sub clsProgData_Finished_FormThread(ByVal clsCurDldProgData As clsDldProgData)
        Try
            Call clsProgData.DownloadSetDownloaded(clsCurDldProgData.EpID, clsCurDldProgData.FinalName)

            Dim viwCurrentView As View = viwBackData(viwBackData.GetUpperBound(0)).View

            If viwCurrentView = View.Downloads And lstDownloads.Items(CStr(clsCurDldProgData.EpID)).Selected Then
                ' The item that has just finished downloading is selected, so update it.
                Call SetContextForSelectedDownload()
            ElseIf viwCurrentView = View.Subscriptions Then
                ' Return to the tab information for subscriptions, as the selection will be lost when we update 
                ' the subscription list.
                Call SetViewDefaults()
            End If

            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
            Call clsProgData.UpdateSubscrList(lstSubscribed)

            If My.Settings.RunAfterCommand <> "" Then
                Try
                    ' Environ("comspec") will give the path to cmd.exe or command.com
                    Call Shell("""" + Environ("comspec") + """ /c " + My.Settings.RunAfterCommand.Replace("%file%", clsCurDldProgData.FinalName), AppWinStyle.NormalNoFocus)
                Catch
                    ' Just ignore the error, as it just means that something has gone wrong with the run after command.
                End Try
            End If

            tmrStartProcess.Enabled = True
        Catch expException As Exception
            ' Errors in a sub called via a delegate are not caught in the right place
            If frmError.Visible = False Then
                Dim clsReport As New clsErrorReporting(expException.GetType.ToString + ": " + expException.Message, expException.GetType.ToString + vbCrLf + expException.StackTrace)
                frmError.AssignReport(clsReport)
                frmError.ShowDialog()
            End If
        End Try
    End Sub

    Private Sub clsProgData_FoundNew(ByVal intProgID As Integer) Handles clsProgData.FoundNew
        Call SetView(MainTab.FindProgramme, View.ProgEpisodes, intProgID)
    End Sub

    Private Sub clsProgData_Progress(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles clsProgData.Progress
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated And Me.IsDisposed = False Then
            Dim DelegateInst As New clsProgData_Progress_Delegate(AddressOf clsProgData_Progress_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData, intPercent, strStatusText, Icon})
        End If
    End Sub

    Private Sub clsProgData_Progress_FormThread(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
        Try
            Static intLastNum As Integer

            If intLastNum = Nothing Then intLastNum = -1
            If intLastNum = intPercent Then Exit Sub
            If intPercent < 0 Then Exit Sub
            If intPercent > 100 Then Exit Sub

            intLastNum = intPercent

            If clsCurDldProgData IsNot Nothing Then
                With clsCurDldProgData
                    Dim lstItem As ListViewItem = lstDownloads.Items(CStr(.EpID))

                    If lstItem IsNot Nothing Then
                        lstItem.SubItems(2).Text = strStatusText
                        prgDldProg.Value = intPercent

                        If lstDownloads.Controls.Count = 0 Then
                            lstDownloads.AddProgressBar(prgDldProg, lstItem, 3)
                        End If

                        Select Case Icon
                            Case IRadioProvider.ProgressIcon.Downloading
                                lstItem.ImageKey = "downloading"
                            Case IRadioProvider.ProgressIcon.Converting
                                lstItem.ImageKey = "converting"
                        End Select
                    End If
                End With
            End If

            Call SetTrayStatus(True)
        Catch expException As Exception
            ' Errors in a sub called via a delegate are not caught in the right place
            If frmError.Visible = False Then
                Dim clsReport As New clsErrorReporting(expException.GetType.ToString + ": " + expException.Message, expException.GetType.ToString + vbCrLf + expException.StackTrace)
                frmError.AssignReport(clsReport)
                frmError.ShowDialog()
            End If
        End Try
    End Sub

    Private Sub frmMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        For Each strCommand As String In Environment.GetCommandLineArgs()
            If strCommand = "-starttray" Then
                Call TrayAnimate(Me, True)
                Me.Visible = False
            End If
        Next
    End Sub

    Private Sub tmrCheckForUpdates_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrCheckForUpdates.Tick
        If My.Settings.UpdateDownloaded Then
            If My.Settings.AskedAboutUpdate = False Then
                ' Set AskedAboutUpdate to true before asking to prevent a messagebox an
                ' hour being popped up until the messagebox is answered.
                My.Settings.AskedAboutUpdate = True

                If MsgBox("A new version of Radio Downloader has been downloaded and is ready to be installed.  Would you like to install the update now?" + vbCrLf + "(If you choose 'No' the update will be installed next time you start Radio Downloader)", MsgBoxStyle.YesNo Or MsgBoxStyle.Question, "Radio Downloader") = MsgBoxResult.Yes Then
                    Call InstallUpdate()
                End If
            End If
        Else
            clsUpdate.CheckForUpdates()
        End If
    End Sub

    Private Sub InstallUpdate()
        My.Settings.AskedAboutUpdate = False
        clsUpdate.InstallUpdate()
        Call mnuTrayExit_Click(mnuTrayExit, New EventArgs)
    End Sub

    Private Sub tbtSubscribe_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtSubscribe.Click
        Dim intProgID As Integer = CInt(viwBackData(viwBackData.GetUpperBound(0)).ViewData)

        If clsProgData.AddSubscription(intProgID) = False Then
            Call MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation)
        Else
            Call SetView(MainTab.Subscriptions, View.Subscriptions, Nothing)
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Private Sub tbtUnsubscribe_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtUnsubscribe.Click
        Dim intProgID As Integer = CInt(lstSubscribed.SelectedItems(0).Tag)

        If MsgBox("Are you sure that you would like to stop having this programme downloaded regularly?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Call clsProgData.RemoveSubscription(intProgID)
            Call SetViewDefaults() ' Revert back to the subscriptions default sidebar and toolbar
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Private Sub tbtCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtCancel.Click
        Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)

        If MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim clsCurrentProgInfo As clsDldProgData
            clsCurrentProgInfo = clsProgData.GetCurrentDownloadInfo

            If clsCurrentProgInfo IsNot Nothing Then
                If clsCurrentProgInfo.EpID = intEpID Then
                    ' The program is currently being downloaded
                    clsProgData.AbortDownloadThread()
                    tmrStartProcess.Enabled = True
                End If
            End If

            clsProgData.RemoveDownload(intEpID)
            clsProgData.UpdateDlList(lstDownloads, prgDldProg)

            ' Set the auto download flag of this episode to false, so if we are subscribed to the programme
            ' it doesn't just download it all over again
            Call clsProgData.EpisodeSetAutoDownload(intEpID, False)
        End If
    End Sub

    Private Sub tbtPlay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtPlay.Click
        Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)

        If clsProgData.DownloadStatus(intEpID) = clsData.Statuses.Downloaded Then
            If Exists(clsProgData.DownloadPath(intEpID)) Then
                Process.Start(clsProgData.DownloadPath(intEpID))

                ' Bump the play count of this item up by one, and update the list so that the icon changes colour
                clsProgData.DownloadBumpPlayCount(intEpID)
                clsProgData.UpdateDlList(lstDownloads, prgDldProg)

                ' Update the prog info pane to show the updated play count
                Call SetContextForSelectedDownload()
            End If
        End If
    End Sub

    Private Sub tbtDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDelete.Click
        Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)
        Dim strDownloadPath As String = clsProgData.DownloadPath(intEpID)
        Dim booExists As Boolean = Exists(strDownloadPath)
        Dim strDelQuestion As String = "Are you sure that you would like to delete this episode"

        If booExists Then
            strDelQuestion += " and the associated audio file"
        End If

        If MsgBox(strDelQuestion + "?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            If booExists Then
                Delete(strDownloadPath)
            End If

            clsProgData.RemoveDownload(intEpID)
            clsProgData.UpdateDlList(lstDownloads, prgDldProg)

            ' Set the auto download flag of this episode to false, so if we are subscribed to the programme
            ' it doesn't just download it all over again
            Call clsProgData.EpisodeSetAutoDownload(intEpID, False)
        End If
    End Sub

    Private Sub tbtRetry_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtRetry.Click
        Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)

        Call clsProgData.ResetDownload(intEpID, False)
        Call SetContextForSelectedDownload() ' Update prog info pane
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub tbtDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownload.Click
        Dim intEpID As Integer = CInt(lstEpisodes.SelectedItems(0).Tag)

        If clsProgData.AddDownload(intEpID) Then
            Call SetView(MainTab.Downloads, View.Downloads, Nothing)
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
            tmrStartProcess.Enabled = True
        Else
            Call MsgBox("This episode is already in the download list!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub tbtReportError_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtReportError.Click
        Dim intEpID As Integer = CInt(lstDownloads.SelectedItems(0).Name)

        Dim clsReport As New clsErrorReporting("Download Error: " + clsProgData.DownloadErrorType(intEpID).ToString, clsProgData.DownloadErrorDetails(intEpID))
        clsReport.SendReport(My.Settings.ErrorReportURL)
    End Sub

    Private Sub tbmOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbmOptions.Click
        Call frmPreferences.ShowDialog()
    End Sub

    Private Sub tbmExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbmExit.Click
        Call mnuTrayExit_Click(mnuTrayExit, e)
    End Sub

    Private Sub tbmAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbmAbout.Click
        Call frmAbout.ShowDialog()
    End Sub

    Private Sub tbmShowHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbmShowHelp.Click
        Start("http://www.nerdoftheherd.com/tools/radiodld/help/")
    End Sub

    Private Sub tbmReportABug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbmReportABug.Click
        Start("http://www.nerdoftheherd.com/tools/radiodld/bug_report.php")
    End Sub

    Private Sub tbtCleanUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtCleanUp.Click
        Call frmCleanUp.ShowDialog()

        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        If tbtDownloads.Checked Then
            Call lstDownloads_SelectedIndexChanged(New Object, New System.EventArgs)
        End If
    End Sub

    Private Sub SetView(ByVal Tab As MainTab, ByVal View As View, ByVal ViewData As Object)
        Dim ViewDataStore As ViewStore

        ViewDataStore.Tab = Tab
        ViewDataStore.View = View
        ViewDataStore.ViewData = ViewData

        ReDim Preserve viwBackData(viwBackData.GetUpperBound(0) + 1)
        viwBackData(viwBackData.GetUpperBound(0)) = ViewDataStore

        If viwFwdData.GetUpperBound(0) > -1 Then
            ReDim viwFwdData(-1)
        End If

        Call PerformViewChanges(ViewDataStore)
    End Sub

    Private Sub PerformViewChanges(ByVal ViewData As ViewStore)
        tbtBack.Enabled = viwBackData.GetUpperBound(0) > 0
        tbtForward.Enabled = viwFwdData.GetUpperBound(0) > -1

        tbtFindNew.Checked = False
        tbtFavourites.Checked = False
        tbtSubscriptions.Checked = False
        tbtDownloads.Checked = False

        Select Case ViewData.Tab
            Case MainTab.FindProgramme
                tbtFindNew.Checked = True
            Case MainTab.Favourites
                tbtFavourites.Checked = True
            Case MainTab.Subscriptions
                tbtSubscriptions.Checked = True
            Case MainTab.Downloads
                tbtDownloads.Checked = True
        End Select

        SetViewDefaults(ViewData)

        lstProviders.Visible = False
        pnlPluginSpace.Visible = False
        lstEpisodes.Visible = False
        lstSubscribed.Visible = False
        lstDownloads.Visible = False

        Select Case ViewData.View
            Case View.FindNewChooseProvider
                lstProviders.Visible = True
            Case View.FindNewProviderForm
                pnlPluginSpace.Visible = True
                pnlPluginSpace.Controls.Clear()
                pnlPluginSpace.Controls.Add(clsProgData.GetFindNewPanel(DirectCast(ViewData.ViewData, Guid)))
            Case View.ProgEpisodes
                lstEpisodes.Visible = True
                RemoveHandler lstEpisodes.ItemCheck, AddressOf lstEpisodes_ItemCheck
                clsProgData.ListEpisodes(CInt(ViewData.ViewData), lstEpisodes)
            Case View.Subscriptions
                lstSubscribed.Visible = True
                Call SetContextForSelectedSubscription()
                lstSubscribed.Focus()
            Case View.Downloads
                lstDownloads.Visible = True
                Call SetContextForSelectedDownload()
                lstDownloads.Focus()
        End Select
    End Sub

    Private Sub SetViewDefaults()
        SetViewDefaults(viwBackData(viwBackData.GetUpperBound(0)))
    End Sub

    Private Sub SetViewDefaults(ByVal ViewData As ViewStore)
        Select Case ViewData.View
            Case View.FindNewChooseProvider
                Call SetToolbarButtons("")
                Call SetSideBar("Find New", "This view allows you to select programmes to download or subscribe to." + vbCrLf + "Select a type of programme to begin.", Nothing)
            Case View.FindNewProviderForm
                Dim gidProvider As Guid = DirectCast(ViewData.ViewData, Guid)
                Call SetToolbarButtons("")
                Call SetSideBar(clsProgData.ProviderName(gidProvider), "This view allows you to select a " & clsProgData.ProviderName(gidProvider) & " programme to view.", Nothing)
            Case View.ProgEpisodes
                Dim intProgID As Integer = CInt(ViewData.ViewData)
                Call SetToolbarButtons("Subscribe")
                Call SetSideBar(clsProgData.ProgrammeName(intProgID), clsProgData.ProgrammeDescription(intProgID), clsProgData.ProgrammeImage(intProgID))
            Case View.Subscriptions
                Call SetToolbarButtons("")
                Call SetSideBar("Subscriptions", "This view shows you the programmes that you are currently subscribed to." + vbCrLf + "To subscribe to a new programme, start by choosing the 'Find New' button on the toolbar." + vbCrLf + "Select a programme in the list to get more information about it.", Nothing)
            Case View.Downloads
                Call SetToolbarButtons("CleanUp")
                Call SetSideBar("Downloads", "Here you can see programmes that are being downloaded, or have been downloaded already." + vbCrLf + "To download a programme, start by choosing the 'Find New' button on the toolbar." + vbCrLf + "Select a programme in the list to get more information about it, or for completed downloads, play it.", Nothing)
        End Select
    End Sub

    Private Sub tbtBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtBack.Click
        ReDim Preserve viwFwdData(viwFwdData.GetUpperBound(0) + 1)
        viwFwdData(viwFwdData.GetUpperBound(0)) = viwBackData(viwBackData.GetUpperBound(0))
        ReDim Preserve viwBackData(viwBackData.GetUpperBound(0) - 1)

        Call PerformViewChanges(viwBackData(viwBackData.GetUpperBound(0)))
    End Sub

    Private Sub tbtForward_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtForward.Click
        ReDim Preserve viwBackData(viwBackData.GetUpperBound(0) + 1)
        viwBackData(viwBackData.GetUpperBound(0)) = viwFwdData(viwFwdData.GetUpperBound(0))
        ReDim Preserve viwFwdData(viwFwdData.GetUpperBound(0) - 1)

        Call PerformViewChanges(viwBackData(viwBackData.GetUpperBound(0)))
    End Sub
End Class
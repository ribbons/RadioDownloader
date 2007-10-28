' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
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

    Private WithEvents clsProgData As clsData
    Private clsDoDBUpdate As clsUpdateDB
    Private clsUpdate As clsAutoUpdate

    Private strCurrentType As String
    Private strCurrentStation As String

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

    Private Sub AddStationToList(ByRef strStationName As String, ByRef strStationId As String, ByRef strStationType As String) Handles clsProgData.AddStationToList
        Dim lstAdd As New System.Windows.Forms.ListViewItem
        lstAdd.Text = strStationName
        lstAdd.Tag = strStationType & "||" & strStationId
        lstAdd.ImageKey = "default" 'imlStations.ListImages(strStationId).Index

        lstAdd = lstStations.Items.Add(lstAdd)
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
            ' As the database already exists, copy the specimin database across from the program folder
            ' and then make sure that the current db's structure matches it.
            IO.File.Copy(My.Application.Info.DirectoryPath + "\store.db", GetAppDataFolder() + "\spec-store.db", True)

            clsDoDBUpdate = New clsUpdateDB(GetAppDataFolder() + "\spec-store.db", GetAppDataFolder() + "\store.db")
            Call clsDoDBUpdate.UpdateStructure()
        End If

        Call lstStationProgs.Columns.Add("Programme Name", 500)
        Call lstSubscribed.Columns.Add("Programme Name", 275)
        Call lstSubscribed.Columns.Add("Station", 125)
        Call lstSubscribed.Columns.Add("Provider", 125)
        Call lstDownloads.Columns.Add("Name", 225)
        Call lstDownloads.Columns.Add("Date", 75)
        Call lstDownloads.Columns.Add("Status", 125)
        Call lstDownloads.Columns.Add("Progress", 100)

        lstStations.LargeImageList = imlStations
        lstStationProgs.SmallImageList = imlListIcons
        lstSubscribed.SmallImageList = imlListIcons
        lstDownloads.SmallImageList = imlListIcons

        clsProgData = New clsData()
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        Call clsProgData.UpdateSubscrList(lstSubscribed)

        clsProgData.StartListingStations()
        Call TabAdjustments()
        nicTrayIcon.Icon = New Icon([Assembly].GetExecutingAssembly().GetManifestResourceStream("RadioDld.Icon.ico"))
        nicTrayIcon.Text = Me.Text
        nicTrayIcon.Visible = True

        clsUpdate = New clsAutoUpdate("http://www.nerdoftheherd.com/tools/radiodld/latestversion.txt", "http://www.nerdoftheherd.com/tools/radiodld/downloads/Radio Downloader.msi", GetAppDataFolder() + "\Radio Downloader.msi", "msiexec", "/i """ + GetAppDataFolder() + "\Radio Downloader.msi"" REINSTALL=ALL REINSTALLMODE=vomus")
        If My.Settings.UpdateDownloaded Then
            Call InstallUpdate()
        End If

        tblInfo.Dock = DockStyle.Left

        lstStations.Dock = DockStyle.Fill
        lstStationProgs.Dock = DockStyle.Fill
        lstSubscribed.Dock = DockStyle.Fill
        lstDownloads.Dock = DockStyle.Fill

        tbtCleanUp.Visible = False
        ttxSearch.Visible = False
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

    Private Sub lstStations_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstStations.ItemActivate
        Dim strSplit() As String
        strSplit = Split(lstStations.SelectedItems(0).Tag.ToString, "||")

        tbtCurrStation.Image = imlStations.Images(lstStations.SelectedItems(0).ImageKey)
        tbtCurrStation.Text = lstStations.SelectedItems(0).Text
        tbtCurrStation.Visible = True
        tbtCurrStation.Checked = True
        tbtFindNew.Checked = False
        strCurrentType = strSplit(0)
        strCurrentStation = strSplit(1)

        Call TabAdjustments()

        lstStationProgs.Items.Clear()
        Call clsProgData.StartListingProgrammes(strSplit(0), strSplit(1))
    End Sub

    Private Sub lstStationProgs_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstStationProgs.SelectedIndexChanged
        If lstStationProgs.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstStationProgs.SelectedItems(0).Tag.ToString, "||")

            With clsProgData
                Call .GetLatest(strSplit(0), strSplit(1), strSplit(2))

                If .LatestDate(strSplit(0), strSplit(1), strSplit(2)) = Nothing = False Then
                    Call SetSideBar(.ProgramTitle(strSplit(0), strSplit(1), strSplit(2), .LatestDate(strSplit(0), strSplit(1), strSplit(2))), .ProgramDetails(strSplit(0), strSplit(1), strSplit(2), .LatestDate(strSplit(0), strSplit(1), strSplit(2))), .ProgramImage(strSplit(0), strSplit(1), strSplit(2), .LatestDate(strSplit(0), strSplit(1), strSplit(2))))
                    Call SetToolbarButtons("Download,Subscribe")
                Else
                    Call SetSideBar("Programme Info", "Unfortunately, there was a problem getting information about this programme." + vbCrLf + "The available data could be invalid." + vbCrLf + "You may like to try again later.", Nothing)
                    Call SetToolbarButtons("")
                End If
            End With
        Else
            Call TabAdjustments() ' Revert back to new items view default page
        End If
    End Sub

    Private Sub lstSubscribed_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSubscribed.SelectedIndexChanged
        If lstSubscribed.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstSubscribed.SelectedItems(0).Tag.ToString, "||")

            Call SetSideBar(clsProgData.ProgramTitle(strSplit(0), strSplit(1), strSplit(2), clsProgData.LatestDate(strSplit(0), strSplit(1), strSplit(2))), clsProgData.ProgramDetails(strSplit(0), strSplit(1), strSplit(2), clsProgData.LatestDate(strSplit(0), strSplit(1), strSplit(2))), clsProgData.ProgramImage(strSplit(0), strSplit(1), strSplit(2), clsProgData.LatestDate(strSplit(0), strSplit(1), strSplit(2))))
            Call SetToolbarButtons("Unsubscribe")
        Else
            Call TabAdjustments() ' Revert back to subscribed items view default page
        End If
    End Sub

    Private Sub lstDownloads_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstDownloads.SelectedIndexChanged
        If lstDownloads.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstDownloads.SelectedItems(0).Name.ToString, "||")

            With clsProgData
                Dim staDownloadStatus As clsData.Statuses
                Dim strInfoBox As String = ""
                Dim strActionString As String

                staDownloadStatus = .DownloadStatus(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))

                If staDownloadStatus = clsData.Statuses.Downloaded Then
                    If Exists(.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))) Then
                        strActionString = "Play,Delete"
                    Else
                        strActionString = "Delete"
                    End If

                    strInfoBox = vbCrLf + vbCrLf + "Play count: " + CStr(.PlayCount(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))))
                ElseIf staDownloadStatus = clsData.Statuses.Errored Then
                    Dim strErrorName As String = ""
                    Dim strErrorDetails As String = .ErrorDetails(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))

                    Select Case .ErrorType(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))
                        Case IRadioProvider.ErrorType.MissingDependency
                            strErrorName = "Missing Dependency"
                        Case IRadioProvider.ErrorType.UnknownError
                            strErrorName = "Unknown Error"
                            strErrorDetails = ""
                    End Select

                    strInfoBox = vbCrLf + vbCrLf + "Error: " + strErrorName
                    If strErrorDetails <> "" Then
                        strInfoBox += vbCrLf + vbCrLf + strErrorDetails
                    End If

                    strActionString = "Retry,Cancel"
                Else
                    strActionString = "Cancel"
                End If

                Call SetSideBar(.ProgramTitle(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))), .ProgramDetails(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))) + strInfoBox, .ProgramImage(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))))
                Call SetToolbarButtons(strActionString)
            End With
        Else
            Call TabAdjustments() ' Revert back to downloads view default page
        End If
    End Sub

    Private Sub TabAdjustments()
        If tbtFindNew.Checked Then
            lstStations.Visible = True
            lstStationProgs.Visible = False
            lstSubscribed.Visible = False
            lstDownloads.Visible = False
            tbtCurrStation.Visible = False
            tbtCleanUp.Enabled = False
            Call SetSideBar("Find New", "This view allows you to browse all of the programmes that are available for you to download or subscribe to." + vbCrLf + "Select a station icon to show the programmes available from it.", Nothing)
            Call SetToolbarButtons("")
        ElseIf tbtCurrStation.Checked Then
            lstStations.Visible = False
            lstStationProgs.Visible = True
            lstSubscribed.Visible = False
            lstDownloads.Visible = False
            Call SetSideBar(lstStations.SelectedItems(0).Text, "This view is a list of programmes available from " & lstStations.SelectedItems(0).Text & "." + vbCrLf + "Select a programme for more information, and to download or subscribe to it.", Nothing)
            Call SetToolbarButtons("")
        ElseIf tbtSubscriptions.Checked Then
            lstStations.Visible = False
            lstStationProgs.Visible = False
            lstSubscribed.Visible = True
            lstDownloads.Visible = False
            tbtCleanUp.Enabled = False
            Call SetSideBar("Subscriptions", "This view shows you the programmes that you are currently subscribed to." + vbCrLf + "To subscribe to a new programme, start by choosing the 'Find New' button on the toolbar." + vbCrLf + "Select a programme in the list to get more information about it.", Nothing)
            Call SetToolbarButtons("")
        ElseIf tbtDownloads.Checked Then
            lstStations.Visible = False
            lstStationProgs.Visible = False
            lstSubscribed.Visible = False
            lstDownloads.Visible = True
            'tbtCleanUp.Enabled = True
            Call SetSideBar("Downloads", "Here you can see programmes that are being downloaded, or have been downloaded already." + vbCrLf + "To download a programme, start by choosing the 'Find New' button on the toolbar." + vbCrLf + "Select a programme in the list to get more information about it, or for completed downloads, play it.", Nothing)
            Call SetToolbarButtons("")
        End If
    End Sub

    Private Sub SetSideBar(ByVal strTitle As String, ByVal strDescription As String, ByVal bmpPicture As Bitmap)
        lblSideMainTitle.Text = strTitle
        lblSideDescript.Text = strDescription

        If bmpPicture Is Nothing = False Then
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
        Call clsProgData.CheckSubscriptions(lstDownloads, tmrStartProcess, prgDldProg)
    End Sub

    Private Sub tmrStartProcess_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrStartProcess.Tick
        Call clsProgData.FindAndDownload()
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        tmrStartProcess.Enabled = False
    End Sub

    Private Sub tbtFindNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtFindNew.Click
        tbtFindNew.Checked = True
        tbtCurrStation.Checked = False
        tbtSubscriptions.Checked = False
        tbtDownloads.Checked = False
        Call TabAdjustments()

        lstStations.Items.Clear()
        clsProgData.StartListingStations()
    End Sub

    Private Sub tbtCurrStation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtCurrStation.Click
        tbtFindNew.Checked = False
        tbtCurrStation.Checked = True
        tbtSubscriptions.Checked = False
        tbtDownloads.Checked = False
        Call TabAdjustments()

        lstStationProgs.Items.Clear()
        Call clsProgData.StartListingProgrammes(strCurrentType, strCurrentStation)
    End Sub

    Private Sub tbtSubscriptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtSubscriptions.Click
        tbtSubscriptions.Checked = True
        tbtCurrStation.Checked = False
        tbtFindNew.Checked = False
        tbtDownloads.Checked = False
        Call TabAdjustments()
    End Sub

    Private Sub tbtDownloads_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownloads.Click
        tbtDownloads.Checked = True
        tbtCurrStation.Checked = False
        tbtSubscriptions.Checked = False
        tbtFindNew.Checked = False
        Call TabAdjustments()
    End Sub

    Private Sub clsProgData_AddProgramToList(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal strProgramName As String) Handles clsProgData.AddProgramToList
        Dim lstAddItem As New ListViewItem
        lstAddItem.Text = strProgramName
        lstAddItem.ImageKey = "new"
        lstAddItem.Tag = strProgramType + "||" + strStationID + "||" + strProgramID
        Call lstStationProgs.Items.Add(lstAddItem)
    End Sub

    Private Sub clsProgData_DldError(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Handles clsProgData.DldError
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsProgData_DldError_Delegate(AddressOf clsProgData_DldError_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData, errType, strErrorDetails})
        End If
    End Sub

    Private Sub clsProgData_DldError_FormThread(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
        Call clsProgData.SetErrored(clsCurDldProgData.ProgramType, clsCurDldProgData.StationID, clsCurDldProgData.ProgramID, clsCurDldProgData.ProgramDate, errType, strErrorDetails)

        ' If the item that has just errored is selected then update the information.
        If tbtDownloads.Checked = True And lstDownloads.Items(clsCurDldProgData.ProgramDate.ToString + "||" + clsCurDldProgData.ProgramID + "||" + clsCurDldProgData.StationID + "||" + clsCurDldProgData.ProgramType).Selected Then
            Call lstDownloads_SelectedIndexChanged(New Object, New System.EventArgs)
        End If

        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsProgData_Finished(ByVal clsCurDldProgData As clsDldProgData) Handles clsProgData.Finished
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsProgData_Finished_Delegate(AddressOf clsProgData_Finished_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData})
        End If
    End Sub

    Private Sub clsProgData_Finished_FormThread(ByVal clsCurDldProgData As clsDldProgData)
        Call clsProgData.SetDownloaded(clsCurDldProgData.ProgramType, clsCurDldProgData.StationID, clsCurDldProgData.ProgramID, clsCurDldProgData.ProgramDate, clsCurDldProgData.FinalName)

        ' If the item that has just finished is selected then update it.
        If tbtDownloads.Checked = True And lstDownloads.Items(clsCurDldProgData.ProgramDate.ToString + "||" + clsCurDldProgData.ProgramID + "||" + clsCurDldProgData.StationID + "||" + clsCurDldProgData.ProgramType).Selected Then
            Call lstDownloads_SelectedIndexChanged(New Object, New System.EventArgs)
        End If

        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        If My.Settings.RunAfterCommand <> "" Then
            Try
                ' Environ("comspec") will give the path to cmd.exe or command.com
                Call Shell("""" + Environ("comspec") + """ /c " + My.Settings.RunAfterCommand.Replace("%file%", clsCurDldProgData.FinalName), AppWinStyle.NormalNoFocus)
            Catch ex As Exception
                ' Just ignore the error, as it just means that something has gone wrong with the run after command.
            End Try
        End If

        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsProgData_Progress(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles clsProgData.Progress
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsProgData_Progress_Delegate(AddressOf clsProgData_Progress_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {clsCurDldProgData, intPercent, strStatusText, Icon})
        End If
    End Sub

    Private Sub clsProgData_Progress_FormThread(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
        Static intLastNum As Integer

        If intLastNum = Nothing Then intLastNum = -1
        If intLastNum = intPercent Then Exit Sub
        If intPercent < 0 Then Exit Sub
        If intPercent > 100 Then Exit Sub

        intLastNum = intPercent

        With clsCurDldProgData
            Dim lstItem As ListViewItem
            lstItem = lstDownloads.Items(.ProgramDate.ToString + "||" + .ProgramID + "||" + .StationID + "||" + .ProgramType)

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
        End With

        Call SetTrayStatus(True)
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
        Dim strSplit() As String
        strSplit = Split(lstStationProgs.SelectedItems(0).Tag.ToString, "||")

        If clsProgData.AddSubscription(strSplit(0), strSplit(1), strSplit(2), lstStationProgs.SelectedItems(0).Text) = False Then
            Call MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation, "Radio Downloader")
        Else
            Call tbtSubscriptions_Click(New Object, New EventArgs)
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Private Sub tbtUnsubscribe_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtUnsubscribe.Click
        Dim strSplit() As String
        strSplit = Split(lstSubscribed.SelectedItems(0).Tag.ToString, "||")

        If MsgBox("Are you sure that you would like to stop having this programme downloaded regularly?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            Call clsProgData.RemoveSubscription(strSplit(0), strSplit(1), strSplit(2))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Private Sub tbtCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtCancel.Click
        Dim strSplit() As String
        If MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            strSplit = Split(lstDownloads.SelectedItems(0).Name.ToString, "||")

            Dim clsCurrentProgInfo As clsDldProgData
            clsCurrentProgInfo = clsProgData.GetCurrentDownloadInfo

            If clsCurrentProgInfo Is Nothing = False Then
                If clsCurrentProgInfo.ProgramType = strSplit(3) And clsCurrentProgInfo.StationID = strSplit(2) And clsCurrentProgInfo.ProgramID = strSplit(1) And clsCurrentProgInfo.ProgramDate = CDate(strSplit(0)) Then
                    ' The program is currently being downloaded
                    clsProgData.AbortDownloadThread()
                    tmrStartProcess.Enabled = True
                End If
            End If

            Call clsProgData.RemoveDownload(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        End If
    End Sub

    Private Sub tbtPlay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtPlay.Click
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItems(0).Name.ToString, "||")

        Process.Start(clsProgData.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))))

        ' Bump the play count of this item up by one, and update the list so that the icon changes colour
        clsProgData.IncreasePlayCount(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))
        clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        ' Update the prog info pane to show the updated play count
        Call lstDownloads_SelectedIndexChanged(New Object, New System.EventArgs)
    End Sub

    Private Sub tbtDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDelete.Click
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItems(0).Name.ToString, "||")

        If MsgBox("Are you sure that you would like to delete this program and the associated audio file?", MsgBoxStyle.Question Or MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            If Exists(clsProgData.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))) Then
                Delete(clsProgData.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))))
            End If
            clsProgData.RemoveDownload(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))
            clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        End If
    End Sub

    Private Sub tbtRetry_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtRetry.Click
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItems(0).Name.ToString, "||")

        Call clsProgData.ResetDownload(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)), False)
        Call lstDownloads_SelectedIndexChanged(New Object, New System.EventArgs) ' Update prog info pane
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub tbtDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownload.Click
        Dim strSplit() As String
        strSplit = Split(lstStationProgs.SelectedItems(0).Tag.ToString, "||")

        If clsProgData.AddDownload(strSplit(0), strSplit(1), strSplit(2)) Then
            Call tbtDownloads_Click(New Object, New EventArgs)
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
            tmrStartProcess.Enabled = True
        Else
            Call MsgBox("The latest episode of this programme is already in the download list!", MsgBoxStyle.Exclamation, "Radio Downloader")
        End If
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

    Private Sub lstStations_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstStations.SelectedIndexChanged

    End Sub
End Class
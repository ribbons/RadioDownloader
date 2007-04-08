Option Strict Off
Option Explicit On

Imports System.IO
Imports System.IO.File
Imports System.Threading
Imports System.Text.ASCIIEncoding

Public Class frmMain
    Inherits System.Windows.Forms.Form

    Private AvailablePlugins As AvailablePlugin()

    Private WithEvents clsBackgroundThread As clsBackground
    Private thrBackgroundThread As Thread

    Private WithEvents clsProgData As clsData

    Private lngLastState As Integer
    Private lngStatus As Integer

    Private Delegate Sub clsBackgroundThread_Progress_Delegate(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    Private Delegate Sub clsBackgroundThread_DldError_Delegate(ByVal strError As String)
    Private Delegate Sub clsBackgroundThread_Finished_Delegate()

    Const lngDLStatCol As Integer = 2

    Private Sub SetNewView(ByVal booStations As Boolean)
        lstNew.Items.Clear()

        If booStations Then
            lstNew.View = View.LargeIcon
        Else
            lstNew.View = View.Details
        End If
    End Sub

    Private Sub AddStations()
        Call SetNewView(True)

        Dim ThisInstance As IRadioProvider

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = CreateInstance(SinglePlugin)

            For Each NewStation As IRadioProvider.StationInfo In ThisInstance.ReturnStations()
                Call AddStation(NewStation.StationName, NewStation.StationUniqueID, ThisInstance.ProviderUniqueID)
            Next
        Next SinglePlugin
    End Sub

    Private Sub AddStation(ByRef strStationName As String, ByRef strStationId As String, ByRef strStationType As String)
        Dim lstAdd As New System.Windows.Forms.ListViewItem
        lstAdd.Text = strStationName
        lstAdd.Tag = strStationType & "||" & strStationId
        lstAdd.ImageKey = "default" 'imlStations.ListImages(strStationId).Index

        lstAdd = lstNew.Items.Add(lstAdd)
    End Sub

    Private Sub frmMain_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        ' Load all of the available plugins into an array for later reference
        AvailablePlugins = FindPlugins(My.Application.Info.DirectoryPath)

        lstSubscribed.Top = lstNew.Top
        lstDownloads.Top = lstNew.Top

        Call lstNew.Columns.Add("Programme Name", 500)
        Call lstSubscribed.Columns.Add("Programme Name", 550)
        Call lstDownloads.Columns.Add("Name", 225)
        Call lstDownloads.Columns.Add("Date", 75)
        Call lstDownloads.Columns.Add("Status", 125)
        Call lstDownloads.Columns.Add("Progress", 100)

        lstNew.SmallImageList = imlListIcons
        lstNew.LargeImageList = imlStations
        lstSubscribed.SmallImageList = imlListIcons
        lstDownloads.SmallImageList = imlListIcons

        Call AddStations()
        Call TabAdjustments()
        nicTrayIcon.Icon = Me.Icon
        nicTrayIcon.Visible = True

        ' Set up the web browser so that public methods of this form can be called from javascript in the html
        webDetails.ObjectForScripting = Me

        clsProgData = New clsData(AvailablePlugins)
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        Call clsProgData.UpdateSubscrList(lstSubscribed)

        stlStatusText.Text = ""
        lstNew.Height = staStatus.Top - lstNew.Top
        lstSubscribed.Height = lstNew.Height
        lstDownloads.Height = lstNew.Height

        'tbrOldToolbar.Buttons("Clean Up").Visible = False
        'tbrOldToolbar.Buttons("Refresh").Visible = False
    End Sub

    Private Sub frmMain_FormClosing(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Dim Cancel As Boolean = eventArgs.Cancel
        Dim UnloadMode As System.Windows.Forms.CloseReason = eventArgs.CloseReason
        If UnloadMode = System.Windows.Forms.CloseReason.UserClosing Then
            Call TrayAnimate(Me, True)
            Me.Visible = False
            Cancel = True
        End If
        eventArgs.Cancel = True
    End Sub

    Private Sub frmMain_FormClosed(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        On Error Resume Next
        Call Kill(My.Application.Info.DirectoryPath + "\temp.htm")
        Call Kill(My.Application.Info.DirectoryPath + "\temp\*.*")
    End Sub

    Private Sub lstNew_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstNew.SelectedIndexChanged
        If lstNew.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

            If lstNew.View = ComctlLib.ListViewConstants.lvwIcon Then
                ' Do nothing
            Else
                Call CreateHtml("Programme Info", clsProgData.ProgramHTML(strSplit(0), strSplit(1), strSplit(2)), "Download,Subscribe")
            End If
        Else
            If lstNew.View = ComctlLib.ListViewConstants.lvwIcon Then
                ' Do nothing
            Else
                Call TabAdjustments() ' Revert back to new items view default page
            End If
        End If
    End Sub

    Private Sub lstNew_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstNew.ItemActivate
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        If lstNew.View = View.LargeIcon Then
            lstNew.View = View.Details
            tbtUp.Enabled = True

            Call CreateHtml(lstNew.SelectedItems(0).Text, "<p>This view is a list of programmes available from " & lstNew.SelectedItems(0).Text & ".</p>Select a programme for more information, and to download or subscribe to it.", "")

            lstNew.Items.Clear()

            Call clsProgData.StartListingStation(strSplit(0), strSplit(1))
        Else
            ' Do nothing
        End If
    End Sub

    Private Sub lstSubscribed_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSubscribed.SelectedIndexChanged
        If lstSubscribed.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstSubscribed.SelectedItems(0).Tag, "||")

            Call CreateHtml("Subscribed Programme", clsProgData.ProgramHTML(strSplit(0), strSplit(1), strSplit(2)), "Unsubscribe")
        Else
            Call TabAdjustments() ' Revert back to subscribed items view default page
        End If
    End Sub

    Private Sub lstDownloads_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstDownloads.SelectedIndexChanged
        If lstDownloads.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstDownloads.SelectedItems(0).Tag, "||")

            Const strTitle As String = "Download Info"

            With clsProgData
                Dim staDownloadStatus As clsData.Statuses
                Dim strActionString As String

                staDownloadStatus = .DownloadStatus(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))

                If staDownloadStatus = clsData.Statuses.Downloaded Then
                    If Exists(.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))) Then
                        strActionString = "Play"
                    Else
                        strActionString = ""
                    End If
                ElseIf staDownloadStatus = clsData.Statuses.Errored Then
                    strActionString = "Retry,Cancel"
                Else
                    strActionString = "Cancel"
                End If

                Call CreateHtml(strTitle, .ProgramHTML(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))), strActionString)
            End With
        Else
            Call TabAdjustments() ' Revert back to downloads view default page
        End If
    End Sub

    Private Sub TabAdjustments()
        If tbtFindNew.Checked Then
            lstNew.Visible = True
            lstSubscribed.Visible = False
            lstDownloads.Visible = False
            tbtCleanUp.Enabled = False
            tbtUp.Enabled = lstNew.View = View.Details
            Call CreateHtml("Choose New Programme", "<p>This view allows you to browse all of the programmes that are available for you to download or subscribe to.</p>Select a station icon to show the programmes available from it.", "")
        ElseIf tbtSubscriptions.Checked Then
            lstNew.Visible = False
            lstSubscribed.Visible = True
            lstDownloads.Visible = False
            tbtCleanUp.Enabled = False
            tbtUp.Enabled = False
            Call CreateHtml("Subscribed Programmes", "<p>This view shows you the programmes that you are currently subscribed to.</p><p>To subscribe to a new programme, start by choosing the 'Find New' button on the toolbar.</p>Select a programme in the list to get more information about it.", "")
        ElseIf tbtDownloads.Checked Then
            lstNew.Visible = False
            lstSubscribed.Visible = False
            lstDownloads.Visible = True
            'tbtCleanUp.Enabled = True
            tbtUp.Enabled = False
            Call CreateHtml("Programme Downloads", "<p>Here you can see programmes that are being downloaded, or have been downloaded already.</p><p>To download a programme, start by choosing the 'Find New' button on the toolbar.</p>Select a programme in the list to get more information about it, or for completed downloads, play it.", "")
        End If
    End Sub

    Private Sub CreateHtml(ByVal strTitle As String, ByVal strMiddleContent As String, ByVal strBottomLinks As String)
        Dim strHtml As String

        Dim strCss As String
        Dim strHtmlStart As String

        strCss = ASCII.GetChars(MyResources.CSS_STYLES)
        'strHtmlStart = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html><head><style type=""text/css"">" & strCss & "</style><script type=""text/javascript"">function handleError() { return true; } window.onerror = handleError;</script></head><body><table><tr><td class=""maintd"">"
        strHtmlStart = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html><head><style type=""text/css"">" & strCss & "</style></head><body><table><tr><td class=""maintd"">"
        Const strHtmlEnd As String = "</td></tr></table></body></html>"

        strHtml = strHtmlStart & "<h1>" & strTitle & "</h1><div class=""contentbox"">" & strMiddleContent & "</div></td></tr><tr><td class=""maintd bottomrow"">"

        Dim strSplitLinks() As String
        Dim strLoopLinks As Object
        Dim strBuiltLinks As String = ""
        strSplitLinks = Split(strBottomLinks, ",")

        For Each strLoopLinks In strSplitLinks
            Select Case strLoopLinks
                Case "Download"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Download Now", "FlDownload", "Download this programme now"))
                Case "Subscribe"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Subscribe", "FlSubscribe", "Get this programme downloaded regularly"))
                Case "Unsubscribe"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Unsubscribe", "FlUnsubscribe", "Stop getting this programme downloaded regularly"))
                Case "Play"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Play", "FlPlay", "Play this programme"))
                Case "Cancel"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Cancel", "FlCancel", "Cancel downloading this programme"))
                Case "Retry"
                    strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Retry", "FlRetry", "Try downloading this programme again"))
            End Select
        Next strLoopLinks

        If strBuiltLinks <> "" Then
            strHtml = strHtml & "<div class=""contentbox""><h2>Actions</h2>"
            strHtml = strHtml & strBuiltLinks
            strHtml = strHtml & "</div>"
        End If

        strHtml = strHtml & strHtmlEnd

        Call ShowHtml(strHtml)
    End Sub

    Private Function BuildLink(ByRef strTitle As String, ByRef strCall As String, ByRef strStatusText As String) As String
        BuildLink = "<a href=""#"" onclick=""window.external." & strCall & "(); return false;"" onmouseover=""window.external.FlStatusText('" & strStatusText & "'); return true;"" onmouseout=""window.external.FlStatusText(''); return true;"">" & strTitle & "</a>"
    End Function

    Private Function AddLink(ByVal strCurrentLinks As String, ByVal strNewlink As String) As String
        If Len(strCurrentLinks) > 0 Then
            AddLink = strCurrentLinks & " | "
        Else
            AddLink = ""
        End If

        AddLink = AddLink + strNewlink
    End Function

    Private Sub ShowHtml(ByRef strHtml As String)
        Dim lngFileNo As Integer
        lngFileNo = FreeFile()

        FileOpen(lngFileNo, My.Application.Info.DirectoryPath + "\temp.htm", OpenMode.Output)
        PrintLine(lngFileNo, strHtml)
        FileClose(lngFileNo)

        webDetails.Navigate(New System.Uri(My.Application.Info.DirectoryPath & "\temp.htm"))
    End Sub

    Public Sub mnuFileExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileExit.Click
        Call mnuTrayExit_Click(mnuTrayExit, eventArgs)
    End Sub

    Public Sub mnuToolsPrefs_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuToolsPrefs.Click
        Call frmPreferences.ShowDialog()
    End Sub

    Private Sub mnuTrayShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTrayShow.Click
        If Me.Visible = False Then
            Call TrayAnimate(Me, False)
            Me.Visible = True
        End If

        If Me.WindowState = FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Normal
        End If
    End Sub

    Public Sub mnuTrayExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuTrayExit.Click
        ' Stop a background download thread from running, as it will error when trying to access the form
        If thrBackgroundThread Is Nothing = False Then
            thrBackgroundThread.Abort()
        End If

        Me.Close()
        Me.Dispose()
    End Sub

    Private Sub nicTrayIcon_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles nicTrayIcon.MouseDoubleClick
        Call mnuTrayShow_Click(sender, e)
    End Sub

    Private Sub tmrCheckSub_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrCheckSub.Tick
        Call clsProgData.CheckSubscriptions(lstDownloads, tmrStartProcess, prgDldProg)
    End Sub

    Private Sub tmrStartProcess_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrStartProcess.Tick
        ' Make sure that it isn't currently working
        If clsBackgroundThread Is Nothing Then
            clsBackgroundThread = clsProgData.FindNewDownload

            ' Look for a program to download
            If clsBackgroundThread Is Nothing = False Then
                clsBackgroundThread.PluginsList = AvailablePlugins

                thrBackgroundThread = New Thread(New ThreadStart(AddressOf clsBackgroundThread.DownloadProgram))
                thrBackgroundThread.Start()
            End If
        End If

        tmrStartProcess.Enabled = False
    End Sub

    Private Sub tbtFindNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtFindNew.Click
        tbtFindNew.Checked = True
        tbtSubscriptions.Checked = False
        tbtDownloads.Checked = False
        Call TabAdjustments()
    End Sub

    Private Sub tbtSubscriptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtSubscriptions.Click
        tbtSubscriptions.Checked = True
        tbtFindNew.Checked = False
        tbtDownloads.Checked = False
        Call TabAdjustments()
    End Sub

    Private Sub tbtDownloads_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtDownloads.Click
        tbtDownloads.Checked = True
        tbtSubscriptions.Checked = False
        tbtFindNew.Checked = False
        Call TabAdjustments()
    End Sub

    Private Sub tbtUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbtUp.Click
        tbtUp.Enabled = False
        If lstNew.View = View.Details Then
            Call AddStations()
        End If
        Call TabAdjustments()
    End Sub

    Private Sub clsProgData_AddProgramToList(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal strProgramName As String) Handles clsProgData.AddProgramToList
        Dim lstAddItem As New ListViewItem
        lstAddItem.Text = strProgramName
        lstAddItem.ImageKey = "new"
        lstAddItem.Tag = strProgramType + "||" + strStationID + "||" + strProgramID
        Call lstNew.Items.Add(lstAddItem)
    End Sub

    Private Sub clsBackgroundThread_DldError(ByVal strError As String) Handles clsBackgroundThread.DldError
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsBackgroundThread_DldError_Delegate(AddressOf clsBackgroundThread_DldError_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {strError})
        End If
    End Sub

    Private Sub clsBackgroundThread_DldError_FormThread(ByVal strError As String)
        Call clsProgData.SetErrored(clsBackgroundThread.ProgramType, clsBackgroundThread.StationID, clsBackgroundThread.ProgramID, clsBackgroundThread.ProgramDate)

        ' If the item that has just errored is selected & the html links are out of date, go back to tab overview.
        If tbtDownloads.Checked = True And lstDownloads.Items(clsBackgroundThread.ProgramDate.ToString + "||" + clsBackgroundThread.ProgramID + "||" + clsBackgroundThread.StationID + "||" + clsBackgroundThread.ProgramType).Selected Then
            Call TabAdjustments()
        End If

        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        clsBackgroundThread = Nothing
        clsBackgroundThread = Nothing
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsBackgroundThread_Finished() Handles clsBackgroundThread.Finished
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsBackgroundThread_Finished_Delegate(AddressOf clsBackgroundThread_Finished_FormThread)
            Call Me.Invoke(DelegateInst)
        End If
    End Sub

    Private Sub clsBackgroundThread_Finished_FormThread()
        Call clsProgData.SetDownloaded(clsBackgroundThread.ProgramType, clsBackgroundThread.StationID, clsBackgroundThread.ProgramID, clsBackgroundThread.ProgramDate, clsBackgroundThread.FinalName)

        ' If the item that has just finished is selected & the html links are out of date, go back to tab overview.
        If tbtDownloads.Checked = True And lstDownloads.Items(clsBackgroundThread.ProgramDate.ToString + "||" + clsBackgroundThread.ProgramID + "||" + clsBackgroundThread.StationID + "||" + clsBackgroundThread.ProgramType).Selected Then
            Call TabAdjustments()
        End If

        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)

        clsBackgroundThread = Nothing
        thrBackgroundThread = Nothing
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsBackgroundThread_Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles clsBackgroundThread.Progress
        ' Check if the form exists still before calling delegate
        If Me.IsHandleCreated Then
            Dim DelegateInst As New clsBackgroundThread_Progress_Delegate(AddressOf clsBackgroundThread_Progress_FormThread)
            Call Me.Invoke(DelegateInst, New Object() {intPercent, strStatusText, Icon})
        End If
    End Sub

    Private Sub clsBackgroundThread_Progress_FormThread(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
        Static intLastNum As Integer

        If intLastNum = Nothing Then intLastNum = -1
        If intLastNum = intPercent Then Exit Sub
        If intPercent > 100 Then Exit Sub

        intLastNum = intPercent

        With clsBackgroundThread
            Dim lstItem As ListViewItem
            lstItem = lstDownloads.Items(.ProgramDate.ToString + "||" + .ProgramID + "||" + .StationID + "||" + .ProgramType)

            lstItem.SubItems(2).Text = strStatusText
            prgDldProg.Value = intPercent

            If prgDldProg.Visible = False Then
                lstDownloads.AddEmbeddedControl(prgDldProg, lstItem, 3)
                prgDldProg.Visible = True
            End If

            Select Case Icon
                Case IRadioProvider.ProgressIcon.Downloading
                    lstItem.ImageKey = "downloading"
                Case IRadioProvider.ProgressIcon.Converting
                    lstItem.ImageKey = "converting"
            End Select
        End With
    End Sub

    ' Called from JavaScript in embedded XHTML -------------------------------------

    Public Sub FlDownload()
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        If clsProgData.AddDownload(strSplit(0), strSplit(1), strSplit(2)) Then
            Call tbtDownloads_Click(New Object, New EventArgs)
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
            tmrStartProcess.Enabled = True
        Else
            Call FlStatusText("")
            Call MsgBox("The latest episode of this programme is already in the download list!", MsgBoxStyle.Exclamation, "Radio Downloader")
            Call FlStatusText("", True)
        End If
    End Sub

    Public Sub FlPlay()
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItems(0).Tag, "||")

        Process.Start(clsProgData.GetDownloadPath(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0))))
    End Sub

    Public Sub FlSubscribe()
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        Call FlStatusText("")

        If clsProgData.AddSubscription(strSplit(0), strSplit(1), strSplit(2)) = False Then
            Call MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation, "Radio Downloader")
        Else
            Call tbtSubscriptions_Click(New Object, New EventArgs)
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If

        Call FlStatusText("", True)
    End Sub

    Public Sub FlUnsubscribe()
        Dim strSplit() As String
        strSplit = Split(lstSubscribed.SelectedItems(0).Tag, "||")

        Call FlStatusText("")

        If MsgBox("Are you sure that you would like to stop having this programme downloaded regularly?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            Call clsProgData.RemoveSubscription(strSplit(0), strSplit(1), strSplit(2))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsProgData.UpdateSubscrList(lstSubscribed)
        End If

        Call FlStatusText("", True)
    End Sub

    Public Sub FlStatusText(ByVal strText As String, Optional ByVal booInIgnoreNext As Boolean = False)
        ' The rather strange ignore next option is for stopping the mouseover text from the html links
        ' popping back into the status bar after a modal dialog is shown from html.

        Static booIgnoreNext As Boolean

        If booIgnoreNext Then
            booIgnoreNext = False
            Exit Sub
        End If

        stlStatusText.Text = strText
        booIgnoreNext = booInIgnoreNext
    End Sub

    Public Sub FlCancel()
        Call FlStatusText("")

        Dim strSplit() As String
        If MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            strSplit = Split(lstDownloads.SelectedItems(0).Tag, "||")

            If clsBackgroundThread Is Nothing = False Then
                thrBackgroundThread.Abort()
                clsBackgroundThread = Nothing
                clsBackgroundThread = Nothing
                tmrStartProcess.Enabled = True
            End If

            Call clsProgData.CancelDownload(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        End If

        Call FlStatusText("", True)
    End Sub

    Public Sub FlRetry()
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItems(0).Tag, "||")

        Call clsProgData.ResetDownload(strSplit(3), strSplit(2), strSplit(1), CDate(strSplit(0)), False)
        Call clsProgData.UpdateDlList(lstDownloads, prgDldProg)
        tmrStartProcess.Enabled = True
    End Sub
End Class
Option Strict Off
Option Explicit On

Imports System.Text.ASCIIEncoding

Friend Class frmMain
	Inherits System.Windows.Forms.Form
	
	Private lngStatus As Integer
	
    Private WithEvents clsBackground As clsBackground
    Private clsTheProgData As clsProgData

    Private lngLastState As Integer

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

        Call AddStation("Radio 1", "radio1", "BBCLA")
        Call AddStation("Radio 2", "radio2", "BBCLA")
        Call AddStation("Radio 3", "radio3", "BBCLA")
        Call AddStation("Radio 4", "radio4", "BBCLA")
        Call AddStation("Five Live", "fivelive", "BBCLA")
        Call AddStation("Six Music", "6music", "BBCLA")
        'Call AddStation("BBC 7", "bbc7", "BBCLA")
    End Sub

    Private Sub AddStation(ByRef strStationName As String, ByRef strStationId As String, ByRef strStationType As String)
        Dim lstAdd As New System.Windows.Forms.ListViewItem
        lstAdd.Text = strStationName
        lstAdd.Tag = strStationType & "||" & strStationId
        lstAdd.ImageKey = "default" 'imlStations.ListImages(strStationId).Index

        lstAdd = lstNew.Items.Add(lstAdd)
    End Sub

    Private Sub clsBackground_Error(ByVal strError As String, ByVal strOutput As String) Handles clsBackground.DldError
        Call clsTheProgData.SetStatus(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate, False, clsBackground.Status.stError)
        Call clsTheProgData.UpdateDlList(lstDownloads)

        'UPGRADE_NOTE: Object clsBackground may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        clsBackground = Nothing
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsBackground_Finished() Handles clsBackground.Finished
        Call clsTheProgData.AdvanceNextAction(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate)

        If clsTheProgData.GetNextActionVal(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate) = clsBackground.NextAction.None Then
            ' All done, set status to completed, and save file path
            Call clsTheProgData.SetStatus(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate, False, clsBackground.Status.stCompleted)
            Call clsTheProgData.SetDownloadPath(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate, clsBackground.FinalName)
        Else
            Call clsTheProgData.SetStatus(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate, False, clsBackground.Status.stWaiting)
        End If

        Call clsTheProgData.UpdateDlList(lstDownloads)

        'UPGRADE_NOTE: Object clsBackground may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        clsBackground = Nothing
        tmrStartProcess.Enabled = True
    End Sub

    Private Sub clsBackground_Progress(ByVal lngPercent As Integer) Handles clsBackground.Progress
        'Dim lstChangeItem As ComctlLib.ListItem
        'lstChangeItem = lstDownloads.FindItem(VB6.Format(clsBackground.ProgramDate) & "||" & clsBackground.ProgramID & "||" & clsBackground.ProgramType, ComctlLib.ListFindItemWhereConstants.lvwTag)

        'lstChangeItem.SubItems(3) = VB6.Format(lngPercent) & "%"
    End Sub

    'Private Sub clsExtender_GetExternal(ByRef oIDispatch As Object) Handles clsExtender.GetExternal
    '	'this allows javascript to access the objects we return
    '	'here is it set so javascript will have access to all functions
    '	'and objects on this form.
    '	oIDispatch = Me
    'End Sub

    Private Sub frmMain_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        lstSubscribed.Top = lstNew.Top
        lstDownloads.Top = lstNew.Top

        Call lstNew.Columns.Add("Programme Name", 500)
        Call lstSubscribed.ColumnHeaders.Add(1, , "Programme Name", 5500)
        Call lstDownloads.ColumnHeaders.Add(1, , "Name", 2250)
        Call lstDownloads.ColumnHeaders.Add(2, , "Date", 750)
        Call lstDownloads.ColumnHeaders.Add(3, , "Status", 1250)
        Call lstDownloads.ColumnHeaders.Add(4, , "Progress", 1000)

        lstNew.SmallImageList = imlListIcons
        lstNew.LargeImageList = imlStations
        ''UPGRADE_WARNING: Couldn't resolve default property of object lstNew.SmallIcons. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'lstNew.SmallIcons = imlListIcons.GetOCX
        ''UPGRADE_WARNING: Couldn't resolve default property of object lstSubscribed.SmallIcons. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'lstSubscribed.SmallIcons = imlListIcons.GetOCX
        ''UPGRADE_WARNING: Couldn't resolve default property of object lstDownloads.SmallIcons. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'lstDownloads.SmallIcons = imlListIcons.GetOCX

        Call AddStations()
        Call TabAdjustments()
        nicTrayIcon.Icon = Me.Icon
        nicTrayIcon.Visible = True

        'clsExtender = New IEDevKit.clsWbExtender
        'clsExtender.HookWebBrowser(webDetails)

        '      clsSubclass = New cSubclass
        '      Call clsSubclass.Subclass(Me.Handle.ToInt32, Me)

        '' When the listview is scrolled (mouse or keyboard) or the columns are
        '' resized, the scrollbars need to be moved too
        ''Call clsSubclass.AddMsg(WM_HSCROLL, MSG_AFTER)
        ''Call clsSubclass.AddMsg(WM_VSCROLL, MSG_AFTER)
        ''Call clsSubclass.AddMsg(WM_KEYDOWN, MSG_AFTER)
        ''Call clsSubclass.AddMsg(WM_NOTIFY, MSG_AFTER)
        ''Call clsSubclass.AddMsg(WM_STYLECHANGING, MSG_BEFORE)
        'Call clsSubclass.AddMsg(WinSubHook2.eMsg.WM_NOTIFY, WinSubHook2.eMsgWhen.MSG_BEFORE)

        clsTheProgData = New clsProgData
        Call clsTheProgData.CleanupUnfinished()
        Call clsTheProgData.UpdateDlList(lstDownloads)
        Call clsTheProgData.UpdateSubscrList(lstSubscribed)

        'tbrOldToolbar.Buttons("Clean Up").Visible = False
        'tbrOldToolbar.Buttons("Refresh").Visible = False

        'picBack.BackColor = Me.BackColor
        'picForward.BackColor = Me.BackColor
        'picCrumbArrow.BackColor = vbWhite

        'Call DrawTransp("NAV_BACK", picForward, 29, 0, 0, 0)
        'Call DrawTransp("NAV_DISABLED", picForward, 27, 0, 0, 0)
        'Call DrawTransp("NAV_BACK", picBack, 0, 0, 0, 0)
        'Call DrawTransp("NAV_DISABLED", picBack, 0, 0, 1, 0)
        'Call DrawTransp("CRUMB_ARROW", picCrumbArrow, 0, 0, 0, 0)

        'Call SetIcon(Me.Handle.ToInt32, "appicon", True)

        System.Windows.Forms.Application.DoEvents()

        Me.Show()

        'If PathIsDirectory(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(My.Application.Info.DirectoryPath) & "Downloads")) = False Then
        'Call frmPreferences.ShowDialog()
        'End If
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

    'UPGRADE_WARNING: Event frmMain.Resize may fire when form is initialized. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="88B12AE1-6DE0-48A0-86F1-60C0686C026A"'
    Private Sub frmMain_Resize(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Resize
        If Me.WindowState <> System.Windows.Forms.FormWindowState.Minimized Then
            lngLastState = Me.WindowState
        End If

        If VB6.PixelsToTwipsX(Me.ClientRectangle.Width) = 0 Then
            ' Looks like we are minimized, stop before we get resizing errors
            Exit Sub
        End If

        Static lngLastHeight As Integer

        If VB6.PixelsToTwipsY(staStatus.Top) - (VB6.PixelsToTwipsY(lstNew.Top)) < 10 * VB6.TwipsPerPixelY Then
            If VB6.PixelsToTwipsY(Me.Height) < lngLastHeight Then
                Me.Height = VB6.TwipsToPixelsY(lngLastHeight)

                ' Nothing to do with webdetails, just a hack to flip the user out
                ' of window resizing mode - webdetails being shown does just that
                webDetails.Visible = False
                webDetails.Visible = True
            End If
        End If

        lngLastHeight = VB6.PixelsToTwipsY(Me.Height)

        'WebBrowser
        webDetails.Height = VB6.TwipsToPixelsY(VB6.PixelsToTwipsY(staStatus.Top) - VB6.PixelsToTwipsY(webDetails.Top))

        'Listviews
        lstDownloads.Height = VB6.TwipsToPixelsY(VB6.PixelsToTwipsY(staStatus.Top) - VB6.PixelsToTwipsY(lstDownloads.Top))
        lstNew.Height = lstDownloads.Height
        lstSubscribed.Height = lstDownloads.Height
        lstDownloads.Width = VB6.TwipsToPixelsX(VB6.PixelsToTwipsX(Me.ClientRectangle.Width) - VB6.PixelsToTwipsX(webDetails.Width))
        lstNew.Width = lstDownloads.Width
        lstSubscribed.Width = lstDownloads.Width

        'Toolbar seperators
        'picSeperator(0).Top = VB6.TwipsToPixelsY(tbrOldToolbar.Buttons("-").Top + 2 * VB6.TwipsPerPixelX)
        'picSeperator(0).Left = VB6.TwipsToPixelsX(tbrOldToolbar.Buttons("-").Left + (tbrOldToolbar.Buttons("-").Width / 2))
        'picSeperator(0).Height = VB6.TwipsToPixelsY(tbrOldToolbar.Buttons("-").Height - 1 * VB6.TwipsPerPixelX)
        'picSeperator(1).Top = VB6.TwipsToPixelsY(tbrOldToolbar.Buttons("--").Top + 2 * VB6.TwipsPerPixelX)
        'picSeperator(1).Left = VB6.TwipsToPixelsX(tbrOldToolbar.Buttons("--").Left + (tbrOldToolbar.Buttons("-").Width / 2))
        'picSeperator(1).Height = VB6.TwipsToPixelsY(tbrOldToolbar.Buttons("--").Height - 1 * VB6.TwipsPerPixelX)

        'Toolbar shadow
        'picShadow.Width = tbrToolbar.Width
        'imgShadow.Width = tbrToolbar.Width
    End Sub

    Private Sub frmMain_FormClosed(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        'UPGRADE_NOTE: Object clsBackground may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        clsBackground = Nothing

        On Error Resume Next
        Call Kill(AddSlash(My.Application.Info.DirectoryPath) & "temp.htm")
        Call Kill(AddSlash(My.Application.Info.DirectoryPath) & "temp\*.*")
    End Sub

    'Private Sub iSubclass_Proc(ByVal bBefore As Boolean, ByRef bHandled As Boolean, ByRef lReturn As Integer, ByRef hWnd As Integer, ByRef uMsg As WinSubHook2.eMsg, ByRef wParam As Integer, ByRef lParam As Integer) Implements WinSubHook2.iSubclass.Proc
    '	Dim nmh As NMHDR
    '	Select Case uMsg
    '		'        '---------------------------------------------------------------
    '		'        ' For scrollbar adjustment
    '		'        '---------------------------------------------------------------
    '		'        Case WM_VSCROLL, WM_HSCROLL
    '		'            Call MoveBars
    '		'        Case WM_KEYDOWN
    '		'            Debug.Print wParam
    '		'            'If key is up, down, left, right, pgup or pgdown then move the bars
    '		'            If wParam >= 33 And wParam <= 40 Then
    '		'                Call MoveBars
    '		'            End If
    '		'        Case WM_NOTIFY
    '		'            ' Find out what the message was
    '		'            Dim uWMNOTIFY_Message As NMHDR
    '		'            Call CopyMemory(uWMNOTIFY_Message, ByVal lParam, Len(uWMNOTIFY_Message))
    '		'
    '		'            If uWMNOTIFY_Message.code = HDN_ENDTRACK Then
    '		'                ' User adjusted column width
    '		'                Call MoveBars
    '		'            End If
    '		'        '---------------------------------------------------------------
    '		'        ' To deal with flickering when adding / changing info in listview
    '		'        '---------------------------------------------------------------
    '		'        Case WM_STYLECHANGING
    '		'            ' Only stop redraw when requested
    '		'            If booLvAdding Then
    '		'                ' If the flag is set and the ListView's LVS_NOLABELWRAP style
    '		'                ' bit is changing (the ListView's LabelWrap property), prevent
    '		'                ' the style change and the resultant unnecessary redrawing,
    '		'                ' flickering, and a serious degradation in loading speed.
    '		'
    '		'                Dim ss As STYLESTRUCT
    '		'                CopyMemory ss, ByVal lParam, Len(ss)
    '		'
    '		'                If ((ss.styleOld Xor ss.styleNew) And LVS_NOLABELWRAP) Then
    '		'                    ss.styleNew = ss.styleOld
    '		'                    CopyMemory ByVal lParam, ss, Len(ss)
    '		'                End If
    '		'            End If
    '		Case WinSubHook2.eMsg.WM_NOTIFY
    '			' Fill the NMHDR struct from the lParam pointer.
    '			' (for any WM_NOTIFY msg, lParam always points to a struct which is
    '			' either the NMHDR struct, or whose 1st member is the NMHDR struct)
    '			'UPGRADE_ISSUE: COM expression not supported: Module methods of COM objects. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="5D48BAC6-2CD4-45AD-B1CC-8E4A241CDB58"'
    '			Call WinSubHook2.Kernel32.CopyMemory(nmh, lParam, Len(nmh))

    '			Select Case nmh.code
    '				Case LVN_BEGINDRAG
    '					'Notifies a list view control's parent window that a
    '					'drag-and-drop operation involving the left mouse button
    '					'is being initiated.
    '					bHandled = True
    '			End Select
    '	End Select
    'End Sub

    Private Sub lstDownloads_ItemClick(ByVal eventSender As System.Object, ByVal eventArgs As AxComctlLib.ListViewEvents_ItemClickEvent) Handles lstDownloads.ItemClick
        Dim strSplit() As String
        strSplit = Split(eventArgs.item.Tag, "||")

        Const strTitle As String = "Download Info"

        If clsTheProgData.DownloadStatus(strSplit(2), strSplit(1), CDate(strSplit(0))) = clsBackground.Status.stCompleted Then
            If PathFileExists(clsTheProgData.GetDownloadPath(strSplit(2), strSplit(1), CDate(strSplit(0)))) Then
                Call CreateHtml(strTitle, clsTheProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "Play")
            Else
                Call CreateHtml(strTitle, clsTheProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "")
            End If
        ElseIf clsTheProgData.DownloadStatus(strSplit(2), strSplit(1), CDate(strSplit(0))) = clsBackground.Status.stError Then
            Call CreateHtml(strTitle, clsTheProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "Retry,Cancel")
        Else
            Call CreateHtml(strTitle, clsTheProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "Cancel")
        End If
    End Sub

    Private Sub lstNew_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstNew.SelectedIndexChanged
        If lstNew.SelectedItems.Count > 0 Then
            Dim strSplit() As String
            strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

            If lstNew.View = ComctlLib.ListViewConstants.lvwIcon Then
                ' Do nothing
            Else
                Call CreateHtml("Programme Info", clsTheProgData.ProgramHTML(strSplit(0), strSplit(1)), "Download,Subscribe")
            End If
        End If
    End Sub

    Private Sub lstNew_ItemActivate(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstNew.ItemActivate
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        If lstNew.View = View.LargeIcon Then
            If strSplit(0) <> "BBCLA" Then Stop

            lstNew.View = View.Details
            'tbrOldToolbar.Buttons("Up").Enabled = True

            Call CreateHtml(lstNew.SelectedItems(0).Text, "<p>This view is a list of programmes available from " & lstNew.SelectedItems(0).Text & ".</p>Select a programme for more information, and to download or subscribe to it.", CStr(clsBackground.NextAction.None))
            Call ListStation(strSplit(1), lstNew)
        Else
            ' Do nothing
        End If
    End Sub

    Private Sub TabAdjustments()
        If tbtFindNew.Checked Then
            lstNew.Visible = True
            lstSubscribed.Visible = False
            lstDownloads.Visible = False
            tbtCleanUp.Enabled = False
            tbtUp.Enabled = lstNew.View = ComctlLib.ListViewConstants.lvwReport
            Call CreateHtml("Choose New Programme", "<p>This view allows you to browse all of the programmes that are available for you to download or subscribe to.</p>Select a station icon to show the programmes available from it.", CStr(clsBackground.NextAction.None))
        ElseIf tbtSubscriptions.Checked Then
            lstNew.Visible = False
            lstSubscribed.Visible = True
            lstDownloads.Visible = False
            tbtCleanUp.Enabled = False
            tbtUp.Enabled = False
            Call CreateHtml("Subscribed Programmes", "<p>This view shows you the programmes that you are currently subscribed to.</p><p>To subscribe to a new programme, start by choosing the 'Find New' button on the toolbar.</p>Select a programme in the list to get more information about it.", CStr(clsBackground.NextAction.None))
        ElseIf tbtDownloads.Checked Then
            lstNew.Visible = False
            lstSubscribed.Visible = False
            lstDownloads.Visible = True
            'tbtCleanUp.Enabled = True
            tbtUp.Enabled = False
            Call CreateHtml("Programme Downloads", "<p>Here you can see programmes that are being downloaded, or have been downloaded already.</p><p>To download a programme, start by choosing the 'Find New' button on the toolbar.</p>Select a programme in the list to get more information about it, or for completed downloads, play it.", CStr(clsBackground.NextAction.None))
        End If
    End Sub

    Private Sub CreateHtml(ByVal strTitle As String, ByVal strMiddleContent As String, ByVal strBottomLinks As String)
        Dim strHtml As String

        Dim strCss As String
        Dim strHtmlStart As String

        'UPGRADE_ISSUE: Constant vbUnicode was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
        'UPGRADE_ISSUE: Global method LoadResData was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6B85A2A7-FE9F-4FBE-AA0C-CF11AC86A305"'
        strCss = ASCII.GetChars(MyResources.CSS_STYLES) ' "" 'StrConv(CStr(VB6.LoadResData("styles", "css")), vbUnicode)
        strHtmlStart = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html><head><style type=""text/css"">" & strCss & "</style><script type=""text/javascript"">function handleError() { return true; } window.onerror = handleError;</script></head><body><table><tr><td class=""maintd"">"
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
        End If

        AddLink = AddLink & strNewlink
    End Function

    Private Sub ShowHtml(ByRef strHtml As String)
        Dim lngFileNo As Integer
        lngFileNo = FreeFile()

        FileOpen(lngFileNo, AddSlash(My.Application.Info.DirectoryPath) & "temp.htm", OpenMode.Output)
        PrintLine(lngFileNo, strHtml)
        FileClose(lngFileNo)

        'UPGRADE_WARNING: Navigate2 was upgraded to Navigate and has a new behavior. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        webDetails.Navigate(New System.Uri(AddSlash(My.Application.Info.DirectoryPath) & "temp.htm"))
    End Sub

    Private Sub lstSubscribed_ItemClick(ByVal eventSender As System.Object, ByVal eventArgs As AxComctlLib.ListViewEvents_ItemClickEvent) Handles lstSubscribed.ItemClick
        Dim strSplit() As String
        strSplit = Split(eventArgs.item.Tag, "||")

        Call CreateHtml("Subscribed Programme", clsTheProgData.ProgramHTML(strSplit(0), strSplit(1)), "Unsubscribe")
    End Sub

    Public Sub mnuFileExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuFileExit.Click
        Call mnuTrayExit_Click(mnuTrayExit, eventArgs)
    End Sub

    Public Sub mnuToolsPrefs_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuToolsPrefs.Click
        Call frmPreferences.ShowDialog()
    End Sub

    Private Sub mnuTrayShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTrayShow.Click
        If Me.WindowState = System.Windows.Forms.FormWindowState.Minimized Then
            Me.WindowState = lngLastState
        End If

        If Me.Visible = False Then
            Call TrayAnimate(Me, False)
            Me.Visible = True
        End If
    End Sub

    Public Sub mnuTrayExit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles mnuTrayExit.Click
        Me.Close()
        Me.Dispose()
    End Sub

    Private Sub nicTrayIcon_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles nicTrayIcon.MouseDoubleClick
        Call mnuTrayShow_Click(sender, e)
    End Sub

    Private Sub tbrToolbar_ButtonClick(ByVal eventSender As System.Object, ByVal eventArgs As AxComctlLib.IToolbarEvents_ButtonClickEvent)
        Select Case eventArgs.button.Key
            Case "Find New", "Subscriptions", "Downloads"
                Call TabAdjustments()
            Case "Up"
                eventArgs.button.Enabled = False
                If lstNew.View = ComctlLib.ListViewConstants.lvwReport Then
                    Call AddStations()
                End If
                Call TabAdjustments()
        End Select
    End Sub

    Private Sub tmrCheckSub_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrCheckSub.Tick
        'Call clsTheProgData.CheckSubscriptions(lstDownloads, tmrStartProcess)
    End Sub

    Private Sub tmrResizeHack_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrResizeHack.Tick
        Call frmMain_Resize(Me, New System.EventArgs())
    End Sub

    Private Sub tmrStartProcess_Tick(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles tmrStartProcess.Tick
        ' Make sure that it isn't currently working
        Dim dldAction As clsBackground.DownloadAction
        'If IsNothing(clsBackground) Then
        '    'UPGRADE_WARNING: Couldn't resolve default property of object dldAction. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '    dldAction = clsTheProgData.FindNextAction

        '    If dldAction.booFound Then
        '        clsBackground = New clsBackground
        '        Call clsTheProgData.SetStatus(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate, True)
        '        Call clsBackground.Start(dldAction.nxtNextAction, dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, clsTheProgData.ProgramDuration(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate), dldAction.dldDownloadID.dteDate, clsTheProgData.ProgramTitle(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate))
        '        Call clsTheProgData.UpdateDlList(lstDownloads)
        '    End If
        'End If

        tmrStartProcess.Enabled = False
    End Sub

    'UPGRADE_ISSUE: ShDocW.WebBrowser.NavigateComplete2 pDisp was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6B85A2A7-FE9F-4FBE-AA0C-CF11AC86A305"'
    'Private Sub webDetails_Navigated(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.WebBrowserNavigatedEventArgs) Handles webDetails.Navigated
    '	Dim url As String = eventArgs.URL.ToString()
    '	' Take away scrollbars, border, and interaction
    '	clsExtender.WbAttributes = IEDevKit.HostAttributes.haNoScrollBars Or IEDevKit.HostAttributes.haNo3DBorder Or IEDevKit.HostAttributes.haDisableSelections
    '	webDetails.Refresh()
    'End Sub

    Private Sub MoveBars()
        '    Dim prgBar As ProgressBar
        '
        '    Call InvalidateRect(lstDownloads.hWnd, ByVal 0&, 0)
        '
        '    Dim recPos As RECT
        '    Call GetSubItemRect(lstDownloads.hWnd, prgBar.Index - 1, 3, LVIR_LABEL, recPos)
        '
        '    With prgBar
        '        .Left = (recPos.Left) * Screen.TwipsPerPixelX
        '        .Width = (recPos.Right - recPos.Left) * Screen.TwipsPerPixelX
        '        .Height = ((recPos.Bottom - recPos.Top) * Screen.TwipsPerPixelY)
        '        .Top = recPos.Top * Screen.TwipsPerPixelY
        '    End With
        '
        '            If recPos.Top <= 10 Then
        '                prgBar.Visible = False
        '            Else
        '                prgBar.Visible = True
        '
        '                Call ValidateRect(lstDownloads.hWnd, recPos)
        '                Call InvalidateRect(prgBar.hWnd, ByVal 0&, 0)
        '            End If
        '        End If
        '    Next prgBar
    End Sub

    ' Called from JavaScript in embedded XHTML -------------------------------------

    Public Sub FlDownload()
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        If clsTheProgData.IsDownloading(strSplit(0), strSplit(1)) Then
            staStatus.SimpleText = ""
            Call MsgBox("You cannot download this programme more than once at the same time!", MsgBoxStyle.Exclamation, "Radio Downloader")
            Exit Sub
        End If

        If clsTheProgData.AddDownload(strSplit(0), strSplit(1)) = False Then
            staStatus.SimpleText = ""
            Call MsgBox("You have already downloaded this programme!", MsgBoxStyle.Exclamation, "Radio Downloader")
        Else
            'tbrOldToolbar.Buttons("Downloads").Value = ComctlLib.ValueConstants.tbrPressed
            Call TabAdjustments()
            Call clsTheProgData.UpdateDlList(lstDownloads)
            tmrStartProcess.Enabled = True
        End If
    End Sub

    Public Sub FlPlay()
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItem.Tag, "||")

        Call ShellExecute(Me.Handle.ToInt32, "open", clsTheProgData.GetDownloadPath(strSplit(2), strSplit(1), CDate(strSplit(0))), CStr(0), CStr(0), SW_SHOWNORMAL)
    End Sub

    Public Sub FlSubscribe()
        Dim strSplit() As String
        strSplit = Split(lstNew.SelectedItems(0).Tag, "||")

        If clsTheProgData.AddSubscription(strSplit(0), strSplit(1)) = False Then
            staStatus.SimpleText = ""
            Call MsgBox("You are already subscribed to this programme!", MsgBoxStyle.Exclamation, "Radio Downloader")
        Else
            'tbrOldToolbar.Buttons("Subscriptions").Value = ComctlLib.ValueConstants.tbrPressed
            Call TabAdjustments()
            Call clsTheProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Public Sub FlUnsubscribe()
        Dim strSplit() As String
        strSplit = Split(lstSubscribed.SelectedItem.Tag, "||")

        staStatus.SimpleText = ""

        If MsgBox("Are you sure that you would like to stop having this programme downloaded regularly?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            Call clsTheProgData.RemoveSubscription(strSplit(0), strSplit(1))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsTheProgData.UpdateSubscrList(lstSubscribed)
        End If
    End Sub

    Public Sub FlStatusText(ByVal strText As String)
        Static strLastUpdate As String

        ' This odd bit of code stops the HTML link statusbar text staying after a
        ' modal dialog is shown
        If strLastUpdate = staStatus.SimpleText Then
            staStatus.SimpleText = strText
        End If

        strLastUpdate = strText
    End Sub

    Public Sub FlCancel()
        staStatus.SimpleText = ""

        Dim strSplit() As String
        If MsgBox("Are you sure that you would like to stop downloading this programme?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Radio Downloader") = MsgBoxResult.Yes Then
            strSplit = Split(lstDownloads.SelectedItem.Tag, "||")

            If clsTheProgData.IsDownloading(strSplit(2), strSplit(1)) Then
                'UPGRADE_NOTE: Object clsBackground may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                clsBackground = Nothing
            End If

            Call clsTheProgData.CancelDownload(strSplit(2), strSplit(1), CDate(strSplit(0)))
            Call TabAdjustments() ' Revert back to tab info so prog info goes
            Call clsTheProgData.UpdateDlList(lstDownloads)
        End If
    End Sub

    Public Sub FlRetry()
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItem.Tag, "||")

        Call clsTheProgData.ResetDownload(strSplit(2), strSplit(1), CDate(strSplit(0)), False)
        Call clsTheProgData.UpdateDlList(lstDownloads)
        tmrStartProcess.Enabled = True
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
End Class
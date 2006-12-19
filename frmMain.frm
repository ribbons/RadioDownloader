VERSION 5.00
Object = "{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}#1.1#0"; "ieframe.dll"
Object = "{6B7E6392-850A-101B-AFC0-4210102A8DA7}#1.3#0"; "comctl32.ocx"
Begin VB.Form frmMain 
   Caption         =   "Radio Downloader"
   ClientHeight    =   6930
   ClientLeft      =   165
   ClientTop       =   555
   ClientWidth     =   11355
   BeginProperty Font 
      Name            =   "Tahoma"
      Size            =   8.25
      Charset         =   0
      Weight          =   400
      Underline       =   0   'False
      Italic          =   0   'False
      Strikethrough   =   0   'False
   EndProperty
   Icon            =   "frmMain.frx":0000
   LinkTopic       =   "Form1"
   LockControls    =   -1  'True
   ScaleHeight     =   6930
   ScaleWidth      =   11355
   StartUpPosition =   2  'CenterScreen
   Begin ComctlLib.Toolbar tbrToolbar 
      Align           =   1  'Align Top
      Height          =   600
      Left            =   0
      TabIndex        =   6
      Top             =   0
      Width           =   11355
      _ExtentX        =   20029
      _ExtentY        =   1058
      ButtonWidth     =   1852
      ButtonHeight    =   953
      Wrappable       =   0   'False
      ImageList       =   "imlToolbar"
      _Version        =   327682
      BeginProperty Buttons {0713E452-850A-101B-AFC0-4210102A8DA7} 
         NumButtons      =   10
         BeginProperty Button1 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Find New"
            Key             =   "Find New"
            Object.Tag             =   ""
            ImageIndex      =   1
            Style           =   2
            Value           =   1
         EndProperty
         BeginProperty Button2 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Subscriptions"
            Key             =   "Subscriptions"
            Object.Tag             =   ""
            ImageIndex      =   2
            Style           =   2
         EndProperty
         BeginProperty Button3 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Downloads"
            Key             =   "Downloads"
            Object.Tag             =   ""
            ImageIndex      =   3
            Style           =   2
         EndProperty
         BeginProperty Button4 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Key             =   "-"
            Object.Tag             =   ""
            Style           =   3
            MixedState      =   -1  'True
         EndProperty
         BeginProperty Button5 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Up"
            Key             =   "Up"
            Object.Tag             =   ""
            ImageIndex      =   4
         EndProperty
         BeginProperty Button6 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Refresh"
            Key             =   "Refresh"
            Object.Tag             =   ""
            ImageIndex      =   5
         EndProperty
         BeginProperty Button7 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Caption         =   "Clean Up"
            Key             =   "Clean Up"
            Object.Tag             =   ""
            ImageIndex      =   6
         EndProperty
         BeginProperty Button8 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Key             =   "Search Box"
            Object.Tag             =   ""
            Style           =   4
            Object.Width           =   2234
            MixedState      =   -1  'True
         EndProperty
         BeginProperty Button9 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Enabled         =   0   'False
            Caption         =   "Search"
            Key             =   "Do Search"
            Object.Tag             =   ""
         EndProperty
         BeginProperty Button10 {0713F354-850A-101B-AFC0-4210102A8DA7} 
            Enabled         =   0   'False
            Key             =   ""
            Object.Tag             =   ""
         EndProperty
      EndProperty
      Begin VB.PictureBox picShadow 
         BorderStyle     =   0  'None
         Height          =   50
         Left            =   0
         ScaleHeight     =   45
         ScaleWidth      =   4395
         TabIndex        =   9
         Top             =   560
         Width           =   4395
         Begin VB.Image imgShadow 
            Height          =   45
            Left            =   0
            Picture         =   "frmMain.frx":000C
            Stretch         =   -1  'True
            Top             =   0
            Width           =   9990
         End
      End
      Begin VB.PictureBox picSeperator 
         Appearance      =   0  'Flat
         BackColor       =   &H80000011&
         BorderStyle     =   0  'None
         ForeColor       =   &H80000008&
         Height          =   435
         Left            =   3240
         ScaleHeight     =   435
         ScaleWidth      =   15
         TabIndex        =   8
         Top             =   120
         Width           =   20
      End
      Begin VB.TextBox txtSearch 
         Enabled         =   0   'False
         Height          =   315
         Left            =   6540
         TabIndex        =   7
         Text            =   "Search..."
         Top             =   120
         Width           =   2235
      End
   End
   Begin ComctlLib.ListView lstDownloads 
      Height          =   1935
      Left            =   3150
      TabIndex        =   0
      Top             =   4680
      Width           =   8175
      _ExtentX        =   14420
      _ExtentY        =   3413
      View            =   3
      LabelEdit       =   1
      LabelWrap       =   -1  'True
      HideSelection   =   0   'False
      _Version        =   327682
      ForeColor       =   -2147483640
      BackColor       =   -2147483643
      Appearance      =   0
      BeginProperty Font {0BE35203-8F91-11CE-9DE3-00AA004BB851} 
         Name            =   "Tahoma"
         Size            =   8.25
         Charset         =   0
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      NumItems        =   0
   End
   Begin ComctlLib.ListView lstSubscribed 
      Height          =   1875
      Left            =   3150
      TabIndex        =   5
      Top             =   2700
      Width           =   8175
      _ExtentX        =   14420
      _ExtentY        =   3307
      View            =   3
      LabelEdit       =   1
      LabelWrap       =   -1  'True
      HideSelection   =   -1  'True
      _Version        =   327682
      ForeColor       =   -2147483640
      BackColor       =   -2147483643
      Appearance      =   0
      NumItems        =   0
   End
   Begin ComctlLib.ListView lstNew 
      Height          =   1335
      Left            =   3150
      TabIndex        =   4
      Top             =   600
      Width           =   8175
      _ExtentX        =   14420
      _ExtentY        =   2355
      View            =   3
      Arrange         =   2
      LabelEdit       =   1
      LabelWrap       =   -1  'True
      HideSelection   =   -1  'True
      _Version        =   327682
      ForeColor       =   -2147483640
      BackColor       =   -2147483643
      Appearance      =   0
      NumItems        =   0
   End
   Begin VB.Timer tmrCheckSub 
      Interval        =   60000
      Left            =   5520
      Top             =   2100
   End
   Begin VB.Timer tmrStartProcess 
      Interval        =   2000
      Left            =   5100
      Top             =   2100
   End
   Begin ComctlLib.StatusBar staStatus 
      Align           =   2  'Align Bottom
      Height          =   315
      Left            =   0
      TabIndex        =   3
      Top             =   6615
      Width           =   11355
      _ExtentX        =   20029
      _ExtentY        =   556
      Style           =   1
      SimpleText      =   ""
      _Version        =   327682
      BeginProperty Panels {0713E89E-850A-101B-AFC0-4210102A8DA7} 
         NumPanels       =   1
         BeginProperty Panel1 {0713E89F-850A-101B-AFC0-4210102A8DA7} 
            Key             =   ""
            Object.Tag             =   ""
         EndProperty
      EndProperty
   End
   Begin ComctlLib.ProgressBar prgItemProgress 
      Height          =   315
      Left            =   6120
      TabIndex        =   1
      Top             =   2100
      Visible         =   0   'False
      Width           =   1935
      _ExtentX        =   3413
      _ExtentY        =   556
      _Version        =   327682
      Appearance      =   1
   End
   Begin SHDocVwCtl.WebBrowser webDetails 
      Height          =   6015
      Left            =   0
      TabIndex        =   2
      Top             =   600
      Width           =   3150
      ExtentX         =   5556
      ExtentY         =   10610
      ViewMode        =   0
      Offline         =   0
      Silent          =   0
      RegisterAsBrowser=   0
      RegisterAsDropTarget=   1
      AutoArrange     =   0   'False
      NoClientEdge    =   0   'False
      AlignLeft       =   0   'False
      NoWebView       =   0   'False
      HideFileNames   =   0   'False
      SingleClick     =   0   'False
      SingleSelection =   0   'False
      NoFolders       =   0   'False
      Transparent     =   0   'False
      ViewID          =   "{0057D0E0-3573-11CF-AE69-08002B2E1262}"
      Location        =   "http:///"
   End
   Begin ComctlLib.ImageList imlToolbar 
      Left            =   4380
      Top             =   2100
      _ExtentX        =   1005
      _ExtentY        =   1005
      BackColor       =   -2147483643
      ImageWidth      =   16
      ImageHeight     =   16
      MaskColor       =   16777215
      UseMaskColor    =   0   'False
      _Version        =   327682
      BeginProperty Images {0713E8C2-850A-101B-AFC0-4210102A8DA7} 
         NumListImages   =   6
         BeginProperty ListImage1 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":0186
            Key             =   ""
         EndProperty
         BeginProperty ListImage2 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":04D8
            Key             =   ""
         EndProperty
         BeginProperty ListImage3 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":082A
            Key             =   ""
         EndProperty
         BeginProperty ListImage4 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":0B7C
            Key             =   ""
         EndProperty
         BeginProperty ListImage5 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":0ECE
            Key             =   ""
         EndProperty
         BeginProperty ListImage6 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":1220
            Key             =   ""
         EndProperty
      EndProperty
   End
   Begin ComctlLib.ImageList imlStations 
      Left            =   3780
      Top             =   2100
      _ExtentX        =   1005
      _ExtentY        =   1005
      BackColor       =   16777215
      ImageWidth      =   24
      ImageHeight     =   24
      MaskColor       =   16777215
      _Version        =   327682
      BeginProperty Images {0713E8C2-850A-101B-AFC0-4210102A8DA7} 
         NumListImages   =   6
         BeginProperty ListImage1 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":1572
            Key             =   "radio1"
         EndProperty
         BeginProperty ListImage2 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":1C84
            Key             =   "radio2"
         EndProperty
         BeginProperty ListImage3 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":2396
            Key             =   "radio3"
         EndProperty
         BeginProperty ListImage4 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":2AA8
            Key             =   "radio4"
         EndProperty
         BeginProperty ListImage5 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":31BA
            Key             =   "fivelive"
         EndProperty
         BeginProperty ListImage6 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":38CC
            Key             =   "6music"
         EndProperty
      EndProperty
   End
   Begin ComctlLib.ImageList imlListIcons 
      Left            =   3180
      Top             =   2100
      _ExtentX        =   1005
      _ExtentY        =   1005
      BackColor       =   -2147483643
      ImageWidth      =   16
      ImageHeight     =   16
      MaskColor       =   16777215
      _Version        =   327682
      BeginProperty Images {0713E8C2-850A-101B-AFC0-4210102A8DA7} 
         NumListImages   =   7
         BeginProperty ListImage1 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":3FDE
            Key             =   ""
         EndProperty
         BeginProperty ListImage2 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":4330
            Key             =   ""
         EndProperty
         BeginProperty ListImage3 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":4682
            Key             =   ""
         EndProperty
         BeginProperty ListImage4 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":49D4
            Key             =   ""
         EndProperty
         BeginProperty ListImage5 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":4D26
            Key             =   ""
         EndProperty
         BeginProperty ListImage6 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":5078
            Key             =   ""
         EndProperty
         BeginProperty ListImage7 {0713E8C3-850A-101B-AFC0-4210102A8DA7} 
            Picture         =   "frmMain.frx":53CA
            Key             =   ""
         EndProperty
      EndProperty
   End
   Begin VB.Menu File 
      Caption         =   "&File"
      Begin VB.Menu mnuFileExit 
         Caption         =   "E&xit"
      End
   End
   Begin VB.Menu mnuTools 
      Caption         =   "&Tools"
      Begin VB.Menu mnuToolsPrefs 
         Caption         =   "&Preferences"
      End
   End
   Begin VB.Menu mnuHelp 
      Caption         =   "&Help"
      Begin VB.Menu mnuHelpAbout 
         Caption         =   "&About"
      End
   End
   Begin VB.Menu mnuTray 
      Caption         =   "Tray"
      Visible         =   0   'False
      Begin VB.Menu mnuTrayShow 
         Caption         =   "&Show Radio Downloader"
      End
      Begin VB.Menu mnuTraySpacer 
         Caption         =   "-"
      End
      Begin VB.Menu mnuTrayExit 
         Caption         =   "E&xit"
      End
   End
End
Attribute VB_Name = "frmMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Implements WinSubHook2.iSubclass

Private lngStatus As Long

Private booLvAdding As Boolean

Private WithEvents clsExtender As clsWbExtender
Attribute clsExtender.VB_VarHelpID = -1
Private WithEvents clsBackground As clsBkgMain
Attribute clsBackground.VB_VarHelpID = -1
Private clsSubclass As cSubclass
Private clsProgData As clsProgData

Private lngLastState As Long

Const lngDLStatCol As Long = 2

Private Sub SetNewView(ByVal booStations As Boolean)
    lstNew.ListItems.Clear
    
    If booStations Then
        lstNew.View = lvwIcon
    Else
        lstNew.View = lvwReport
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

Private Sub AddStation(strStationName As String, strStationId As String, strStationType As String)
    Dim lstAdd As ListItem
    
    Set lstAdd = lstNew.ListItems.Add
    lstAdd.Text = strStationName
    lstAdd.Tag = strStationType + "||" + strStationId
    lstAdd.Icon = imlStations.ListImages(strStationId).Index
End Sub

Private Sub clsBackground_Error(ByVal strError As String)
    Call clsProgData.SetStatus(clsBackground.ProgramID, clsBackground.ProgramDate, False, stError)
    Call clsProgData.UpdateDlList(lstDownloads)
    
    Set clsBackground = Nothing
    tmrStartProcess.Enabled = True
End Sub

Private Sub clsBackground_Finished()
    Call clsProgData.AdvanceNextAction(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate)
    
    If clsProgData.GetNextActionVal(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate) = NextAction.None Then
        ' All done, set status to completed, and save file path
        Call clsProgData.SetStatus(clsBackground.ProgramID, clsBackground.ProgramDate, False, stCompleted)
        Call clsProgData.SetDownloadPath(clsBackground.ProgramType, clsBackground.ProgramID, clsBackground.ProgramDate, clsBackground.FinalName)
    Else
        Call clsProgData.SetStatus(clsBackground.ProgramID, clsBackground.ProgramDate, False, stWaiting)
    End If
    
    Call clsProgData.UpdateDlList(lstDownloads)
    
    Set clsBackground = Nothing
    tmrStartProcess.Enabled = True
End Sub

Private Sub clsBackground_Progress(ByVal lngPercent As Long)
    Dim lstChangeItem As ListItem
    Set lstChangeItem = lstDownloads.FindItem(Format(clsBackground.ProgramDate) + "||" + clsBackground.ProgramID, lvwTag)
    
    lstChangeItem.SubItems(3) = lngPercent
End Sub

Private Sub clsExtender_GetExternal(oIDispatch As Object)
    'this allows javascript to access the objects we return
    'here is it set so javascript will have access to all functions
    'and objects on this form.
    Set oIDispatch = Me
End Sub

Private Sub Form_Load()
    If App.PrevInstance Then
        Unload Me
        Exit Sub
    End If
    
    lstSubscribed.Top = lstNew.Top
    lstDownloads.Top = lstNew.Top
    
    Call lstNew.ColumnHeaders.Add(1, , "Program Name", 5500)
    
    Call lstSubscribed.ColumnHeaders.Add(1, , "Program Name", 5500)
    
    Call lstDownloads.ColumnHeaders.Add(1, , "Name", 2250)
    Call lstDownloads.ColumnHeaders.Add(2, , "Date", 750)
    Call lstDownloads.ColumnHeaders.Add(3, , "Status", 1250)
    Call lstDownloads.ColumnHeaders.Add(4, , "Progress", 1000)
    
    lstNew.Icons = imlStations
    lstNew.SmallIcons = imlListIcons
    lstSubscribed.SmallIcons = imlListIcons
    lstDownloads.SmallIcons = imlListIcons
    
    Call SetParent(prgItemProgress.hWnd, lstDownloads.hWnd)
    
    Call AddStations
    Call SetupToolbar
    Call TabAdjustments
    Call AddToSystray(Me)
    
    Set clsExtender = New clsWbExtender
    clsExtender.HookWebBrowser webDetails
    
    Set clsSubclass = New cSubclass
    Call clsSubclass.Subclass(Me.hWnd, Me)
    
    ' When the listview is scrolled (mouse or keyboard) or the columns are
    ' resized, the scrollbars need to be moved too
    'Call clsSubclass.AddMsg(WM_HSCROLL, MSG_AFTER)
    'Call clsSubclass.AddMsg(WM_VSCROLL, MSG_AFTER)
    'Call clsSubclass.AddMsg(WM_KEYDOWN, MSG_AFTER)
    'Call clsSubclass.AddMsg(WM_NOTIFY, MSG_AFTER)
    'Call clsSubclass.AddMsg(WM_STYLECHANGING, MSG_BEFORE)
    Call clsSubclass.AddMsg(TRAY_CALLBACK, MSG_BEFORE)
    Call clsSubclass.AddMsg(WM_NOTIFY, MSG_BEFORE)
    
    Set clsProgData = New clsProgData
    Call clsProgData.CleanupUnfinished
    Call clsProgData.UpdateDlList(lstDownloads)
    Call clsProgData.UpdateSubscrList(lstSubscribed)
    
    'picBack.BackColor = Me.BackColor
    'picForward.BackColor = Me.BackColor
    'picCrumbArrow.BackColor = vbWhite
    
    'Call DrawTransp("NAV_BACK", picForward, 29, 0, 0, 0)
    'Call DrawTransp("NAV_DISABLED", picForward, 27, 0, 0, 0)
    'Call DrawTransp("NAV_BACK", picBack, 0, 0, 0, 0)
    'Call DrawTransp("NAV_DISABLED", picBack, 0, 0, 1, 0)
    'Call DrawTransp("CRUMB_ARROW", picCrumbArrow, 0, 0, 0, 0)
    
    Call SetIcon(Me.hWnd, "appicon", True)
    
    DoEvents
    
    Me.Show
    
    If PathIsDirectory(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(App.Path) + "Downloads")) = False Then
        Call frmPreferences.Show(vbModal)
    End If
End Sub

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)
    If UnloadMode = vbFormControlMenu Then
        Call TrayAnimate(Me, True)
        Me.Visible = False
        Cancel = True
    End If
End Sub

Private Sub Form_Resize()
    If Me.WindowState <> vbMinimized Then
        lngLastState = Me.WindowState
    End If
    
    Static lngLastHeight As Long
    
    If staStatus.Top - (lstNew.Top) < 10 * Screen.TwipsPerPixelY Then
        If Me.Height < lngLastHeight Then
            Me.Height = lngLastHeight
            
            ' Nothing to do with webdetails, just a hack to flip the user out
            ' of window resizing mode - webdetails being shown does just that
            webDetails.Visible = False
            webDetails.Visible = True
        End If
    End If
    
    lngLastHeight = Me.Height
    
    'WebBrowser
    webDetails.Height = staStatus.Top - webDetails.Top
    
    'Listviews
    lstDownloads.Height = staStatus.Top - lstDownloads.Top
    lstNew.Height = lstDownloads.Height
    lstSubscribed.Height = lstDownloads.Height
    lstDownloads.Width = Me.ScaleWidth - webDetails.Width
    lstNew.Width = lstDownloads.Width
    lstSubscribed.Width = lstDownloads.Width
    
    'Search box in toolbar
    txtSearch.Left = tbrToolbar.Buttons("Search Box").Left
    txtSearch.Top = (tbrToolbar.Buttons("Search Box").Height - txtSearch.Height) / 2
    
    'Toolbar seperator
    picSeperator.Top = tbrToolbar.Buttons("-").Top + 2 * Screen.TwipsPerPixelX
    picSeperator.Left = tbrToolbar.Buttons("-").Left + (tbrToolbar.Buttons("-").Width / 2)
    picSeperator.Height = tbrToolbar.Buttons("-").Height - 4 * Screen.TwipsPerPixelX
    
    'Toolbar shadow
    picShadow.Width = tbrToolbar.Width
    imgShadow.Width = tbrToolbar.Width
End Sub

Private Sub Form_Unload(Cancel As Integer)
    Set clsBackground = Nothing
    
    Call RemoveFromSystray
    
    On Error Resume Next
    Call Kill(AddSlash(App.Path) + "temp.htm")
    Call Kill(AddSlash(App.Path) + "temp\*.*")
End Sub

Private Sub iSubclass_Proc(ByVal bBefore As Boolean, bHandled As Boolean, lReturn As Long, hWnd As Long, uMsg As WinSubHook2.eMsg, wParam As Long, lParam As Long)
    Select Case uMsg
'        '---------------------------------------------------------------
'        ' For scrollbar adjustment
'        '---------------------------------------------------------------
'        Case WM_VSCROLL, WM_HSCROLL
'            Call MoveBars
'        Case WM_KEYDOWN
'            Debug.Print wParam
'            'If key is up, down, left, right, pgup or pgdown then move the bars
'            If wParam >= 33 And wParam <= 40 Then
'                Call MoveBars
'            End If
'        Case WM_NOTIFY
'            ' Find out what the message was
'            Dim uWMNOTIFY_Message As NMHDR
'            Call CopyMemory(uWMNOTIFY_Message, ByVal lParam, Len(uWMNOTIFY_Message))
'
'            If uWMNOTIFY_Message.code = HDN_ENDTRACK Then
'                ' User adjusted column width
'                Call MoveBars
'            End If
'        '---------------------------------------------------------------
'        ' To deal with flickering when adding / changing info in listview
'        '---------------------------------------------------------------
'        Case WM_STYLECHANGING
'            ' Only stop redraw when requested
'            If booLvAdding Then
'                ' If the flag is set and the ListView's LVS_NOLABELWRAP style
'                ' bit is changing (the ListView's LabelWrap property), prevent
'                ' the style change and the resultant unnecessary redrawing,
'                ' flickering, and a serious degradation in loading speed.
'
'                Dim ss As STYLESTRUCT
'                CopyMemory ss, ByVal lParam, Len(ss)
'
'                If ((ss.styleOld Xor ss.styleNew) And LVS_NOLABELWRAP) Then
'                    ss.styleNew = ss.styleOld
'                    CopyMemory ByVal lParam, ss, Len(ss)
'                End If
'            End If
        Case TRAY_CALLBACK
            Select Case lParam
                Case WM_LBUTTONDBLCLK
                    Call mnuTrayShow_Click
                Case WM_RBUTTONUP
                    Call PopupMenu(mnuTray, , , , mnuTrayShow)
            End Select
        Case WM_NOTIFY
            ' Fill the NMHDR struct from the lParam pointer.
            ' (for any WM_NOTIFY msg, lParam always points to a struct which is
            ' either the NMHDR struct, or whose 1st member is the NMHDR struct)
            Dim nmh As NMHDR
            Call CopyMemory(nmh, ByVal lParam, Len(nmh))
    
            Select Case nmh.code
                Case LVN_BEGINDRAG
                    'Notifies a list view control's parent window that a
                    'drag-and-drop operation involving the left mouse button
                    'is being initiated.
                    bHandled = True
            End Select
    End Select
End Sub

Private Sub lstDownloads_ItemClick(ByVal Item As ComctlLib.ListItem)
    Dim strSplit() As String
    strSplit = Split(Item.Tag, "||")
    
    Const strTitle As String = "Download Info"
    
    If clsProgData.DownloadStatus(strSplit(2), strSplit(1), CDate(strSplit(0))) = stCompleted Then
        If PathFileExists(clsProgData.GetDownloadPath(strSplit(2), strSplit(1), CDate(strSplit(0)))) Then
            Call CreateHtml(strTitle, clsProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "Play")
        Else
            Call CreateHtml(strTitle, clsProgData.ProgramHTML(strSplit(2), strSplit(1), CDate(strSplit(0))), "")
        End If
    ElseIf clsProgData.DownloadStatus(strSplit(2), strSplit(1), CDate(strSplit(0))) = stError Then
        Call CreateHtml(strTitle, clsProgData.ProgramHTML(strSplit(1), CDate(strSplit(0))), "Retry,Cancel")
    Else
        Call CreateHtml(strTitle, clsProgData.ProgramHTML(strSplit(1), CDate(strSplit(0))), "Cancel")
    End If
End Sub

Private Sub lstNew_DblClick()
    Dim strSplit() As String
    strSplit = Split(lstNew.SelectedItem.Tag, "||")
    
    If lstNew.View = lvwIcon Then
        If strSplit(0) <> "BBCLA" Then Stop
        
        lstNew.View = lvwReport
        tbrToolbar.Buttons("Up").Enabled = True
        
        Call CreateHtml(lstNew.SelectedItem.Text, "", None)
        Call ListviewStartAdd
        Call ListStation(strSplit(1), lstNew)
        Call ListviewEndAdd
    Else
        ' Do nothing
    End If
End Sub

Private Sub lstNew_ItemClick(ByVal Item As ComctlLib.ListItem)
    Dim strSplit() As String
    strSplit = Split(Item.Tag, "||")
    
    If lstNew.View = lvwIcon Then
        ' Do nothing
    Else
        Call CreateHtml("Program Info", clsProgData.ProgramHTML(strSplit(0), strSplit(1)), "Download,Subscribe")
    End If
End Sub

Private Sub TabAdjustments()
    If tbrToolbar.Buttons("Find New").Value = tbrPressed Then
        lstNew.Visible = True
        lstSubscribed.Visible = False
        lstDownloads.Visible = False
        'tbrToolbar.Buttons("Clean Up").Enabled = False
        'tbrToolbar.Buttons("Up").Enabled = lstNew.View = lvwReport
        Call CreateHtml("Choose New Program", "<p>This view allows you to browse all of the programs that are available for you to download or subscribe to.</p><p>Select a station icon to show the programs available from it.</p>", None)
    ElseIf tbrToolbar.Buttons("Subscriptions").Value = tbrPressed Then
        lstNew.Visible = False
        lstSubscribed.Visible = True
        lstDownloads.Visible = False
        'tbrToolbar.Buttons("Clean Up").Enabled = False
        'tbrToolbar.Buttons("Up").Enabled = False
        Call CreateHtml("Subscribed Programs", "", None)
    ElseIf tbrToolbar.Buttons("Downloads").Value = tbrPressed Then
        lstNew.Visible = False
        lstSubscribed.Visible = False
        lstDownloads.Visible = True
        'tbrToolbar.Buttons("Clean Up").Enabled = True
        'tbrToolbar.Buttons("Up").Enabled = False
        Call CreateHtml("Program Downloads", "<p>Here you can see programs that are being downloaded, or have been downloaded already.</p>", None)
    End If
End Sub

Private Sub CreateHtml(ByVal strTitle As String, ByVal strMiddleContent As String, ByVal strBottomLinks As String)
    Dim strHtml As String
    
    Const strHtmlBBCInfoStyles As String = " #show big { font-weight: bold; font-size: 10pt; }  #show img { margin-right: 10px; } #showtitle { padding-bottom: 10px; } .txinfo { color:#666; font-weight:normal; font-size: 8pt; } "
    Const strHtmlStyles As String = "body { background-color: #3F3F3F; font: 10pt tahoma; } html, body, table { height: 100%; margin: 0px; } h1 { font-size: 12pt; margin-bottom: 8px; } table { width: 100%; border-collapse: collapse; } td { vertical-align: top; margin: 0px; } .bottomrow { vertical-align: bottom; }" + strHtmlBBCInfoStyles
    Const strHtmlStart As String = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html><head><style type=""text/css"">" + strHtmlStyles + "</style><script type=""text/javascript"">function handleError() { return true; } window.onerror = handleError;</script></head><body><table><tr><td>"
    Const strHtmlEnd As String = "</td></tr></table></body></html>"
    
    strHtml = strHtmlStart + "<h1>" + strTitle + "</h1>" + strMiddleContent + "</td></tr><tr><td class=""bottomrow"">"
    
    Dim strSplitLinks() As String
    Dim strLoopLinks
    Dim strBuiltLinks As String
    strSplitLinks = Split(strBottomLinks, ",")
    
    For Each strLoopLinks In strSplitLinks
        Select Case strLoopLinks
            Case "Download":
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Download Now", "FlDownload", "Download this program now"))
            Case "Subscribe":
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Subscribe", "FlSubscribe", "Get this program downloaded regularly"))
            Case "Unsubscribe":
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Unsubscribe", "FlUnsubscribe", "Stop getting this program downloaded regularly"))
            Case "Play"
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Play", "FlPlay", "Play this program"))
            Case "Cancel"
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Cancel", "FlCancel", "Cancel downloading this program"))
            Case "Retry"
                strBuiltLinks = AddLink(strBuiltLinks, BuildLink("Retry", "FlRetry", "Try downloading this program again"))
        End Select
    Next strLoopLinks
    
    strHtml = strHtml + strBuiltLinks
    strHtml = strHtml + strHtmlEnd
    
    Call ShowHtml(strHtml)
End Sub

Private Function BuildLink(strTitle As String, strCall As String, strStatusText As String) As String
    BuildLink = "<a href=""#"" onclick=""window.external." + strCall + "(); return false;"" onmouseover=""window.external.FlStatusText('" + strStatusText + "'); return true;"" onmouseout=""window.external.FlStatusText(''); return true;"">" + strTitle + "</a>"
End Function

Private Function AddLink(ByVal strCurrentLinks As String, ByVal strNewlink As String) As String
    If Len(strCurrentLinks) > 0 Then
        AddLink = strCurrentLinks + " | "
    End If
    
    AddLink = AddLink + strNewlink
End Function

Private Sub ShowHtml(strHtml As String)
    Dim lngFileNo As Long
    lngFileNo = FreeFile
    
    Open AddSlash(App.Path) + "temp.htm" For Output As lngFileNo
    Print #lngFileNo, strHtml
    Close lngFileNo
    
    webDetails.Navigate2 AddSlash(App.Path) + "temp.htm"
End Sub

Private Sub lstSubscribed_ItemClick(ByVal Item As ComctlLib.ListItem)
    Dim strSplit() As String
    strSplit = Split(Item.Tag, "||")
    
    Call CreateHtml("Subscribed Program", clsProgData.ProgramHTML(strSplit(0), strSplit(1)), "Unsubscribe")
End Sub

Private Sub mnuFileExit_Click()
    Call mnuTrayExit_Click
End Sub

Private Sub mnuToolsPrefs_Click()
    Call frmPreferences.Show(vbModal)
End Sub

Private Sub mnuTrayExit_Click()
    Unload Me
End Sub

Private Sub mnuTrayShow_Click()
    If Me.WindowState = vbMinimized Then
        Me.WindowState = lngLastState
    End If
    
    If Me.Visible = False Then
        Call TrayAnimate(Me, False)
        Me.Visible = True
    End If
End Sub

Private Sub tbrToolbar_ButtonClick(ByVal Button As ComctlLib.Button)
    If Button.Key = "Find New" Or Button.Key = "Subscriptions" Or Button.Key = "Downloads" Then
        Call TabAdjustments
    End If
End Sub

'Private Sub tbrToolbar_ButtonClick(ByVal lButton As Long)
'    Select Case tbrToolbar.ButtonKey(lButton)
'        Case "Up"
'            tbrToolbar.ButtonEnabled(lButton) = False
'            If lstNew.View = lvwReport Then
'                Call AddStations
'            End If
'    End Select
'End Sub

Private Sub tmrCheckSub_Timer()
    Call clsProgData.CheckSubscriptions(lstDownloads, tmrStartProcess)
End Sub

Private Sub tmrStartProcess_Timer()
    ' Make sure that it isn't currently working
    If IsNothing(clsBackground) Then
        Dim dldAction As DownloadAction
        dldAction = clsProgData.FindNextAction
        
        If dldAction.booFound Then
            Set clsBackground = New clsBkgMain
            Call clsProgData.SetStatus(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate, True)
            Call clsBackground.Start(dldAction.nxtNextAction, dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, clsProgData.ProgramDuration(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate), dldAction.dldDownloadID.dteDate, clsProgData.ProgramTitle(dldAction.dldDownloadID.strProgramType, dldAction.dldDownloadID.strProgramID, dldAction.dldDownloadID.dteDate))
            Call clsProgData.UpdateDlList(lstDownloads)
        End If
    End If
    
    tmrStartProcess.Enabled = False
End Sub

Private Sub webDetails_NavigateComplete2(ByVal pDisp As Object, url As Variant)
    ' Take away scrollbars, border, and interaction
    clsExtender.WbAttributes = haNoScrollBars Or haNo3DBorder Or haDisableSelections
    webDetails.Refresh
End Sub

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

Private Sub ListviewStartAdd()
    booLvAdding = True
End Sub

Private Sub ListviewEndAdd()
    booLvAdding = False

    DoEvents

    'Add the contents of the listview to the update region
    Call InvalidateRect(lstDownloads.hWnd, ByVal 0&, 0)
    
    'Then loop round and remove the areas where progressbars are to
    'prevent flickering as they are blanked out and then redrawn
    
'    Dim prgBar As ProgressBar
'
'    For Each prgBar In prgItemProgress
'        If prgBar.Index > 0 Then
'            'Only process visible progressbars to be most efficient
'            If prgBar.Visible = True Then
'                Dim recPos As RECT
'
'                Call GetSubItemRect(lstDownloads.hwnd, prgBar.Index - 1, 2, LVIR_LABEL, recPos)
'                Call ValidateRect(lstDownloads.hwnd, recPos)
'            End If
'        End If
'    Next prgBar
End Sub

Private Sub SetupToolbar()
'    With tbrToolbar
'        Call .CreateToolbar(24, , True, False)
'        .ImageSource = CTBExternalImageList
'        Call .SetImageList(imlToolbar)
'        '.buttonte
'        Call .AddButton(, 0, , , "Test", CTBNormal, "test")
'    End With
'
'    With rbrRebar
'        Call .CreateRebar(Me.hWnd)
'        Call .AddBandByHwnd(tbrToolbar.hWnd)
'    End With
End Sub

' Called from JavaScript in embedded XHTML -------------------------------------

Public Sub FlDownload()
    Dim strSplit() As String
    strSplit = Split(lstDownloads.SelectedItem.Tag, "||")
    
    If clsProgData.IsDownloading(strSplit(0), strSplit(1)) Then
        staStatus.SimpleText = ""
        Call MsgBox("You cannot download this program more than once at the same time!", vbExclamation, "Radio Downloader")
        Exit Sub
    End If
    
    If clsProgData.AddDownload(strSplit(0), strSplit(1)) = False Then
        staStatus.SimpleText = ""
        Call MsgBox("You have already downloaded this program!", vbExclamation, "Radio Downloader")
    Else
        'Set tabMain.SelectedItem = tabMain.Tabs(3)
        Call clsProgData.UpdateDlList(lstDownloads)
        tmrStartProcess.Enabled = True
    End If
End Sub

Public Sub FlPlay()
    Dim strSplit() As String
    strSplit = Split(lstDownloads.SelectedItem.Tag, "||")
    
    Call ShellExecute(Me.hWnd, "open", clsProgData.GetDownloadPath(strSplit(2), strSplit(1), strSplit(0)), 0&, 0&, SW_SHOWNORMAL)
End Sub

Public Sub FlSubscribe()
    Dim strSplit() As String
    strSplit = Split(lstNew.SelectedItem.Tag, "||")
    
    If clsProgData.AddSubscription(strSplit(0), strSplit(1)) = False Then
        staStatus.SimpleText = ""
        Call MsgBox("You are already subscribed to this program!", vbExclamation, "Radio Downloader")
    Else
        'Set tabMain.SelectedItem = tabMain.Tabs(2)
        Call clsProgData.UpdateSubscrList(lstSubscribed)
    End If
End Sub

Public Sub FlUnsubscribe()
    Dim strSplit() As String
    strSplit = Split(lstSubscribed.SelectedItem.Tag, "||")

    staStatus.SimpleText = ""
    
    If MsgBox("Are you sure that you would like to stop having this program downloaded regularly?", vbQuestion + vbYesNo, "Radio Downloader") = vbYes Then
        Call clsProgData.RemoveSubscription(strSplit(0), strSplit(1))
        Call clsProgData.UpdateSubscrList(lstSubscribed)
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
    
    If MsgBox("Are you sure that you would like to stop downloading this program?", vbQuestion + vbYesNo, "Radio Downloader") = vbYes Then
        Dim strSplit() As String
        strSplit = Split(lstDownloads.SelectedItem.Tag, "||")
        
        If clsProgData.IsDownloading(strSplit(2), strSplit(1)) Then
            Set clsBackground = Nothing
        End If
        
        Call clsProgData.CancelDownload(strSplit(2), strSplit(1), strSplit(0))
        Call clsProgData.UpdateDlList(lstDownloads)
    End If
End Sub

Public Sub FlRetry()
    Dim strSplit() As String
    strSplit = Split(lstDownloads.SelectedItem.Tag, "||")
    
    Call clsProgData.ResetDownload(strSplit(2), strSplit(1), strSplit(0), False)
    Call clsProgData.UpdateDlList(lstDownloads)
    tmrStartProcess.Enabled = True
End Sub

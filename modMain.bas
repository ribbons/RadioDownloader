Attribute VB_Name = "modMain"
Option Explicit

Public Type RECT
    Left    As Long
    Top     As Long
    Right   As Long
    Bottom  As Long
End Type

Public Type NMHDR
    hwndFrom    As Long        ' Window handle of control that sends the message
    idFrom      As Long        ' Identifier of control that sends the message
    code        As Long        ' Notification code
End Type

Public Type STYLESTRUCT
  styleOld  As Long
  styleNew  As Long
End Type

Public Type BrowseInfo
    lngHwnd        As Long
    pIDLRoot       As Long
    pszDisplayName As Long
    lpszTitle      As Long
    ulFlags        As Long
    lpfnCallback   As Long
    lParam         As Long
    iImage         As Long
End Type

Private Type NOTIFYICONDATA
    cbSize As Long
    hWnd As Long
    uID As Long
    uFlags As Long
    uCallbackMessage As Long
    hIcon As Long
    szTip As String * 64
End Type

Public Const LVIR_LABEL As Long = 2
Public Const LVM_FIRST As Long = &H1000
Public Const LVM_GETSUBITEMRECT As Long = (LVM_FIRST + 56)
Public Const HDN_FIRST As Long = (0 - 300)
Public Const HDN_ENDTRACK As Long = (HDN_FIRST - 1)
Public Const LVS_NOLABELWRAP = &H80
Public Const SW_SHOWNORMAL = 1
Public Const BIF_RETURNONLYFSDIRS = 1
Public Const MAX_PATH = 260
Public Const BIF_NEWDIALOGSTYLE = 64
Public Const NIM_ADD = &H0    ' Add an icon
Public Const NIM_MODIFY = &H1 ' Modify an icon
Public Const NIM_DELETE = &H2 ' Delete an icon
Private Const NIF_MESSAGE = &H1       ' To change uCallBackMessage member
Private Const NIF_ICON = &H2          ' To change the icon
Private Const NIF_TIP = &H4           ' To change the tooltip text
Private Const WM_USER = &H400&
Public Const TRAY_CALLBACK = (WM_USER + 101&)
Private Const GW_CHILD = 5
Private Const GW_HWNDNEXT = 2
Private Const IDANI_OPEN = &H1
Private Const IDANI_CLOSE = &H2
Private Const IDANI_CAPTION = &H3
Public Const LVN_FIRST = -100&
Public Const LVN_BEGINDRAG = (LVN_FIRST - 9)

Public Declare Function SetParent Lib "user32" (ByVal hWndChild As Long, ByVal hWndNewParent As Long) As Long
Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As Any) As Long
Public Declare Function InvalidateRect Lib "user32" (ByVal hWnd As Long, lpRect As Any, ByVal bErase As Long) As Long
Public Declare Function ValidateRect Lib "user32" (ByVal hWnd As Long, lpRect As Any) As Long
Public Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (ByVal hWnd As Long, ByVal lpOperation As String, ByVal lpFile As String, ByVal lpParameters As String, ByVal lpDirectory As String, ByVal nShowCmd As Long) As Long
Public Declare Sub CoTaskMemFree Lib "ole32.dll" (ByVal hMem As Long)
Public Declare Function lstrcat Lib "kernel32" Alias "lstrcatA" (ByVal lpString1 As String, ByVal lpString2 As String) As Long
Public Declare Function SHBrowseForFolder Lib "shell32" (lpbi As BrowseInfo) As Long
Public Declare Function SHGetPathFromIDList Lib "shell32" (ByVal pidList As Long, ByVal lpBuffer As String) As Long
Public Declare Function PathFileExists Lib "shlwapi.dll" Alias "PathFileExistsA" (ByVal pszPath As String) As Long
Public Declare Function PathIsDirectory Lib "shlwapi.dll" Alias "PathIsDirectoryA" (ByVal pszPath As String) As Long
Public Declare Function Shell_NotifyIcon Lib "shell32" Alias "Shell_NotifyIconA" (ByVal dwMessage As Long, pnid As NOTIFYICONDATA) As Boolean
Public Declare Function DrawAnimatedRects Lib "user32" (ByVal hWnd As Long, ByVal idAni As Long, lprcFrom As RECT, lprcTo As RECT) As Long
Private Declare Function SetRect Lib "user32" (lpRect As RECT, ByVal X1 As Long, ByVal Y1 As Long, ByVal X2 As Long, ByVal Y2 As Long) As Long
Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Long
Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Long, ByVal wCmd As Long) As Long
Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As Long, ByVal lpClassName As String, ByVal nMaxCount As Long) As Long
Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As Long, lpRect As RECT) As Long
Public Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (ByVal lpExistingFileName As String, ByVal lpNewFileName As String) As Long

Private strBaseFolder As String
Private nid As NOTIFYICONDATA

Public Sub AddToSystray(frmForm As Form)
    nid.cbSize = Len(nid)
    nid.hWnd = frmForm.hWnd
    nid.uID = 0
    nid.uFlags = NIF_ICON Or NIF_TIP Or NIF_MESSAGE
    nid.uCallbackMessage = TRAY_CALLBACK
    nid.hIcon = SystrayIcon("appicon")
    
    nid.szTip = "Radio Downloader" + vbNullChar
    Shell_NotifyIcon NIM_ADD, nid
End Sub

Public Sub RemoveFromSystray()
    Shell_NotifyIcon NIM_DELETE, nid
End Sub

Public Function GetSubItemRect(ByVal hWndLV As Long, ByVal iItem As Long, ByVal iSubItem As Long, ByVal code As Long, lpRect As RECT) As Boolean
    'Get the Coordinates of the ListItem specified with iITEM and iSubItem
    lpRect.Top = iSubItem
    lpRect.Left = code
    
    GetSubItemRect = SendMessage(hWndLV, LVM_GETSUBITEMRECT, ByVal iItem, lpRect)
End Function

Public Function AddSlash(ByVal strString As String) As String
    If Len(strString) Then
        If Right$(strString, 1) <> "\" Then
            If Right$(strString, 1) <> "/" Then
                strString = strString & "\"
            End If
        End If
    End If
    
    AddSlash = strString
End Function

Public Function BrowseForFolder(ByVal lngHwnd As Long, ByVal strPrompt As String, ByVal strStartIn As String) As String
    On Error GoTo ehBrowseForFolder 'Trap for errors

    Dim intNull As Integer
    Dim lngIDList As Long, lngResult As Long
    Dim strPath As String
    Dim udtBI As BrowseInfo
    Dim lpSelPath As Long
    
    strBaseFolder = AddSlash(strStartIn)
    
    'Set API properties (housed in a UDT)
    With udtBI
        .lngHwnd = lngHwnd
        .lpszTitle = lstrcat(strPrompt, "")
        .ulFlags = BIF_RETURNONLYFSDIRS + BIF_NEWDIALOGSTYLE
        .lpfnCallback = Address_Of(AddressOf BrowseCallbackProc)
    End With

    'Display the browse folder...
    lngIDList = SHBrowseForFolder(udtBI)

    If lngIDList <> 0 Then
        'Create string of nulls so it will fill in with the path
        strPath = String(MAX_PATH, 0)

        'Retrieves the path selected, places in the null
         'character filled string
        lngResult = SHGetPathFromIDList(lngIDList, strPath)

        'Frees memory
        Call CoTaskMemFree(lngIDList)

        'Find the first instance of a null character,
         'so we can get just the path
        intNull = InStr(strPath, vbNullChar)
        'Greater than 0 means the path exists...
        If intNull > 0 Then
            'Set the value
            strPath = Left(strPath, intNull - 1)
        End If
    End If

    'Return the path name
    BrowseForFolder = strPath
    Exit Function 'Abort

ehBrowseForFolder:

    'Return no value
    BrowseForFolder = Empty
End Function

Private Function Address_Of(ByVal n As Long) As Long
   Address_Of = n
   End Function

Private Function BrowseCallbackProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal lParam As Long, ByVal lpData As Long) As Long
   Const WM_USER = &H400&
   Const BFFM_INITIALIZED = 1
   Const BFFM_SETSELECTIONA = (WM_USER + 102)
   
   Dim default_path() As Byte
      
   If uMsg = BFFM_INITIALIZED Then
      default_path = StrConv(strBaseFolder, vbFromUnicode) + vbNullString
      SendMessage hWnd, BFFM_SETSELECTIONA, 1&, ByVal VarPtr(default_path(0))
    End If
End Function

Public Function IsNothing(pvar As Variant) As Boolean
   On Error Resume Next
   IsNothing = (pvar Is Nothing)
   Err.Clear
   On Error GoTo 0
End Function

Public Sub TrayAnimate(frmForm As Form, booDown As Boolean)
    Dim rctWindow As RECT
    Dim rctSystemTray As RECT
    
    rctSystemTray = GetSysTrayPos
    Call GetWindowRect(frmForm.hWnd, rctWindow)
    
    If booDown = True Then
        DrawAnimatedRects frmForm.hWnd, IDANI_CLOSE Or IDANI_CAPTION, rctWindow, rctSystemTray
    Else
        DrawAnimatedRects frmForm.hWnd, IDANI_OPEN Or IDANI_CAPTION, rctSystemTray, rctWindow
    End If
End Sub

Private Function GetSysTrayPos() As RECT
    Dim lngTaskbarHwnd As Long
    Dim lngTrayHwnd As Long
    Dim strClassName As String * 250
    
    'Get taskbar handle
    lngTaskbarHwnd = FindWindow("Shell_traywnd", vbNullString)
    
    'Get system tray handle
    lngTrayHwnd = GetWindow(lngTaskbarHwnd, GW_CHILD)
    Do
        GetClassName lngTrayHwnd, strClassName, 250
        If TrimNull(strClassName) = "TrayNotifyWnd" Then Exit Do
        lngTrayHwnd = GetWindow(lngTrayHwnd, GW_HWNDNEXT)
    Loop
    
    Call GetWindowRect(lngTrayHwnd, GetSysTrayPos)
End Function

Public Function TrimNull(ByVal strString As String) As String
    Dim lngPos As Long
    lngPos = InStr(strString, Chr$(0))
    
    If lngPos Then strString = Left$(strString, lngPos - 1)
    TrimNull = strString
End Function

'Return whether we're running in the IDE. Public for general utility purposes
Public Function InIDE() As Boolean
  Debug.Assert SetTrue(InIDE)
End Function

'Worker function for InIDE - will only be called whilst running in the IDE
Private Function SetTrue(bValue As Boolean) As Boolean
  SetTrue = True
  bValue = True
End Function

Option Strict Off
Option Explicit On

Module modMain
    Public Structure RECT
        'UPGRADE_NOTE: Left was upgraded to Left_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim Left_Renamed As Integer
        Dim Top As Integer
        'UPGRADE_NOTE: Right was upgraded to Right_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim Right_Renamed As Integer
        Dim Bottom As Integer
    End Structure

    Public Structure NMHDR
        Dim hwndFrom As Integer ' Window handle of control that sends the message
        Dim idFrom As Integer ' Identifier of control that sends the message
        Dim code As Integer ' Notification code
    End Structure

    Public Structure STYLESTRUCT
        Dim styleOld As Integer
        Dim styleNew As Integer
    End Structure

    Public Structure BrowseInfo
        Dim lngHwnd As Integer
        Dim pIDLRoot As Integer
        Dim pszDisplayName As Integer
        Dim lpszTitle As Integer
        Dim ulFlags As Integer
        Dim lpfnCallback As Integer
        Dim lParam As Integer
        Dim iImage As Integer
    End Structure

    Public Const LVIR_LABEL As Integer = 2
    Public Const LVM_FIRST As Integer = &H1000S
    Public Const LVM_GETSUBITEMRECT As Integer = (LVM_FIRST + 56)
    Public Const HDN_FIRST As Integer = (0 - 300)
    Public Const HDN_ENDTRACK As Integer = (HDN_FIRST - 1)
    Public Const LVS_NOLABELWRAP As Short = &H80S
    Public Const SW_SHOWNORMAL As Short = 1
    Public Const BIF_RETURNONLYFSDIRS As Short = 1
    Public Const MAX_PATH As Short = 260
    Public Const BIF_NEWDIALOGSTYLE As Short = 64
    Public Const NIM_ADD As Short = &H0S ' Add an icon
    Public Const NIM_MODIFY As Short = &H1S ' Modify an icon
    Public Const NIM_DELETE As Short = &H2S ' Delete an icon
    Private Const NIF_MESSAGE As Short = &H1S ' To change uCallBackMessage member
    Private Const NIF_ICON As Short = &H2S ' To change the icon
    Private Const NIF_TIP As Short = &H4S ' To change the tooltip text
    Private Const WM_USER As Integer = &H400
    Public Const TRAY_CALLBACK As Decimal = (WM_USER + 101)
    Private Const GW_CHILD As Short = 5
    Private Const GW_HWNDNEXT As Short = 2
    Private Const IDANI_OPEN As Short = &H1S
    Private Const IDANI_CLOSE As Short = &H2S
    Private Const IDANI_CAPTION As Short = &H3S
    Public Const LVN_FIRST As Short = -100
    Public Const LVN_BEGINDRAG As Short = (LVN_FIRST - 9)

    'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
    'Public Declare Function SendMessage Lib "user32"  Alias "SendMessageA"(ByVal hWnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByRef lParam As Any) As Integer
    'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
    'Public Declare Function InvalidateRect Lib "user32" (ByVal hWnd As Integer, ByRef lpRect As Any, ByVal bErase As Integer) As Integer
    'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
    'Public Declare Function ValidateRect Lib "user32" (ByVal hWnd As Integer, ByRef lpRect As Any) As Integer
    Public Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (ByVal hWnd As Integer, ByVal lpOperation As String, ByVal lpFile As String, ByVal lpParameters As String, ByVal lpDirectory As String, ByVal nShowCmd As Integer) As Integer
    Public Declare Sub CoTaskMemFree Lib "ole32.dll" (ByVal hMem As Integer)
    Public Declare Function lstrcat Lib "kernel32" Alias "lstrcatA" (ByVal lpString1 As String, ByVal lpString2 As String) As Integer
    'UPGRADE_WARNING: Structure BrowseInfo may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    Public Declare Function SHBrowseForFolder Lib "shell32" (ByRef lpbi As BrowseInfo) As Integer
    Public Declare Function SHGetPathFromIDList Lib "shell32" (ByVal pidList As Integer, ByVal lpBuffer As String) As Integer
    Public Declare Function PathIsDirectory Lib "shlwapi.dll" Alias "PathIsDirectoryA" (ByVal pszPath As String) As Integer
    'UPGRADE_WARNING: Structure RECT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    'UPGRADE_WARNING: Structure RECT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    Public Declare Function DrawAnimatedRects Lib "user32" (ByVal hWnd As Integer, ByVal idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As Integer
    'UPGRADE_WARNING: Structure RECT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    Private Declare Function SetRect Lib "user32" (ByRef lpRect As RECT, ByVal X1 As Integer, ByVal Y1 As Integer, ByVal X2 As Integer, ByVal Y2 As Integer) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Private Declare Function GetWindow Lib "user32" (ByVal hWnd As Integer, ByVal wCmd As Integer) As Integer
    Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As Integer, ByVal lpClassName As String, ByVal nMaxCount As Integer) As Integer
    'UPGRADE_WARNING: Structure RECT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    Private Declare Function GetWindowRect Lib "user32" (ByVal hWnd As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (ByVal lpExistingFileName As String, ByVal lpNewFileName As String) As Integer

    Private strBaseFolder As String

    Public Function BrowseForFolder(ByVal lngHwnd As Integer, ByVal strPrompt As String, ByVal strStartIn As String) As String
        On Error GoTo ehBrowseForFolder 'Trap for errors

        Dim intNull As Short
        Dim lngIDList, lngResult As Integer
        Dim strPath As String
        Dim udtBI As BrowseInfo
        Dim lpSelPath As Integer

        strBaseFolder = strStartIn

        'Set API properties (housed in a UDT)
        With udtBI
            .lngHwnd = lngHwnd
            .lpszTitle = lstrcat(strPrompt, "")
            .ulFlags = BIF_RETURNONLYFSDIRS + BIF_NEWDIALOGSTYLE
            'UPGRADE_WARNING: Add a delegate for AddressOf BrowseCallbackProc Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="E9E157F7-EF0C-4016-87B7-7D7FBBC6EE08"'
            '.lpfnCallback = Address_Of(AddressOf BrowseCallbackProc)
        End With

        'Display the browse folder...
        lngIDList = SHBrowseForFolder(udtBI)

        If lngIDList <> 0 Then
            'Create string of nulls so it will fill in with the path
            strPath = New String(Chr(0), MAX_PATH)

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
        BrowseForFolder = CStr(Nothing)
    End Function

    Private Function Address_Of(ByVal n As Integer) As Integer
        Address_Of = n
    End Function

    Private Function BrowseCallbackProc(ByVal hWnd As Integer, ByVal uMsg As Integer, ByVal lParam As Integer, ByVal lpData As Integer) As Integer
        Const WM_USER As Integer = &H400
        Const BFFM_INITIALIZED As Short = 1
        Const BFFM_SETSELECTIONA As Decimal = (WM_USER + 102)

        Dim default_path() As Byte

        If uMsg = BFFM_INITIALIZED Then
            'UPGRADE_ISSUE: Constant vbFromUnicode was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
            'UPGRADE_TODO: Code was upgraded to use System.Text.UnicodeEncoding.Unicode.GetBytes() which may not have the same behavior. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="93DD716C-10E3-41BE-A4A8-3BA40157905B"'
            'default_path = System.Text.UnicodeEncoding.Unicode.GetBytes(StrConv(strBaseFolder, vbFromUnicode) & vbNullString)
            'UPGRADE_ISSUE: VarPtr function is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="367764E5-F3F8-4E43-AC3E-7FE0B5E074E2"'
            'SendMessage(hWnd, BFFM_SETSELECTIONA, 1, VarPtr(default_path(0)))
        End If
    End Function

    Public Sub TrayAnimate(ByRef frmForm As System.Windows.Forms.Form, ByRef booDown As Boolean)
        Dim rctWindow As RECT
        Dim rctSystemTray As RECT

        'UPGRADE_WARNING: Couldn't resolve default property of object rctSystemTray. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rctSystemTray = GetSysTrayPos()
        Call GetWindowRect(frmForm.Handle.ToInt32, rctWindow)

        If booDown = True Then
            DrawAnimatedRects(frmForm.Handle.ToInt32, IDANI_CLOSE Or IDANI_CAPTION, rctWindow, rctSystemTray)
        Else
            DrawAnimatedRects(frmForm.Handle.ToInt32, IDANI_OPEN Or IDANI_CAPTION, rctSystemTray, rctWindow)
        End If
    End Sub

    Private Function GetSysTrayPos() As RECT
        Dim lngTaskbarHwnd As Integer
        Dim lngTrayHwnd As Integer
        Dim strClassName As New VB6.FixedLengthString(250)

        'Get taskbar handle
        lngTaskbarHwnd = FindWindow("Shell_traywnd", vbNullString)

        'Get system tray handle
        lngTrayHwnd = GetWindow(lngTaskbarHwnd, GW_CHILD)
        Do
            GetClassName(lngTrayHwnd, strClassName.Value, 250)
            If TrimNull(strClassName.Value) = "TrayNotifyWnd" Then Exit Do
            lngTrayHwnd = GetWindow(lngTrayHwnd, GW_HWNDNEXT)
        Loop

        Call GetWindowRect(lngTrayHwnd, GetSysTrayPos)
    End Function

    Public Function TrimNull(ByVal strString As String) As String
        Dim lngPos As Integer
        lngPos = InStr(strString, Chr(0))

        If lngPos Then strString = Left(strString, lngPos - 1)
        TrimNull = strString
    End Function

    'Return whether we're running in the IDE. Public for general utility purposes
    Public Function InIDE() As Boolean
        System.Diagnostics.Debug.Assert(SetTrue(InIDE), "")
    End Function

    'Worker function for InIDE - will only be called whilst running in the IDE
    Private Function SetTrue(ByRef bValue As Boolean) As Boolean
        SetTrue = True
        bValue = True
    End Function
End Module
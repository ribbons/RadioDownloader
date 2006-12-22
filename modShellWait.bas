Attribute VB_Name = "modShellWait"
Option Explicit

' Store for command output (for passing back on error)
Public strOutput As String

Private Type SECURITY_ATTRIBUTES
    nLength As Long
    lpSecurityDescriptor As Long
    bInheritHandle As Long
End Type

Private Type STARTUPINFO
    cb As Long
    lpReserved As Long
    lpDesktop As Long
    lpTitle As Long
    dwX As Long
    dwY As Long
    dwXSize As Long
    dwYSize As Long
    dwXCountChars As Long
    dwYCountChars As Long
    dwFillAttribute As Long
    dwFlags As Long
    wShowWindow As Integer
    cbReserved2 As Integer
    lpReserved2 As Byte
    hStdInput As Long
    hStdOutput As Long
    hStdError As Long
End Type

Private Type PROCESS_INFORMATION
    hProcess As Long
    hThread As Long
    dwProcessId As Long
    dwThreadId As Long
End Type

Private Type OVERLAPPED
    ternal As Long
    ternalHigh As Long
    offset As Long
    OffsetHigh As Long
    hEvent As Long
End Type

Private Const STARTF_USESHOWWINDOW As Long = &H1
Private Const STARTF_USESTDHANDLES As Long = &H100
Private Const SW_HIDE As Long = 0

Private Declare Function CreatePipe Lib "kernel32.dll" (ByRef phReadPipe As Long, ByRef phWritePipe As Long, ByRef lpPipeAttributes As SECURITY_ATTRIBUTES, ByVal nSize As Long) As Long
Private Declare Function CreateProcess Lib "kernel32.dll" Alias "CreateProcessA" (ByVal lpApplicationName As String, ByVal lpCommandLine As String, ByRef lpProcessAttributes As SECURITY_ATTRIBUTES, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, ByVal bInheritHandles As Long, ByVal dwCreationFlags As Long, ByRef lpEnvironment As Any, ByVal lpCurrentDriectory As String, ByRef lpStartupInfo As STARTUPINFO, ByRef lpProcessInformation As PROCESS_INFORMATION) As Long
Private Declare Function CloseHandle Lib "kernel32.dll" (ByVal hObject As Long) As Long
Private Declare Function ReadFile Lib "kernel32.dll" (ByVal hFile As Long, ByRef lpBuffer As Any, ByVal nNumberOfBytesToRead As Long, ByRef lpNumberOfBytesRead As Long, ByRef lpOverlapped As OVERLAPPED) As Long

'Added by mjr
Private Declare Function PeekNamedPipe Lib "kernel32.dll" (ByVal hNamedPipe As Long, ByRef lpBuffer As Any, ByVal nBufferSize As Long, ByRef lpBytesRead As Long, ByRef lpTotalBytesAvail As Long, ByRef lpBytesLeftThisMessage As Long) As Long
Private Declare Sub Sleep Lib "kernel32.dll" (ByVal dwMilliseconds As Long)

Public lngLogFile As Long

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

Public Function ExecAndCapture(ByRef clsCaller As clsBkgMain, ByVal sCommandLine As String, ByVal strCallback As String, Optional ByVal sStartInFolder As String = vbNullString) As Boolean
Const BUFSIZE         As Long = 1024 * 10
Dim hPipeRead         As Long
Dim hPipeWrite        As Long
Dim sa                As SECURITY_ATTRIBUTES
Dim si                As STARTUPINFO
Dim pi                As PROCESS_INFORMATION
Dim baOutput(BUFSIZE) As Byte
Dim sOutput           As String
Dim lBytesRead        As Long
    
    With sa
        .nLength = Len(sa)
        .bInheritHandle = 1    ' get inheritable pipe
            ' handles
    End With 'SA
    
    If CreatePipe(hPipeRead, hPipeWrite, sa, 0) = 0 Then
        Exit Function
    End If

    With si
        .cb = Len(si)
        .dwFlags = STARTF_USESHOWWINDOW Or STARTF_USESTDHANDLES
        .wShowWindow = SW_HIDE          ' hide the window
        .hStdOutput = hPipeWrite
        .hStdError = hPipeWrite
    End With 'SI
    
    
    Dim secProcess As SECURITY_ATTRIBUTES
    Dim secThread As SECURITY_ATTRIBUTES
    
    If CreateProcess(vbNullString, sCommandLine, secProcess, secThread, 1, 0&, ByVal 0&, sStartInFolder, si, pi) Then
        Call CloseHandle(hPipeWrite)
        Call CloseHandle(pi.hThread)
        hPipeWrite = 0
        Do
            DoEvents
            
            Dim ovlOverlapped As OVERLAPPED
            
            ' Following chunk added by mjr - checks if any data is in pipe
            ' before calling readfile, which prevents blocking of this process
            Dim lngInPipe As Long
            If PeekNamedPipe(hPipeRead, 0&, 0, 0&, lngInPipe, 0&) = 0 Then
                Exit Do
            End If
            
            If lngInPipe > 0 Then ' Only try reading if there is data in pipe
                If ReadFile(hPipeRead, baOutput(0), BUFSIZE, lBytesRead, ovlOverlapped) = 0 Then
                    Exit Do
                End If
                
                sOutput = Left$(StrConv(baOutput(), vbUnicode), lBytesRead)
                
                strOutput = strOutput + sOutput
                
                Select Case strCallback
                    Case "Download"
                        If InStr(1, sOutput, "Core dumped ;)") > 0 Then
                            ExecAndCapture = True
                        End If
                        ' No callback as no progress messages
                    Case "ConvWav"
                        If InStr(1, sOutput, "Exiting... (End of file)") > 0 Then
                            ExecAndCapture = True
                        End If
                        Call clsCaller.ConvWavCallback(sOutput)
                    Case "ConvMp3"
                        If InStr(1, sOutput, "done") > 0 Then
                            ExecAndCapture = True
                        End If
                        Call clsCaller.ConvMp3Callback(sOutput)
                    Case Else
                        MsgBox sOutput
                End Select
            Else
                'faked callback for download - just call function every 1/2
                ' second with no data
                If strCallback = "Download" Then
                    Call clsCaller.DownloadCallback
                End If
                
                Call Sleep(500)
            End If
        Loop
        Call CloseHandle(pi.hProcess)
    End If
    ' To make sure...
    Call CloseHandle(hPipeRead)
    Call CloseHandle(hPipeWrite)
End Function

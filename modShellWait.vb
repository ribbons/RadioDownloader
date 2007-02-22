Option Strict Off
Option Explicit On
Module modShellWait
	
	' Store for command output (for passing back on error)
	Public strOutput As String
	
	Private Structure SECURITY_ATTRIBUTES
		Dim nLength As Integer
		Dim lpSecurityDescriptor As Integer
		Dim bInheritHandle As Integer
	End Structure
	
	Private Structure STARTUPINFO
		Dim cb As Integer
		Dim lpReserved As Integer
		Dim lpDesktop As Integer
		Dim lpTitle As Integer
		Dim dwX As Integer
		Dim dwY As Integer
		Dim dwXSize As Integer
		Dim dwYSize As Integer
		Dim dwXCountChars As Integer
		Dim dwYCountChars As Integer
		Dim dwFillAttribute As Integer
		Dim dwFlags As Integer
		Dim wShowWindow As Short
		Dim cbReserved2 As Short
		Dim lpReserved2 As Byte
		Dim hStdInput As Integer
		Dim hStdOutput As Integer
		Dim hStdError As Integer
	End Structure
	
	Private Structure PROCESS_INFORMATION
		Dim hProcess As Integer
		Dim hThread As Integer
		Dim dwProcessId As Integer
		Dim dwThreadId As Integer
	End Structure
	
	Private Structure OVERLAPPED
		Dim ternal As Integer
		Dim ternalHigh As Integer
		Dim offset As Integer
		Dim OffsetHigh As Integer
		Dim hEvent As Integer
	End Structure
	
	Private Const STARTF_USESHOWWINDOW As Integer = &H1s
	Private Const STARTF_USESTDHANDLES As Integer = &H100s
	Private Const SW_HIDE As Integer = 0
	
	'UPGRADE_WARNING: Structure SECURITY_ATTRIBUTES may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	Private Declare Function CreatePipe Lib "kernel32.dll" (ByRef phReadPipe As Integer, ByRef phWritePipe As Integer, ByRef lpPipeAttributes As SECURITY_ATTRIBUTES, ByVal nSize As Integer) As Integer
	'UPGRADE_WARNING: Structure PROCESS_INFORMATION may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	'UPGRADE_WARNING: Structure STARTUPINFO may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
	'UPGRADE_WARNING: Structure SECURITY_ATTRIBUTES may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	'UPGRADE_WARNING: Structure SECURITY_ATTRIBUTES may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
    'Private Declare Function CreateProcess Lib "kernel32.dll"  Alias "CreateProcessA"(ByVal lpApplicationName As String, ByVal lpCommandLine As String, ByRef lpProcessAttributes As SECURITY_ATTRIBUTES, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, ByVal bInheritHandles As Integer, ByVal dwCreationFlags As Integer, ByRef lpEnvironment As Any, ByVal lpCurrentDriectory As String, ByRef lpStartupInfo As STARTUPINFO, ByRef lpProcessInformation As PROCESS_INFORMATION) As Integer
	Private Declare Function CloseHandle Lib "kernel32.dll" (ByVal hObject As Integer) As Integer
	'UPGRADE_WARNING: Structure OVERLAPPED may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
    'Private Declare Function ReadFile Lib "kernel32.dll" (ByVal hFile As Integer, ByRef lpBuffer As Any, ByVal nNumberOfBytesToRead As Integer, ByRef lpNumberOfBytesRead As Integer, ByRef lpOverlapped As OVERLAPPED) As Integer
	
	'Added by mjr
	'UPGRADE_ISSUE: Declaring a parameter 'As Any' is not supported. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"'
    'Private Declare Function PeekNamedPipe Lib "kernel32.dll" (ByVal hNamedPipe As Integer, ByRef lpBuffer As Any, ByVal nBufferSize As Integer, ByRef lpBytesRead As Integer, ByRef lpTotalBytesAvail As Integer, ByRef lpBytesLeftThisMessage As Integer) As Integer
	Private Declare Sub Sleep Lib "kernel32.dll" (ByVal dwMilliseconds As Integer)
	
	Public lngLogFile As Integer
	
    Public Function ExecAndCapture(ByRef clsCaller As clsBackground, ByVal sCommandLine As String, ByVal strCallback As String, Optional ByVal sStartInFolder As String = vbNullString) As Boolean
        Const BUFSIZE As Integer = 1024 * 10
        Dim hPipeRead As Integer
        Dim hPipeWrite As Integer
        Dim sa As SECURITY_ATTRIBUTES
        Dim si As STARTUPINFO
        Dim pi As PROCESS_INFORMATION
        Dim baOutput(BUFSIZE) As Byte
        Dim sOutput As String
        Dim lBytesRead As Integer

        With sa
            .nLength = Len(sa)
            .bInheritHandle = 1 ' get inheritable pipe
            ' handles
        End With 'SA

        If CreatePipe(hPipeRead, hPipeWrite, sa, 0) = 0 Then
            Exit Function
        End If

        With si
            .cb = Len(si)
            .dwFlags = STARTF_USESHOWWINDOW Or STARTF_USESTDHANDLES
            .wShowWindow = SW_HIDE ' hide the window
            .hStdOutput = hPipeWrite
            .hStdError = hPipeWrite
        End With 'SI


        Dim secProcess As SECURITY_ATTRIBUTES
        Dim secThread As SECURITY_ATTRIBUTES

        Dim ovlOverlapped As OVERLAPPED
        Dim lngInPipe As Integer
        'If CreateProcess(vbNullString, sCommandLine, secProcess, secThread, 1, 0, 0, sStartInFolder, si, pi) Then
        Call CloseHandle(hPipeWrite)
        Call CloseHandle(pi.hThread)
        hPipeWrite = 0
        Do
            System.Windows.Forms.Application.DoEvents()


            ' Following chunk added by mjr - checks if any data is in pipe
            ' before calling readfile, which prevents blocking of this process
            'If PeekNamedPipe(hPipeRead, 0, 0, 0, lngInPipe, 0) = 0 Then
            'Exit Do
            'End If

            If lngInPipe > 0 Then ' Only try reading if there is data in pipe
                'If ReadFile(hPipeRead, baOutput(0), BUFSIZE, lBytesRead, ovlOverlapped) = 0 Then
                'Exit Do
                'End If

                'UPGRADE_ISSUE: Constant vbUnicode was not upgraded. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="55B59875-9A95-4B71-9D6A-7C294BF7139D"'
                'sOutput = Left(StrConv(System.Text.UnicodeEncoding.Unicode.GetString(baOutput), vbUnicode), lBytesRead)

                strOutput = strOutput & sOutput

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
                        MsgBox(sOutput)
                End Select
            Else
                'faked callback for download - just call function every 1/2
                ' second with no data
                If strCallback = "Download" Then
                    Call clsCaller.DownloadCallback()
                End If

                Call Sleep(500)
            End If
        Loop
        Call CloseHandle(pi.hProcess)
        'End If
        ' To make sure...
        Call CloseHandle(hPipeRead)
        Call CloseHandle(hPipeWrite)
    End Function
End Module
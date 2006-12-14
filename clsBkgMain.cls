VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "clsBkgMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Option Explicit

Public Type DownloadID
    strProgramType As String
    strProgramID As String
    dteDate As Date
End Type

Public Type DownloadAction
    booFound As Boolean
    dldDownloadID As DownloadID
    nxtNextAction As NextAction
End Type

Public Enum Status
    stWaiting
    stDownloading
    stDecoding
    stEncoding
    stCompleted
    stError
End Enum

Public Enum NextAction
    Download
    Decode
    EncodeMp3
    None
End Enum

Private Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (ByVal lpExistingFileName As String, ByVal lpNewFileName As String) As Long

' Implement the runnable interface, to allow asynchronous downloading
Implements Runnable

' Private variables to store information about the current task
Private enmAction As NextAction
Private strProgType As String
Private strProgID As String
Private lngDuration As Long
Private dteProgDate As Date
Private strProgTitle As String
Private strFinalName As String

' Vars to store speed info for estimating download progress
Private sngPrevSpeeds As Single
Private sngStartTime As Single

Public Event Finished()
Public Event Error(ByVal strError As String)
Public Event Progress(ByVal lngPercent As Long)

Public Property Get ProgramType() As String
    ProgramType = strProgType
End Property

Public Property Get ProgramID() As String
    ProgramID = strProgID
End Property

Public Property Get ProgramDate() As Date
    ProgramDate = dteProgDate
End Property

Public Property Get FinalName() As String
    FinalName = strFinalName
End Property

' This is called from the interface, to start some asynchronous processing
Public Sub Start(ByVal enmInAction As NextAction, ByVal strInProgType As String, ByVal strInProgID As String, ByVal lngInDuration As Long, ByVal dteInDate As Date, ByVal strInTitle As String)
    enmAction = enmInAction
    strProgType = strInProgType
    strProgID = strInProgID
    lngDuration = lngInDuration
    dteProgDate = dteInDate
    strProgTitle = strInTitle
    
    modAsyncStart.Start Me
End Sub

' This is called from modAsyncStart after control has passed back.
Private Sub Runnable_Start()
    ' Set up static var in showprogress
    Call ReturnProgress(-2)
    
    If booLog Then
        lngLogFile = FreeFile
        Open AddSlash(App.Path) + "backgroundlog.txt" For Output As lngLogFile
    End If
    
    Select Case enmAction
        Case Download
            Call ActDownload
        Case Decode
            Call ActToWav
        Case EncodeMp3
            Call ActToMp3
        Case Else
            If booLog Then
                Print #lngLogFile, "Nowhere to go"
            End If
    End Select
    
    If booLog Then
        Close #lngLogFile
    End If
End Sub

Public Sub ReturnProgress(lngPercent As Long)
    Static lngLastProgress As Long
    
    If lngPercent > -2 Then
        If lngPercent <> lngLastProgress Then
            RaiseEvent Progress(lngPercent)
        End If
    End If
    
    lngLastProgress = lngPercent
End Sub

Private Sub ActDownload()
    Call ReturnProgress(0)
    
    Dim lngTrimPos As Long
    Dim strJustName As String
    
    lngTrimPos = InStr(1, strProgID, "/")
    strJustName = Mid$(strProgID, lngTrimPos + 1)
    
    sngStartTime = Timer
    
    Dim booSuccess As Boolean
    booSuccess = ExecAndCapture(Me, """" + AddSlash(App.Path) + "components\mplayer.exe"" -dumpstream -playlist http://www.bbc.co.uk/radio/aod/shows/rpms/" + strProgID + ".ram -dumpfile """ + AddSlash(App.Path) + "temp\" + strJustName + ".ram"" -bandwidth 10000000", "Download")
    
    If lngDuration > 0 Then
        Dim sngSpeed As Single
        sngSpeed = lngDuration / (Timer - sngStartTime)
        
        Dim sngNewAverage As Single
        sngNewAverage = (sngPrevSpeeds * 9 + sngSpeed) / 10
        
        Call SaveSetting("Radio Downloader", "Background", "Average Speed", Format(sngNewAverage))
    End If
    
    If booSuccess Then
        Call ReturnProgress(100)
        RaiseEvent Finished
    Else
        RaiseEvent Error("Download Error")
    End If
End Sub

Public Sub DownloadCallback()
    Static lngStart As Long
    
    If lngDuration = 0 Then
        Exit Sub
    End If
    
    If sngPrevSpeeds = 0 Then
        sngPrevSpeeds = CSng(GetSetting("RadioDownloader", "Background", "AverageSpeed", "0.1"))
    End If
    
    Dim lngPercent As Long
    
    lngPercent = (((Timer - sngStartTime) / ((lngDuration * 60) * sngPrevSpeeds)) * 100) * 0.99
    If lngPercent > 99 Then lngPercent = 99
    
    Call ReturnProgress(lngPercent)
End Sub

Private Sub ActToWav()
    Call ReturnProgress(0)
    
    Dim lngTrimPos As Long
    Dim strJustName As String
    
    lngTrimPos = InStr(1, strProgID, "/")
    strJustName = Mid$(strProgID, lngTrimPos + 1)
    
    Dim booSuccess As Boolean
    booSuccess = ExecAndCapture(Me, """" + AddSlash(App.Path) + "components\mplayer.exe"" """ + strJustName + ".ram"" -ao pcm:file=""" + strJustName + ".wav""", "ConvWav", AddSlash(App.Path) + "temp\")
    
    If booSuccess Then
        Call ReturnProgress(100)
        Call Kill(AddSlash(App.Path) + "temp\" + strJustName + ".ram")
        RaiseEvent Finished
    Else
        RaiseEvent Error("Decoding Error")
    End If
End Sub

Public Sub ConvWavCallback(strReturned As String)
    Dim lngPos As Long
    lngPos = InStr(1, strReturned, vbCr)
    If lngPos > 0 Then
        strReturned = Left$(strReturned, lngPos - 1)
        strReturned = Trim(strReturned)
        
        If lngDuration = 0 Then
            Exit Sub
        End If
        
        Dim objRegExp As RegExp
        Dim objMatch As Match
        Dim colMatches As MatchCollection
        
        Set objRegExp = New RegExp
        objRegExp.Pattern = "A:(.*?) \(.*?\)  .*?%"
        objRegExp.IgnoreCase = True
        objRegExp.Global = True

        If objRegExp.Test(strReturned) = False Then
            Exit Sub
        End If

        Set colMatches = objRegExp.Execute(strReturned)
        Set objMatch = colMatches(0)
        
        Dim lngPercent As Long
        
        lngPercent = ((objMatch.SubMatches(0) / (lngDuration * 60)) * 100) * 0.99
        If lngPercent > 99 Then lngPercent = 99
        
        Call ReturnProgress(lngPercent)
    End If
End Sub

Private Sub ActToMp3()
    Call ReturnProgress(0)
    
    Dim lngTrimPos As Long
    Dim strJustName As String
    
    lngTrimPos = InStr(1, strProgID, "/")
    strJustName = Mid$(strProgID, lngTrimPos + 1)
    
    Dim booSuccess As Boolean
    booSuccess = ExecAndCapture(Me, """" + AddSlash(App.Path) + "components\lame.exe"" -b 128 -m j -q 2 """ + strJustName + ".wav"" """ + strJustName + ".mp3""", "ConvMp3", AddSlash(App.Path) + "temp\")
    
    If booSuccess Then
        Call ReturnProgress(100)
        Call Kill(AddSlash(App.Path) + "temp\" + strJustName + ".wav")
        
        Call MoveFile(AddSlash(App.Path) + "temp\" + strJustName + ".mp3", CreateFinalName)
        strFinalName = CreateFinalName
        
        RaiseEvent Finished
    Else
        RaiseEvent Error("Error Converting to MP3")
    End If
End Sub

Public Sub ConvMp3Callback(strReturned As String)
    Dim lngPos As Long
    lngPos = InStrRev(strReturned, vbCr)
    If lngPos > 0 Then
        strReturned = Mid$(strReturned, lngPos + 1)
        
        Dim objRegExp As RegExp
        Dim objMatch As Match
        Dim colMatches As MatchCollection
        
        Set objRegExp = New RegExp
        objRegExp.Pattern = "(.*?)/(.*?)\((.*?)%\)\|(.*?)/(.*?)\|(.*?)/(.*?)\|(.*?)x\|(.*) "
        objRegExp.IgnoreCase = True
        objRegExp.Global = True

        If objRegExp.Test(strReturned) = False Then
            Exit Sub
        End If

        Set colMatches = objRegExp.Execute(strReturned)
        Set objMatch = colMatches(0)
        
        '"Progress: " + Trim(objMatch.SubMatches(2)) + "% (Speed: " + Left$(Trim(objMatch.SubMatches(7)), 3) + "%)  Remaining: " + Trim(objMatch.SubMatches(8))
        Call ReturnProgress(CLng(Trim(objMatch.SubMatches(2))))
    End If
End Sub

Private Function CreateFinalName() As String
    Dim strCleanedTitle As String
    Dim strTrimmedTitle As String
    
    strCleanedTitle = Replace$(strProgTitle, "\", " ")
    strCleanedTitle = Replace$(strCleanedTitle, "/", " ")
    strCleanedTitle = Replace$(strCleanedTitle, ":", " ")
    strCleanedTitle = Replace$(strCleanedTitle, "*", " ")
    strCleanedTitle = Replace$(strCleanedTitle, "?", " ")
    strCleanedTitle = Replace$(strCleanedTitle, """", " ")
    strCleanedTitle = Replace$(strCleanedTitle, ">", " ")
    strCleanedTitle = Replace$(strCleanedTitle, "<", " ")
    strCleanedTitle = Replace$(strCleanedTitle, "|", " ")
    
    Do While strTrimmedTitle <> strCleanedTitle
        strTrimmedTitle = strCleanedTitle
        strCleanedTitle = Replace$(strCleanedTitle, "  ", " ")
    Loop
    
    CreateFinalName = AddSlash(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(App.Path) + "Downloads")) + Trim(strCleanedTitle) + " " + Format(dteProgDate, "dd-mm-yy") + ".mp3"
End Function

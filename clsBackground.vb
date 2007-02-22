Option Strict Off
Option Explicit On
Imports VB = Microsoft.VisualBasic
<System.Runtime.InteropServices.ProgId("clsBackground_NET.clsBackground")> Public Class clsBackground

    Public Structure DownloadID
        Dim strProgramType As String
        Dim strProgramID As String
        Dim dteDate As Date
    End Structure

    Public Structure DownloadAction
        Dim booFound As Boolean
        Dim dldDownloadID As DownloadID
        Dim nxtNextAction As NextAction
    End Structure

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

    Private Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (ByVal lpExistingFileName As String, ByVal lpNewFileName As String) As Integer

    ' Private variables to store information about the current task
    Private enmAction As NextAction
    Private strProgType As String
    Private strProgID As String
    Private lngDuration As Integer
    Private dteProgDate As Date
    Private strProgTitle As String
    Private strFinalName As String

    ' Vars to store speed info for estimating download progress
    Private sngPrevSpeeds As Single
    Private sngStartTime As Single

    Public Event Finished()
    'UPGRADE_NOTE: Error was upgraded to Error_Renamed. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Public Event DldError(ByVal strError As String, ByVal strCommandOutput As String)
    Public Event Progress(ByVal lngPercent As Integer)

    Public ReadOnly Property ProgramType() As String
        Get
            ProgramType = strProgType
        End Get
    End Property

    Public ReadOnly Property ProgramID() As String
        Get
            ProgramID = strProgID
        End Get
    End Property

    Public ReadOnly Property ProgramDate() As Date
        Get
            ProgramDate = dteProgDate
        End Get
    End Property

    Public ReadOnly Property FinalName() As String
        Get
            FinalName = strFinalName
        End Get
    End Property

    ' This is called from the interface, to start some asynchronous processing
    Public Sub Start(ByVal enmInAction As NextAction, ByVal strInProgType As String, ByVal strInProgID As String, ByVal lngInDuration As Integer, ByVal dteInDate As Date, ByVal strInTitle As String)
        ' Set up static var in showprogress
        Call ReturnProgress(-2)

        Select Case enmAction
            Case NextAction.Download
                Call ActDownload()
            Case NextAction.Decode
                Call ActToWav()
            Case NextAction.EncodeMp3
                Call ActToMp3()
            Case Else
                strOutput = strOutput & "Nowhere to go"
        End Select
    End Sub

    Public Sub ReturnProgress(ByRef lngPercent As Integer)
        Static lngLastProgress As Integer

        If lngPercent > -2 Then
            If lngPercent <> lngLastProgress Then
                RaiseEvent Progress(lngPercent)
            End If
        End If

        lngLastProgress = lngPercent
    End Sub

    Private Sub ActDownload()
        Call ReturnProgress(0)

        Dim lngTrimPos As Integer
        Dim strJustName As String

        lngTrimPos = InStr(1, strProgID, "/")
        strJustName = Mid(strProgID, lngTrimPos + 1)

        sngStartTime = VB.Timer()

        Dim booSuccess As Boolean
        booSuccess = ExecAndCapture(Me, """" & AddSlash(My.Application.Info.DirectoryPath) & "components\mplayer.exe"" -dumpstream -playlist http://www.bbc.co.uk/radio/aod/shows/rpms/" & strProgID & ".ram -dumpfile """ & AddSlash(My.Application.Info.DirectoryPath) & "temp\" & strJustName & ".ram"" -bandwidth 10000000", "Download")

        Dim sngSpeed As Single
        Dim sngNewAverage As Single
        If lngDuration > 0 Then
            sngSpeed = lngDuration / (VB.Timer() - sngStartTime)

            sngNewAverage = (sngPrevSpeeds * 9 + sngSpeed) / 10

            Call SaveSetting("Radio Downloader", "Background", "Average Speed", VB6.Format(sngNewAverage))
        End If

        If booSuccess Then
            Call ReturnProgress(100)
            RaiseEvent Finished()
        Else
            RaiseEvent DldError("Download Error", strOutput)
        End If
    End Sub

    Public Sub DownloadCallback()
        Static lngStart As Integer

        If lngDuration = 0 Then
            Exit Sub
        End If

        If sngPrevSpeeds = 0 Then
            sngPrevSpeeds = CSng(GetSetting("RadioDownloader", "Background", "AverageSpeed", "0.1"))
        End If

        Dim lngPercent As Integer

        lngPercent = (((VB.Timer() - sngStartTime) / ((lngDuration * 60) * sngPrevSpeeds)) * 100) * 0.99
        If lngPercent > 99 Then lngPercent = 99

        Call ReturnProgress(lngPercent)
    End Sub

    Private Sub ActToWav()
        Call ReturnProgress(0)

        Dim lngTrimPos As Integer
        Dim strJustName As String

        lngTrimPos = InStr(1, strProgID, "/")
        strJustName = Mid(strProgID, lngTrimPos + 1)

        Dim booSuccess As Boolean
        booSuccess = ExecAndCapture(Me, """" & AddSlash(My.Application.Info.DirectoryPath) & "components\mplayer.exe"" """ & strJustName & ".ram"" -ao pcm:file=""" & strJustName & ".wav""", "ConvWav", AddSlash(My.Application.Info.DirectoryPath) & "temp\")

        If booSuccess Then
            Call ReturnProgress(100)
            Call Kill(AddSlash(My.Application.Info.DirectoryPath) & "temp\" & strJustName & ".ram")
            RaiseEvent Finished()
        Else
            RaiseEvent DldError("Decoding Error", strOutput)
        End If
    End Sub

    Public Sub ConvWavCallback(ByRef strReturned As String)
        Dim lngPos As Integer
        lngPos = InStr(1, strReturned, vbCr)
        Dim objRegExp As VBScript_RegExp_55.RegExp
        Dim objMatch As VBScript_RegExp_55.Match
        Dim colMatches As VBScript_RegExp_55.MatchCollection
        Dim lngPercent As Integer
        If lngPos > 0 Then
            strReturned = Left(strReturned, lngPos - 1)
            strReturned = Trim(strReturned)

            If lngDuration = 0 Then
                Exit Sub
            End If


            objRegExp = New VBScript_RegExp_55.RegExp
            objRegExp.Pattern = "A:(.*?) \(.*?\)  .*?%"
            objRegExp.IgnoreCase = True
            objRegExp.Global = True

            If objRegExp.Test(strReturned) = False Then
                Exit Sub
            End If

            colMatches = objRegExp.Execute(strReturned)
            objMatch = colMatches(0)


            'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            lngPercent = ((objMatch.SubMatches(0) / (lngDuration * 60)) * 100) * 0.99
            If lngPercent > 99 Then lngPercent = 99

            Call ReturnProgress(lngPercent)
        End If
    End Sub

    Private Sub ActToMp3()
        Call ReturnProgress(0)

        Dim lngTrimPos As Integer
        Dim strJustName As String

        lngTrimPos = InStr(1, strProgID, "/")
        strJustName = Mid(strProgID, lngTrimPos + 1)

        Dim booSuccess As Boolean
        booSuccess = ExecAndCapture(Me, """" & AddSlash(My.Application.Info.DirectoryPath) & "components\lame.exe"" -b 128 -m j -q 2 """ & strJustName & ".wav"" """ & strJustName & ".mp3""", "ConvMp3", AddSlash(My.Application.Info.DirectoryPath) & "temp\")

        If booSuccess Then
            Call ReturnProgress(100)
            Call Kill(AddSlash(My.Application.Info.DirectoryPath) & "temp\" & strJustName & ".wav")

            Call MoveFile(AddSlash(My.Application.Info.DirectoryPath) & "temp\" & strJustName & ".mp3", CreateFinalName)
            strFinalName = CreateFinalName()

            RaiseEvent Finished()
        Else
            RaiseEvent DldError("Error Converting to MP3", strOutput)
        End If
    End Sub

    Public Sub ConvMp3Callback(ByRef strReturned As String)
        Dim lngPos As Integer
        lngPos = InStrRev(strReturned, vbCr)
        Dim objRegExp As VBScript_RegExp_55.RegExp
        Dim objMatch As VBScript_RegExp_55.Match
        Dim colMatches As VBScript_RegExp_55.MatchCollection
        If lngPos > 0 Then
            strReturned = Mid(strReturned, lngPos + 1)


            objRegExp = New VBScript_RegExp_55.RegExp
            objRegExp.Pattern = "(.*?)/(.*?)\((.*?)%\)\|(.*?)/(.*?)\|(.*?)/(.*?)\|(.*?)x\|(.*) "
            objRegExp.IgnoreCase = True
            objRegExp.Global = True

            If objRegExp.Test(strReturned) = False Then
                Exit Sub
            End If

            colMatches = objRegExp.Execute(strReturned)
            objMatch = colMatches(0)

            '"Progress: " + Trim(objMatch.SubMatches(2)) + "% (Speed: " + Left$(Trim(objMatch.SubMatches(7)), 3) + "%)  Remaining: " + Trim(objMatch.SubMatches(8))
            'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Call ReturnProgress(CInt(Trim(objMatch.SubMatches(2))))
        End If
    End Sub

    Private Function CreateFinalName() As String
        Dim strCleanedTitle As String
        Dim strTrimmedTitle As String = ""

        strCleanedTitle = Replace(strProgTitle, "\", " ")
        strCleanedTitle = Replace(strCleanedTitle, "/", " ")
        strCleanedTitle = Replace(strCleanedTitle, ":", " ")
        strCleanedTitle = Replace(strCleanedTitle, "*", " ")
        strCleanedTitle = Replace(strCleanedTitle, "?", " ")
        strCleanedTitle = Replace(strCleanedTitle, """", " ")
        strCleanedTitle = Replace(strCleanedTitle, ">", " ")
        strCleanedTitle = Replace(strCleanedTitle, "<", " ")
        strCleanedTitle = Replace(strCleanedTitle, "|", " ")

        Do While strTrimmedTitle <> strCleanedTitle
            strTrimmedTitle = strCleanedTitle
            strCleanedTitle = Replace(strCleanedTitle, "  ", " ")
        Loop

        CreateFinalName = AddSlash(GetSetting("Radio Downloader", "Interface", "SaveFolder", AddSlash(My.Application.Info.DirectoryPath) & "Downloads")) & Trim(strCleanedTitle) & " " & VB6.Format(dteProgDate, "dd-mm-yy") & ".mp3"
    End Function
End Class
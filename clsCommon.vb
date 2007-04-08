Option Explicit On
Option Strict On

Public Class clsCommon
    ' URL Download Declarations ------------------------------
    Private Declare Function InternetOpen Lib "wininet.dll" Alias "InternetOpenA" (ByVal sAgent As String, ByVal lAccessType As Integer, ByVal sProxyName As String, ByVal sProxyBypass As String, ByVal lFlags As Integer) As Integer
    Private Declare Function InternetOpenUrl Lib "wininet.dll" Alias "InternetOpenUrlA" (ByVal hInternetSession As Integer, ByVal sURL As String, ByVal sHeaders As String, ByVal lHeadersLength As Integer, ByVal lFlags As Integer, ByVal lContext As Integer) As Integer
    Private Declare Function InternetReadFile Lib "wininet.dll" (ByVal hFile As Integer, ByVal sBuffer As String, ByVal lNumBytesToRead As Integer, ByRef lNumberOfBytesRead As Integer) As Short
    Private Declare Function InternetCloseHandle Lib "wininet.dll" (ByVal hInet As Integer) As Short

    Private Const IF_FROM_CACHE As Integer = &H1000000
    Private Const IF_MAKE_PERSISTENT As Integer = &H2000000
    Private Const IF_NO_CACHE_WRITE As Integer = &H4000000
    Private Const BUFFER_LEN As Short = 256

    Public Function GetUrlAsString(ByRef sURL As String) As String
        Debug.Print("Downloading: " & sURL)

        Dim sBuffer As New VB6.FixedLengthString(BUFFER_LEN)
        Dim iResult As Short
        Dim sData As String = ""
        Dim hSession, hInternet, lReturn As Integer

        'get the handle of the current internet connection
        hSession = InternetOpen("", 1, vbNullString, vbNullString, 0)
        'get the handle of the url
        If CBool(hSession) Then hInternet = InternetOpenUrl(hSession, sURL, vbNullString, 0, IF_NO_CACHE_WRITE, 0)
        'if we have the handle, then start reading the web page
        If CBool(hInternet) Then
            'get the first chunk & buffer it.
            iResult = InternetReadFile(hInternet, sBuffer.Value, BUFFER_LEN, lReturn)
            sData = sBuffer.Value
            'if there's more data then keep reading it into the buffer
            Do While lReturn <> 0
                iResult = InternetReadFile(hInternet, sBuffer.Value, BUFFER_LEN, lReturn)
                sData = sData & Mid(sBuffer.Value, 1, lReturn)
            Loop
        End If

        'close the URL
        iResult = InternetCloseHandle(hInternet)

        GetUrlAsString = sData
    End Function
End Class

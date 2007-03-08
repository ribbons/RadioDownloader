Option Strict Off
Option Explicit On

Imports System.Data.SQLite

Friend Class clsData
    Private sqlConnection As SQLiteConnection

    Public Sub New()
        MyBase.New()
        sqlConnection = New SQLiteConnection("version=3,URI=file:" + My.Application.Info.DirectoryPath + "store.db")
        sqlConnection.Open()
    End Sub

    Protected Overrides Sub Finalize()
        sqlConnection.Close()
        MyBase.Finalize()
    End Sub

    'Public Function GetNextActionVal(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As clsBackground.NextAction
    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")

    '    With rstRecordset
    '        If .EOF = False Then
    '            .MoveFirst()
    '            GetNextActionVal = .Fields("NextAction").Value
    '        End If
    '    End With

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Function

    'Public Sub SetStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByRef booAuto As Boolean, Optional ByRef staValue As clsBackground.Status = 0)
    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")

    '    With rstRecordset
    '        If .EOF = False Then
    '            .MoveFirst()
    '            .Edit()
    '            If booAuto Then
    '                Select Case .Fields("NextAction").Value
    '                    Case clsBackground.NextAction.Download
    '                        .Fields("Status").Value = clsBackground.Status.stDownloading
    '                    Case clsBackground.NextAction.Decode
    '                        .Fields("Status").Value = clsBackground.Status.stDecoding
    '                    Case clsBackground.NextAction.EncodeMp3
    '                        .Fields("Status").Value = clsBackground.Status.stEncoding
    '                End Select
    '            Else
    '                .Fields("Status").Value = staValue
    '            End If
    '            .Update()
    '        End If
    '    End With

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Sub

    'Public Sub AdvanceNextAction(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")

    '    With rstRecordset
    '        If .EOF = False Then
    '            .MoveFirst()
    '            .Edit()
    '            .Fields("NextAction").Value = .Fields("NextAction").Value + 1
    '            .Update()
    '        End If
    '    End With

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Sub

    'Public Function FindNextAction() As clsBackground.DownloadAction
    '    Const lngMaxErrors As Integer = 2

    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE status=" & VB6.Format(clsBackground.Status.stWaiting) & " or (status=" & VB6.Format(clsBackground.Status.stError) & " and errors<" & VB6.Format(lngMaxErrors) & ") order by status")

    '    With rstRecordset
    '        If .EOF = False Then
    '            If .Fields("Status").Value = clsBackground.Status.stError Then
    '                Call ResetDownload(.Fields("Type").Value, .Fields("ID").Value, .Fields("Date").Value, True)
    '            End If

    '            .MoveFirst()
    '            FindNextAction.dldDownloadID.strProgramType = .Fields("Type").Value
    '            FindNextAction.dldDownloadID.strProgramID = .Fields("ID").Value
    '            FindNextAction.dldDownloadID.dteDate = .Fields("Date").Value
    '            FindNextAction.nxtNextAction = .Fields("NextAction").Value
    '            FindNextAction.booFound = True
    '        End If
    '    End With

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Function

    'Public Sub CleanupUnfinished()
    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE status<>" & VB6.Format(clsBackground.Status.stWaiting) & " AND status<>" & VB6.Format(clsBackground.Status.stCompleted) & " AND status<>" & VB6.Format(clsBackground.Status.stError))

    '    With rstRecordset
    '        If .EOF = False Then
    '            .MoveFirst()

    '            Do While .EOF = False
    '                .Edit()
    '                .Fields("Status").Value = clsBackground.Status.stWaiting
    '                .Fields("NextAction").Value = clsBackground.NextAction.Download
    '                .Update()

    '                .MoveNext()
    '            Loop
    '        End If
    '    End With

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Sub

    Public Sub SetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal strPath As String)
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Path=""+strPath+"" WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function GetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        GetDownloadPath = sqlReader.GetString("Path")

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As clsBackground.Status
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        DownloadStatus = sqlReader.GetString("Status")

        sqlReader.Close()
    End Function

    Public Function ProgramDuration(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDuration = sqlReader.GetString("Duration")

        sqlReader.Close()
    End Function

    Public Function ProgramTitle(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramTitle = sqlReader.GetString("Name")

        sqlReader.Close()
    End Function

    Public Function ProgramHTML(ByVal strProgramType As String, ByVal strProgramID As String, Optional ByVal dteProgramDate As Date = #12:00:00 AM#) As String
        Dim sqlCommand As SQLiteCommand

        If dteProgramDate > System.DateTime.FromOADate(0) Then
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Else
            Call GetLatest(strProgramType, strProgramID)
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ ORDER BY Date DESC")
        End If

        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader
        sqlReader.Read()

        Dim lngHours As Integer
        Dim lngMins As Integer

        With sqlReader
            ProgramHTML = "<h2>" + .GetString("Name") + "</h2>"
            ProgramHTML = ProgramHTML & "<p><img src=""" + .GetString("ImageURL") + """ />"
            ProgramHTML = ProgramHTML + .GetString("Description") + "</p>"

            ProgramHTML = ProgramHTML & "<div style=""clear: both;""></div>"
            ProgramHTML = ProgramHTML & .GetDateTime("Date").ToString("ddd dd/mmm/yy hh:mm") & " "

            lngMins = .GetString("Duration") Mod 60
            lngHours = .GetString("Duration") \ 60

            ProgramHTML = ProgramHTML & "<span style=""white-space: nowrap;"">"

            If lngHours > 0 Then
                ProgramHTML = ProgramHTML & VB6.Format(lngHours) & "hr"
                If lngHours > 1 Then
                    ProgramHTML = ProgramHTML & "s"
                End If
            End If
            If lngHours > 0 And lngMins > 0 Then
                ProgramHTML = ProgramHTML & " "
            End If
            If lngMins > 0 Then
                ProgramHTML = ProgramHTML & VB6.Format(lngMins) & "min"
            End If

            ProgramHTML = ProgramHTML & "</span>"
        End With

        sqlReader.Close()
    End Function

    'Public Function IsDownloading(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
    '    'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
    '    Dim rstRecordset As DAO.Recordset
    '    rstRecordset = dbDatabase.OpenRecordset("select * from tblDownloads where type='" & strProgramType & "' and id='" & strProgramID & "' and status<" & VB6.Format(clsBackground.Status.stCompleted))

    '    IsDownloading = rstRecordset.EOF = False

    '    rstRecordset.Close()
    '    'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '    rstRecordset = Nothing
    'End Function

    Public Sub UpdateDlList(ByRef lstListview As ListView)
        Dim lstAdd As ListViewItem

        Dim comCommand As New SQLiteCommand("select * from tblDownloads order by Date desc")
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        lstListview.Items.Clear()

        Do While sqlReader.Read()
            lstAdd = New ListViewItem
            lstAdd.Text = ProgramTitle(sqlReader.GetString("Type"), sqlReader.GetString("ID"), sqlReader.GetDateTime("Date").ToString())
            lstAdd.SubItems(1).Text = sqlReader.GetDateTime("Date").ToShortDateString()
            lstAdd.Tag = sqlReader.GetDateTime("Date").ToString() & "||" + sqlReader.GetString("ID") + "||" + sqlReader.GetString("Type")

            Select Case sqlReader.GetInt32("Status")
                Case clsBackground.Status.stWaiting
                    lstAdd.SubItems(2).Text = "Waiting"
                    lstAdd.ImageKey = "waiting"
                Case clsBackground.Status.stDownloading
                    lstAdd.SubItems(2).Text = "(1/3) Downloading"
                    lstAdd.ImageKey = "downloading"
                Case clsBackground.Status.stDecoding
                    lstAdd.SubItems(2).Text = "(2/3) Decoding"
                    lstAdd.ImageKey = "decoding"
                Case clsBackground.Status.stEncoding
                    lstAdd.SubItems(2).Text = "(3/3) Encoding"
                    lstAdd.ImageKey = "encoding"
                Case clsBackground.Status.stCompleted
                    lstAdd.SubItems(2).Text = "Completed"
                    lstAdd.ImageKey = "completed"
                Case clsBackground.Status.stError
                    lstAdd.SubItems(2).Text = "Error"
                    lstAdd.ImageKey = "error"
            End Select

            lstListview.Items.Add(lstAdd)
        Loop

        sqlReader.Close()
    End Sub

    Public Sub UpdateSubscrList(ByRef lstListview As ListView)
        Dim lstAdd As ListViewItem

        Dim sqlCommand As New SQLiteCommand("select * from tblSubscribed")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        lstListview.Items.Clear()

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString("Type"), .GetString("ID"))

                lstAdd = New ListViewItem

                lstAdd.Text = ProgramTitle(.GetString("Type"), .GetString("ID"), .GetDateTime("Type").ToString())
                lstAdd.Tag = .GetString("Type") + "||" + .GetString("ID")
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Sub CheckSubscriptions(ByRef lstList As ListView, ByRef tmrTimer As System.Windows.Forms.Timer)
        Dim sqlCommand As New SQLiteCommand("select * from tblSubscribed")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString("Type"), .GetString("ID"))

                Dim sqlComCheckDld As New SQLiteCommand("select * from tblDownloads where type=""" + .GetString("Type") + """ and ID=""" + .GetString("ID") + """ and Date=#" + LatestDate(.GetString("Type"), .GetString("ID")).ToString("mm/dd/yyyy Hh:Nn") + "#")
                Dim sqlRdrCheckDld As SQLiteDataReader = sqlCommand.ExecuteReader

                If sqlRdrCheckDld.Read() Then
                    Call AddDownload(.GetString("Type"), .GetString("ID"))
                    Call UpdateDlList(lstList)
                    tmrTimer.Enabled = True
                End If

                sqlRdrCheckDld.Close()
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Function AddDownload(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        On Error GoTo Error_Renamed

        Call GetLatest(strProgramType, strProgramID)

        'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim rstRecordset As DAO.Recordset
        rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads")

        With rstRecordset
            .AddNew()
            .Fields("Type").Value = strProgramType
            .Fields("ID").Value = strProgramID
            .Fields("Date").Value = LatestDate(strProgramType, strProgramID)
            .Fields("NextAction").Value = clsBackground.NextAction.Download
            .Fields("Status").Value = clsBackground.Status.stWaiting
            .Update()
        End With

        rstRecordset.Close()
        'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        rstRecordset = Nothing

        AddDownload = True
        Exit Function
Error_Renamed:
        Select Case Err.Number
            Case 3022 'Trying to create duplicate in database, ie trying to download twice!
                AddDownload = False
            Case Else
                On Error GoTo 0
                Resume
        End Select
    End Function

    Public Function AddSubscription(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        On Error GoTo Error_Renamed

        'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim rstRecordset As DAO.Recordset
        rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblSubscribed")

        With rstRecordset
            .AddNew()
            .Fields("Type").Value = strProgramType
            .Fields("ID").Value = strProgramID
            .Update()
        End With

        rstRecordset.Close()
        'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        rstRecordset = Nothing

        AddSubscription = True
        Exit Function
Error_Renamed:
        Select Case Err.Number
            Case 3022 'Trying to create duplicate in database, ie trying to download twice!
                AddSubscription = False
            Case Else
                On Error GoTo 0
                Resume
        End Select
    End Function

    Public Sub RemoveSubscription(ByVal strProgramType As String, ByVal strProgramID As String)
        dbDatabase.Execute(("DELETE FROM tblSubscribed WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """"))
    End Sub

    Private Sub GetLatest(ByVal strProgramType As String, ByVal strProgramID As String)
        If HaveLatest(strProgramType, strProgramID) = False Then
            Call StoreLatestInfo(strProgramType, strProgramID)
        End If
    End Sub

    Private Function LatestDate(ByVal strProgramType As String, ByVal strProgramID As String) As Date
        'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim rstRecordset As DAO.Recordset
        rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ ORDER BY Date DESC")

        With rstRecordset
            If .EOF Then
                Exit Function
            End If

            .MoveFirst()
            LatestDate = .Fields("Date").Value
        End With

        rstRecordset.Close()
        'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        rstRecordset = Nothing
    End Function

    Private Function HaveLatest(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        If strProgramType <> "BBCLA" Then
            ' In that case we'll need some more code!
            Stop
        End If

        Dim dteLatest As Date
        dteLatest = LatestDate(strProgramType, strProgramID)

        If dteLatest = System.DateTime.FromOADate(0) Then
            HaveLatest = False
            Exit Function
        End If

        ' If the current info is less than a week old, then
        If DateAdd(Microsoft.VisualBasic.DateInterval.WeekOfYear, 1, dteLatest) < Now Then
            HaveLatest = False
        Else
            HaveLatest = True
        End If
    End Function

    Private Sub StoreLatestInfo(ByVal strProgramType As String, ByRef strProgramID As String)
        If strProgramType <> "BBCLA" Then
            ' In that case we'll need some more code!
            Stop
        End If

        Dim strProgTitle As String
        Dim strProgDescription As String
        Dim lngProgDuration As Integer
        Dim dteProgDate As Date
        Dim strProgImgUrl As String

        Dim strInfo As String
        strInfo = GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/radio1/aod.shtml?" & strProgramID)

        strInfo = Replace(strInfo, "src=""", "src=""http://www.bbc.co.uk")

        Dim objRegExp As VBScript_RegExp_55.RegExp
        Dim objMatch As VBScript_RegExp_55.Match
        Dim colMatches As VBScript_RegExp_55.MatchCollection

        objRegExp = New VBScript_RegExp_55.RegExp
        objRegExp.Pattern = "<div id=""show"">" & vbLf & "<div id=""showtitle""><big>(.*?)</big> <span class=""txinfo"">\((.*?)\)<br />" & vbLf & "(.*?) - (.*?)</span><br />" & vbLf & "</div>" & vbLf & "<table cellpadding=""0"" cellspacing=""0"" border=""0"">" & vbLf & "<tr>" & vbLf & "<td valign=""top""><img src=""(.*?)"" width=""70"" height=""70"" alt="""" border=""0"" /></td>" & vbLf & "<td valign=""top"">(.*?)</td>" & vbLf & "</tr>" & vbLf & "</table>"
        objRegExp.IgnoreCase = True
        objRegExp.Global = True

        If objRegExp.Test(strInfo) = False Then
            Exit Sub
        End If

        colMatches = objRegExp.Execute(strInfo)
        objMatch = colMatches(0)

        ' objMatch.SubMatches(0) is Program Title
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strProgTitle = objMatch.SubMatches(0)

        ' objMatch.SubMatches(1) is Duration String, eg 1hr 30min
        Dim strDuration As String
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strDuration = objMatch.SubMatches(1)

        ' objMatch.SubMatches(3) is Date Sting eg Wed 26 Jul - 14:00
        Dim strDateString As String
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strDateString = objMatch.SubMatches(3)

        ' objMatch.SubMatches(4) is Image URL
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strProgImgUrl = objMatch.SubMatches(4)

        ' objMatch.SubMatches(5) is Program Description
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strProgDescription = objMatch.SubMatches(5)

        objRegExp.Pattern = "(([0-9]*?) hr)? ?(([0-9]*?) min)?"
        If objRegExp.Test(strDuration) = False Then
            Exit Sub
        End If

        colMatches = objRegExp.Execute(strDuration)
        objMatch = colMatches(0)

        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(3). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object objMatch.SubMatches(). Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        lngProgDuration = objMatch.SubMatches(1) * 60 + objMatch.SubMatches(3)
        ' Because the year isn't included in the program date, guess it is the
        ' current year (normally right).  If this guess ends up as a date in the
        ' future, then we have just passed the end of a year, and it is actually
        ' a program from last year.
        'UPGRADE_WARNING: DateDiff behavior may be different. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6B38EC3F-686D-4B2E-B5A5-9E8E7A762E32"'
        If DateDiff(Microsoft.VisualBasic.DateInterval.Minute, CDate(Mid(Replace(strDateString, "- ", ""), 5)), Now) < 0 Then
            dteProgDate = CDate(Mid(Replace(strDateString, "- ", VB6.Format(Year(Now) - 1) & " "), 5))
        Else
            dteProgDate = CDate(Mid(Replace(strDateString, "- ", ""), 5))
        End If

        ' Now store in DB

        'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim rstRecordset As DAO.Recordset
        rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgDate, "mm/dd/yyyy Hh:Nn") & "#")

        With rstRecordset
            If .EOF Then ' Make sure that it doesn't already exist
                .AddNew()
                .Fields("Type").Value = strProgramType
                .Fields("ID").Value = strProgramID
                .Fields("Date").Value = dteProgDate
                .Fields("Name").Value = strProgTitle
                .Fields("Description").Value = strProgDescription
                .Fields("ImageURL").Value = strProgImgUrl
                .Fields("Duration").Value = lngProgDuration
                .Update()
            End If
        End With

        rstRecordset.Close()
        'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        rstRecordset = Nothing
    End Sub

    Public Function ResetDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booAuto As Boolean) As Object
        'UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        Dim rstRecordset As DAO.Recordset
        rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")

        With rstRecordset
            If .EOF = False Then
                .MoveFirst()
                .Edit()
                .Fields("NextAction").Value = clsBackground.NextAction.Download
                .Fields("Status").Value = clsBackground.Status.stWaiting
                If booAuto Then
                    .Fields("Errors").Value = .Fields("Errors").Value + 1
                Else
                    .Fields("Errors").Value = 0
                End If
                .Update()
            End If
        End With

        rstRecordset.Close()
        'UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        rstRecordset = Nothing
    End Function

    Public Sub CancelDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Call dbDatabase.Execute("DELETE FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
    End Sub
End Class
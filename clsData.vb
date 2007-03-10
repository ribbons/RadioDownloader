Option Strict Off
Option Explicit On

Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Friend Class clsData
    Private sqlConnection As SQLiteConnection

    Public Sub New()
        MyBase.New()
        sqlConnection = New SQLiteConnection("Data Source=" + My.Application.Info.DirectoryPath + "\store.db;Version=3;New=False")
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
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Path=""" + strPath + """ WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString() + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function GetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString() + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        GetDownloadPath = sqlReader.GetString("Path")

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As clsBackground.Status
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString() + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        DownloadStatus = sqlReader.GetString("Status")

        sqlReader.Close()
    End Function

    Public Function ProgramDuration(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString() + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDuration = sqlReader.GetString("Duration")

        sqlReader.Close()
    End Function

    Public Function ProgramTitle(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString() + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramTitle = sqlReader.GetString("Name")

        sqlReader.Close()
    End Function

    Public Function ProgramHTML(ByVal strProgramType As String, ByVal strProgramID As String, Optional ByVal dteProgramDate As Date = #12:00:00 AM#) As String
        Dim sqlCommand As SQLiteCommand

        If dteProgramDate > System.DateTime.FromOADate(0) Then
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString() + """", sqlConnection)
        Else
            Call GetLatest(strProgramType, strProgramID)
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ ORDER BY Date DESC", sqlConnection)
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

                Dim sqlComCheckDld As New SQLiteCommand("select * from tblDownloads where type=""" + .GetString("Type") + """ and ID=""" + .GetString("ID") + """ and Date=""" + LatestDate(.GetString("Type"), .GetString("ID")).ToString() + """")
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
        Call GetLatest(strProgramType, strProgramID)

        Dim sqlCommand As New SQLiteCommand("INSERT INTO tblDownloads (Type, ID, Date, NextAction, Status) VALUES (""" + strProgramType + """, """ + strProgramID + """, """ + LatestDate(strProgramType, strProgramID) + """, """ + clsBackground.NextAction.Download + """, """ + clsBackground.Status.stWaiting + """)")

        Try
            Call sqlCommand.ExecuteNonQuery()
        Catch
            Stop
            Select Case Err.Number
                Case 3022 'Trying to create duplicate in database, ie trying to download twice!
                    Return False
                Case Else
            End Select
        End Try

        Return True
    End Function

    Public Function AddSubscription(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        Dim sqlCommand As New SQLiteCommand("INSERT INTO tblSubscribed (Type, ID) VALUES (""" + strProgramType + """, """ + strProgramID + """)")

        Try
            Call sqlCommand.ExecuteNonQuery()
        Catch
            Stop
            Select Case Err.Number
                Case 3022 'Trying to create duplicate in database, ie trying to subscribe twice!
                    Return False
                Case Else
            End Select
        End Try

        Return True
    End Function

    Public Sub RemoveSubscription(ByVal strProgramType As String, ByVal strProgramID As String)
        Dim sqlCommand As New SQLiteCommand("DELETE FROM tblSubscribed WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """")
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Private Sub GetLatest(ByVal strProgramType As String, ByVal strProgramID As String)
        If HaveLatest(strProgramType, strProgramID) = False Then
            Call StoreLatestInfo(strProgramType, strProgramID)
        End If
    End Sub

    Private Function LatestDate(ByVal strProgramType As String, ByVal strProgramID As String) As Date
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ ORDER BY Date DESC", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                Exit Function
            End If

            LatestDate = .GetDateTime("Date")
        End With

        sqlReader.Close()
    End Function

    Private Function HaveLatest(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        If strProgramType <> "BBCLA" Then
            ' In that case we'll need some more code!
            Stop
        End If

        Dim dteLatest As Date
        dteLatest = LatestDate(strProgramType, strProgramID)

        If dteLatest = System.DateTime.FromOADate(0) Then
            Return False
        End If

        ' If the current info is less than a week old, then
        If DateAdd(Microsoft.VisualBasic.DateInterval.WeekOfYear, 1, dteLatest) < Now Then
            Return False
        Else
            Return True
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
        Dim dteProgDate As DateTime
        Dim strProgImgUrl As String
        Dim strDuration As String
        Dim strDateString As String

        Dim strInfo As String
        strInfo = GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/radio1/aod.shtml?" & strProgramID)

        strInfo = Replace(strInfo, "src=""", "src=""http://www.bbc.co.uk")

        Dim RegExpression As Regex
        Dim grpMatches As GroupCollection

        RegExpression = New Regex("<div id=""show"">" & vbLf & "<div id=""showtitle""><big>(.*?)</big> <span class=""txinfo"">\((.*?)\)<br />" & vbLf & "(.*?) - (.*?)</span><br />" & vbLf & "</div>" & vbLf & "<table cellpadding=""0"" cellspacing=""0"" border=""0"">" & vbLf & "<tr>" & vbLf & "<td valign=""top""><img src=""(.*?)"" width=""70"" height=""70"" alt="""" border=""0"" /></td>" & vbLf & "<td valign=""top"">(.*?)</td>" & vbLf & "</tr>" & vbLf & "</table>")
        
        If RegExpression.IsMatch(strInfo) = False Then
            Exit Sub
        End If

        grpMatches = RegExpression.Match(strInfo).Groups

        ' objMatch.SubMatches(1) is Program Title
        strProgTitle = grpMatches(1).ToString()
        ' objMatch.SubMatches(2) is Duration String, eg 1hr 30min
        strDuration = grpMatches(2).ToString()
        ' objMatch.SubMatches(4) is Date Sting eg Wed 26 Jul - 14:00
        strDateString = grpMatches(4).ToString()
        ' objMatch.SubMatches(5) is Image URL
        strProgImgUrl = grpMatches(5).ToString()
        ' objMatch.SubMatches(6) is Program Description
        strProgDescription = grpMatches(6).ToString()

        RegExpression = New Regex("(([0-9]*?) hr)? ?(([0-9]*?) min)?")
        If RegExpression.IsMatch(strDuration) = False Then
            Exit Sub
        End If

        grpMatches = RegExpression.Match(strDuration).Groups

        lngProgDuration = grpMatches(2).ToString() * 60 + grpMatches(4).ToString()

        ' Now split up the date string
        RegExpression = New Regex("(?<dayname>(\w){3}) (?<day>(\d){2}) (?<monthname>(\w){3}) - (?<hour>(\d){2}):(?<minute>(\d){2})")
        If RegExpression.IsMatch(strDateString) = False Then
            Exit Sub
        End If

        grpMatches = RegExpression.Match(strDateString).Groups

        Dim intMonthNum As Integer = Array.IndexOf("jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec".Split("|".ToCharArray), grpMatches("monthname").ToString.ToLower) + 1

        ' Because the year isn't included in the program date, guess it is the current year (normally right)
        dteProgDate = New Date(Now.Year, intMonthNum, CInt(grpMatches("day").ToString), CInt(grpMatches("hour").ToString), CInt(grpMatches("minute").ToString), 0)

        ' If this guess ends up as a date in the future, then we have just passed the end of a year, and it is actually
        ' a program from last year.
        If dteProgDate > Now Then
            dteProgDate = New Date(Now.Year - 1, intMonthNum, CInt(grpMatches("day").ToString), CInt(grpMatches("hour").ToString), CInt(grpMatches("minute").ToString), 0)
        End If

        ' Now store in DB

        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE ID=""" & strProgramID & """ AND Date=""" & dteProgDate.ToString() + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read = False Then
            sqlCommand = New SQLiteCommand("INSERT INTO tblInfo (Type, ID, Date, Name, Description, ImageURL, Duration) VALUES (""" + strProgramType + """, """ + strProgramID + """, """ + dteProgDate + """, """ + strProgTitle + """, """ + strProgDescription + """, """ + strProgImgUrl + """, """ + lngProgDuration + """)")
            Call sqlCommand.ExecuteNonQuery()
        End If

        sqlReader.Close()
    End Sub

    Public Function ResetDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booAuto As Boolean) As Object
        ''UPGRADE_WARNING: Arrays in structure rstRecordset may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
        'Dim rstRecordset As DAO.Recordset
        'rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")

        'With rstRecordset
        '    If .EOF = False Then
        '        .MoveFirst()
        '        .Edit()
        '        .Fields("NextAction").Value = clsBackground.NextAction.Download
        '        .Fields("Status").Value = clsBackground.Status.stWaiting
        '        If booAuto Then
        '            .Fields("Errors").Value = .Fields("Errors").Value + 1
        '        Else
        '            .Fields("Errors").Value = 0
        '        End If
        '        .Update()
        '    End If
        'End With

        'rstRecordset.Close()
        ''UPGRADE_NOTE: Object rstRecordset may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSExpressCC.v80/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        'rstRecordset = Nothing
    End Function

    Public Sub CancelDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Dim sqlCommand As New SQLiteCommand("DELETE FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Call sqlCommand.ExecuteNonQuery()
    End Sub
End Class
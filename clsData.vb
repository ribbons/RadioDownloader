Option Strict On
Option Explicit On

Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Friend Class clsData
    Public Enum Statuses
        Waiting
        Downloaded
        Errored
    End Enum

    Private Const strSqlDateFormat As String = "yyyy-MM-dd hh:mm"
    Private sqlConnection As SQLiteConnection

    Public Event AddProgramToList(ByVal strType As String, ByVal strID As String, ByVal strName As String)

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

    Public Function FindNewDownload() As clsBackground
        Const lngMaxErrors As Integer = 2
        Dim clsBkgInst As clsBackground = Nothing

        Dim sqlCommand As New SQLiteCommand("select * from tblDownloads where status=" + Statuses.Waiting.ToString + " or (status=" + Statuses.Waiting.ToString + " and errors<" + lngMaxErrors.ToString + ") order by status")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() Then
            With sqlReader
                If .GetInt32(.GetOrdinal("Status")) = Statuses.Errored Then
                    Call ResetDownload(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("ID")), .GetString(.GetOrdinal("Date")), True)
                End If

                clsBkgInst = New clsBackground
                clsBkgInst.ProgramType = .GetString(.GetOrdinal("Type"))
                clsBkgInst.ProgramID = .GetString(.GetOrdinal("ID"))
                clsBkgInst.ProgramDate = .GetDateTime(.GetOrdinal("Date"))
            End With

            sqlReader.Close()
        End If

        Return clsBkgInst
    End Function

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
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Path=""" + strPath + """ WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function GetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        GetDownloadPath = sqlReader.GetString(sqlReader.GetOrdinal("Path"))

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Statuses
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        DownloadStatus = CType(sqlReader.GetInt32(sqlReader.GetOrdinal("Status")), Statuses)

        sqlReader.Close()
    End Function

    Public Function ProgramDuration(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """")
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDuration = sqlReader.GetInt32(sqlReader.GetOrdinal("Duration"))

        sqlReader.Close()
    End Function

    Public Function ProgramTitle(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramTitle = sqlReader.GetString(sqlReader.GetOrdinal("Name"))

        sqlReader.Close()
    End Function

    Public Function ProgramHTML(ByVal strProgramType As String, ByVal strProgramID As String, Optional ByVal dteProgramDate As Date = #12:00:00 AM#) As String
        Dim sqlCommand As SQLiteCommand

        If dteProgramDate > System.DateTime.FromOADate(0) Then
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Else
            Call GetLatest(strProgramType, strProgramID)
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ ORDER BY Date DESC", sqlConnection)
        End If

        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader
        sqlReader.Read()

        Dim lngHours As Integer
        Dim lngMins As Integer

        With sqlReader
            ProgramHTML = "<h2>" + .GetString(.GetOrdinal("Name")) + "</h2>"
            ProgramHTML = ProgramHTML + "<p><img src=""" + .GetString(.GetOrdinal("ImageURL")) + """ />"
            ProgramHTML = ProgramHTML + .GetString(.GetOrdinal("Description")) + "</p>"

            ProgramHTML = ProgramHTML + "<div style=""clear: both;""></div>"
            ProgramHTML = ProgramHTML + .GetDateTime(.GetOrdinal("Date")).ToString("ddd dd/mmm/yy hh:mm") & " "

            lngMins = .GetInt32(.GetOrdinal("Duration")) Mod 60
            lngHours = .GetInt32(.GetOrdinal("Duration")) \ 60

            ProgramHTML = ProgramHTML + "<span style=""white-space: nowrap;"">"

            If lngHours > 0 Then
                ProgramHTML = ProgramHTML + CStr(lngHours) + "hr"
                If lngHours > 1 Then
                    ProgramHTML = ProgramHTML + "s"
                End If
            End If
            If lngHours > 0 And lngMins > 0 Then
                ProgramHTML = ProgramHTML + " "
            End If
            If lngMins > 0 Then
                ProgramHTML = ProgramHTML + CStr(lngMins) + "min"
            End If

            ProgramHTML = ProgramHTML + "</span>"
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

        Dim comCommand As New SQLiteCommand("select * from tblDownloads order by Date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        lstListview.Items.Clear()

        Do While sqlReader.Read()
            lstAdd = New ListViewItem
            lstAdd.Text = ProgramTitle(sqlReader.GetString(sqlReader.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("ID")), sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")))
            lstAdd.SubItems.Add(sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToShortDateString())
            lstAdd.Tag = sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToString(strSqlDateFormat) & "||" + sqlReader.GetString(sqlReader.GetOrdinal("ID")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Type"))

            Select Case sqlReader.GetInt32(sqlReader.GetOrdinal("Status"))
                Case Statuses.Waiting
                    lstAdd.SubItems.Add("Waiting")
                    lstAdd.ImageKey = "waiting"
                Case Statuses.Downloaded
                    lstAdd.SubItems.Add("Downloaded")
                    lstAdd.ImageKey = "downloaded"
                Case Statuses.Errored
                    lstAdd.SubItems.Add("Error")
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
                Call GetLatest(.GetString(sqlReader.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("ID")))

                lstAdd = New ListViewItem

                lstAdd.Text = ProgramTitle(.GetString(sqlReader.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("ID")), .GetDateTime(sqlReader.GetOrdinal("Type")))
                lstAdd.Tag = .GetString(sqlReader.GetOrdinal("Type")) + "||" + .GetString(sqlReader.GetOrdinal("ID"))
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    'Public Sub CheckSubscriptions(ByRef lstList As ListView, ByRef tmrTimer As System.Windows.Forms.Timer)
    '    Dim sqlCommand As New SQLiteCommand("select * from tblSubscribed")
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    With sqlReader
    '        Do While .Read()
    '            Call GetLatest(.GetString("Type"), .GetString("ID"))

    '            Dim sqlComCheckDld As New SQLiteCommand("select * from tblDownloads where type=""" + .GetString("Type") + """ and ID=""" + .GetString("ID") + """ and Date=""" + LatestDate(.GetString("Type"), .GetString("ID")).ToString() + """")
    '            Dim sqlRdrCheckDld As SQLiteDataReader = sqlCommand.ExecuteReader

    '            If sqlRdrCheckDld.Read() Then
    '                Call AddDownload(.GetString("Type"), .GetString("ID"))
    '                Call UpdateDlList(lstList)
    '                tmrTimer.Enabled = True
    '            End If

    '            sqlRdrCheckDld.Close()
    '        Loop
    '    End With

    '    sqlReader.Close()
    'End Sub

    Public Function AddDownload(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
        Call GetLatest(strProgramType, strProgramID)

        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" & strProgramID + """ AND date=""" + LatestDate(strProgramType, strProgramID).ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() Then
            Return False
        End If

        sqlReader.Close()

        sqlCommand = New SQLiteCommand("INSERT INTO tblDownloads (Type, ID, Date, Status) VALUES (""" + strProgramType + """, """ + strProgramID + """, """ + LatestDate(strProgramType, strProgramID).ToString(strSqlDateFormat) + """, " + CStr(Statuses.Waiting) + ")", sqlConnection)
        Call sqlCommand.ExecuteNonQuery()

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

    Private Function LatestDate(ByVal strProgramType As String, ByVal strProgramID As String) As DateTime
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ ORDER BY Date DESC", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                Exit Function
            End If

            LatestDate = .GetDateTime(.GetOrdinal("Date"))
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
        Dim clsCommon As New clsCommon
        strInfo = clsCommon.GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/radio1/aod.shtml?" & strProgramID)

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

        If grpMatches(2).ToString() <> "" Then
            lngProgDuration = CInt(CDbl(grpMatches(2).Value) * 60)
        End If

        If grpMatches(4).ToString() <> "" Then
            lngProgDuration += CInt(grpMatches(4).Value)
        End If

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

        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE ID=""" & strProgramID & """ AND Date=""" & dteProgDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read = False Then
            sqlCommand = New SQLiteCommand("INSERT INTO tblInfo (Type, ID, Date, Name, Description, ImageURL, Duration) VALUES (""" + strProgramType + """, """ + strProgramID + """, """ + dteProgDate.ToString(strSqlDateFormat) + """, """ + strProgTitle + """, """ + strProgDescription + """, """ + strProgImgUrl + """, """ + CStr(lngProgDuration) + """)", sqlConnection)
            Call sqlCommand.ExecuteNonQuery()
        End If

        sqlReader.Close()
    End Sub

    Public Sub ResetDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booAuto As Boolean)
        Dim sqlCommand As New SQLiteCommand("update tblDownloads set status=" + Statuses.Waiting.ToString + " where type=""" & strProgramType & """ and id=""" & strProgramID & """ and date=" & dteProgramDate.ToString(strSqlDateFormat))
        sqlCommand.ExecuteNonQuery()

        If booAuto Then
            sqlCommand = New SQLiteCommand("update tblDownloads set ErrorCount=ErrorCount+1 where type=""" & strProgramType & """ and id=""" & strProgramID & """ and date=" & dteProgramDate.ToString(strSqlDateFormat))
            sqlCommand.ExecuteNonQuery()
        Else
            sqlCommand = New SQLiteCommand("update tblDownloads set ErrorCount=0 where type=""" & strProgramType & """ and id=""" & strProgramID & """ and date=" & dteProgramDate.ToString(strSqlDateFormat))
            sqlCommand.ExecuteNonQuery()
        End If
    End Sub

    Public Sub CancelDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Dim sqlCommand As New SQLiteCommand("DELETE FROM tblDownloads WHERE type=""" & strProgramType & """ AND ID=""" & strProgramID & """ AND Date=#" & VB6.Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") & "#")
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub StartListingStation(ByVal AvailablePlugins() As AvailablePlugin, ByVal strType As String, ByVal strID As String)
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = CType(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strType Then
                Exit For
            End If
        Next SinglePlugin

        Dim Programs() As IRadioProvider.ProgramListItem
        Programs = ThisInstance.ListProgramIDs(New clsCommon, strID)

        For Each SingleProg As IRadioProvider.ProgramListItem In Programs
            Call GetLatest(strType, SingleProg.ProgramID)

            Dim strProgTitle As String
            strProgTitle = SingleProg.ProgramName

            If strProgTitle = Nothing Then
                strProgTitle = ProgramTitle(strType, SingleProg.ProgramID, LatestDate(strType, SingleProg.ProgramID))
            End If

            RaiseEvent AddProgramToList(strType, SingleProg.ProgramID, strProgTitle)
        Next
    End Sub
End Class
VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "clsProgData"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Private dbDatabase As Database

Private Sub Class_Initialize()
    Set dbDatabase = OpenDatabase(AddSlash(App.Path) + "Store.mdb")
End Sub

Private Sub Class_Terminate()
    dbDatabase.Close
    Set dbDatabase = Nothing
End Sub

Public Function GetNextActionVal(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As NextAction
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            GetNextActionVal = ![NextAction]
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Sub SetStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, booAuto As Boolean, Optional staValue As Status)
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            .Edit
            If booAuto Then
                Select Case ![NextAction]
                    Case NextAction.Download
                        ![Status] = Status.stDownloading
                    Case NextAction.Decode
                        ![Status] = Status.stDecoding
                    Case NextAction.EncodeMp3
                        ![Status] = Status.stEncoding
                End Select
            Else
                ![Status] = staValue
            End If
            .Update
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Sub AdvanceNextAction(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            .Edit
            ![NextAction] = ![NextAction] + 1
            .Update
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Function FindNextAction() As DownloadAction
    Const lngMaxErrors As Long = 2
    
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE status=" + Format(Status.stWaiting) + " or (status=" + Format(Status.stError) + " and errors<" + Format(lngMaxErrors) + ") order by status")
    
    With rstRecordset
        If .EOF = False Then
            If ![Status] = Status.stError Then
                Call ResetDownload(![Type], ![ID], ![Date], True)
            End If
            
            .MoveFirst
            FindNextAction.dldDownloadID.strProgramType = ![Type]
            FindNextAction.dldDownloadID.strProgramID = ![ID]
            FindNextAction.dldDownloadID.dteDate = ![Date]
            FindNextAction.nxtNextAction = ![NextAction]
            FindNextAction.booFound = True
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Sub CleanupUnfinished()
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE status<>" + Format(Status.stWaiting) + " AND status<>" + Format(Status.stCompleted) + " AND status<>" + Format(Status.stError))
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            
            Do While .EOF = False
                .Edit
                ![Status] = stWaiting
                .Update
                
                .MoveNext
            Loop
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Sub SetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal strPath As String)
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        .MoveFirst
        .Edit
        ![Path] = strPath
        .Update
        .Close
    End With
    
    Set rstRecordset = Nothing
End Sub

Public Function GetDownloadPath(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    rstRecordset.MoveFirst
    GetDownloadPath = rstRecordset![Path]
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Function DownloadStatus(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Status
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
        
    rstRecordset.MoveFirst
    DownloadStatus = rstRecordset![Status]
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Function ProgramDuration(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Long
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
        
    rstRecordset.MoveFirst
    ProgramDuration = rstRecordset![Duration]
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Function ProgramTitle(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    rstRecordset.MoveFirst
    ProgramTitle = rstRecordset![Name]
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Function ProgramHTML(ByVal strProgramType As String, ByVal strProgramID As String, Optional ByVal dteProgramDate As Date) As String
    Dim rstRecordset As Recordset
        
    If dteProgramDate > 0 Then
        Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    Else
        Call GetLatest(strProgramType, strProgramID)
        Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ ORDER BY Date DESC")
    End If
    
    rstRecordset.MoveFirst
    
    With rstRecordset
        ProgramHTML = "<h2>" + ![Name] + "</h2>"
        ProgramHTML = ProgramHTML + "<p><img src=""" + ![ImageURL] + """ />"
        ProgramHTML = ProgramHTML + ![Description] + "</p>"
        
        ProgramHTML = ProgramHTML + "<div style=""clear: both;""></div>"
        
        ProgramHTML = ProgramHTML + Format(![Date], "ddd dd/mmm/yy hh:mm") + " "
        
        Dim lngHours As Long
        Dim lngMins As Long
        
        lngMins = ![Duration] Mod 60
        lngHours = ![Duration] \ 60
        
        ProgramHTML = ProgramHTML + "<span style=""white-space: nowrap;"">"
        
        If lngHours > 0 Then
            ProgramHTML = ProgramHTML + Format(lngHours) + "hr"
            If lngHours > 1 Then
                ProgramHTML = ProgramHTML + "s"
            End If
        End If
        If lngHours > 0 And lngMins > 0 Then
            ProgramHTML = ProgramHTML + " "
        End If
        If lngMins > 0 Then
            ProgramHTML = ProgramHTML + Format(lngMins) + "min"
        End If
        
        ProgramHTML = ProgramHTML + "</span>"
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Function IsDownloading(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("select * from tblDownloads where type='" + strProgramType + "' and id='" + strProgramID + "' and status<" + Format(Status.stCompleted))
    
    IsDownloading = rstRecordset.EOF = False
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Sub UpdateDlList(ByRef lstListview As ListView)
    Dim rstRecordset As Recordset
    Dim lstAdd As ListItem
    
    Set rstRecordset = dbDatabase.OpenRecordset("select * from tblDownloads order by Date desc")
    
    With rstRecordset
        lstListview.ListItems.Clear
            
        If .EOF = False Then
            .MoveFirst
            
            Do While .EOF = False
                Set lstAdd = lstListview.ListItems.Add
                lstAdd.Text = ProgramTitle(![Type], ![ID], ![Date])
                lstAdd.SubItems(1) = FormatDateTime(![Date], vbShortDate)
                lstAdd.Tag = Format(![Date]) + "||" + ![ID] + "||" + ![Type]
                
                Select Case ![Status]
                    Case stWaiting
                        lstAdd.SubItems(2) = "Waiting"
                        lstAdd.SmallIcon = 2
                    Case stDownloading
                        lstAdd.SubItems(2) = "(1/3) Downloading"
                        lstAdd.SmallIcon = 1
                    Case stDecoding
                        lstAdd.SubItems(2) = "(2/3) Decoding"
                        lstAdd.SmallIcon = 3
                    Case stEncoding
                        lstAdd.SubItems(2) = "(3/3) Encoding"
                        lstAdd.SmallIcon = 3
                    Case stCompleted
                        lstAdd.SubItems(2) = "Completed"
                        lstAdd.SmallIcon = 4
                    Case stError
                        lstAdd.SubItems(2) = "Error"
                        lstAdd.SmallIcon = 7
                End Select
                .MoveNext
            Loop
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Sub UpdateSubscrList(ByRef lstListview As ListView)
    Dim rstRecordset As Recordset
    Dim lstAdd As ListItem
    
    Set rstRecordset = dbDatabase.OpenRecordset("select * from tblSubscribed")
    
    With rstRecordset
        lstListview.ListItems.Clear
        
        If .EOF = False Then
            .MoveFirst
            
            Do While .EOF = False
                Call GetLatest(![Type], ![ID])
                
                Set lstAdd = lstListview.ListItems.Add
                lstAdd.Text = ProgramTitle(![Type], ![ID], LatestDate(![Type], ![ID]))
                lstAdd.Tag = ![Type] + "||" + ![ID]
                lstAdd.SmallIcon = 6
                
                .MoveNext
            Loop
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Sub CheckSubscriptions(ByRef lstList As ListView, ByRef tmrTimer As Timer)
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("select * from tblSubscribed")
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            
            Do While .EOF = False
                Call GetLatest(![Type], ![ID])
                
                Dim rstCheckDld As Recordset
                Set rstCheckDld = dbDatabase.OpenRecordset("select * from tblDownloads where type=""+strProgramType+"" ID=""" + ![ID] + """ and Date=#" + Format(LatestDate(![Type], ![ID]), "mm/dd/yyyy Hh:Nn") + "#")
                
                If rstCheckDld.EOF Then
                    Call AddDownload(![Type], ![ID])
                    Call UpdateDlList(lstList)
                    tmrTimer.Enabled = True
                End If
                
                .MoveNext
            Loop
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Function AddDownload(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
    On Error GoTo Error
    
    Call GetLatest(strProgramType, strProgramID)
    
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads")
    
    With rstRecordset
        .AddNew
        ![Type] = strProgramType
        ![ID] = strProgramID
        ![Date] = LatestDate(strProgramType, strProgramID)
        ![NextAction] = NextAction.Download
        ![Status] = Status.stWaiting
        .Update
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
    
    AddDownload = True
Exit Function
Error:
    Select Case Err.Number
        Case 3022 'Trying to create duplicate in database, ie trying to download twice!
            AddDownload = False
        Case Else
            On Error GoTo 0
            Resume
    End Select
End Function

Public Function AddSubscription(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
    On Error GoTo Error

    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblSubscribed")
    
    With rstRecordset
        .AddNew
        ![Type] = strProgramType
        ![ID] = strProgramID
        .Update
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
    
    AddSubscription = True
Exit Function
Error:
    Select Case Err.Number
        Case 3022 'Trying to create duplicate in database, ie trying to download twice!
            AddSubscription = False
        Case Else
            On Error GoTo 0
            Resume
    End Select
End Function

Public Sub RemoveSubscription(ByVal strProgramType As String, ByVal strProgramID As String)
    dbDatabase.Execute ("DELETE FROM tblSubscribed WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """")
End Sub

Private Sub GetLatest(ByVal strProgramType As String, ByVal strProgramID As String)
    If HaveLatest(strProgramType, strProgramID) = False Then
        Call StoreLatestInfo(strProgramType, strProgramID)
    End If
End Sub

Private Function LatestDate(ByVal strProgramType As String, ByVal strProgramID As String) As Date
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ ORDER BY Date DESC")
    
    With rstRecordset
        If .EOF Then
            Exit Function
        End If
        
        .MoveFirst
        LatestDate = ![Date]
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Private Function HaveLatest(ByVal strProgramType As String, ByVal strProgramID As String) As Boolean
    If strProgramType <> "BBCLA" Then
        ' In that case we'll need some more code!
        Stop
    End If
    
    Dim dteLatest As Date
    dteLatest = LatestDate(strProgramType, strProgramID)
    
    If dteLatest = 0 Then
        HaveLatest = False
        Exit Function
    End If
    
    ' If the current info is less than a week old, then
    If DateAdd("ww", 1, dteLatest) < Now() Then
        HaveLatest = False
    Else
        HaveLatest = True
    End If
End Function

Private Sub StoreLatestInfo(ByVal strProgramType As String, strProgramID As String)
    If strProgramType <> "BBCLA" Then
        ' In that case we'll need some more code!
        Stop
    End If
    
    Dim strProgTitle As String
    Dim strProgDescription As String
    Dim lngProgDuration As Long
    Dim dteProgDate As Date
    Dim strProgImgUrl As String
    
    Dim strInfo As String
    strInfo = GetUrlAsString("http://www.bbc.co.uk/radio/aod/networks/radio1/aod.shtml?" + strProgramID)
    
    strInfo = Replace$(strInfo, "src=""", "src=""http://www.bbc.co.uk")
    
    Dim objRegExp As RegExp
    Dim objMatch As Match
    Dim colMatches As MatchCollection

    Set objRegExp = New RegExp
    objRegExp.Pattern = "<div id=""show"">" + vbLf + "<div id=""showtitle""><big>(.*?)</big> <span class=""txinfo"">\((.*?)\)<br />" + vbLf + "(.*?) - (.*?)</span><br />" + vbLf + "</div>" + vbLf + "<table cellpadding=""0"" cellspacing=""0"" border=""0"">" + vbLf + "<tr>" + vbLf + "<td valign=""top""><img src=""(.*?)"" width=""70"" height=""70"" alt="""" border=""0"" /></td>" + vbLf + "<td valign=""top"">(.*?)</td>" + vbLf + "</tr>" + vbLf + "</table>"
    objRegExp.IgnoreCase = True
    objRegExp.Global = True

    If objRegExp.Test(strInfo) = False Then
        Exit Sub
    End If

    Set colMatches = objRegExp.Execute(strInfo)
    Set objMatch = colMatches(0)
    
    ' objMatch.SubMatches(0) is Program Title
    strProgTitle = objMatch.SubMatches(0)
    
    ' objMatch.SubMatches(1) is Duration String, eg 1hr 30min
    Dim strDuration As String
    strDuration = objMatch.SubMatches(1)
    
    ' objMatch.SubMatches(3) is Date Sting eg Wed 26 Jul - 14:00
    Dim strDateString As String
    strDateString = objMatch.SubMatches(3)
    
    ' objMatch.SubMatches(4) is Image URL
    strProgImgUrl = objMatch.SubMatches(4)
    
    ' objMatch.SubMatches(5) is Program Description
    strProgDescription = objMatch.SubMatches(5)
    
    objRegExp.Pattern = "(([0-9]*?) hr)? ?(([0-9]*?) min)?"
    If objRegExp.Test(strDuration) = False Then
        Exit Sub
    End If
    
    Set colMatches = objRegExp.Execute(strDuration)
    Set objMatch = colMatches(0)
    
    lngProgDuration = objMatch.SubMatches(1) * 60 + objMatch.SubMatches(3)
    dteProgDate = Mid$(Replace$(strDateString, "- ", ""), 5)
    
    ' Now store in DB
    
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblInfo WHERE ID=""" + strProgramID + """ AND Date=#" + Format(dteProgDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        If .EOF Then ' Make sure that it doesn't already exist
            .AddNew
            ![Type] = strProgramType
            ![ID] = strProgramID
            ![Date] = dteProgDate
            ![Name] = strProgTitle
            ![Description] = strProgDescription
            ![ImageURL] = strProgImgUrl
            ![Duration] = lngProgDuration
            .Update
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Sub

Public Function ResetDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booAuto As Boolean)
    Dim rstRecordset As Recordset
    Set rstRecordset = dbDatabase.OpenRecordset("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND Date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
    
    With rstRecordset
        If .EOF = False Then
            .MoveFirst
            .Edit
            ![NextAction] = NextAction.Download
            ![Status] = Status.stWaiting
            If booAuto Then
                ![Errors] = ![Errors] + 1
            Else
                ![Errors] = 0
            End If
            .Update
        End If
    End With
    
    rstRecordset.Close
    Set rstRecordset = Nothing
End Function

Public Sub CancelDownload(ByVal strProgramType As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
    Call dbDatabase.Execute("DELETE FROM tblDownloads WHERE type=""" + strProgramType + """ AND ID=""" + strProgramID + """ AND Date=#" + Format(dteProgramDate, "mm/dd/yyyy Hh:Nn") + "#")
End Sub

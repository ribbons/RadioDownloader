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

    Private Const strSqlDateFormat As String = "yyyy-MM-dd HH:mm"

    Private sqlConnection As SQLiteConnection
    Private AvailablePlugins() As AvailablePlugin

    Public Event AddProgramToList(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal strProgramName As String)

    Public Sub New(ByVal AvailablePlugins() As AvailablePlugin)
        MyBase.New()

        sqlConnection = New SQLiteConnection("Data Source=" + My.Application.Info.DirectoryPath + "\store.db;Version=3;New=False")
        sqlConnection.Open()

        Me.AvailablePlugins = AvailablePlugins
    End Sub

    Protected Overrides Sub Finalize()
        sqlConnection.Close()
        MyBase.Finalize()
    End Sub

    Public Sub SetErrored(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Status=" + CStr(Statuses.Errored) + ", ErrorTime=""" + Now.ToString(strSqlDateFormat) + """ WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub SetDownloaded(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal strDownloadPath As String)
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Status=""" + CStr(Statuses.Downloaded) + """, Path=""" + strDownloadPath + """ WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function FindNewDownload() As clsBackground
        Const lngMaxErrors As Integer = 2
        Dim clsBkgInst As clsBackground = Nothing

        Dim sqlCommand As New SQLiteCommand("select * from tblDownloads where status=" + CStr(Statuses.Waiting) + " or (status=" + CStr(Statuses.Errored) + " and ErrorCount<" + lngMaxErrors.ToString + ") order by status", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() Then
            With sqlReader
                If .GetInt32(.GetOrdinal("Status")) = Statuses.Errored Then
                    Call ResetDownload(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), .GetDateTime(.GetOrdinal("Date")), True)
                End If

                clsBkgInst = New clsBackground
                clsBkgInst.ProgramType = .GetString(.GetOrdinal("Type"))
                clsBkgInst.StationID = .GetString(.GetOrdinal("Station"))
                clsBkgInst.ProgramID = .GetString(.GetOrdinal("ID"))
                clsBkgInst.ProgramDate = .GetDateTime(.GetOrdinal("Date"))
                clsBkgInst.ProgramDuration = ProgramDuration(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), .GetDateTime(.GetOrdinal("Date")))
                clsBkgInst.ProgramTitle = ProgramTitle(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), .GetDateTime(.GetOrdinal("Date")))
            End With

            sqlReader.Close()
        End If

        Return clsBkgInst
    End Function

    Public Function GetDownloadPath(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        GetDownloadPath = sqlReader.GetString(sqlReader.GetOrdinal("Path"))

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Statuses
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        DownloadStatus = DirectCast(sqlReader.GetInt32(sqlReader.GetOrdinal("Status")), Statuses)

        sqlReader.Close()
    End Function

    Public Function ProgramDuration(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDuration = sqlReader.GetInt32(sqlReader.GetOrdinal("Duration"))

        sqlReader.Close()
    End Function

    Public Function ProgramTitle(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramTitle = sqlReader.GetString(sqlReader.GetOrdinal("Name"))

        sqlReader.Close()
    End Function

    Public Function ProgramHTML(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, Optional ByVal dteProgramDate As Date = #12:00:00 AM#) As String
        Dim sqlCommand As SQLiteCommand

        If dteProgramDate > System.DateTime.FromOADate(0) Then
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Else
            Call GetLatest(strProgramType, strStationID, strProgramID)
            sqlCommand = New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ ORDER BY Date DESC", sqlConnection)
        End If

        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() = False Then
            Return ""
        End If

        Dim lngHours As Integer
        Dim lngMins As Integer

        With sqlReader
            ProgramHTML = "<h2>" + .GetString(.GetOrdinal("Name")) + "</h2>"
            ProgramHTML = ProgramHTML + "<p><img src=""" + .GetString(.GetOrdinal("ImageURL")) + """ />"
            ProgramHTML = ProgramHTML + .GetString(.GetOrdinal("Description")) + "</p>"

            ProgramHTML = ProgramHTML + "<div style=""clear: both;""></div>"
            ProgramHTML = ProgramHTML + .GetDateTime(.GetOrdinal("Date")).ToString("ddd dd/MMM/yy hh:mm") & " "

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

    Public Sub UpdateDlList(ByRef lstListview As ExtListView, ByRef prgProgressBar As ProgressBar)
        Dim comCommand As New SQLiteCommand("select * from tblDownloads order by Date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        Dim lstAdd As ListViewItem
        lstListview.Items.Clear()

        If prgProgressBar.Visible Then
            prgProgressBar.Visible = False
            lstListview.RemoveAllControls()
        End If

        Do While sqlReader.Read()
            Dim strUniqueId As String = sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToString & "||" + sqlReader.GetString(sqlReader.GetOrdinal("ID")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Station")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Type"))

            lstAdd = lstListview.Items.Add(strUniqueId, "", 0)
            lstAdd.Tag = strUniqueId
            lstAdd.Text = ProgramTitle(sqlReader.GetString(sqlReader.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station")), sqlReader.GetString(sqlReader.GetOrdinal("ID")), sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")))

            lstAdd.SubItems.Add(sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToShortDateString())

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

            lstAdd.SubItems.Add("")
        Loop

        sqlReader.Close()
    End Sub

    Public Sub UpdateSubscrList(ByRef lstListview As ListView)
        Dim lstAdd As ListViewItem

        Dim sqlCommand As New SQLiteCommand("select * from tblSubscribed", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        lstListview.Items.Clear()

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString(sqlReader.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("Station")), .GetString(sqlReader.GetOrdinal("ID")))

                lstAdd = New ListViewItem

                lstAdd.Text = ProgramTitle(.GetString(.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), LatestDate(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))))
                lstAdd.Tag = .GetString(sqlReader.GetOrdinal("Type")) + "||" + .GetString(sqlReader.GetOrdinal("Station")) + "||" + .GetString(sqlReader.GetOrdinal("ID"))
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Sub CheckSubscriptions(ByRef lstList As ExtListView, ByRef tmrTimer As System.Windows.Forms.Timer, ByRef prgDldProg As ProgressBar)
        Dim sqlCommand As New SQLiteCommand("select * from tblSubscribed", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString(.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

                Dim sqlComCheckDld As New SQLiteCommand("select * from tblDownloads where type=""" + .GetString(.GetOrdinal("Type")) + """ and Station=""" + .GetString(.GetOrdinal("Station")) + """ and ID=""" + .GetString(.GetOrdinal("ID")) + """ and Date=""" + LatestDate(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))).ToString(strSqlDateFormat) + """", sqlConnection)
                Dim sqlRdrCheckDld As SQLiteDataReader = sqlComCheckDld.ExecuteReader

                If sqlRdrCheckDld.Read() = False Then
                    Call AddDownload(.GetString(.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))
                    Call UpdateDlList(lstList, prgDldProg)
                    tmrTimer.Enabled = True
                End If

                sqlRdrCheckDld.Close()
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Function AddDownload(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As Boolean
        Call GetLatest(strProgramType, strStationID, strProgramID)

        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + LatestDate(strProgramType, strStationID, strProgramID).ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() Then
            Return False
        End If

        sqlReader.Close()

        sqlCommand = New SQLiteCommand("INSERT INTO tblDownloads (Type, Station, ID, Date, Status) VALUES (""" + strProgramType + """, """ + strStationID + """, """ + strProgramID + """, """ + LatestDate(strProgramType, strStationID, strProgramID).ToString(strSqlDateFormat) + """, " + CStr(Statuses.Waiting) + ")", sqlConnection)
        Call sqlCommand.ExecuteNonQuery()

        Return True
    End Function

    Public Function AddSubscription(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As Boolean
        Dim sqlCommand As New SQLiteCommand("INSERT INTO tblSubscribed (Type, Station, ID) VALUES (""" + strProgramType + """, """ + strStationID + """, """ + strProgramID + """)", sqlConnection)

        Try
            Call sqlCommand.ExecuteNonQuery()
        Catch e As SQLiteException
            ' Probably trying to create duplicate in database, ie trying to subscribe twice!
            Return False
        End Try

        Return True
    End Function

    Public Sub RemoveSubscription(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String)
        Dim sqlCommand As New SQLiteCommand("DELETE FROM tblSubscribed WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """", sqlConnection)
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Private Sub GetLatest(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String)
        If HaveLatest(strProgramType, strStationID, strProgramID) = False Then
            Call StoreLatestInfo(strProgramType, strStationID, strProgramID)
        End If
    End Sub

    Private Function LatestDate(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As DateTime
        Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """  and Station=""" + strStationID + """ and ID=""" & strProgramID & """ ORDER BY Date DESC", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                Exit Function
            End If

            LatestDate = .GetDateTime(.GetOrdinal("Date"))
        End With

        sqlReader.Close()
    End Function

    Private Function HaveLatest(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As Boolean
        Dim dteLatest As Date
        dteLatest = LatestDate(strProgramType, strStationID, strProgramID)

        If dteLatest = DateTime.FromOADate(0) Then
            Return False
        End If

        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Return ThisInstance.IsLatestProgram(strStationID, strProgramID, dteLatest)
    End Function

    Private Sub StoreLatestInfo(ByVal strProgramType As String, ByVal strStationID As String, ByRef strProgramID As String)
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Dim ProgInfo As IRadioProvider.ProgramInfo
        ProgInfo = ThisInstance.GetLatestProgramInfo(strStationID, strProgramID)

        If ProgInfo.Success Then
            Dim sqlCommand As New SQLiteCommand("SELECT * FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """ AND Date=""" + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
            Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

            If sqlReader.Read = False Then
                sqlCommand = New SQLiteCommand("INSERT INTO tblInfo (Type, Station, ID, Date, Name, Description, ImageURL, Duration) VALUES (""" + strProgramType + """, """ + strStationID + """, """ + strProgramID + """, """ + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """, """ + ProgInfo.ProgramName + """, """ + ProgInfo.ProgramDescription + """, """ + ProgInfo.ImageUrl + """, """ + CStr(ProgInfo.ProgramDuration) + """)", sqlConnection)
                Call sqlCommand.ExecuteNonQuery()
            End If

            sqlReader.Close()
        End If
    End Sub

    Public Sub ResetDownload(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal booAuto As Boolean)
        Dim sqlCommand As New SQLiteCommand("update tblDownloads set status=" + CStr(Statuses.Waiting) + " where type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()

        If booAuto Then
            sqlCommand = New SQLiteCommand("update tblDownloads set ErrorCount=ErrorCount+1 where type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
            sqlCommand.ExecuteNonQuery()
        Else
            sqlCommand = New SQLiteCommand("update tblDownloads set ErrorCount=0 where type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
            sqlCommand.ExecuteNonQuery()
        End If
    End Sub

    Public Sub CancelDownload(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Dim sqlCommand As New SQLiteCommand("DELETE FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" + strProgramID + """ AND Date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub StartListingStation(ByVal strProgramType As String, ByVal strStationID As String)
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Dim Programs() As IRadioProvider.ProgramListItem
        Programs = ThisInstance.ListProgramIDs(strStationID)

        For Each SingleProg As IRadioProvider.ProgramListItem In Programs
            Dim strProgTitle As String
            strProgTitle = SingleProg.ProgramName
            RaiseEvent AddProgramToList(strProgramType, SingleProg.StationID, SingleProg.ProgramID, strProgTitle)
        Next
    End Sub
End Class
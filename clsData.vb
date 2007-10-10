' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007  www.nerdoftheherd.com
'
' This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
' Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
' option) any later version.
'
' This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
' implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
' License for more details.
'
' You should have received a copy of the GNU General Public License along with this program; if not, write
' to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

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

        sqlConnection = New SQLiteConnection("Data Source=" + GetAppDataFolder() + "\store.db;Version=3;New=False")
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

        Dim sqlCommand As New SQLiteCommand("select status, type, station, id, date from tblDownloads where status=" + CStr(Statuses.Waiting) + " or (status=" + CStr(Statuses.Errored) + " and ErrorCount<" + lngMaxErrors.ToString + ") order by date", sqlConnection)
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
        Dim sqlCommand As New SQLiteCommand("SELECT path FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        GetDownloadPath = sqlReader.GetString(sqlReader.GetOrdinal("Path"))

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Statuses
        Dim sqlCommand As New SQLiteCommand("SELECT status FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" + strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        DownloadStatus = DirectCast(sqlReader.GetInt32(sqlReader.GetOrdinal("Status")), Statuses)

        sqlReader.Close()
    End Function

    Public Function ProgramDuration(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT duration FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDuration = sqlReader.GetInt32(sqlReader.GetOrdinal("Duration"))

        sqlReader.Close()
    End Function

    Public Function ProgramTitle(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT name FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramTitle = sqlReader.GetString(sqlReader.GetOrdinal("Name"))

        sqlReader.Close()
    End Function

    Public Function ProgramDescription(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT description FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramDescription = sqlReader.GetString(sqlReader.GetOrdinal("description"))

        sqlReader.Close()
    End Function

    Public Function ProgramDetails(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        ProgramDetails = ProgramDescription(strProgramType, strStationID, strProgramID, dteProgramDate) + vbCrLf + vbCrLf
        ProgramDetails += "Date: " + dteProgramDate.ToString("ddd dd/MMM/yy HH:mm") + vbCrLf
        ProgramDetails += "Duration: "

        Dim lngDuration As Integer = ProgramDuration(strProgramType, strStationID, strProgramID, dteProgramDate)
        Dim lngHours As Integer = lngDuration \ 60
        Dim lngMins As Integer = lngDuration Mod 60

        If lngHours > 0 Then
            ProgramDetails += CStr(lngHours) + "hr"
            If lngHours > 1 Then
                ProgramDetails += "s"
            End If
        End If
        If lngHours > 0 And lngMins > 0 Then
            ProgramDetails += " "
        End If
        If lngMins > 0 Then
            ProgramDetails += CStr(lngMins) + "min"
        End If
    End Function

    Public Function ProgramImgUrl(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT imageurl FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        sqlReader.Read()
        ProgramImgUrl = sqlReader.GetString(sqlReader.GetOrdinal("imageurl"))

        sqlReader.Close()
    End Function

    Public Sub UpdateDlList(ByRef lstListview As ExtListView, ByRef prgProgressBar As ProgressBar)
        Dim comCommand As New SQLiteCommand("select Type,Station,ID,Date,Status,PlayCount from tblDownloads order by Date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        lstListview.RemoveAllControls()

        Dim lstItem As ListViewItem
        Dim booErrorStatus As Boolean = False
        Dim intExistingPos As Integer = 0

        Do While sqlReader.Read()
            Dim strUniqueId As String = sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToString & "||" + sqlReader.GetString(sqlReader.GetOrdinal("ID")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Station")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Type"))

            If lstListview.Items.Count - 1 < intExistingPos Then
                lstItem = lstListview.Items.Add(strUniqueId, "", 0)
            Else
                While lstListview.Items.ContainsKey(strUniqueId) And CStr(lstListview.Items(intExistingPos).Name) <> strUniqueId
                    lstListview.Items.RemoveAt(intExistingPos)
                End While

                If CStr(lstListview.Items(intExistingPos).Name) = strUniqueId Then
                    lstItem = lstListview.Items(intExistingPos)
                Else
                    lstItem = lstListview.Items.Insert(intExistingPos, strUniqueId, "", 0)
                End If
            End If

            intExistingPos += 1

            lstItem.SubItems.Clear()
            lstItem.Name = strUniqueId
            lstItem.Text = ProgramTitle(sqlReader.GetString(sqlReader.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station")), sqlReader.GetString(sqlReader.GetOrdinal("ID")), sqlReader.GetDateTime(sqlReader.GetOrdinal("Date"))) '+ " " + strUniqueId

            lstItem.SubItems.Add(sqlReader.GetDateTime(sqlReader.GetOrdinal("Date")).ToShortDateString())

            Select Case sqlReader.GetInt32(sqlReader.GetOrdinal("Status"))
                Case Statuses.Waiting
                    lstItem.SubItems.Add("Waiting")
                    lstItem.ImageKey = "waiting"
                Case Statuses.Downloaded
                    If sqlReader.GetInt32(sqlReader.GetOrdinal("PlayCount")) > 0 Then
                        lstItem.SubItems.Add("Downloaded")
                        lstItem.ImageKey = "downloaded"
                    Else
                        lstItem.SubItems.Add("Newly Downloaded")
                        lstItem.ImageKey = "downloaded_new"
                    End If
                Case Statuses.Errored
                    lstItem.SubItems.Add("Error")
                    lstItem.ImageKey = "error"
                    booErrorStatus = True
            End Select

            lstItem.SubItems.Add("")
        Loop

        If booErrorStatus Then
            frmMain.SetTrayStatus(False, frmMain.ErrorStatus.Error)
        Else
            frmMain.SetTrayStatus(False, frmMain.ErrorStatus.Normal)
        End If

        sqlReader.Close()
    End Sub

    Public Sub UpdateSubscrList(ByRef lstListview As ListView)
        Dim lstAdd As ListViewItem

        Dim sqlCommand As New SQLiteCommand("select type, station, id from tblSubscribed", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        lstListview.Items.Clear()

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString(sqlReader.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("Station")), .GetString(sqlReader.GetOrdinal("ID")))

                lstAdd = New ListViewItem

                lstAdd.Text = ProgramTitle(.GetString(.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), LatestDate(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))))
                lstAdd.SubItems.Add(StationName(.GetString(.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station"))))
                lstAdd.SubItems.Add(ProviderName(.GetString(.GetOrdinal("Type"))))
                lstAdd.Tag = .GetString(sqlReader.GetOrdinal("Type")) + "||" + .GetString(sqlReader.GetOrdinal("Station")) + "||" + .GetString(sqlReader.GetOrdinal("ID"))
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Sub CheckSubscriptions(ByRef lstList As ExtListView, ByRef tmrTimer As System.Windows.Forms.Timer, ByRef prgDldProg As ProgressBar)
        Dim sqlCommand As New SQLiteCommand("select type, station, id from tblSubscribed", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            Do While .Read()
                Call GetLatest(.GetString(.GetOrdinal("Type")), .GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

                Dim sqlComCheckDld As New SQLiteCommand("select id from tblDownloads where type=""" + .GetString(.GetOrdinal("Type")) + """ and Station=""" + .GetString(.GetOrdinal("Station")) + """ and ID=""" + .GetString(.GetOrdinal("ID")) + """ and Date=""" + LatestDate(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))).ToString(strSqlDateFormat) + """", sqlConnection)
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

        Dim sqlCommand As New SQLiteCommand("SELECT id FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + LatestDate(strProgramType, strStationID, strProgramID).ToString(strSqlDateFormat) + """", sqlConnection)
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

    Public Sub GetLatest(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String)
        If HaveLatest(strProgramType, strStationID, strProgramID) = False Then
            Call StoreLatestInfo(strProgramType, strStationID, strProgramID)
        End If
    End Sub

    Public Function LatestDate(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As DateTime
        Dim sqlCommand As New SQLiteCommand("SELECT date FROM tblInfo WHERE type=""" + strProgramType + """  and Station=""" + strStationID + """ and ID=""" & strProgramID & """ ORDER BY Date DESC", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                Return Nothing
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

        Return ThisInstance.CouldBeNewEpisode(strStationID, strProgramID, dteLatest) = False
    End Function

    Private Sub StoreLatestInfo(ByVal strProgramType As String, ByVal strStationID As String, ByRef strProgramID As String)
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Dim sqlCommand As SQLiteCommand
        Dim sqlReader As SQLiteDataReader
        Dim dteLastAttempt As Date = Nothing

        sqlCommand = New SQLiteCommand("SELECT LastTry FROM tblLastFetch WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """", sqlConnection)
        sqlReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            dteLastAttempt = sqlReader.GetDateTime(sqlReader.GetOrdinal("LastTry"))
        End If

        Dim ProgInfo As IRadioProvider.ProgramInfo
        ProgInfo = ThisInstance.GetLatestProgramInfo(strStationID, strProgramID, LatestDate(strProgramType, strStationID, strProgramID), dteLastAttempt)

        If ProgInfo.Result = IRadioProvider.ProgInfoResult.Success Then
            sqlCommand = New SQLiteCommand("SELECT id FROM tblInfo WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """ AND Date=""" + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
            sqlReader = sqlCommand.ExecuteReader

            If sqlReader.Read = False Then
                sqlCommand = New SQLiteCommand("INSERT INTO tblInfo (Type, Station, ID, Date, Name, Description, ImageURL, Duration) VALUES (""" + strProgramType + """, """ + strStationID + """, """ + strProgramID + """, """ + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """, """ + ProgInfo.ProgramName + """, """ + ProgInfo.ProgramDescription + """, """ + ProgInfo.ImageUrl + """, """ + CStr(ProgInfo.ProgramDuration) + """)", sqlConnection)
                Call sqlCommand.ExecuteNonQuery()
            End If

            sqlReader.Close()
        End If

        If ProgInfo.Result <> IRadioProvider.ProgInfoResult.Skipped And ProgInfo.Result <> IRadioProvider.ProgInfoResult.TempError Then
            ' Remove the previous record of when we tried to download info about this program (if it exists)
            sqlCommand = New SQLiteCommand("DELETE FROM tblLastFetch WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """", sqlConnection)
            Call sqlCommand.ExecuteNonQuery()

            ' Record when we tried to get information about this program
            sqlCommand = New SQLiteCommand("INSERT INTO tblLastFetch (Type, Station, ID, LastTry) VALUES (""" + strProgramType + """, """ + strStationID + """, """ & strProgramID & """, """ + Now.ToString(strSqlDateFormat) + """)", sqlConnection)
            Call sqlCommand.ExecuteNonQuery()
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

    Public Sub RemoveDownload(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
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

    Public Function IsProgramStillAvailable(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean
        Dim booIsLatestProg As Boolean

        Call GetLatest(strProgramType, strStationID, strProgramID)
        booIsLatestProg = (dteProgramDate = LatestDate(strProgramType, strStationID, strProgramID))

        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Return ThisInstance.IsStillAvailable(strStationID, strProgramID, dteProgramDate, booIsLatestProg)
    End Function

    Public Function StationName(ByVal strProgramType As String, ByVal strStationID As String) As String
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Return ThisInstance.ReturnStations.Item(strStationID).StationName
    End Function

    Public Function ProviderName(ByVal strProgramType As String) As String
        Dim ThisInstance As IRadioProvider = Nothing

        For Each SinglePlugin As AvailablePlugin In AvailablePlugins
            ThisInstance = DirectCast(CreateInstance(SinglePlugin), IRadioProvider)

            If ThisInstance.ProviderUniqueID = strProgramType Then
                Exit For
            End If
        Next SinglePlugin

        Return ThisInstance.ProviderName
    End Function

    Public Sub IncreasePlayCount(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date)
        Dim sqlCommand As New SQLiteCommand("update tblDownloads set PlayCount=PlayCount+1 where type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function PlayCount(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Integer
        Dim sqlCommand As New SQLiteCommand("SELECT PlayCount FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read Then
                PlayCount = .GetInt32(.GetOrdinal("PlayCount"))
            End If
        End With

        sqlReader.Close()
    End Function
End Class
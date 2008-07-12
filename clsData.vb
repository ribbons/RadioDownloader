' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2008  www.nerdoftheherd.com
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

Imports System.IO
Imports System.IO.File
Imports System.Threading
Imports System.Data.SQLite
Imports System.Text.RegularExpressions

Public Class clsData
    Public Enum Statuses
        Waiting
        Downloaded
        Errored
    End Enum

    Private Const strSqlDateFormat As String = "yyyy-MM-dd HH:mm"

    Private sqlConnection As SQLiteConnection
    Private clsPluginsInst As clsPlugins

    Private clsCurDldProgData As clsDldProgData
    Private thrDownloadThread As Thread
    Private WithEvents DownloadPluginInst As IRadioProvider
    Private WithEvents FindNewPluginInst As IRadioProvider

    Public Event FoundNew(ByVal intProgID As Integer)
    Public Event Progress(ByVal clsCurDldProgData As clsDldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    Public Event DldError(ByVal clsCurDldProgData As clsDldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
    Public Event Finished(ByVal clsCurDldProgData As clsDldProgData)

    Public Sub New()
        MyBase.New()

        sqlConnection = New SQLiteConnection("Data Source=" + GetAppDataFolder() + "\store.db;Version=3;New=False")
        sqlConnection.Open()

        ' Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
        Call VacuumDatabase()

        ' Perform any database changes required:
        If GetDBSetting("databaseversion") Is Nothing Then
            ' Nothing to do, as this should be the most up to date version of the database
        Else
            Dim intCurrentVersion As Integer = Convert.ToInt32(GetDBSetting("databaseversion"))

            ' Logic for database updates can go here when there is some
        End If

        ' Now set the current database version
        SetDBSetting("databaseversion", "1")

        ' Thin out the records in the info table if required
        Call PruneInfoTable()

        ' Create the temp table for caching HTTP requests
        Dim sqlCommand As New SQLiteCommand("create temporary table httpcache (uri varchar (1000), lastfetch datetime, data blob)", sqlConnection)
        sqlCommand.ExecuteNonQuery()

        clsPluginsInst = New clsPlugins(My.Application.Info.DirectoryPath)
    End Sub

    Protected Overrides Sub Finalize()
        sqlConnection.Close()
        Call AbortDownloadThread()
        MyBase.Finalize()
    End Sub

    Public Sub SetErrored(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Status=" + CStr(Statuses.Errored) + ", ErrorTime=""" + Now.ToString(strSqlDateFormat) + """, ErrorType=" + CStr(CInt(errType)) + ", ErrorDetails=""" + strErrorDetails.Replace("""", """""") + """ WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub SetDownloaded(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date, ByVal strDownloadPath As String)
        Dim sqlCommand As New SQLiteCommand("UPDATE tblDownloads SET Status=""" + CStr(Statuses.Downloaded) + """, Path=""" + strDownloadPath + """ WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function FindAndDownload() As Boolean
        If thrDownloadThread Is Nothing Then
            Dim sqlCommand As New SQLiteCommand("select status, type, station, id, date, errorcount from tblDownloads where status=" + CStr(Statuses.Waiting) + " or (status=" + CStr(Statuses.Errored) + " and ((ErrorCount=0 and ErrorTime<""" + Now.AddHours(-2).ToString(strSqlDateFormat) + """) or (ErrorCount=1 and ErrorTime<""" + Now.AddHours(-8).ToString(strSqlDateFormat) + """))) order by date", sqlConnection)
            Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

            With sqlReader
                While thrDownloadThread Is Nothing
                    If sqlReader.Read Then
                        Dim gidPluginID As New Guid(.GetString(.GetOrdinal("Type")))

                        If clsPluginsInst.PluginExists(gidPluginID) Then
                            If IsProgramStillAvailable(gidPluginID, .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date")) = False Then
                                Call RemoveDownload(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date"))
                            Else
                                clsCurDldProgData = New clsDldProgData
                                clsCurDldProgData.PluginID = gidPluginID
                                clsCurDldProgData.StationID = .GetString(.GetOrdinal("Station"))
                                clsCurDldProgData.ProgramID = .GetString(.GetOrdinal("ID"))
                                clsCurDldProgData.ProgramDate = GetCustFormatDateTime(sqlReader, "Date")
                                clsCurDldProgData.ProgramDuration = ProgramDuration(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date"))
                                clsCurDldProgData.DownloadUrl = ProgramDldUrl(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date"))
                                clsCurDldProgData.FinalName = FindFreeSaveFileName(My.Settings.FileNameFormat, ProgramTitle(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date")), "mp3", GetCustFormatDateTime(sqlReader, "Date"), GetSaveFolder())
                                clsCurDldProgData.BandwidthLimit = My.Settings.BandwidthLimit

                                If .GetInt32(.GetOrdinal("Status")) = Statuses.Errored Then
                                    Call ResetDownload(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date"), True)
                                    clsCurDldProgData.AttemptNumber = .GetInt32(.GetOrdinal("ErrorCount")) + 2
                                Else
                                    clsCurDldProgData.AttemptNumber = 1
                                End If

                                thrDownloadThread = New Thread(AddressOf DownloadProgThread)
                                thrDownloadThread.Start()

                                FindAndDownload = True
                            End If
                        End If
                    Else
                        Exit While
                    End If
                End While

                sqlReader.Close()
            End With
        End If
    End Function

    Public Sub DownloadProgThread()
        DownloadPluginInst = clsPluginsInst.GetPluginInstance(clsCurDldProgData.PluginID)

        Try
            DownloadPluginInst.DownloadProgram(clsCurDldProgData.StationID, clsCurDldProgData.ProgramID, clsCurDldProgData.ProgramDate, clsCurDldProgData.ProgramDuration, clsCurDldProgData.DownloadUrl, clsCurDldProgData.FinalName, clsCurDldProgData.BandwidthLimit, clsCurDldProgData.AttemptNumber)
        Catch expUnknown As Exception
            Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.UnknownError, expUnknown.GetType.ToString + ": " + expUnknown.Message + vbCrLf + expUnknown.StackTrace)
        End Try
    End Sub

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

    Public Function ProgramImage(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Bitmap
        Dim sqlCommand As New SQLiteCommand("SELECT image FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim objBlob As Object = sqlReader.GetValue(sqlReader.GetOrdinal("image"))

            If IsDBNull(objBlob) = False Then
                Dim stmStream As New MemoryStream(CType(objBlob, Byte()))
                ProgramImage = New Bitmap(stmStream)
            Else
                ProgramImage = Nothing
            End If
        Else
            ProgramImage = Nothing
        End If

        sqlReader.Close()
    End Function

    Private Function ProgramDldUrl(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT downloadurl FROM tblInfo WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """ AND date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If IsDBNull(sqlReader.GetValue(sqlReader.GetOrdinal("DownloadUrl"))) Then
                ProgramDldUrl = ""
            Else
                ProgramDldUrl = sqlReader.GetString(sqlReader.GetOrdinal("DownloadUrl"))
            End If
        Else
            ProgramDldUrl = ""
        End If

        sqlReader.Close()
    End Function

    Public Sub UpdateDlList(ByRef lstListview As ExtListView, ByRef prgProgressBar As ProgressBar)
        Dim comCommand As New SQLiteCommand("select Type,Station,ID,Date,Status,PlayCount from tblDownloads order by Date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        lstListview.RemoveAllControls()

        Dim lstItem As ListViewItem
        Dim booErrorStatus As Boolean = False
        Dim intExistingPos As Integer = 0

        Do While sqlReader.Read
            Dim strUniqueId As String = GetCustFormatDateTime(sqlReader, "Date").ToString & "||" + sqlReader.GetString(sqlReader.GetOrdinal("ID")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Station")) + "||" + sqlReader.GetString(sqlReader.GetOrdinal("Type"))

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
            lstItem.Text = ProgramTitle(sqlReader.GetString(sqlReader.GetOrdinal("Type")), sqlReader.GetString(sqlReader.GetOrdinal("Station")), sqlReader.GetString(sqlReader.GetOrdinal("ID")), GetCustFormatDateTime(sqlReader, "Date")) '+ " " + strUniqueId

            lstItem.SubItems.Add(GetCustFormatDateTime(sqlReader, "Date").ToShortDateString())

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

        ' Remove any left over items from the end of the list
        While lstListview.Items.Count > intExistingPos
            lstListview.Items.RemoveAt(intExistingPos)
        End While

        If booErrorStatus Then
            frmMain.SetTrayStatus(False, frmMain.ErrorStatus.Error)
        Else
            frmMain.SetTrayStatus(False, frmMain.ErrorStatus.Normal)
        End If

        sqlReader.Close()
    End Sub

    Public Sub UpdateSubscrList(ByRef lstListview As ListView)
        Dim lstAdd As ListViewItem

        Dim sqlCommand As New SQLiteCommand("select progid from subscriptions", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        lstListview.Items.Clear()

        With sqlReader
            Do While .Read()
                Dim gidPluginID As New Guid(.GetString(.GetOrdinal("Type")))
                lstAdd = New ListViewItem

                Dim strDynamicTitle As String
                strDynamicTitle = ProgramTitle(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), LatestDate(gidPluginID, .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))))

                If ProviderDynamicSubscriptionName(gidPluginID) Then
                    lstAdd.Text = strDynamicTitle
                Else
                    lstAdd.Text = SubscriptionName(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

                    If lstAdd.Text = "" Then
                        lstAdd.Text = strDynamicTitle
                    End If
                End If

                Dim dteLastDownload As Date = LatestDownloadDate(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

                If dteLastDownload = Nothing Then
                    lstAdd.SubItems.Add("Never")
                Else
                    lstAdd.SubItems.Add(dteLastDownload.ToShortDateString)
                End If

                'lstAdd.SubItems.Add(StationName(gidPluginID, .GetString(.GetOrdinal("Station"))))
                lstAdd.SubItems.Add(ProviderName(gidPluginID))
                lstAdd.Tag = .GetString(.GetOrdinal("Type")) + "||" + .GetString(.GetOrdinal("Station")) + "||" + .GetString(.GetOrdinal("ID"))
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    Private Function SubscriptionName(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As String
        Dim sqlCommand As New SQLiteCommand("SELECT SubscriptionName FROM tblSubscribed WHERE type=""" & strProgramType & """ and Station=""" + strStationID + """ AND ID=""" & strProgramID & """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If IsDBNull(sqlReader.GetValue(sqlReader.GetOrdinal("SubscriptionName"))) Then
                SubscriptionName = ""
            Else
                SubscriptionName = sqlReader.GetString(sqlReader.GetOrdinal("SubscriptionName"))
            End If
        Else
            SubscriptionName = ""
        End If

        sqlReader.Close()
    End Function

    Public Sub CheckSubscriptions(ByRef lstList As ExtListView, ByRef prgDldProg As ProgressBar)
        'Dim sqlCommand As New SQLiteCommand("select type, station, id from tblSubscribed", sqlConnection)
        'Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        'With sqlReader
        '    Do While .Read()
        '        Dim gidPluginID As New Guid(.GetString(.GetOrdinal("Type")))

        '        Call GetLatest(gidPluginID, .GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

        '        Dim sqlComCheckDld As New SQLiteCommand("select id from tblDownloads where type=""" + .GetString(.GetOrdinal("Type")) + """ and Station=""" + .GetString(.GetOrdinal("Station")) + """ and ID=""" + .GetString(.GetOrdinal("ID")) + """ and Date=""" + LatestDate(gidPluginID, .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))).ToString(strSqlDateFormat) + """", sqlConnection)
        '        Dim sqlRdrCheckDld As SQLiteDataReader = sqlComCheckDld.ExecuteReader

        '        If sqlRdrCheckDld.Read() = False Then
        '            Call AddDownload(gidPluginID, .GetString(sqlReader.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))
        '            Call UpdateDlList(lstList, prgDldProg)
        '        End If

        '        sqlRdrCheckDld.Close()
        '    Loop
        'End With

        'sqlReader.Close()
    End Sub

    Public Function AddDownload(ByVal gidPluginID As Guid, ByVal strStationID As String, ByVal strProgramID As String) As Boolean
        Call GetLatest(gidPluginID, strStationID, strProgramID)

        Dim sqlCommand As New SQLiteCommand("SELECT id FROM tblDownloads WHERE type=""" + gidPluginID.ToString + """ and Station=""" + strStationID + """ AND ID=""" & strProgramID + """ AND date=""" + LatestDate(gidPluginID, strStationID, strProgramID).ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Return False
        End If

        sqlReader.Close()

        sqlCommand = New SQLiteCommand("INSERT INTO tblDownloads (Type, Station, ID, Date, Status) VALUES (""" + gidPluginID.ToString + """, """ + strStationID + """, """ + strProgramID + """, """ + LatestDate(gidPluginID, strStationID, strProgramID).ToString(strSqlDateFormat) + """, " + CStr(Statuses.Waiting) + ")", sqlConnection)
        Call sqlCommand.ExecuteNonQuery()

        Return True
    End Function

    Public Function AddSubscription(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal strSubscriptionName As String) As Boolean
        Dim sqlCommand As New SQLiteCommand("INSERT INTO tblSubscribed (Type, Station, ID, SubscriptionName) VALUES (""" + strProgramType + """, """ + strStationID + """, """ + strProgramID + """, """ + strSubscriptionName.Replace("""", """""") + """)", sqlConnection)

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

    Public Sub GetLatest(ByVal gidPluginID As Guid, ByVal strStationID As String, ByVal strProgramID As String)
        If HaveLatest(gidPluginID, strStationID, strProgramID) = False Then
            Call StoreLatestInfo(gidPluginID, strStationID, strProgramID)
        End If
    End Sub

    Private Function GetCustFormatDateTime(ByVal sqlReader As SQLiteDataReader, ByVal strColumn As String) As Date
        Try
            GetCustFormatDateTime = DateTime.ParseExact(sqlReader.GetString(sqlReader.GetOrdinal(strColumn)), strSqlDateFormat, Nothing)
        Catch expException As InvalidCastException
            ' Somehow we have a value which can't be cast as a date, return nothing
            GetCustFormatDateTime = Nothing
        Catch expException As FormatException
            ' Somehow we have a valid date string of the wrong format, return nothing
            GetCustFormatDateTime = Nothing
        End Try
    End Function

    Public Function LatestDate(ByVal gidProviderID As Guid, ByVal strStationID As String, ByVal strProgramID As String) As DateTime
        Dim sqlCommand As New SQLiteCommand("SELECT date FROM tblInfo WHERE type=""" + gidProviderID.ToString + """  and Station=""" + strStationID + """ and ID=""" & strProgramID & """ ORDER BY Date DESC", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                ' No programs of this type in the database, return nothing
                LatestDate = Nothing
            Else
                LatestDate = GetCustFormatDateTime(sqlReader, "Date")
            End If
        End With

        sqlReader.Close()
    End Function

    Public Function LatestDownloadDate(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String) As DateTime
        Dim sqlCommand As New SQLiteCommand("select date from tblDownloads where type=""" + strProgramType + """  and station=""" + strStationID + """ and id=""" + strProgramID + """ order by date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            If .Read = False Then
                ' No downloads of this program, return nothing
                LatestDownloadDate = Nothing
            Else
                LatestDownloadDate = GetCustFormatDateTime(sqlReader, "date")
            End If
        End With

        sqlReader.Close()
    End Function

    Private Function HaveLatest(ByVal gidProviderID As Guid, ByVal strStationID As String, ByVal strProgramID As String) As Boolean
        Dim dteLatest As Date
        dteLatest = LatestDate(gidProviderID, strStationID, strProgramID)

        If dteLatest = Nothing Then
            Return False
        End If

        If clsPluginsInst.PluginExists(gidProviderID) Then
            Dim ThisInstance As IRadioProvider
            ThisInstance = clsPluginsInst.GetPluginInstance(gidProviderID)
            Return ThisInstance.CouldBeNewEpisode(strStationID, strProgramID, dteLatest) = False
        Else
            ' As we can't check if we have the latest, just assume that we do for the moment
            Return True
        End If
    End Function

    Private Sub StoreLatestInfo(ByVal gidPluginID As Guid, ByVal strStationID As String, ByRef strProgramID As String)
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            ' The plugin type for this program is unavailable, so no point trying to call it
            Exit Sub
        End If

        Dim sqlCommand As SQLiteCommand
        Dim sqlReader As SQLiteDataReader
        Dim dteLastAttempt As Date = Nothing
        Dim ThisInstance As IRadioProvider

        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        sqlCommand = New SQLiteCommand("SELECT LastTry FROM tblLastFetch WHERE type=""" + gidPluginID.ToString + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """", sqlConnection)
        sqlReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            dteLastAttempt = GetCustFormatDateTime(sqlReader, "LastTry")
        End If

        'Dim ProgInfo As IRadioProvider.ProgramInfo
        'ProgInfo = ThisInstance.GetLatestProgramInfo(strStationID, strProgramID, LatestDate(gidPluginID, strStationID, strProgramID), dteLastAttempt)

        'If ProgInfo.Result = IRadioProvider.ProgInfoResult.Success Then
        '    sqlCommand = New SQLiteCommand("SELECT id FROM tblInfo WHERE type=""" + gidPluginID.ToString + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """ AND Date=""" + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        '    sqlReader = sqlCommand.ExecuteReader

        '    If sqlReader.Read = False Then
        '        sqlCommand = New SQLiteCommand("INSERT INTO tblInfo (Type, Station, ID, Date, Name, Description, DownloadUrl, Image, Duration) VALUES (""" + gidPluginID.ToString + """, """ + strStationID + """, """ + strProgramID + """, """ + ProgInfo.ProgramDate.ToString(strSqlDateFormat) + """, """ + ProgInfo.ProgramName.Replace("""", """""") + """, """ + ProgInfo.ProgramDescription.Replace("""", """""") + """, """ + ProgInfo.ProgramDldUrl.Replace("""", """""") + """, @image, " + CStr(ProgInfo.ProgramDuration) + ")", sqlConnection)
        '        Dim sqlImage As SQLiteParameter

        '        If ProgInfo.Image IsNot Nothing Then
        '            ' Convert the image into a byte array
        '            Dim mstImage As New MemoryStream()
        '            ProgInfo.Image.Save(mstImage, Imaging.ImageFormat.Bmp)
        '            Dim bteImage(CInt(mstImage.Length - 1)) As Byte
        '            mstImage.Position = 0
        '            mstImage.Read(bteImage, 0, CInt(mstImage.Length))

        '            ' Add the image as a parameter
        '            sqlImage = New SQLiteParameter("@image", bteImage)
        '        Else
        '            sqlImage = New SQLiteParameter("@image", Nothing)
        '        End If

        '        sqlCommand.Parameters.Add(sqlImage)
        '        sqlCommand.ExecuteNonQuery()
        '    End If

        '    sqlReader.Close()
        'End If

        'If ProgInfo.Result <> IRadioProvider.ProgInfoResult.Skipped And ProgInfo.Result <> IRadioProvider.ProgInfoResult.TempError Then
        '    ' Remove the previous record of when we tried to download info about this program (if it exists)
        '    sqlCommand = New SQLiteCommand("DELETE FROM tblLastFetch WHERE type=""" + gidPluginID.ToString + """ and Station=""" + strStationID + """ and ID=""" & strProgramID & """", sqlConnection)
        '    Call sqlCommand.ExecuteNonQuery()

        '    ' Record when we tried to get information about this program
        '    sqlCommand = New SQLiteCommand("INSERT INTO tblLastFetch (Type, Station, ID, LastTry) VALUES (""" + gidPluginID.ToString + """, """ + strStationID + """, """ & strProgramID & """, """ + Now.ToString(strSqlDateFormat) + """)", sqlConnection)
        '    Call sqlCommand.ExecuteNonQuery()
        'End If
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

    Public Function IsProgramStillAvailable(ByVal gidPluginID As Guid, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As Boolean
        Dim booIsLatestProg As Boolean

        Call GetLatest(gidPluginID, strStationID, strProgramID)
        booIsLatestProg = (dteProgramDate = LatestDate(gidPluginID, strStationID, strProgramID))

        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            ' Guess that the program is still available, as otherwise if the plugin is unavailable for a 
            ' short time, downloads will be deleted that should be downloaded when it becomes available again.
            Return True
        End If

        Dim ThisInstance As IRadioProvider
        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        Return ThisInstance.IsStillAvailable(strStationID, strProgramID, dteProgramDate, booIsLatestProg)
    End Function

    Public Function ProviderName(ByVal gidPluginID As Guid) As String
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            Return ""
        End If

        Dim ThisInstance As IRadioProvider
        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        Return ThisInstance.ProviderName
    End Function

    Public Function ProviderDynamicSubscriptionName(ByVal gidPluginID As Guid) As Boolean
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            ' Just choose an option, as there is no way of finding out when the plugin is not available
            Return True
        End If

        Dim ThisInstance As IRadioProvider
        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        Return ThisInstance.DynamicSubscriptionName
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

    Public Function ErrorType(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As IRadioProvider.ErrorType
        Dim sqlCommand As New SQLiteCommand("SELECT ErrorType FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If IsDBNull(sqlReader.GetValue(sqlReader.GetOrdinal("ErrorType"))) Then
                ErrorType = IRadioProvider.ErrorType.UnknownError
            Else
                ErrorType = CType(sqlReader.GetInt32(sqlReader.GetOrdinal("ErrorType")), IRadioProvider.ErrorType)
            End If
        Else
            ErrorType = IRadioProvider.ErrorType.UnknownError
        End If

        sqlReader.Close()
    End Function

    Public Function ErrorDetails(ByVal strProgramType As String, ByVal strStationID As String, ByVal strProgramID As String, ByVal dteProgramDate As Date) As String
        Dim sqlCommand As New SQLiteCommand("SELECT ErrorDetails FROM tblDownloads WHERE type=""" + strProgramType + """ and Station=""" + strStationID + """ and id=""" + strProgramID + """ and date=""" + dteProgramDate.ToString(strSqlDateFormat) + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If IsDBNull(sqlReader.GetValue(sqlReader.GetOrdinal("ErrorDetails"))) Then
                ErrorDetails = ""
            Else
                ErrorDetails = sqlReader.GetString(sqlReader.GetOrdinal("ErrorDetails"))
            End If
        Else
            ErrorDetails = ""
        End If

        sqlReader.Close()
    End Function

    Private Sub DownloadPluginInst_DldError(ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Handles DownloadPluginInst.DldError
        RaiseEvent DldError(clsCurDldProgData, errType, strErrorDetails)
        thrDownloadThread = Nothing
        clsCurDldProgData = Nothing
    End Sub

    Private Sub DownloadPluginInst_Finished() Handles DownloadPluginInst.Finished
        RaiseEvent Finished(clsCurDldProgData)
        thrDownloadThread = Nothing
        clsCurDldProgData = Nothing
    End Sub

    Private Sub DownloadPluginInst_Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles DownloadPluginInst.Progress
        RaiseEvent Progress(clsCurDldProgData, intPercent, strStatusText, Icon)
    End Sub

    Public Sub AbortDownloadThread()
        If thrDownloadThread IsNot Nothing Then
            thrDownloadThread.Abort()
            thrDownloadThread = Nothing
        End If
    End Sub

    Public Function GetCurrentDownloadInfo() As clsDldProgData
        Return clsCurDldProgData
    End Function

    Public Sub UpdateProviderList(ByVal lstProviders As ListView)
        Dim gidPluginIDs() As Guid
        gidPluginIDs = clsPluginsInst.GetPluginIdList

        Dim ThisInstance As IRadioProvider
        Dim lstAdd As ListViewItem

        For Each gidPluginID As Guid In gidPluginIDs
            ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

            lstAdd = New ListViewItem
            lstAdd.Text = ThisInstance.ProviderName
            lstAdd.Tag = ThisInstance.ProviderID
            lstAdd.ImageKey = "default" 'ThisInstance.ProviderID.ToString

            lstProviders.Items.Add(lstAdd)
        Next
    End Sub

    Private Function IsStationVisible(ByVal strPluginID As String, ByVal strStationID As String, ByVal booDefault As Boolean) As Boolean
        Dim sqlCommand As New SQLiteCommand("select Visible from tblStationVisibility where ProviderID=""" + strPluginID + """ and StationID=""" + strStationID + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If sqlReader.GetBoolean(sqlReader.GetOrdinal("Visible")) Then
                IsStationVisible = True
            Else
                IsStationVisible = False
            End If
        Else
            If booDefault Then
                IsStationVisible = True
            Else
                IsStationVisible = False
            End If
        End If

        sqlReader.Close()
    End Function

    Public Sub SetStationVisibility(ByVal strStationType As String, ByVal strStationID As String, ByVal booVisible As Boolean)
        Dim sqlCommand As New SQLiteCommand("select Visible from tblStationVisibility where ProviderID=""" + strStationType + """ and StationID=""" + strStationID + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        Dim strVisible As String
        If booVisible Then
            strVisible = "1"
        Else
            strVisible = "0"
        End If


        If sqlReader.Read Then
            sqlCommand = New SQLiteCommand("update tblStationVisibility set Visible=" + strVisible + " where ProviderID=""" + strStationType + """ and StationID=""" + strStationID + """", sqlConnection)
            sqlCommand.ExecuteNonQuery()
        Else
            sqlCommand = New SQLiteCommand("insert into tblStationVisibility (ProviderID, StationID, Visible) VALUES (""" + strStationType + """, """ + strStationID + """, " + strVisible + ")", sqlConnection)
            Call sqlCommand.ExecuteNonQuery()
        End If

        sqlReader.Close()
    End Sub

    Public Sub PerformCleanup()
        Dim sqlCommand As New SQLiteCommand("select type,station,id,date,path from tblDownloads where status=" + CStr(Statuses.Downloaded), sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        With sqlReader
            Do While .Read
                ' Remove programmes for which the associated audio file no longer exists
                If Exists(.GetString(.GetOrdinal("path"))) = False Then
                    RemoveDownload(.GetString(.GetOrdinal("type")), .GetString(.GetOrdinal("station")), .GetString(.GetOrdinal("id")), GetCustFormatDateTime(sqlReader, "date"))
                End If
            Loop
        End With

        sqlReader.Close()
    End Sub

    Private Sub SetDBSetting(ByVal strPropertyName As String, ByVal strValue As String)
        Dim sqlCommand As New SQLiteCommand("select value from tblSettings where property=""" + strPropertyName + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        If sqlReader.Read Then
            sqlCommand = New SQLiteCommand("update tblSettings set value=""" + strValue.Replace("""", """""") + """ where property=""" + strPropertyName + """", sqlConnection)
            sqlCommand.ExecuteNonQuery()
        Else
            sqlCommand = New SQLiteCommand("insert into tblSettings (property, value) VALUES (""" + strPropertyName + """, """ + strValue.Replace("""", """""") + """)", sqlConnection)
            Call sqlCommand.ExecuteNonQuery()
        End If

        sqlCommand.Cancel()
    End Sub

    Private Function GetDBSetting(ByVal strPropertyName As String) As String
        Dim sqlCommand As New SQLiteCommand("select value from tblSettings where property=""" + strPropertyName + """", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        If sqlReader.Read = False Then
            Return Nothing
        End If

        Dim strValue As String = sqlReader.GetString(sqlReader.GetOrdinal("value"))

        sqlReader.Close()

        Return strValue
    End Function

    Private Sub PruneInfoTable()
        ' Only prune the info table once a month, so that we don't slow down every startup
        Dim booRunPrune As Boolean
        Dim strLastPrune As String = GetDBSetting("lastinfoprune")

        If strLastPrune Is Nothing Then
            booRunPrune = True
        Else
            booRunPrune = DateTime.ParseExact(strLastPrune, strSqlDateFormat, Nothing).AddMonths(1) < Now
        End If

        If booRunPrune Then
            Dim booDoDelete As Boolean
            Dim sqlCheckCmd As SQLiteCommand
            Dim sqlCheckRdr As SQLiteDataReader
            Dim sqlRemoveCmd As SQLiteCommand

            Dim sqlCommand As New SQLiteCommand("select type, station, id, date from tblInfo", sqlConnection)
            Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

            With sqlReader
                While .Read
                    booDoDelete = False

                    sqlCheckCmd = New SQLiteCommand("select count(*) from tblDownloads where type=""" + .GetString(.GetOrdinal("type")) + """ and station=""" + .GetString(.GetOrdinal("station")) + """ and id=""" + .GetString(.GetOrdinal("id")) + """ and date=""" + GetCustFormatDateTime(sqlReader, "date").ToString(strSqlDateFormat) + """", sqlConnection)
                    sqlCheckRdr = sqlCheckCmd.ExecuteReader
                    sqlCheckRdr.Read()

                    If sqlCheckRdr.GetInt32(sqlCheckRdr.GetOrdinal("count(*)")) = 0 Then
                        If LatestDate(New Guid(.GetString(.GetOrdinal("type"))), .GetString(.GetOrdinal("station")), .GetString(.GetOrdinal("id"))) <> GetCustFormatDateTime(sqlReader, "date") Then
                            ' Can be deleted, as it is not the latest info and doesn't relate to a downloaded programme
                            booDoDelete = True
                        Else
                            If GetCustFormatDateTime(sqlReader, "date").AddMonths(3) < Now Then
                                sqlCheckCmd = New SQLiteCommand("select count(*) from tblSubscribed where type=""" + .GetString(.GetOrdinal("type")) + """ and station=""" + .GetString(.GetOrdinal("station")) + """ and id=""" + .GetString(.GetOrdinal("id")) + """", sqlConnection)
                                sqlCheckRdr = sqlCheckCmd.ExecuteReader
                                sqlCheckRdr.Read()

                                If sqlCheckRdr.GetInt32(sqlCheckRdr.GetOrdinal("count(*)")) = 0 Then
                                    ' Can be deleted, as is older than 3 months and doesn't relate to a download or subscription
                                    booDoDelete = True
                                End If
                            End If
                        End If
                    End If

                    If booDoDelete Then
                        sqlRemoveCmd = New SQLiteCommand("delete from tblInfo where type=""" + .GetString(.GetOrdinal("type")) + """ and station=""" + .GetString(.GetOrdinal("station")) + """ and id=""" + .GetString(.GetOrdinal("id")) + """ and date=""" + GetCustFormatDateTime(sqlReader, "date").ToString(strSqlDateFormat) + """", sqlConnection)
                        sqlRemoveCmd.ExecuteNonQuery()
                    End If
                End While

                .Close()
            End With

            SetDBSetting("lastinfoprune", Now.ToString(strSqlDateFormat))
        End If
    End Sub

    Private Sub VacuumDatabase()
        ' Vacuum the database every couple of months - vacuums are spaced like this as they take ages to run
        Dim booRunVacuum As Boolean
        Dim strLastVacuum As String = GetDBSetting("lastvacuum")

        If strLastVacuum Is Nothing Then
            booRunVacuum = True
        Else
            booRunVacuum = DateTime.ParseExact(strLastVacuum, strSqlDateFormat, Nothing).AddMonths(2) < Now
        End If

        If booRunVacuum Then
            ' Make SQLite recreate the database to reduce the size on disk and remove fragmentation
            Dim sqlCommand As New SQLiteCommand("vacuum", sqlConnection)
            sqlCommand.ExecuteNonQuery()

            SetDBSetting("lastvacuum", Now.ToString(strSqlDateFormat))
        End If
    End Sub

    Public Function GetFindNewPanel(ByVal gidPluginID As Guid) As Panel
        If clsPluginsInst.PluginExists(gidPluginID) Then
            FindNewPluginInst = clsPluginsInst.GetPluginInstance(gidPluginID)
            Return FindNewPluginInst.GetFindNewPanel(New clsCachedWebClient(Me))
        Else
            Return New Panel
        End If
    End Function

    Private Sub FindNewPluginInst_FoundNew(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String) Handles FindNewPluginInst.FoundNew
        Dim PluginException As Exception = Nothing
        If GetProgrammeInfo(gidPluginID, strProgExtID, PluginException) = False Then
            If PluginException IsNot Nothing Then
                If MsgBox("A problem was encountered while attempting to retrieve information about this programme." + vbCrLf + "Would you like to report this to NerdoftheHerd.com to help us improve Radio Downloader?", MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation) = MsgBoxResult.Yes Then
                    Dim clsReport As New clsErrorReporting(PluginException.GetType.ToString + ": " + PluginException.Message, PluginException.GetType.ToString + vbCrLf + PluginException.StackTrace)
                    clsReport.SendReport(My.Settings.ErrorReportURL)
                End If

                Exit Sub
            Else
                Call MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation)
                Exit Sub
            End If
        End If

        Dim intProgID As Integer = ExtIDToProgID(gidPluginID, strProgExtID)
        RaiseEvent FoundNew(intProgID)
    End Sub

    Private Function GetProgrammeInfo(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String, ByRef PluginException As Exception) As Boolean
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            Return False
        End If

        Dim ThisInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(gidPluginID)
        Dim ProgInfo As IRadioProvider.ProgrammeInfo

        Try
            ProgInfo = ThisInstance.GetProgrammeInfo(New clsCachedWebClient(Me), strProgExtID)
        Catch PluginException
            ' Catch unhandled errors in the plugin
            Return False
        End Try

        If ProgInfo.Success = False Then
            Return False
        End If

        Dim intProgID As Integer = ExtIDToProgID(gidPluginID, strProgExtID)
        Dim sqlCommand As SQLiteCommand

        If intProgID = Nothing Then
            sqlCommand = New SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", sqlConnection)
            sqlCommand.Parameters.Add(New SQLiteParameter("@pluginid", gidPluginID.ToString))
            sqlCommand.Parameters.Add(New SQLiteParameter("@extid", strProgExtID))
            sqlCommand.ExecuteNonQuery()

            sqlCommand = New SQLiteCommand("select last_insert_rowid()", sqlConnection)
            intProgID = CInt(sqlCommand.ExecuteScalar)
        End If

        Dim intImgID As Integer
        intImgID = StoreImage(ProgInfo.Image)

        sqlCommand = New SQLiteCommand("update programmes set name=@name, description=@description, image=@image, lastupdate=datetime('now') where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@name", ProgInfo.Name))
        sqlCommand.Parameters.Add(New SQLiteParameter("@description", ProgInfo.Description))
        sqlCommand.Parameters.Add(New SQLiteParameter("@image", intImgID))
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        sqlCommand.ExecuteNonQuery()

        Return True
    End Function

    Private Function StoreImage(ByVal bmpImage As Bitmap) As Integer
        If bmpImage Is Nothing Then
            Return Nothing
        End If

        ' Convert the image into a byte array
        Dim mstImage As New MemoryStream()
        bmpImage.Save(mstImage, Imaging.ImageFormat.Bmp)
        Dim bteImage(CInt(mstImage.Length - 1)) As Byte
        mstImage.Position = 0
        mstImage.Read(bteImage, 0, CInt(mstImage.Length))

        Dim sqlCommand As New SQLiteCommand("select imgid from images where image=@image", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@image", bteImage))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read() Then
            Return sqlReader.GetInt32(sqlReader.GetOrdinal("imgid"))
        End If

        sqlCommand = New SQLiteCommand("insert into images (image) values (@image)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@image", bteImage))
        sqlCommand.ExecuteNonQuery()

        sqlCommand = New SQLiteCommand("select last_insert_rowid()", sqlConnection)
        Return CInt(sqlCommand.ExecuteScalar)
    End Function

    Private Function ExtIDToProgID(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String) As Integer
        Dim sqlCommand As New SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@pluginid", gidPluginID.ToString))
        sqlCommand.Parameters.Add(New SQLiteParameter("@extid", strProgExtID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Return sqlReader.GetInt32(sqlReader.GetOrdinal("progid"))
        Else
            Return Nothing
        End If
    End Function

    Public Sub AddToHTTPCache(ByVal strURI As String, ByVal bteData As Byte())
        Dim sqlTransaction As SQLiteTransaction = sqlConnection.BeginTransaction()

        Dim sqlCommand As New SQLiteCommand("delete from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        sqlCommand.ExecuteNonQuery()

        sqlCommand = New SQLiteCommand("insert into httpcache (uri, lastfetch, data) values(@uri, datetime('now'), @data)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        sqlCommand.Parameters.Add(New SQLiteParameter("@data", bteData))
        sqlCommand.ExecuteNonQuery()

        sqlTransaction.Commit()
    End Sub

    Public Function GetHTTPCacheLastUpdate(ByVal strURI As String) As Date
        Dim sqlCommand As New SQLiteCommand("select lastfetch from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Return sqlReader.GetDateTime(sqlReader.GetOrdinal("lastfetch"))
        Else
            Return Nothing
        End If
    End Function

    Public Function GetHTTPCacheContent(ByVal strURI As String) As Byte()
        Dim sqlCommand As New SQLiteCommand("select data from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ' Get the length of the content by passing nothing to getbytes
            Dim intContentLength As Integer = CInt(sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, Nothing, 0, 0))
            Dim bteContent(intContentLength) As Byte

            sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, bteContent, 0, intContentLength)
            Return bteContent
        Else
            Return Nothing
        End If
    End Function
End Class
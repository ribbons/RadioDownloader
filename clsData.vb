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
Imports System.Collections.Generic
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

    Public Sub DownloadSetErrored(ByVal intEpID As Integer, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
        Dim sqlCommand As New SQLiteCommand("update downloads set status=@status, errortime=@errortime, errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Errored))
        sqlCommand.Parameters.Add(New SQLiteParameter("@errortime", Now))
        sqlCommand.Parameters.Add(New SQLiteParameter("@errortype", errType))
        sqlCommand.Parameters.Add(New SQLiteParameter("@errordetails", strErrorDetails))
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub DownloadSetDownloaded(ByVal intEpID As Integer, ByVal strDownloadPath As String)
        Dim sqlCommand As New SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
        sqlCommand.Parameters.Add(New SQLiteParameter("@filepath", strDownloadPath))
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function FindAndDownload() As Boolean
        If thrDownloadThread Is Nothing Then
            Dim sqlCommand As New SQLiteCommand("select pluginid, pr.name as progname, pr.description as progdesc, pr.image as progimg, ep.name as epname, ep.description as epdesc, ep.duration, ep.date, ep.image as epimg, pr.extid as progextid, ep.extid as epextid, dl.status, ep.epid, dl.errorcount from downloads as dl, episodes as ep, programmes as pr where dl.epid=ep.epid and ep.progid=pr.progid and (dl.status=@statuswait or (dl.status=@statuserr and ((dl.errorcount=1 and dl.errortime<@twohoursago) or (dl.errorcount=2 and dl.errortime<@eighthoursago)))) order by ep.date", sqlConnection)
            sqlCommand.Parameters.Add(New SQLiteParameter("@statuswait", Statuses.Waiting))
            sqlCommand.Parameters.Add(New SQLiteParameter("@statuserr", Statuses.Errored))
            sqlCommand.Parameters.Add(New SQLiteParameter("@twohoursago", Now.AddHours(-2)))
            sqlCommand.Parameters.Add(New SQLiteParameter("@eighthoursago", Now.AddHours(-8)))

            Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

            While thrDownloadThread Is Nothing
                If sqlReader.Read Then
                    Dim gidPluginID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))
                    Dim intEpID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))

                    If clsPluginsInst.PluginExists(gidPluginID) Then
                        clsCurDldProgData = New clsDldProgData

                        Dim priProgInfo As IRadioProvider.ProgrammeInfo
                        priProgInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("progname"))
                        priProgInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("progdesc"))
                        priProgInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("progimg")))

                        Dim epiEpInfo As IRadioProvider.EpisodeInfo
                        epiEpInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("epname"))
                        epiEpInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("epdesc"))
                        epiEpInfo.DurationSecs = sqlReader.GetInt32(sqlReader.GetOrdinal("duration"))
                        epiEpInfo.Date = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
                        epiEpInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("epimg")))
                        epiEpInfo.ExtInfo = New Dictionary(Of String, String)

                        Dim sqlExtCommand As New SQLiteCommand("select name, value from episodeext where epid=@epid", sqlConnection)
                        sqlExtCommand.Parameters.Add(New SQLiteParameter("@epid", sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))))
                        Dim sqlExtReader As SQLiteDataReader = sqlExtCommand.ExecuteReader

                        While sqlExtReader.Read
                            epiEpInfo.ExtInfo.Add(sqlExtReader.GetString(sqlExtReader.GetOrdinal("name")), sqlExtReader.GetString(sqlExtReader.GetOrdinal("value")))
                        End While

                        With clsCurDldProgData
                            .PluginID = gidPluginID
                            .ProgExtID = sqlReader.GetString(sqlReader.GetOrdinal("progextid"))
                            .EpID = intEpID
                            .EpisodeExtID = sqlReader.GetString(sqlReader.GetOrdinal("epextid"))
                            .ProgInfo = priProgInfo
                            .EpisodeInfo = epiEpInfo
                        End With

                        clsCurDldProgData.FinalName = FindFreeSaveFileName(My.Settings.FileNameFormat, sqlReader.GetString(sqlReader.GetOrdinal("progname")), sqlReader.GetDateTime(sqlReader.GetOrdinal("date")), GetSaveFolder())
                        clsCurDldProgData.BandwidthLimit = My.Settings.BandwidthLimit

                        If sqlReader.GetInt32(sqlReader.GetOrdinal("status")) = Statuses.Errored Then
                            Call ResetDownload(intEpID, True)
                            clsCurDldProgData.AttemptNumber = sqlReader.GetInt32(sqlReader.GetOrdinal("errorcount")) + 1
                        Else
                            clsCurDldProgData.AttemptNumber = 1
                        End If

                        thrDownloadThread = New Thread(AddressOf DownloadProgThread)
                        thrDownloadThread.Start()

                        FindAndDownload = True
                    End If
                Else
                    Exit While
                End If
            End While

            sqlReader.Close()
        End If
    End Function

    Public Sub DownloadProgThread()
        DownloadPluginInst = clsPluginsInst.GetPluginInstance(clsCurDldProgData.PluginID)

        Try
            With clsCurDldProgData
                DownloadPluginInst.DownloadProgramme(.ProgExtID, .EpisodeExtID, .ProgInfo, .EpisodeInfo, .FinalName, .BandwidthLimit, .AttemptNumber)
            End With
        Catch expUnknown As Exception
            Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.UnknownError, expUnknown.GetType.ToString + ": " + expUnknown.Message + vbCrLf + expUnknown.StackTrace)
        End Try
    End Sub

    Public Function DownloadPath(ByVal intEpID As Integer) As String
        Dim sqlCommand As New SQLiteCommand("select filepath from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            DownloadPath = sqlReader.GetString(sqlReader.GetOrdinal("filepath"))
        Else
            DownloadPath = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function DownloadStatus(ByVal intEpID As Integer) As Statuses
        Dim sqlCommand As New SQLiteCommand("select status from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            DownloadStatus = DirectCast(sqlReader.GetInt32(sqlReader.GetOrdinal("status")), Statuses)
        Else
            DownloadStatus = Nothing
        End If

        sqlReader.Close()
    End Function

    Private Function UpdateProgInfoAsRequired(ByVal intProgID As Integer) As Date
        Dim sqlCommand As New SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim gidProviderID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))

            If clsPluginsInst.PluginExists(gidProviderID) Then
                Dim ThisInstance As IRadioProvider
                ThisInstance = clsPluginsInst.GetPluginInstance(gidProviderID)

                If sqlReader.GetDateTime(sqlReader.GetOrdinal("lastupdate")).AddDays(ThisInstance.ProgInfoUpdateFreqDays) < Now Then
                    Call StoreProgrammeInfo(gidProviderID, sqlReader.GetString(sqlReader.GetOrdinal("extid")), Nothing)
                End If
            End If
        End If

        sqlReader.Close()
    End Function

    Public Function ProgrammeName(ByVal intProgID As Integer) As String
        Call UpdateProgInfoAsRequired(intProgID)

        Dim sqlCommand As New SQLiteCommand("select name from programmes where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ProgrammeName = sqlReader.GetString(sqlReader.GetOrdinal("name"))
        Else
            ProgrammeName = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function ProgrammeDescription(ByVal intProgID As Integer) As String
        Call UpdateProgInfoAsRequired(intProgID)

        Dim sqlCommand As New SQLiteCommand("select description from programmes where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ProgrammeDescription = sqlReader.GetString(sqlReader.GetOrdinal("description"))
        Else
            ProgrammeDescription = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function ProgrammeImage(ByVal intProgID As Integer) As Bitmap
        Call UpdateProgInfoAsRequired(intProgID)

        Dim sqlCommand As New SQLiteCommand("select image from programmes where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim intImgID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("image"))

            If intImgID <> Nothing Then
                ProgrammeImage = RetrieveImage(intImgID)
            Else
                ProgrammeImage = Nothing
            End If
        Else
            ProgrammeImage = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeName(ByVal intEpID As Integer) As String
        Dim sqlCommand As New SQLiteCommand("select name from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            EpisodeName = sqlReader.GetString(sqlReader.GetOrdinal("name"))
        Else
            EpisodeName = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeDescription(ByVal intEpID As Integer) As String
        Dim sqlCommand As New SQLiteCommand("select description from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If sqlReader.IsDBNull(sqlReader.GetOrdinal("description")) Then
                EpisodeDescription = Nothing
            Else
                EpisodeDescription = sqlReader.GetString(sqlReader.GetOrdinal("description"))
            End If
        Else
            EpisodeDescription = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeDate(ByVal intEpID As Integer) As DateTime
        Dim sqlCommand As New SQLiteCommand("select date from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            EpisodeDate = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
        Else
            EpisodeDate = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeDuration(ByVal intEpID As Integer) As Integer
        Dim sqlCommand As New SQLiteCommand("select duration from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            EpisodeDuration = sqlReader.GetInt32(sqlReader.GetOrdinal("duration"))
        Else
            EpisodeDuration = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeImage(ByVal intEpID As Integer) As Bitmap
        Dim sqlCommand As New SQLiteCommand("select image from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim intImgID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("image"))

            If intImgID <> Nothing Then
                EpisodeImage = RetrieveImage(intImgID)
            Else
                EpisodeImage = Nothing
            End If
        Else
            EpisodeImage = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function EpisodeAutoDownload(ByVal intEpID As Integer) As Boolean
        Dim sqlCommand As New SQLiteCommand("select autodownload from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            EpisodeAutoDownload = sqlReader.GetInt32(sqlReader.GetOrdinal("autodownload")) = 1
        Else
            EpisodeAutoDownload = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Sub EpisodeSetAutoDownload(ByVal intEpID As Integer, ByVal booAutoDownload As Boolean)
        Dim intAutoDownload As Integer = 0

        If booAutoDownload Then
            intAutoDownload = 1
        End If

        Dim sqlCommand As New SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.Parameters.Add(New SQLiteParameter("@autodownload", intAutoDownload))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function EpisodeDetails(ByVal intEpID As Integer) As String
        EpisodeDetails = ""

        Dim strDescription As String = EpisodeDescription(intEpID)

        If strDescription <> Nothing Then
            EpisodeDetails += strDescription + vbCrLf + vbCrLf
        End If

        EpisodeDetails += "Date: " + EpisodeDate(intEpID).ToString("ddd dd/MMM/yy HH:mm")

        Dim intDuration As Integer = EpisodeDuration(intEpID)

        If intDuration <> Nothing Then
            EpisodeDetails += vbCrLf + "Duration: "

            intDuration = intDuration \ 60
            Dim intHours As Integer = intDuration \ 60
            Dim intMins As Integer = intDuration Mod 60

            If intHours > 0 Then
                EpisodeDetails += CStr(intHours) + "hr"

                If intHours > 1 Then
                    EpisodeDetails += "s"
                End If
            End If

            If intHours > 0 And intMins > 0 Then
                EpisodeDetails += " "
            End If

            If intMins > 0 Then
                EpisodeDetails += CStr(intMins) + "min"
            End If
        End If
    End Function

    Public Sub UpdateDlList(ByRef lstListview As ExtListView, ByRef prgProgressBar As ProgressBar)
        Dim comCommand As New SQLiteCommand("select episodes.epid, name, date, status, playcount from episodes, downloads where episodes.epid=downloads.epid order by date desc", sqlConnection)
        Dim sqlReader As SQLiteDataReader = comCommand.ExecuteReader()

        lstListview.RemoveAllControls()

        Dim lstItem As ListViewItem
        Dim booErrorStatus As Boolean = False
        Dim intExistingPos As Integer = 0

        Do While sqlReader.Read
            Dim intEpID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))

            If lstListview.Items.Count - 1 < intExistingPos Then
                lstItem = lstListview.Items.Add(CStr(intEpID), "", 0)
            Else
                While lstListview.Items.ContainsKey(CStr(intEpID)) And CInt(lstListview.Items(intExistingPos).Name) <> intEpID
                    lstListview.Items.RemoveAt(intExistingPos)
                End While

                If CInt(lstListview.Items(intExistingPos).Name) = intEpID Then
                    lstItem = lstListview.Items(intExistingPos)
                Else
                    lstItem = lstListview.Items.Insert(intExistingPos, CStr(intEpID), "", 0)
                End If
            End If

            intExistingPos += 1

            lstItem.SubItems.Clear()
            lstItem.Name = CStr(intEpID)
            lstItem.Text = sqlReader.GetString(sqlReader.GetOrdinal("name"))

            lstItem.SubItems.Add(sqlReader.GetDateTime(sqlReader.GetOrdinal("date")).ToShortDateString())

            Select Case sqlReader.GetInt32(sqlReader.GetOrdinal("status"))
                Case Statuses.Waiting
                    lstItem.SubItems.Add("Waiting")
                    lstItem.ImageKey = "waiting"
                Case Statuses.Downloaded
                    If sqlReader.GetInt32(sqlReader.GetOrdinal("playcount")) > 0 Then
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
                'strDynamicTitle = ProgramTitle(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")), LatestDate(gidPluginID, .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID"))))

                'If ProviderDynamicSubscriptionName(gidPluginID) Then
                lstAdd.Text = strDynamicTitle
                'Else
                'lstAdd.Text = SubscriptionName(.GetString(.GetOrdinal("Type")), .GetString(.GetOrdinal("Station")), .GetString(.GetOrdinal("ID")))

                'If lstAdd.Text = "" Then
                'lstAdd.Text = strDynamicTitle
                'End If
                'End If

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

    Public Sub ListEpisodes(ByVal intProgID As Integer, ByRef lstListview As ListView)
        Dim intAvailable As Integer()
        intAvailable = GetAvailableEpisodes(intProgID)
        Array.Reverse(intAvailable)

        lstListview.Items.Clear()

        For Each intEpID As Integer In intAvailable
            Dim lstAdd As New ListViewItem

            lstAdd.Text = EpisodeDate(intEpID).ToShortDateString
            lstAdd.SubItems.Add(EpisodeName(intEpID))
            lstAdd.Checked = EpisodeAutoDownload(intEpID)
            lstAdd.Tag = intEpID

            lstListview.Items.Add(lstAdd)
        Next
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

    Public Function AddDownload(ByVal intEpID As Integer) As Boolean
        Dim sqlCommand As New SQLiteCommand("select epid from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Return False
        End If

        sqlReader.Close()

        sqlCommand = New SQLiteCommand("insert into downloads (epid, status) values (@epid, @status)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Waiting))
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

        'If clsPluginsInst.PluginExists(gidProviderID) Then
        '    Dim ThisInstance As IRadioProvider
        '    ThisInstance = clsPluginsInst.GetPluginInstance(gidProviderID)
        '    Return ThisInstance.CouldBeNewEpisode(strStationID, strProgramID, dteLatest) = False
        'Else
        '    ' As we can't check if we have the latest, just assume that we do for the moment
        Return True
        'End If
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

    Public Sub ResetDownload(ByVal intEpID As Integer, ByVal booAuto As Boolean)
        Dim sqlCommand As New SQLiteCommand("update downloads set status=@status where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Waiting))
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.ExecuteNonQuery()

        If booAuto = False Then
            sqlCommand = New SQLiteCommand("update downloads set errorcount=0 where epid=@epid", sqlConnection)
            sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
            sqlCommand.ExecuteNonQuery()
        End If
    End Sub

    Public Sub RemoveDownload(ByVal intEpID As Integer)
        Dim sqlCommand As New SQLiteCommand("delete from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function ProviderName(ByVal gidPluginID As Guid) As String
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            Return ""
        End If

        Dim ThisInstance As IRadioProvider
        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        Return ThisInstance.ProviderName
    End Function

    Public Sub DownloadBumpPlayCount(ByVal intEpID As Integer)
        Dim sqlCommand As New SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function DownloadPlayCount(ByVal intEpID As Integer) As Integer
        Dim sqlCommand As New SQLiteCommand("select playcount from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            DownloadPlayCount = sqlReader.GetInt32(sqlReader.GetOrdinal("playcount"))
        Else
            DownloadPlayCount = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function DownloadErrorType(ByVal intEpID As Integer) As IRadioProvider.ErrorType
        Dim sqlCommand As New SQLiteCommand("select errortype from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim intErrorType As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("ErrorType"))

            If intErrorType <> Nothing Then
                DownloadErrorType = CType(intErrorType, IRadioProvider.ErrorType)
            Else
                DownloadErrorType = IRadioProvider.ErrorType.UnknownError
            End If
        Else
            DownloadErrorType = IRadioProvider.ErrorType.UnknownError
        End If

        sqlReader.Close()
    End Function

    Public Function DownloadErrorDetails(ByVal intEpID As Integer) As String
        Dim sqlCommand As New SQLiteCommand("select errordetails from downloads where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim strErrorDetails As String = sqlReader.GetString(sqlReader.GetOrdinal("errordetails"))

            If strErrorDetails IsNot Nothing Then
                DownloadErrorDetails = strErrorDetails
            Else
                DownloadErrorDetails = ""
            End If
        Else
            DownloadErrorDetails = ""
        End If

        sqlReader.Close()
    End Function

    Private Sub DownloadPluginInst_DldError(ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String) Handles DownloadPluginInst.DldError
        RaiseEvent DldError(clsCurDldProgData, errType, strErrorDetails)
        thrDownloadThread = Nothing
        clsCurDldProgData = Nothing
    End Sub

    Private Sub DownloadPluginInst_Finished(ByVal strFileExtension As String) Handles DownloadPluginInst.Finished
        clsCurDldProgData.FinalName += "." + strFileExtension

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

    Public Sub PerformCleanup()
        Dim sqlCommand As New SQLiteCommand("select epid, filepath from downloads where status=@status", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        With sqlReader
            Do While .Read
                ' Remove programmes for which the associated audio file no longer exists
                If Exists(.GetString(.GetOrdinal("filepath"))) = False Then
                    Dim intEpID As Integer = .GetInt32(.GetOrdinal("epid"))

                    ' Take the download out of the list
                    RemoveDownload(intEpID)

                    ' Set the auto download flag of this episode to false, so if we are subscribed to the programme
                    ' it doesn't just download it all over again
                    Call EpisodeSetAutoDownload(intEpID, False)
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

    Private Sub FindNewPluginInst_FoundNew(ByVal strProgExtID As String) Handles FindNewPluginInst.FoundNew
        Dim gidPluginID As Guid = FindNewPluginInst.ProviderID
        Dim PluginException As Exception = Nothing

        If StoreProgrammeInfo(gidPluginID, strProgExtID, PluginException) = False Then
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

    Private Function StoreProgrammeInfo(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String, ByRef PluginException As Exception) As Boolean
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

        sqlCommand = New SQLiteCommand("update programmes set name=@name, description=@description, image=@image, lastupdate=@lastupdate where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@name", ProgInfo.Name))
        sqlCommand.Parameters.Add(New SQLiteParameter("@description", ProgInfo.Description))
        sqlCommand.Parameters.Add(New SQLiteParameter("@image", StoreImage(ProgInfo.Image)))
        sqlCommand.Parameters.Add(New SQLiteParameter("@lastupdate", Now))
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

    Private Function RetrieveImage(ByVal intImgID As Integer) As Bitmap
        Dim sqlCommand As New SQLiteCommand("select image from images where imgid=@imgid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@imgid", intImgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ' Get the size of the image data by passing nothing to getbytes
            Dim intDataLength As Integer = CInt(sqlReader.GetBytes(sqlReader.GetOrdinal("image"), 0, Nothing, 0, 0))
            Dim bteContent(intDataLength - 1) As Byte

            sqlReader.GetBytes(sqlReader.GetOrdinal("image"), 0, bteContent, 0, intDataLength)
            RetrieveImage = New Bitmap(New MemoryStream(bteContent))
        Else
            RetrieveImage = Nothing
        End If

        sqlReader.Close()
    End Function

    Private Function ExtIDToProgID(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String) As Integer
        Dim sqlCommand As New SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@pluginid", gidPluginID.ToString))
        sqlCommand.Parameters.Add(New SQLiteParameter("@extid", strProgExtID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ExtIDToProgID = sqlReader.GetInt32(sqlReader.GetOrdinal("progid"))
        Else
            ExtIDToProgID = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Sub AddToHTTPCache(ByVal strURI As String, ByVal bteData As Byte())
        Dim sqlCommand As New SQLiteCommand("delete from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        sqlCommand.ExecuteNonQuery()

        sqlCommand = New SQLiteCommand("insert into httpcache (uri, lastfetch, data) values(@uri, @lastfetch, @data)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        sqlCommand.Parameters.Add(New SQLiteParameter("@lastfetch", Now))
        sqlCommand.Parameters.Add(New SQLiteParameter("@data", bteData))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function GetHTTPCacheLastUpdate(ByVal strURI As String) As Date
        Dim sqlCommand As New SQLiteCommand("select lastfetch from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            GetHTTPCacheLastUpdate = sqlReader.GetDateTime(sqlReader.GetOrdinal("lastfetch"))
        Else
            GetHTTPCacheLastUpdate = Nothing
        End If

        sqlReader.Close()
    End Function

    Public Function GetHTTPCacheContent(ByVal strURI As String) As Byte()
        Dim sqlCommand As New SQLiteCommand("select data from httpcache where uri=@uri", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@uri", strURI))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            ' Get the length of the content by passing nothing to getbytes
            Dim intContentLength As Integer = CInt(sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, Nothing, 0, 0))
            Dim bteContent(intContentLength - 1) As Byte

            sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, bteContent, 0, intContentLength)
            GetHTTPCacheContent = bteContent
        Else
            GetHTTPCacheContent = Nothing
        End If

        sqlReader.Close()
    End Function

    Private Function GetAvailableEpisodes(ByVal intProgID As Integer) As Integer()
        Dim intEpisodeIDs(-1) As Integer
        GetAvailableEpisodes = intEpisodeIDs

        Dim sqlCommand As New SQLiteCommand("select pluginid, extid from programmes where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read = False Then
            Exit Function
        End If

        Dim gidProviderID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))
        Dim strProgExtID As String = sqlReader.GetString(sqlReader.GetOrdinal("extid"))

        sqlReader.Close()

        If clsPluginsInst.PluginExists(gidProviderID) = False Then
            Exit Function
        End If

        Dim strEpisodeExtIDs As String()
        Dim clsCachedWebInst As New clsCachedWebClient(Me)
        Dim ThisInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(gidProviderID)

        Try
            strEpisodeExtIDs = ThisInstance.GetAvailableEpisodeIDs(clsCachedWebInst, strProgExtID)
        Catch expException As Exception
            ' Catch any unhandled provider exceptions
            Exit Function
        End Try

        Dim EpisodeInfo As IRadioProvider.EpisodeInfo

        If strEpisodeExtIDs IsNot Nothing Then
            ' Reverse the array so that we fetch the oldest episodes first, and the older episodes
            ' get added to the download list first if we are checking subscriptions
            Array.Reverse(strEpisodeExtIDs)

            Dim sqlFindCmd As New SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", sqlConnection)
            Dim sqlAddEpisodeCmd As New SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", sqlConnection)
            Dim sqlGetRowIDCmd As New SQLiteCommand("select last_insert_rowid()", sqlConnection)
            Dim sqlAddExtInfoCmd As New SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", sqlConnection)

            For Each strEpisodeExtID As String In strEpisodeExtIDs
                With sqlFindCmd
                    .Parameters.Add(New SQLiteParameter("@progid", intProgID))
                    .Parameters.Add(New SQLiteParameter("@extid", strEpisodeExtID))
                    sqlReader = .ExecuteReader
                End With

                If sqlReader.Read Then
                    ReDim Preserve intEpisodeIDs(intEpisodeIDs.GetUpperBound(0) + 1)
                    intEpisodeIDs(intEpisodeIDs.GetUpperBound(0)) = sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))
                Else
                    Try
                        EpisodeInfo = ThisInstance.GetEpisodeInfo(clsCachedWebInst, strProgExtID, strEpisodeExtID)
                    Catch expException As Exception
                        ' Catch any unhandled provider exceptions
                        sqlReader.Close()
                        Continue For
                    End Try

                    If EpisodeInfo.Success = False Then
                        sqlReader.Close()
                        Continue For
                    End If

                    If EpisodeInfo.Name = "" Or EpisodeInfo.Date = Nothing Then
                        sqlReader.Close()
                        Continue For
                    End If

                    With sqlAddEpisodeCmd
                        .Parameters.Add(New SQLiteParameter("@progid", intProgID))
                        .Parameters.Add(New SQLiteParameter("@extid", strEpisodeExtID))
                        .Parameters.Add(New SQLiteParameter("@name", EpisodeInfo.Name))
                        .Parameters.Add(New SQLiteParameter("@description", EpisodeInfo.Description))
                        .Parameters.Add(New SQLiteParameter("@duration", EpisodeInfo.DurationSecs))
                        .Parameters.Add(New SQLiteParameter("@date", EpisodeInfo.Date))
                        .Parameters.Add(New SQLiteParameter("@image", StoreImage(EpisodeInfo.Image)))
                        .ExecuteNonQuery()
                    End With

                    Dim intEpID As Integer = CInt(sqlGetRowIDCmd.ExecuteScalar)

                    If EpisodeInfo.ExtInfo IsNot Nothing Then
                        For Each kvpItem As KeyValuePair(Of String, String) In EpisodeInfo.ExtInfo
                            With sqlAddExtInfoCmd
                                .Parameters.Add(New SQLiteParameter("@epid", intEpID))
                                .Parameters.Add(New SQLiteParameter("@name", kvpItem.Key))
                                .Parameters.Add(New SQLiteParameter("@value", kvpItem.Value))
                                .ExecuteNonQuery()
                            End With
                        Next
                    End If

                    ReDim Preserve intEpisodeIDs(intEpisodeIDs.GetUpperBound(0) + 1)
                    intEpisodeIDs(intEpisodeIDs.GetUpperBound(0)) = intEpID
                End If

                sqlReader.Close()
            Next
        End If

        Return intEpisodeIDs
    End Function
End Class
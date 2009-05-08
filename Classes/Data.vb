' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2009 Matt Robinson
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

Friend Class Data
    Public Enum Statuses
        Waiting
        Downloaded
        Errored
    End Enum

    Private Shared clsDataInstance As Data

    Private sqlConnection As SQLiteConnection
    Private clsPluginsInst As Plugins

    Private clsCurDldProgData As DldProgData
    Private thrDownloadThread As Thread
    Private WithEvents DownloadPluginInst As IRadioProvider
    Private WithEvents FindNewPluginInst As IRadioProvider

    Public Event FindNewViewChange(ByVal objView As Object)
    Public Event FoundNew(ByVal intProgID As Integer)
    Public Event Progress(ByVal clsCurDldProgData As DldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    Public Event DldError(ByVal clsCurDldProgData As DldProgData, ByVal errType As IRadioProvider.ErrorType, ByVal strErrorDetails As String)
    Public Event Finished(ByVal clsCurDldProgData As DldProgData)

    Public Shared Function GetInstance() As Data
        If clsDataInstance Is Nothing Then
            clsDataInstance = New Data
        End If

        Return clsDataInstance
    End Function

    Private Sub New()
        MyBase.New()

        sqlConnection = New SQLiteConnection("Data Source=" + GetAppDataFolder() + "\store.db;Version=3;New=False")
        sqlConnection.Open()

        ' Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
        Call VacuumDatabase()

        ' Create the temp table for caching HTTP requests
        Dim sqlCommand As New SQLiteCommand("create temporary table httpcache (uri varchar (1000), lastfetch datetime, data blob)", sqlConnection)
        sqlCommand.ExecuteNonQuery()

        ' Setup an instance of the plugins class
        clsPluginsInst = New Plugins(My.Application.Info.DirectoryPath)


        ' Fetch the version of the database
        Dim intCurrentVer As Integer

        If GetDBSetting("databaseversion") Is Nothing Then
            intCurrentVer = 1
        Else
            intCurrentVer = CInt(GetDBSetting("databaseversion"))
        End If

        ' Set the current database version.  This is done before the upgrades are attempted so that
        ' if the upgrade throws an exception this can be reported, but the programme will run next time.
        ' NB: Remember to change default version in the database if this next line is changed!
        SetDBSetting("databaseversion", 2)

        Select Case intCurrentVer
            Case 1
                Call UpgradeDBv1to2()
            Case 2
                ' Nothing to do, this is the current version.
        End Select
    End Sub

    Private Sub UpgradeDBv1to2()
        Dim sqlCommand As New SQLiteCommand("select count(*) from tblDownloads", sqlConnection)
        Dim intCount As Integer = 0
        Dim intTotal As Integer = CInt(sqlCommand.ExecuteScalar)

        Status.lblStatus.Text = "Migrating downloads..."
        Status.prgProgress.Visible = True
        Status.prgProgress.Maximum = intTotal
        Status.prgProgress.Value = 0
        Status.Visible = True
        Application.DoEvents()

        ' Migrate the downloads
        sqlCommand = New SQLiteCommand("select inf.name, inf.description, inf.duration, inf.image, dld.date as dlddate, dld.path, dld.playcount from tblDownloads as dld, tblInfo as inf where dld.type=inf.type and dld.id=inf.id and inf.date=dld.date and inf.station=dld.station and status=1", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        Dim intImage As Integer
        Dim intEpID As Integer
        Dim intDataLength As Integer

        Dim sqlCheckEpisode As New SQLiteCommand("select epid from episodes where name=@name and description=@description and duration=@duration and date=@date", sqlConnection)
        Dim sqlCheckEpRdr As SQLiteDataReader
        Dim sqlMigrateInf As New SQLiteCommand("insert into episodes (name, description, duration, date, image) values (@name, @description, @duration, @date, @image)", sqlConnection)
        Dim sqlGetRowIDCmd As New SQLiteCommand("select last_insert_rowid()", sqlConnection)
        Dim sqlCheckDld As New SQLiteCommand("select count(*) from downloads where epid=@epid", sqlConnection)
        Dim sqlMigrateDld As New SQLiteCommand("insert into downloads (epid, status, filepath, playcount) values (@epid, @status, @filepath, @playcount)", sqlConnection)

        With sqlReader
            While .Read
                Status.lblStatus.Text = "Migrating download " + CStr(intCount) + " of " + CStr(intTotal) + "..."
                Status.prgProgress.Value = intCount
                Application.DoEvents()

                If sqlReader.IsDBNull(sqlReader.GetOrdinal("image")) Then
                    intImage = Nothing
                Else
                    ' Get the image as a bitmap
                    intDataLength = CInt(sqlReader.GetBytes(sqlReader.GetOrdinal("image"), 0, Nothing, 0, 0))
                    Dim bteContent(intDataLength - 1) As Byte
                    sqlReader.GetBytes(sqlReader.GetOrdinal("image"), 0, bteContent, 0, intDataLength)
                    intImage = StoreImage(New Bitmap(New MemoryStream(bteContent)))
                End If

                sqlCheckEpisode.Parameters.Add(New SQLiteParameter("@name", .GetString(.GetOrdinal("name"))))
                sqlCheckEpisode.Parameters.Add(New SQLiteParameter("@description", .GetString(.GetOrdinal("description"))))
                sqlCheckEpisode.Parameters.Add(New SQLiteParameter("@duration", .GetInt32(.GetOrdinal("duration")) * 60))
                sqlCheckEpisode.Parameters.Add(New SQLiteParameter("@date", UpgradeDBv1to2CustDateFmt(sqlReader, "dlddate")))
                sqlCheckEpRdr = sqlCheckEpisode.ExecuteReader

                If sqlCheckEpRdr.Read = False Then
                    sqlMigrateInf.Parameters.Add(New SQLiteParameter("@name", .GetString(.GetOrdinal("name"))))
                    sqlMigrateInf.Parameters.Add(New SQLiteParameter("@description", .GetString(.GetOrdinal("description"))))
                    sqlMigrateInf.Parameters.Add(New SQLiteParameter("@duration", .GetInt32(.GetOrdinal("duration")) * 60))
                    sqlMigrateInf.Parameters.Add(New SQLiteParameter("@date", UpgradeDBv1to2CustDateFmt(sqlReader, "dlddate")))
                    sqlMigrateInf.Parameters.Add(New SQLiteParameter("@image", intImage))
                    sqlMigrateInf.ExecuteNonQuery()

                    intEpID = CInt(sqlGetRowIDCmd.ExecuteScalar)
                Else
                    intEpID = sqlCheckEpRdr.GetInt32(sqlCheckEpRdr.GetOrdinal("epid"))
                End If

                sqlCheckEpRdr.Close()

                sqlCheckDld.Parameters.Add(New SQLiteParameter("@epid", intEpID))
                If CInt(sqlCheckDld.ExecuteScalar) = 0 Then
                    sqlMigrateDld.Parameters.Add(New SQLiteParameter("@epid", intEpID))
                    sqlMigrateDld.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
                    sqlMigrateDld.Parameters.Add(New SQLiteParameter("@filepath", .GetString(.GetOrdinal("path"))))
                    sqlMigrateDld.Parameters.Add(New SQLiteParameter("@playcount", .GetInt32(.GetOrdinal("playcount"))))
                    sqlMigrateDld.ExecuteNonQuery()
                End If

                intCount += 1
            End While

            .Close()
        End With

        Status.lblStatus.Text = "Performing cleanup.."
        Status.prgProgress.Visible = False
        Application.DoEvents()

        ' Delete all of the images stored in the info table, so the old data doesn't take up loads of space.
        Dim sqlReduceClutter As New SQLiteCommand("update tblInfo set image=null", sqlConnection)
        sqlReduceClutter.ExecuteNonQuery()

        sqlCommand = New SQLiteCommand("select count(*) from tblSubscribed", sqlConnection)
        intCount = -1
        intTotal = CInt(sqlCommand.ExecuteScalar)

        Status.lblStatus.Text = "Attempting to migrate subscriptions..."
        Status.prgProgress.Visible = True
        Status.prgProgress.Maximum = intTotal
        Status.prgProgress.Value = 0
        Application.DoEvents()

        ' Then migrate the subscriptions
        sqlCommand = New SQLiteCommand("select type, id from tblSubscribed", sqlConnection)
        sqlReader = sqlCommand.ExecuteReader

        With sqlReader
            While .Read
                intCount += 1
                Status.lblStatus.Text = "Attempting to migrate subscription " + CStr(intCount) + " of " + CStr(intTotal) + "..."
                Status.prgProgress.Value = intCount
                Application.DoEvents()

                Select Case .GetString(.GetOrdinal("type"))
                    Case "BBCPODCAST"
                        Dim gidPluginID As New Guid("3cfbe63e-95b8-4f80-8570-4ace909e0921")
                        Dim strExtID As String = "http://downloads.bbc.co.uk/podcasts/" + .GetString(.GetOrdinal("id")) + "/rss.xml"

                        If StoreProgrammeInfo(gidPluginID, strExtID, New Exception) Then
                            Dim intProgID As Integer = ExtIDToProgID(gidPluginID, strExtID)

                            If intProgID <> Nothing Then
                                AddSubscription(intProgID)
                            End If
                        End If
                End Select
            End While

            .Close()
        End With

        Status.Visible = False
        Application.DoEvents()
    End Sub

    Private Function UpgradeDBv1to2CustDateFmt(ByVal sqlReader As SQLiteDataReader, ByVal strColumn As String) As Date
        Try
            Return DateTime.ParseExact(sqlReader.GetString(sqlReader.GetOrdinal(strColumn)), "yyyy-MM-dd HH:mm", Nothing)
        Catch expException As InvalidCastException
            ' Somehow we have a value which can't be cast as a date, return nothing
            Return Nothing
        Catch expException As FormatException
            ' Somehow we have a valid date string of the wrong format, return nothing
            Return Nothing
        End Try
    End Function

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
                        clsCurDldProgData = New DldProgData

                        Dim priProgInfo As IRadioProvider.ProgrammeInfo
                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("progname")) Then
                            priProgInfo.Name = Nothing
                        Else
                            priProgInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("progname"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("progdesc")) Then
                            priProgInfo.Description = Nothing
                        Else
                            priProgInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("progdesc"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("progimg")) Then
                            priProgInfo.Image = Nothing
                        Else
                            priProgInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("progimg")))
                        End If

                        Dim epiEpInfo As IRadioProvider.EpisodeInfo
                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("epname")) Then
                            epiEpInfo.Name = Nothing
                        Else
                            epiEpInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("epname"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("epdesc")) Then
                            epiEpInfo.Description = Nothing
                        Else
                            epiEpInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("epdesc"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("duration")) Then
                            epiEpInfo.DurationSecs = Nothing
                        Else
                            epiEpInfo.DurationSecs = sqlReader.GetInt32(sqlReader.GetOrdinal("duration"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("date")) Then
                            epiEpInfo.Date = Nothing
                        Else
                            epiEpInfo.Date = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
                        End If

                        If sqlReader.IsDBNull(sqlReader.GetOrdinal("epimg")) Then
                            epiEpInfo.Image = Nothing
                        Else
                            epiEpInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("epimg")))
                        End If

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

                        clsCurDldProgData.FinalName = FindFreeSaveFileName(My.Settings.FileNameFormat, sqlReader.GetString(sqlReader.GetOrdinal("progname")), EpisodeName(intEpID), sqlReader.GetDateTime(sqlReader.GetOrdinal("date")), GetSaveFolder())
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

            If intImgID = Nothing Then
                ' Find the id of the latest episode's image
                Dim sqlLatestCmd As New SQLiteCommand("select image from episodes where progid=@progid and image notnull order by date desc limit 1", sqlConnection)
                sqlLatestCmd.Parameters.Add(New SQLiteParameter("@progid", intProgID))
                Dim sqlLatestRdr As SQLiteDataReader = sqlLatestCmd.ExecuteReader

                If sqlLatestRdr.Read Then
                    intImgID = sqlLatestRdr.GetInt32(sqlReader.GetOrdinal("image"))
                End If

                sqlLatestRdr.Close()
            End If

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

    Public Function EpisodeExists(ByVal intEpID As Integer) As Boolean
        Dim sqlCommand As New SQLiteCommand("select count(*) from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))

        Return CInt(sqlCommand.ExecuteScalar) > 0
    End Function

    Public Function EpisodeName(ByVal intEpID As Integer) As String
        Dim sqlCommand As New SQLiteCommand("select name, date from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            If sqlReader.IsDBNull(sqlReader.GetOrdinal("name")) Then
                EpisodeName = Nothing
            Else
                EpisodeName = sqlReader.GetString(sqlReader.GetOrdinal("name"))

                If sqlReader.IsDBNull(sqlReader.GetOrdinal("date")) = False Then
                    Dim dteEpisodeDate As Date = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))

                    ' Use regex to remove a number of different date formats from programme titles.
                    ' Will only remove dates with the same month & year as the programme itself, but any day of the month
                    ' as there is sometimes a mismatch of a day or two between the date in a title and the publish date.
                    Dim regStripDate As New Regex("\A(" + dteEpisodeDate.ToString("yyyy") + "/" + dteEpisodeDate.ToString("MM") + "/\d{2} ?-? )?(?<name>.*?)( ?:? (\d{2}/" + dteEpisodeDate.ToString("MM") + "/" + dteEpisodeDate.ToString("yyyy") + "|((Mon|Tue|Wed|Thu|Fri) )?(\d{1,2}(st|nd|rd|th)? )?(" + dteEpisodeDate.ToString("MMMM") + "|" + dteEpisodeDate.ToString("MMM") + ")( \d{1,2}(st|nd|rd|th)?| (" + dteEpisodeDate.ToString("yy") + "|" + dteEpisodeDate.ToString("yyyy") + "))?))?\Z")

                    If regStripDate.IsMatch(EpisodeName) Then
                        EpisodeName = regStripDate.Match(EpisodeName).Groups("name").ToString
                    End If
                End If
            End If
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
        Dim sqlCommand As New SQLiteCommand("select image, progid from episodes where epid=@epid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read Then
            Dim intImgID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("image"))

            If intImgID <> Nothing Then
                EpisodeImage = RetrieveImage(intImgID)
            Else
                EpisodeImage = Nothing
            End If

            If EpisodeImage Is Nothing Then
                If sqlReader.IsDBNull(sqlReader.GetOrdinal("progid")) = False Then
                    EpisodeImage = ProgrammeImage(sqlReader.GetInt32(sqlReader.GetOrdinal("progid")))
                End If
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

    Public Function CountDownloadsNew() As Integer
        Dim sqlCommand As New SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
        Return CInt(sqlCommand.ExecuteScalar())
    End Function

    Public Function CountDownloadsErrored() As Integer
        Dim sqlCommand As New SQLiteCommand("select count(epid) from downloads where status=@status", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Errored))
        Return CInt(sqlCommand.ExecuteScalar())
    End Function

    Public Sub UpdateDlList(ByRef lstListview As ExtListView)
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
            lstItem.Text = EpisodeName(intEpID)

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
            Main.SetTrayStatus(False, Main.ErrorStatus.Error)
        Else
            Main.SetTrayStatus(False, Main.ErrorStatus.Normal)
        End If

        sqlReader.Close()
    End Sub

    Public Sub UpdateSubscrList(ByRef lstListview As ExtListView)
        Dim lstAdd As ListViewItem

        Dim sqlCommand As New SQLiteCommand("select pluginid, name, subscriptions.progid from subscriptions, programmes where subscriptions.progid=programmes.progid order by name", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        lstListview.Items.Clear()

        With sqlReader
            Do While .Read()
                Dim gidPluginID As New Guid(.GetString(.GetOrdinal("pluginid")))
                lstAdd = New ListViewItem

                lstAdd.Text = .GetString(.GetOrdinal("name"))

                Dim dteLastDownload As Date = LatestDownloadDate(.GetInt32(.GetOrdinal("progid")))

                If dteLastDownload = Nothing Then
                    lstAdd.SubItems.Add("Never")
                Else
                    lstAdd.SubItems.Add(dteLastDownload.ToShortDateString)
                End If

                lstAdd.SubItems.Add(ProviderName(gidPluginID))
                lstAdd.Tag = .GetInt32(.GetOrdinal("progid"))
                lstAdd.ImageKey = "subscribed"

                lstListview.Items.Add(lstAdd)
            Loop
        End With

        sqlReader.Close()
    End Sub

    Public Sub ListEpisodes(ByVal intProgID As Integer, ByRef lstListview As ExtListView)
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

    Public Sub CheckSubscriptions(ByRef lstList As ExtListView)
        Dim sqlCommand As New SQLiteCommand("select progid from subscriptions", sqlConnection)
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        With sqlReader
            Do While .Read()
                Dim intAvailableEps() As Integer
                intAvailableEps = GetAvailableEpisodes(.GetInt32(.GetOrdinal("progid")))

                For Each intEpID As Integer In intAvailableEps
                    If EpisodeAutoDownload(intEpID) Then
                        Dim sqlCheckCmd As New SQLiteCommand("select epid from downloads where epid=@epid", sqlConnection)
                        sqlCheckCmd.Parameters.Add(New SQLiteParameter("@epid", intEpID))
                        Dim sqlCheckRdr As SQLiteDataReader = sqlCheckCmd.ExecuteReader

                        If sqlCheckRdr.Read = False Then
                            Call AddDownload(intEpID)
                            Call UpdateDlList(lstList)
                        End If

                        sqlCheckRdr.Close()
                    End If
                Next
            Loop
        End With

        sqlReader.Close()
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

    Public Function IsSubscribed(ByVal intProgID As Integer) As Boolean
        Dim sqlCommand As New SQLiteCommand("select progid from subscriptions where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        Return sqlReader.Read
    End Function

    Public Sub AddSubscription(ByVal intProgID As Integer)
        If IsSubscribed(intProgID) Then
            Return
        End If

        Dim sqlCommand As New SQLiteCommand("insert into subscriptions (progid) values (@progid)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Public Sub RemoveSubscription(ByVal intProgID As Integer)
        Dim sqlCommand As New SQLiteCommand("delete from subscriptions where progid=@progid", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Call sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function LatestDownloadDate(ByVal intProgID As Integer) As DateTime
        Dim sqlCommand As New SQLiteCommand("select date from episodes, downloads where episodes.epid=downloads.epid and progid=@progid order by date desc limit 1", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        If sqlReader.Read = False Then
            ' No downloads of this program, return nothing
            LatestDownloadDate = Nothing
        Else
            LatestDownloadDate = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
        End If

        sqlReader.Close()
    End Function

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

    Public Function ProviderDescription(ByVal gidPluginID As Guid) As String
        If clsPluginsInst.PluginExists(gidPluginID) = False Then
            Return ""
        End If

        Dim ThisInstance As IRadioProvider
        ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

        Return ThisInstance.ProviderDescription
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

    Public Function GetCurrentDownloadInfo() As DldProgData
        Return clsCurDldProgData
    End Function

    Public Sub UpdateProviderList(ByVal lstProviders As ListView, ByVal imlProviders As ImageList)
        Dim gidPluginIDs() As Guid
        gidPluginIDs = clsPluginsInst.GetPluginIdList

        Dim ThisInstance As IRadioProvider
        Dim lstAdd As ListViewItem

        For Each gidPluginID As Guid In gidPluginIDs
            ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

            lstAdd = New ListViewItem
            lstAdd.Text = ThisInstance.ProviderName
            lstAdd.Tag = gidPluginID

            Dim bmpIcon As Bitmap = ThisInstance.ProviderIcon

            If bmpIcon IsNot Nothing Then
                imlProviders.Images.Add(gidPluginID.ToString, bmpIcon)
                lstAdd.ImageKey = gidPluginID.ToString
            Else
                lstAdd.ImageKey = "default"
            End If

            lstProviders.Items.Add(lstAdd)
        Next
    End Sub

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

    Private Sub SetDBSetting(ByVal strPropertyName As String, ByVal objValue As Object)
        Dim sqlCommand As New SQLiteCommand("delete from settings where property=@property", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@property", strPropertyName))
        sqlCommand.ExecuteNonQuery()

        sqlCommand = New SQLiteCommand("insert into settings (property, value) VALUES (@property, @value)", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@property", strPropertyName))
        sqlCommand.Parameters.Add(New SQLiteParameter("@value", objValue))
        sqlCommand.ExecuteNonQuery()
    End Sub

    Private Function GetDBSetting(ByVal strPropertyName As String) As Object
        Dim sqlCommand As New SQLiteCommand("select value from settings where property=@property", sqlConnection)
        sqlCommand.Parameters.Add(New SQLiteParameter("@property", strPropertyName))
        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        If sqlReader.Read = False Then
            Return Nothing
        End If

        GetDBSetting = sqlReader.GetValue(sqlReader.GetOrdinal("value"))
        sqlReader.Close()
    End Function

    Private Sub VacuumDatabase()
        ' Vacuum the database every few months - vacuums are spaced like this as they take ages to run
        Dim booRunVacuum As Boolean
        Dim objLastVacuum As Object = GetDBSetting("lastvacuum")

        If objLastVacuum Is Nothing Then
            booRunVacuum = True
        Else
            booRunVacuum = DateTime.ParseExact(CStr(objLastVacuum), "O", Nothing).AddMonths(3) < Now
        End If

        If booRunVacuum Then
            Status.lblStatus.Text = "Compacting Database..."
            Status.prgProgress.Visible = False
            Status.Visible = True
            Application.DoEvents()

            ' Make SQLite recreate the database to reduce the size on disk and remove fragmentation
            Dim sqlCommand As New SQLiteCommand("vacuum", sqlConnection)
            sqlCommand.ExecuteNonQuery()

            SetDBSetting("lastvacuum", Now.ToString("O"))

            Status.prgProgress.Value = 1
            Application.DoEvents()

            Status.Visible = False
            Application.DoEvents()
        End If
    End Sub

    Public Function GetFindNewPanel(ByVal gidPluginID As Guid, ByVal objView As Object) As Panel
        If clsPluginsInst.PluginExists(gidPluginID) Then
            FindNewPluginInst = clsPluginsInst.GetPluginInstance(gidPluginID)
            Return FindNewPluginInst.GetFindNewPanel(objView)
        Else
            Return New Panel
        End If
    End Function

    Private Sub FindNewPluginInst_FindNewException(ByVal expException As System.Exception) Handles FindNewPluginInst.FindNewException
        If ReportError.Visible = False Then
            Dim clsReport As New ErrorReporting(expException)
            ReportError.AssignReport(clsReport)
            ReportError.ShowDialog()
        End If
    End Sub

    Private Sub FindNewPluginInst_FindNewViewChange(ByVal objView As Object) Handles FindNewPluginInst.FindNewViewChange
        RaiseEvent FindNewViewChange(objView)
    End Sub

    Private Sub FindNewPluginInst_FoundNew(ByVal strProgExtID As String) Handles FindNewPluginInst.FoundNew
        Dim gidPluginID As Guid = FindNewPluginInst.ProviderID
        Dim PluginException As Exception = Nothing

        If StoreProgrammeInfo(gidPluginID, strProgExtID, PluginException) = False Then
            If PluginException IsNot Nothing Then
                If MsgBox("A problem was encountered while attempting to retrieve information about this programme." + vbCrLf + "Would you like to report this to NerdoftheHerd.com to help us improve Radio Downloader?", MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation) = MsgBoxResult.Yes Then
                    Dim clsReport As New ErrorReporting(PluginException)
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
            ProgInfo = ThisInstance.GetProgrammeInfo(strProgExtID)
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
        bmpImage.Save(mstImage, Imaging.ImageFormat.Png)
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
        Dim clsCachedWebInst As New CachedWebClient()
        Dim ThisInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(gidProviderID)

        Try
            strEpisodeExtIDs = ThisInstance.GetAvailableEpisodeIDs(strProgExtID)
        Catch expException As Exception
            ' Catch any unhandled provider exceptions
            Exit Function
        End Try

        Dim EpisodeInfo As IRadioProvider.EpisodeInfo

        If strEpisodeExtIDs IsNot Nothing Then
            ' Remove any duplicates, so that episodes don't get listed twice
            Dim arlExtIDs As New ArrayList()

            For intRemoveDups As Integer = 0 To strEpisodeExtIDs.Length - 1
                If arlExtIDs.Contains(strEpisodeExtIDs(intRemoveDups)) = False Then
                    arlExtIDs.Add(strEpisodeExtIDs(intRemoveDups))
                End If
            Next

            strEpisodeExtIDs = New String(arlExtIDs.Count - 1) {}
            arlExtIDs.CopyTo(strEpisodeExtIDs)

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
                        EpisodeInfo = ThisInstance.GetEpisodeInfo(strProgExtID, strEpisodeExtID)
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
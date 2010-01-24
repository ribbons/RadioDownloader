' Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
' Copyright © 2007-2010 Matt Robinson
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

Imports System.Collections.Generic
Imports System.Data.SQLite
Imports System.Globalization
Imports System.IO
Imports System.IO.File
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization

Friend Class Data
    Public Enum DownloadStatus
        Waiting = 0
        Downloaded = 1
        Errored = 2
    End Enum

    Public Structure DownloadData
        Dim name As String
        Dim description As String
        Dim duration As Integer
        Dim episodedate As Date
        Dim status As DownloadStatus
    End Structure

    Private Shared clsDataInstance As New Data
    <ThreadStatic()> Private dbConnection As SQLiteConnection

    Private clsPluginsInst As Plugins
    Private clsCurDldProgData As DldProgData
    Private thrDownloadThread As Thread
    Private WithEvents DownloadPluginInst As IRadioProvider
    Private WithEvents FindNewPluginInst As IRadioProvider

    Dim downloadSortCache As Dictionary(Of Integer, Integer)

    Public Event FindNewViewChange(ByVal objView As Object)
    Public Event FoundNew(ByVal intProgID As Integer)
    'Public Event Progress(ByVal currentDldProgData As DldProgData, ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon)
    'Public Event DldError(ByVal currentDldProgData As DldProgData, ByVal errorType As IRadioProvider.ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem))
    'Public Event Finished(ByVal currentDldProgData As DldProgData)
    'Public Event SubscriptionAdded(ByVal progId As Integer)
    'Public Event SubscriptionRemoved(ByVal progId As Integer)
    'Public Event SubscriptionUpdate(ByVal progId As Integer)
    Public Event DownloadAdded(ByVal epid As Integer)
    'Public Event DownloadRemoved(ByVal epid As Integer)
    'Public Event DownloadUpdate(ByVal epid As Integer)

    Public Shared Function GetInstance() As Data
        Return clsDataInstance
    End Function

    Private Function FetchDbConn() As SQLiteConnection
        If dbConnection Is Nothing Then
            dbConnection = New SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "store.db") + ";Version=3;New=False")
            dbConnection.Open()
        End If

        Return dbConnection
    End Function

    Private Sub New()
        MyBase.New()

        ' Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
        Call VacuumDatabase()

        ' Create the temp table for caching HTTP requests
        'Dim sqlCommand As New SQLiteCommand("create temporary table httpcache (uri varchar (1000), lastfetch datetime, success int, data blob)", sqlConnection)
        'sqlCommand.ExecuteNonQuery()

        ' Setup an instance of the plugins class
        clsPluginsInst = New Plugins(My.Application.Info.DirectoryPath)

        ' Fetch the version of the database
        Dim currentVer As Integer

        If GetDBSetting("databaseversion") Is Nothing Then
            currentVer = 1
        Else
            currentVer = CInt(GetDBSetting("databaseversion"))
        End If

        ' Set the current database version.  This is done before the upgrades are attempted so that
        ' if the upgrade throws an exception this can be reported, but the programme will run next time.
        ' NB: Remember to change default version in the database if this next line is changed!
        SetDBSetting("databaseversion", 3)

        Select Case currentVer
            Case 2
                Call UpgradeDBv2to3()
            Case 3
                ' Nothing to do, this is the current version.
        End Select
    End Sub

    Private Sub UpgradeDBv2to3()
        Dim count As Integer = 0
        Dim unusedTables() As String = {"tblDownloads", "tblInfo", "tblLastFetch", "tblSettings", "tblStationVisibility", "tblSubscribed"}

        Status.StatusText = "Removing unused tables..."
        Status.ProgressBarMarquee = False
        Status.ProgressBarValue = 0
        Status.ProgressBarMax = unusedTables.GetUpperBound(0) + 1
        Status.Show()
        Application.DoEvents()

        ' Delete the unused (v0.4 era) tables if they exist
        For Each unusedTable As String In unusedTables
            Status.StatusText = "Removing unused table " + CStr(count) + " of " + CStr(unusedTables.GetUpperBound(0) + 1) + "..."
            Status.ProgressBarValue = count
            Application.DoEvents()

            Using command As New SQLiteCommand("drop table if exists " + unusedTable, FetchDbConn)
                command.ExecuteNonQuery()
            End Using

            count += 1
        Next

        Status.StatusText = "Finished removing unused tables"
        Status.ProgressBarValue = count
        Application.DoEvents()

        ' Work through the images and re-save them to ensure they are compressed
        Dim compressImages As New List(Of Integer)

        Using command As New SQLiteCommand("select imgid from images", FetchDbConn)
            Using reader As SQLiteDataReader = command.ExecuteReader
                While reader.Read
                    compressImages.Add(reader.GetInt32(reader.GetOrdinal("imgid")))
                End While
            End Using
        End Using

        Status.StatusText = "Compressing images..."
        Status.ProgressBarValue = 0
        Status.ProgressBarMax = compressImages.Count
        Application.DoEvents()

        Using deleteCmd As New SQLiteCommand("delete from images where imgid=@imgid", FetchDbConn), _
              updateProgs As New SQLiteCommand("update programmes set image=@newimgid where image=@oldimgid", FetchDbConn), _
              updateEps As New SQLiteCommand("update episodes set image=@newimgid where image=@oldimgid", FetchDbConn)

            Dim newImageID As Integer
            Dim image As Bitmap
            count = 1

            For Each oldImageID As Integer In compressImages
                Status.StatusText = "Compressing image " + CStr(count) + " of " + CStr(compressImages.Count) + "..."
                Status.ProgressBarValue = count - 1
                Application.DoEvents()

                image = RetrieveImage(oldImageID)

                deleteCmd.Parameters.Add(New SQLiteParameter("@imgid", oldImageID))
                deleteCmd.ExecuteNonQuery()

                newImageID = StoreImage(image)
                Application.DoEvents()

                updateProgs.Parameters.Add(New SQLiteParameter("@oldimgid", oldImageID))
                updateProgs.Parameters.Add(New SQLiteParameter("@newimgid", newImageID))
                updateProgs.ExecuteNonQuery()

                updateEps.Parameters.Add(New SQLiteParameter("@oldimgid", oldImageID))
                updateEps.Parameters.Add(New SQLiteParameter("@newimgid", newImageID))
                updateEps.ExecuteNonQuery()

                count += 1
            Next
        End Using

        Status.StatusText = "Finished compressing images"
        Status.ProgressBarValue = compressImages.Count
        Application.DoEvents()

        Status.Hide()
        Application.DoEvents()
    End Sub

    Protected Overrides Sub Finalize()
        'sqlConnection.Close()
        'Call AbortDownloadThread()
        MyBase.Finalize()
    End Sub

    'Public Function DownloadSetErrored(ByVal intEpID As Integer, ByVal errType As IRadioProvider.ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem)) As Boolean
    '    Select Case errType
    '        Case IRadioProvider.ErrorType.RemoveFromList
    '            Call RemoveDownload(intEpID)
    '            Return False
    '        Case IRadioProvider.ErrorType.UnknownError
    '            furtherDetails.Add(New DldErrorDataItem("details", errorDetails))

    '            Dim detailsStringWriter As New StringWriter(CultureInfo.InvariantCulture)
    '            Dim detailsSerializer As New XmlSerializer(GetType(List(Of DldErrorDataItem)))
    '            detailsSerializer.Serialize(detailsStringWriter, furtherDetails)
    '            errorDetails = detailsStringWriter.ToString
    '    End Select

    '    Dim sqlCommand As New SQLiteCommand("update downloads set status=@status, errortime=datetime('now'), errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Errored))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@errortype", errType))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@errordetails", errorDetails))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.ExecuteNonQuery()

    '    Return True
    'End Function

    'Public Sub DownloadSetDownloaded(ByVal intEpID As Integer, ByVal strDownloadPath As String)
    '    Dim sqlCommand As New SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@filepath", strDownloadPath))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Function FindAndDownload() As Boolean
    '    If thrDownloadThread Is Nothing Then
    '        Dim sqlCommand As New SQLiteCommand("select pluginid, pr.name as progname, pr.description as progdesc, pr.image as progimg, ep.name as epname, ep.description as epdesc, ep.duration, ep.date, ep.image as epimg, pr.extid as progextid, ep.extid as epextid, dl.status, ep.epid, dl.errorcount from downloads as dl, episodes as ep, programmes as pr where dl.epid=ep.epid and ep.progid=pr.progid and (dl.status=@statuswait or (dl.status=@statuserr and dl.errortime < datetime('now', '-' || power(2, dl.errorcount) || ' hours'))) order by ep.date", sqlConnection)
    '        sqlCommand.Parameters.Add(New SQLiteParameter("@statuswait", Statuses.Waiting))
    '        sqlCommand.Parameters.Add(New SQLiteParameter("@statuserr", Statuses.Errored))

    '        Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '        While thrDownloadThread Is Nothing
    '            If sqlReader.Read Then
    '                Dim gidPluginID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))
    '                Dim intEpID As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))

    '                If clsPluginsInst.PluginExists(gidPluginID) Then
    '                    clsCurDldProgData = New DldProgData

    '                    Dim priProgInfo As IRadioProvider.ProgrammeInfo
    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("progname")) Then
    '                        priProgInfo.Name = Nothing
    '                    Else
    '                        priProgInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("progname"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("progdesc")) Then
    '                        priProgInfo.Description = Nothing
    '                    Else
    '                        priProgInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("progdesc"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("progimg")) Then
    '                        priProgInfo.Image = Nothing
    '                    Else
    '                        priProgInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("progimg")))
    '                    End If

    '                    Dim epiEpInfo As IRadioProvider.EpisodeInfo
    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("epname")) Then
    '                        epiEpInfo.Name = Nothing
    '                    Else
    '                        epiEpInfo.Name = sqlReader.GetString(sqlReader.GetOrdinal("epname"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("epdesc")) Then
    '                        epiEpInfo.Description = Nothing
    '                    Else
    '                        epiEpInfo.Description = sqlReader.GetString(sqlReader.GetOrdinal("epdesc"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("duration")) Then
    '                        epiEpInfo.DurationSecs = Nothing
    '                    Else
    '                        epiEpInfo.DurationSecs = sqlReader.GetInt32(sqlReader.GetOrdinal("duration"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("date")) Then
    '                        epiEpInfo.Date = Nothing
    '                    Else
    '                        epiEpInfo.Date = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
    '                    End If

    '                    If sqlReader.IsDBNull(sqlReader.GetOrdinal("epimg")) Then
    '                        epiEpInfo.Image = Nothing
    '                    Else
    '                        epiEpInfo.Image = RetrieveImage(sqlReader.GetInt32(sqlReader.GetOrdinal("epimg")))
    '                    End If

    '                    epiEpInfo.ExtInfo = New Dictionary(Of String, String)

    '                    Dim sqlExtCommand As New SQLiteCommand("select name, value from episodeext where epid=@epid", sqlConnection)
    '                    sqlExtCommand.Parameters.Add(New SQLiteParameter("@epid", sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))))
    '                    Dim sqlExtReader As SQLiteDataReader = sqlExtCommand.ExecuteReader

    '                    While sqlExtReader.Read
    '                        epiEpInfo.ExtInfo.Add(sqlExtReader.GetString(sqlExtReader.GetOrdinal("name")), sqlExtReader.GetString(sqlExtReader.GetOrdinal("value")))
    '                    End While

    '                    With clsCurDldProgData
    '                        .PluginID = gidPluginID
    '                        .ProgExtID = sqlReader.GetString(sqlReader.GetOrdinal("progextid"))
    '                        .EpID = intEpID
    '                        .EpisodeExtID = sqlReader.GetString(sqlReader.GetOrdinal("epextid"))
    '                        .ProgInfo = priProgInfo
    '                        .EpisodeInfo = epiEpInfo
    '                    End With

    '                    If sqlReader.GetInt32(sqlReader.GetOrdinal("status")) = Statuses.Errored Then
    '                        Call ResetDownload(intEpID, True)
    '                        clsCurDldProgData.AttemptNumber = sqlReader.GetInt32(sqlReader.GetOrdinal("errorcount")) + 1
    '                    Else
    '                        clsCurDldProgData.AttemptNumber = 1
    '                    End If

    '                    thrDownloadThread = New Thread(AddressOf DownloadProgThread)
    '                    thrDownloadThread.Start()

    '                    FindAndDownload = True
    '                End If
    '            Else
    '                Exit While
    '            End If
    '        End While

    '        sqlReader.Close()
    '    End If
    'End Function

    'Public Sub DownloadProgThread()
    '    DownloadPluginInst = clsPluginsInst.GetPluginInstance(clsCurDldProgData.PluginID)

    '    Try
    '        ' Make sure that the temp folder still exists
    '        Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath, "RadioDownloader"))

    '        With clsCurDldProgData
    '            Try
    '                clsCurDldProgData.FinalName = FileUtils.FindFreeSaveFileName(My.Settings.FileNameFormat, clsCurDldProgData.ProgInfo.Name, clsCurDldProgData.EpisodeInfo.Name, clsCurDldProgData.EpisodeInfo.Date, FileUtils.GetSaveFolder())
    '            Catch dirNotFoundExp As DirectoryNotFoundException
    '                Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.", New List(Of DldErrorDataItem))
    '                Exit Sub
    '            End Try

    '            DownloadPluginInst.DownloadProgramme(.ProgExtID, .EpisodeExtID, .ProgInfo, .EpisodeInfo, .FinalName, .AttemptNumber)
    '        End With
    '    Catch unknownExp As Exception
    '        Dim extraDetails As New List(Of DldErrorDataItem)
    '        extraDetails.Add(New DldErrorDataItem("error", unknownExp.GetType.ToString + ": " + unknownExp.Message))
    '        extraDetails.Add(New DldErrorDataItem("exceptiontostring", unknownExp.ToString))

    '        If unknownExp.Data IsNot Nothing Then
    '            For Each dataEntry As DictionaryEntry In unknownExp.Data
    '                If dataEntry.Key.GetType Is GetType(String) And dataEntry.Value.GetType Is GetType(String) Then
    '                    extraDetails.Add(New DldErrorDataItem("expdata:Data:" + CStr(dataEntry.Key), CStr(dataEntry.Value)))
    '                End If
    '            Next
    '        End If

    '        Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.UnknownError, unknownExp.GetType.ToString + vbCrLf + unknownExp.StackTrace, extraDetails)
    '    End Try
    'End Sub

    'Public Function DownloadPath(ByVal intEpID As Integer) As String
    '    Dim sqlCommand As New SQLiteCommand("select filepath from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        DownloadPath = sqlReader.GetString(sqlReader.GetOrdinal("filepath"))
    '    Else
    '        DownloadPath = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function DownloadStatus(ByVal intEpID As Integer) As Statuses
    '    Dim sqlCommand As New SQLiteCommand("select status from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        DownloadStatus = DirectCast(sqlReader.GetInt32(sqlReader.GetOrdinal("status")), Statuses)
    '    Else
    '        DownloadStatus = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Private Function UpdateProgInfoAsRequired(ByVal intProgID As Integer) As Date
    '    Dim sqlCommand As New SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        Dim gidProviderID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))

    '        If clsPluginsInst.PluginExists(gidProviderID) Then
    '            Dim ThisInstance As IRadioProvider
    '            ThisInstance = clsPluginsInst.GetPluginInstance(gidProviderID)

    '            If sqlReader.GetDateTime(sqlReader.GetOrdinal("lastupdate")).AddDays(ThisInstance.ProgInfoUpdateFreqDays) < Now Then
    '                Call StoreProgrammeInfo(gidProviderID, sqlReader.GetString(sqlReader.GetOrdinal("extid")), Nothing)
    '            End If
    '        End If
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function ProgrammeName(ByVal intProgID As Integer) As String
    '    Call UpdateProgInfoAsRequired(intProgID)

    '    Dim sqlCommand As New SQLiteCommand("select name from programmes where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        ProgrammeName = sqlReader.GetString(sqlReader.GetOrdinal("name"))
    '    Else
    '        ProgrammeName = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function ProgrammeDescription(ByVal intProgID As Integer) As String
    '    Call UpdateProgInfoAsRequired(intProgID)

    '    Dim sqlCommand As New SQLiteCommand("select description from programmes where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        ProgrammeDescription = sqlReader.GetString(sqlReader.GetOrdinal("description"))
    '    Else
    '        ProgrammeDescription = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    Public Function FetchProgrammeImage(ByVal progid As Integer) As Bitmap
        'Call UpdateProgInfoAsRequired(intProgID)

        Using command As New SQLiteCommand("select image from programmes where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader

                If reader.Read Then
                    Dim imgid As Integer = reader.GetInt32(reader.GetOrdinal("image"))

                    If imgid = Nothing Then
                        ' Find the id of the latest episode's image
                        Using latestCmd As New SQLiteCommand("select image from episodes where progid=@progid and image notnull order by date desc limit 1", FetchDbConn)
                            latestCmd.Parameters.Add(New SQLiteParameter("@progid", progid))

                            Using latestRdr As SQLiteDataReader = latestCmd.ExecuteReader
                                If latestRdr.Read Then
                                    imgid = latestRdr.GetInt32(reader.GetOrdinal("image"))
                                End If
                            End Using
                        End Using
                    End If

                    If imgid <> Nothing Then
                        FetchProgrammeImage = RetrieveImage(imgid)
                    Else
                        FetchProgrammeImage = Nothing
                    End If
                Else
                    FetchProgrammeImage = Nothing
                End If
            End Using
        End Using
    End Function

    'Public Function ProgrammeIsSingleEpisode(ByVal intProgID As Integer) As Boolean
    '    Call UpdateProgInfoAsRequired(intProgID)

    '    Dim sqlCommand As New SQLiteCommand("select singleepisode from programmes where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        ProgrammeIsSingleEpisode = sqlReader.GetBoolean(sqlReader.GetOrdinal("singleepisode"))
    '    Else
    '        ProgrammeIsSingleEpisode = False
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function EpisodeExists(ByVal intEpID As Integer) As Boolean
    '    Dim sqlCommand As New SQLiteCommand("select count(*) from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))

    '    Return CInt(sqlCommand.ExecuteScalar) > 0
    'End Function

    'Public Function EpisodeName(ByVal intEpID As Integer) As String
    '    Dim sqlCommand As New SQLiteCommand("select name, date from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        If sqlReader.IsDBNull(sqlReader.GetOrdinal("name")) Then
    '            EpisodeName = Nothing
    '        Else
    '            EpisodeName = sqlReader.GetString(sqlReader.GetOrdinal("name"))

    '            If sqlReader.IsDBNull(sqlReader.GetOrdinal("date")) = False Then
    '                Dim dteEpisodeDate As Date = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))

    '                ' Use regex to remove a number of different date formats from programme titles.
    '                ' Will only remove dates with the same month & year as the programme itself, but any day of the month
    '                ' as there is sometimes a mismatch of a day or two between the date in a title and the publish date.
    '                Dim regStripDate As New Regex("\A(" + dteEpisodeDate.ToString("yyyy", CultureInfo.InvariantCulture) + "/" + dteEpisodeDate.ToString("MM", CultureInfo.InvariantCulture) + "/\d{2} ?-? )?(?<name>.*?)( ?:? (\d{2}/" + dteEpisodeDate.ToString("MM", CultureInfo.InvariantCulture) + "/" + dteEpisodeDate.ToString("yyyy", CultureInfo.InvariantCulture) + "|((Mon|Tue|Wed|Thu|Fri) )?(\d{1,2}(st|nd|rd|th)? )?(" + dteEpisodeDate.ToString("MMMM", CultureInfo.InvariantCulture) + "|" + dteEpisodeDate.ToString("MMM", CultureInfo.InvariantCulture) + ")( \d{1,2}(st|nd|rd|th)?| (" + dteEpisodeDate.ToString("yy", CultureInfo.InvariantCulture) + "|" + dteEpisodeDate.ToString("yyyy", CultureInfo.InvariantCulture) + "))?))?\Z")

    '                If regStripDate.IsMatch(EpisodeName) Then
    '                    EpisodeName = regStripDate.Match(EpisodeName).Groups("name").ToString
    '                End If
    '            End If
    '        End If
    '    Else
    '        EpisodeName = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function EpisodeDescription(ByVal intEpID As Integer) As String
    '    Dim sqlCommand As New SQLiteCommand("select description from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        If sqlReader.IsDBNull(sqlReader.GetOrdinal("description")) Then
    '            EpisodeDescription = Nothing
    '        Else
    '            EpisodeDescription = sqlReader.GetString(sqlReader.GetOrdinal("description"))
    '        End If
    '    Else
    '        EpisodeDescription = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function EpisodeDate(ByVal intEpID As Integer) As DateTime
    '    Dim sqlCommand As New SQLiteCommand("select date from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        EpisodeDate = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
    '    Else
    '        EpisodeDate = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function EpisodeDuration(ByVal intEpID As Integer) As Integer
    '    Dim sqlCommand As New SQLiteCommand("select duration from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        EpisodeDuration = sqlReader.GetInt32(sqlReader.GetOrdinal("duration"))
    '    Else
    '        EpisodeDuration = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    Public Function FetchEpisodeImage(ByVal epid As Integer) As Bitmap
        Using command As New SQLiteCommand("select image, progid from episodes where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Dim imgid As Integer = reader.GetInt32(reader.GetOrdinal("image"))

                    If imgid <> Nothing Then
                        FetchEpisodeImage = RetrieveImage(imgid)
                    Else
                        FetchEpisodeImage = Nothing
                    End If

                    If FetchEpisodeImage Is Nothing Then
                        If reader.IsDBNull(reader.GetOrdinal("progid")) = False Then
                            FetchEpisodeImage = FetchProgrammeImage(reader.GetInt32(reader.GetOrdinal("progid")))
                        End If
                    End If
                Else
                    FetchEpisodeImage = Nothing
                End If
            End Using
        End Using
    End Function

    'Public Function EpisodeAutoDownload(ByVal intEpID As Integer) As Boolean
    '    Dim sqlCommand As New SQLiteCommand("select autodownload from episodes where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        EpisodeAutoDownload = sqlReader.GetInt32(sqlReader.GetOrdinal("autodownload")) = 1
    '    Else
    '        EpisodeAutoDownload = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Sub EpisodeSetAutoDownload(ByVal intEpID As Integer, ByVal booAutoDownload As Boolean)
    '    Dim intAutoDownload As Integer = 0

    '    If booAutoDownload Then
    '        intAutoDownload = 1
    '    End If

    '    Dim sqlCommand As New SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@autodownload", intAutoDownload))
    '    sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Function CountDownloadsNew() As Integer
    '    Dim sqlCommand As New SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
    '    Return CInt(sqlCommand.ExecuteScalar())
    'End Function

    'Public Function CountDownloadsErrored() As Integer
    '    Dim sqlCommand As New SQLiteCommand("select count(epid) from downloads where status=@status", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Errored))
    '    Return CInt(sqlCommand.ExecuteScalar())
    'End Function

    'Public Sub UpdateDlList(ByRef lstListview As ExtListView)
    '    If booErrorStatus Then
    '        Main.SetTrayStatus(False, Main.ErrorStatus.Error)
    '    Else
    '        Main.SetTrayStatus(False, Main.ErrorStatus.Normal)
    '    End If

    '    sqlReader.Close()
    'End Sub

    'Public Sub UpdateSubscrList(ByRef lstListview As ExtListView)
    '    Dim lstAdd As ListViewItem

    '    Dim sqlCommand As New SQLiteCommand("select pluginid, name, subscriptions.progid from subscriptions, programmes where subscriptions.progid=programmes.progid order by name", sqlConnection)
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    lstListview.Items.Clear()

    '    With sqlReader
    '        Do While .Read()
    '            Dim gidPluginID As New Guid(.GetString(.GetOrdinal("pluginid")))
    '            lstAdd = New ListViewItem

    '            lstAdd.Text = .GetString(.GetOrdinal("name"))

    '            Dim dteLastDownload As Date = LatestDownloadDate(.GetInt32(.GetOrdinal("progid")))

    '            If dteLastDownload = Nothing Then
    '                lstAdd.SubItems.Add("Never")
    '            Else
    '                lstAdd.SubItems.Add(dteLastDownload.ToShortDateString)
    '            End If

    '            lstAdd.SubItems.Add(ProviderName(gidPluginID))
    '            lstAdd.Tag = .GetInt32(.GetOrdinal("progid"))
    '            lstAdd.ImageKey = "subscribed"

    '            lstListview.Items.Add(lstAdd)
    '        Loop
    '    End With

    '    sqlReader.Close()
    'End Sub

    'Public Sub ListEpisodes(ByVal intProgID As Integer, ByRef lstListview As ExtListView)
    '    Dim intAvailable As Integer()
    '    intAvailable = GetAvailableEpisodes(intProgID)
    '    Array.Reverse(intAvailable)

    '    lstListview.Items.Clear()

    '    For Each intEpID As Integer In intAvailable
    '        Dim lstAdd As New ListViewItem

    '        lstAdd.Text = EpisodeDate(intEpID).ToShortDateString
    '        lstAdd.SubItems.Add(EpisodeName(intEpID))
    '        lstAdd.Checked = EpisodeAutoDownload(intEpID)
    '        lstAdd.Tag = intEpID

    '        lstListview.Items.Add(lstAdd)
    '    Next
    'End Sub

    'Public Sub CheckSubscriptions(ByRef lstList As ExtListView)
    '    Dim sqlCommand As New SQLiteCommand("select progid from subscriptions", sqlConnection)
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    With sqlReader
    '        Do While .Read()
    '            Dim intAvailableEps() As Integer
    '            intAvailableEps = GetAvailableEpisodes(.GetInt32(.GetOrdinal("progid")))

    '            For Each intEpID As Integer In intAvailableEps
    '                If EpisodeAutoDownload(intEpID) Then
    '                    Dim sqlCheckCmd As New SQLiteCommand("select epid from downloads where epid=@epid", sqlConnection)
    '                    sqlCheckCmd.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '                    Dim sqlCheckRdr As SQLiteDataReader = sqlCheckCmd.ExecuteReader

    '                    If sqlCheckRdr.Read = False Then
    '                        Call AddDownload(intEpID)
    '                        'Call UpdateDlList(lstList)
    '                    End If

    '                    sqlCheckRdr.Close()
    '                End If
    '            Next
    '        Loop
    '    End With

    '    sqlReader.Close()
    'End Sub

    'Public Function AddDownload(ByVal intEpID As Integer) As Boolean
    '    Dim sqlCommand As New SQLiteCommand("select epid from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        Return False
    '    End If

    '    sqlReader.Close()

    '    sqlCommand = New SQLiteCommand("insert into downloads (epid, status) values (@epid, @status)", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Waiting))
    '    Call sqlCommand.ExecuteNonQuery()

    '    Return True
    'End Function

    'Public Function IsSubscribed(ByVal intProgID As Integer) As Boolean
    '    Dim sqlCommand As New SQLiteCommand("select progid from subscriptions where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    Return sqlReader.Read
    'End Function

    'Public Sub AddSubscription(ByVal intProgID As Integer)
    '    If IsSubscribed(intProgID) Then
    '        Return
    '    End If

    '    Dim sqlCommand As New SQLiteCommand("insert into subscriptions (progid) values (@progid)", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Call sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Sub RemoveSubscription(ByVal intProgID As Integer)
    '    Dim sqlCommand As New SQLiteCommand("delete from subscriptions where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Call sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Function LatestDownloadDate(ByVal intProgID As Integer) As DateTime
    '    Dim sqlCommand As New SQLiteCommand("select date from episodes, downloads where episodes.epid=downloads.epid and progid=@progid order by date desc limit 1", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read = False Then
    '        ' No downloads of this program, return nothing
    '        LatestDownloadDate = Nothing
    '    Else
    '        LatestDownloadDate = sqlReader.GetDateTime(sqlReader.GetOrdinal("date"))
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Sub ResetDownload(ByVal intEpID As Integer, ByVal booAuto As Boolean)
    '    Dim sqlCommand As New SQLiteCommand("update downloads set status=@status where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Waiting))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.ExecuteNonQuery()

    '    If booAuto = False Then
    '        sqlCommand = New SQLiteCommand("update downloads set errorcount=0 where epid=@epid", sqlConnection)
    '        sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '        sqlCommand.ExecuteNonQuery()
    '    End If
    'End Sub

    'Public Sub RemoveDownload(ByVal intEpID As Integer)
    '    Dim sqlCommand As New SQLiteCommand("delete from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Call sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Function ProviderName(ByVal gidPluginID As Guid) As String
    '    If clsPluginsInst.PluginExists(gidPluginID) = False Then
    '        Return ""
    '    End If

    '    Dim ThisInstance As IRadioProvider
    '    ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

    '    Return ThisInstance.ProviderName
    'End Function

    'Public Function ProviderDescription(ByVal gidPluginID As Guid) As String
    '    If clsPluginsInst.PluginExists(gidPluginID) = False Then
    '        Return ""
    '    End If

    '    Dim ThisInstance As IRadioProvider
    '    ThisInstance = clsPluginsInst.GetPluginInstance(gidPluginID)

    '    Return ThisInstance.ProviderDescription
    'End Function

    'Public Sub DownloadBumpPlayCount(ByVal intEpID As Integer)
    '    Dim sqlCommand As New SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    sqlCommand.ExecuteNonQuery()
    'End Sub

    'Public Function DownloadPlayCount(ByVal intEpID As Integer) As Integer
    '    Dim sqlCommand As New SQLiteCommand("select playcount from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        DownloadPlayCount = sqlReader.GetInt32(sqlReader.GetOrdinal("playcount"))
    '    Else
    '        DownloadPlayCount = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function DownloadErrorType(ByVal intEpID As Integer) As IRadioProvider.ErrorType
    '    Dim sqlCommand As New SQLiteCommand("select errortype from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        Dim intErrorType As Integer = sqlReader.GetInt32(sqlReader.GetOrdinal("ErrorType"))

    '        If intErrorType <> Nothing Then
    '            DownloadErrorType = CType(intErrorType, IRadioProvider.ErrorType)
    '        Else
    '            DownloadErrorType = IRadioProvider.ErrorType.UnknownError
    '        End If
    '    Else
    '        DownloadErrorType = IRadioProvider.ErrorType.UnknownError
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Function DownloadErrorDetails(ByVal intEpID As Integer) As String
    '    Dim sqlCommand As New SQLiteCommand("select errordetails from downloads where epid=@epid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        Dim strErrorDetails As String = sqlReader.GetString(sqlReader.GetOrdinal("errordetails"))

    '        If strErrorDetails IsNot Nothing Then
    '            DownloadErrorDetails = strErrorDetails
    '        Else
    '            DownloadErrorDetails = ""
    '        End If
    '    Else
    '        DownloadErrorDetails = ""
    '    End If

    '    sqlReader.Close()
    'End Function

    'Public Sub DownloadReportError(ByVal episodeID As Integer)
    '    Dim errorText As String = String.Empty
    '    Dim extraDetailsString As String = DownloadErrorDetails(episodeID)
    '    Dim errorExtraDetails As New Dictionary(Of String, String)

    '    Dim detailsSerializer As New XmlSerializer(GetType(List(Of DldErrorDataItem)))

    '    Try
    '        Dim extraDetails As List(Of DldErrorDataItem)
    '        extraDetails = DirectCast(detailsSerializer.Deserialize(New StringReader(extraDetailsString)), List(Of DldErrorDataItem))

    '        For Each detailItem As DldErrorDataItem In extraDetails
    '            Select Case detailItem.Name
    '                Case "error"
    '                    errorText = detailItem.Data
    '                Case "details"
    '                    extraDetailsString = detailItem.Data
    '                Case Else
    '                    errorExtraDetails.Add(detailItem.Name, detailItem.Data)
    '            End Select
    '        Next
    '    Catch invalidOperationExp As InvalidOperationException
    '        ' Do nothing, and fall back to reporting all the details as one string
    '    Catch invalidCastExp As InvalidCastException
    '        ' Do nothing, and fall back to reporting all the details as one string
    '    End Try

    '    If errorText = String.Empty Then
    '        errorText = DownloadErrorType(episodeID).ToString
    '    End If

    '    Dim command As New SQLiteCommand("select ep.name as epname, ep.description as epdesc, date, duration, ep.extid as epextid, pr.name as progname, pr.description as progdesc, pr.extid as progextid, pluginid from episodes as ep, programmes as pr where epid=@epid and ep.progid=pr.progid", sqlConnection)
    '    command.Parameters.Add(New SQLiteParameter("@epid", episodeID))
    '    Dim reader As SQLiteDataReader = command.ExecuteReader

    '    If reader.Read Then
    '        errorExtraDetails.Add("episode:name", reader.GetString(reader.GetOrdinal("epname")))
    '        errorExtraDetails.Add("episode:description", reader.GetString(reader.GetOrdinal("epdesc")))
    '        errorExtraDetails.Add("episode:date", reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture))
    '        errorExtraDetails.Add("episode:duration", CStr(reader.GetInt32(reader.GetOrdinal("duration"))))
    '        errorExtraDetails.Add("episode:extid", reader.GetString(reader.GetOrdinal("epextid")))

    '        errorExtraDetails.Add("programme:name", reader.GetString(reader.GetOrdinal("progname")))
    '        errorExtraDetails.Add("programme:description", reader.GetString(reader.GetOrdinal("progdesc")))
    '        errorExtraDetails.Add("programme:extid", reader.GetString(reader.GetOrdinal("progextid")))

    '        Dim providerGuid As New Guid(reader.GetString(reader.GetOrdinal("pluginid")))
    '        errorExtraDetails.Add("provider:id", reader.GetString(reader.GetOrdinal("pluginid")))
    '        errorExtraDetails.Add("provider:name", ProviderName(providerGuid))
    '        errorExtraDetails.Add("provider:description", ProviderDescription(providerGuid))
    '    End If

    '    reader.Close()

    '    Dim clsReport As New ErrorReporting("Download Error: " + errorText, extraDetailsString, errorExtraDetails)
    '    clsReport.SendReport(My.Settings.ErrorReportURL)
    'End Sub

    'Private Sub DownloadPluginInst_DldError(ByVal errorType As IRadioProvider.ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem)) Handles DownloadPluginInst.DldError
    '    'RaiseEvent DldError(clsCurDldProgData, errorType, errorDetails, furtherDetails)
    '    thrDownloadThread = Nothing
    '    clsCurDldProgData = Nothing
    'End Sub

    'Private Sub DownloadPluginInst_Finished(ByVal strFileExtension As String) Handles DownloadPluginInst.Finished
    '    clsCurDldProgData.FinalName += "." + strFileExtension

    '    'RaiseEvent Finished(clsCurDldProgData)
    '    thrDownloadThread = Nothing
    '    clsCurDldProgData = Nothing
    'End Sub

    'Private Sub DownloadPluginInst_Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles DownloadPluginInst.Progress
    '    'RaiseEvent Progress(clsCurDldProgData, intPercent, strStatusText, Icon)
    'End Sub

    'Public Sub AbortDownloadThread()
    '    If thrDownloadThread IsNot Nothing Then
    '        thrDownloadThread.Abort()
    '        thrDownloadThread = Nothing
    '    End If
    'End Sub

    'Public Function GetCurrentDownloadInfo() As DldProgData
    '    Return clsCurDldProgData
    'End Function

    'Public Sub UpdateProviderList(ByVal providerList As ListView, ByVal providerIcons As ImageList, ByVal providerOptsMenu As MenuItem)
    '    Dim pluginIDList() As Guid
    '    pluginIDList = clsPluginsInst.GetPluginIdList

    '    Dim providerInstance As IRadioProvider
    '    Dim addListItem As ListViewItem
    '    Dim addMenuItem As MenuItem

    '    For Each pluginID As Guid In pluginIDList
    '        providerInstance = clsPluginsInst.GetPluginInstance(pluginID)

    '        addListItem = New ListViewItem
    '        addListItem.Text = providerInstance.ProviderName
    '        addListItem.Tag = pluginID

    '        Dim providerIcon As Bitmap = providerInstance.ProviderIcon

    '        If providerIcon IsNot Nothing Then
    '            providerIcons.Images.Add(pluginID.ToString, providerIcon)
    '            addListItem.ImageKey = pluginID.ToString
    '        Else
    '            addListItem.ImageKey = "default"
    '        End If

    '        providerList.Items.Add(addListItem)

    '        addMenuItem = New MenuItem(providerInstance.ProviderName + " Provider")

    '        If providerInstance.GetShowOptionsHandler IsNot Nothing Then
    '            AddHandler addMenuItem.Click, providerInstance.GetShowOptionsHandler
    '        Else
    '            addMenuItem.Enabled = False
    '        End If


    '        providerOptsMenu.MenuItems.Add(addMenuItem)
    '    Next

    '    If providerOptsMenu.MenuItems.Count = 0 Then
    '        addMenuItem = New MenuItem("No providers")
    '        addMenuItem.Enabled = False
    '        providerOptsMenu.MenuItems.Add(addMenuItem)
    '    End If
    'End Sub

    Public Sub PerformCleanup()
        '    Dim sqlCommand As New SQLiteCommand("select epid, filepath from downloads where status=@status", sqlConnection)
        '    sqlCommand.Parameters.Add(New SQLiteParameter("@status", Statuses.Downloaded))
        '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader()

        '    With sqlReader
        '        Do While .Read
        '            ' Remove programmes for which the associated audio file no longer exists
        '            If Exists(.GetString(.GetOrdinal("filepath"))) = False Then
        '                Dim intEpID As Integer = .GetInt32(.GetOrdinal("epid"))

        '                ' Take the download out of the list
        '                RemoveDownload(intEpID)

        '                ' Set the auto download flag of this episode to false, so if we are subscribed to the programme
        '                ' it doesn't just download it all over again
        '                Call EpisodeSetAutoDownload(intEpID, False)
        '            End If
        '        Loop
        '    End With

        '    sqlReader.Close()
    End Sub

    Private Sub SetDBSetting(ByVal propertyName As String, ByVal value As Object)
        Using command As New SQLiteCommand("delete from settings where property=@property", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@property", propertyName))
            command.ExecuteNonQuery()
        End Using

        Using command As New SQLiteCommand("insert into settings (property, value) values (@property, @value)", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@property", propertyName))
            command.Parameters.Add(New SQLiteParameter("@value", value))
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Function GetDBSetting(ByVal strPropertyName As String) As Object
        Using command As New SQLiteCommand("select value from settings where property=@property", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@property", strPropertyName))

            Using reader As SQLiteDataReader = command.ExecuteReader()
                If reader.Read = False Then
                    Return Nothing
                End If

                Return reader.GetValue(reader.GetOrdinal("value"))
            End Using
        End Using
    End Function

    Private Sub VacuumDatabase()
        ' Vacuum the database every few months - vacuums are spaced like this as they take ages to run
        Dim runVacuum As Boolean
        Dim lastVacuum As Object = GetDBSetting("lastvacuum")

        If lastVacuum Is Nothing Then
            runVacuum = True
        Else
            runVacuum = DateTime.ParseExact(CStr(lastVacuum), "O", CultureInfo.InvariantCulture).AddMonths(3) < Now
        End If

        If runVacuum Then
            Status.StatusText = "Compacting Database..." + Environment.NewLine + Environment.NewLine + "This may take some time if you have downloaded a lot of programmes."
            Status.ProgressBarMarquee = True
            Status.Show()
            Application.DoEvents()

            ' Make SQLite recreate the database to reduce the size on disk and remove fragmentation
            Using command As New SQLiteCommand("vacuum", FetchDbConn)
                command.ExecuteNonQuery()
            End Using

            SetDBSetting("lastvacuum", Now.ToString("O", CultureInfo.InvariantCulture))

            Status.Hide()
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

    Private Sub FindNewPluginInst_FindNewException(ByVal exception As Exception, ByVal unhandled As Boolean) Handles FindNewPluginInst.FindNewException
        Dim reportException As New ErrorReporting("Find New Error", exception)

        If unhandled Then
            If ReportError.Visible = False Then
                ReportError.AssignReport(reportException)
                ReportError.ShowDialog()
            End If
        Else
            reportException.SendReport(My.Settings.ErrorReportURL)
        End If
    End Sub

    Private Sub FindNewPluginInst_FindNewViewChange(ByVal objView As Object) Handles FindNewPluginInst.FindNewViewChange
        RaiseEvent FindNewViewChange(objView)
    End Sub

    'Private Sub FindNewPluginInst_FoundNew(ByVal strProgExtID As String) Handles FindNewPluginInst.FoundNew
    '    Dim gidPluginID As Guid = FindNewPluginInst.ProviderID
    '    Dim PluginException As Exception = Nothing

    '    If StoreProgrammeInfo(gidPluginID, strProgExtID, PluginException) = False Then
    '        If PluginException IsNot Nothing Then
    '            If MsgBox("A problem was encountered while attempting to retrieve information about this programme." + vbCrLf + "Would you like to report this to NerdoftheHerd.com to help us improve Radio Downloader?", MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation) = MsgBoxResult.Yes Then
    '                Dim clsReport As New ErrorReporting(PluginException)
    '                clsReport.SendReport(My.Settings.ErrorReportURL)
    '            End If

    '            Exit Sub
    '        Else
    '            Call MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation)
    '            Exit Sub
    '        End If
    '    End If

    '    Dim intProgID As Integer = ExtIDToProgID(gidPluginID, strProgExtID)
    '    RaiseEvent FoundNew(intProgID)
    'End Sub

    'Private Function StoreProgrammeInfo(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String, ByRef PluginException As Exception) As Boolean
    '    If clsPluginsInst.PluginExists(gidPluginID) = False Then
    '        Return False
    '    End If

    '    Dim ThisInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(gidPluginID)
    '    Dim getProgInfo As IRadioProvider.GetProgrammeInfoReturn

    '    Try
    '        getProgInfo = ThisInstance.GetProgrammeInfo(strProgExtID)
    '    Catch PluginException
    '        ' Catch unhandled errors in the plugin
    '        Return False
    '    End Try

    '    If getProgInfo.Success = False Then
    '        Return False
    '    End If

    '    Dim intProgID As Integer = ExtIDToProgID(gidPluginID, strProgExtID)
    '    Dim sqlCommand As SQLiteCommand

    '    If intProgID = Nothing Then
    '        sqlCommand = New SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", sqlConnection)
    '        sqlCommand.Parameters.Add(New SQLiteParameter("@pluginid", gidPluginID.ToString))
    '        sqlCommand.Parameters.Add(New SQLiteParameter("@extid", strProgExtID))
    '        sqlCommand.ExecuteNonQuery()

    '        sqlCommand = New SQLiteCommand("select last_insert_rowid()", sqlConnection)
    '        intProgID = CInt(sqlCommand.ExecuteScalar)
    '    End If

    '    sqlCommand = New SQLiteCommand("update programmes set name=@name, description=@description, image=@image, singleepisode=@singleepisode, lastupdate=@lastupdate where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@name", getProgInfo.ProgrammeInfo.Name))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@description", getProgInfo.ProgrammeInfo.Description))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@image", StoreImage(getProgInfo.ProgrammeInfo.Image)))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@singleepisode", getProgInfo.ProgrammeInfo.SingleEpisode))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@lastupdate", Now))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    sqlCommand.ExecuteNonQuery()

    '    Return True
    'End Function

    Private Function StoreImage(ByVal image As Bitmap) As Integer
        If image Is Nothing Then
            Return Nothing
        End If

        ' Convert the image into a byte array
        Dim memstream As New MemoryStream()
        image.Save(memstream, Imaging.ImageFormat.Png)
        Dim imageAsBytes(CInt(memstream.Length - 1)) As Byte
        memstream.Position = 0
        memstream.Read(imageAsBytes, 0, CInt(memstream.Length))

        Using command As New SQLiteCommand("select imgid from images where image=@image", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@image", imageAsBytes))

            Using sqlReader As SQLiteDataReader = command.ExecuteReader
                If sqlReader.Read() Then
                    Return sqlReader.GetInt32(sqlReader.GetOrdinal("imgid"))
                End If
            End Using
        End Using


        Using command As New SQLiteCommand("insert into images (image) values (@image)", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@image", imageAsBytes))
            command.ExecuteNonQuery()
        End Using

        Using command As New SQLiteCommand("select last_insert_rowid()", FetchDbConn)
            Return CInt(command.ExecuteScalar)
        End Using
    End Function

    Private Function RetrieveImage(ByVal imgid As Integer) As Bitmap
        Using command As New SQLiteCommand("select image from images where imgid=@imgid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@imgid", imgid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If Not reader.Read Then
                    Return Nothing
                End If

                ' Get the size of the image data by passing nothing to getbytes
                Dim dataLength As Integer = CInt(reader.GetBytes(reader.GetOrdinal("image"), 0, Nothing, 0, 0))
                Dim content(dataLength - 1) As Byte

                reader.GetBytes(reader.GetOrdinal("image"), 0, content, 0, dataLength)
                RetrieveImage = New Bitmap(New MemoryStream(content))
            End Using
        End Using
    End Function

    'Private Function ExtIDToProgID(ByVal gidPluginID As System.Guid, ByVal strProgExtID As String) As Integer
    '    Dim sqlCommand As New SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@pluginid", gidPluginID.ToString))
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@extid", strProgExtID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read Then
    '        ExtIDToProgID = sqlReader.GetInt32(sqlReader.GetOrdinal("progid"))
    '    Else
    '        ExtIDToProgID = Nothing
    '    End If

    '    sqlReader.Close()
    'End Function

    Public Sub AddToHTTPCache(ByVal uri As String, ByVal requestSuccess As Boolean, ByVal data As Byte())
        'Dim sqlCommand As New SQLiteCommand("delete from httpcache where uri=@uri", sqlConnection)
        'sqlCommand.Parameters.Add(New SQLiteParameter("@uri", uri))
        'sqlCommand.ExecuteNonQuery()

        'sqlCommand = New SQLiteCommand("insert into httpcache (uri, lastfetch, success, data) values(@uri, @lastfetch, @success, @data)", sqlConnection)
        'sqlCommand.Parameters.Add(New SQLiteParameter("@uri", uri))
        'sqlCommand.Parameters.Add(New SQLiteParameter("@lastfetch", Now))
        'sqlCommand.Parameters.Add(New SQLiteParameter("@success", requestSuccess))
        'sqlCommand.Parameters.Add(New SQLiteParameter("@data", data))
        'sqlCommand.ExecuteNonQuery()
    End Sub

    Public Function GetHTTPCacheLastUpdate(ByVal uri As String) As Date
        'Dim sqlCommand As New SQLiteCommand("select lastfetch from httpcache where uri=@uri", sqlConnection)
        'sqlCommand.Parameters.Add(New SQLiteParameter("@uri", uri))
        'Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        'If sqlReader.Read Then
        '    GetHTTPCacheLastUpdate = sqlReader.GetDateTime(sqlReader.GetOrdinal("lastfetch"))
        'Else
        '    GetHTTPCacheLastUpdate = Nothing
        'End If

        'sqlReader.Close()
    End Function

    Public Function GetHTTPCacheContent(ByVal uri As String, ByRef requestSuccess As Boolean) As Byte()
        'Dim sqlCommand As New SQLiteCommand("select success, data from httpcache where uri=@uri", sqlConnection)
        'sqlCommand.Parameters.Add(New SQLiteParameter("@uri", uri))
        'Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

        'If sqlReader.Read Then
        '    requestSuccess = sqlReader.GetBoolean(sqlReader.GetOrdinal("success"))

        '    ' Get the length of the content by passing nothing to getbytes
        '    Dim intContentLength As Integer = CInt(sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, Nothing, 0, 0))
        '    Dim bteContent(intContentLength - 1) As Byte

        '    sqlReader.GetBytes(sqlReader.GetOrdinal("data"), 0, bteContent, 0, intContentLength)
        '    GetHTTPCacheContent = bteContent
        'Else
        '    GetHTTPCacheContent = Nothing
        'End If

        'sqlReader.Close()

        Return Nothing
    End Function

    'Private Function GetAvailableEpisodes(ByVal intProgID As Integer) As Integer()
    '    Dim intEpisodeIDs(-1) As Integer
    '    GetAvailableEpisodes = intEpisodeIDs

    '    Dim sqlCommand As New SQLiteCommand("select pluginid, extid from programmes where progid=@progid", sqlConnection)
    '    sqlCommand.Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '    Dim sqlReader As SQLiteDataReader = sqlCommand.ExecuteReader

    '    If sqlReader.Read = False Then
    '        Exit Function
    '    End If

    '    Dim gidProviderID As New Guid(sqlReader.GetString(sqlReader.GetOrdinal("pluginid")))
    '    Dim strProgExtID As String = sqlReader.GetString(sqlReader.GetOrdinal("extid"))

    '    sqlReader.Close()

    '    If clsPluginsInst.PluginExists(gidProviderID) = False Then
    '        Exit Function
    '    End If

    '    Dim strEpisodeExtIDs As String()
    '    Dim ThisInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(gidProviderID)

    '    Try
    '        strEpisodeExtIDs = ThisInstance.GetAvailableEpisodeIDs(strProgExtID)
    '    Catch expException As Exception
    '        ' Catch any unhandled provider exceptions
    '        Exit Function
    '    End Try

    '    Dim episodeInfoReturn As IRadioProvider.GetEpisodeInfoReturn

    '    If strEpisodeExtIDs IsNot Nothing Then
    '        ' Remove any duplicates, so that episodes don't get listed twice
    '        Dim arlExtIDs As New ArrayList()

    '        For intRemoveDups As Integer = 0 To strEpisodeExtIDs.Length - 1
    '            If arlExtIDs.Contains(strEpisodeExtIDs(intRemoveDups)) = False Then
    '                arlExtIDs.Add(strEpisodeExtIDs(intRemoveDups))
    '            End If
    '        Next

    '        strEpisodeExtIDs = New String(arlExtIDs.Count - 1) {}
    '        arlExtIDs.CopyTo(strEpisodeExtIDs)

    '        ' Reverse the array so that we fetch the oldest episodes first, and the older episodes
    '        ' get added to the download list first if we are checking subscriptions
    '        Array.Reverse(strEpisodeExtIDs)

    '        Dim sqlFindCmd As New SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", sqlConnection)
    '        Dim sqlAddEpisodeCmd As New SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", sqlConnection)
    '        Dim sqlGetRowIDCmd As New SQLiteCommand("select last_insert_rowid()", sqlConnection)
    '        Dim sqlAddExtInfoCmd As New SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", sqlConnection)

    '        For Each strEpisodeExtID As String In strEpisodeExtIDs
    '            With sqlFindCmd
    '                .Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '                .Parameters.Add(New SQLiteParameter("@extid", strEpisodeExtID))
    '                sqlReader = .ExecuteReader
    '            End With

    '            If sqlReader.Read Then
    '                ReDim Preserve intEpisodeIDs(intEpisodeIDs.GetUpperBound(0) + 1)
    '                intEpisodeIDs(intEpisodeIDs.GetUpperBound(0)) = sqlReader.GetInt32(sqlReader.GetOrdinal("epid"))
    '            Else
    '                Try
    '                    episodeInfoReturn = ThisInstance.GetEpisodeInfo(strProgExtID, strEpisodeExtID)
    '                Catch expException As Exception
    '                    ' Catch any unhandled provider exceptions
    '                    sqlReader.Close()
    '                    Continue For
    '                End Try

    '                If episodeInfoReturn.Success = False Then
    '                    sqlReader.Close()
    '                    Continue For
    '                End If

    '                If episodeInfoReturn.EpisodeInfo.Name = "" Or episodeInfoReturn.EpisodeInfo.Date = Nothing Then
    '                    sqlReader.Close()
    '                    Continue For
    '                End If

    '                With sqlAddEpisodeCmd
    '                    .Parameters.Add(New SQLiteParameter("@progid", intProgID))
    '                    .Parameters.Add(New SQLiteParameter("@extid", strEpisodeExtID))
    '                    .Parameters.Add(New SQLiteParameter("@name", episodeInfoReturn.EpisodeInfo.Name))
    '                    .Parameters.Add(New SQLiteParameter("@description", episodeInfoReturn.EpisodeInfo.Description))
    '                    .Parameters.Add(New SQLiteParameter("@duration", episodeInfoReturn.EpisodeInfo.DurationSecs))
    '                    .Parameters.Add(New SQLiteParameter("@date", episodeInfoReturn.EpisodeInfo.Date))
    '                    .Parameters.Add(New SQLiteParameter("@image", StoreImage(episodeInfoReturn.EpisodeInfo.Image)))
    '                    .ExecuteNonQuery()
    '                End With

    '                Dim intEpID As Integer = CInt(sqlGetRowIDCmd.ExecuteScalar)

    '                If episodeInfoReturn.EpisodeInfo.ExtInfo IsNot Nothing Then
    '                    For Each kvpItem As KeyValuePair(Of String, String) In episodeInfoReturn.EpisodeInfo.ExtInfo
    '                        With sqlAddExtInfoCmd
    '                            .Parameters.Add(New SQLiteParameter("@epid", intEpID))
    '                            .Parameters.Add(New SQLiteParameter("@name", kvpItem.Key))
    '                            .Parameters.Add(New SQLiteParameter("@value", kvpItem.Value))
    '                            .ExecuteNonQuery()
    '                        End With
    '                    Next
    '                End If

    '                ReDim Preserve intEpisodeIDs(intEpisodeIDs.GetUpperBound(0) + 1)
    '                intEpisodeIDs(intEpisodeIDs.GetUpperBound(0)) = intEpID
    '            End If

    '            sqlReader.Close()
    '        Next
    '    End If

    '    Return intEpisodeIDs
    'End Function

    Public Function CompareDownloads(ByVal epid1 As Integer, ByVal epid2 As Integer) As Integer
        If downloadSortCache Is Nothing Then
            downloadSortCache = New Dictionary(Of Integer, Integer)

            Dim sort As Integer = 0

            Using command As New SQLiteCommand("select downloads.epid from downloads, episodes where downloads.epid=episodes.epid order by date desc", FetchDbConn)
                Using reader As SQLiteDataReader = command.ExecuteReader()
                    Dim epidOrdinal As Integer = reader.GetOrdinal("epid")

                    Do While reader.Read
                        downloadSortCache.Add(reader.GetInt32(epidOrdinal), sort)
                        sort += 1
                    Loop
                End Using
            End Using
        End If

        Return downloadSortCache(epid1) - downloadSortCache(epid2)
    End Function

    Public Sub InitDownloadList()
        Using command As New SQLiteCommand("select epid from downloads", FetchDbConn)
            Using reader As SQLiteDataReader = command.ExecuteReader()
                Dim epidOrdinal As Integer = reader.GetOrdinal("epid")

                Do While reader.Read
                    RaiseEvent DownloadAdded(reader.GetInt32(epidOrdinal))
                Loop
            End Using
        End Using
    End Sub

    Public Function FetchDownloadData(ByVal epid As Integer) As DownloadData
        Using command As New SQLiteCommand("select name, description, date, duration, status from downloads, episodes where downloads.epid=@epid and episodes.epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Return Nothing
                End If

                Dim descriptionOrdinal As Integer = reader.GetOrdinal("description")

                Dim info As New DownloadData
                info.name = reader.GetString(reader.GetOrdinal("name"))

                If Not reader.IsDBNull(descriptionOrdinal) Then
                    info.description = reader.GetString(descriptionOrdinal)
                End If

                info.episodedate = reader.GetDateTime(reader.GetOrdinal("date"))
                info.duration = reader.GetInt32(reader.GetOrdinal("duration"))
                info.status = DirectCast(reader.GetInt32(reader.GetOrdinal("status")), DownloadStatus)

                Return info
            End Using
        End Using
    End Function
End Class
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

    Public Structure ProviderData
        Dim name As String
        Dim description As String
        Dim icon As Bitmap
        Dim showOptionsHandler As EventHandler
    End Structure

    Public Structure EpisodeData
        Dim name As String
        Dim description As String
        Dim episodeDate As Date
        Dim duration As Integer
        Dim autoDownload As Boolean
    End Structure

    Public Structure ProgrammeData
        Dim name As String
        Dim description As String
        Dim subscribed As Boolean
        Dim singleEpisode As Boolean
    End Structure

    Public Structure SubscriptionData
        Dim name As String
        Dim description As String
        Dim latestDownload As Date
        Dim providerName As String
    End Structure

    Public Structure DownloadData
        Dim name As String
        Dim description As String
        Dim duration As Integer
        Dim episodeDate As Date
        Dim status As DownloadStatus
        Dim errorType As IRadioProvider.ErrorType
        Dim errorDetails As String
        Dim downloadPath As String
        Dim playCount As Integer
    End Structure

    <ThreadStatic()> _
    Private dbConn As SQLiteConnection

    Private Shared clsDataInstance As New Data

    Private clsPluginsInst As Plugins
    Private clsCurDldProgData As DldProgData
    Private thrDownloadThread As Thread
    Private WithEvents DownloadPluginInst As IRadioProvider
    Private WithEvents FindNewPluginInst As IRadioProvider

    Private subscriptionSortCache As Dictionary(Of Integer, Integer)
    Private subscriptionSortCacheLock As New Object
    Private downloadSortCache As Dictionary(Of Integer, Integer)
    Private downloadSortCacheLock As New Object

    Private checkSubsLock As New Object
    Private findDownloadLock As New Object

    Public Event ProviderAdded(ByVal providerId As Guid)
    Public Event FindNewViewChange(ByVal objView As Object)
    Public Event FoundNew(ByVal intProgID As Integer)
    Public Event ProgrammeUpdated(ByVal progid As Integer)
    Public Event EpisodeAdded(ByVal epid As Integer)
    'Public Event EpisodeUpdated(ByVal epid As Integer)
    Public Event SubscriptionAdded(ByVal progid As Integer)
    Public Event SubscriptionUpdated(ByVal progid As Integer)
    Public Event SubscriptionRemoved(ByVal progid As Integer)
    Public Event DownloadAdded(ByVal epid As Integer)
    Public Event DownloadUpdated(ByVal epid As Integer)
    Public Event DownloadProgress(ByVal epid As Integer, ByVal percent As Integer, ByVal statusText As String, ByVal icon As IRadioProvider.ProgressIcon)
    Public Event DownloadRemoved(ByVal epid As Integer)

    Public Shared Function GetInstance() As Data
        Return clsDataInstance
    End Function

    Private Function FetchDbConn() As SQLiteConnection
        If dbConn Is Nothing Then
            dbConn = New SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "store.db") + ";Version=3;New=False")
            dbConn.Open()
        End If

        Return dbConn
    End Function

    Private Sub New()
        MyBase.New()

        ' Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
        Call VacuumDatabase()

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

    Public Sub StartDownload()
        ThreadPool.QueueUserWorkItem(AddressOf StartDownloadAsync)
    End Sub

    Private Sub StartDownloadAsync(ByVal dummy As Object)
        Call StartDownloadAsync()
    End Sub

    Private Sub StartDownloadAsync()
        SyncLock findDownloadLock
            If thrDownloadThread Is Nothing Then
                Using command As New SQLiteCommand("select pluginid, pr.name as progname, pr.description as progdesc, pr.image as progimg, ep.name as epname, ep.description as epdesc, ep.duration, ep.date, ep.image as epimg, pr.extid as progextid, ep.extid as epextid, dl.status, ep.epid, dl.errorcount from downloads as dl, episodes as ep, programmes as pr where dl.epid=ep.epid and ep.progid=pr.progid and (dl.status=@statuswait or (dl.status=@statuserr and dl.errortime < datetime('now', '-' || power(2, dl.errorcount) || ' hours'))) order by ep.date", FetchDbConn)
                    command.Parameters.Add(New SQLiteParameter("@statuswait", DownloadStatus.Waiting))
                    command.Parameters.Add(New SQLiteParameter("@statuserr", DownloadStatus.Errored))

                    Using reader As SQLiteDataReader = command.ExecuteReader
                        While thrDownloadThread Is Nothing
                            If Not reader.Read Then
                                Return
                            End If

                            Dim pluginId As New Guid(reader.GetString(reader.GetOrdinal("pluginid")))
                            Dim epid As Integer = reader.GetInt32(reader.GetOrdinal("epid"))

                            If clsPluginsInst.PluginExists(pluginId) Then
                                clsCurDldProgData = New DldProgData

                                Dim progInfo As IRadioProvider.ProgrammeInfo
                                If reader.IsDBNull(reader.GetOrdinal("progname")) Then
                                    progInfo.Name = Nothing
                                Else
                                    progInfo.Name = reader.GetString(reader.GetOrdinal("progname"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("progdesc")) Then
                                    progInfo.Description = Nothing
                                Else
                                    progInfo.Description = reader.GetString(reader.GetOrdinal("progdesc"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("progimg")) Then
                                    progInfo.Image = Nothing
                                Else
                                    progInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("progimg")))
                                End If

                                Dim epiEpInfo As IRadioProvider.EpisodeInfo
                                If reader.IsDBNull(reader.GetOrdinal("epname")) Then
                                    epiEpInfo.Name = Nothing
                                Else
                                    epiEpInfo.Name = reader.GetString(reader.GetOrdinal("epname"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("epdesc")) Then
                                    epiEpInfo.Description = Nothing
                                Else
                                    epiEpInfo.Description = reader.GetString(reader.GetOrdinal("epdesc"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("duration")) Then
                                    epiEpInfo.DurationSecs = Nothing
                                Else
                                    epiEpInfo.DurationSecs = reader.GetInt32(reader.GetOrdinal("duration"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("date")) Then
                                    epiEpInfo.Date = Nothing
                                Else
                                    epiEpInfo.Date = reader.GetDateTime(reader.GetOrdinal("date"))
                                End If

                                If reader.IsDBNull(reader.GetOrdinal("epimg")) Then
                                    epiEpInfo.Image = Nothing
                                Else
                                    epiEpInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("epimg")))
                                End If

                                epiEpInfo.ExtInfo = New Dictionary(Of String, String)

                                Using extCommand As New SQLiteCommand("select name, value from episodeext where epid=@epid", FetchDbConn)
                                    extCommand.Parameters.Add(New SQLiteParameter("@epid", reader.GetInt32(reader.GetOrdinal("epid"))))
                                    Using extReader As SQLiteDataReader = extCommand.ExecuteReader

                                        While extReader.Read
                                            epiEpInfo.ExtInfo.Add(extReader.GetString(extReader.GetOrdinal("name")), extReader.GetString(extReader.GetOrdinal("value")))
                                        End While
                                    End Using
                                End Using

                                With clsCurDldProgData
                                    .PluginID = pluginId
                                    .ProgExtID = reader.GetString(reader.GetOrdinal("progextid"))
                                    .EpID = epid
                                    .EpisodeExtID = reader.GetString(reader.GetOrdinal("epextid"))
                                    .ProgInfo = progInfo
                                    .EpisodeInfo = epiEpInfo
                                End With

                                If reader.GetInt32(reader.GetOrdinal("status")) = DownloadStatus.Errored Then
                                    Call ResetDownloadAsync(epid, True)
                                    clsCurDldProgData.AttemptNumber = reader.GetInt32(reader.GetOrdinal("errorcount")) + 1
                                Else
                                    clsCurDldProgData.AttemptNumber = 1
                                End If

                                thrDownloadThread = New Thread(AddressOf DownloadProgThread)
                                thrDownloadThread.IsBackground = True
                                thrDownloadThread.Start()

                                Return
                            End If
                        End While
                    End Using
                End Using
            End If
        End SyncLock
    End Sub

    Public Sub DownloadProgThread()
        DownloadPluginInst = clsPluginsInst.GetPluginInstance(clsCurDldProgData.PluginID)

        Try
            ' Make sure that the temp folder still exists
            Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath, "RadioDownloader"))

            With clsCurDldProgData
                Try
                    clsCurDldProgData.FinalName = FileUtils.FindFreeSaveFileName(My.Settings.FileNameFormat, clsCurDldProgData.ProgInfo.Name, clsCurDldProgData.EpisodeInfo.Name, clsCurDldProgData.EpisodeInfo.Date, FileUtils.GetSaveFolder())
                Catch dirNotFoundExp As DirectoryNotFoundException
                    Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.", New List(Of DldErrorDataItem))
                    Exit Sub
                End Try

                DownloadPluginInst.DownloadProgramme(.ProgExtID, .EpisodeExtID, .ProgInfo, .EpisodeInfo, .FinalName, .AttemptNumber)
            End With
        Catch threadAbortExp As ThreadAbortException
            ' The download has been aborted, so ignore the exception
        Catch unknownExp As Exception
            Dim extraDetails As New List(Of DldErrorDataItem)
            extraDetails.Add(New DldErrorDataItem("error", unknownExp.GetType.ToString + ": " + unknownExp.Message))
            extraDetails.Add(New DldErrorDataItem("exceptiontostring", unknownExp.ToString))

            If unknownExp.Data IsNot Nothing Then
                For Each dataEntry As DictionaryEntry In unknownExp.Data
                    If dataEntry.Key.GetType Is GetType(String) And dataEntry.Value.GetType Is GetType(String) Then
                        extraDetails.Add(New DldErrorDataItem("expdata:Data:" + CStr(dataEntry.Key), CStr(dataEntry.Value)))
                    End If
                Next
            End If

            Call DownloadPluginInst_DldError(IRadioProvider.ErrorType.UnknownError, unknownExp.GetType.ToString + vbCrLf + unknownExp.StackTrace, extraDetails)
        End Try
    End Sub

    Public Sub UpdateProgInfoIfRequired(ByVal progid As Integer)
        ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf UpdateProgInfoIfRequiredAsync), progid)
    End Sub

    Private Sub UpdateProgInfoIfRequiredAsync(ByVal progid As Object)
        Call UpdateProgInfoIfRequiredAsync(CInt(progid))
    End Sub

    Private Sub UpdateProgInfoIfRequiredAsync(ByVal progid As Integer)
        Using command As New SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Dim providerId As New Guid(reader.GetString(reader.GetOrdinal("pluginid")))

                    If clsPluginsInst.PluginExists(providerId) Then
                        Dim pluginInstance As IRadioProvider
                        pluginInstance = clsPluginsInst.GetPluginInstance(providerId)

                        If reader.GetDateTime(reader.GetOrdinal("lastupdate")).AddDays(pluginInstance.ProgInfoUpdateFreqDays) < Now Then
                            Call StoreProgrammeInfo(providerId, reader.GetString(reader.GetOrdinal("extid")), Nothing)
                        End If
                    End If
                End If
            End Using
        End Using
    End Sub

    Public Function FetchProgrammeImage(ByVal progid As Integer) As Bitmap
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

    Public Sub EpisodeSetAutoDownload(ByVal epid As Integer, ByVal autoDownload As Boolean)
        ThreadPool.QueueUserWorkItem(AddressOf EpisodeSetAutoDownloadAsync, New ArrayList(New Object() {epid, autoDownload}))
    End Sub

    Private Sub EpisodeSetAutoDownloadAsync(ByVal params As Object)
        Dim paramList As ArrayList = DirectCast(params, ArrayList)
        EpisodeSetAutoDownloadAsync(CInt(paramList(0)), CBool(paramList(1)))
    End Sub

    Private Sub EpisodeSetAutoDownloadAsync(ByVal epid As Integer, ByVal autoDownload As Boolean)
        Using command As New SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))
            command.Parameters.Add(New SQLiteParameter("@autodownload", If(autoDownload, 1, 0)))
            command.ExecuteNonQuery()
        End Using
    End Sub

    Public Function CountDownloadsNew() As Integer
        Using command As New SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Downloaded))
            Return CInt(command.ExecuteScalar())
        End Using
    End Function

    Public Function CountDownloadsErrored() As Integer
        Using command As New SQLiteCommand("select count(epid) from downloads where status=@status", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Errored))
            Return CInt(command.ExecuteScalar())
        End Using
    End Function

    'Public Sub UpdateDlList(ByRef lstListview As ExtListView)
    '    If booErrorStatus Then
    '        Main.SetTrayStatus(False, Main.ErrorStatus.Error)
    '    Else
    '        Main.SetTrayStatus(False, Main.ErrorStatus.Normal)
    '    End If

    '    sqlReader.Close()
    'End Sub

    Public Sub CheckSubscriptions()
        ThreadPool.QueueUserWorkItem(AddressOf CheckSubscriptionsAsync)
    End Sub

    Private Sub CheckSubscriptionsAsync(ByVal dummy As Object)
        SyncLock checkSubsLock
            Using getSubsCmd As New SQLiteCommand("select progid from subscriptions", FetchDbConn), _
                  checkCmd As New SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn), _
                  findCmd As New SQLiteCommand("select epid, autodownload from episodes where progid=@progid and extid=@extid", FetchDbConn)

                Dim epidParam As New SQLiteParameter("@epid")
                Dim progidParam As New SQLiteParameter("@progid")
                Dim extidParam As New SQLiteParameter("@extid")

                findCmd.Parameters.Add(progidParam)
                findCmd.Parameters.Add(extidParam)
                checkCmd.Parameters.Add(epidParam)

                Using reader As SQLiteDataReader = getSubsCmd.ExecuteReader
                    With reader
                        Dim progidOrdinal As Integer = .GetOrdinal("progid")

                        Do While .Read()
                            Dim progid As Integer = reader.GetInt32(progidOrdinal)

                            Dim providerId As Guid
                            Dim progExtId As String

                            Using command As New SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn)
                                command.Parameters.Add(New SQLiteParameter("@progid", progid))

                                Using idReader As SQLiteDataReader = command.ExecuteReader
                                    If idReader.Read = False Then
                                        Continue Do
                                    End If

                                    providerId = New Guid(idReader.GetString(idReader.GetOrdinal("pluginid")))
                                    progExtId = idReader.GetString(idReader.GetOrdinal("extid"))
                                End Using
                            End Using

                            Dim episodeExtIds As List(Of String)

                            Try
                                episodeExtIds = GetAvailableEpisodes(providerId, progExtId)
                            Catch unhandled As Exception
                                ' Catch any unhandled provider exceptions
                                Exit Sub
                            End Try

                            If episodeExtIds IsNot Nothing Then
                                For Each episodeExtId As String In episodeExtIds
                                    progidParam.Value = progid
                                    extidParam.Value = episodeExtId

                                    Dim epid As Integer
                                    Dim autoDownload As Boolean = True

                                    Using findReader As SQLiteDataReader = findCmd.ExecuteReader
                                        If findReader.Read Then
                                            epid = findReader.GetInt32(findReader.GetOrdinal("epid"))
                                            autoDownload = findReader.GetInt32(findReader.GetOrdinal("autodownload")) = 1
                                        Else
                                            Try
                                                epid = StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId)
                                            Catch unhandled As Exception
                                                ' Catch any unhandled provider exceptions
                                                Continue For
                                            End Try

                                            If epid < 0 Then
                                                Continue For
                                            End If
                                        End If
                                    End Using

                                    If autoDownload Then
                                        epidParam.Value = epid

                                        Using sqlCheckRdr As SQLiteDataReader = checkCmd.ExecuteReader
                                            If sqlCheckRdr.Read = False Then
                                                Call AddDownloadAsync(epid)
                                            End If
                                        End Using
                                    End If
                                Next
                            End If
                        Loop
                    End With
                End Using
            End Using
        End SyncLock
    End Sub

    Public Function AddDownload(ByVal epid As Integer) As Boolean
        Using command As New SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Return False
                End If
            End Using
        End Using

        ThreadPool.QueueUserWorkItem(AddressOf AddDownloadAsync, epid)

        Return True
    End Function

    Private Sub AddDownloadAsync(ByVal epid As Object)
        AddDownloadAsync(CInt(epid))
    End Sub

    Private Sub AddDownloadAsync(ByVal epid As Integer)
        Using command As New SQLiteCommand("insert into downloads (epid, status) values (@epid, @status)", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))
            command.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Waiting))
            Call command.ExecuteNonQuery()
        End Using

        RaiseEvent DownloadAdded(epid)

        Call StartDownload()
    End Sub

    Public Function AddSubscription(ByVal progid As Integer) As Boolean
        Using command As New SQLiteCommand("select progid from subscriptions where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Return False
                End If
            End Using
        End Using

        ThreadPool.QueueUserWorkItem(AddressOf AddSubscriptionAsync, progid)

        Return True
    End Function

    Private Sub AddSubscriptionAsync(ByVal progid As Object)
        AddSubscriptionAsync(CInt(progid))
    End Sub

    Private Sub AddSubscriptionAsync(ByVal progid As Integer)
        Using command As New SQLiteCommand("insert into subscriptions (progid) values (@progid)", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))
            Call command.ExecuteNonQuery()
        End Using

        RaiseEvent ProgrammeUpdated(progid)
        RaiseEvent SubscriptionAdded(progid)
    End Sub

    Public Sub RemoveSubscription(ByVal progid As Integer)
        ThreadPool.QueueUserWorkItem(AddressOf RemoveSubscriptionAsync, progid)
    End Sub

    Private Sub RemoveSubscriptionAsync(ByVal progid As Object)
        RemoveSubscriptionAsync(CInt(progid))
    End Sub

    Private Sub RemoveSubscriptionAsync(ByVal progid As Integer)
        Using command As New SQLiteCommand("delete from subscriptions where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))
            Call command.ExecuteNonQuery()
        End Using

        RaiseEvent ProgrammeUpdated(progid)
        RaiseEvent SubscriptionRemoved(progid)
    End Sub

    Private Function LatestDownloadDate(ByVal progid As Integer) As Date
        Using command As New SQLiteCommand("select date from episodes, downloads where episodes.epid=downloads.epid and progid=@progid order by date desc limit 1", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    ' No downloads of this program
                    Return Nothing
                Else
                    Return reader.GetDateTime(reader.GetOrdinal("date"))
                End If
            End Using
        End Using
    End Function

    Public Sub ResetDownload(ByVal epid As Integer)
        ThreadPool.QueueUserWorkItem(AddressOf ResetDownloadAsync, epid)
    End Sub

    Private Sub ResetDownloadAsync(ByVal epidObj As Object)
        ResetDownloadAsync(CInt(epidObj), False)
    End Sub

    Private Sub ResetDownloadAsync(ByVal epid As Integer, ByVal auto As Boolean)
        Using trans As SQLiteTransaction = FetchDbConn.BeginTransaction
            Using command As New SQLiteCommand("update downloads set status=@status where epid=@epid", FetchDbConn, trans)
                command.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Waiting))
                command.Parameters.Add(New SQLiteParameter("@epid", epid))
                command.ExecuteNonQuery()
            End Using

            If auto = False Then
                Using command As New SQLiteCommand("update downloads set errorcount=0 where epid=@epid", FetchDbConn, trans)
                    command.Parameters.Add(New SQLiteParameter("@epid", epid))
                    command.ExecuteNonQuery()
                End Using
            End If

            trans.Commit()
        End Using

        SyncLock downloadSortCacheLock
            downloadSortCache = Nothing
        End SyncLock

        RaiseEvent DownloadUpdated(epid)

        If auto = False Then
            StartDownloadAsync()
        End If
    End Sub

    Public Sub DownloadRemove(ByVal epid As Integer)
        ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf DownloadRemoveAsync), epid)
    End Sub

    Private Sub DownloadRemoveAsync(ByVal epidObj As Object)
        DownloadRemoveAsync(CInt(epidObj))
    End Sub

    Private Sub DownloadRemoveAsync(ByVal epid As Integer)
        If clsCurDldProgData IsNot Nothing Then
            If clsCurDldProgData.EpID = epid Then
                ' The program is currently being downloaded

                If thrDownloadThread IsNot Nothing Then
                    thrDownloadThread.Abort()
                    thrDownloadThread = Nothing
                End If

                StartDownload()
            End If
        End If

        Using trans As SQLiteTransaction = FetchDbConn.BeginTransaction
            Using command As New SQLiteCommand("delete from downloads where epid=@epid", FetchDbConn, trans)
                command.Parameters.Add(New SQLiteParameter("@epid", epid))
                command.ExecuteNonQuery()
            End Using

            ' Unet the auto download flag, so if the user is subscribed it doesn't just download again
            EpisodeSetAutoDownloadAsync(epid, False)

            trans.Commit()
        End Using

        ' No need to clear the sort cache, as this will still work with the extra entry

        RaiseEvent DownloadRemoved(epid)
    End Sub

    Public Sub DownloadBumpPlayCount(ByVal epid As Integer)
        ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf DownloadBumpPlayCountAsync), epid)
    End Sub

    Private Sub DownloadBumpPlayCountAsync(ByVal epidObj As Object)
        DownloadBumpPlayCountAsync(CInt(epidObj))
    End Sub

    Private Sub DownloadBumpPlayCountAsync(ByVal epid As Integer)
        Using command As New SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))
            command.ExecuteNonQuery()
        End Using

        SyncLock downloadSortCacheLock
            downloadSortCache = Nothing
        End SyncLock

        RaiseEvent DownloadUpdated(epid)
    End Sub

    Public Sub DownloadReportError(ByVal epid As Integer)
        Dim errorType As IRadioProvider.ErrorType
        Dim errorText As String = Nothing
        Dim extraDetailsString As String
        Dim errorExtraDetails As New Dictionary(Of String, String)

        Dim detailsSerializer As New XmlSerializer(GetType(List(Of DldErrorDataItem)))

        Using command As New SQLiteCommand("select errortype, errordetails, ep.name as epname, ep.description as epdesc, date, duration, ep.extid as epextid, pr.name as progname, pr.description as progdesc, pr.extid as progextid, pluginid from downloads as dld, episodes as ep, programmes as pr where dld.epid=@epid and ep.epid=@epid and ep.progid=pr.progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If Not reader.Read Then
                    Throw New ArgumentException("Episode " + epid.ToString + " does not exit, or is not in the download list!", "epid")
                End If

                errorType = CType(reader.GetInt32(reader.GetOrdinal("errortype")), IRadioProvider.ErrorType)
                extraDetailsString = reader.GetString(reader.GetOrdinal("errordetails"))

                errorExtraDetails.Add("episode:name", reader.GetString(reader.GetOrdinal("epname")))
                errorExtraDetails.Add("episode:description", reader.GetString(reader.GetOrdinal("epdesc")))
                errorExtraDetails.Add("episode:date", reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture))
                errorExtraDetails.Add("episode:duration", CStr(reader.GetInt32(reader.GetOrdinal("duration"))))
                errorExtraDetails.Add("episode:extid", reader.GetString(reader.GetOrdinal("epextid")))

                errorExtraDetails.Add("programme:name", reader.GetString(reader.GetOrdinal("progname")))
                errorExtraDetails.Add("programme:description", reader.GetString(reader.GetOrdinal("progdesc")))
                errorExtraDetails.Add("programme:extid", reader.GetString(reader.GetOrdinal("progextid")))

                Dim pluginId As New Guid(reader.GetString(reader.GetOrdinal("pluginid")))
                Dim providerInst As IRadioProvider = clsPluginsInst.GetPluginInstance(pluginId)

                errorExtraDetails.Add("provider:id", pluginId.ToString)
                errorExtraDetails.Add("provider:name", providerInst.ProviderName)
                errorExtraDetails.Add("provider:description", providerInst.ProviderDescription)
            End Using
        End Using

        If extraDetailsString IsNot Nothing Then
            Try
                Dim extraDetails As List(Of DldErrorDataItem)
                extraDetails = DirectCast(detailsSerializer.Deserialize(New StringReader(extraDetailsString)), List(Of DldErrorDataItem))

                For Each detailItem As DldErrorDataItem In extraDetails
                    Select Case detailItem.Name
                        Case "error"
                            errorText = detailItem.Data
                        Case "details"
                            extraDetailsString = detailItem.Data
                        Case Else
                            errorExtraDetails.Add(detailItem.Name, detailItem.Data)
                    End Select
                Next
            Catch invalidOperationExp As InvalidOperationException
                ' Do nothing, and fall back to reporting all the details as one string
            Catch invalidCastExp As InvalidCastException
                ' Do nothing, and fall back to reporting all the details as one string
            End Try
        End If

        If errorText Is Nothing OrElse errorText = String.Empty Then
            errorText = errorType.ToString
        End If

        Dim report As New ErrorReporting("Download Error: " + errorText, extraDetailsString, errorExtraDetails)
        report.SendReport(My.Settings.ErrorReportURL)
    End Sub

    Private Sub DownloadPluginInst_DldError(ByVal errorType As IRadioProvider.ErrorType, ByVal errorDetails As String, ByVal furtherDetails As List(Of DldErrorDataItem)) Handles DownloadPluginInst.DldError
        Select Case errorType
            Case IRadioProvider.ErrorType.RemoveFromList
                Call DownloadRemoveAsync(clsCurDldProgData.EpID)
                Return
            Case IRadioProvider.ErrorType.UnknownError
                furtherDetails.Add(New DldErrorDataItem("details", errorDetails))

                Dim detailsStringWriter As New StringWriter(CultureInfo.InvariantCulture)
                Dim detailsSerializer As New XmlSerializer(GetType(List(Of DldErrorDataItem)))
                detailsSerializer.Serialize(detailsStringWriter, furtherDetails)
                errorDetails = detailsStringWriter.ToString
        End Select

        Using sqlCommand As New SQLiteCommand("update downloads set status=@status, errortime=datetime('now'), errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", FetchDbConn)
            sqlCommand.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Errored))
            sqlCommand.Parameters.Add(New SQLiteParameter("@errortype", errorType))
            sqlCommand.Parameters.Add(New SQLiteParameter("@errordetails", errorDetails))
            sqlCommand.Parameters.Add(New SQLiteParameter("@epid", clsCurDldProgData.EpID))
            sqlCommand.ExecuteNonQuery()
        End Using

        SyncLock downloadSortCacheLock
            downloadSortCache = Nothing
        End SyncLock

        RaiseEvent DownloadUpdated(clsCurDldProgData.EpID)

        thrDownloadThread = Nothing
        clsCurDldProgData = Nothing

        Call StartDownloadAsync()
    End Sub

    Private Sub DownloadPluginInst_Finished(ByVal strFileExtension As String) Handles DownloadPluginInst.Finished
        clsCurDldProgData.FinalName += "." + strFileExtension

        Using command As New SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@status", DownloadStatus.Downloaded))
            command.Parameters.Add(New SQLiteParameter("@filepath", clsCurDldProgData.FinalName))
            command.Parameters.Add(New SQLiteParameter("@epid", clsCurDldProgData.EpID))
            command.ExecuteNonQuery()
        End Using

        SyncLock downloadSortCacheLock
            downloadSortCache = Nothing
        End SyncLock

        RaiseEvent DownloadUpdated(clsCurDldProgData.EpID)

        If My.Settings.RunAfterCommand <> "" Then
            Try
                ' Environ("comspec") will give the path to cmd.exe or command.com
                Call Shell("""" + Environ("comspec") + """ /c " + My.Settings.RunAfterCommand.Replace("%file%", clsCurDldProgData.FinalName), AppWinStyle.NormalNoFocus)
            Catch
                ' Just ignore the error, as it just means that something has gone wrong with the run after command.
            End Try
        End If

        thrDownloadThread = Nothing
        clsCurDldProgData = Nothing

        Call StartDownloadAsync()
    End Sub

    Private Sub DownloadPluginInst_Progress(ByVal intPercent As Integer, ByVal strStatusText As String, ByVal Icon As IRadioProvider.ProgressIcon) Handles DownloadPluginInst.Progress
        Static lastNum As Integer = -1

        ' Don't raise the progress event if the value is the same as last time, or is outside the range
        If intPercent = lastNum OrElse intPercent < 0 OrElse intPercent > 100 Then
            Exit Sub
        End If

        lastNum = intPercent

        RaiseEvent DownloadProgress(clsCurDldProgData.EpID, intPercent, strStatusText, Icon)
    End Sub

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

    Private Sub FindNewPluginInst_FoundNew(ByVal progExtId As String) Handles FindNewPluginInst.FoundNew
        Dim pluginId As Guid = FindNewPluginInst.ProviderID
        Dim pluginExp As Exception = Nothing

        If StoreProgrammeInfo(pluginId, progExtId, pluginExp) = False Then
            If pluginExp IsNot Nothing Then
                If MsgBox("A problem was encountered while attempting to retrieve information about this programme." + Environment.NewLine + "Would you like to report this to NerdoftheHerd.com to help us improve Radio Downloader?", MsgBoxStyle.YesNo Or MsgBoxStyle.Exclamation) = MsgBoxResult.Yes Then
                    Dim clsReport As New ErrorReporting(pluginExp)
                    clsReport.SendReport(My.Settings.ErrorReportURL)
                End If

                Exit Sub
            Else
                Call MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation)
                Exit Sub
            End If
        End If

        Dim progid As Integer = ExtIDToProgID(pluginId, progExtId)
        RaiseEvent FoundNew(progid)
    End Sub

    Private Function StoreProgrammeInfo(ByVal pluginId As System.Guid, ByVal progExtId As String, ByRef pluginExp As Exception) As Boolean
        If clsPluginsInst.PluginExists(pluginId) = False Then
            Return False
        End If

        Dim pluginInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(pluginId)
        Dim progInfo As IRadioProvider.GetProgrammeInfoReturn

        Try
            progInfo = pluginInstance.GetProgrammeInfo(progExtId)
        Catch pluginExp
            ' Catch unhandled errors in the plugin
            Return False
        End Try

        If progInfo.Success = False Then
            Return False
        End If

        Dim progid As Integer = ExtIDToProgID(pluginId, progExtId)

        If progid = Nothing Then
            Using command As New SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", FetchDbConn)
                command.Parameters.Add(New SQLiteParameter("@pluginid", pluginId.ToString))
                command.Parameters.Add(New SQLiteParameter("@extid", progExtId))
                command.ExecuteNonQuery()
            End Using

            Using command As New SQLiteCommand("select last_insert_rowid()", FetchDbConn)
                progid = CInt(command.ExecuteScalar)
            End Using
        End If

        Using command As New SQLiteCommand("update programmes set name=@name, description=@description, image=@image, singleepisode=@singleepisode, lastupdate=@lastupdate where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@name", progInfo.ProgrammeInfo.Name))
            command.Parameters.Add(New SQLiteParameter("@description", progInfo.ProgrammeInfo.Description))
            command.Parameters.Add(New SQLiteParameter("@image", StoreImage(progInfo.ProgrammeInfo.Image)))
            command.Parameters.Add(New SQLiteParameter("@singleepisode", progInfo.ProgrammeInfo.SingleEpisode))
            command.Parameters.Add(New SQLiteParameter("@lastupdate", Now))
            command.Parameters.Add(New SQLiteParameter("@progid", progid))
            command.ExecuteNonQuery()
        End Using

        ' If the programme is in the list of subscriptions, clear the sort cache and raise an updated event
        Using command As New SQLiteCommand("select progid from subscriptions where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    SyncLock subscriptionSortCacheLock
                        subscriptionSortCache = Nothing
                    End SyncLock

                    RaiseEvent SubscriptionUpdated(progid)
                End If
            End Using
        End Using

        Return True
    End Function

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

    Private Function ExtIDToProgID(ByVal pluginID As System.Guid, ByVal progExtId As String) As Integer
        Using command As New SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@pluginid", pluginID.ToString))
            command.Parameters.Add(New SQLiteParameter("@extid", progExtId))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read Then
                    Return reader.GetInt32(reader.GetOrdinal("progid"))
                Else
                    Return Nothing
                End If
            End Using
        End Using
    End Function

    Private Function GetAvailableEpisodes(ByVal providerId As Guid, ByVal progExtId As String) As List(Of String)
        If clsPluginsInst.PluginExists(providerId) = False Then
            Return Nothing
        End If

        Dim extIds As String()
        Dim providerInst As IRadioProvider = clsPluginsInst.GetPluginInstance(providerId)

        extIds = providerInst.GetAvailableEpisodeIDs(progExtId)

        If extIds Is Nothing Then
            Return Nothing
        End If

        ' Remove any duplicates from the list of episodes
        Dim extIdsUnique As New List(Of String)

        For Each removeDups As String In extIds
            If extIdsUnique.Contains(removeDups) = False Then
                extIdsUnique.Add(removeDups)
            End If
        Next

        Return extIdsUnique
    End Function

    Private Function StoreEpisodeInfo(ByVal pluginId As Guid, ByVal progid As Integer, ByVal progExtId As String, ByVal episodeExtId As String) As Integer
        Dim providerInst As IRadioProvider = clsPluginsInst.GetPluginInstance(pluginId)
        Dim episodeInfoReturn As IRadioProvider.GetEpisodeInfoReturn

        episodeInfoReturn = providerInst.GetEpisodeInfo(progExtId, episodeExtId)

        If episodeInfoReturn.Success = False Then
            Return -1
        End If

        If episodeInfoReturn.EpisodeInfo.Name Is Nothing OrElse episodeInfoReturn.EpisodeInfo.Name = String.Empty Then
            Throw New InvalidDataException("Episode name cannot be nothing or an empty string")
        End If

        If episodeInfoReturn.EpisodeInfo.Date = Nothing Then
            Throw New InvalidDataException("Episode date cannot be nothing or an empty string")
        End If

        Using trans As SQLiteTransaction = FetchDbConn.BeginTransaction
            Using addEpisodeCmd As New SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", FetchDbConn, trans)
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@progid", progid))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@extid", episodeExtId))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@name", episodeInfoReturn.EpisodeInfo.Name))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@description", episodeInfoReturn.EpisodeInfo.Description))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@duration", episodeInfoReturn.EpisodeInfo.DurationSecs))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@date", episodeInfoReturn.EpisodeInfo.Date))
                addEpisodeCmd.Parameters.Add(New SQLiteParameter("@image", StoreImage(episodeInfoReturn.EpisodeInfo.Image)))
                addEpisodeCmd.ExecuteNonQuery()
            End Using

            Dim epid As Integer

            Using getRowIDCmd As New SQLiteCommand("select last_insert_rowid()", FetchDbConn, trans)
                epid = CInt(getRowIDCmd.ExecuteScalar)
            End Using

            If episodeInfoReturn.EpisodeInfo.ExtInfo IsNot Nothing Then
                Using addExtInfoCmd As New SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", FetchDbConn, trans)
                    For Each extItem As KeyValuePair(Of String, String) In episodeInfoReturn.EpisodeInfo.ExtInfo
                        With addExtInfoCmd
                            .Parameters.Add(New SQLiteParameter("@epid", epid))
                            .Parameters.Add(New SQLiteParameter("@name", extItem.Key))
                            .Parameters.Add(New SQLiteParameter("@value", extItem.Value))
                            .ExecuteNonQuery()
                        End With
                    Next
                End Using
            End If

            trans.Commit()
            Return epid
        End Using
    End Function

    Public Function CompareDownloads(ByVal epid1 As Integer, ByVal epid2 As Integer) As Integer
        SyncLock downloadSortCacheLock
            If downloadSortCache Is Nothing OrElse Not downloadSortCache.ContainsKey(epid1) OrElse Not downloadSortCache.ContainsKey(epid2) Then
                ' The sort cache is either empty or missing one of the values that are required, so recreate it
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
        End SyncLock
    End Function

    Public Function CompareSubscriptions(ByVal progid1 As Integer, ByVal progid2 As Integer) As Integer
        SyncLock subscriptionSortCacheLock
            If subscriptionSortCache Is Nothing OrElse Not subscriptionSortCache.ContainsKey(progid1) OrElse Not subscriptionSortCache.ContainsKey(progid2) Then
                ' The sort cache is either empty or missing one of the values that are required, so recreate it
                subscriptionSortCache = New Dictionary(Of Integer, Integer)

                Dim sort As Integer = 0

                Using command As New SQLiteCommand("select subscriptions.progid from subscriptions, programmes where programmes.progid=subscriptions.progid order by name", FetchDbConn)
                    Using reader As SQLiteDataReader = command.ExecuteReader()
                        Dim progidOrdinal As Integer = reader.GetOrdinal("progid")

                        Do While reader.Read
                            subscriptionSortCache.Add(reader.GetInt32(progidOrdinal), sort)
                            sort += 1
                        Loop
                    End Using
                End Using
            End If

            Return subscriptionSortCache(progid1) - subscriptionSortCache(progid2)
        End SyncLock
    End Function

    Public Sub InitProviderList()
        Dim pluginIdList() As Guid
        pluginIdList = clsPluginsInst.GetPluginIdList

        For Each pluginId As Guid In pluginIdList
            RaiseEvent ProviderAdded(pluginId)
        Next
    End Sub

    Public Sub InitEpisodeList(ByVal progid As Integer)
        Dim providerId As Guid
        Dim progExtId As String

        Using command As New SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Exit Sub
                End If

                providerId = New Guid(reader.GetString(reader.GetOrdinal("pluginid")))
                progExtId = reader.GetString(reader.GetOrdinal("extid"))
            End Using
        End Using

        Dim episodeExtIDs As List(Of String) = GetAvailableEpisodes(providerId, progExtId)

        If episodeExtIDs IsNot Nothing Then
            Using findCmd As New SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", FetchDbConn)
                Dim progidParam As New SQLiteParameter("@progid")
                Dim extidParam As New SQLiteParameter("@extid")

                findCmd.Parameters.Add(progidParam)
                findCmd.Parameters.Add(extidParam)

                For Each episodeExtId As String In episodeExtIDs
                    progidParam.Value = progid
                    extidParam.Value = episodeExtId

                    Using reader As SQLiteDataReader = findCmd.ExecuteReader
                        Dim epid As Integer

                        If reader.Read Then
                            epid = reader.GetInt32(reader.GetOrdinal("epid"))
                        Else
                            epid = StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId)

                            If epid < 0 Then
                                Continue For
                            End If
                        End If

                        RaiseEvent EpisodeAdded(epid)
                    End Using
                Next
            End Using
        End If
    End Sub

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

    Public Sub InitSubscriptionList()
        Using command As New SQLiteCommand("select progid from subscriptions", FetchDbConn)
            Using reader As SQLiteDataReader = command.ExecuteReader()
                Dim progidOrdinal As Integer = reader.GetOrdinal("progid")

                Do While reader.Read
                    RaiseEvent SubscriptionAdded(reader.GetInt32(progidOrdinal))
                Loop
            End Using
        End Using
    End Sub

    Public Function FetchDownloadData(ByVal epid As Integer) As DownloadData
        Using command As New SQLiteCommand("select name, description, date, duration, status, errortype, errordetails, filepath, playcount from downloads, episodes where downloads.epid=@epid and episodes.epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Return Nothing
                End If

                Dim descriptionOrdinal As Integer = reader.GetOrdinal("description")
                Dim filepathOrdinal As Integer = reader.GetOrdinal("filepath")

                Dim info As New DownloadData
                info.name = reader.GetString(reader.GetOrdinal("name"))

                If Not reader.IsDBNull(descriptionOrdinal) Then
                    info.description = reader.GetString(descriptionOrdinal)
                End If

                info.episodeDate = reader.GetDateTime(reader.GetOrdinal("date"))
                info.duration = reader.GetInt32(reader.GetOrdinal("duration"))
                info.status = DirectCast(reader.GetInt32(reader.GetOrdinal("status")), DownloadStatus)

                If info.status = DownloadStatus.Errored Then
                    info.errorType = CType(reader.GetInt32(reader.GetOrdinal("errortype")), IRadioProvider.ErrorType)

                    If info.errorType <> IRadioProvider.ErrorType.UnknownError Then
                        info.errorDetails = reader.GetString(reader.GetOrdinal("errordetails"))
                    End If
                End If

                If Not reader.IsDBNull(filepathOrdinal) Then
                    info.downloadPath = reader.GetString(filepathOrdinal)
                End If

                info.playCount = reader.GetInt32(reader.GetOrdinal("playcount"))

                Return info
            End Using
        End Using
    End Function

    Public Function FetchSubscriptionData(ByVal progid As Integer) As SubscriptionData
        Using command As New SQLiteCommand("select name, description, pluginid from programmes where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Return Nothing
                End If

                Dim descriptionOrdinal As Integer = reader.GetOrdinal("description")

                Dim info As New SubscriptionData
                info.name = reader.GetString(reader.GetOrdinal("name"))

                If Not reader.IsDBNull(descriptionOrdinal) Then
                    info.description = reader.GetString(descriptionOrdinal)
                End If

                info.latestDownload = LatestDownloadDate(progid)

                Dim pluginId As New Guid(reader.GetString(reader.GetOrdinal("pluginid")))
                Dim providerInst As IRadioProvider = clsPluginsInst.GetPluginInstance(pluginId)
                info.providerName = providerInst.ProviderName

                Return info
            End Using
        End Using
    End Function

    Public Function FetchEpisodeData(ByVal epid As Integer) As EpisodeData
        Using command As New SQLiteCommand("select name, description, date, duration, autodownload from episodes where epid=@epid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@epid", epid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Return Nothing
                End If

                Dim descriptionOrdinal As Integer = reader.GetOrdinal("description")

                Dim info As New EpisodeData
                info.name = reader.GetString(reader.GetOrdinal("name"))

                If Not reader.IsDBNull(descriptionOrdinal) Then
                    info.description = reader.GetString(descriptionOrdinal)
                End If

                info.episodeDate = reader.GetDateTime(reader.GetOrdinal("date"))
                info.duration = reader.GetInt32(reader.GetOrdinal("duration"))
                info.autoDownload = reader.GetInt32(reader.GetOrdinal("autodownload")) = 1

                Return info
            End Using
        End Using
    End Function

    Public Function FetchProgrammeData(ByVal progid As Integer) As ProgrammeData
        Dim info As New ProgrammeData

        Using command As New SQLiteCommand("select name, description, singleepisode from programmes where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                If reader.Read = False Then
                    Return Nothing
                End If

                Dim descriptionOrdinal As Integer = reader.GetOrdinal("description")

                info.name = reader.GetString(reader.GetOrdinal("name"))

                If Not reader.IsDBNull(descriptionOrdinal) Then
                    info.description = reader.GetString(descriptionOrdinal)
                End If

                info.singleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"))
            End Using
        End Using

        Using command As New SQLiteCommand("select progid from subscriptions where progid=@progid", FetchDbConn)
            command.Parameters.Add(New SQLiteParameter("@progid", progid))

            Using reader As SQLiteDataReader = command.ExecuteReader
                info.subscribed = reader.Read
            End Using
        End Using

        Return info
    End Function

    Public Function FetchProviderData(ByVal providerId As Guid) As ProviderData
        Dim providerInstance As IRadioProvider = clsPluginsInst.GetPluginInstance(providerId)

        Dim info As New ProviderData
        info.name = providerInstance.ProviderName
        info.description = providerInstance.ProviderDescription
        info.icon = providerInstance.ProviderIcon
        info.showOptionsHandler = providerInstance.GetShowOptionsHandler

        Return info
    End Function
End Class
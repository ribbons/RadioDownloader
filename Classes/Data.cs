using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.


using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
namespace RadioDld
{

	internal class Data
	{
		public enum DownloadStatus
		{
			Waiting = 0,
			Downloaded = 1,
			Errored = 2
		}

		public enum DownloadCols
		{
			EpisodeName = 0,
			EpisodeDate = 1,
			Status = 2,
			Progress = 3,
			Duration = 4
		}

		public struct ProviderData
		{
			public string name;
			public string description;
			public Bitmap icon;
			public EventHandler showOptionsHandler;
		}

		public struct EpisodeData
		{
			public string name;
			public string description;
			public System.DateTime episodeDate;
			public int duration;
			public bool autoDownload;
		}

		public struct ProgrammeData
		{
			public string name;
			public string description;
			public bool favourite;
			public bool subscribed;
			public bool singleEpisode;
		}

		public struct FavouriteData
		{
			public int progid;
			public string name;
			public string description;
			public bool subscribed;
			public bool singleEpisode;
			public string providerName;
		}

		public struct SubscriptionData
		{
			public string name;
			public string description;
			public bool favourite;
			public System.DateTime latestDownload;
			public string providerName;
		}

		public struct DownloadData
		{
			public int epid;
			public string name;
			public string description;
			public int duration;
			public System.DateTime episodeDate;
			public DownloadStatus status;
			public ErrorType errorType;
			public string errorDetails;
			public string downloadPath;
			public int playCount;
		}

		[ThreadStatic()]

		private static SQLiteConnection dbConn;
		private static Data dataInstance;

		private static object dataInstanceLock = new object();

		private object dbUpdateLock = new object();
		private Plugins pluginsInst;

		private DataSearch search;
		private Thread episodeListThread;

		private object episodeListThreadLock = new object();
		private DldProgData curDldProgData;
		private Thread downloadThread;
		private IRadioProvider withEventsField_DownloadPluginInst;
		private IRadioProvider DownloadPluginInst {
			get { return withEventsField_DownloadPluginInst; }
			set {
				if (withEventsField_DownloadPluginInst != null) {
					withEventsField_DownloadPluginInst.Finished -= DownloadPluginInst_Finished;
					withEventsField_DownloadPluginInst.Progress -= DownloadPluginInst_Progress;
				}
				withEventsField_DownloadPluginInst = value;
				if (withEventsField_DownloadPluginInst != null) {
                    withEventsField_DownloadPluginInst.Finished += DownloadPluginInst_Finished;
					withEventsField_DownloadPluginInst.Progress += DownloadPluginInst_Progress;
				}
			}
		}
		private IRadioProvider withEventsField_FindNewPluginInst;
		private IRadioProvider FindNewPluginInst {
			get { return withEventsField_FindNewPluginInst; }
			set {
				if (withEventsField_FindNewPluginInst != null) {
					withEventsField_FindNewPluginInst.FindNewException -= FindNewPluginInst_FindNewException;
					withEventsField_FindNewPluginInst.FindNewViewChange -= FindNewPluginInst_FindNewViewChange;
					withEventsField_FindNewPluginInst.FoundNew -= FindNewPluginInst_FoundNew;
				}
				withEventsField_FindNewPluginInst = value;
				if (withEventsField_FindNewPluginInst != null) {
					withEventsField_FindNewPluginInst.FindNewException += FindNewPluginInst_FindNewException;
					withEventsField_FindNewPluginInst.FindNewViewChange += FindNewPluginInst_FindNewViewChange;
					withEventsField_FindNewPluginInst.FoundNew += FindNewPluginInst_FoundNew;
				}
			}

		}
		private Dictionary<int, int> favouriteSortCache;

		private object favouriteSortCacheLock = new object();
		private Dictionary<int, int> subscriptionSortCache;

		private object subscriptionSortCacheLock = new object();
		private DownloadCols downloadSortBy = DownloadCols.EpisodeDate;
		private bool downloadSortAsc;
		private Dictionary<int, int> downloadSortCache;

		private object downloadSortCacheLock = new object();

		private object findDownloadLock = new object();
		public event ProviderAddedEventHandler ProviderAdded;
		public delegate void ProviderAddedEventHandler(Guid providerId);
		public event FindNewViewChangeEventHandler FindNewViewChange;
		public delegate void FindNewViewChangeEventHandler(object viewData);
		public event FoundNewEventHandler FoundNew;
		public delegate void FoundNewEventHandler(int progid);
		public event ProgrammeUpdatedEventHandler ProgrammeUpdated;
		public delegate void ProgrammeUpdatedEventHandler(int progid);
		public event EpisodeAddedEventHandler EpisodeAdded;
		public delegate void EpisodeAddedEventHandler(int epid);
		public event FavouriteAddedEventHandler FavouriteAdded;
		public delegate void FavouriteAddedEventHandler(int progid);
		public event FavouriteUpdatedEventHandler FavouriteUpdated;
		public delegate void FavouriteUpdatedEventHandler(int progid);
		public event FavouriteRemovedEventHandler FavouriteRemoved;
		public delegate void FavouriteRemovedEventHandler(int progid);
		public event SubscriptionAddedEventHandler SubscriptionAdded;
		public delegate void SubscriptionAddedEventHandler(int progid);
		public event SubscriptionUpdatedEventHandler SubscriptionUpdated;
		public delegate void SubscriptionUpdatedEventHandler(int progid);
		public event SubscriptionRemovedEventHandler SubscriptionRemoved;
		public delegate void SubscriptionRemovedEventHandler(int progid);
		public event DownloadAddedEventHandler DownloadAdded;
		public delegate void DownloadAddedEventHandler(int epid);
		public event DownloadUpdatedEventHandler DownloadUpdated;
		public delegate void DownloadUpdatedEventHandler(int epid);
		public event DownloadProgressEventHandler DownloadProgress;
		public delegate void DownloadProgressEventHandler(int epid, int percent, string statusText, ProgressIcon icon);
		public event DownloadRemovedEventHandler DownloadRemoved;
		public delegate void DownloadRemovedEventHandler(int epid);
		public event DownloadProgressTotalEventHandler DownloadProgressTotal;
		public delegate void DownloadProgressTotalEventHandler(bool downloading, int percent);

		public static Data GetInstance()
		{
			// Need to use a lock instead of declaring the instance variable as New, as otherwise
			// on first run the constructor gets called before the template database is in place
			lock (dataInstanceLock) {
				if (dataInstance == null) {
					dataInstance = new Data();
				}

				return dataInstance;
			}
		}

		private SQLiteConnection FetchDbConn()
		{
			if (dbConn == null) {
				dbConn = new SQLiteConnection("Data Source=" + Path.Combine(FileUtils.GetAppDataFolder(), "store.db") + ";Version=3;New=False");
				dbConn.Open();
			}

			return dbConn;
		}

		private Data() : base()
		{

			// Vacuum the database every so often.  Works best as the first command, as reduces risk of conflicts.
			VacuumDatabase();

			// Setup an instance of the plugins class
			pluginsInst = new Plugins(RadioDld.My.MyProject.Application.Info.DirectoryPath);

			// Fetch the version of the database
			int currentVer = 0;

			if (GetDBSetting("databaseversion") == null) {
				currentVer = 1;
			} else {
				currentVer = Convert.ToInt32(GetDBSetting("databaseversion"));
			}

			// Set the current database version.  This is done before the upgrades are attempted so that
			// if the upgrade throws an exception this can be reported, but the programme will run next time.
			// NB: Remember to change default version in the database if this next line is changed!
			SetDBSetting("databaseversion", 3);

			switch (currentVer) {
				case 2:
					UpgradeDBv2to3();
					break;
				case 3:
					break;
				// Nothing to do, this is the current version.
			}

			search = DataSearch.GetInstance(this);

			// Start regularly checking for new subscriptions in the background
			ThreadPool.QueueUserWorkItem(() => CheckSubscriptions());
		}

		private void UpgradeDBv2to3()
		{
			int count = 0;
			string[] unusedTables = {
				"tblDownloads",
				"tblInfo",
				"tblLastFetch",
				"tblSettings",
				"tblStationVisibility",
				"tblSubscribed"
			};

			My.MyProject.Forms.Status.StatusText = "Removing unused tables...";
			My.MyProject.Forms.Status.ProgressBarMarquee = false;
			My.MyProject.Forms.Status.ProgressBarValue = 0;
			My.MyProject.Forms.Status.ProgressBarMax = unusedTables.GetUpperBound(0) + 1;
			My.MyProject.Forms.Status.Show();
			Application.DoEvents();

			// Delete the unused (v0.4 era) tables if they exist
			foreach (string unusedTable in unusedTables) {
				My.MyProject.Forms.Status.StatusText = "Removing unused table " + Convert.ToString(count) + " of " + Convert.ToString(unusedTables.GetUpperBound(0) + 1) + "...";
				My.MyProject.Forms.Status.ProgressBarValue = count;
				Application.DoEvents();

				lock (dbUpdateLock) {
					using (SQLiteCommand command = new SQLiteCommand("drop table if exists " + unusedTable, FetchDbConn())) {
						command.ExecuteNonQuery();
					}
				}

				count += 1;
			}

			My.MyProject.Forms.Status.StatusText = "Finished removing unused tables";
			My.MyProject.Forms.Status.ProgressBarValue = count;
			Application.DoEvents();

			// Work through the images and re-save them to ensure they are compressed
			List<int> compressImages = new List<int>();

			using (SQLiteCommand command = new SQLiteCommand("select imgid from images", FetchDbConn())) {
				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					while (reader.Read()) {
						compressImages.Add(reader.GetInt32(reader.GetOrdinal("imgid")));
					}
				}
			}

			My.MyProject.Forms.Status.StatusText = "Compressing images...";
			My.MyProject.Forms.Status.ProgressBarValue = 0;
			My.MyProject.Forms.Status.ProgressBarMax = compressImages.Count;
			Application.DoEvents();

			using (SQLiteCommand deleteCmd = new SQLiteCommand("delete from images where imgid=@imgid", FetchDbConn)) {
				using (SQLiteCommand updateProgs = new SQLiteCommand("update programmes set image=@newimgid where image=@oldimgid", FetchDbConn)) {
					using (SQLiteCommand updateEps = new SQLiteCommand("update episodes set image=@newimgid where image=@oldimgid", FetchDbConn())) {

						int newImageID = 0;
						Bitmap image = null;
						count = 1;

						lock (dbUpdateLock) {
							foreach (int oldImageID in compressImages) {
								My.MyProject.Forms.Status.StatusText = "Compressing image " + Convert.ToString(count) + " of " + Convert.ToString(compressImages.Count) + "...";
								My.MyProject.Forms.Status.ProgressBarValue = count - 1;
								Application.DoEvents();

								image = RetrieveImage(oldImageID);

								deleteCmd.Parameters.Add(new SQLiteParameter("@imgid", oldImageID));
								deleteCmd.ExecuteNonQuery();

								newImageID = StoreImage(image);
								Application.DoEvents();

								updateProgs.Parameters.Add(new SQLiteParameter("@oldimgid", oldImageID));
								updateProgs.Parameters.Add(new SQLiteParameter("@newimgid", newImageID));

								updateProgs.ExecuteNonQuery();

								updateEps.Parameters.Add(new SQLiteParameter("@oldimgid", oldImageID));
								updateEps.Parameters.Add(new SQLiteParameter("@newimgid", newImageID));

								updateEps.ExecuteNonQuery();

								count += 1;
							}
						}
					}
				}
			}

			My.MyProject.Forms.Status.StatusText = "Finished compressing images";
			My.MyProject.Forms.Status.ProgressBarValue = compressImages.Count;
			Application.DoEvents();

			My.MyProject.Forms.Status.Hide();
			Application.DoEvents();
		}

		public void StartDownload()
		{
			ThreadPool.QueueUserWorkItem(() => StartDownloadAsync());
		}

		private void StartDownloadAsync()
		{
			lock (findDownloadLock) {
				if (downloadThread == null) {
					using (SQLiteCommand command = new SQLiteCommand("select pluginid, pr.name as progname, pr.description as progdesc, pr.image as progimg, ep.name as epname, ep.description as epdesc, ep.duration, ep.date, ep.image as epimg, pr.extid as progextid, ep.extid as epextid, dl.status, ep.epid from downloads as dl, episodes as ep, programmes as pr where dl.epid=ep.epid and ep.progid=pr.progid and (dl.status=@statuswait or (dl.status=@statuserr and dl.errortime < datetime('now', '-' || power(2, dl.errorcount) || ' hours'))) order by ep.date", FetchDbConn())) {
						command.Parameters.Add(new SQLiteParameter("@statuswait", DownloadStatus.Waiting));
						command.Parameters.Add(new SQLiteParameter("@statuserr", DownloadStatus.Errored));

						using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
							while (downloadThread == null) {
								if (!reader.Read()) {
									return;
								}

								Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
								int epid = reader.GetInt32(reader.GetOrdinal("epid"));

								if (pluginsInst.PluginExists(pluginId)) {
									curDldProgData = new DldProgData();

									ProgrammeInfo progInfo = default(ProgrammeInfo);
									if (reader.IsDBNull(reader.GetOrdinal("progname"))) {
										progInfo.Name = null;
									} else {
										progInfo.Name = reader.GetString(reader.GetOrdinal("progname"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("progdesc"))) {
										progInfo.Description = null;
									} else {
										progInfo.Description = reader.GetString(reader.GetOrdinal("progdesc"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("progimg"))) {
										progInfo.Image = null;
									} else {
										progInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("progimg")));
									}

									EpisodeInfo epiEpInfo = default(EpisodeInfo);
									if (reader.IsDBNull(reader.GetOrdinal("epname"))) {
										epiEpInfo.Name = null;
									} else {
										epiEpInfo.Name = reader.GetString(reader.GetOrdinal("epname"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("epdesc"))) {
										epiEpInfo.Description = null;
									} else {
										epiEpInfo.Description = reader.GetString(reader.GetOrdinal("epdesc"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("duration"))) {
										epiEpInfo.DurationSecs = null;
									} else {
										epiEpInfo.DurationSecs = reader.GetInt32(reader.GetOrdinal("duration"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("date"))) {
										epiEpInfo.Date = null;
									} else {
										epiEpInfo.Date = reader.GetDateTime(reader.GetOrdinal("date"));
									}

									if (reader.IsDBNull(reader.GetOrdinal("epimg"))) {
										epiEpInfo.Image = null;
									} else {
										epiEpInfo.Image = RetrieveImage(reader.GetInt32(reader.GetOrdinal("epimg")));
									}

									epiEpInfo.ExtInfo = new Dictionary<string, string>();

									using (SQLiteCommand extCommand = new SQLiteCommand("select name, value from episodeext where epid=@epid", FetchDbConn())) {
										extCommand.Parameters.Add(new SQLiteParameter("@epid", reader.GetInt32(reader.GetOrdinal("epid"))));

										using (SQLiteMonDataReader extReader = new SQLiteMonDataReader(extCommand.ExecuteReader())) {
											while (extReader.Read()) {
												epiEpInfo.ExtInfo.Add(extReader.GetString(extReader.GetOrdinal("name")), extReader.GetString(extReader.GetOrdinal("value")));
											}
										}
									}

									var _with1 = curDldProgData;
									_with1.PluginId = pluginId;
									_with1.ProgExtId = reader.GetString(reader.GetOrdinal("progextid"));
									_with1.EpId = epid;
									_with1.EpisodeExtId = reader.GetString(reader.GetOrdinal("epextid"));
									_with1.ProgInfo = progInfo;
									_with1.EpisodeInfo = epiEpInfo;

									if (reader.GetInt32(reader.GetOrdinal("status")) == DownloadStatus.Errored) {
										ResetDownloadAsync(epid, true);
									}

									downloadThread = new Thread(DownloadProgThread);
									downloadThread.IsBackground = true;
									downloadThread.Start();

									return;
								}
							}
						}
					}
				}
			}
		}

		public void DownloadProgThread()
		{
			DownloadPluginInst = pluginsInst.GetPluginInstance(curDldProgData.PluginId);

			try {
				// Make sure that the temp folder still exists
				Directory.CreateDirectory(Path.Combine(System.IO.Path.GetTempPath(), "RadioDownloader"));

				var _with2 = curDldProgData;
				try {
					curDldProgData.FinalName = FileUtils.FindFreeSaveFileName(RadioDld.My.Settings.FileNameFormat, curDldProgData.ProgInfo.Name, curDldProgData.EpisodeInfo.Name, curDldProgData.EpisodeInfo.Date, FileUtils.GetSaveFolder());
				} catch (DirectoryNotFoundException dirNotFoundExp) {
					DownloadError(ErrorType.LocalProblem, "Your chosen location for saving downloaded programmes no longer exists.  Select a new one under Options -> Main Options.", null);
					return;
				} catch (IOException ioExp) {
					DownloadError(ErrorType.LocalProblem, "Encountered an error generating the download file name.  The error message was '" + ioExp.Message + "'.  You may need to select a new location for saving downloaded programmes under Options -> Main Options.", null);
					return;
				}

				DownloadPluginInst.DownloadProgramme(_with2.ProgExtId, _with2.EpisodeExtId, _with2.ProgInfo, _with2.EpisodeInfo, _with2.FinalName);
			} catch (ThreadAbortException threadAbortExp) {
			// The download has been aborted, so ignore the exception
			} catch (DownloadException downloadExp) {
				DownloadError(downloadExp.TypeOfError, downloadExp.Message, downloadExp.ErrorExtraDetails);
			} catch (Exception unknownExp) {
				List<DldErrorDataItem> extraDetails = new List<DldErrorDataItem>();
				extraDetails.Add(new DldErrorDataItem("error", unknownExp.GetType().ToString() + ": " + unknownExp.Message));
				extraDetails.Add(new DldErrorDataItem("exceptiontostring", unknownExp.ToString()));

				if (unknownExp.Data != null) {
					foreach (DictionaryEntry dataEntry in unknownExp.Data) {
						if (object.ReferenceEquals(dataEntry.Key.GetType(), typeof(string)) & object.ReferenceEquals(dataEntry.Value.GetType(), typeof(string))) {
							extraDetails.Add(new DldErrorDataItem("expdata:Data:" + Convert.ToString(dataEntry.Key), Convert.ToString(dataEntry.Value)));
						}
					}
				}

				DownloadError(ErrorType.UnknownError, unknownExp.GetType().ToString() + Environment.NewLine + unknownExp.StackTrace, extraDetails);
			}
		}

		public void UpdateProgInfoIfRequired(int progid)
		{
			ThreadPool.QueueUserWorkItem(() => UpdateProgInfoIfRequiredAsync(progid));
		}

		private void UpdateProgInfoIfRequiredAsync(int progid)
		{
			Guid providerId = null;
			string updateExtid = null;

			// Test to see if an update is required, and then free up the database
			using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid, lastupdate from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));

						if (pluginsInst.PluginExists(providerId)) {
							IRadioProvider pluginInstance = null;
							pluginInstance = pluginsInst.GetPluginInstance(providerId);

							if (reader.GetDateTime(reader.GetOrdinal("lastupdate")).AddDays(pluginInstance.ProgInfoUpdateFreqDays) < DateAndTime.Now) {
								updateExtid = reader.GetString(reader.GetOrdinal("extid"));
							}
						}
					}
				}
			}

			// Now perform the update if required
			if (updateExtid != null) {
				StoreProgrammeInfo(providerId, updateExtid);
			}
		}

		public Bitmap FetchProgrammeImage(int progid)
		{
			Bitmap functionReturnValue = null;
			using (SQLiteCommand command = new SQLiteCommand("select image from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						int imgid = reader.GetInt32(reader.GetOrdinal("image"));

						if (imgid == null) {
							// Find the id of the latest episode's image
							using (SQLiteCommand latestCmd = new SQLiteCommand("select image from episodes where progid=@progid and image notnull order by date desc limit 1", FetchDbConn())) {
								latestCmd.Parameters.Add(new SQLiteParameter("@progid", progid));

								using (SQLiteMonDataReader latestRdr = new SQLiteMonDataReader(latestCmd.ExecuteReader())) {
									if (latestRdr.Read()) {
										imgid = latestRdr.GetInt32(reader.GetOrdinal("image"));
									}
								}
							}
						}

						if (imgid != null) {
							functionReturnValue = RetrieveImage(imgid);
						} else {
							functionReturnValue = null;
						}
					} else {
						functionReturnValue = null;
					}
				}
			}
			return functionReturnValue;
		}

		public Bitmap FetchEpisodeImage(int epid)
		{
			Bitmap functionReturnValue = null;
			using (SQLiteCommand command = new SQLiteCommand("select image, progid from episodes where epid=@epid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", epid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						int imgid = reader.GetInt32(reader.GetOrdinal("image"));

						if (imgid != null) {
							functionReturnValue = RetrieveImage(imgid);
						} else {
							functionReturnValue = null;
						}

						if (functionReturnValue == null) {
							if (reader.IsDBNull(reader.GetOrdinal("progid")) == false) {
								functionReturnValue = FetchProgrammeImage(reader.GetInt32(reader.GetOrdinal("progid")));
							}
						}
					} else {
						functionReturnValue = null;
					}
				}
			}
			return functionReturnValue;
		}

		public void EpisodeSetAutoDownload(int epid, bool autoDownload)
		{
			ThreadPool.QueueUserWorkItem(() => EpisodeSetAutoDownloadAsync(epid, autoDownload));
		}

		private void EpisodeSetAutoDownloadAsync(int epid, bool autoDownload)
		{
			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("update episodes set autodownload=@autodownload where epid=@epid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@epid", epid));
					command.Parameters.Add(new SQLiteParameter("@autodownload", autoDownload ? 1 : 0));
					command.ExecuteNonQuery();
				}
			}
		}

		public int CountDownloadsNew()
		{
			using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where playcount=0 and status=@status", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		public int CountDownloadsErrored()
		{
			using (SQLiteCommand command = new SQLiteCommand("select count(epid) from downloads where status=@status", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
				return Convert.ToInt32(command.ExecuteScalar());
			}
		}

		private void CheckSubscriptions()
		{
			// Wait for 10 minutes to give a pause between each check for new episodes
			Thread.Sleep(600000);

			List<int> progids = new List<int>();

			// Fetch the current subscriptions into a list, so that the reader doesn't remain open while
			// checking all of the subscriptions, as this blocks writes to the database from other threads
			using (SQLiteCommand command = new SQLiteCommand("select progid from subscriptions", FetchDbConn())) {
				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					int progidOrdinal = reader.GetOrdinal("progid");

					while (reader.Read()) {
						progids.Add(reader.GetInt32(progidOrdinal));
					}
				}
			}

			// Work through the list of subscriptions and check for new episodes
			using (SQLiteCommand progInfCmd = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn)) {
				using (SQLiteCommand checkCmd = new SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn)) {
					using (SQLiteCommand findCmd = new SQLiteCommand("select epid, autodownload from episodes where progid=@progid and extid=@extid", FetchDbConn())) {

						SQLiteParameter epidParam = new SQLiteParameter("@epid");
						SQLiteParameter progidParam = new SQLiteParameter("@progid");
						SQLiteParameter extidParam = new SQLiteParameter("@extid");

						progInfCmd.Parameters.Add(progidParam);
						findCmd.Parameters.Add(progidParam);
						findCmd.Parameters.Add(extidParam);
						checkCmd.Parameters.Add(epidParam);

						foreach (int progid in progids) {
							Guid providerId = default(Guid);
							string progExtId = null;

							progidParam.Value = progid;

							using (SQLiteMonDataReader progInfReader = new SQLiteMonDataReader(progInfCmd.ExecuteReader())) {
								if (progInfReader.Read() == false) {
									continue;
								}

								providerId = new Guid(progInfReader.GetString(progInfReader.GetOrdinal("pluginid")));
								progExtId = progInfReader.GetString(progInfReader.GetOrdinal("extid"));
							}

							List<string> episodeExtIds = null;

							try {
								episodeExtIds = GetAvailableEpisodes(providerId, progExtId);
							} catch (Exception unhandled) {
								// Catch any unhandled provider exceptions
								continue;
							}

							if (episodeExtIds != null) {
								foreach (string episodeExtId in episodeExtIds) {
									extidParam.Value = episodeExtId;

									bool needEpInfo = true;
									int epid = 0;

									using (SQLiteMonDataReader findReader = new SQLiteMonDataReader(findCmd.ExecuteReader())) {
										if (findReader.Read()) {
											needEpInfo = false;
											epid = findReader.GetInt32(findReader.GetOrdinal("epid"));

											if (findReader.GetInt32(findReader.GetOrdinal("autodownload")) != 1) {
												// Don't download the episode automatically, skip to the next one
												continue;
											}
										}
									}

									if (needEpInfo) {
										try {
											epid = StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId);
										} catch {
											// Catch any unhandled provider exceptions
											continue;
										}

										if (epid < 0) {
											continue;
										}
									}

									epidParam.Value = epid;

									using (SQLiteMonDataReader checkRdr = new SQLiteMonDataReader(checkCmd.ExecuteReader())) {
										if (checkRdr.Read() == false) {
											AddDownloadAsync(epid);
										}
									}
								}
							}
						}
					}
				}
			}

			// Queue the next subscription check.  This is used instead of a loop
			// as it frees up a slot in the thread pool other actions are waiting.
			ThreadPool.QueueUserWorkItem(() => CheckSubscriptions());
		}

		public bool AddDownload(int epid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", epid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						return false;
					}
				}
			}

			ThreadPool.QueueUserWorkItem(() => AddDownloadAsync(epid));

			return true;
		}

		private void AddDownloadAsync(int epid)
		{
			lock (dbUpdateLock) {
				// Check again that the download doesn't exist, as it may have been
				// added while this call was waiting in the thread pool
				using (SQLiteCommand command = new SQLiteCommand("select epid from downloads where epid=@epid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@epid", epid));

					using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
						if (reader.Read()) {
							return;
						}
					}
				}

				using (SQLiteCommand command = new SQLiteCommand("insert into downloads (epid) values (@epid)", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@epid", epid));
					command.ExecuteNonQuery();
				}
			}

			search.AddDownload(epid);

			if (search.DownloadIsVisible(epid)) {
				if (DownloadAdded != null) {
					DownloadAdded(epid);
				}
			}

			StartDownload();
		}

		public bool AddFavourite(int progid)
		{
			if (IsFavourite(progid)) {
				return false;
			}

			ThreadPool.QueueUserWorkItem(() => AddFavouriteAsync(progid));

			return true;
		}

		private void AddFavouriteAsync(int progid)
		{
			lock (dbUpdateLock) {
				// Check again that the favourite doesn't exist, as it may have been
				// added while this call was waiting in the thread pool
				if (IsFavourite(progid)) {
					return;
				}

				using (SQLiteCommand command = new SQLiteCommand("insert into favourites (progid) values (@progid)", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@progid", progid));
					command.ExecuteNonQuery();
				}
			}

			if (ProgrammeUpdated != null) {
				ProgrammeUpdated(progid);
			}
			if (FavouriteAdded != null) {
				FavouriteAdded(progid);
			}

			if (IsSubscribed(progid)) {
				if (SubscriptionUpdated != null) {
					SubscriptionUpdated(progid);
				}
			}
		}

		public void RemoveFavourite(int progid)
		{
			ThreadPool.QueueUserWorkItem(() => RemoveFavouriteAsync(progid));
		}

		private void RemoveFavouriteAsync(int progid)
		{
			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("delete from favourites where progid=@progid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@progid", progid));
					command.ExecuteNonQuery();
				}
			}

			if (ProgrammeUpdated != null) {
				ProgrammeUpdated(progid);
			}
			if (FavouriteRemoved != null) {
				FavouriteRemoved(progid);
			}

			if (IsSubscribed(progid)) {
				if (SubscriptionUpdated != null) {
					SubscriptionUpdated(progid);
				}
			}
		}

		public bool AddSubscription(int progid)
		{
			if (IsSubscribed(progid)) {
				return false;
			}

			ThreadPool.QueueUserWorkItem(() => AddSubscriptionAsync(progid));

			return true;
		}

		private void AddSubscriptionAsync(int progid)
		{
			lock (dbUpdateLock) {
				// Check again that the subscription doesn't exist, as it may have been
				// added while this call was waiting in the thread pool
				if (IsSubscribed(progid)) {
					return;
				}

				using (SQLiteCommand command = new SQLiteCommand("insert into subscriptions (progid) values (@progid)", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@progid", progid));
					command.ExecuteNonQuery();
				}
			}

			if (ProgrammeUpdated != null) {
				ProgrammeUpdated(progid);
			}
			if (SubscriptionAdded != null) {
				SubscriptionAdded(progid);
			}

			if (IsFavourite(progid)) {
				if (FavouriteUpdated != null) {
					FavouriteUpdated(progid);
				}
			}
		}

		public void RemoveSubscription(int progid)
		{
			ThreadPool.QueueUserWorkItem(() => RemoveSubscriptionAsync(progid));
		}

		private void RemoveSubscriptionAsync(int progid)
		{
			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("delete from subscriptions where progid=@progid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@progid", progid));
					command.ExecuteNonQuery();
				}
			}

			if (ProgrammeUpdated != null) {
				ProgrammeUpdated(progid);
			}
			if (SubscriptionRemoved != null) {
				SubscriptionRemoved(progid);
			}

			if (IsFavourite(progid)) {
				if (FavouriteUpdated != null) {
					FavouriteUpdated(progid);
				}
			}
		}

		private System.DateTime LatestDownloadDate(int progid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select date from episodes, downloads where episodes.epid=downloads.epid and progid=@progid order by date desc limit 1", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						// No downloads of this program
						return null;
					} else {
						return reader.GetDateTime(reader.GetOrdinal("date"));
					}
				}
			}
		}

		public void ResetDownload(int epid)
		{
			ThreadPool.QueueUserWorkItem(() => ResetDownloadAsync(epid, false));
		}

		private void ResetDownloadAsync(int epid, bool auto)
		{
			lock (dbUpdateLock) {
				using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction())) {
					using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortype=null, errortime=null, errordetails=null where epid=@epid", FetchDbConn(), transMon.Trans)) {
						command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Waiting));
						command.Parameters.Add(new SQLiteParameter("@epid", epid));
						command.ExecuteNonQuery();
					}

					if (auto == false) {
						using (SQLiteCommand command = new SQLiteCommand("update downloads set errorcount=0 where epid=@epid", FetchDbConn(), transMon.Trans)) {
							command.Parameters.Add(new SQLiteParameter("@epid", epid));
							command.ExecuteNonQuery();
						}
					}

					transMon.Trans.Commit();
				}
			}

			lock (downloadSortCacheLock) {
				downloadSortCache = null;
			}

			search.UpdateDownload(epid);

			if (search.DownloadIsVisible(epid)) {
				if (DownloadUpdated != null) {
					DownloadUpdated(epid);
				}
			}

			if (auto == false) {
				StartDownloadAsync();
			}
		}

		public void DownloadRemove(int epid)
		{
			ThreadPool.QueueUserWorkItem(() => DownloadRemoveAsync(epid, false));
		}

		private void DownloadRemoveAsync(int epid, bool auto)
		{
			lock (dbUpdateLock) {
				using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction())) {
					using (SQLiteCommand command = new SQLiteCommand("delete from downloads where epid=@epid", FetchDbConn(), transMon.Trans)) {
						command.Parameters.Add(new SQLiteParameter("@epid", epid));
						command.ExecuteNonQuery();
					}

					if (auto == false) {
						// Unet the auto download flag, so if the user is subscribed it doesn't just download again
						EpisodeSetAutoDownloadAsync(epid, false);
					}

					transMon.Trans.Commit();
				}
			}

			lock (downloadSortCacheLock) {
				// No need to clear the sort cache, just remove this episodes entry
				if (downloadSortCache != null) {
					downloadSortCache.Remove(epid);
				}
			}

			if (search.DownloadIsVisible(epid)) {
				if (DownloadRemoved != null) {
					DownloadRemoved(epid);
				}
			}

			if (DownloadProgressTotal != null) {
				DownloadProgressTotal(false, 0);
			}
			search.RemoveDownload(epid);

			if (curDldProgData != null) {
				if (curDldProgData.EpId == epid) {
					// This episode is currently being downloaded

					if (downloadThread != null) {
						if (auto == false) {
							// This is called by the download thread if it is an automatic removal
							downloadThread.Abort();
						}

						downloadThread = null;
					}
				}

				StartDownload();
			}
		}

		public void DownloadBumpPlayCount(int epid)
		{
			ThreadPool.QueueUserWorkItem(() => DownloadBumpPlayCountAsync(epid));
		}

		private void DownloadBumpPlayCountAsync(int epid)
		{
			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("update downloads set playcount=playcount+1 where epid=@epid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@epid", epid));
					command.ExecuteNonQuery();
				}
			}

			lock (downloadSortCacheLock) {
				downloadSortCache = null;
			}

			search.UpdateDownload(epid);

			if (search.DownloadIsVisible(epid)) {
				if (DownloadUpdated != null) {
					DownloadUpdated(epid);
				}
			}
		}

		public void DownloadReportError(int epid)
		{
			ErrorType errorType = default(ErrorType);
			string errorText = null;
			string extraDetailsString = null;
			Dictionary<string, string> errorExtraDetails = new Dictionary<string, string>();

			XmlSerializer detailsSerializer = new XmlSerializer(typeof(List<DldErrorDataItem>));

			using (SQLiteCommand command = new SQLiteCommand("select errortype, errordetails, ep.name as epname, ep.description as epdesc, date, duration, ep.extid as epextid, pr.name as progname, pr.description as progdesc, pr.extid as progextid, pluginid from downloads as dld, episodes as ep, programmes as pr where dld.epid=@epid and ep.epid=@epid and ep.progid=pr.progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", epid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (!reader.Read()) {
						throw new ArgumentException("Episode " + epid.ToString(CultureInfo.InvariantCulture) + " does not exit, or is not in the download list!", "epid");
					}

					errorType = (ErrorType)reader.GetInt32(reader.GetOrdinal("errortype"));
					extraDetailsString = reader.GetString(reader.GetOrdinal("errordetails"));

					errorExtraDetails.Add("episode:name", reader.GetString(reader.GetOrdinal("epname")));
					errorExtraDetails.Add("episode:description", reader.GetString(reader.GetOrdinal("epdesc")));
					errorExtraDetails.Add("episode:date", reader.GetDateTime(reader.GetOrdinal("date")).ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture));
					errorExtraDetails.Add("episode:duration", Convert.ToString(reader.GetInt32(reader.GetOrdinal("duration"))));
					errorExtraDetails.Add("episode:extid", reader.GetString(reader.GetOrdinal("epextid")));

					errorExtraDetails.Add("programme:name", reader.GetString(reader.GetOrdinal("progname")));
					errorExtraDetails.Add("programme:description", reader.GetString(reader.GetOrdinal("progdesc")));
					errorExtraDetails.Add("programme:extid", reader.GetString(reader.GetOrdinal("progextid")));

					Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
					IRadioProvider providerInst = pluginsInst.GetPluginInstance(pluginId);

					errorExtraDetails.Add("provider:id", pluginId.ToString());
					errorExtraDetails.Add("provider:name", providerInst.ProviderName);
					errorExtraDetails.Add("provider:description", providerInst.ProviderDescription);
				}
			}

			if (extraDetailsString != null) {
				try {
					List<DldErrorDataItem> extraDetails = null;
					extraDetails = (List<DldErrorDataItem>)detailsSerializer.Deserialize(new StringReader(extraDetailsString));

					foreach (DldErrorDataItem detailItem in extraDetails) {
						switch (detailItem.Name) {
							case "error":
								errorText = detailItem.Data;
								break;
							case "details":
								extraDetailsString = detailItem.Data;
								break;
							default:
								errorExtraDetails.Add(detailItem.Name, detailItem.Data);
								break;
						}
					}
				} catch (InvalidOperationException invalidOperationExp) {
				// Do nothing, and fall back to reporting all the details as one string
				} catch (InvalidCastException invalidCastExp) {
					// Do nothing, and fall back to reporting all the details as one string
				}
			}

			if (errorText == null || errorText == string.Empty) {
				errorText = errorType.ToString();
			}

			ErrorReporting report = new ErrorReporting("Download Error: " + errorText, extraDetailsString, errorExtraDetails);
			report.SendReport(RadioDld.My.Settings.ErrorReportURL);
		}

		private void DownloadError(ErrorType errorType, string errorDetails, List<DldErrorDataItem> furtherDetails)
		{
			switch (errorType) {
				case errorType.RemoveFromList:
					DownloadRemoveAsync(curDldProgData.EpId, true);
					return;
				case errorType.UnknownError:
					if (furtherDetails == null) {
						furtherDetails = new List<DldErrorDataItem>();
					}

					if (errorDetails != null) {
						furtherDetails.Add(new DldErrorDataItem("details", errorDetails));
					}

					StringWriter detailsStringWriter = new StringWriter(CultureInfo.InvariantCulture);
					XmlSerializer detailsSerializer = new XmlSerializer(typeof(List<DldErrorDataItem>));
					detailsSerializer.Serialize(detailsStringWriter, furtherDetails);
					errorDetails = detailsStringWriter.ToString();
					break;
			}

			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, errortime=datetime('now'), errortype=@errortype, errordetails=@errordetails, errorcount=errorcount+1, totalerrors=totalerrors+1 where epid=@epid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Errored));
					command.Parameters.Add(new SQLiteParameter("@errortype", errorType));
					command.Parameters.Add(new SQLiteParameter("@errordetails", errorDetails));
					command.Parameters.Add(new SQLiteParameter("@epid", curDldProgData.EpId));
					command.ExecuteNonQuery();
				}
			}

			lock (downloadSortCacheLock) {
				downloadSortCache = null;
			}

			search.UpdateDownload(curDldProgData.EpId);

			if (search.DownloadIsVisible(curDldProgData.EpId)) {
				if (DownloadUpdated != null) {
					DownloadUpdated(curDldProgData.EpId);
				}
			}

			if (DownloadProgressTotal != null) {
				DownloadProgressTotal(false, 0);
			}

			downloadThread = null;
			curDldProgData = null;

			StartDownloadAsync();
		}

		private void DownloadPluginInst_Finished(string fileExtension)
		{
			curDldProgData.FinalName += "." + fileExtension;

			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("update downloads set status=@status, filepath=@filepath where epid=@epid", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));
					command.Parameters.Add(new SQLiteParameter("@filepath", curDldProgData.FinalName));
					command.Parameters.Add(new SQLiteParameter("@epid", curDldProgData.EpId));
					command.ExecuteNonQuery();
				}
			}

			lock (downloadSortCacheLock) {
				downloadSortCache = null;
			}

			search.UpdateDownload(curDldProgData.EpId);

			if (search.DownloadIsVisible(curDldProgData.EpId)) {
				if (DownloadUpdated != null) {
					DownloadUpdated(curDldProgData.EpId);
				}
			}

			if (DownloadProgressTotal != null) {
				DownloadProgressTotal(false, 100);
			}

			// If the episode's programme is a subscription, clear the sort cache and raise an updated event
			using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from episodes, subscriptions where epid=@epid and subscriptions.progid = episodes.progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", curDldProgData.EpId));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						lock (subscriptionSortCacheLock) {
							subscriptionSortCache = null;
						}

						if (SubscriptionUpdated != null) {
							SubscriptionUpdated(reader.GetInt32(reader.GetOrdinal("progid")));
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(RadioDld.My.Settings.RunAfterCommand)) {
				try {
					// Environ("comspec") will give the path to cmd.exe or command.com
					Interaction.Shell("\"" + Interaction.Environ("comspec") + "\" /c " + RadioDld.My.Settings.RunAfterCommand.Replace("%file%", curDldProgData.FinalName), AppWinStyle.NormalNoFocus);
				} catch {
					// Just ignore the error, as it just means that something has gone wrong with the run after command.
				}
			}

			downloadThread = null;
			curDldProgData = null;

			StartDownloadAsync();
		}
		readonly Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag static_DownloadPluginInst_Progress_lastNum_Init = new Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag();
		int static_DownloadPluginInst_Progress_lastNum;

		private void DownloadPluginInst_Progress(int percent, string statusText, ProgressIcon icon)
		{
			lock (static_DownloadPluginInst_Progress_lastNum_Init) {
				try {
					if (InitStaticVariableHelper(static_DownloadPluginInst_Progress_lastNum_Init)) {
						static_DownloadPluginInst_Progress_lastNum = -1;
					}
				} finally {
					static_DownloadPluginInst_Progress_lastNum_Init.State = 1;
				}
			}

			// Don't raise the progress event if the value is the same as last time, or is outside the range
			if (percent == static_DownloadPluginInst_Progress_lastNum || percent < 0 || percent > 100) {
				return;
			}

			static_DownloadPluginInst_Progress_lastNum = percent;

			if (search.DownloadIsVisible(curDldProgData.EpId)) {
				if (DownloadProgress != null) {
					DownloadProgress(curDldProgData.EpId, percent, statusText, icon);
				}
			}

			if (DownloadProgressTotal != null) {
				DownloadProgressTotal(true, percent);
			}
		}

		public void PerformCleanup()
		{
			using (SQLiteCommand command = new SQLiteCommand("select epid, filepath from downloads where status=@status", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@status", DownloadStatus.Downloaded));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					int epidOrd = reader.GetOrdinal("epid");
					int filepathOrd = reader.GetOrdinal("filepath");

					while (reader.Read()) {
						// Remove programmes for which the associated audio file no longer exists
						if (File.Exists(reader.GetString(filepathOrd)) == false) {
							// Take the download out of the list and set the auto download flag to false
							DownloadRemoveAsync(reader.GetInt32(epidOrd), false);
						}
					}
				}
			}
		}

		private void SetDBSetting(string propertyName, object value)
		{
			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("insert or replace into settings (property, value) values (@property, @value)", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@property", propertyName));
					command.Parameters.Add(new SQLiteParameter("@value", value));
					command.ExecuteNonQuery();
				}
			}
		}

		private object GetDBSetting(string propertyName)
		{
			using (SQLiteCommand command = new SQLiteCommand("select value from settings where property=@property", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@property", propertyName));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					return reader.GetValue(reader.GetOrdinal("value"));
				}
			}
		}

		private void VacuumDatabase()
		{
			// Vacuum the database every few months - vacuums are spaced like this as they take ages to run
			bool runVacuum = false;
			object lastVacuum = GetDBSetting("lastvacuum");

			if (lastVacuum == null) {
				runVacuum = true;
			} else {
				runVacuum = DateTime.ParseExact(Convert.ToString(lastVacuum), "O", CultureInfo.InvariantCulture).AddMonths(3) < DateAndTime.Now;
			}

			if (runVacuum) {
				My.MyProject.Forms.Status.StatusText = "Compacting Database..." + Environment.NewLine + Environment.NewLine + "This may take some time if you have downloaded a lot of programmes.";
				My.MyProject.Forms.Status.ProgressBarMarquee = true;
				My.MyProject.Forms.Status.Show();
				Application.DoEvents();

				// Make SQLite recreate the database to reduce the size on disk and remove fragmentation
				lock (dbUpdateLock) {
					using (SQLiteCommand command = new SQLiteCommand("vacuum", FetchDbConn())) {
						command.ExecuteNonQuery();
					}
				}

				SetDBSetting("lastvacuum", DateAndTime.Now.ToString("O", CultureInfo.InvariantCulture));

				My.MyProject.Forms.Status.Hide();
				Application.DoEvents();
			}
		}

		public Panel GetFindNewPanel(Guid pluginID, object view)
		{
			if (pluginsInst.PluginExists(pluginID)) {
				FindNewPluginInst = pluginsInst.GetPluginInstance(pluginID);
				return FindNewPluginInst.GetFindNewPanel(view);
			} else {
				return new Panel();
			}
		}

		private void FindNewPluginInst_FindNewException(Exception exception, bool unhandled)
		{
			ErrorReporting reportException = new ErrorReporting("Find New Error", exception);

			if (unhandled) {
				if (My.MyProject.Forms.ReportError.Visible == false) {
					My.MyProject.Forms.ReportError.AssignReport(reportException);
					My.MyProject.Forms.ReportError.ShowDialog();
				}
			} else {
				reportException.SendReport(RadioDld.My.Settings.ErrorReportURL);
			}
		}

		private void FindNewPluginInst_FindNewViewChange(object view)
		{
			if (FindNewViewChange != null) {
				FindNewViewChange(view);
			}
		}

		private void FindNewPluginInst_FoundNew(string progExtId)
		{
			Guid pluginId = FindNewPluginInst.ProviderId;

			if (StoreProgrammeInfo(pluginId, progExtId) == false) {
				Interaction.MsgBox("There was a problem retrieving information about this programme.  You might like to try again later.", MsgBoxStyle.Exclamation);
				return;
			}

			int progid = ExtIDToProgID(pluginId, progExtId);
			if (FoundNew != null) {
				FoundNew(progid);
			}
		}

		private bool StoreProgrammeInfo(System.Guid pluginId, string progExtId)
		{
			if (pluginsInst.PluginExists(pluginId) == false) {
				return false;
			}

			IRadioProvider pluginInstance = pluginsInst.GetPluginInstance(pluginId);
			GetProgrammeInfoReturn progInfo = default(GetProgrammeInfoReturn);

			progInfo = pluginInstance.GetProgrammeInfo(progExtId);

			if (progInfo.Success == false) {
				return false;
			}

			int progid = 0;

			lock (dbUpdateLock) {
				progid = ExtIDToProgID(pluginId, progExtId);

				using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction())) {
					if (progid == null) {
						using (SQLiteCommand command = new SQLiteCommand("insert into programmes (pluginid, extid) values (@pluginid, @extid)", FetchDbConn())) {
							command.Parameters.Add(new SQLiteParameter("@pluginid", pluginId.ToString()));
							command.Parameters.Add(new SQLiteParameter("@extid", progExtId));
							command.ExecuteNonQuery();
						}

						using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", FetchDbConn())) {
							progid = Convert.ToInt32(command.ExecuteScalar());
						}
					}

					using (SQLiteCommand command = new SQLiteCommand("update programmes set name=@name, description=@description, image=@image, singleepisode=@singleepisode, lastupdate=@lastupdate where progid=@progid", FetchDbConn())) {
						command.Parameters.Add(new SQLiteParameter("@name", progInfo.ProgrammeInfo.Name));
						command.Parameters.Add(new SQLiteParameter("@description", progInfo.ProgrammeInfo.Description));
						command.Parameters.Add(new SQLiteParameter("@image", StoreImage(progInfo.ProgrammeInfo.Image)));
						command.Parameters.Add(new SQLiteParameter("@singleepisode", progInfo.ProgrammeInfo.SingleEpisode));
						command.Parameters.Add(new SQLiteParameter("@lastupdate", DateAndTime.Now));
						command.Parameters.Add(new SQLiteParameter("@progid", progid));
						command.ExecuteNonQuery();
					}

					transMon.Trans.Commit();
				}
			}

			// If the programme is in the list of favourites, clear the sort cache and raise an updated event
			if (IsFavourite(progid)) {
				lock (favouriteSortCacheLock) {
					favouriteSortCache = null;
				}

				if (FavouriteUpdated != null) {
					FavouriteUpdated(progid);
				}
			}

			// If the programme is in the list of subscriptions, clear the sort cache and raise an updated event
			if (IsSubscribed(progid)) {
				lock (subscriptionSortCacheLock) {
					subscriptionSortCache = null;
				}

				if (SubscriptionUpdated != null) {
					SubscriptionUpdated(progid);
				}
			}

			return true;
		}

		private int StoreImage(Bitmap image)
		{
			if (image == null) {
				return null;
			}

			// Convert the image into a byte array
			MemoryStream memstream = new MemoryStream();
			image.Save(memstream, System.Drawing.Imaging.ImageFormat.Png);
			byte[] imageAsBytes = new byte[Convert.ToInt32(memstream.Length - 1) + 1];
			memstream.Position = 0;
			memstream.Read(imageAsBytes, 0, Convert.ToInt32(memstream.Length));

			lock (dbUpdateLock) {
				using (SQLiteCommand command = new SQLiteCommand("select imgid from images where image=@image", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));

					using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
						if (reader.Read()) {
							return reader.GetInt32(reader.GetOrdinal("imgid"));
						}
					}
				}

				using (SQLiteCommand command = new SQLiteCommand("insert into images (image) values (@image)", FetchDbConn())) {
					command.Parameters.Add(new SQLiteParameter("@image", imageAsBytes));
					command.ExecuteNonQuery();
				}

				using (SQLiteCommand command = new SQLiteCommand("select last_insert_rowid()", FetchDbConn())) {
					return Convert.ToInt32(command.ExecuteScalar());
				}
			}
		}

		private Bitmap RetrieveImage(int imgid)
		{
			Bitmap functionReturnValue = null;
			using (SQLiteCommand command = new SQLiteCommand("select image from images where imgid=@imgid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@imgid", imgid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (!reader.Read()) {
						return null;
					}

					// Get the size of the image data by passing nothing to getbytes
					int dataLength = Convert.ToInt32(reader.GetBytes(reader.GetOrdinal("image"), 0, null, 0, 0));
					byte[] content = new byte[dataLength];

					reader.GetBytes(reader.GetOrdinal("image"), 0, content, 0, dataLength);
					functionReturnValue = new Bitmap(new MemoryStream(content));
				}
			}
			return functionReturnValue;
		}

		private int ExtIDToProgID(System.Guid pluginID, string progExtId)
		{
			using (SQLiteCommand command = new SQLiteCommand("select progid from programmes where pluginid=@pluginid and extid=@extid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@pluginid", pluginID.ToString()));
				command.Parameters.Add(new SQLiteParameter("@extid", progExtId));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read()) {
						return reader.GetInt32(reader.GetOrdinal("progid"));
					} else {
						return null;
					}
				}
			}
		}

		private List<string> GetAvailableEpisodes(Guid providerId, string progExtId)
		{
			if (pluginsInst.PluginExists(providerId) == false) {
				return null;
			}

			string[] extIds = null;
			IRadioProvider providerInst = pluginsInst.GetPluginInstance(providerId);

			extIds = providerInst.GetAvailableEpisodeIds(progExtId);

			if (extIds == null) {
				return null;
			}

			// Remove any duplicates from the list of episodes
			List<string> extIdsUnique = new List<string>();

			foreach (string removeDups in extIds) {
				if (extIdsUnique.Contains(removeDups) == false) {
					extIdsUnique.Add(removeDups);
				}
			}

			return extIdsUnique;
		}

		private int StoreEpisodeInfo(Guid pluginId, int progid, string progExtId, string episodeExtId)
		{
			IRadioProvider providerInst = pluginsInst.GetPluginInstance(pluginId);
			GetEpisodeInfoReturn episodeInfoReturn = default(GetEpisodeInfoReturn);

			episodeInfoReturn = providerInst.GetEpisodeInfo(progExtId, episodeExtId);

			if (episodeInfoReturn.Success == false) {
				return -1;
			}

			if (episodeInfoReturn.EpisodeInfo.Name == null || episodeInfoReturn.EpisodeInfo.Name == string.Empty) {
				throw new InvalidDataException("Episode name cannot be nothing or an empty string");
			}

			if (episodeInfoReturn.EpisodeInfo.Date == null) {
				throw new InvalidDataException("Episode date cannot be nothing or an empty string");
			}

			lock (dbUpdateLock) {
				using (SQLiteMonTransaction transMon = new SQLiteMonTransaction(FetchDbConn().BeginTransaction())) {
					int epid = 0;

					using (SQLiteCommand addEpisodeCmd = new SQLiteCommand("insert into episodes (progid, extid, name, description, duration, date, image) values (@progid, @extid, @name, @description, @duration, @date, @image)", FetchDbConn(), transMon.Trans)) {
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@progid", progid));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@extid", episodeExtId));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@name", episodeInfoReturn.EpisodeInfo.Name));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@description", episodeInfoReturn.EpisodeInfo.Description));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@duration", episodeInfoReturn.EpisodeInfo.DurationSecs));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@date", episodeInfoReturn.EpisodeInfo.Date));
						addEpisodeCmd.Parameters.Add(new SQLiteParameter("@image", StoreImage(episodeInfoReturn.EpisodeInfo.Image)));
						addEpisodeCmd.ExecuteNonQuery();
					}

					using (SQLiteCommand getRowIDCmd = new SQLiteCommand("select last_insert_rowid()", FetchDbConn(), transMon.Trans)) {
						epid = Convert.ToInt32(getRowIDCmd.ExecuteScalar());
					}

					if (episodeInfoReturn.EpisodeInfo.ExtInfo != null) {
						using (SQLiteCommand addExtInfoCmd = new SQLiteCommand("insert into episodeext (epid, name, value) values (@epid, @name, @value)", FetchDbConn(), transMon.Trans)) {
							foreach (KeyValuePair<string, string> extItem in episodeInfoReturn.EpisodeInfo.ExtInfo) {
								var _with3 = addExtInfoCmd;
								_with3.Parameters.Add(new SQLiteParameter("@epid", epid));
								_with3.Parameters.Add(new SQLiteParameter("@name", extItem.Key));
								_with3.Parameters.Add(new SQLiteParameter("@value", extItem.Value));
								_with3.ExecuteNonQuery();
							}
						}
					}

					transMon.Trans.Commit();
					return epid;
				}
			}
		}

		public int CompareDownloads(int epid1, int epid2)
		{
			lock (downloadSortCacheLock) {
				if (downloadSortCache == null || !downloadSortCache.ContainsKey(epid1) || !downloadSortCache.ContainsKey(epid2)) {
					// The sort cache is either empty or missing one of the values that are required, so recreate it
					downloadSortCache = new Dictionary<int, int>();

					int sort = 0;
					string orderBy = null;

					switch (downloadSortBy) {
						case DownloadCols.EpisodeName:
							orderBy = "name" + downloadSortAsc ? string.Empty : " desc";
							break;
						case DownloadCols.EpisodeDate:
							orderBy = "date" + downloadSortAsc ? string.Empty : " desc";
							break;
						case DownloadCols.Status:
							orderBy = "status = 0" + downloadSortAsc ? " desc" : string.Empty + ", status" + downloadSortAsc ? " desc" : string.Empty + ", playcount > 0" + downloadSortAsc ? string.Empty : " desc" + ", date" + downloadSortAsc ? " desc" : string.Empty;
							break;
						case DownloadCols.Duration:
							orderBy = "duration" + downloadSortAsc ? string.Empty : " desc";
							break;
						default:
							throw new InvalidDataException("Invalid column: " + downloadSortBy.ToString());
					}

					using (SQLiteCommand command = new SQLiteCommand("select downloads.epid from downloads, episodes where downloads.epid=episodes.epid order by " + orderBy, FetchDbConn())) {
						using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
							int epidOrdinal = reader.GetOrdinal("epid");

							while (reader.Read()) {
								downloadSortCache.Add(reader.GetInt32(epidOrdinal), sort);
								sort += 1;
							}
						}
					}
				}

				try {
					return downloadSortCache[epid1] - downloadSortCache[epid2];
				} catch (KeyNotFoundException keyNotFoundExp) {
					// One of the entries has been removed from the database, but not yet from the list
					return 0;
				}
			}
		}

		public int CompareSubscriptions(int progid1, int progid2)
		{
			lock (subscriptionSortCacheLock) {
				if (subscriptionSortCache == null || !subscriptionSortCache.ContainsKey(progid1) || !subscriptionSortCache.ContainsKey(progid2)) {
					// The sort cache is either empty or missing one of the values that are required, so recreate it
					subscriptionSortCache = new Dictionary<int, int>();

					int sort = 0;

					using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from subscriptions, programmes where programmes.progid=subscriptions.progid order by name", FetchDbConn())) {
						using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
							int progidOrdinal = reader.GetOrdinal("progid");

							while (reader.Read()) {
								subscriptionSortCache.Add(reader.GetInt32(progidOrdinal), sort);
								sort += 1;
							}
						}
					}
				}

				return subscriptionSortCache[progid1] - subscriptionSortCache[progid2];
			}
		}

		public int CompareFavourites(int progid1, int progid2)
		{
			lock (favouriteSortCacheLock) {
				if (favouriteSortCache == null || !favouriteSortCache.ContainsKey(progid1) || !favouriteSortCache.ContainsKey(progid2)) {
					// The sort cache is either empty or missing one of the values that are required, so recreate it
					favouriteSortCache = new Dictionary<int, int>();

					int sort = 0;

					using (SQLiteCommand command = new SQLiteCommand("select favourites.progid from favourites, programmes where programmes.progid=favourites.progid order by name", FetchDbConn())) {
						using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
							int progidOrdinal = reader.GetOrdinal("progid");

							while (reader.Read()) {
								favouriteSortCache.Add(reader.GetInt32(progidOrdinal), sort);
								sort += 1;
							}
						}
					}
				}

				return favouriteSortCache[progid1] - favouriteSortCache[progid2];
			}
		}

		public void InitProviderList()
		{
			Guid[] pluginIdList = null;
			pluginIdList = pluginsInst.GetPluginIdList();

			foreach (Guid pluginId in pluginIdList) {
				if (ProviderAdded != null) {
					ProviderAdded(pluginId);
				}
			}
		}

		public void InitEpisodeList(int progid)
		{
			lock (episodeListThreadLock) {
				episodeListThread = new Thread(() => InitEpisodeListThread(progid));
				episodeListThread.IsBackground = true;
				episodeListThread.Start();
			}
		}

		public void CancelEpisodeListing()
		{
			lock (episodeListThreadLock) {
				episodeListThread = null;
			}
		}

		private void InitEpisodeListThread(int progid)
		{
			Guid providerId = default(Guid);
			string progExtId = null;

			using (SQLiteCommand command = new SQLiteCommand("select pluginid, extid from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return;
					}

					providerId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
					progExtId = reader.GetString(reader.GetOrdinal("extid"));
				}
			}

			List<string> episodeExtIDs = GetAvailableEpisodes(providerId, progExtId);

			if (episodeExtIDs != null) {
				using (SQLiteCommand findCmd = new SQLiteCommand("select epid from episodes where progid=@progid and extid=@extid", FetchDbConn())) {
					SQLiteParameter progidParam = new SQLiteParameter("@progid");
					SQLiteParameter extidParam = new SQLiteParameter("@extid");

					findCmd.Parameters.Add(progidParam);
					findCmd.Parameters.Add(extidParam);

					foreach (string episodeExtId in episodeExtIDs) {
						progidParam.Value = progid;
						extidParam.Value = episodeExtId;

						bool needEpInfo = true;
						int epid = 0;

						using (SQLiteMonDataReader reader = new SQLiteMonDataReader(findCmd.ExecuteReader())) {
							if (reader.Read()) {
								needEpInfo = false;
								epid = reader.GetInt32(reader.GetOrdinal("epid"));
							}
						}

						if (needEpInfo) {
							epid = StoreEpisodeInfo(providerId, progid, progExtId, episodeExtId);

							if (epid < 0) {
								continue;
							}
						}

						lock (episodeListThreadLock) {
							if (!object.ReferenceEquals(Thread.CurrentThread, episodeListThread)) {
								return;
							}

							if (EpisodeAdded != null) {
								EpisodeAdded(epid);
							}
						}
					}
				}
			}
		}

		public void InitSubscriptionList()
		{
			using (SQLiteCommand command = new SQLiteCommand("select subscriptions.progid from subscriptions, programmes where subscriptions.progid = programmes.progid", FetchDbConn())) {
				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					int progidOrdinal = reader.GetOrdinal("progid");

					while (reader.Read()) {
						if (SubscriptionAdded != null) {
							SubscriptionAdded(reader.GetInt32(progidOrdinal));
						}
					}
				}
			}
		}

		public List<DownloadData> FetchDownloadList(bool filtered)
		{
			List<DownloadData> downloadList = new List<DownloadData>();

			using (SQLiteCommand command = new SQLiteCommand("select downloads.epid, name, description, date, duration, status, errortype, errordetails, filepath, playcount from downloads, episodes where downloads.epid=episodes.epid", FetchDbConn())) {
				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					int epidOrdinal = reader.GetOrdinal("epid");

					while (reader.Read()) {
						int epid = reader.GetInt32(epidOrdinal);

						if (!filtered || search.DownloadIsVisible(epid)) {
							downloadList.Add(ReadDownloadData(epid, ref reader));
						}
					}
				}
			}

			return downloadList;
		}

		public DownloadData FetchDownloadData(int epid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select name, description, date, duration, status, errortype, errordetails, filepath, playcount from downloads, episodes where downloads.epid=@epid and episodes.epid=@epid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", epid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					return ReadDownloadData(epid, ref reader);
				}
			}
		}

		private DownloadData ReadDownloadData(int epid, ref SQLiteMonDataReader reader)
		{
			int descriptionOrdinal = reader.GetOrdinal("description");
			int filepathOrdinal = reader.GetOrdinal("filepath");

			DownloadData info = new DownloadData();
			info.epid = epid;
			info.episodeDate = reader.GetDateTime(reader.GetOrdinal("date"));
			info.name = TextUtils.StripDateFromName(reader.GetString(reader.GetOrdinal("name")), info.episodeDate);

			if (!reader.IsDBNull(descriptionOrdinal)) {
				info.description = reader.GetString(descriptionOrdinal);
			}

			info.duration = reader.GetInt32(reader.GetOrdinal("duration"));
			info.status = (DownloadStatus)reader.GetInt32(reader.GetOrdinal("status"));

			if (info.status == DownloadStatus.Errored) {
				info.errorType = (ErrorType)reader.GetInt32(reader.GetOrdinal("errortype"));

				if (info.errorType != ErrorType.UnknownError) {
					info.errorDetails = reader.GetString(reader.GetOrdinal("errordetails"));
				}
			}

			if (!reader.IsDBNull(filepathOrdinal)) {
				info.downloadPath = reader.GetString(filepathOrdinal);
			}

			info.playCount = reader.GetInt32(reader.GetOrdinal("playcount"));

			return info;
		}

		public List<FavouriteData> FetchFavouriteList()
		{
			List<FavouriteData> favouriteList = new List<FavouriteData>();

			using (SQLiteCommand command = new SQLiteCommand("select favourites.progid, name, description, singleepisode, pluginid from favourites, programmes where favourites.progid = programmes.progid", FetchDbConn())) {
				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					int progidOrdinal = reader.GetOrdinal("progid");

					while (reader.Read()) {
						favouriteList.Add(ReadFavouriteData(reader.GetInt32(progidOrdinal), ref reader));
					}
				}
			}

			return favouriteList;
		}

		public FavouriteData FetchFavouriteData(int progid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select name, description, singleepisode, pluginid from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					return ReadFavouriteData(progid, ref reader);
				}
			}
		}

		private FavouriteData ReadFavouriteData(int progid, ref SQLiteMonDataReader reader)
		{
			int descriptionOrdinal = reader.GetOrdinal("description");

			FavouriteData info = new FavouriteData();
			info.progid = progid;
			info.name = reader.GetString(reader.GetOrdinal("name"));

			if (!reader.IsDBNull(descriptionOrdinal)) {
				info.description = reader.GetString(descriptionOrdinal);
			}

			info.singleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));

			Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
			IRadioProvider providerInst = pluginsInst.GetPluginInstance(pluginId);
			info.providerName = providerInst.ProviderName;

			info.subscribed = IsSubscribed(progid);

			return info;
		}

		public SubscriptionData FetchSubscriptionData(int progid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select name, description, pluginid from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					int descriptionOrdinal = reader.GetOrdinal("description");

					SubscriptionData info = new SubscriptionData();
					info.name = reader.GetString(reader.GetOrdinal("name"));

					if (!reader.IsDBNull(descriptionOrdinal)) {
						info.description = reader.GetString(descriptionOrdinal);
					}

					info.latestDownload = LatestDownloadDate(progid);

					Guid pluginId = new Guid(reader.GetString(reader.GetOrdinal("pluginid")));
					IRadioProvider providerInst = pluginsInst.GetPluginInstance(pluginId);
					info.providerName = providerInst.ProviderName;

					info.favourite = IsFavourite(progid);

					return info;
				}
			}
		}

		public EpisodeData FetchEpisodeData(int epid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select name, description, date, duration, autodownload from episodes where epid=@epid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@epid", epid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					int descriptionOrdinal = reader.GetOrdinal("description");

					EpisodeData info = new EpisodeData();
					info.episodeDate = reader.GetDateTime(reader.GetOrdinal("date"));
					info.name = TextUtils.StripDateFromName(reader.GetString(reader.GetOrdinal("name")), info.episodeDate);

					if (!reader.IsDBNull(descriptionOrdinal)) {
						info.description = reader.GetString(descriptionOrdinal);
					}

					info.duration = reader.GetInt32(reader.GetOrdinal("duration"));
					info.autoDownload = reader.GetInt32(reader.GetOrdinal("autodownload")) == 1;

					return info;
				}
			}
		}

		public ProgrammeData FetchProgrammeData(int progid)
		{
			ProgrammeData info = new ProgrammeData();

			using (SQLiteCommand command = new SQLiteCommand("select name, description, singleepisode from programmes where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));

				using (SQLiteMonDataReader reader = new SQLiteMonDataReader(command.ExecuteReader())) {
					if (reader.Read() == false) {
						return null;
					}

					int descriptionOrdinal = reader.GetOrdinal("description");

					info.name = reader.GetString(reader.GetOrdinal("name"));

					if (!reader.IsDBNull(descriptionOrdinal)) {
						info.description = reader.GetString(descriptionOrdinal);
					}

					info.singleEpisode = reader.GetBoolean(reader.GetOrdinal("singleepisode"));
				}
			}

			info.favourite = IsFavourite(progid);
			info.subscribed = IsSubscribed(progid);

			return info;
		}

		public ProviderData FetchProviderData(Guid providerId)
		{
			IRadioProvider providerInstance = pluginsInst.GetPluginInstance(providerId);

			ProviderData info = new ProviderData();
			info.name = providerInstance.ProviderName;
			info.description = providerInstance.ProviderDescription;
			info.icon = providerInstance.ProviderIcon;
			info.showOptionsHandler = providerInstance.GetShowOptionsHandler();

			return info;
		}

		public DownloadCols DownloadSortByCol {
			get { return downloadSortBy; }
			set {
				lock (downloadSortCacheLock) {
					if (value != downloadSortBy) {
						downloadSortCache = null;
					}

					downloadSortBy = value;
				}
			}
		}

		public bool DownloadSortAscending {
			get { return downloadSortAsc; }
			set {
				lock (downloadSortCacheLock) {
					if (value != downloadSortAsc) {
						downloadSortCache = null;
					}

					downloadSortAsc = value;
				}
			}
		}

		public string DownloadQuery {
			get { return search.DownloadQuery; }
			set { search.DownloadQuery = value; }
		}

		private bool IsFavourite(int progid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select count(*) from favourites where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));
				return Convert.ToInt32(command.ExecuteScalar()) > 0;
			}
		}

		private bool IsSubscribed(int progid)
		{
			using (SQLiteCommand command = new SQLiteCommand("select count(*) from subscriptions where progid=@progid", FetchDbConn())) {
				command.Parameters.Add(new SQLiteParameter("@progid", progid));
				return Convert.ToInt32(command.ExecuteScalar()) > 0;
			}
		}
		static bool InitStaticVariableHelper(Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag flag)
		{
			if (flag.State == 0) {
				flag.State = 2;
				return true;
			} else if (flag.State == 2) {
				throw new Microsoft.VisualBasic.CompilerServices.IncompleteInitialization();
			} else {
				return false;
			}
		}
	}
}

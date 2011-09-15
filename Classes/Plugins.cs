/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2011 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace RadioDld
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Windows.Forms;

    public delegate void FindNewViewChangeEventHandler(object view);

    public delegate void FindNewExceptionEventHandler(Exception findExp, bool unhandled);

    public delegate void FoundNewEventHandler(string progExtId);

    public delegate void ProgressEventHandler(int percent, string statusText, ProgressIcon icon);

    public delegate void FinishedEventHandler(string fileExtension);

    public enum ProgressIcon
    {
        Downloading,
        Converting
    }

    public interface IRadioProvider
    {
        event FindNewViewChangeEventHandler FindNewViewChange;

        event FindNewExceptionEventHandler FindNewException;

        event FoundNewEventHandler FoundNew;

        event ProgressEventHandler Progress;

        event FinishedEventHandler Finished;

        Guid ProviderId { get; }

        string ProviderName { get; }

        Bitmap ProviderIcon { get; }

        string ProviderDescription { get; }

        int ProgInfoUpdateFreqDays { get; }

        EventHandler GetShowOptionsHandler();

        Panel GetFindNewPanel(object view);

        GetProgrammeInfoReturn GetProgrammeInfo(string progExtId);

        string[] GetAvailableEpisodeIds(string progExtId);

        GetEpisodeInfoReturn GetEpisodeInfo(string progExtId, string episodeExtId);

        void DownloadProgramme(string progExtId, string episodeExtId, ProgrammeInfo progInfo, EpisodeInfo epInfo, string finalName);
    }

    public struct ProgrammeInfo
    {
        public string Name;
        public string Description;
        public Bitmap Image;
        public bool SingleEpisode;
    }

    public struct GetProgrammeInfoReturn
    {
        public ProgrammeInfo ProgrammeInfo;
        public bool Success;
        public Exception Exception;
    }

    public struct EpisodeInfo
    {
        public string Name;
        public string Description;
        public int? DurationSecs;
        public DateTime? Date;
        public Bitmap Image;
        public Dictionary<string, string> ExtInfo;
    }

    public struct GetEpisodeInfoReturn
    {
        public EpisodeInfo EpisodeInfo;
        public bool Success;
    }

    // Parts of this class are based on VB.net code from http://www.developerfusion.co.uk/show/4371/3/
    internal static class Plugins
    {
        private static string interfaceName = typeof(IRadioProvider).Name;
        private static Dictionary<Guid, AvailablePlugin> availablePlugins = new Dictionary<Guid, AvailablePlugin>();

        static Plugins()
        {
            // Get an array of all of the dlls ending with the text Provider in the application folder
            string pluginPath = AppDomain.CurrentDomain.BaseDirectory;
            string[] dlls = Directory.GetFileSystemEntries(pluginPath, "*Provider.dll");
            
            foreach (string dll in dlls)
            {
                try
                {
                    Assembly thisDll = Assembly.LoadFrom(dll);
                    Plugins.ExamineAssembly(thisDll);
                }
                catch
                {
                    // Error loading DLL, we don't need to do anything special
                }
            }
        }

        public static bool PluginExists(Guid pluginId)
        {
            return availablePlugins.ContainsKey(pluginId);
        }

        public static IRadioProvider GetPluginInstance(Guid pluginId)
        {
            if (PluginExists(pluginId))
            {
                return CreateInstance(availablePlugins[pluginId]);
            }
            else
            {
                return null;
            }
        }

        public static Guid[] GetPluginIdList()
        {
            Guid[] pluginIDs = new Guid[availablePlugins.Keys.Count];
            availablePlugins.Keys.CopyTo(pluginIDs, 0);

            return pluginIDs;
        }

        private static void ExamineAssembly(Assembly dll)
        {
            Type implInterface = null;
            AvailablePlugin pluginInfo = default(AvailablePlugin);

            // Loop through each type in the assembly
            foreach (Type thisType in dll.GetTypes())
            {
                // Only look at public types
                if (thisType.IsPublic)
                {
                    // Ignore abstract classes
                    if (!((thisType.Attributes & TypeAttributes.Abstract) == TypeAttributes.Abstract))
                    {
                        // See if this type implements our interface
                        implInterface = thisType.GetInterface(interfaceName, true);

                        if (implInterface != null)
                        {
                            pluginInfo = new AvailablePlugin();
                            pluginInfo.AssemblyPath = dll.Location;
                            pluginInfo.ClassName = thisType.FullName;

                            try
                            {
                                IRadioProvider pluginInst = null;
                                pluginInst = CreateInstance(pluginInfo);
                                availablePlugins.Add(pluginInst.ProviderId, pluginInfo);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private static IRadioProvider CreateInstance(AvailablePlugin plugin)
        {
            try
            {
                // Load the assembly
                Assembly dll = Assembly.LoadFrom(plugin.AssemblyPath);

                // Create and return class instance
                object pluginInst = dll.CreateInstance(plugin.ClassName);

                // Cast to an IRadioProvider and return
                return (IRadioProvider)pluginInst;
            }
            catch
            {
                return null;
            }
        }

        private struct AvailablePlugin
        {
            public string AssemblyPath;
            public string ClassName;
        }
    }
}

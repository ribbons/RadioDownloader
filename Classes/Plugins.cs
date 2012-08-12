/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 Matt Robinson
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
    using System.IO;
    using System.Reflection;

    internal static class Plugins
    {
        private static Dictionary<Guid, AvailablePlugin> availablePlugins = new Dictionary<Guid, AvailablePlugin>();

        static Plugins()
        {
            // Get an array of all of the dlls ending with the text Provider in the application folder
            string[] dlls = Directory.GetFileSystemEntries(AppDomain.CurrentDomain.BaseDirectory, "*Provider.dll");
            
            foreach (string dll in dlls)
            {
                try
                {
                    Plugins.ExamineAssembly(Assembly.LoadFrom(dll));
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

        public static string PluginClass(Guid pluginId)
        {
            if (PluginExists(pluginId))
            {
                return availablePlugins[pluginId].ClassName;
            }
            else
            {
                return null;
            }
        }

        public static string PluginName(Guid pluginId)
        {
            if (PluginExists(pluginId))
            {
                return availablePlugins[pluginId].DisplayName;
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

        public static string PluginInfo(Guid pluginId)
        {
            IRadioProvider providerInst = Plugins.GetPluginInstance(pluginId);

            return providerInst.ProviderName +
                "\r\nDescription: " + providerInst.ProviderDescription +
                "\r\nID: " + pluginId.ToString();
        }

        private static void ExamineAssembly(Assembly dll)
        {
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
                        Type implInterface = thisType.GetInterface(typeof(IRadioProvider).Name, true);

                        if (implInterface != null)
                        {
                            AvailablePlugin pluginInfo = new AvailablePlugin();
                            pluginInfo.AssemblyPath = dll.Location;
                            pluginInfo.ClassName = thisType.FullName;

                            try
                            {
                                IRadioProvider pluginInst = CreateInstance(pluginInfo);
                                pluginInfo.DisplayName = pluginInst.ProviderName;
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
                return (IRadioProvider)dll.CreateInstance(plugin.ClassName);
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
            public string DisplayName;
        }
    }
}

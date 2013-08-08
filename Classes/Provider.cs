/* 
 * This file is part of Radio Downloader.
 * Copyright © 2007-2013 Matt Robinson
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

    internal class Provider
    {
        private static Dictionary<Guid, Provider> availableProviders = new Dictionary<Guid, Provider>();

        private Assembly assembly;
        private string fullClass;

        static Provider()
        {
            // Get an array of all of the dlls ending with the text Provider in the application folder
            string[] dlls = Directory.GetFileSystemEntries(AppDomain.CurrentDomain.BaseDirectory, "*Provider.dll");
            
            foreach (string dll in dlls)
            {
                try
                {
                    Provider.ExamineAssembly(Assembly.LoadFrom(dll));
                }
                catch
                {
                    // Error loading DLL, we don't need to do anything special
                }
            }
        }

        public Guid Id { get; private set; }

        public string Class { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Bitmap Icon { get; private set; }

        public EventHandler ShowOptionsHandler { get; private set; }

        public static bool Exists(Guid pluginId)
        {
            return availableProviders.ContainsKey(pluginId);
        }
        
        public static Provider GetFromId(Guid pluginId)
        {
            if (Exists(pluginId))
            {
                return availableProviders[pluginId];
            }
            else
            {
                return null;
            }
        }

        public static Provider[] GetAll()
        {
            Provider[] providers = new Provider[availableProviders.Values.Count];
            availableProviders.Values.CopyTo(providers, 0);

            return providers;
        }

        public IRadioProvider CreateInstance()
        {
            try
            {
                return (IRadioProvider)this.assembly.CreateInstance(this.fullClass);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.Name +
                "\r\nDescription: " + this.Description +
                "\r\nID: " + this.Id.ToString();
        }

        private static void ExamineAssembly(Assembly assembly)
        {
            // Loop through each type in the assembly
            foreach (Type thisType in assembly.GetTypes())
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
                            Provider provider = new Provider();
                            provider.assembly  = assembly;
                            provider.fullClass = thisType.FullName;
                            provider.Class = thisType.Name;

                            try
                            {
                                IRadioProvider instance = (IRadioProvider)assembly.CreateInstance(thisType.FullName);
                                provider.Id = instance.ProviderId;
                                provider.Name = instance.ProviderName;
                                provider.Description = instance.ProviderDescription;
                                provider.Icon = instance.ProviderIcon;
                                provider.ShowOptionsHandler = instance.GetShowOptionsHandler();
                                availableProviders.Add(instance.ProviderId, provider);
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
    }
}

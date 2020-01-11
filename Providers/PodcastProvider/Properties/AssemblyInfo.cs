/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2020 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Podcast Plugin")]
[assembly: AssemblyDescription("Plugin to support downloading podcasts within Radio Downloader.")]
[assembly: AssemblyCompany("NerdoftheHerd.com")]
[assembly: AssemblyProduct("Podcast Plugin")]
[assembly: AssemblyTrademark("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-GB")]

[assembly: InternalsVisibleTo("PodcastProvider Test")]

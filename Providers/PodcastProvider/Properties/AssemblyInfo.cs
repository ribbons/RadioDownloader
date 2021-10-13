/*
 * Copyright © 2008-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
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

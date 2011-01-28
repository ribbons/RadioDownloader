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

    internal class DldProgData
    {
        public Guid PluginId { get; set; }

        public int ProgId { get; set; }

        public string ProgExtId { get; set; }

        public int EpId { get; set; }

        public string EpisodeExtId { get; set; }

        public ProgrammeInfo ProgInfo { get; set; }

        public EpisodeInfo EpisodeInfo { get; set; }

        public string FinalName { get; set; }
    }
}

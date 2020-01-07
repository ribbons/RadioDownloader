/*
 * This file is part of Radio Downloader.
 * Copyright © 2020 by the authors - see the AUTHORS file for details.
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

namespace RadioDldTest.Model
{
    using System;
    using System.Globalization;
    using System.IO;

    using RadioDld.Model;
    using Xunit;

    /// <summary>
    /// Tests for the Model.Download class
    /// </summary>
    public class TestDownload
    {
        /// <summary>
        /// Test that CreateSaveFileName replaces all tokens in filename templates
        /// </summary>
        [Fact]
        public void CreateSaveFileNameTemplates()
        {
            var prog = new Programme();
            prog.Name = "PN";

            var episode = new Episode();
            episode.Name = "EN";
            episode.Date = new DateTime(2020, 1, 6, 19, 50, 0);

            Assert.Equal(
                episode.Date.ToString("PN EN yyyy yy MM MMM MMMM dd HH mm", CultureInfo.CurrentCulture),
                Download.CreateSaveFileName("%progname% %epname% %longyear% %year% %month% %shortmonthname% %monthname% %day% %hour% %minute%", prog, episode));
        }

        /// <summary>
        /// Test that CreateSaveFileName removes problematic characters
        /// </summary>
        [Fact]
        public void CreateSaveFileNameCharacters()
        {
            var prog = new Programme();
            prog.Name = ":\"<PN>/\x0\n";

            var episode = new Episode();
            episode.Name = "|EN\\\a?*";

            Assert.Equal(
                "PN EN",
                Download.CreateSaveFileName("%progname% %epname%", prog, episode));
        }

        /// <summary>
        /// Test that FindFreeSaveFileName creates subfolders
        /// </summary>
        [Fact]
        public void FindFreeSaveFileNameSubfolders()
        {
            string tempBase = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempBase);

            var prog = new Programme();
            prog.Name = "PN1";

            var episode = new Episode();
            episode.Name = "EN";

            Assert.Equal(
                Path.Combine(tempBase, Path.Combine("PN1", "EN")),
                Download.FindFreeSaveFileName(@"%progname%\%epname%", prog, episode, tempBase));

            Assert.True(Directory.Exists(Path.Combine(tempBase, "PN1")));

            prog.Name = "PN2";

            Assert.Equal(
                Path.Combine(tempBase, Path.Combine("PN2", "EN")),
                Download.FindFreeSaveFileName(@"%progname%/%epname%", prog, episode, tempBase));

            Assert.True(Directory.Exists(Path.Combine(tempBase, "PN2")));

            Directory.Delete(tempBase, true);
        }
    }
}

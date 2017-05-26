/*
 * This file is part of Radio Downloader.
 * Copyright © 2017 by the authors - see the AUTHORS file for details.
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

namespace RadioDldTest
{
    using System;

    using RadioDld;
    using Xunit;

    /// <summary>
    /// Tests for functions in the static TextUtils class
    /// </summary>
    public class TestTextUtils
    {
        /// <summary>
        /// Test that the StripDateFromName function strips all reasonable
        /// date formats that we have encountered
        /// </summary>
        [Fact]
        public void StripDateFromNameFormats()
        {
            DateTime date = new DateTime(2009, 02, 13);

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Feb 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Feb 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 February 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13th Feb 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13th February 2009", date));
        }

        /// <summary>
        /// Test that the StripDateFromName function strips dates from
        /// different positions in the name
        /// </summary>
        [Fact]
        public void StripDateFromNamePositions()
        {
            DateTime date = new DateTime(2009, 02, 13);

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("2009/02/13 Test", date));
        }

        /// <summary>
        /// Test that the StripDateFromName function strips dates within
        /// a few days of the given date
        /// </summary>
        [Fact]
        public void StripDateFromNameSimilarDate()
        {
            DateTime date = new DateTime(2009, 02, 13);

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 16/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 11/02/2009", date));
        }

        /// <summary>
        /// Test that the StripDateFromName function doesn't strip dates which
        /// are completely different
        /// </summary>
        [Fact]
        public void StripDateFromNameIgnoreDifferent()
        {
            DateTime date = new DateTime(2009, 02, 13);
            Assert.Equal("Test 26/05/2010", TextUtils.StripDateFromName("Test 26/05/2010", date));
        }
    }
}

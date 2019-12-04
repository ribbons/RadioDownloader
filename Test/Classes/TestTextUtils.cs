/*
 * This file is part of Radio Downloader.
 * Copyright © 2017-2019 by the authors - see the AUTHORS file for details.
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
        /// date formats that we have encountered from the start, middle or
        /// end of the name.
        /// </summary>
        [Fact]
        public void StripDateFromNameFormats()
        {
            DateTime date = new DateTime(2009, 02, 13);

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13.2.09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13.2.2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13.02.09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13.02.2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-2-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-2-2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-02-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-02-2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/2/09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/2/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/02/09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/02/2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Feb 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Feb '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Feb 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-Feb-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13-Feb-2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 February 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 February '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 February 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13th Feb 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13th February 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009-02-13", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009/02/13", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009.02.13", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 13th 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 13th '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test:Feb 13th 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test. Feb 13 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test, Feb 13 '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test - Feb 13 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("13 Feb 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13 Feb '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13 Feb 2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th Feb 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th Feb '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th Feb 2009 Test", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("13 February 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13 February '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13 February 2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th February 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th February '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("13th February 2009 Test", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13th 09, Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13th '09: Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13th 2009 - Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13 09.Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13 '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Feb 13 2009 Test", date));

            Assert.Equal("Test: Test", TextUtils.StripDateFromName("Test: 13.2.09: Test", date));
            Assert.Equal("Test, Test", TextUtils.StripDateFromName("Test, 13.2.2009, Test", date));
            Assert.Equal("Test - Test", TextUtils.StripDateFromName("Test - 13.02.09 - Test", date));
            Assert.Equal("Test, Test", TextUtils.StripDateFromName("Test, 13.02.2009, Test", date));

            Assert.Equal("Test, Test", TextUtils.StripDateFromName("Test, 13-2-09: Test", date));
            Assert.Equal("Test - Test", TextUtils.StripDateFromName("Test - 13-2-2009. Test", date));
            Assert.Equal("Test. Test", TextUtils.StripDateFromName("Test. 13-02-09, Test", date));
            Assert.Equal("Test: Test", TextUtils.StripDateFromName("Test: 13-02-2009 - Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13/2/09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13/2/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13/02/09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13/02/2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13-Feb-09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13-Feb-2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th Feb 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-2-13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/2/13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.2.13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-2-13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/2/13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.2.13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-02-13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/02/13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.02.13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-02-13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/02/13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.02.13 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Feb 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th Feb 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 February 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13th February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13th 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test Feb 13 2009 Test", date));

            // test dates with leading zeros
            date = new DateTime(2009, 02, 03);

            Assert.Equal("Test 1", TextUtils.StripDateFromName("Test 1 3.2.09", date));
            Assert.Equal("Test 2", TextUtils.StripDateFromName("Test 2 3.2.2009", date));
            Assert.Equal("Test 3", TextUtils.StripDateFromName("Test 3 3.02.09", date));
            Assert.Equal("Test 4", TextUtils.StripDateFromName("Test 4 3.02.2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-2-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-2-2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-02-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-02-2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3/2/09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3/2/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3/02/09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3/02/2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 Feb 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 Feb '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 Feb 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-Feb-09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3-Feb-2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 February 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 February '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3 February 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3rd Feb 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 3rd February 2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009-2-3", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009/2/3", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009.2.3", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009-2-03", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009/2/03", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009.2.03", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009-02-3", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009/02/3", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 2009.02.3", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 3rd 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 3rd '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 3rd 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 03 09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 03 '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test Feb 03 2009", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3.2.09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3.2.2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3.02.09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3.02.2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-2-09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-2-2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-02-09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-02-2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3/2/09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3/2/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3/02/09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3/02/2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 Feb 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-Feb-09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3-Feb-2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3 February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3rd Feb 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 3rd February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-2-3 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/2/3 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.2.3 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-2-03 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/2/03 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.2.03 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009-02-3 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/02/3 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009.02.3 Test", date));

            date = new DateTime(2009, 02, 01);

            Assert.Equal("Test", TextUtils.StripDateFromName("1 Feb 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1 Feb '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1 Feb 2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st Feb 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st Feb '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st Feb 2009 Test", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("1 February 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1 February '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1 February 2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st February 09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st February '09 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("1st February 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 Feb 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st Feb 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st Feb '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st Feb 2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 February 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st February 09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st February '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1st February 2009 Test", date));

            // test dates in September including the potential 'Sept' abbreviation
            date = new DateTime(2009, 09, 01);

            Assert.Equal("Test", TextUtils.StripDateFromName("2009/9/1 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("2009/09/1 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 Sep 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 01 Sept '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 1 Sept 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 01/09/2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/9/1 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/09/1 Test", date));

            date = new DateTime(2009, 09, 13);

            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Sep 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Sept '09", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13 Sept 2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 13/09/2009", date));

            Assert.Equal("Test", TextUtils.StripDateFromName("2009/9/13 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("2009/09/13 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Sep 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Sept '09 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13 Sept 2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 13/09/2009 Test", date));

            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/9/13 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 2009/09/13 Test", date));
        }

        /// <summary>
        /// Test that the StripDateFromName function strips dates within
        /// a few days of the given date
        /// </summary>
        [Fact]
        public void StripDateFromNameSimilarDate()
        {
            DateTime date = new DateTime(2009, 02, 13);

            // older than date by 5-6 days
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 08/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("07/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 07/02/2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 08/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("7/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 07/02/2009 Test", date));

            // older than date by 1-2 days
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 11/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("12/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 11/02/2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 11/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("11/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 12/02/2009 Test", date));

            // newer than date by 1-2 days
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 15/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("14/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 15/02/2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 14/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("15/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 14/02/2009 Test", date));

            // newer than date by 5-6 days
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 18/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("19/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 19/02/2009 Test", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("Test 19/02/2009", date));
            Assert.Equal("Test", TextUtils.StripDateFromName("18/02/2009 Test", date));
            Assert.Equal("Test Test", TextUtils.StripDateFromName("Test 18/02/2009 Test", date));
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
            Assert.Equal("Test 26/05/2011", TextUtils.StripDateFromName("Test 26/05/2011", date));
            Assert.Equal("Test 2010/05/26", TextUtils.StripDateFromName("Test 2010/05/26", date));
            Assert.Equal("Test 26/08/2010", TextUtils.StripDateFromName("Test 26/08/2010", date));
            Assert.Equal("Test 06/05/2012", TextUtils.StripDateFromName("Test 06/05/2012", date));

            // older than date by >9 days
            Assert.Equal("Test 01/02/2009", TextUtils.StripDateFromName("Test 01/02/2009", date));
            Assert.Equal("01/02/2009 Test", TextUtils.StripDateFromName("01/02/2009 Test", date));
            Assert.Equal("Test 02/02/2009 Test", TextUtils.StripDateFromName("Test 02/02/2009 Test", date));
            Assert.Equal("Test 02/02/2009", TextUtils.StripDateFromName("Test 02/02/2009", date));
            Assert.Equal("01/02/2009 Test", TextUtils.StripDateFromName("01/02/2009 Test", date));
            Assert.Equal("Test 02/02/2009 Test", TextUtils.StripDateFromName("Test 02/02/2009 Test", date));

            // newer than date by >9 days
            Assert.Equal("Test 22/02/2009", TextUtils.StripDateFromName("Test 22/02/2009", date));
            Assert.Equal("24/02/2009 Test", TextUtils.StripDateFromName("24/02/2009 Test", date));
            Assert.Equal("Test 23/02/2009 Test", TextUtils.StripDateFromName("Test 23/02/2009 Test", date));
            Assert.Equal("Test 24/02/2009", TextUtils.StripDateFromName("Test 24/02/2009", date));
            Assert.Equal("22/02/2009 Test", TextUtils.StripDateFromName("22/02/2009 Test", date));
            Assert.Equal("Test 22/02/2009 Test", TextUtils.StripDateFromName("Test 22/02/2009 Test", date));
        }

        /// <summary>
        /// Test that the EpisodeSmartName function correctly normalises episode names
        /// </summary>
        [Fact]
        public void EpisodeSmartName()
        {
            DateTime date = new DateTime(2017, 12, 05);

            Assert.Equal(
                "Programme 1: Episode 1",
                TextUtils.EpisodeSmartName("Programme 1", "Episode 1", date));

            Assert.Equal(
                "Programme 2: Episode 1",
                TextUtils.EpisodeSmartName("Programme 2", "Programme 2: Episode 1", date));

            Assert.Equal(
                "Programme 3: Episode 1",
                TextUtils.EpisodeSmartName("Programme 3", "Programme 3 - Episode 1 - 05/12/2017", date));

            Assert.Equal(
                "Programme 4: Episode 1",
                TextUtils.EpisodeSmartName("Programme 4", "Episode 1: 05/12/2017", date));

            Assert.Equal(
                "Programme 5",
                TextUtils.EpisodeSmartName("Programme 5", "05/12/2017", date));

            Assert.Equal(
                "Programme 6",
                TextUtils.EpisodeSmartName("Programme 6", "Programme 6", date));

            Assert.Equal(
                "Programme 7",
                TextUtils.EpisodeSmartName("Programme 7", "Programme 7. 05/12/2017", date));

            Assert.Equal(
                "Programme 8",
                TextUtils.EpisodeSmartName("Programme 8", "Programme 8, 05/12/2017", date));

            Assert.Equal(
                "Programme - 9: Episode - 1",
                TextUtils.EpisodeSmartName("Programme - 9", "Episode - 1", date));

            Assert.Equal(
                "Programme [10]: Episode (2)",
                TextUtils.EpisodeSmartName("Programme [10]", "Programme [10]: Episode (2)", date));

            Assert.Equal(
                "Programme 11: Episode 1",
                TextUtils.EpisodeSmartName("Programme 11", "Programme 11: 05/12/2017: Episode 1", date));

            Assert.Equal(
                "Programme 12: Episode 1",
                TextUtils.EpisodeSmartName("Programme 12", "05/12/2017: Programme 12, Episode 1", date));

            Assert.Equal(
                "Programme 13",
                TextUtils.EpisodeSmartName("Programme 13", "Programme 13 05/12/2017", date));

            Assert.Equal(
                "The Programme 14: Episode 1",
                TextUtils.EpisodeSmartName("The Programme 14", "Programme 14: Episode 1", date));
        }
    }
}

/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2017 by the authors - see the AUTHORS file for details.
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

namespace RadioDld
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class TextUtils
    {
        // Move Static RegEx content here to declutte the 'StripDateFromName' function
        // Declarations

        // Key date formats to look out for
        // 1 / d(dd)(st | nd | rd | th) | m(mm)(mmm) | yyyy('yy)(yy)
        // 2 / yyyy('yy)(yy)|m(mm)(mmm)|d(dd)(st|nd|rd|th)
        // 3 / mmm|d(dd)(st|nd|rd|th)|yyyy('yy)(yy)

        // ## Common Delims
        //    .| -|/|{ ws}
        //
        // ###Test Beds
        // https://www.debuggex.com/
        // http://regexstorm.net/tester
        // https://regex101.com/
        //
        // new RegEx in Java
        // (((3[01]|2\d|1\d|0?\d)(st|nd|rd|th)?(\.|\-|\/| )((2\d|1\d|0?\d)|(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\.|\-|\/| )(\d{4}|\'\d{2}|\d{2}))( ?)|((\d{4}|\'\d{2}|\d{2})(\.|\-|\/| )((2\d|1\d|0?\d)|(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\.|\-|\/| )(3[01]|2\d|1\d|0?\d)(st|nd|rd|th)?( ?))|((?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\.|\-|\/| )(3[01]|2\d|1\d|0?\d)(st|nd|rd|th)?(\.|\-|\/| )((?:(?:\d{4})|(?:\d{2})(?![\\d])|(?:\')(?:\d{2})(?![\\d])))( ?))

        // Common Groups
        private static string reDelim = @"(\.|\-|\/| )"; // Common Delims .| -|/|{ ws}
        private static string reDay = @"(3[01]|2\d|1\d|0?\d)(st|nd|rd|th)?";       // d(dd)(st | nd | rd | th)
        private static string reDaySuffix = "(?<=[0-9])(?:st |nd |rd |th )";
        private static string reMonth = @"(2\d|1\d|0?\d)"; // m(mm)
        private static string reMonthText = @"(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)"; // (mmm)
        private static string reYear = @"((?:(?:\d{4})|(?:\d{2})(?![\\d])|(?:\')(?:\d{2})(?![\\d])))"; // yyyy('yy)(yy)
        private static string reWS = @"( ?)"; // trailing white space

        // Key date formats to look out for
        // 1 / d(dd)(st | nd | rd | th) | m(mm)(mmm) | yyyy('yy)(yy)
        private static string reF1 = @reDay + reDelim + "(" + reMonth + "|" + reMonthText + ")" + reDelim + reYear + reWS;

        // 2 / yyyy('yy)(yy)|m(mm)(mmm)|d(dd)(st|nd|rd|th)
        private static string reF2 = @"(" + reYear + reDelim + "(" + reMonth + "|" + reMonthText + ")" + reDelim + reDay + reWS + ")";

        // 3 / mmm|d(dd)(st|nd|rd|th)|yyyy('yy)(yy)
        private static string reF3 = @"(" + reMonthText + ")" + reDelim + reDay + reDelim + reYear + reWS;

        // Build RegEx string
        private static Regex matchStripDate = new Regex("(" + reF1 + "|" + reF2 + "|" + reF3 + ")", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
        private static Regex removeDaySuffix = new Regex(reDaySuffix, RegexOptions.IgnoreCase);

        // Set day range for Similar date check
        private static int iSimilarDateDayRange = 6;
        //// private static int iSimilarDateDayRange = 3;

        /* OLD FUNCTION for record
        *  public static string oldStripDateFromName(string name, DateTime stripDate)
        *  {
        *      // Use regex to remove a number of different date formats from episode titles.
        *      // Will only remove dates with the same month & year as the programme itself, but any day of the month
        *      // as there is sometimes a mismatch of a day or two between the date in a title and the publish date.
        *      Regex matchStripDate = new Regex("\\A(" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/\\d{2} ?-? )?(?<name>.*?)( ?:? (\\d{2}/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "|((Mon|Tue|Wed|Thu|Fri) )?(\\d{1,2}(st|nd|rd|th)? )?(" + stripDate.ToString("MMMM", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("MMM", CultureInfo.InvariantCulture) + ")( \\d{1,2}(st|nd|rd|th)?| (" + stripDate.ToString("yy", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "))?))?\\Z");
        *      if (matchStripDate.IsMatch(name))
        *      {
        *          name = matchStripDate.Match(name).Groups["name"].ToString();
        *      }
        *      return name;
        *  }
        */

        public static string StripDateFromName(string name, DateTime stripDate)
        {
            // Use regex to remove a number of different date formats from episode titles.
            // Will only remove dates within a set range eitherside of the programme date.

            // Issue #59
            // - Look for date in middle of name (as well as start and end)
            // - look for other date formats like 13.02.2009; also 13 Feb 2009; 13th feb 2009; 13th-Feb-2009; 13feb09;
            // - consider full day names as well eg Monday, Tuesday...
            // - also other delims before and or after the date e.g. 'title - 2009-02-13'
            try
            {
                // Checks for match on ANY date in the string...
                if (matchStripDate.IsMatch(name))
                {
                    // Date Found - now get the date value found
                    string sDateFound = matchStripDate.Match(name).ToString();

                    // Strip trouble characters eg ' and check for 'sept' and remove 't' also remove (st | nd | rd | th) and keep numeric
                    sDateFound = sDateFound.Replace("'", string.Empty);
                    sDateFound = sDateFound.Replace("Sept", "Sep").Replace("sept", "sep");
                    sDateFound = removeDaySuffix.Replace(sDateFound, " ");

                    // Convert to DateTime for comparison to similardate
                    DateTime dtDateFound = Convert.ToDateTime(sDateFound, CultureInfo.CurrentCulture);

                    // Check if date is within the +/- 'similardateday' range of 'date'
                    if (dtDateFound >= stripDate.AddDays(-iSimilarDateDayRange) && dtDateFound <= stripDate.AddDays(iSimilarDateDayRange))
                    {
                       name = matchStripDate.Replace(name, string.Empty).ToString().Trim();
                    }

                    // name = matchStripDate.Replace(name, string.Empty).ToString().Trim();
                }

                return name;
            }
            catch
            {
                return null;
            }
        }

        public static string DescDuration(int duration)
        {
            string readable = string.Empty;

            if (duration != 0)
            {
                readable += Environment.NewLine + "Duration: ";

                int mins = Convert.ToInt32(Math.Round((double)(duration / 60), 0));
                int hours = mins / 60;
                mins = mins % 60;

                if (hours > 0)
                {
                    readable += Convert.ToString(hours, CultureInfo.CurrentCulture) + "hr" + (hours == 1 ? string.Empty : "s");
                }

                if (hours > 0 && mins > 0)
                {
                    readable += " ";
                }

                if (mins > 0)
                {
                    readable += Convert.ToString(mins, CultureInfo.CurrentCulture) + "min";
                }
            }

            return readable;
        }
    }
}

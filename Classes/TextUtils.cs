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
        public static string StripDateFromName(string name, DateTime stripDate)
        {
            // Use regex to remove a number of different date formats from episode titles.
            // Will only remove dates with the same month & year as the programme itself, but any day of the month
            // as there is sometimes a mismatch of a day or two between the date in a title and the publish date.
            // old RegEx
            // Regex matchStripDate = new Regex("\\A(" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/\\d{2} ?-? )?(?<name>.*?)( ?:? (\\d{2}/" + stripDate.ToString("MM", CultureInfo.InvariantCulture) + "/" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "|((Mon|Tue|Wed|Thu|Fri) )?(\\d{1,2}(st|nd|rd|th)? )?(" + stripDate.ToString("MMMM", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("MMM", CultureInfo.InvariantCulture) + ")( \\d{1,2}(st|nd|rd|th)?| (" + stripDate.ToString("yy", CultureInfo.InvariantCulture) + "|" + stripDate.ToString("yyyy", CultureInfo.InvariantCulture) + "))?))?\\Z");

            // Issue #59
            // - Look for date in middle of name (as well as start and end)
            // - look for other date formats like 13.02.2009; also 13 Feb 2009; 13th feb 2009; 13th-Feb-2009; 13feb09;
            // - consider full day names as well eg Monday, Tuesday...
            // - also other delims before and or after the date e.g. 'title - 2009-02-13'

            // Key formats
            // 1 / d(dd)(st | nd | rd | th) | m(mm)(mmm) | yyyy('yy)(yy)
            // 2 / yyyy('yy)(yy)|m(mm)(mmm)|d(dd)(st|nd|rd|th)
            // 3 / mmm|d(dd)(st|nd|rd|th)|yyyy('yy)(yy)

            // ## Common Delims
            //    .| -|/|{ ws}
            //
            // ###Test Beds
            // https://www.debuggex.com/
            // http://regexstorm.net/tester
            //
            // new RegEx
            string re1 = "((3[01]|2\\d|1\\d|0?\\d)(st|nd|rd|th)?(\\.|\\-|\\/| )((2\\d|1\\d|0?\\d)|(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\\.|\\-|\\/| )(\\d{4}|\'\\d{2}|\\d{2}))( ?)";
            string re2 = "((\\d{4}|\'\\d{2}|\\d{2})(\\.|\\-|\\/| )((2\\d|1\\d|0?\\d)|(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\\.|\\-|\\/| )(3[01]|2\\d|1\\d|0?\\d)(st|nd|rd|th)?( ?))";
            string re3 = "((?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?))(\\.|\\-|\\/| )(3[01]|2\\d|1\\d|0?\\d)(st|nd|rd|th)?(\\.|\\-|\\/| )((?:(?:\\d{4})|(?:\\d{2})(?![\\d])|(?:\')(?:\\d{2})(?![\\d])))( ?)";

            // Checks for match on ANY date...
            // actually need to check for / compare the 'date' as passed in to function
            Regex matchStripDate = new Regex("(" + re1 + "|" + re2 + "|" + re3 + ")", RegexOptions.IgnoreCase);

            if (matchStripDate.IsMatch(name))
            {
                // name = matchStripDate.Match(name).Groups["name"].ToString();
                // name = matchStripDate.Match(name).ToString();
                name = matchStripDate.Replace(name, string.Empty).ToString();
            }

            return name;
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

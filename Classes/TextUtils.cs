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
        // ## Key date formats to look out for
        // 1 / d(dd)(st | nd | rd | th) | m(mm)(mmm) | yyyy('yy)(yy)
        // 2 / yyyy('yy)(yy)|m(mm)(mmm)|d(dd)(st|nd|rd|th)
        // 3 / mmm|d(dd)(st|nd|rd|th)|yyyy('yy)(yy)

        // Common Groups
        // TODO change notion format
        private const string MatchDelim = @"(\.|\-|\/| )";
        private const string MatchDay = @"(3[01]|2\d|1\d|0?\d)(st|nd|rd|th)?";
        private const string MatchDaySuffix = "(?<=[0-9])(?:st |nd |rd |th )";
        private const string MatchMonth = @"(2\d|1\d|0?\d)";
        private const string MatchMonthText = @"(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)";
        private const string MatchYear = @"((?:(?:\d{4})|(?:\d{2})(?![\\d])|(?:\')(?:\d{2})(?![\\d])))";
        private const string MatchWS = @"( ?)";

        private const string MatchFormat1 = @MatchDay + MatchDelim + "(" + MatchMonth + "|" + MatchMonthText + ")" + MatchDelim + MatchYear + MatchWS;
        private const string MatchFormat2 = @"(" + MatchYear + MatchDelim + "(" + MatchMonth + "|" + MatchMonthText + ")" + MatchDelim + MatchDay + MatchWS + ")";
        private const string MatchFormat3 = @"(" + MatchMonthText + ")" + MatchDelim + MatchDay + MatchDelim + MatchYear + MatchWS;

        private static Regex matchStripDate = new Regex("(" + MatchFormat1 + "|" + MatchFormat2 + "|" + MatchFormat3 + ")", RegexOptions.IgnoreCase);
        private static Regex removeDaySuffix = new Regex(MatchDaySuffix, RegexOptions.IgnoreCase);

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

        /// <summary>
        /// Use regex to remove a number of different date formats from episode titles.
        /// Will only remove dates within a set range eitherside of the programme date.
        /// </summary>
        /// <param name="name">Episode Name to be checked.</param>
        /// <param name="stripDate">Date value to be striped from the Episode Name.</param>
        /// <returns>Episode name with date content removed</returns>
        public static string StripDateFromName(string name, DateTime stripDate)
        {
            // try
            // {
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
                }

                return name;

            // }
            // catch
            // {
            //     return null;
            // }
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

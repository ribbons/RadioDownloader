/*
 * Copyright © 2007-2020 Matt Robinson
 * Copyright © 2017-2018 Neil Blanchard
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class TextUtils
    {
        private const string DateDelimPattern = @"[./ -]";
        private const string DayPattern = @"(?:3[01]|2\d|1\d|0?\d)(?:st|nd|rd|th)?";
        private const string MonthNumPattern = @"(?:1[012]|0?\d)";
        private const string MonthTextPattern = @"(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:t(?:ember)?)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)";
        private const string YearPattern = @"(?:\d{4}|'?\d{2}(?![\\d]))";
        private const string DelimOrSpacePattern = @" *[:,.-]? *";

        private static Regex matchDate = new Regex(
            @"(" +
                DayPattern + DateDelimPattern + "(?:" + MonthNumPattern + "|" + MonthTextPattern + ")" + DateDelimPattern + YearPattern +
                "|" +
                YearPattern + DateDelimPattern + "(?:" + MonthNumPattern + "|" + MonthTextPattern + ")" + DateDelimPattern + DayPattern +
                "|" +
                "(?:" + MonthTextPattern + ")" + DateDelimPattern + DayPattern + DateDelimPattern + YearPattern +
            ")" + DelimOrSpacePattern,
            RegexOptions.IgnoreCase);

        private static Regex removeDaySuffix = new Regex("(?<=[0-9])(?:st |nd |rd |th )", RegexOptions.IgnoreCase);
        private static Regex matchSpacesDelimsStart = new Regex("^" + DelimOrSpacePattern);
        private static Regex matchSpacesDelimsEnd = new Regex(DelimOrSpacePattern + "$");

        private static int similarDateDayRange = 6;

        /// <summary>
        /// Remove a number of different date formats from episode titles.
        /// Will only remove dates within a set range (similarDateDayRange) eitherside of the programme date.
        /// </summary>
        /// <param name="name">Episode Name to be checked.</param>
        /// <param name="stripDate">Date value to be stripped from the Episode Name.</param>
        /// <returns>Episode name with date removed.</returns>
        public static string StripDateFromName(string name, DateTime stripDate)
        {
            int startAt = 0;
            Match match;

            while ((match = matchDate.Match(name, startAt)).Success)
            {
                string dateStringFound = match.Groups[1].Value;

                // Strip trouble characters eg ' and check for 'sept' and remove 't' also remove (st | nd | rd | th) and keep numeric
                dateStringFound = dateStringFound.Replace("'", string.Empty);
                dateStringFound = dateStringFound.Replace("Sept", "Sep").Replace("sept", "sep");
                dateStringFound = removeDaySuffix.Replace(dateStringFound, " ");

                // Convert to DateTime for comparison to similardate
                DateTime dateFound;
                var formats = new[]
                {
                    "d/M/yy", "d/M/yyyy", "d/MM/yy", "d/MM/yyyy", "dd/MM/yy", "dd/MM/yyyy",
                    "d MMM yy", "d MMM yyyy", "d MMMM yy", "d MMMM yyyy",
                    "d.M.yy", "d.M.yyyy", "d.MM.yy", "d.MM.yyyy",
                    "d-M-yy", "d-M-yyyy", "d-MM-yy", "d-MM-yyyy", "d-MMM-yy", "d-MMM-yyyy",
                    "yy/M/d", "yyyy/M/d", "yy/MM/d", "yyyy/MM/d", "yy/MM/dd", "yyyy/MM/dd",
                    "yy.M.d", "yyyy.M.d", "yy.MM.d", "yyyy.MM.d", "yy.MM.dd", "yyyy.MM.dd",
                    "yy-M-d", "yyyy-M-d", "yyyy/MM/dd", "yyyy-MM-dd",
                    "MM dd yy", "MM dd yyyy", "MMM d yy", "MMM d yyyy", "MMM dd yy", "MMM dd yyyy",
                };

                if (DateTime.TryParseExact(dateStringFound, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFound))
                {
                    // Check if date is within the +/- 'similardateday' range of 'date'
                    if (dateFound >= stripDate.AddDays(-similarDateDayRange) && dateFound <= stripDate.AddDays(similarDateDayRange))
                    {
                        name = name.Remove(match.Index, match.Length);
                        return matchSpacesDelimsEnd.Replace(name, string.Empty);
                    }

                    return name;
                }

                // Move start position along by the smallest possible
                // distance between matches (one digit and a separator char)
                startAt += 2;
            }

            return name;
        }

        /// <summary>
        /// Build a combined Programme and Episode name in the form Programme Name: Episode Name
        /// The episode date is removed from the episode name if present.
        /// </summary>
        /// <param name="programmeName">Name of the programme which the episode is part of.</param>
        /// <param name="episodeName">Name of the episode itself.</param>
        /// <param name="stripDate">Date value to be removed from the episode name if present.</param>
        /// <returns>Normalised, combined programme and episode name.</returns>
        public static string EpisodeSmartName(string programmeName, string episodeName, DateTime stripDate)
        {
            episodeName = StripDateFromName(episodeName, stripDate);
            int remove = 0;

            // Remove programme name if present at start of episode name
            if (episodeName.StartsWith(programmeName, StringComparison.CurrentCulture))
            {
                remove = programmeName.Length;
            }
            else if (programmeName.StartsWith("The ", StringComparison.CurrentCulture) &&
                     episodeName.StartsWith(programmeName.Substring(4), StringComparison.CurrentCulture))
            {
                remove = programmeName.Length - 4;
            }

            if (remove > 0)
            {
                episodeName = episodeName.Remove(0, remove);
                episodeName = matchSpacesDelimsStart.Replace(episodeName, string.Empty);
            }

            if (string.IsNullOrEmpty(episodeName))
            {
                return programmeName;
            }
            else
            {
                return programmeName + ": " + episodeName;
            }
        }

        /// <summary>
        /// Convert duration value into a human readable string.
        /// </summary>
        /// <param name="duration">The duration value in seconds.</param>
        /// <returns>A human readable string with the number of hours and/or minutes, rounded down to the nearest minute.</returns>
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

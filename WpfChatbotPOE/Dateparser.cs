using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfChatbotPOE
{
        /// <summary>
        /// Finds dates written in everyday language ("tomorrow", "in 3 days",
        /// "next Monday", "25 December", "14/11/2026") inside a sentence.
        /// Part of the NLP simulation (Task 3).
        /// </summary>
        public static class Dateparser
        {
            private const RegexOptions Opts = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

            // Optional word in front of a date that should be removed together with it ("on", "by", "for"...)
            private const string Prefix = @"(?:\b(?:on|by|for|due|at)\s+)?";

            private const string NumberWord = @"\d+|a|an|one|two|three|four|five|six|seven|eight|nine|ten";

            private static readonly string[] DayNames =
            {
            "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday"
        };

            private static readonly string[] MonthNames =
            {
            "january", "february", "march", "april", "may", "june",
            "july", "august", "september", "october", "november", "december"
        };

            private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>
        {
            { "a", 1 }, { "an", 1 }, { "one", 1 }, { "two", 2 }, { "three", 3 },
            { "four", 4 }, { "five", 5 }, { "six", 6 }, { "seven", 7 },
            { "eight", 8 }, { "nine", 9 }, { "ten", 10 }
        };

            /// <summary>
            /// Tries to find a date in the text. If one is found, 'date' holds it and
            /// 'remainingText' is the original text with the date wording removed.
            /// </summary>
            public static bool TryExtract(string input, DateTime now, out DateTime date, out string remainingText)
            {
                Match match;

                if (TryFindDate(input, now, out date, out match))
                {
                    remainingText = Regex.Replace(input.Remove(match.Index, match.Length), @"\s+", " ").Trim();
                    return true;
                }

                remainingText = input;
                return false;
            }

            private static bool TryFindDate(string input, DateTime now, out DateTime date, out Match match)
            {
                date = DateTime.MinValue;
                DateTime today = now.Date;

                // "in 3 days", "in two weeks", "in an hour"
                match = Regex.Match(input,
                    @"\bin\s+(" + NumberWord + @")\s+(minute|minutes|hour|hours|day|days|week|weeks|month|months)\b", Opts);
                if (match.Success)
                {
                    int amount = ParseNumber(match.Groups[1].Value);
                    string unit = match.Groups[2].Value.ToLower();

                    if (unit.StartsWith("minute")) date = now.AddMinutes(amount);
                    else if (unit.StartsWith("hour")) date = now.AddHours(amount);
                    else if (unit.StartsWith("day")) date = today.AddDays(amount);
                    else if (unit.StartsWith("week")) date = today.AddDays(7 * amount);
                    else date = today.AddMonths(amount);

                    return true;
                }

                // "5 days from now"
                match = Regex.Match(input,
                    @"\b(" + NumberWord + @")\s+(days?|weeks?|months?)\s+from\s+now\b", Opts);
                if (match.Success)
                {
                    int amount = ParseNumber(match.Groups[1].Value);
                    string unit = match.Groups[2].Value.ToLower();

                    if (unit.StartsWith("day")) date = today.AddDays(amount);
                    else if (unit.StartsWith("week")) date = today.AddDays(7 * amount);
                    else date = today.AddMonths(amount);

                    return true;
                }

                // "day after tomorrow"
                match = Regex.Match(input, Prefix + @"\bday\s+after\s+tomorrow\b", Opts);
                if (match.Success)
                {
                    date = today.AddDays(2);
                    return true;
                }

                // "today", "tonight", "this evening"
                match = Regex.Match(input, Prefix + @"\b(?:today|tonight|this\s+(?:morning|afternoon|evening))\b", Opts);
                if (match.Success)
                {
                    date = today;
                    return true;
                }

                // "tomorrow"
                match = Regex.Match(input, Prefix + @"\btomorrow\b", Opts);
                if (match.Success)
                {
                    date = today.AddDays(1);
                    return true;
                }

                // "next week", "next month"
                match = Regex.Match(input, Prefix + @"\bnext\s+(week|month)\b", Opts);
                if (match.Success)
                {
                    date = match.Groups[1].Value.ToLower() == "week" ? today.AddDays(7) : today.AddMonths(1);
                    return true;
                }

                // "monday", "next friday", "on tuesday"
                match = Regex.Match(input,
                    Prefix + @"(?:(?:next|this|coming)\s+)?\b(" + string.Join("|", DayNames) + @")\b", Opts);
                if (match.Success)
                {
                    int target = Array.IndexOf(DayNames, match.Groups[1].Value.ToLower());
                    int daysAhead = (target - (int)today.DayOfWeek + 7) % 7;
                    if (daysAhead == 0) daysAhead = 7;

                    date = today.AddDays(daysAhead);
                    return true;
                }

                // "14/11/2026" (day/month/year)
                match = Regex.Match(input, Prefix + @"\b(\d{1,2})[/\-.](\d{1,2})[/\-.](\d{4})\b", Opts);
                if (match.Success)
                {
                    int day = int.Parse(match.Groups[1].Value);
                    int month = int.Parse(match.Groups[2].Value);
                    int year = int.Parse(match.Groups[3].Value);

                    if (TryBuildDate(year, month, day, out date))
                    {
                        return true;
                    }
                }

                // "25 December", "3rd of March 2027"
                match = Regex.Match(input,
                    Prefix + @"\b(\d{1,2})(?:st|nd|rd|th)?\s+(?:of\s+)?" +
                    @"(january|february|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|jun|jul|aug|sept|sep|oct|nov|dec)\b" +
                    @"(?:\s+(\d{4}))?", Opts);
                if (match.Success)
                {
                    int day = int.Parse(match.Groups[1].Value);
                    string monthText = match.Groups[2].Value.ToLower().Substring(0, 3);
                    int month = Array.FindIndex(MonthNames, m => m.StartsWith(monthText)) + 1;

                    bool hasYear = match.Groups[3].Success;
                    int year = hasYear ? int.Parse(match.Groups[3].Value) : today.Year;

                    if (TryBuildDate(year, month, day, out date))
                    {
                        // No year given and the date has already passed - use next year
                        if (!hasYear && date < today)
                        {
                            TryBuildDate(year + 1, month, day, out date);
                        }

                        return true;
                    }
                }

                match = null;
                return false;
            }

            private static bool TryBuildDate(int year, int month, int day, out DateTime date)
            {
                date = DateTime.MinValue;

                if (year < 1 || year > 9999 || month < 1 || month > 12 ||
                    day < 1 || day > DateTime.DaysInMonth(year, month))
                {
                    return false;
                }

                date = new DateTime(year, month, day);
                return true;
            }

            private static int ParseNumber(string text)
            {
                int number;
                if (int.TryParse(text, out number))
                {
                    return number;
                }

                return NumberWords.TryGetValue(text.ToLower(), out number) ? number : 1;
            }

            /// <summary>
            /// Turns a date into friendly text, for example "tomorrow (Saturday, 14 November 2026)".
            /// </summary>
            public static string Describe(DateTime date, DateTime now)
            {
                string full = date.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);

                if (date.Date == now.Date)
                {
                    return "today";
                }

                if (date.Date == now.Date.AddDays(1))
                {
                    return "tomorrow (" + full + ")";
                }

                return full;
            }
        }    
}

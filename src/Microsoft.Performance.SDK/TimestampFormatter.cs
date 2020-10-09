// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for formatting
    ///     <see cref="Timestamp"/> instances.
    /// </summary>
    public static class TimestampFormatter
    {
        /// <summary>
        ///     Format the timestamp in a grouped way.
        /// </summary>
        public const string FormatGrouped = "N";

        /// <summary>
        ///     Place units in the formatted string.
        /// </summary>
        public const string FormatWithUnits = "U";

        /// <summary>
        ///     Format as seconds.
        /// </summary>
        public const string FormatSeconds = "s";

        /// <summary>
        ///     Format as milliseconds.
        /// </summary>
        public const string FormatMilliseconds = "m";

        /// <summary>
        ///     Format as microseconds.
        /// </summary>
        public const string FormatMicroseconds = "u";

        /// <summary>
        ///     Format as nanoseconds.
        /// </summary>
        public const string FormatNanoseconds = "n";

        /// <summary>
        ///     Format as seconds grouped.
        /// </summary>
        public const string FormatSecondsGrouped = "sN";

        /// <summary>
        ///     Format as milliseconds grouped.
        /// </summary>
        public const string FormatMillisecondsGrouped = "mN";

        /// <summary>
        ///     Format as microseconds grouped.
        /// </summary>
        public const string FormatMicrosecondsGrouped = "uN";

        /// <summary>
        ///     Format as nanoseconds grouped.
        /// </summary>
        public const string FormatNanosecondsGrouped = "nN";

        private static bool TryParseFormat(string format, out TimeUnit timeUnits, out bool includeUnits, out bool includeThousandsSeparator)
        {
            // We could do regex and all sorts of robust parsing for 'format', but our needs are currently simple.
            timeUnits = TimeUnit.Nanoseconds;
            includeUnits = false;
            includeThousandsSeparator = false;

            if (format == null)
            {
                return true;
            }

            bool haveUnits = false;

            for (int i = 0; i < format.Length; ++i)
            {
                switch (format[i])
                {
                    case 'N':
                        if (includeThousandsSeparator)
                        {
                            return false;
                        }

                        includeThousandsSeparator = true;
                        break;

                    case 'U':
                        if (includeUnits)
                        {
                            return false;
                        }

                        includeUnits = true;
                        break;

                    case 'n':
                        if (haveUnits)
                        {
                            return false;
                        }

                        timeUnits = TimeUnit.Nanoseconds;
                        break;

                    case 'u':
                        if (haveUnits)
                        {
                            return false;
                        }

                        timeUnits = TimeUnit.Microseconds;
                        break;

                    case 'm':
                        if (haveUnits)
                        {
                            return false;
                        }

                        timeUnits = TimeUnit.Milliseconds;
                        break;

                    case 's':
                        if (haveUnits)
                        {
                            return false;
                        }

                        timeUnits = TimeUnit.Seconds;
                        break;

                    default:
                        return false;
                }
            }
            
            return true;
        }

        /// <summary>
        ///     Converts the given count of nanoseconds to a string.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The nanoseconds to convert.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="formatProvider">
        ///     The format provider.
        /// </param>
        /// <returns>
        ///     The string representation of the nanoseconds.
        /// </returns>
        public static string ToString(long nanoseconds, string format, IFormatProvider formatProvider)
        {
            TimeUnit timeUnits;
            bool includeThousandsSeparator;
            bool includeUnits;

            if (!TryParseFormat(format, out timeUnits, out includeUnits, out includeThousandsSeparator))
            {
                throw new FormatException("format is invalid or not supported");
            }

            return ToString(nanoseconds, formatProvider, timeUnits, includeUnits, includeThousandsSeparator);
        }

        /// <summary>
        ///     Converts the given count of nanoseconds to a string suitable for the system
        ///     clipboard.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The nanoseconds to convert.
        /// </param>
        /// <param name="format">
        ///     A composite format string.
        /// </param>
        /// <param name="formatProvider">
        ///     The format provider.
        /// </param>
        /// <param name="includeUnits">
        ///     Whether to include units in the generated string.
        /// </param>
        /// <returns>
        ///     The string representation of the nanoseconds.
        /// </returns>
        public static string ToClipboardString(long nanoseconds, string format, IFormatProvider formatProvider, bool includeUnits)
        {
            TimeUnit timeUnits;
            bool includeThousandsSeparator;
            bool formatSpecifiedIncludeUnits;

            if (!TryParseFormat(format, out timeUnits, out formatSpecifiedIncludeUnits, out includeThousandsSeparator))
            {
                throw new FormatException("format is invalid or not supported");
            }

            // Clipboard format never includes separators
            includeThousandsSeparator = false;

            return ToString(nanoseconds, formatProvider, timeUnits, includeUnits, includeThousandsSeparator);
        }

        /// <summary>
        ///     Converts the given count of nanoseconds to a string.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The nanoseconds to convert.
        /// </param>
        /// <param name="formatProvider">
        ///     The format provider.
        /// </param>
        /// <param name="timeUnits">
        ///     The units to use.
        /// </param>
        /// <param name="includeUnits">
        ///     Whether to include units in the generated string.
        /// </param>
        /// <param name="includeThousandsSeparator">
        ///     Whether to include the thousands separator in the generates string.
        /// </param>
        /// <returns>
        ///     The string representation of the nanoseconds.
        /// </returns>
        public static string ToString(long nanoseconds, IFormatProvider formatProvider, TimeUnit timeUnits, bool includeUnits, bool includeThousandsSeparator)
        {
            string fixedFormat;
            string numberFormat;
            decimal value;
            string unitsStr;

            switch (timeUnits)
            {
                case TimeUnit.Nanoseconds:
                    value = (decimal)nanoseconds;
                    unitsStr = "ns";
                    fixedFormat = "F0";
                    numberFormat = "N0";
                    break;

                case TimeUnit.Microseconds:
                    value = (decimal)nanoseconds / (decimal)1000;
                    unitsStr = "us";
                    fixedFormat = "F3";
                    numberFormat = "N3";
                    break;

                case TimeUnit.Milliseconds:
                    value = (decimal)nanoseconds / (decimal)1000000;
                    unitsStr = "ms";
                    fixedFormat = "F6";
                    numberFormat = "N6";
                    break;

                case TimeUnit.Seconds:
                    value = (decimal)nanoseconds / (decimal)1000000000;
                    unitsStr = "s";
                    fixedFormat = "F9";
                    numberFormat = "N9";
                    break;

                default:
                    throw new InvalidEnumArgumentException("timeUnits", (int)timeUnits, typeof(TimeUnit));
            }

            string valueFormat = includeThousandsSeparator ? numberFormat : fixedFormat;

            string text = string.Format(formatProvider, "{0}{1}", value.ToString(valueFormat, formatProvider), includeUnits ? unitsStr : string.Empty);

            return text;
        }


        private static readonly string[] secondsSuffixes = new string[] { "s" };
        private static readonly string[] millisecondsSuffixes = new string[] { "m", "ms" };
        private static readonly string[] microsecondsSuffixes = new string[] { "u", "us", "µs" };
        private static readonly string[] nanosecondsSuffixes = new string[] { "n", "ns" };

        /// <summary>
        ///     Parses the given string to nanoseconds.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The parsed nanoseconds.
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The string cannot be parsed.
        /// </exception>
        /// <exception cref="System.OverflowException">
        ///     The string represents a value not representable by a
        ///     <see cref="long"/>.
        /// </exception>
        public static long ParseToNanoseconds(string s)
        {
            return ParseToNanoseconds(s, TimeUnit.Nanoseconds);
        }

        /// <summary>
        ///     Parses the given string to nanoseconds.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if none are specified in the string.
        /// </param>
        /// <returns>
        ///     The parsed nanoseconds.
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The string cannot be parsed.
        /// </exception>
        /// <exception cref="System.OverflowException">
        ///     The string represents a value not representable by a
        ///     <see cref="long"/>.
        /// </exception>
        public static long ParseToNanoseconds(string s, TimeUnit defaultUnits)
        {
            long result;
            bool success = TryParse(s, defaultUnits, out result);

            if (!success && result == 0)
            {
                throw new FormatException();
            }
            else if (!success && (result == long.MinValue || result == long.MaxValue))
            {
                throw new OverflowException();
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        ///     Parses the string as a timestamp.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="resultNS">
        ///     The parsed timestamp, in units of nanoseconds.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the string was parsed successfully; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, out long resultNS)
        {
            return TryParse(s, TimeUnit.Nanoseconds, out resultNS);
        }

        /// <summary>
        ///     Parses the string as a timestamp.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if none are specified in the string.
        /// </param>
        /// <param name="resultNS">
        ///     The parsed timestamp, in units of nanoseconds.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the string was parsed successfully; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out long resultNS)
        {
            TimeUnit originalUnits;
            return TryParse(s, defaultUnits, out resultNS, out originalUnits);
        }

        /// <summary>
        ///     Parses the string as a timestamp which may include a specification for the units of time.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if none are specified in the string.
        /// </param>
        /// <param name="resultNS">
        ///     The parsed timestamp, in units of nanoseconds.
        /// </param>
        /// <param name="parsedUnits">
        ///     The units that were parsed from the string, or defaultUnits if none were found.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the string was parsed successfully; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out long resultNS, out TimeUnit parsedUnits)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (string.IsNullOrWhiteSpace(s))
            {
                resultNS = 0;
                parsedUnits = TimeUnit.Nanoseconds;
                return false;
            }

            s = s.Trim();

            string numberText;
            TimeUnit units;
            SplitNumberAndUnits(s, defaultUnits, out numberText, out units);

            parsedUnits = units;

            decimal number;
            if (!decimal.TryParse(numberText, out number))
            {
                resultNS = 0;
                parsedUnits = TimeUnit.Nanoseconds;
                return false;
            }

            decimal nanoseconds = DecimalUtils.ConvertToNanoseconds(number, units);

            bool roundResult = DecimalUtils.TryRoundToInt64(nanoseconds, MidpointRounding.AwayFromZero, out resultNS);
            return roundResult;
        }

        private static void SplitNumberAndUnits(string s, TimeUnit defaultUnits, out string remainingText, out TimeUnit units)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(s));
            Debug.Assert(string.Equals(s, s.Trim(), StringComparison.Ordinal));

            string unitsText;

            if (TryExtract(s, nanosecondsSuffixes, out remainingText, out unitsText))
            {
                units = TimeUnit.Nanoseconds;
                return;
            }

            if (TryExtract(s, microsecondsSuffixes, out remainingText, out unitsText))
            {
                units = TimeUnit.Microseconds;
                return;
            }

            if (TryExtract(s, millisecondsSuffixes, out remainingText, out unitsText))
            {
                units = TimeUnit.Milliseconds;
                return;
            }

            if (TryExtract(s, secondsSuffixes, out remainingText, out unitsText))
            {
                units = TimeUnit.Seconds;
                return;
            }

            // At this point we don't guarantee that the rest of the string can be parsed as a number.

            remainingText = s;
            units = defaultUnits;
            return;
        }

        private static bool TryExtract(string s, string[] unitsTexts, out string remainder, out string unitsText)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(s));
            Debug.Assert(string.Equals(s, s.Trim(), StringComparison.Ordinal));

            for (int i = 0; i < unitsTexts.Length; ++i)
            {
                if (TryExtract(s, unitsTexts[i], out remainder))
                {
                    unitsText = unitsTexts[i];
                    return true;
                }
            }

            unitsText = null;
            remainder = null;
            return false;
        }

        private static bool TryExtract(string s, string unitsText, out string remainder)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(s));
            Debug.Assert(string.Equals(s, s.Trim(), StringComparison.Ordinal));

            if (s.EndsWith(unitsText, StringComparison.CurrentCultureIgnoreCase))
            {
                int unitsIndex = s.IndexOf(unitsText, StringComparison.CurrentCultureIgnoreCase);
                remainder = s.Substring(0, unitsIndex);
                return true;
            }
            else
            {
                unitsText = null;
                remainder = null;
                return false;
            }
        }
    }
}

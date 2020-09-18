// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for dealing
    ///     with quantities of bytes and their representations.
    /// </summary>
    public static class BytesConverterUtils
    {
        private static readonly string[] bytesSuffixes = new string[] { "B", "Bytes" };
        private static readonly string[] kilobytesSuffixes = new string[] { "KB", "Kilobytes" };
        private static readonly string[] megabytesSuffixes = new string[] { "MB", "Megabytes" };
        private static readonly string[] gigabytesSuffixes = new string[] { "GB", "Gigabytes" };

        /// <summary>
        ///     Attempts to split the given string argument into the number
        ///     piece and the units piece.
        /// </summary>
        /// <param name="str">
        ///     The <see cref="string"/> to split.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if no units are found in <paramref name="str"/>.
        /// </param>
        /// <param name="remainingText">
        ///     The part of <paramref name="str"/> that could not be parsed.
        /// </param>
        /// <param name="units">
        ///     The units that were parsed / used.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="str"/> is <c>null</c>
        ///     - or -
        ///     <paramref name="str"/> is whitespace.
        /// </exception>
        public static void SplitNumberAndUnits(string str, BytesUnits defaultUnits, out string remainingText, out BytesUnits units)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException("str");
            }

            Debug.Assert(string.Equals(str, str.Trim(), StringComparison.Ordinal));

            string unitsText;

            if (TryExtract(str, kilobytesSuffixes, out remainingText, out unitsText))
            {
                units = BytesUnits.Kilobytes;
                return;
            }

            if (TryExtract(str, megabytesSuffixes, out remainingText, out unitsText))
            {
                units = BytesUnits.Megabytes;
                return;
            }

            if (TryExtract(str, gigabytesSuffixes, out remainingText, out unitsText))
            {
                units = BytesUnits.Gigabytes;
                return;
            }

            if (TryExtract(str, bytesSuffixes, out remainingText, out unitsText))
            {
                units = BytesUnits.Bytes;
                return;
            }

            // At this point we don't guarantee that the rest of the string can be parsed as a number.

            remainingText = str;
            units = defaultUnits;
            return;
        }

        private static bool TryExtract(string str, string[] supportedUnits, out string remainder, out string unitsText)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException("str");
            }

            Debug.Assert(string.Equals(str, str.Trim(), StringComparison.Ordinal));

            for (int i = 0; i < supportedUnits.Length; ++i)
            {
                if (TryExtractForUnit(str, supportedUnits[i], out remainder))
                {
                    unitsText = supportedUnits[i];
                    return true;
                }
            }

            unitsText = null;
            remainder = null;
            return false;
        }

        private static bool TryExtractForUnit(string str, string unitsText, out string remainder)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException("str");
            }

            Debug.Assert(string.Equals(str, str.Trim(), StringComparison.Ordinal));

            if (str.EndsWith(unitsText, StringComparison.CurrentCultureIgnoreCase))
            {
                int unitsIndex = str.IndexOf(unitsText, StringComparison.CurrentCultureIgnoreCase);
                remainder = str.Substring(0, unitsIndex);
                return true;
            }
            else
            {
                unitsText = null;
                remainder = null;
                return false;
            }
        }

        /// <summary>
        ///     Converts the given raw quantity of bytes to a quantity of
        ///     bytes using the given unit.
        /// </summary>
        /// <param name="value">
        ///     The raw number of bytes.
        /// </param>
        /// <param name="units">
        ///     The units with which to represent the quantity specified by
        ///     <paramref name="value"/>.
        /// </param>
        /// <returns>
        ///     A quantity, in the given <paramref name="units"/>, representing the
        ///     given quantity of bytes.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        ///     <paramref name="units"/> is not a valid member of the <see cref="BytesUnits"/>
        ///     enumeration.
        /// </exception>
        public static decimal ConvertToBytes(decimal value, BytesUnits units)
        {
            switch (units)
            {
                case BytesUnits.Bytes:
                    return value;

                case BytesUnits.Kilobytes:
                    return Decimal.Multiply(value, (decimal)1024d);

                case BytesUnits.Megabytes:
                    return Decimal.Multiply(value, (decimal)1048576d);

                case BytesUnits.Gigabytes:
                    return Decimal.Multiply(value, (decimal)1073741824d);

                default:
                    throw new InvalidEnumArgumentException("units", (int)units, typeof(BytesUnits));
            }
        }

        /// <summary>
        ///     Converts the given byte value to a quantity using the largest possible unit
        ///     that still leaves a quantity with a magnitude of at least one in said unit.
        /// </summary>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <param name="bytesUnits">
        ///     The units into which <paramref name="value"/> was converted.
        /// </param>
        /// <returns>
        ///     The converted quantity expressed in <paramref name="bytesUnits"/> units.
        /// </returns>
        public static decimal ConvertToBytesUnits(decimal value, out BytesUnits bytesUnits)
        {
            BytesUnits[] units = new[] { BytesUnits.Bytes, BytesUnits.Kilobytes, BytesUnits.Megabytes, BytesUnits.Gigabytes };
            int index = 0;
            while (value > 1024 && index < (units.Count()-1))
            {
                value /= 1024;
                index++;
            }

            bytesUnits = units[index];

            return value;
        }
    }
}

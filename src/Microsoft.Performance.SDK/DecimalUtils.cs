// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="decimal"/> instances.
    /// </summary>
    public static class DecimalUtils
    {
        private static readonly decimal int64MaxAsDecimal = (decimal)long.MaxValue;
        private static readonly decimal int64MinAsDecimal = (decimal)long.MinValue;
        private static readonly decimal uInt64MaxAsDecimal = (decimal)ulong.MaxValue;
        private static readonly decimal uInt64MinAsDecimal = (decimal)ulong.MinValue;

        /// <summary>
        ///     Attempts to round the given value to an <see cref="Int64"/>. If the
        ///     rounded value is outside of the range of an <see cref="Int64"/>, then
        ///     <paramref name="closestValue"/> will be set to the closer of
        ///     <see cref="Int64.MinValue"/> or <see cref="Int64.MinValue"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to round.
        /// </param>
        /// <param name="mode">
        ///     Specifies the method of rounding to use.
        /// </param>
        /// <param name="closestValue">
        ///     The rounded value.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the rounded value is in the range [<see cref="Int64.MinValue" />, <see cref="Int64.MaxValue" />];
        ///     otherwise <c>false</c>, setting <paramref name="closestValue"/> to the closer of
        ///     <see cref="Int64.MinValue"/> or <see cref="Int64.MinValue"/>.
        /// </returns>
        public static bool TryRoundToInt64(decimal value, MidpointRounding mode, out long closestValue)
        {
            decimal roundedValue = decimal.Round(value, mode);

            if (roundedValue < int64MinAsDecimal)
            {
                closestValue = Int64.MinValue;
                return false;
            }

            if (roundedValue > int64MaxAsDecimal)
            {
                closestValue = Int64.MaxValue;
                return false;
            }

            closestValue = (long)roundedValue;
            return true;
        }

        /// <summary>
        ///     Attempts to round the given value to an <see cref="UInt64"/>. If the
        ///     rounded value is outside of the range of an <see cref="UInt64"/>, then
        ///     <paramref name="closestValue"/> will be set to the closer of
        ///     <see cref="UInt64.MinValue"/> or <see cref="UInt64.MinValue"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to round.
        /// </param>
        /// <param name="mode">
        ///     Specifies the method of rounding to use.
        /// </param>
        /// <param name="closestValue">
        ///     The rounded value.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the rounded value is in the range [<see cref="UInt64.MinValue" />, <see cref="UInt64.MaxValue" />];
        ///     otherwise <c>false</c>, setting <paramref name="closestValue"/> to the closer of
        ///     <see cref="UInt64.MinValue"/> or <see cref="UInt64.MinValue"/>.
        /// </returns>
        public static bool TryRoundToUInt64(decimal value, MidpointRounding mode, out ulong closestValue)
        {
            decimal roundedValue = decimal.Round(value, mode);

            if (roundedValue < uInt64MinAsDecimal)
            {
                closestValue = ulong.MinValue;
                return false;
            }

            if (roundedValue > uInt64MaxAsDecimal)
            {
                closestValue = ulong.MaxValue;
                return false;
            }

            closestValue = (ulong)roundedValue;
            return true;
        }

        /// <summary>
        ///     Converts the given quantity, expressed in the given units,
        ///     to the equivalent quantity in nanoseconds.
        /// </summary>
        /// <param name="value">
        ///     The value to convert.
        /// </param>
        /// <param name="units">
        ///     The <see cref="TimeUnit"/> with which <paramref name="value"/> is
        ///     currently expressed.
        /// </param>
        /// <returns>
        ///     The number of nanoseconds equivalent to <paramref name="value"/>
        ///     <see cref="TimeUnit"/>s.
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        ///     <paramref name="units"/> is not a valid member of the <see cref="TimeUnit"/>
        ///     enumeration.
        /// </exception>
        public static decimal ConvertToNanoseconds(decimal value, TimeUnit units)
        {
            switch (units)
            {
                case TimeUnit.Seconds:
                    return value * (decimal)1000000000;

                case TimeUnit.Milliseconds:
                    return value * (decimal)1000000;

                case TimeUnit.Microseconds:
                    return value * (decimal)1000;

                case TimeUnit.Nanoseconds:
                    return value;

                default:
                    throw new InvalidEnumArgumentException("units", (int)units, typeof(TimeUnit));
            }
        }

        /// <summary>
        ///     Gets the precision of the given <see cref="decimal"/>
        ///     number.
        /// </summary>
        /// <param name="number">
        ///     The quantity whose precision is to be measured.
        /// </param>
        /// <returns>
        ///     The precision of <paramref name="number"/>.
        /// </returns>
        public static int GetPrecision(decimal number)
        {
            return (Decimal.GetBits(number)[3] >> 16) & 0x000000FF;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="Int64"/> instances.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class Int64Utils
    {
        private const long minBoxCacheValue = -128;
        private const long maxBoxCacheValue = 128;
        private static object[] boxCache;

        /// <summary>
        ///     Determines whether the two ranges overlap.
        /// </summary>
        /// <remarks>
        ///     The begin and end values for each range are implicitly swapped if begin is greater 
        ///     than end.
        ///     The range is treated as having an inclusive begin value, and exclusive end value.
        ///     In other words: [begin, end)
        /// </remarks>
        /// <param name="firstBegin">
        ///     The beginning of the first range.
        /// </param>
        /// <param name="firstEnd">
        ///     The end of the first range.
        /// </param>
        /// <param name="secondBegin">
        ///     The beginning of the second range.
        /// </param>
        /// <param name="secondEnd">
        ///     The end of the second range.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges overlap; <c>false</c> otherwise.
        /// </returns>
        public static bool DoRangesOverlap(long firstBegin, long firstEnd, long secondBegin, long secondEnd)
        {
            long firstMin = Math.Min(firstBegin, firstEnd);
            long firstMax = Math.Max(firstBegin, firstEnd);
            long secondMin = Math.Min(secondBegin, secondEnd);
            long secondMax = Math.Max(secondBegin, secondEnd);

            if (firstMin < secondMin && firstMax <= secondMin)
            {
                return false;
            }

            if (firstMin >= secondMax)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Determines whether the two ranges overlap.
        /// </summary>
        /// <remarks>
        ///     The begin and end values for each range are implicitly swapped if begin is greater 
        ///     than end.
        ///     This function behaves the same as the above function, except that the ranges 
        ///     are treated as having both inclusive begin values and inclusive end values
        ///     In other words: [begin, end]
        /// </remarks>
        /// <param name="firstBegin">
        ///     The beginning of the first range.
        /// </param>
        /// <param name="firstEnd">
        ///     The end of the first range.
        /// </param>
        /// <param name="secondBegin">
        ///     The beginning of the second range.
        /// </param>
        /// <param name="secondEnd">
        ///     The end of the second range.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges overlap; <c>false</c> otherwise.
        /// </returns>
        public static bool DoInclusiveRangesOverlap(long firstBegin, long firstEnd, long secondBegin, long secondEnd)
        {
            long firstMin = Math.Min(firstBegin, firstEnd);
            long firstMax = Math.Max(firstBegin, firstEnd);
            long secondMin = Math.Min(secondBegin, secondEnd);
            long secondMax = Math.Max(secondBegin, secondEnd);

            if (firstMin < secondMin && firstMax < secondMin)
            {
                return false;
            }

            if (firstMin > secondMax)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Boxes the given <see cref="long" />
        /// </summary>
        /// <param name="value">
        ///     The <see cref="long"/> to box.
        /// </param>
        /// <returns>
        ///     The boxed <see cref="long"/>.
        /// </returns>
        public static object GetBoxed(long value)
        {
            if (value < minBoxCacheValue || value > maxBoxCacheValue)
            {
                return (object)value;
            }

            if (boxCache == null)
            {
                boxCache = new object[maxBoxCacheValue - minBoxCacheValue + 1];
            }

            long cacheIndex = value - minBoxCacheValue;

            if (boxCache[cacheIndex] == null)
            {
                boxCache[cacheIndex] = (object)value;
            }

            return boxCache[cacheIndex];
        }

        /// <summary>
        ///     Clamps the given value to the range [min, max].
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="min">
        ///     The minimum allowable value. The lower bound of the clamp.
        /// </param>
        /// <param name="max">
        ///     The maximum allowable value. The upper bound of the clamp.
        /// </param>
        /// <returns>
        ///     If value is less than min, then min. If value is between min
        ///     and max, then value. If value is greater than max, then max.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="max"/> is less than <paramref name="min"/>.
        /// </exception>
        public static long Clamp(this long value, long min, long max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    CultureInfo.CurrentCulture,
                    "max ({0}) must be greater than or equal to min ({1})",
                    min,
                    max));
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        ///     Clamps the given value to the range [min, max], ensuring
        ///     that <paramref name="extreme1"/> is less than <paramref name="extreme2"/>
        ///     when calculating the clamp.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="extreme1">
        ///     The first bound of the clamp.
        /// </param>
        /// <param name="extreme2">
        ///     The other bound of the clamp.
        /// </param>
        /// <returns>
        ///     The value, if value is in the range specified by <paramref name="extreme1"/>
        ///     and <paramref name="extreme2"/>. Otherwise, either <paramref name="extreme1"/>
        ///     or <paramref name="extreme2"/>, whichever is closer.
        /// </returns>
        public static long SafeClamp(this long value, long extreme1, long extreme2)
        {
            return Clamp(value, Math.Min(extreme1, extreme2), Math.Max(extreme1, extreme2));
        }

        /// <summary>
        ///     Determines whether the given value is within the specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="min">
        ///     The minimum value.
        /// </param>
        /// <param name="max">
        ///     The maximum value.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value"/> is between
        ///     <paramref name="min"/> and <paramref name="max"/> inclusive;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsClamped(long value, long min, long max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    CultureInfo.CurrentCulture,
                    "max ({0}) must be greater than or equal to min ({1})",
                    min,
                    max));
            }

            return value >= min && value <= max;
        }

        /// <summary>
        ///     Determines whether the given value is within the specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="extreme1">
        ///     The first extreme of the range.
        /// </param>
        /// <param name="extreme2">
        ///     The other extreme of the range.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value"/> is between
        ///     <paramref name="extreme1"/> and <paramref name="extreme2"/> inclusive;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool SafeIsClamped(long value, long extreme1, long extreme2)
        {
            return IsClamped(value, Math.Min(extreme1, extreme2), Math.Max(extreme1, extreme2));
        }
    }
}

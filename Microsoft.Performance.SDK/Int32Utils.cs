// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual basic) methods for interacting
    ///     with <see cref="Int32"/> instances.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class Int32Utils
    {
        private const int minBoxCacheValue = -128;
        private const int maxBoxCacheValue = 128;
        private static object[] boxCache;

        /// <summary>
        ///     Boxes the given <see cref="int" />
        /// </summary>
        /// <param name="value">
        ///     The <see cref="int"/> to box.
        /// </param>
        /// <returns>
        ///     The boxed <see cref="int"/>.
        /// </returns>
        public static object GetBoxed(int value)
        {
            if (value < minBoxCacheValue || value > maxBoxCacheValue)
            {
                return (object)value;
            }

            if (boxCache == null)
            {
                boxCache = new object[maxBoxCacheValue - minBoxCacheValue + 1];
            }

            int cacheIndex = value - minBoxCacheValue;

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
        public static int Clamp(int value, int min, int max)
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
        public static int SafeClamp(int value, int extreme1, int extreme2)
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
        public static bool IsClamped(int value, int min, int max)
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
        public static bool SafeIsClamped(int value, int extreme1, int extreme2)
        {
            return IsClamped(value, Math.Min(extreme1, extreme2), Math.Max(extreme1, extreme2));
        }

        /// <summary>
        ///     Determines whether the given index represents a valid index
        ///     into a collection with the given count.
        /// </summary>
        /// <param name="index">
        ///     The index to check.
        /// </param>
        /// <param name="count">
        ///     The size of the collection.
        /// </param>
        /// <returns>
        ///     <c>true</c> if index can safely be used in a collection of
        ///     size <paramref name="count"/>; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="count"/> is less than zero (0.)
        /// </exception>
        public static bool IsIndexValid(int index, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return (index >= 0) && (index < count);
        }

        /// <summary>
        ///     Performs the operation "x mod y", correctly accounting
        ///     for negative values of x.
        ///     <remarks>
        ///         The modulo function built into C# (%) does not handle negative numbers
        ///         the way one might expect (i.e. (-x) % y = -(x % y)).
        ///         This function returns the remainder when x is divided by y,
        ///         whether x is positive or negative.
        ///     </remarks>
        /// </summary>
        /// <param name="x">
        ///     The value.
        /// </param>
        /// <param name="y">
        ///     The modulus.
        /// </param>
        /// <returns>
        ///     The result of "x mod y"
        /// </returns>
        public static int AbsMod(int x, int y)
        {
            return (y + (x % y)) % y;
        }
    }
}

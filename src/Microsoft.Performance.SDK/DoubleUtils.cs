// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="double"/>s.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class DoubleUtils
    {
        internal const double epsilon = 2.2204460492503131e-016; // smallest such that 1.0+epsilon != 1.0

        private static readonly object boxedZero = (object)0.0;
        private static readonly object boxedOne = (object)1.0;

        /// <summary>
        ///     Gets the value of zero (0), but already boxed into an object.
        /// </summary>
        public static object BoxedZero
        {
            get
            {
                return boxedZero;
            }
        }

        /// <summary>
        ///     Gets the value of one (1.0), but already boxed.
        /// </summary>
        public static object BoxedOne
        {
            get
            {
                return boxedOne;
            }
        }

        /// <summary>
        ///     Gets the boxed value of the given <see cref="double"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to box.
        /// </param>
        /// <returns>
        ///     The boxed value.
        /// </returns>
        public static object GetBoxed(double value)
        {
            if (value == 0.0)
            {
                return BoxedZero;
            }
            else if (value == 1.0)
            {
                return BoxedOne;
            }
            else
            {
                return (object)value;
            }
        }

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
        public static bool DoRangesOverlap(
            double firstBegin, 
            double firstEnd, 
            double secondBegin, 
            double secondEnd)
        {
            if (!firstBegin.IsFinite() || !firstEnd.IsFinite() || !secondBegin.IsFinite() || !secondEnd.IsFinite())
            {
                return false;
            }

            double firstMin = Math.Min(firstBegin, firstEnd);
            double firstMax = Math.Max(firstBegin, firstEnd);
            double secondMin = Math.Min(secondBegin, secondEnd);
            double secondMax = Math.Max(secondBegin, secondEnd);

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
        public static double Clamp(double value, double min, double max)
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
        public static double SafeClamp(double value, double extreme1, double extreme2)
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
        public static bool IsClamped(double value, double min, double max)
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
        public static bool SafeIsClamped(double value, double extreme1, double extreme2)
        {
            return IsClamped(value, Math.Min(extreme1, extreme2), Math.Max(extreme1, extreme2));
        }

        /// <summary>
        ///     Clamps the given value to the range of a <see cref="byte"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to [<see cref="Byte.MinValue"/>, <see cref="Byte.MaxValue"/>].
        /// </returns>
        public static byte ClampToByte(double value)
        {
            if (value <= (double)byte.MinValue)
            {
                return byte.MinValue;
            }
            else if (value >= (double)byte.MaxValue)
            {
                return byte.MaxValue;
            }

            return (byte)value;
        }

        /// <summary>
        ///     Clamps the given value to the range of an <see cref="Int64"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to [<see cref="Int64.MinValue"/>, <see cref="Int64.MaxValue"/>].
        /// </returns>
        public static long ClampToInt64(double value)
        {
            if (value <= (double)long.MinValue)
            {
                return long.MinValue;
            }
            else if (value >= (double)long.MaxValue)
            {
                return long.MaxValue;
            }

            return (long)value;
        }

        /// <summary>
        ///     Clamps the given value to the range of an <see cref="UInt64"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to [<see cref="UInt64.MinValue"/>, <see cref="UInt64.MaxValue"/>].
        /// </returns>
        public static ulong ClampToUInt64(double value)
        {
            if (value <= (double)ulong.MinValue)
            {
                return ulong.MinValue;
            }
            else if (value >= (double)ulong.MaxValue)
            {
                return ulong.MaxValue;
            }

            return (ulong)value;
        }

        /// <summary>
        ///     Clamps the given value to the range of an <see cref="float"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to [<see cref="float.MinValue"/>, <see cref="float.MaxValue"/>].
        /// </returns>
        public static float ClampToFloat(double value)
        {
            if (value <= (double)float.MinValue)
            {
                return float.MinValue;
            }
            else if (value >= (double)float.MaxValue)
            {
                return float.MaxValue;
            }

            return (float)value;
        }

        /// <summary>
        ///     Returns the max of two values.
        /// </summary>
        /// <param name="val0">
        ///     The first value to compare.
        /// </param>
        /// <param name="val1">
        ///     The second value to compare.
        /// </param>
        /// <returns>
        ///     The maximum of the two values.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "val")]
        public static double Max(double val0, double val1)
        {
            return Math.Max(val0, val1);
        }

        /// <summary>
        ///     Returns the max of three values.
        /// </summary>
        /// <param name="val0">
        ///     The first value to compare.
        /// </param>
        /// <param name="val1">
        ///     The second value to compare.
        /// </param>
        /// <param name="val2">
        ///     The third value to compare.
        /// </param>
        /// <returns>
        ///     The maximum of the three values.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "val")]
        public static double Max(double val0, double val1, double val2)
        {
            return Math.Max(val0, Math.Max(val1, val2));
        }

        /// <summary>
        /// AreClose - Returns whether or not two doubles are "close".  That is, whether or 
        /// not they are within epsilon of each other.  Note that this epsilon is proportional
        /// to the numbers themselves to that AreClose survives scalar multiplication.
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <param name="value1"> The first double to compare. </param>
        /// <param name="value2"> The second double to compare. </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value1"/> and <paramref name="value2"/>
        ///     are considered to be close
        /// </returns>
        public static bool AreClose(double value1, double value2)
        {
            //in case they are Infinities (then epsilon check does not work)
            if (value1 == value2) return true;
            // This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < DBL_EPSILON
            double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * epsilon;
            double delta = value1 - value2;
            return (-eps < delta) && (eps > delta);
        }

        /// <summary>
        /// LessThan - Returns whether or not the first double is less than the second double.
        /// That is, whether or not the first is strictly less than *and* not within epsilon of
        /// the other number.  Note that this epsilon is proportional to the numbers themselves
        /// to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// bool - the result of the LessThan comparison.
        /// </returns>
        /// <param name="value1"> The first double to compare. </param>
        /// <param name="value2"> The second double to compare. </param>
        public static bool LessThan(double value1, double value2)
        {
            return (value1 < value2) && !DoubleUtils.AreClose(value1, value2);
        }

        /// <summary>
        /// GreaterThan - Returns whether or not the first double is greater than the second double.
        /// That is, whether or not the first is strictly greater than *and* not within epsilon of
        /// the other number.  Note that this epsilon is proportional to the numbers themselves
        /// to that AreClose survives scalar multiplication.  Note,
        /// There are plenty of ways for this to return false even for numbers which
        /// are theoretically identical, so no code calling this should fail to work if this 
        /// returns false.  This is important enough to repeat:
        /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
        /// used for optimizations *only*.
        /// </summary>
        /// <returns>
        /// bool - the result of the GreaterThan comparison.
        /// </returns>
        /// <param name="value1"> The first double to compare. </param>
        /// <param name="value2"> The second double to compare. </param>
        public static bool GreaterThan(double value1, double value2)
        {
            return (value1 > value2) && !DoubleUtils.AreClose(value1, value2);
        }

        // The standard CLR double.IsNaN() function is approximately 100 times slower than our own wrapper,
        // so please make sure to use DoubleUtils.IsNaN() in performance sensitive code.
        // PS item that tracks the CLR improvement is DevDiv Schedule : 26916.
        // IEEE 754 : If the argument is any value in the range 0x7ff0000000000001L through 0x7fffffffffffffffL 
        // or in the range 0xfff0000000000001L through 0xffffffffffffffffL, the result will be NaN.         

        /// <summary>
        ///     Determines if the given value represents the "Not a Number" (NaN)
        ///     value.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value"/> is NaN;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool IsNaN(double value)
        {
            NanUnion t = new NanUnion();
            t.DoubleValue = value;

            UInt64 exp = t.UintValue & 0xfff0000000000000;
            UInt64 man = t.UintValue & 0x000fffffffffffff;

            return (exp == 0x7ff0000000000000 || exp == 0xfff0000000000000) && (man != 0);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            [FieldOffset(0)]
            internal double DoubleValue;

            [FieldOffset(0)]
            internal UInt64 UintValue;
        }
    }
}

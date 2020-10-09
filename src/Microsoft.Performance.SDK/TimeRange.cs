// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a range between two <see cref="Timestamp"/>s.
    /// </summary>
    [TypeConverter(typeof(TimeRangeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{StartTime.ToNanoseconds}ns - {EndTime.ToNanoseconds}ns")]
    public struct TimeRange
        : IEquatable<TimeRange>,
          IFormatForClipboard,
          IConvertible
    {
        private Timestamp startTime;
        private Timestamp endTime;

        /// <summary>
        ///     Gets or sets the start of the range.
        /// </summary>
        public Timestamp StartTime
        {
            get
            {
                return this.startTime;
            }

            set
            {
                this.startTime = value;
            }
        }

        /// <summary>
        ///     Gets or sets the end of the range.
        /// </summary>
        public Timestamp EndTime
        {
            get
            {
                return this.endTime;
            }

            set
            {
                this.endTime = value;
            }
        }

        /// <summary>
        ///     Explicitly casts the given <see cref="TimeRange"/> to 
        ///     an <see cref="Int64Range"/> consisting of the nanosecond
        ///     representations of the times.
        /// </summary>
        /// <param name="range">
        ///     The range to convert.
        /// </param>
        public static explicit operator Int64Range(TimeRange range)
        {
            return new Int64Range(range.StartTime.ToNanoseconds, range.EndTime.ToNanoseconds);
        }

        /// <summary>
        ///     Explicitly casts the given <see cref="Int64Range"/>
        ///     consisting of the nanosecond representations of the times
        ///     to a <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="rangeNS">
        ///     The range to convert.
        /// </param>
        public static explicit operator TimeRange(Int64Range rangeNS)
        {
            return new TimeRange(Timestamp.FromNanoseconds(rangeNS.Begin), Timestamp.FromNanoseconds(rangeNS.End));
        }

        /// <summary>
        ///     Determines whether the given instances are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the instances are considered to be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(TimeRange first, TimeRange second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether the given instances are not considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the instances are not considered to be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(TimeRange first, TimeRange second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj != null) && (obj is TimeRange) && Equals((TimeRange)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(this.startTime.GetHashCode(), this.endTime.GetHashCode());
        }

        /// <inheritdoc />
        public bool Equals(TimeRange other)
        {
            return (this.startTime == other.startTime) && (this.endTime == other.endTime);
        }

        /// <summary>
        ///     Gets the duration represented by this <see cref="TimeRange"/>
        ///     as a <see cref="TimestampDelta"/>.
        /// </summary>
        public TimestampDelta Duration
        {
            get 
            { 
                return this.EndTime - this.StartTime; 
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeRange"/>
        ///     struct.
        /// </summary>
        /// <param name="startTime">
        ///     The start time of the range.
        /// </param>
        /// <param name="endTime">
        ///     The end time for the range.
        /// </param>
        public TimeRange(Timestamp startTime, Timestamp endTime)
        {
            this.startTime = startTime;
            this.endTime = endTime;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeRange"/>
        ///     struct.
        /// </summary>
        /// <param name="startTime">
        ///     The start time of the range.
        /// </param>
        /// <param name="duration">
        ///     The duration of the range.
        /// </param>
        public TimeRange(Timestamp startTime, TimestampDelta duration)
        {
            this.startTime = startTime;
            this.endTime = startTime + duration;
        }

        /// <summary>
        ///     Determines whether this instance contains the given <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="time">
        ///     The time to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="time"/> is contained within this range;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Contains(Timestamp time)
        {
            return (this.StartTime <= time) && (time <= this.EndTime);
        }

        /// <summary>
        ///     Determines whether this instance intersects with
        ///     the given instance.
        ///     <para/>
        ///     Does [this.startTime, this.endTime) intersect with [other.startTime, other.endTime)
        /// </summary>
        /// <param name="other">
        ///     The range to check against.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges intersect; <c>false</c> otherwise.
        /// </returns>
        public bool IntersectsWith(TimeRange other)
        {
            return Int64Utils.DoRangesOverlap(this.startTime.ToNanoseconds, this.endTime.ToNanoseconds, other.startTime.ToNanoseconds, other.endTime.ToNanoseconds);
        }

        // Does [this.startTime, this.endTime] intersect with [other.startTime, other.endTime]
        /// <summary>
        ///     Determines whether this instance intersects inclusively with
        ///     the given instance.
        ///     <para/>
        ///     Does [this.startTime, this.endTime] intersect with [other.startTime, other.endTime]
        /// </summary>
        /// <param name="other">
        ///     The range to check against.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges intersect; <c>false</c> otherwise.
        /// </returns>
        public bool IntersectsInclusivelyWith(TimeRange other)
        {
            return Int64Utils.DoInclusiveRangesOverlap(this.startTime.ToNanoseconds, this.endTime.ToNanoseconds, other.startTime.ToNanoseconds, other.endTime.ToNanoseconds);
        }

        /// <summary>
        ///     Gets the intersection of the two <see cref="TimeRange"/> instances.
        /// </summary>
        /// <param name="x">
        ///     The first time range.
        /// </param>
        /// <param name="y">
        ///     The second time range.
        /// </param>
        /// <returns>
        ///     The intersection, if one exists; <c>null</c> otherwise.
        /// </returns>
        public static TimeRange? Intersect(TimeRange x, TimeRange y)
        {
            Timestamp maxStartTime = Timestamp.Max(x.AsOrdered().StartTime, y.AsOrdered().StartTime);
            Timestamp minEndTime = Timestamp.Min(x.AsOrdered().EndTime, y.AsOrdered().EndTime);

            if (minEndTime < maxStartTime)
            {
                return null;
            }

            return new TimeRange(maxStartTime, minEndTime);
        }

        /// <summary>
        ///     Merges the set of <see cref="TimeRange"/> instances into the smallest
        ///     non-overlapping set.
        /// </summary>
        /// <param name="ranges">
        ///     The ranges.
        /// </param>
        /// <returns>
        ///     The smallest set of non-overlapping <see cref="TimeRange"/>s.
        /// </returns>
        public static IList<TimeRange> Union(IEnumerable<TimeRange> ranges)
        {
            TimeRange[] orderedRanges = ranges.Select(tr => tr.AsOrdered())
                                              .OrderBy(tr => tr.StartTime)
                                              .ToArray();

            List<TimeRange> rangeUnion = new List<TimeRange>();

            if (!orderedRanges.Any())
            {
                return rangeUnion;
            }

            TimeRange currentRange = orderedRanges[0];
            for (int i = 1; i < orderedRanges.Length; i++)
            {
                TimeRange newRange = orderedRanges[i];

                if (newRange.IntersectsWith(currentRange))
                {
                    currentRange.EndTime = Timestamp.Max(currentRange.EndTime, newRange.EndTime);
                }
                else
                {
                    rangeUnion.Add(currentRange);
                    currentRange = newRange;
                }
            }

            rangeUnion.Add(currentRange);

            return rangeUnion;
        }

        /// <summary>
        ///     Gets the default <see cref="TimeRange"/>.
        /// </summary>
        public static TimeRange Default
        {
            get 
            { 
                return new TimeRange(Timestamp.Zero, Timestamp.MaxValue); 
            }
        }

        /// <summary>
        ///     Gets the largest possible <see cref="TimeRange"/>.
        /// </summary>
        public static TimeRange FullRange
        {
            get 
            { 
                return new TimeRange(Timestamp.MinValue, Timestamp.MaxValue); 
            }
        }

        /// <summary>
        ///     Gets a <see cref="TimeRange"/> representing no time.
        /// </summary>
        public static TimeRange Zero
        {
            get 
            { 
                return new TimeRange(Timestamp.Zero, Timestamp.Zero); 
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(new StringBuilder()).ToString();
        }

        private StringBuilder ToString(StringBuilder sb)
        {
            return sb.Append('[').Append(this.StartTime.ToString()).Append(", ").Append(this.EndTime.ToString()).Append(']');
        }

        /// <summary>
        ///     Parses the given string into a <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="TimeRange" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="s"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.FormatException">
        ///     <paramref name="s"/> is not parseable into a <see cref="TimeRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static TimeRange Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            TimeRange result;
            if (!TryParse(s, out result))
            {
                throw new FormatException();
            }

            return result;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="result">
        ///     The parsed <see cref="TimeRange"/>; if successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if parsing was successful; <c>false</c>
        ///     otherwise.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static bool TryParse(string s, out TimeRange result)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentOutOfRangeException("s");
            }

            // "[0,0]" is the shortest string that would work.
            if (s.Length < 5)
            {
                result = TimeRange.Zero;
                return false;
            }

            int leftBracketIndex = s.IndexOf('[');
            if (leftBracketIndex == -1)
            {
                leftBracketIndex = s.IndexOf('(');
            }

            int commaIndex = s.IndexOf(',');
            int rightBracketIndex = s.IndexOf(']');
            if (rightBracketIndex == -1)
            {
                rightBracketIndex = s.IndexOf(')');
            }

            if (leftBracketIndex != 0 || commaIndex == -1 || rightBracketIndex != (s.Length - 1))
            {
                result = TimeRange.Zero;
                return false;
            }

            string beginStr = s.Substring(leftBracketIndex + 1, commaIndex - leftBracketIndex - 1);
            string endStr = s.Substring(commaIndex + 1, rightBracketIndex - commaIndex - 1);

            Timestamp begin;
            Timestamp end;

            if (!Timestamp.TryParse(beginStr, out begin) || !Timestamp.TryParse(endStr, out end))
            {
                result = TimeRange.Zero;
                return false;
            }

            result = new TimeRange(begin, end);
            return true;
        }

        /// <inheritdoc />
        string IFormatForClipboard.ToClipboardString(string format, bool includeUnits)
        {
            return TimestampFormatter.ToClipboardString(this.Duration.ToNanoseconds, format, null, includeUnits);
        }

        /// <inheritdoc />
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(TimestampDelta))
            {
                return this.Duration;
            }
            else if (conversionType == typeof(TimeRange)) // needed since Convert.ToType goes down this code-path
            {
                return this;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        /// <inheritdoc />
        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <inheritdoc />
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.Duration.ToNanoseconds;
        }

        /// <inheritdoc />
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.ToString();
        }

        /// <inheritdoc />
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this.Duration.ToNanoseconds);
        }

        /// <inheritdoc />
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this.Duration.ToNanoseconds);
        }
    }
}

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
    ///     Represents a range between two <see cref="Timestamp"/>s on a resource with an <see cref="uint"/> id.
    /// </summary>
    [TypeConverter(typeof(ResourceTimeRangeConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ResourceId}: {StartTime.ToNanoseconds}ns - {EndTime.ToNanoseconds}ns")]
    public struct ResourceTimeRange
        : IEquatable<ResourceTimeRange>,
          IFormatForClipboard,
          IConvertible
    {
        private uint resourceId;
        private TimeRange timeRange;

        /// <summary>
        ///     Gets or sets the resource id.
        /// </summary>
        public uint ResourceId
        {
            get
            {
                return this.resourceId;
            }

            set
            {
                this.resourceId = value;
            }
        }

        /// <summary>
        ///     Gets or sets the time range.
        /// </summary>
        public TimeRange TimeRange
        {
            get
            {
                return this.timeRange;
            }

            set
            {
                this.timeRange = value;
            }
        }

        /// <summary>
        ///     Gets or sets the start of the range.
        /// </summary>
        public Timestamp StartTime
        {
            get
            {
                return this.timeRange.StartTime;
            }

            set
            {
                this.timeRange.StartTime = value;
            }
        }

        /// <summary>
        ///     Gets or sets the end of the range.
        /// </summary>
        public Timestamp EndTime
        {
            get
            {
                return this.timeRange.EndTime;
            }

            set
            {
                this.timeRange.EndTime = value;
            }
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
        public static bool operator ==(ResourceTimeRange first, ResourceTimeRange second)
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
        public static bool operator !=(ResourceTimeRange first, ResourceTimeRange second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj != null) && (obj is ResourceTimeRange resourceTimeRange) && Equals(resourceTimeRange);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(this.resourceId.GetHashCode(), this.timeRange.GetHashCode());
        }

        /// <inheritdoc />
        public bool Equals(ResourceTimeRange other)
        {
            return (this.resourceId == other.resourceId) && (this.timeRange == other.timeRange);
        }

        /// <summary>
        ///     Gets the duration represented by this <see cref="ResourceTimeRange"/>
        ///     as a <see cref="TimestampDelta"/>.
        /// </summary>
        public TimestampDelta Duration
        {
            get
            {
                return this.timeRange.Duration;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceTimeRange"/>
        ///     struct.
        /// </summary>
        /// <param name="resourceId">
        ///     The resource id of the resource time range.
        /// </param>
        /// <param name="timeRange">
        ///     The time range of the resource time range.
        /// </param>
        public ResourceTimeRange(uint resourceId, TimeRange timeRange)
        {
            this.resourceId = resourceId;
            this.timeRange = timeRange;
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
            return this.timeRange.Contains(time);
        }

        /// <summary>
        ///     Gets a <see cref="ResourceTimeRange"/> representing no resource time.
        /// </summary>
        public static ResourceTimeRange Zero
        {
            get
            {
                return new ResourceTimeRange(0, TimeRange.Zero);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(new StringBuilder()).ToString();
        }

        private StringBuilder ToString(StringBuilder sb)
        {
            return sb.Append('[').Append(this.resourceId.ToString()).Append(", ").Append(this.timeRange.ToString()).Append(']');
        }

        /// <summary>
        ///     Parses the given string into a <see cref="ResourceTimeRange"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="ResourceTimeRange" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="s"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.FormatException">
        ///     <paramref name="s"/> is not parseable into a <see cref="ResourceTimeRange"/>.
        /// </exception>
        public static ResourceTimeRange Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            ResourceTimeRange result;
            if (!TryParse(s, out result))
            {
                throw new FormatException();
            }

            return result;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="ResourceTimeRange"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="result">
        ///     The parsed <see cref="ResourceTimeRange"/>; if successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if parsing was successful; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool TryParse(string s, out ResourceTimeRange result)
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
                result = ResourceTimeRange.Zero;
                return false;
            }

            int leftBracketIndex = s.IndexOf('[');
            if (leftBracketIndex == -1)
            {
                leftBracketIndex = s.IndexOf('(');
            }

            int commaIndex = s.IndexOf(',');
            int rightBracketIndex = s.LastIndexOf(']');
            if (rightBracketIndex == -1)
            {
                rightBracketIndex = s.LastIndexOf(')');
            }

            if (leftBracketIndex != 0 || commaIndex == -1 || rightBracketIndex != (s.Length - 1))
            {
                result = ResourceTimeRange.Zero;
                return false;
            }

            string beginStr = s.Substring(leftBracketIndex + 1, commaIndex - leftBracketIndex - 1);
            string endStr = s.Substring(commaIndex + 1, rightBracketIndex - commaIndex - 1);

            uint resourceId;
            TimeRange timeRange;

            if (!uint.TryParse(beginStr, out resourceId) || !TimeRange.TryParse(endStr, out timeRange))
            {
                result = ResourceTimeRange.Zero;
                return false;
            }

            result = new ResourceTimeRange(resourceId, timeRange);
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
            else if (conversionType == typeof(ResourceTimeRange)) // needed since Convert.ToType goes down this code-path
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

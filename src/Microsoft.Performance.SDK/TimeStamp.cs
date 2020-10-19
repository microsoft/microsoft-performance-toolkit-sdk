// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a timestamp. A timestamp is a point in time
    ///     relative to zero (0) in a data analysis.
    /// </summary>
    [TypeConverter(typeof(TimestampConverter))]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ToNanoseconds}ns")]
    public readonly struct Timestamp
        : IComparable<Timestamp>,
          IEquatable<Timestamp>,
          IFormattable,
          IFormatForClipboard
    {
        private static readonly object boxedZero = (object)default(Timestamp);
        private readonly long nanoseconds;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Timestamp"/>
        ///     class.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The value of the timestamp in nanoseconds.
        /// </param>
        public Timestamp(long nanoseconds)
        {
            this.nanoseconds = nanoseconds;
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of nanoseconds.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The value in nanoseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromNanoseconds(long nanoseconds)
        {
            return new Timestamp(nanoseconds);
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of microseconds.
        /// </summary>
        /// <param name="microseconds">
        ///     The value in microseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromMicroseconds(long microseconds)
        {
            return new Timestamp(microseconds * 1000);
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of milliseconds.
        /// </summary>
        /// <param name="milliseconds">
        ///     The value in milliseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromMilliseconds(long milliseconds)
        {
            return new Timestamp(milliseconds * 1000000);
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of seconds.
        /// </summary>
        /// <param name="seconds">
        ///     The value in seconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromSeconds(long seconds)
        {
            return new Timestamp(seconds * 1000000000);
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of nanoseconds.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The value in nanoseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromNanoseconds(double nanoseconds)
        {
            return new Timestamp(DoubleUtils.ClampToInt64(nanoseconds));
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of microseconds.
        /// </summary>
        /// <param name="microseconds">
        ///     The value in microseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromMicroseconds(double microseconds)
        {
            return new Timestamp(DoubleUtils.ClampToInt64(microseconds * 1000));
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of milliseconds.
        /// </summary>
        /// <param name="milliseconds">
        ///     The value in milliseconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromMilliseconds(double milliseconds)
        {
            return new Timestamp(DoubleUtils.ClampToInt64(milliseconds * 1000000));
        }

        /// <summary>
        ///     Gets a <see cref="Timestamp"/> from the given
        ///     quantity of seconds.
        /// </summary>
        /// <param name="seconds">
        ///     The value in seconds.
        /// </param>
        /// <returns>
        ///     The <see cref="Timestamp"/>.
        /// </returns>
        public static Timestamp FromSeconds(double seconds)
        {
            return new Timestamp(DoubleUtils.ClampToInt64(seconds * 1000000000));
        }

        /// <summary>
        ///     Gets the value of this instance in nanoseconds.
        /// </summary>
        public long ToNanoseconds
        {
            get 
            { 
                return this.nanoseconds; 
            }
        }

        /// <summary>
        ///     Gets the value of this instance in microseconds.
        /// </summary>
        public long ToMicroseconds
        {
            get 
            { 
                return this.nanoseconds / 1000; 
            }
        }

        /// <summary>
        ///     Gets the value of this instance in milliseconds.
        /// </summary>
        public long ToMilliseconds
        {
            get 
            { 
                return this.nanoseconds / 1000000; 
            }
        }

        /// <summary>
        ///     Gets the value of this instance in seconds.
        /// </summary>
        public long ToSeconds
        {
            get 
            { 
                return this.nanoseconds / 1000000000; 
            }
        }

        /// <summary>
        ///     Gets the value of this instance as a <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan ToTimeSpan
        {
            get
            {
                return new TimeSpan(this.nanoseconds / 100);
            }
        }

        /// <summary>
        ///     Gets a <see cref="TimestampDelta"/> representing the absolute
        ///     value of the given <see cref="TimestampDelta"/>.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <returns>
        ///     The absolute value of <paramref name="value"/>.
        /// </returns>
        public static Timestamp Abs(Timestamp value)
        {
            return Timestamp.FromNanoseconds(Math.Abs(value.nanoseconds));
        }

        /// <summary>
        ///     Gets the minimum of two <see cref="TimestampDelta"/>
        ///     instances.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     The instance that is considered to be the minimum of the two.
        /// </returns>
        public static Timestamp Min(Timestamp first, Timestamp second)
        {
            return new Timestamp(Math.Min(first.ToNanoseconds, second.ToNanoseconds));
        }

        /// <summary>
        ///     Gets the maximum of two <see cref="TimestampDelta"/>
        ///     instances.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     The instance that is considered to be the maximum of the two.
        /// </returns>
        public static Timestamp Max(Timestamp first, Timestamp second)
        {
            return new Timestamp(Math.Max(first.ToNanoseconds, second.ToNanoseconds));
        }

        /// <summary>
        ///     Clamps the given instance to a value in the range specified.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="min">
        ///     The minimum allowable value.
        /// </param>
        /// <param name="max">
        ///     The maximum allowable value.
        /// </param>
        /// <returns>
        ///     The value clamped to an acceptable value within the specified range.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="max"/> is less than <paramref name="min"/>.
        /// </exception>
        public static Timestamp Clamp(Timestamp value, Timestamp min, Timestamp max)
        {
            return new Timestamp(Int64Utils.Clamp(value.ToNanoseconds, min.ToNanoseconds, max.ToNanoseconds));
        }

        /// <summary>
        ///     Clamps the given instance to a value in the range specified.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="minMax">
        ///     The range to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to an acceptable value within the specified range.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     The end time is less than the start time.
        /// </exception>
        public static Timestamp Clamp(Timestamp value, TimeRange minMax)
        {
            return Clamp(value, minMax.StartTime, minMax.EndTime);
        }

        /// <summary>
        ///     Clamps the given instance to a value in the range specified.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="extreme1">
        ///     One of the extremes of the range.
        /// </param>
        /// <param name="extreme2">
        ///     The other extreme of the range.
        /// </param>
        /// <returns>
        ///     The value clamped to an acceptable value within the specified range.
        /// </returns>
        public static Timestamp SafeClamp(Timestamp value, Timestamp extreme1, Timestamp extreme2)
        {
            return new Timestamp(Int64Utils.SafeClamp(value.ToNanoseconds, extreme1.ToNanoseconds, extreme2.ToNanoseconds));
        }

        /// <summary>
        ///     Clamps the given instance to a value in the range specified.
        /// </summary>
        /// <param name="value">
        ///     The value to clamp.
        /// </param>
        /// <param name="extremes">
        ///     The range to clamp.
        /// </param>
        /// <returns>
        ///     The value clamped to an acceptable value within the specified range.
        /// </returns>
        public static Timestamp SafeClamp(Timestamp value, TimeRange extremes)
        {
            return SafeClamp(value, extremes.StartTime, extremes.EndTime);
        }

        /// <summary>
        ///     Determines whether the given value has been clamped into the
        ///     specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="min">
        ///     The minimum value of the clamp.
        /// </param>
        /// <param name="max">
        ///     The maximum value of the clamp.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value has been clamped; <c>false</c> otherwise.
        /// </returns>
        public static bool IsClamped(Timestamp value, Timestamp min, Timestamp max)
        {
            return Int64Utils.IsClamped(value.ToNanoseconds, min.ToNanoseconds, max.ToNanoseconds);
        }

        /// <summary>
        ///     Determines whether the given value has been clamped into the
        ///     specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="minMax">
        ///     The range of the clamp.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value has been clamped; <c>false</c> otherwise.
        /// </returns>
        public static bool IsClamped(Timestamp value, TimeRange minMax)
        {
            return IsClamped(value, minMax.StartTime, minMax.EndTime);
        }

        /// <summary>
        ///     Determines whether the given value has been clamped into the
        ///     specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="extreme1">
        ///     One extreme of the clamp.
        /// </param>
        /// <param name="extreme2">
        ///     The other extreme of the clamp.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value has been clamped; <c>false</c> otherwise.
        /// </returns>
        public static bool SafeIsClamped(Timestamp value, Timestamp extreme1, Timestamp extreme2)
        {
            return Int64Utils.SafeIsClamped(value.ToNanoseconds, extreme1.ToNanoseconds, extreme2.ToNanoseconds);
        }

        /// <summary>
        ///     Determines whether the given value has been clamped into the
        ///     specified range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <param name="extremes">
        ///     The extremes of the clamp.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the value has been clamped; <c>false</c> otherwise.
        /// </returns>
        public static bool SafeIsClamped(Timestamp value, TimeRange extremes)
        {
            return SafeIsClamped(value, extremes.StartTime, extremes.EndTime);
        }

        /// <summary>
        ///     Gets the instance that represents a timestamp of zero (0) nanoseconds.
        /// </summary>
        public static Timestamp Zero
        {
            get 
            { 
                return default(Timestamp); 
            }
        }

        /// <summary>
        ///     Gets the instance representing the minimum representable
        ///     value.
        /// </summary>
        public static Timestamp MinValue
        {
            get 
            { 
                return new Timestamp(long.MinValue); 
            }
        }

        /// <summary>
        ///     Gets the instance representing the maximum representable
        ///     value.
        /// </summary>
        public static Timestamp MaxValue
        {
            get 
            { 
                return new Timestamp(long.MaxValue); 
            }
        }

        /// <summary>
        ///     Gets the instance representing the minimum representable
        ///     value. This is exists for backwards compatibility and
        ///     is not to be used.
        /// </summary>
        public static Timestamp LegacyMin
        {
            get
            {
                return new Timestamp(0);
            }
        }

        /// <summary>
        ///     Gets the instance representing the maximum representable
        ///     value. This is exists for backwards compatibility and
        ///     is not to be used.
        /// </summary>
        public static Timestamp LegacyMax
        {
            get
            {
                return new Timestamp(long.MaxValue);
            }
        }

        /// <summary>
        ///     Represents the largest possible timestamp valid for
        ///     the end of a data stream. This is exists for backwards
        ///     compatibility and is not to be used.
        /// </summary>
        public static Timestamp LegacyEnd
        {
            get
            {
                return new Timestamp(long.MaxValue - 1);
            }
        }

        /// <summary>
        ///     Gets the instance that represents a delta of zero (0) nanoseconds,
        ///     already boxed.
        /// </summary>
        public static object BoxedZero
        {
            get 
            { 
                return boxedZero; 
            }
        }

        /// <inheritdoc />
        public int CompareTo(Timestamp other)
        {
            // The inliner is not inlining this.nanoseconds.CompareTo method.
            return (this.nanoseconds < other.nanoseconds) ? -1 :
                   (this.nanoseconds > other.nanoseconds) ? 1 :
                   0;
        }

        /// <inheritdoc />
        public bool Equals(Timestamp other)
        {
            return this.nanoseconds == other.nanoseconds;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Timestamp"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(Timestamp first, Timestamp second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Timestamp"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(Timestamp first, Timestamp second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Timestamp"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(Timestamp first, Timestamp second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="Timestamp"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(Timestamp first, Timestamp second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="Timestamp"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(Timestamp first, Timestamp second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="Timestamp"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Timestamp"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Timestamp"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(Timestamp first, Timestamp second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is Timestamp) && Equals((Timestamp)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.nanoseconds.GetHashCode();
        }

        /// <summary>
        ///     Subtracts two quantities of <see cref="Timestamp"/> from
        ///     each other.
        /// </summary>
        /// <param name="first">
        ///     The minuend.
        /// </param>
        /// <param name="second">
        ///     The subtrahend.
        /// </param>
        /// <returns>
        ///     The result of the subtraction of <paramref name="second"/> from
        ///     <paramref name="first"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static TimestampDelta operator -(Timestamp first, Timestamp second)
        {
            return new TimestampDelta(first.nanoseconds - second.nanoseconds);
        }

        /// <summary>
        ///     Adds the given delta to the given timestamp.
        /// </summary>
        /// <param name="time">
        ///     The first addend.
        /// </param>
        /// <param name="duration">
        ///     The second addend.
        /// </param>
        /// <returns>
        ///     The result of the addition of <paramref name="time"/> with
        ///     <paramref name="duration"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Timestamp operator +(Timestamp time, TimestampDelta duration)
        {
            return new Timestamp(time.nanoseconds + duration.ToNanoseconds);
        }

        /// <summary>
        ///     Subtracts the given delta from the given timestamp.
        /// </summary>
        /// <param name="time">
        ///     The minuend.
        /// </param>
        /// <param name="duration">
        ///     The subtrahend.
        /// </param>
        /// <returns>
        ///     The result of the subtraction of <paramref name="duration"/>
        ///     from <paramref name="time"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static Timestamp operator -(Timestamp time, TimestampDelta duration)
        {
            return new Timestamp(time.nanoseconds - duration.ToNanoseconds);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <inheritdoc />
        public string ToString(string format)
        {
            return this.ToString(format, null);
        }

        /// <inheritdoc />
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return TimestampFormatter.ToString(this.nanoseconds, format, formatProvider);
        }

        /// <inheritdoc />
        public string ToClipboardString(string format, bool includeUnits)
        {
            return TimestampFormatter.ToClipboardString(this.nanoseconds, format, null, includeUnits);
        }

        /// <summary>
        ///     Gets the string representation of this
        ///     instance using the specified units.
        /// </summary>
        /// <param name="units">
        ///     The units.
        /// </param>
        /// <returns>
        ///     The string representation.
        /// </returns>
        public string ToString2(TimeUnit units)
        {
            return TimestampFormatter.ToString(this.nanoseconds, null, units, true, false);
        }

        /// <summary>
        ///     Parses the given string into a <see cref="Timestamp" />,
        ///     assuming nanoseconds if no units are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="Timestamp"/>
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The given string is not parseable into a <see cref="Timestamp"/>.
        /// </exception>
        public static Timestamp Parse(string s)
        {
            return Parse(s, TimeUnit.Nanoseconds);
        }

        /// <summary>
        ///     Parses the given string into a <see cref="Timestamp" />,
        ///     assuming the given units if no units are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if no units are present in <paramref name="s"/>.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="Timestamp"/>
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The given string is not parseable into a <see cref="Timestamp"/>.
        /// </exception>
        /// <exception cref="System.OverflowException">
        ///     The string represents a value not representable by a
        ///     <see cref="Timestamp"/>.
        /// </exception>
        public static Timestamp Parse(string s, TimeUnit defaultUnits)
        {
            return Timestamp.FromNanoseconds(TimestampFormatter.ParseToNanoseconds(s, defaultUnits));
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="Timestamp"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="result">
        ///     The result of parsing, if successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="Timestamp"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, out Timestamp result)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, out resultNS);
            result = Timestamp.FromNanoseconds(resultNS);
            return success;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="Timestamp"/>,
        ///     assuming the given units if none are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if none are specified in <paramref name="s"/>.
        /// </param>
        /// <param name="result">
        ///     The result of parsing, if successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="Timestamp"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out Timestamp result)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, defaultUnits, out resultNS);
            result = Timestamp.FromNanoseconds(resultNS);
            return success;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="Timestamp"/>,
        ///     assuming the given units if none are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if none are specified in <paramref name="s"/>.
        /// </param>
        /// <param name="result">
        ///     The result of parsing, if successful.
        /// </param>
        /// <param name="parsedUnits">
        ///     The units parsed.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="Timestamp"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out Timestamp result, out TimeUnit parsedUnits)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, defaultUnits, out resultNS, out parsedUnits);
            result = Timestamp.FromNanoseconds(resultNS);
            return success;
        }
    }
}

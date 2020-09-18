// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a delta between two <see cref="Timestamp"/>
    ///     instances.
    /// </summary>
    [TypeConverter(typeof(TimestampDeltaConverter))]
    [DebuggerDisplay("{" + nameof(ToNanoseconds) + "}ns")]
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct TimestampDelta
        : IComparable<TimestampDelta>,
          IEquatable<TimestampDelta>,
          IComparable<TimeRange>,
          IEquatable<TimeRange>,
          IFormattable,
          IFormatForClipboard
    {
        private static readonly object boxedZero = (object)default(TimestampDelta);
        private readonly long nanoseconds;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimestampDelta"/>
        ///     class.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The difference in nanoseconds between the timestamps.
        /// </param>
        public TimestampDelta(long nanoseconds)
        {
            this.nanoseconds = nanoseconds;
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of nanoseconds.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The number of nanoseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromNanoseconds(long nanoseconds)
        {
            return new TimestampDelta(nanoseconds);
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of microseconds.
        /// </summary>
        /// <param name="microseconds">
        ///     The number of microseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromMicroseconds(long microseconds)
        {
            return new TimestampDelta(microseconds * 1000);
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of milliseconds.
        /// </summary>
        /// <param name="milliseconds">
        ///     The number of milliseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromMilliseconds(long milliseconds)
        {
            return new TimestampDelta(milliseconds * 1000000);
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of seconds.
        /// </summary>
        /// <param name="seconds">
        ///     The number of seconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromSeconds(long seconds)
        {
            return new TimestampDelta(seconds * 1000000000);
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of nanoseconds.
        /// </summary>
        /// <param name="nanoseconds">
        ///     The number of nanoseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromNanoseconds(double nanoseconds)
        {
            return new TimestampDelta(DoubleUtils.ClampToInt64(nanoseconds));
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of microseconds.
        /// </summary>
        /// <param name="microseconds">
        ///     The number of microseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromMicroseconds(double microseconds)
        {
            return new TimestampDelta(DoubleUtils.ClampToInt64(microseconds * 1000));
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of milliseconds.
        /// </summary>
        /// <param name="milliseconds">
        ///     The number of milliseconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromMilliseconds(double milliseconds)
        {
            return new TimestampDelta(DoubleUtils.ClampToInt64(milliseconds * 1000000));
        }

        /// <summary>
        ///     Creates a new <see cref="TimestampDelta"/> from the given
        ///     count of seconds.
        /// </summary>
        /// <param name="seconds">
        ///     The number of seconds.
        /// </param>
        /// <returns>
        ///     A new <see cref="TimestampDelta"/>.
        /// </returns>
        public static TimestampDelta FromSeconds(double seconds)
        {
            return new TimestampDelta(DoubleUtils.ClampToInt64(seconds * 1000000000));
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
        public static TimestampDelta Abs(TimestampDelta value)
        {
            return new TimestampDelta(Math.Abs(value.nanoseconds));
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
        public static TimestampDelta Min(TimestampDelta first, TimestampDelta second)
        {
            return new TimestampDelta(Math.Min(first.ToNanoseconds, second.ToNanoseconds));
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
        public static TimestampDelta Max(TimestampDelta first, TimestampDelta second)
        {
            return new TimestampDelta(Math.Max(first.ToNanoseconds, second.ToNanoseconds));
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
        public static TimestampDelta Clamp(TimestampDelta value, TimestampDelta min, TimestampDelta max)
        {
            return new TimestampDelta(Int64Utils.Clamp(value.ToNanoseconds, min.ToNanoseconds, max.ToNanoseconds));
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
        public static TimestampDelta SafeClamp(TimestampDelta value, TimestampDelta extreme1, TimestampDelta extreme2)
        {
            return new TimestampDelta(Int64Utils.SafeClamp(value.ToNanoseconds, extreme1.ToNanoseconds, extreme2.ToNanoseconds));
        }

        /// <summary>
        ///     Gets the instance that represents a delta of zero (0) nanoseconds.
        /// </summary>
        public static TimestampDelta Zero
        {
            get 
            { 
                return default(TimestampDelta); 
            }
        }

        /// <summary>
        ///     Gets the instance representing the minimum representable
        ///     delta.
        /// </summary>
        public static TimestampDelta MinValue
        {
            get 
            { 
                return new TimestampDelta(long.MinValue); 
            }
        }

        /// <summary>
        ///     Gets the instance representing the maximum representable
        ///     delta.
        /// </summary>
        public static TimestampDelta MaxValue
        {
            get 
            { 
                return new TimestampDelta(long.MaxValue); 
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
        public int CompareTo(TimeRange other)
        {
            return this.CompareTo(other.Duration);
        }

        /// <inheritdoc />
        public int CompareTo(TimestampDelta other)
        {
            return (this.nanoseconds < other.nanoseconds) ? -1 : (this.nanoseconds > other.nanoseconds) ? 1 : 0;
        }

        /// <inheritdoc />
        public bool Equals(TimeRange other)
        {
            return this.Equals(other.Duration);
        }

        /// <inheritdoc />
        public bool Equals(TimestampDelta other)
        {
            return this.nanoseconds == other.nanoseconds;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="TimestampDelta"/>
        ///     is strictly less than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly less than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <(TimestampDelta first, TimestampDelta second)
        {
            return first.CompareTo(second) < 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="TimestampDelta"/>
        ///     is strictly greater than another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be strictly greater than <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >(TimestampDelta first, TimestampDelta second)
        {
            return first.CompareTo(second) > 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="TimestampDelta"/>
        ///     is less than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be less than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator <=(TimestampDelta first, TimestampDelta second)
        {
            return first.CompareTo(second) <= 0;
        }

        /// <summary>
        ///     Determines whether one instance of an <see cref="TimestampDelta"/>
        ///     is greater than or equal to another instance.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be greater than or equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator >=(TimestampDelta first, TimestampDelta second)
        {
            return first.CompareTo(second) >= 0;
        }

        /// <summary>
        ///     Determines whether two <see cref="TimestampDelta"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(TimestampDelta first, TimestampDelta second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="TimestampDelta"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="TimestampDelta"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(TimestampDelta first, TimestampDelta second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return (other is TimestampDelta) && Equals((TimestampDelta)other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.nanoseconds.GetHashCode();
        }

        /// <summary>
        ///     Subtracts two quantities of <see cref="TimestampDelta"/> from
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
        public static TimestampDelta operator -(TimestampDelta first, TimestampDelta second)
        {
            return new TimestampDelta(first.nanoseconds - second.nanoseconds);
        }

        /// <summary>
        ///     Adds two quantities of <see cref="TimestampDelta"/> to
        ///     each other.
        /// </summary>
        /// <param name="first">
        ///     The first addend.
        /// </param>
        /// <param name="second">
        ///     The second addend.
        /// </param>
        /// <returns>
        ///     The result of the addition of <paramref name="first"/> with
        ///     <paramref name="second"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static TimestampDelta operator +(TimestampDelta first, TimestampDelta second)
        {
            return new TimestampDelta(first.nanoseconds + second.nanoseconds);
        }

        /// <summary>
        ///     Multiplies the given <see cref="TimestampDelta"/> by the given
        ///     quantity.
        /// </summary>
        /// <param name="value">
        ///     The multiplicand.
        /// </param>
        /// <param name="factor">
        ///     The multiplier.
        /// </param>
        /// <returns>
        ///     The product of <paramref name="value"/> multiplied by the <paramref name="factor"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static TimestampDelta operator *(TimestampDelta value, double factor)
        {
            return new TimestampDelta((long)(value.nanoseconds * factor));
        }

        /// <summary>
        ///     Multiplies the given <see cref="TimestampDelta"/> by the given
        ///     quantity.
        /// </summary>
        /// <param name="factor">
        ///     The multiplicand.
        /// </param>
        /// <param name="value">
        ///     The multiplier.
        /// </param>
        /// <returns>
        ///     The product of <paramref name="value"/> multiplied by the <paramref name="factor"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static TimestampDelta operator *(double factor, TimestampDelta value)
        {
            return new TimestampDelta((long)(factor * value.nanoseconds));
        }

        /// <summary>
        ///     Gets the unary negation of the given <see cref="TimestampDelta"/>.
        /// </summary>
        /// <param name="value">
        ///     The value to negate.
        /// </param>
        /// <returns>
        ///     The negation of <paramref name="value"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static TimestampDelta operator -(TimestampDelta value)
        {
            return new TimestampDelta(-value.nanoseconds);
        }

        /// <summary>
        ///     Divides this instance by another instance, rounding the result
        ///     of the division up.
        /// </summary>
        /// <param name="other">
        ///     The divisor.
        /// </param>
        /// <returns>
        ///     The quotient as a <see cref="long"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RoundUp")]
        public long DivideRoundUp(TimestampDelta other)
        {
            return (this.nanoseconds + other.nanoseconds - 1) / other.nanoseconds;
        }

        /// <summary>
        ///     Divides this instance by another instance, rounding the result
        ///     of the division up.
        /// </summary>
        /// <param name="other">
        ///     The divisor.
        /// </param>
        /// <returns>
        ///     The quotient as a <see cref="double"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RoundUp")]
        public double DivideRoundUpAsDouble(TimestampDelta other)
        {
            return
                (other.nanoseconds >= 0)
                    ? (this.nanoseconds >= 0)
                        ? ((double)(this.nanoseconds + other.nanoseconds - 1) / other.nanoseconds)
                        : -(-this).DivideRoundUp(other)
                    : -DivideRoundUp(-other);
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
        ///     Parses the given string into a <see cref="TimestampDelta" />,
        ///     assuming nanoseconds if no units are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="TimestampDelta"/>
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The given string is not parseable into a <see cref="TimestampDelta"/>.
        /// </exception>
        public static TimestampDelta Parse(string s)
        {
            return Parse(s, TimeUnit.Nanoseconds);
        }

        /// <summary>
        ///     Parses the given string into a <see cref="TimestampDelta" />,
        ///     assuming the given units if no units are specified in the string.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="defaultUnits">
        ///     The units to assume if no units are present in <paramref name="s"/>.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="TimestampDelta"/>
        /// </returns>
        /// <exception cref="System.FormatException">
        ///     The given string is not parseable into a <see cref="TimestampDelta"/>.
        /// </exception>
        /// <exception cref="System.OverflowException">
        ///     The string represents a value not representable by a
        ///     <see cref="TimestampDelta"/>.
        /// </exception>
        public static TimestampDelta Parse(string s, TimeUnit defaultUnits)
        {
            return TimestampDelta.FromNanoseconds(TimestampFormatter.ParseToNanoseconds(s, defaultUnits));
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="TimestampDelta"/>.
        /// </summary>
        /// <param name="s">
        ///     The string to parse.
        /// </param>
        /// <param name="result">
        ///     The result of parsing, if successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="TimestampDelta"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, out TimestampDelta result)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, out resultNS);
            result = TimestampDelta.FromNanoseconds(resultNS);
            return success;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="TimestampDelta"/>,
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
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="TimestampDelta"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out TimestampDelta result)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, defaultUnits, out resultNS);
            result = TimestampDelta.FromNanoseconds(resultNS);
            return success;
        }

        /// <summary>
        ///     Attempts to parse the given string into a <see cref="TimestampDelta"/>,
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
        ///     <c>true</c> if <paramref name="s"/> can be parsed into a <see cref="TimestampDelta"/>
        ///     instance; <c>false</c> otherwise.
        /// </returns>
        public static bool TryParse(string s, TimeUnit defaultUnits, out TimestampDelta result, out TimeUnit parsedUnits)
        {
            long resultNS;
            bool success = TimestampFormatter.TryParse(s, defaultUnits, out resultNS, out parsedUnits);
            result = TimestampDelta.FromNanoseconds(resultNS);
            return success;
        }
    }
}

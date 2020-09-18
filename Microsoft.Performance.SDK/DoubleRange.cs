// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a range of values using <see cref="double"/>s.
    ///     The range is taken to be [begin, end)
    /// </summary>
    [TypeConverter(typeof(DoubleRangeConverter))]
    public struct DoubleRange
        : IEquatable<DoubleRange>
    {
        private double begin;
        private double end;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoubleRange"/>
        ///     class spanning the given interval.
        /// </summary>
        /// <param name="begin">
        ///     The beginning of the range.
        /// </param>
        /// <param name="end">
        ///     The end of the range.
        /// </param>
        public DoubleRange(double begin, double end)
        {
            this.begin = Math.Min(begin, end);
            this.end = Math.Max(begin, end);
        }

        /// <summary>
        ///     Gets the empty range.
        /// </summary>
        public static DoubleRange Zero
        {
            get
            {
                return new DoubleRange(0, 0);
            }
        }
        
        /// <summary>
        ///     Gets or sets the beginning of this range.
        /// </summary>
        public double Begin
        {
            get
            {
                return Math.Min(this.begin, this.end);
            }

            set
            {
                this.begin = value;
            }
        }

        /// <summary>
        ///     Gets or sets the end of this range.
        /// </summary>
        public double End
        {
            get
            {
                return Math.Max(this.begin, this.end);
            }

            set
            {
                this.end = value;
            }
        }

        /// <summary>
        ///     Gets the length of this range. The length
        ///     is the difference between <see cref="End"/> and
        ///     <see cref="Begin"/>.
        /// </summary>
        public double Length
        {
            get
            {
                return this.End - this.Begin;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this range has a
        ///     length. An infinite range does not have a length.
        ///     A range with <see cref="Begin"/> equal to <see cref="End"/>
        ///     does not have a length.
        /// </summary>
        public bool HasLength
        {
            get
            {
                return IsFinite && (this.Begin != this.End);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is finite.
        /// </summary>
        public bool IsFinite
        {
            get
            {
                return this.begin.IsFinite() && this.end.IsFinite();
            }
        }

        /// <summary>
        ///     Determines if two <see cref="DoubleRange"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered to be
        ///     equal to <paramref name="second"/>; <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(DoubleRange first, DoubleRange second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines if two <see cref="DoubleRange"/> instances
        ///     are considered to *not* be equal.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered not to be
        ///     equal to <paramref name="second"/>; <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(DoubleRange first, DoubleRange second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        ///     Explicitly casts a <see cref="DoubleRange"/> to an
        ///     <see cref="Int64Range"/>.
        /// </summary>
        /// <param name="range">
        ///     The <see cref="DoubleRange"/> to cast.
        /// </param>
        public static explicit operator Int64Range(DoubleRange range)
        {
            return new Int64Range((int)range.Begin, (int)range.End);
        }

        /// <summary>
        ///     Gets the intersection of the two ranges, it one exists.
        /// </summary>
        /// <param name="first">
        ///     The first range.
        /// </param>
        /// <param name="second">
        ///     The second range.
        /// </param>
        /// <returns>
        ///     The range of intersection between <paramref name="first"/>
        ///     and <paramref name="second"/>. If no intersection exists,
        ///     the <c>null</c> is returned.
        /// </returns>
        public static DoubleRange? Intersect(DoubleRange first, DoubleRange second)
        {
            if (!first.IsFinite || !second.IsFinite)
            {
                return null;
            }

            double maxBegin = Math.Max(first.Begin, second.Begin);
            double minEnd = Math.Min(first.End, second.End);

            if (maxBegin > minEnd)
            {
                return null;
            }

            DoubleRange intersection = new DoubleRange(maxBegin, minEnd);

            return intersection;
        }

        /// <summary>
        ///     Performs and exclusive or of the two <see cref="DoubleRange"/>s.
        ///     The exclusive or of two ranges is the collection of all ranges that
        ///     1) are contained in one range but not the other and
        ///     2) when the union is taken, the result is equal to the union of the
        ///        original ranges.
        /// </summary>
        /// <param name="first">
        ///     The first instance.
        /// </param>
        /// <param name="second">
        ///     The second instance.
        /// </param>
        /// <returns>
        ///     A collection of <see cref="DoubleRange"/> instances that represent
        ///     the exclusive or of the two ranges.
        /// </returns>
        public static DoubleRange[] Xor(DoubleRange first, DoubleRange second)
        {
            if (!first.IsFinite || !second.IsFinite)
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, "cannot accept ranges with non-finite values. first={0}, second={1}", first, second));
            }

            DoubleRange[] result;

            if (first == second)
            {
                result = EmptyArray<DoubleRange>.Instance;
            }
            else if (!first.HasLength && !second.HasLength)
            {
                result = EmptyArray<DoubleRange>.Instance;
            }
            else if (!first.HasLength)
            {
                result = new DoubleRange[1] { second };
            }
            else if (!second.HasLength)
            {
                result = new DoubleRange[1] { first };
            }
            else if (first.Begin == second.Begin && first.End != second.End)
            {
                // A and B have the same start value, but different end values
                // A:       [----------------)
                // B:       [-----------)
                // A xor B:             [----)
                result = new DoubleRange[1]
                {
                    new DoubleRange(Math.Min(first.End, second.End), Math.Max(first.End, second.End))
                };
            }
            else if (first.Begin != second.Begin && first.End == second.End)
            {
                // A and B have different start values, but the same end value
                // A:       [----------------)
                // B:            [-----------)
                // A xor B: [----)
                result = new DoubleRange[1]
                {
                    new DoubleRange(Math.Min(first.Begin, second.Begin), Math.Max(first.Begin, second.Begin))
                };
            }
            else if (first.Begin <= second.Begin && first.End >= second.End)
            {
                // A contains B
                // A:       [----------------)
                // B:            [------)
                // A xor B: [----)      [----)
                result = new DoubleRange[2]
                {
                    new DoubleRange(first.Begin, second.Begin),
                    new DoubleRange(second.End, first.End)
                };
            }
            else if (second.Begin <= first.Begin && second.End >= first.End)
            {
                // B contains A
                // A:            [------)
                // B:       [----------------)
                // A xor B: [----)      [----)
                result = new DoubleRange[2]
                {
                    new DoubleRange(second.Begin, first.Begin),
                    new DoubleRange(first.End, second.End)
                };
            }
            else if ((first.Begin <= second.Begin && second.Begin < first.End && first.End <= second.End) ||
                     (second.Begin <= first.Begin && first.Begin < second.End && second.End <= first.End))
            {
                // A overlaps B, aka B overlaps A
                // A:       [----------)      or       [---------)
                // B:            [---------)  or  [----------)      
                // A xor B: [----)     [---)  or  [----)     [---)
                result = new DoubleRange[2]
                {
                    new DoubleRange(Math.Min(first.Begin, second.Begin), Math.Max(first.Begin, second.Begin)),
                    new DoubleRange(Math.Min(first.End, second.End), Math.Max(first.End, second.End))
                };
            }
            else
            {
                // disjoint time ranges
                // A:       [------)
                // B:                  [------)
                // A xor B: [------)   [------)
                result = new DoubleRange[2]
                {
                    new DoubleRange(first.Begin, first.End),
                    new DoubleRange(second.Begin, second.End)
                };
            }

            return result;
        }

        /// <summary>
        ///     Parses the given <see cref="string"/> into a <see cref="DoubleRange"/>
        ///     instance. A string representation of a range is "[x, y)" where x and y
        ///     are doubles.
        /// </summary>
        /// <param name="s">
        ///     The <see cref="string"/> to parse.
        /// </param>
        /// <returns>
        ///     The parsed <see cref="DoubleRange"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="s"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.FormatException">
        ///     <paramref name="s"/> is not parseable into a <see cref="DoubleRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static DoubleRange Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            DoubleRange doubleRange;
            if (!TryParse(s, out doubleRange))
            {
                throw new FormatException();
            }

            return doubleRange;
        }

        /// <summary>
        ///     Tries to parse the given <see cref="string"/> into a <see cref="DoubleRange"/>
        ///     instance. A string representation of a range is "[x, y)" where x and y
        ///     are doubles.
        /// </summary>
        /// <param name="s">
        ///     The <see cref="string"/> to parse.
        /// </param>
        /// <param name="result">
        ///     When <paramref name="s"/> is able to be parsed, this contains the result
        ///     of parsing into a <see cref="DoubleRange"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="s"/> is parsed into a <see cref="DoubleRange"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static bool TryParse(string s, out DoubleRange result)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                result = DoubleRange.Zero;
                return false;
            }

            // "[0,0)" is the shortest string that would work.
            if (s.Length < 5)
            {
                result = DoubleRange.Zero;
                return false;
            }

            int leftBracketIndex = s.IndexOf('[');
            int commaIndex = s.IndexOf(',');
            int rightBracketIndex = s.IndexOf(')');

            if (leftBracketIndex != 0 || commaIndex == -1 || rightBracketIndex != (s.Length - 1))
            {
                result = DoubleRange.Zero;
                return false;
            }

            string beginStr = s.Substring(leftBracketIndex + 1, commaIndex - leftBracketIndex - 1);
            string endStr = s.Substring(commaIndex + 1, rightBracketIndex - commaIndex - 1);

            double begin;
            double end;

            // TODO: need to specify CultureInfo.InvariantCulture here, and/or make ToString() and Parse() work regardless of culture (can't always use comma separator)
            if (!Double.TryParse(beginStr, out begin) || !Double.TryParse(endStr, out end))
            {
                result = DoubleRange.Zero;
                return false;
            }

            result = new DoubleRange(begin, end);
            return true;
        }

        /// <summary>
        ///     Determines whether this range overlaps with the given range.
        ///     See <see cref="DoubleUtils.DoRangesOverlap"/> for more information.
        /// </summary>
        /// <param name="other">
        ///     The range to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance overlaps with <paramref name="other"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Overlaps(DoubleRange other)
        {
            return DoubleUtils.DoRangesOverlap(this.begin, this.end, other.begin, other.end);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCodeUtils.CombineHashCodeValues(
                this.Begin.GetHashCode(),
                this.End.GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj != null) && (obj is DoubleRange) && Equals((DoubleRange)obj);
        }

        /// <inheritdoc />
        public bool Equals(DoubleRange other)
        {
            return this.Begin == other.Begin && this.End == other.End;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            const string format = "[{0}, {1})";
            string beginStr = this.Begin.ToString("R", CultureInfo.InvariantCulture);
            string endStr = this.End.ToString("R", CultureInfo.InvariantCulture);
            string toString = string.Format(CultureInfo.InvariantCulture, format, beginStr, endStr);
            return toString;
        }
    }
}

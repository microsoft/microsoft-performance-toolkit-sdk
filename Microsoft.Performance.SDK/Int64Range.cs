// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a range of values using <see cref="Int64"/>.
    /// </summary>
    public readonly struct Int64Range
        : IEquatable<Int64Range>
    {
        private readonly long begin;
        private readonly long end;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Int64Range"/>
        ///     class.
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        public Int64Range(long begin, long end)
        {
            this.begin = Math.Min(begin, end);
            this.end = Math.Max(begin, end);
        }

        /// <summary>
        ///     Gets the beginning of the range.
        /// </summary>
        public long Begin
        {
            get
            {
                return Math.Min(this.begin, this.end);
            }
        }

        /// <summary>
        ///     Gets the end of the range.
        /// </summary>
        public long End
        {
            get
            {
                return Math.Max(this.begin, this.end);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this range has a
        ///     length.
        /// </summary>
        public bool HasLength
        {
            get
            {
                return Length != 0;
            }
        }

        /// <summary>
        ///     Gets the length of this range. The length
        ///     is the difference between <see cref="End"/> and
        ///     <see cref="Begin"/>.
        /// </summary>
        public long Length
        {
            get
            {
                return this.End - this.Begin;
            }
        }

        /// <summary>
        ///     Determines whether the two <see cref="Int64Range"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first range.
        /// </param>
        /// <param name="second">
        ///     The second range.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges are considered to be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(Int64Range first, Int64Range second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether the two <see cref="Int64Range"/> instances
        ///     are not considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first range.
        /// </param>
        /// <param name="second">
        ///     The second range.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the ranges are not considered to be equal;
        ///     <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(Int64Range first, Int64Range second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        ///     Implicitly casts the given <see cref="Int64Range"/> to
        ///     a <see cref="DoubleRange"/>.
        /// </summary>
        /// <param name="range">
        ///     The range to cast.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator DoubleRange(Int64Range range)
        {
            return new DoubleRange(range.Begin, range.End);
        }

        /// <summary>
        ///     Determines whether this range overlaps with the given range.
        ///     See <see cref="Int64Utils.DoRangesOverlap"/> for more information.
        /// </summary>
        /// <param name="other">
        ///     The range to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance overlaps with <paramref name="other"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Overlaps(Int64Range other)
        {
            return Int64Utils.DoRangesOverlap(this.begin, this.end, other.begin, other.end);
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
            return (obj != null) && (obj is Int64Range) && Equals((Int64Range)obj);
        }

        /// <inheritdoc />
        public bool Equals(Int64Range other)
        {
            // Note that we treat two ranges as equal even if the begin and end fields are
            // swapped (which is possible if someone uses unsafe memory manipulation).
            // (in other words, if this.begin == other.end && this.end == other.begin)
            return this.Begin == other.Begin && this.End == other.End;
        }
    }
}

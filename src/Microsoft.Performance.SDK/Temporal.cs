// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a temporal interval.
    /// </summary>
    public readonly struct Temporal
        : IEquatable<Temporal>
    {
        private readonly TimeRange timeRange;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Temporal"/>
        ///     struct.
        /// </summary>
        /// <param name="startTime">
        ///     The start time.
        /// </param>
        /// <param name="endTime">
        ///     The end time.
        /// </param>
        public Temporal(Timestamp startTime, Timestamp endTime)
            : this(new TimeRange(startTime, endTime))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Temporal"/>
        ///     struct from the given time range.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        public Temporal(TimeRange timeRange)
        {
            this.timeRange = timeRange;
        }

        /// <summary>
        ///     Gets the default value of <see cref="Temporal"/>s.
        /// </summary>
        public static Temporal Default
        {
            get { return new Temporal(TimeRange.Default); }
        }

        /// <summary>
        ///     Gets the start time.
        /// </summary>
        public Timestamp StartTime
        {
            get { return this.timeRange.StartTime; }
        }

        /// <summary>
        ///     Gets the end time.
        /// </summary>
        public Timestamp EndTime
        {
            get { return this.timeRange.EndTime; }
        }

        /// <summary>
        ///     Gets the range encompassed by this instance.
        /// </summary>
        public TimeRange TimeRange
        {
            get { return this.timeRange; }
        }

        /// <summary>
        ///     Determines whether this instance encompasses
        ///     the given time.
        /// </summary>
        /// <param name="time">
        ///     The time.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this instance contains <paramref name="time"/>;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool Contains(Timestamp time)
        {
            return this.timeRange.Contains(time);
        }

        /// <summary>
        ///     Explicitly casts a <see cref="TimeRange"/> to a <see cref="Temporal"/>.
        /// </summary>
        /// <param name="timeRange">
        ///     The range to cast.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static explicit operator Temporal(TimeRange timeRange)
        {
            return new Temporal(timeRange);
        }

        /// <summary>
        ///     Implicitly casts a <see cref="Temporal"/> to a <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="temporal">
        ///     The temporal to cast.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator TimeRange(Temporal temporal)
        {
            return temporal.timeRange;
        }

        /// <summary>
        ///     Determines whether two <see cref="Temporal"/> instances
        ///     are considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Temporal"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Temporal"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator ==(Temporal first, Temporal second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two <see cref="Temporal"/> instances
        ///     are *not* considered to be equal.
        /// </summary>
        /// <param name="first">
        ///     The first <see cref="Temporal"/> to compare.
        /// </param>
        /// <param name="second">
        ///     The second <see cref="Temporal"/> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="first"/> is not considered
        ///     to be equal to <paramref name="second"/>; <c>false</c>
        ///     otherwise.
        /// </returns>
        public static bool operator !=(Temporal first, Temporal second)
        {
            return !first.Equals(second);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj != null) && (obj is Temporal) && Equals((Temporal)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.timeRange.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(Temporal other)
        {
            return this.timeRange == other.timeRange;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides information about a data source, including the
    ///     time range encompassed by the data source.
    /// </summary>
    public sealed class DataSourceInfo
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Gets the instance representing default <see cref="DataSourceInfo"/>.
        /// </summary>
        public static readonly DataSourceInfo Default = new DataSourceInfo(0, long.MaxValue, Epoch);

        /// <summary>
        ///     Gets the instance representing no <see cref="DataSourceInfo"/>.
        /// </summary>
        public static readonly DataSourceInfo None = new DataSourceInfo(0, 0, Epoch);

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceInfo"/>
        ///     class.
        ///     <para />
        ///     The timestamps are relative to the data source, and are not actual
        ///     times (e.g. UTC).
        /// </summary>
        /// <param name="firstEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, at which data in the source begins.
        /// </param>
        /// <param name="lastEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, at which data in the source ends.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="firstEventTimestampNanoseconds"/> is less than zero (0.)
        ///     - or -
        ///     <paramref name="lastEventTimestampNanoseconds"/> is less than
        ///     <paramref name="firstEventTimestampNanoseconds"/>.
        /// </exception>
        [Obsolete("Wall clock is be required. Please use the constructor that takes a wall clock.", true)]
        public DataSourceInfo(
            long firstEventTimestampNanoseconds,
            long lastEventTimestampNanoseconds)
            : this(firstEventTimestampNanoseconds, lastEventTimestampNanoseconds, Epoch)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSourceInfo"/>
        ///     class.
        ///     <para />
        ///     The timestamps are relative to the beginning of the wallclock
        ///     time spanned by this data source, and are not actual times (e.g. UTC).
        ///     For more information, see remarks below.
        /// </summary>
        /// <param name="firstEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, of the first data point, relative to
        ///     the beginning of the wallclock time spanned by this data source.
        /// </param>
        /// <param name="lastEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, of the last data point, relative to
        ///     the beginning of the wallclock time spanned by this data source.
        /// </param>
        /// <param name="firstEventWallClockUtc">
        ///     The UTC wallclock time of the first data point, i.e. the absolute time
        ///     of the data point referred to in <paramref name="firstEventTimestampNanoseconds"/>.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="firstEventTimestampNanoseconds"/> is less than zero (0.)
        ///     - or -
        ///     <paramref name="lastEventTimestampNanoseconds"/> is less than
        ///     <paramref name="firstEventTimestampNanoseconds"/>.
        /// </exception>
        /// <remarks>
        ///     Timestamps are relative to the beginning of the wallclock time spanned
        ///     by this data source. "The beginning of the time spanned by this data
        ///     source" is not explicitly passed to this class as a parameter since it
        ///     can be inferred with <paramref name="firstEventTimestampNanoseconds"/> and
        ///     <paramref name="firstEventWallClockUtc"/>.
        ///     <para />
        ///     For example, consider the following scenario:
        ///         - A data source begins collecting information at 9:00:00.000 UTC
        ///           (9am UTC)
        ///         - The first even the data source observes occurs at 9:00:05.000 UTC
        ///           (5 seconds later)
        ///     In this scenario, <paramref name="firstEventWallClockUtc"/> should be
        ///     9:00:05.000 UTC and <paramref name="firstEventTimestampNanoseconds"/>
        ///     should be 5x10^9 (i.e. 5 seconds in nanoseconds). These two pieces of
        ///     information encode that the first event happened at 9:00:05.000,
        ///     5 seconds after the data source begins, which must be 9:00:00.000.
        ///     <para />
        ///     If a data source's first even coincides with the start of the data source,
        ///     <paramref name="firstEventTimestampNanoseconds"/> will always be 0.
        /// </remarks>
        public DataSourceInfo(
            long firstEventTimestampNanoseconds,
            long lastEventTimestampNanoseconds,
            DateTime firstEventWallClockUtc)
        {
            Guard.GreaterThanOrEqualTo(firstEventTimestampNanoseconds, 0, nameof(firstEventTimestampNanoseconds));
            Guard.GreaterThanOrEqualTo(lastEventTimestampNanoseconds, firstEventTimestampNanoseconds, nameof(firstEventTimestampNanoseconds));

            if (firstEventWallClockUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException($"{nameof(firstEventWallClockUtc)} time must be UTC", nameof(firstEventWallClockUtc));
            }

            this.FirstEventTimestampNanoseconds = firstEventTimestampNanoseconds;
            this.EndTimestampNanoseconds = lastEventTimestampNanoseconds;
            this.FirstEventWallClockUtc = firstEventWallClockUtc;
            this.Errors = new List<ErrorInfo>();
        }

        /// <summary>
        ///     Gets the timestamp of the first event in nanoseconds, relative to
        ///     the beginning of the wallclock time spanned by this data source.
        /// </summary>
        public long FirstEventTimestampNanoseconds { get; }

        /// <summary>
        ///     Gets the timestamp of the last event in nanoseconds, relative to
        ///     the beginning of the wallclock time spanned by this data source.
        /// </summary>
        public long EndTimestampNanoseconds { get; }

        /// <summary>
        ///     Gets the wallclock time of the first event in the data stream, in UTC.
        /// </summary>
        public DateTime FirstEventWallClockUtc { get; }

        /// <summary>
        ///     Gets the collection of errors encountered while processing the
        ///     data source.
        /// </summary>
        public IList<ErrorInfo> Errors { get; }

        /// <summary>
        ///     Gets the wallclock time of the beginning of the time spanned by
        ///     this data source, in UTC.
        /// </summary>
        public DateTime StartWallClockUtc
        {
            get
            {
                return this.FirstEventWallClockUtc.AddTicks(-this.FirstEventTimestampNanoseconds / 100);
            }
        }

        /// <summary>
        ///     Gets the wallclock time of the end of the time spanned by
        ///     this data source, in UTC.
        /// </summary>
        public DateTime EndWallClockUtc
        {
            get
            {
                return this.FirstEventWallClockUtc.AddTicks(this.EndTimestampNanoseconds / 100);
            }
        }
    }
}

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
        ///     The timestamps are relative to the data source, and are not actual
        ///     times (e.g. UTC).
        /// </summary>
        /// <param name="firstEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, at which data in the source begins.
        /// </param>
        /// <param name="lastEventTimestampNanoseconds">
        ///     The timestamp, in nanoseconds, at which data in the source ends.
        /// </param>
        /// <param name="firstEventWallClockUtc">
        ///     The start time of the data first data point, in real time.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     <paramref name="firstEventTimestampNanoseconds"/> is less than zero (0.)
        ///     - or -
        ///     <paramref name="lastEventTimestampNanoseconds"/> is less than
        ///     <paramref name="firstEventTimestampNanoseconds"/>.
        /// </exception>
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
        ///     Gets the timestamp of the first event in nanoseconds, relative to the start
        ///     of the data stream.
        /// </summary>
        public long FirstEventTimestampNanoseconds { get; }

        /// <summary>
        ///     Gets the timestamp of the last event in nanoseconds, relative to the start
        ///     of the data stream.
        /// </summary>
        public long EndTimestampNanoseconds { get; }

        /// <summary>
        ///     Gets the real time of the first event in the data stream, in UTC.
        /// </summary>
        public DateTime FirstEventWallClockUtc { get; }

        /// <summary>
        ///     Gets the collection of errors encountered while processing the
        ///     data source.
        /// </summary>
        public IList<ErrorInfo> Errors { get; }

        /// <summary>
        ///     Gets the real time of the start of the data stream, in UTC
        /// </summary>
        public DateTime StartWallClockUtc
        {
            get
            {
                return this.FirstEventWallClockUtc.AddTicks(-this.FirstEventTimestampNanoseconds / 100);
            }
        }

        /// <summary>
        ///     Gets the real time of the end of the data stream, in UTC
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

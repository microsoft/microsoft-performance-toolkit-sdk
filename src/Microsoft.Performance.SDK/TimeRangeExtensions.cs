// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for dealing
    ///     with <see cref="TimeRange"/> instances.
    /// </summary>
    public static class TimeRangeExtensions
    {
        /// <summary>
        ///     Attempts to get a range that spans all of the time ranges
        ///     in the given collection.
        /// </summary>
        /// <param name="sortedTimeRanges">
        ///     A sorted collection of timeranges to span.
        /// </param>
        /// <returns>
        ///     A spanning timerange, if one exists; <c>null</c> otherwise.
        /// </returns>
        public static TimeRange? TryGetSpanningRange(this IList<TimeRange> sortedTimeRanges)
        {
            if (sortedTimeRanges == null)
            {
                return null;
            }

            if (sortedTimeRanges.Count == 0)
            {
                return null;
            }

            return new TimeRange(sortedTimeRanges[0].StartTime, sortedTimeRanges[sortedTimeRanges.Count - 1].EndTime);
        }

        /// <summary>
        ///     Gets the minimum time in the <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        /// <returns>
        ///     The minimum value.
        /// </returns>
        public static Timestamp MinTime(this TimeRange timeRange)
        {
            return Timestamp.Min(timeRange.StartTime, timeRange.EndTime);
        }

        /// <summary>
        ///     Gets the maximum time in the <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        /// <returns>
        ///     The maximum value.
        /// </returns>
        public static Timestamp MaxTime(this TimeRange timeRange)
        {
            return Timestamp.Max(timeRange.StartTime, timeRange.EndTime);
        }

        /// <summary>
        ///     Gets the absolute value of the duration of the given time range.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        /// <returns>
        ///     The absolute value of the duration of the timerange.
        /// </returns>
        public static TimestampDelta AbsDuration(this TimeRange timeRange)
        {
            return (timeRange.MaxTime() - timeRange.MinTime());
        }

        /// <summary>
        ///     Gets the given <see cref="TimeRange"/> as an ordered
        ///     <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        /// <returns>
        ///     The ordered timerange.
        /// </returns>
        public static TimeRange AsOrdered(this TimeRange timeRange)
        {
            return new TimeRange(timeRange.MinTime(), timeRange.MaxTime());
        }

        /// <summary>
        ///     Gets the given <see cref="TimeRange"/> as an ordered
        ///     <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="timeRange">
        ///     The time range.
        /// </param>
        /// <returns>
        ///     The ordered timerange.
        /// </returns>
        public static TimeRange? AsOrdered(this TimeRange? timeRange)
        {
            if (timeRange.HasValue)
            {
                return timeRange.Value.AsOrdered();
            }
            else
            {
                return null;
            }
        }
    }
}

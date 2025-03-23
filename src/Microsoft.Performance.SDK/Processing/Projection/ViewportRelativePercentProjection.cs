// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Helper class containing useful methods for generating projections.
        /// </summary>
        public static class VisibleDomainRelativePercent
        {
            /// <summary>
            ///     Creates a projection that returns the percentage of the visible time consumed by this
            ///     <see cref="TimestampDelta"/>.
            /// </summary>
            /// <param name="timestampDeltaColumn">
            ///     Timestamp delta.
            /// </param>
            /// <returns>
            ///     Percent of time consumed by the given delta in a given visible domain.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, TimestampDelta> timestampDeltaColumn)
            {
                Guard.NotNull(timestampDeltaColumn, nameof(timestampDeltaColumn));

                var aggregateRowsInVisibleDomainColumn
                    = Projection.AggregateInVisibleDomain<TimestampDelta, TimestampDelta>(timestampDeltaColumn);

                var columnRelativeToVisibleDomain
                    = Percent.Create(timestampDeltaColumn, aggregateRowsInVisibleDomainColumn);

                return columnRelativeToVisibleDomain;
            }

            /// <summary>
            ///     Creates a projection that returns the percentage of the visible time consumed by this
            ///     <see cref="TimeRange"/>.
            /// </summary>
            /// <param name="timeRangeColumn">
            ///     Time range.
            /// </param>
            /// <returns>
            ///     Percent of time consumed by the given time range in a given visible domain.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, TimeRange> timeRangeColumn)
            {
                Guard.NotNull(timeRangeColumn, nameof(timeRangeColumn));

                var aggregateRowsInVisibleDomainColumn
                    = Projection.AggregateInVisibleDomain<TimeRange, TimestampDelta>(timeRangeColumn);

                var columnRelativeToVisibleDomain
                    = Percent.Create(timeRangeColumn.Compose(timeRange => timeRange.Duration), aggregateRowsInVisibleDomainColumn);

                return columnRelativeToVisibleDomain;
            }

            /// <summary>
            ///     Creates a projection that returns the percentage of the visible time consumed by this
            ///     <see cref="ResourceTimeRange"/>.
            /// </summary>
            /// <param name="resourceTimeRangeColumn">
            ///     Resource time range.
            /// </param>
            /// <returns>
            ///     Percent of time consumed by the given resource time range in a given visible domain.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, ResourceTimeRange> resourceTimeRangeColumn)
            {
                Guard.NotNull(resourceTimeRangeColumn, nameof(resourceTimeRangeColumn));

                var aggregateRowsInVisibleDomainColumn
                    = Projection.AggregateInVisibleDomain<ResourceTimeRange, TimestampDelta>(resourceTimeRangeColumn);

                var columnRelativeToVisibleDomain
                    = Percent.Create(resourceTimeRangeColumn.Compose(resourceTimeRange => resourceTimeRange.Duration), aggregateRowsInVisibleDomainColumn);

                return columnRelativeToVisibleDomain;
            }

            /// <summary>
            ///     Creates a projection that maps a percentage relative to an entire domain to a percentage
            ///     relative to the current visible domain.
            /// </summary>
            /// <param name="percentColumn">
            ///     Percentage projection returning a double.
            /// </param>
            /// <returns>
            ///     Percentage relative to a given visible domain.
            /// </returns>
            public static IProjection<int, double> Create(IProjection<int, double> percentColumn)
            {
                Guard.NotNull(percentColumn, nameof(percentColumn));

                var percentColumnAggregate
                    = Projection.AggregateInVisibleDomain<double, double>(percentColumn);

                var columnRelativeToVisibleDomain
                    = Percent.Create(percentColumn, percentColumnAggregate);

                return columnRelativeToVisibleDomain;
            }
        }
    }
}

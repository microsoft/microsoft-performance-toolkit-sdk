// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Helper class containing useful methods for generating projections.
        /// </summary>
        public static class ViewportRelativePercent
        {
            /// <summary>
            ///     Creates a projection that returns the percentage of the viewport time aggregation consumed by this
            ///     <see cref="TimestampDelta"/>.
            /// </summary>
            /// <param name="timestampDeltaColumn">
            ///     Timestamp delta.
            /// </param>
            /// <returns>
            ///     Percent of time consumed by delta in the given viewport projection.
            /// </returns>
            public static IProjection<int, double> Create(
                IProjection<int, TimestampDelta> timestampDeltaColumn)
            {
                Guard.NotNull(timestampDeltaColumn, nameof(timestampDeltaColumn));

                var aggregateRowsInViewportColumn 
                    = Projection.AggregateInViewport<TimestampDelta, TimestampDelta>(timestampDeltaColumn);

                var columnRelativeToViewport 
                    =  Percent.Create(timestampDeltaColumn, aggregateRowsInViewportColumn);

                return columnRelativeToViewport;
            }

            /// <summary>
            ///     Creates a projection that returns the percentage of the viewport time aggregation is consumed by this
            ///     <see cref="double"/>.
            /// </summary>
            /// <param name="percentColumn">
            ///     Percentage projection returning a double.
            /// </param>
            /// <returns>
            ///     Percent of value consumed by delta in the given viewport projection.
            /// </returns>
            public static IProjection<int, double> Create(IProjection<int, double> percentColumn)
            {
                Guard.NotNull(percentColumn, nameof(percentColumn));

                var percentColumnAggregate 
                    = Projection.AggregateInViewport<double, double>(percentColumn);

                var columnRelativeToViewport 
                    = Percent.Create(percentColumn, percentColumnAggregate);

                return columnRelativeToViewport;
            }
        }
    }
}

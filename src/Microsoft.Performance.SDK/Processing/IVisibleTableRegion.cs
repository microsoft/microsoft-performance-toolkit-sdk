// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the portion of the table that is currently
    ///     viewable in the application.
    /// </summary>
    public interface IVisibleTableRegion
    {
        /// <summary>
        ///     Gets the index of the first row that is currently 
        ///     visible in the UI for the table.
        /// </summary>
        int TableRowStart { get; }

        /// <summary>
        ///     Gets the count of rows that are currently visible
        ///     in the UI for the table.
        /// </summary>
        int TableRowCount { get; }

        /// <summary>
        ///     Gets the currently selected viewport for the table.
        /// </summary>
        TimeRange Viewport { get; }

        /// <summary>
        ///     Aggregates the given projection using the specified aggregation
        ///     mode, aggregating only those values that are currently visible.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="System.Type"/> of result from the projection.
        /// </typeparam>
        /// <typeparam name="TAggregate">
        ///     The <see cref="System.Type"/> of the aggregated result.
        /// </typeparam>
        /// <param name="projection">
        ///     The projection whose results are to be aggregated.
        /// </param>
        /// <param name="aggregationMode">
        ///     The way in which to aggregate the data in the projection.
        /// </param>
        /// <returns>
        ///     The aggregated result.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="projection"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        ///     <paramref name="aggregationMode"/> is <see cref="AggregationMode.None"/>
        ///     - or -
        ///     <paramref name="aggregationMode"/> is not a valid member
        ///     of the <see cref="AggregationMode"/> enumeration.
        ///     - or -
        ///     <paramref name="aggregationMode"/> is <see cref="AggregationMode.None"/>.
        /// </exception>
        TAggregate AggregateRowsInViewport<T, TAggregate>(
            IProjection<int, T> projection,
            AggregationMode aggregationMode);
    }
}

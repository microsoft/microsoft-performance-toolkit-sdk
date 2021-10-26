// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Defines the domain region that is currently visible in
    ///     the SDK driver.
    /// </summary>
    /// <remarks>
    ///     Currently, only <seealso cref="TimeRange"/>s are supported domains.
    /// </remarks>
    public interface IVisibleDomainRegion
    {
        /// <summary>
        ///     Gets the currently visible domain in the SDK driver.
        /// </summary>
        /// <remarks>
        ///     Currently, only <seealso cref="TimeRange"/>s are supported domains.
        /// </remarks>
        TimeRange Domain { get; }

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
        TAggregate AggregateVisibleRows<T, TAggregate>(
            IProjection<int, T> projection,
            AggregationMode aggregationMode);
    }
}

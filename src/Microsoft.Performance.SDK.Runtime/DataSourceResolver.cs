// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     Resolves the <see cref="ProcessingSourceReference" />s that can
    ///     process a given set of <see cref="IDataSource"/>s.
    /// </summary>
    public static class DataSourceResolver
    {
        /// <summary>
        ///     Given a collection of <see cref="IDataSource"/>s and
        ///     <see cref="ProcessingSourceReference"/>s, creates a mapping
        ///     that assigns each <see cref="IDataSource"/> to one or more
        ///     <see cref="ProcessingSourceReference"/>s on the basis of what 
        ///     can be processed by each Custom Data Source.
        ///     <para />
        ///     Each <see cref="ProcessingSourceReference"/> in <paramref name="customDataSources"/>
        ///     will be a key in the returned mapping. The value for any given key represents
        ///     the <see cref="IDataSource"/>s that can be processed by the given 
        ///     <see cref="ProcessingSourceReference"/>/. If no <see cref="IDataSource"/>s
        ///     can be processed by a given <see cref="ProcessingSourceReference"/>, then the
        ///     value will be an empty collection.
        /// </summary>
        /// <param name="dataSources">
        ///     The <see cref="IDataSource"/>s to be mapped to the <see cref="ProcessingSourceReference"/>s
        ///     that are able to process them. Any duplicate values in this parameter will be
        ///     ignored. Any <c>null</c> elements in this parameter will be ignored.
        /// </param>
        /// <param name="customDataSources">
        ///     The collection of <see cref="ProcessingSourceReference"/>s that are potentially
        ///     eligible to process the elements of <paramref name="dataSources"/>.
        ///     Any duplicate values in this parameter will be ignored. Any <c>null</c> elements
        ///     in this parameter will be ignored.
        /// </param>
        /// <returns>
        ///     A mapping of <see cref="ProcessingSourceReference"/>s to collections of zero (0) 
        ///     or more <see cref="IDataSource"/> that can be processed by said 
        ///     <see cref="ProcessingSourceReference"/>s.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSources"/> is <c>null</c>.
        /// </exception>
        public static IDictionary<ProcessingSourceReference, IEnumerable<IDataSource>> Assign(
            IEnumerable<IDataSource> dataSources,
            IEnumerable<ProcessingSourceReference> customDataSources)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(customDataSources, nameof(customDataSources));

            var dedupedDs = dataSources.Where(x => x is object).Distinct().ToList();
            var dedupedCds = customDataSources.Where(x => x is object).Distinct().ToList();

            bool isSupported(ProcessingSourceReference cdsr, IDataSource dataSource)
            {
                return cdsr.DataSources?.Any(
                    x => dataSource.GetType().Is(x.Type) &&
                         x.Accepts(dataSource) &&
                         cdsr.Supports(dataSource)) ?? false;
            }

            return dedupedCds.ToDictionary(
                x => x,
                x => dedupedDs.Where(ds => isSupported(x, ds)).ToList().AsEnumerable());
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     A default <see cref="IDataSourceGrouper"/> that places each <see cref="IDataSource"/> in its own group
    ///     with the <see cref="DefaultProcessingMode"/>.
    /// </summary>
    public class DisjointGroupsDataSourceGrouper
        : IDataSourceGrouper
    {
        /// <summary>
        ///     Returns a 1-to-1 mapping of <paramref name="dataSources"/> items to <see cref="IDataSourceGroup"/>s. Each
        ///     <see cref="IDataSource"/> is placed in its own <see cref="IDataSourceGroup"/>, indicating no two instances
        ///     can be processed together.
        /// </summary>
        /// <param name="dataSources">
        ///     The <see cref="IDataSource"/> instances to be grouped.
        /// </param>
        /// <param name="options">
        ///     Ignored by this implementation.
        /// </param>
        /// <returns>
        ///     A <see cref="IDataSourceGroup"/> for each item in <paramref name="dataSources"/>.
        /// </returns>
        public IReadOnlyCollection<IDataSourceGroup> Group(IEnumerable<IDataSource> dataSources, ProcessorOptions options)
        {
            return dataSources
                .Select(dataSource => new DataSourceGroup(new IDataSource[] { dataSource }.ToList(), new DefaultProcessingMode()))
                .ToList();
        }
    }
}
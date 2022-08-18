// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.DataSourceGrouping;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     Responsible for grouping disparate <see cref="IDataSource"/> instances into <see cref="IDataSourceGroup"/>
    ///     instances.
    /// </summary>
    public interface IDataSourceGrouper
    {
        /// <summary>
        ///     Groups the specified <paramref name="dataSources" /> into groups that each can be processed by a
        ///     <see cref="ICustomDataProcessor" /> as a single, combined <see cref="IDataSource" />.
        ///     <para />
        ///     The returned value must include every valid combination of <see cref="IDataSource" />s in the given
        ///     <paramref name="dataSources" />.
        /// </summary>
        /// <param name="dataSources">
        ///     The <see cref="IDataSource"/> instances to be grouped.
        /// </param>
        /// <returns>
        ///     All valid groups of <see cref="IDataSource"/>s.
        /// </returns>
        IReadOnlyCollection<IDataSourceGroup> Group(IEnumerable<IDataSource> dataSources);
    }
}
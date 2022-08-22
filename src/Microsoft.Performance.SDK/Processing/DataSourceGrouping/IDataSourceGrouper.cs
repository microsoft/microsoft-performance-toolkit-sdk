﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     An (optional) interface that <see cref="IProcessingSource"/> instances can implement to specify how
    ///     its supported <see cref="IDataSource"/>s are capable of being grouped for processing.
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
        ///     The <see cref="IDataSource"/> instances to be grouped. This will be every opened <see cref="IDataSource"/>
        ///     that the implementing <see cref="IProcessingSource"/> specified it can open.
        /// </param>
        /// <param name="options">
        ///     Options that will be passed to the implementing <see cref="IProcessingSource"/> for processing. This
        ///     can be used, for instance, to limit the types of <see cref="IDataSourceGroup"/>s that will be returned.
        /// </param>
        /// <returns>
        ///     All valid groups of <see cref="IDataSource"/>s given the specified <paramref name="options"/>.
        /// </returns>
        IReadOnlyCollection<IDataSourceGroup> Group(IEnumerable<IDataSource> dataSources, ProcessorOptions options);
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <inheritdoc/>
    public sealed class DataSourceGroup
        : IDataSourceGroup
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="DataSourceGroup"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The <see cref="IDataSource"/>s in this group. This parameter may not be <c>null</c>.
        /// </param>
        /// <param name="processingMode">
        ///     The <see cref="IProcessingMode"/> that this group should be processed in. This parameter may not
        ///     be <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> or <paramref name="processingMode"/> is <c>null</c>.
        /// </exception>
        public DataSourceGroup(IReadOnlyCollection<IDataSource> dataSources, IProcessingMode processingMode)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processingMode, nameof(processingMode));

            DataSources = dataSources;
            ProcessingMode = processingMode;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IDataSource> DataSources { get; }
        
        /// <inheritdoc/>
        public IProcessingMode ProcessingMode { get; }
    }
}
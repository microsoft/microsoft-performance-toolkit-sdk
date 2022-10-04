// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <inheritdoc/>
    public sealed class DataSourceGroup
        : IDataSourceGroup
    {
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
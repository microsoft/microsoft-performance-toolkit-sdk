// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     A collection of <see cref="IDataSource"/>s that a <see cref="ICustomDataProcessor"/> can process together
    ///     in a specified <see cref="IProcessingMode"/>.
    /// </summary>
    public interface IDataSourceGroup
    {
        /// <summary>
        ///     Gets the <see cref="IDataSource"/>s in this group.
        /// </summary>
        IReadOnlyCollection<IDataSource> DataSources { get; }
        
        /// <summary>
        ///     Gets the <see cref="IProcessingMode"/> for this group.
        /// </summary>
        IProcessingMode ProcessingMode { get; }
    }
}
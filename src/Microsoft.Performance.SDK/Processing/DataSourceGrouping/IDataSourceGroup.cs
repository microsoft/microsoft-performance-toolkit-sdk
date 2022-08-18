// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     A <see cref="IDataSource"/> collection that a <see cref="ICustomDataProcessor"/> can process together as
    ///     a single, combined <see cref="IDataSource"/> in the specified <see cref="ProcessingMode"/>.
    /// </summary>
    public interface IDataSourceGroup
    {
        /// <summary>
        ///     Gets the <see cref="IDataSource"/>s in this group.
        /// </summary>
        IReadOnlyCollection<IDataSource> DataSources { get; }
        
        /// <summary>
        ///     
        /// </summary>
        IProcessingMode ProcessingMode { get; }
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing.DataSourceGrouping
{
    /// <summary>
    ///     Responsible for choosing the <see cref="IDataSourceGroup"/> instances that will be used for processing.
    /// </summary>
    public interface IDataSourceGroupResolver
    {
        /// <summary>
        ///     Selects which <see cref="IDataSourceGroup"/>s to use for processing from <paramref name="allValidGroups"/>.
        ///     Each returned <see cref="IDataSourceGroup"/> will be processed independently by a unique
        ///     <see cref="ICustomDataProcessor"/>.
        /// </summary>
        /// <param name="processingSourceGuid">
        ///     The <see cref="Guid"/> of the <see cref="ProcessingSource"/> to resolve groups for.
        /// </param>
        /// <param name="allValidGroups">
        ///     All <see cref="IDataSourceGroup"/>s that can be processed by the <see cref="ICustomDataProcessor"/>
        ///     created by the <see cref="ProcessingSource"/> with the specified <paramref name="processingSourceGuid"/>.
        /// </param>
        /// <returns>
        ///     The <see cref="IDataSourceGroup"/>s to use for processing. Each returned <see cref="IDataSourceGroup"/>
        ///     MUST be an instance from <paramref name="allValidGroups"/>.
        /// </returns>
        IReadOnlyCollection<IDataSourceGroup> Resolve(Guid processingSourceGuid, IReadOnlyCollection<IDataSourceGroup> allValidGroups);
    }
}
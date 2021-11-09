// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides an interface for providing tables that are exposed
    ///     by <see cref="ProcessingSource"/>s.
    /// </summary>
    public interface IProcessingSourceTableProvider
    {
        /// <summary>
        ///     Returns the collection of tables that should be associated with an <see cref="IProcessingSource"/>.
        ///     <para/>
        ///     See also <seealso cref="ProcessingSource.ProcessingSource(IProcessingSourceTableProvider)"/>.
        /// </summary>
        /// <param name="tableConfigSerializer">
        ///     The serializer used to deserialize table configurations.
        /// </param>
        /// <returns>
        ///     A collection of tables. Each <see cref="DiscoveredTable"/> in the collection must have
        ///     a unique <see cref="TableDescriptor"/>.
        /// </returns>
        IEnumerable<TableDescriptor> Discover(ITableConfigurationsSerializer tableConfigSerializer);
    }
}

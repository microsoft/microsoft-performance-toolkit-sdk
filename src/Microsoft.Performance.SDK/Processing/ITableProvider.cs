// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Provides an interface for providing tables that are exposed
    ///     by <see cref="ProcessingSource"/>s.
    /// </summary>
    public interface ITableProvider
    {
        /// <summary>
        ///     Returns the collection of tables that should be associated with a Processing Source.
        /// </summary>
        /// <param name="tableConfigSerializer">
        ///     The serializer used to deserialize table configurations.
        /// </param>
        /// <returns>
        ///     A collection of tables.
        /// </returns>
        ISet<DiscoveredTable> Discover(ISerializer tableConfigSerializer);
    }
}

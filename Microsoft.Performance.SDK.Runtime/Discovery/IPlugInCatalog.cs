// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Exposes SDK plug-ins.
    /// </summary>
    public interface IPlugInCatalog
    {
        /// <summary>
        ///     Gets a value indicating whether the catalog is loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        ///     Gets an enumeration of plug-ins.
        /// </summary>
        IEnumerable<CustomDataSourceReference> PlugIns { get; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Exposes SDK plug-ins.
    /// </summary>
    public interface IPlugInCatalog
        : IDisposable
    {
        /// <summary>
        ///     Gets a value indicating whether the catalog is loaded.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        bool IsLoaded { get; }

        /// <summary>
        ///     Gets an enumeration of plug-ins.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception> 
        IEnumerable<CustomDataSourceReference> PlugIns { get; }
    }
}

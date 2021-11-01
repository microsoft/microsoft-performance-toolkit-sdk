// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Discovery
{
    /// <summary>
    ///     Exposes SDK processing sources.
    /// </summary>
    public interface IProcessingSourceCatalog
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
        ///     Gets an enumeration of plugins.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception> 
        IEnumerable<ProcessingSourceReference> ProcessingSources { get; }
    }
}

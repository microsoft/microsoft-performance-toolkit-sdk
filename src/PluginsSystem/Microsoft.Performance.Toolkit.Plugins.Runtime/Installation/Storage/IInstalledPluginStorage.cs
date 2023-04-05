// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a storage for installed plugins.
    /// </summary>
    public interface IInstalledPluginStorage
    {
        /// <summary>
        ///     Adds the given plugin package to the storage.
        /// </summary>
        /// <param name="package">
        ///     The plugin package to add.
        /// </param>
        /// <param name="cancellationToken">
        ///     The cancellation token.
        /// </param>
        /// <param name="progress">
        ///     The progress of the operation.
        /// </param>
        /// <returns>
        ///     The installed plugin.
        /// </returns>
        Task AddAsync(
            PluginPackage package,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}

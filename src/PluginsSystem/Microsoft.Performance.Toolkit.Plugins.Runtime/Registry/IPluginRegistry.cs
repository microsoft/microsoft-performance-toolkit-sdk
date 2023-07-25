// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a registry that stores information about installed plugins and supports registering, unregistering and updating plugins.
    ///     This registry also provides a lock that can be used to synchronize access to the registry.
    /// </summary>
    public interface IPluginRegistry
        : IKeyedRepository<InstalledPluginInfo, string>,
          ISynchronizedObject
    {
        /// <summary>
        ///     Resets the plugin registry to an empty state.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An awaitable task that completes when the registry has been reset.
        /// </returns>
        Task Reset(CancellationToken cancellationToken);
    }
}

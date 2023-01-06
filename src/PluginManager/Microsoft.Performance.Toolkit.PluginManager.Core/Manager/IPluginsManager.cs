// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Manager
{
    /// <summary>
    ///     Contains logic for discovering, installing, uninstalling and updating plugins.
    /// </summary>
    public interface IPluginsManager
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<UriPluginSource> PluginSources { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginSources"></param>
        void SetPluginSources(IEnumerable<UriPluginSource> pluginSources);

        /// <summary>
        ///     Gets all available plugins in their latest versions.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Gets all available versions of a given plugin.
        /// </summary>
        /// <param name="pluginIdentity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(PluginIdentity pluginIdentity, CancellationToken cancellationToken);
    }
}

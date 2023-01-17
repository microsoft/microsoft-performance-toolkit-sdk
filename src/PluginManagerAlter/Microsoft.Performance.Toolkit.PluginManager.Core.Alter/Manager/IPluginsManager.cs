// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Manager
{
    /// <summary>
    ///     Contains logic for discovering, installing, uninstalling and updating plugins.
    /// </summary>
    public interface IPluginsManager
    {
        /// <summary>
        ///     Gets all plugin sources managed by this plugins manager.
        /// </summary>
        IEnumerable<PluginSource> PluginSources { get; }

        /// <summary>
        ///     Adds a plugin source of type <typeparamref name="TSource"> to the plugins manager.
        /// </summary>
        /// <param name="sources">
        ///     The plugin sources to be added.
        /// /param>
        void AddPluginSources(IEnumerable<PluginSource> sources);

        /// <summary>
        ///     Clears all plugin sources in this plugin manager.
        /// </summary>
        void ClearPluginSources();

        /// <summary>
        ///     Gets all available plugins in their latest versions.
        /// </summary>
        /// <param name="cancellationToken">
        ///      A cancellation token.
        /// </param>
        /// <returns>
        ///     A collection available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///      Gets all available plugins in their latest versions from a given source.
        /// </summary>
        /// <param name="source">
        ///     The source to discover plugins from.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets all available versions of a given plugin.
        /// </summary>
        /// <param name="availablePlugin">
        ///     A discovered plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token.
        /// </param>
        /// <returns>
        ///     A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPlugin(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Loads additional <see cref="IPluginResource"/>s from <paramref name="directory"/> to this plugin manager.
        /// </summary>
        /// <param name="directory">
        ///     The directory to load resource assemblies from.
        /// </param>
        void LoadAdditionalPluginResources(string directory);
    }
}

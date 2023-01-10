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
        ///     Gets all plugin sources managed by this plugins manager.
        /// </summary>
        IEnumerable<IPluginSource> PluginSources { get; }

        /// <summary>
        ///     Add a plugin source of type <typeparamref name="TSource"> to the plugins manager.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The type of plugin source to be added.
        /// </typeparam>
        /// <param name="source">
        ///     The plugin source to be added.
        /// /param>
        void AddPluginSource<TSource>(TSource source) where TSource : class, IPluginSource;

        /// <summary>
        ///     Clear all plugin sources in this plugin manager.
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
        Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestAsync(CancellationToken cancellationToken);

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
        Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(
            IAvailablePlugin availablePlugin,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Loads additional discoverer sources from <paramref name="directory"/> to this plugin manager.
        /// </summary>
        /// <param name="directory">
        ///     The directory to load discoverer sources assemblies from.
        /// </param>
        void LoadAdditionalDiscovererSources(string directory);

    }
}

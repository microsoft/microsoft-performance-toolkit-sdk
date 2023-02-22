// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
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
        ///     Adds a collection of plugin sources to the plugins manager.
        /// </summary>
        /// <param name="sources">
        ///     The plugin sources to be added.
        /// </param>
        void AddPluginSources(IEnumerable<PluginSource> sources);

        /// <summary>
        ///     Clears all plugin sources in this plugin manager.
        /// </summary>
        void ClearPluginSources();

        /// <summary>
        ///     Gets all available plugins in their latest versions.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(CancellationToken cancellationToken);

        /// <summary>
        ///      Gets all available plugins in their latest versions from a given plugin source.
        /// </summary>
        /// <param name="source">
        ///      The source to discover plugins from.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///      A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Loads additional <see cref="IPluginManagerResource"/>s from <paramref name="directory"/> to this plugin manager.
        /// </summary>
        /// <param name="directory">
        ///     The directory to load resource assemblies from.
        /// </param>
        /// <returns>
        ///     Whether the loading was successful.
        /// </returns>
        bool LoadAdditionalPluginResources(string directory);

        /// <summary>
        ///     Gets all available versions of a plugin by discovering from all plugin sources.
        /// </summary>
        /// <param name="pluginIdentity">
        ///     The identity of the target plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets all available versions of a plugin from a particular source.
        /// </summary>
        /// <param name="source">
        ///     The plugin source to discover plugins from.
        /// </param>
        /// <param name="pluginIdentity">
        ///     The identity of the target plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginFromSourceAsync(
            PluginSource source,
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets all installed plugins
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of installed plugins.
        /// </returns>
        Task<IReadOnlyCollection<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Installs an available plugin if no other versions of this plugin installed.
        /// </summary>
        /// <param name="availablePlugin">
        ///     The available plugin to be installed.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin installation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin has been successfully installed. <c>false</c> otherwise.
        /// </returns>
        Task<bool> InstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Installs a local plugin package.
        /// </summary>
        /// <param name="pluginPackagePath">
        ///     The path to the plugin package to be installed.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin installation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin has been successfully installed. <c>false</c> otherwise.
        /// </returns>
        Task<bool> InstallLocalPluginAsync(
           string pluginPackagePath,
           CancellationToken cancellationToken,
           IProgress<int> progress);

        /// <summary>
        ///     Uninstalls a plugin.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The plugin to be uninstalled.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin uninstallation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin has been successfully uninstalled. <c>false</c> otherwise.
        /// </returns>
        Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Attempts to clean up all obsolete (unreigstered) plugin files.
        ///     This method should be called safely by plugins consumers.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An await-able <see cref="Task"/> that, upon completion, indicates the files have been cleaned up.
        /// </returns>
        Task CleanupObsoletePluginsAsync(CancellationToken cancellationToken);
    }
}

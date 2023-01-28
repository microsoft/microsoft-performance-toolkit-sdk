// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Extensibility;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Registry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Manager
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
        ///     Gets all available versions of a given plugin.
        /// </summary>
        /// <param name="availablePlugin">
        ///     A discovered plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPlugin(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Loads additional <see cref="IPluginManagerResource"/>s from <paramref name="directory"/> to this plugin manager.
        /// </summary>
        /// <param name="directory">
        ///     The directory to load resource assemblies from.
        /// </param>
        void LoadAdditionalPluginResources(string directory);

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
        ///     Installs an available plugin to a given directory.
        /// </summary>
        /// <param name="availablePlugin">
        ///     The available plugin to be installed.
        /// </param>
        /// <param name="installationDir">
        ///     The directory where the plugin will be installed.
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
            string installationDir,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Installs a local plugin package to a given directory.
        /// </summary>
        /// <param name="pluginPackagePath">
        ///     The path to the plugin package to be installed.
        /// </param>
        /// <param name="installationDir">
        ///      The directory where the plugin will be installed.
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
           string installationDir,
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
        ///     Updates the plugin to another version.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The currently installed plugin.
        /// </param>
        /// <param name="targetPlugin">
        ///     The available plugin to update to.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin uninstallation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin has been successfully updated. <c>false</c> otherwise.
        /// </returns>
        Task<bool> UpdatePluginAsync(
            InstalledPlugin installedPlugin,
            AvailablePlugin targetPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Gets metadata of an installed plugin.
        /// </summary>
        /// <param name="installedPlugin">
        ///     An installed plugin    
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     Metadata of the given plugin.
        /// </returns>
        Task<PluginMetadata> GetInstalledPluginMetadataAsync(InstalledPlugin installedPlugin, CancellationToken cancellationToken);
    }
}

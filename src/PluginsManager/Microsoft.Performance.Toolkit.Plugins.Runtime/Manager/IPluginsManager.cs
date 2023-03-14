// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <summary>
    ///     Contains logic for discovering, installing, uninstalling and updating plugin.
    /// </summary>
    /// <typeparam name="TLocalPluginLocator">
    ///     The type of the local plugin locator used by this plugins manager to install local plugins.
    ///     e.g. A plugins manager that installs a plugin package stored on the file system will use <see cref="System.IO.FileInfo"/>.
    /// </typeparam>
    public interface IPluginsManager<TLocalPluginLocator>
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
        /// <returns>
        ///     A task that completes when all plugin sources have been added.
        /// </returns>
        Task AddPluginSourcesAsync(IEnumerable<PluginSource> sources);

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
        /// <param name="pluginSource">
        ///      The source to discover plugins from.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///      A collection of available plugins.
        /// </returns>
        Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource pluginSource,
            CancellationToken cancellationToken);

        // TODO: Re-enable when we start to support loading additional resources.
        ///// <summary>
        /////     Loads additional <see cref="IPluginsManagerResource"/>s from <paramref name="directory"/> to this plugin manager.
        ///// </summary>
        ///// <param name="directory">
        /////     The directory to load resource assemblies from.
        ///// </param>
        ///// <returns>
        /////     Whether the loading was successful.
        ///// </returns>
        //bool LoadAdditionalPluginResources(string directory);

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
        /// <param name="pluginSource">
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
            PluginSource pluginSource,
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets all installed plugins
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="InstalledPluginsResults"/> instance containing all valid and invalid installed plugins.
        /// </returns>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when there is an error reading or writing to plugin registry.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     The operation was canceled.
        /// </exception>
        Task<InstalledPluginsResults> GetInstalledPluginsAsync(CancellationToken cancellationToken);

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
        ///     The <see cref="InstalledPluginInfo"/> if plugin is successfully installed. <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="availablePlugin"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="PluginFetchingException">
        ///     Throws when the plugin package cannot be fetched.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when there is an error reading or writing to plugin registry.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginPackageCreationException">
        ///     Throws when there is an error creating plugin package.
        /// </exception>
        /// <exception cref="PluginPackageExtractionException">
        ///     Throws when there is an error extracting plugin package.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was canceled.
        /// </exception>
        Task<InstalledPluginInfo> TryInstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Installs a local plugin package.
        /// </summary>
        /// <param name="localPluginLocator">
        ///     An object that locates an local plugin package.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin installation.
        /// </param>
        /// <returns>
        ///     The <see cref="InstalledPluginInfo"/> if plugin is successfully installed.. <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="localPluginLocator"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="PluginLocalLoadingException">
        ///     Throws when the plugin package cannot be loaded locally.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when there is an error reading or writing to plugin registry.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="PluginPackageCreationException">
        ///     Throws when there is an error creating plugin package.
        /// </exception>
        /// <exception cref="PluginPackageExtractionException">
        ///     Throws when there is an error extracting plugin package.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was canceled.
        /// </exception>
        Task<InstalledPluginInfo> TryInstallLocalPluginAsync(
           TLocalPluginLocator localPluginLocator,
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
        /// <returns>
        ///     <c>true</c> if the plugin has been successfully uninstalled. <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="installedPlugin"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when there is an error reading or writing to plugin registry.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was canceled.
        /// </exception>
        Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken);

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
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when there is an error reading or writing to plugin registry.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the plugin registry is in an invalid state.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was canceled.
        /// </exception>
        Task CleanupObsoletePluginsAsync(CancellationToken cancellationToken);

        /// <summary>
        ///    Raised when an error occurs interacting with a paticular <see cref="PluginSource"/>.
        ///    Subsribe to this event to handle errors related to a particular <see cref="PluginSource"/>.
        /// </summary>
        event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;
    }
}

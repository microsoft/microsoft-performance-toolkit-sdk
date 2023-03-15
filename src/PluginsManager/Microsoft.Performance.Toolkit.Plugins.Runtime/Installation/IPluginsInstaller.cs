// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents an installer that supports listing, installing, uninstalling and cleaning up plugins.
    /// </summary>
    public interface IPluginsInstaller
    {
        /// <summary>
        ///     Gets all installed plugins.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="InstalledPluginsResults"/> instance containing all valid and invalid installed plugins.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken);
        
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
        Task<InstalledPluginInfo> InstallPluginAsync(
            Stream pluginStream,
            string installationRoot,
            Uri sourceUri,
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
    }
}

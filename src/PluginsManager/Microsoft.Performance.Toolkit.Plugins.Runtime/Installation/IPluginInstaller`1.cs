// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents an installer that supports listing, installing, uninstalling and cleaning up plugins at a given location.
    /// </summary>
    /// <typeparam name="TDestination">
    ///     The type of the install location.
    /// </typeparam>
    public interface IPluginInstaller<TDestination>
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
        ///     Installs a plugin from a stream.
        /// </summary>
        /// <param name="pluginPackage">
        ///     A stream containing the plugin to install.
        /// </param>
        /// <param name="installLocation">
        ///     The location to install the plugin to.
        /// </param>
        /// <param name="sourceUri">
        ///     The URI of the <see cref="Core.Discovery.PluginSource"/> that the plugin was discovered from.
        /// </param>
        /// <param name="cancellationToken">
        ///     Indicates the progress of the installation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the installation.
        /// </param>
        /// <returns>
        ///     The <see cref="InstalledPluginInfo"/> of the installed plugin if the installation was successful. Otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///      Throws when <paramref name="pluginPackage"/> or <paramref name="installLocation"/> or
        ///      <paramref name="sourceUri"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        Task<InstalledPluginInfo> InstallPluginAsync(
            PluginPackage pluginPackage,
            TDestination installLocation,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Uninstalls a plugin.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The plugin to uninstall.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     True if the uninstallation was successful. Otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="installedPlugin"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Attempts to clean up all obsolete (unreigstered) plugin files from the given installation location.
        /// </summary>
        /// <param name="installLocation">
        ///     The installation location to clean up.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A task that completes when the cleanup is complete.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was canceled.
        /// </exception>
        Task CleanupObsoletePluginsAsync(
            TDestination installLocation,
            CancellationToken cancellationToken);
    }
}

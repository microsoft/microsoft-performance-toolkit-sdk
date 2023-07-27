// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;

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
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the installed plugins registry is corrupted, requiring <see cref="ResetInstalledPlugins"/>
        ///     to be invoked.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when the installed plugins registry cannot be read or written to.
        /// </exception>
        Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken);

        /// <summary>
        ///     Installs a plugin from a stream.
        /// </summary>
        /// <param name="pluginStream">
        ///     A stream containing the plugin to install.
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
        ///     Throws when <paramref name="pluginStream"/> or <paramref name="sourceUri"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the installed plugins registry is corrupted, requiring <see cref="ResetInstalledPlugins"/>
        ///     to be invoked.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when the installed plugins registry cannot be read or written to.
        /// </exception>
        Task<InstalledPlugin> InstallPluginAsync(
            Stream pluginStream,
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
        /// <exception cref="RepositoryCorruptedException">
        ///     Throws when the installed plugins registry is corrupted, requiring <see cref="ResetInstalledPlugins"/>
        ///     to be invoked.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when the installed plugins registry cannot be read or written to.
        /// </exception>
        Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Resets the state of installed plugins, which uninstall every formerly installed plugin.
        ///     This is necessary if the plugin installation registry becomes corrupted.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     An await-able <see cref="Task"/>.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        /// <exception cref="RepositoryDataAccessException">
        ///     Throws when the installed plugins registry cannot be reset.
        /// </exception>
        Task ResetInstalledPlugins(CancellationToken cancellationToken);
    }
}

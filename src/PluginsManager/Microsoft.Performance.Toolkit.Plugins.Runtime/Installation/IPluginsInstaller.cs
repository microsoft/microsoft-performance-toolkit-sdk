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
        ///     Throws when <paramref name="pluginPackage"/> or <paramref name="installationRoot"/> or
        ///     <paramref name="sourceUri"/> is null.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
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
        Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken);
    }
}

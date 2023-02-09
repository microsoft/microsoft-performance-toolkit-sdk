// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Concurrency;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Registry;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Installation
{
    // TODO: #254 docstrings
    public sealed class PluginInstaller
    {
        private readonly PluginRegistry pluginRegistry;

        /// <summary>
        ///     Creates an instance of a <see cref="PluginInstaller"/>.
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The <see cref="PluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        public PluginInstaller(PluginRegistry pluginRegistry)
        {
            this.pluginRegistry = pluginRegistry;
        }

        /// <summary>
        ///     Returns all plugins that are installed to the registry.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A collection of installed plugins.
        /// </returns>
        public async Task<IReadOnlyCollection<InstalledPlugin>> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                return await this.pluginRegistry.GetInstalledPlugins();
            }  
        }

        /// <summary>
        ///     Installs a plugin package to a given directory and register it to the plugin registry.
        /// </summary>
        /// <param name="pluginPackage">
        ///     The plugin package to be installed.
        /// </param>
        /// <param name="installationRoot">
        ///     The root installation directory.
        /// </param>
        /// <param name="sourceUri">
        ///     The plugin source URI.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the installation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin is successfully installed. <c>false</c> otherwise.
        /// </returns>
        public async Task<bool> InstallPluginAsync(
            PluginPackage pluginPackage,
            string installationRoot,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                // Check if any version of this plugin is already installed.
                InstalledPlugin installedPlugin = await GetInstalledPluginIfExistsAsync(pluginPackage.Id, cancellationToken);
                bool needsUninstall = false;

                if (installedPlugin != null)
                {
                    if (installedPlugin.Version.Equals(pluginPackage.Version) && await ValidateInstalledPlugin(installedPlugin))
                    {
                        return true;
                    }

                    needsUninstall = true;
                }

                string installationDir = Path.GetFullPath(Path.Combine(installationRoot, $"{pluginPackage.Id}-{pluginPackage.Version}"));
                try
                {
                    // TODO: #245 This could overwrite binaries that have been unregistered but have not been cleaned up.
                    // We need to revist this code once we have a mechanism for checking whether individual plugin are in use by plugin consumers.
                    bool success = await pluginPackage.ExtractPackageAsync(installationDir, cancellationToken, progress);
                    if (!success)
                    {
                        cleanUpExtractedFiles(installationDir);
                        return false;
                    }

                    string checksum = await HashUtils.GetDirectoryHash(installationDir);
                    var pluginToInstall = new InstalledPlugin(
                        pluginPackage.Id,
                        pluginPackage.Version,
                        sourceUri,
                        pluginPackage.DisplayName,
                        pluginPackage.Description,
                        installationDir,
                        DateTime.UtcNow,
                        checksum);

                    if (needsUninstall)
                    {
                        return await this.pluginRegistry.UpdatePluginAync(installedPlugin, pluginToInstall);
                    }
                    else
                    {
                        return await this.pluginRegistry.RegisterPluginAsync(pluginToInstall);
                    }
                }
                catch (Exception e)
                {
                    cleanUpExtractedFiles(installationDir);

                    if (e is OperationCanceledException)
                    {
                        throw;
                    }

                    //TODO: #259 Log exception
                    return false;
                }
            }

            void cleanUpExtractedFiles(string extractionDir)
            {
                if (Directory.Exists(extractionDir))
                {
                    Directory.Delete(extractionDir, true);
                }
            };
        }

        /// <summary>
        ///     Uninstall a plugin from the plugin registry.
        /// </summary>
        /// <param name="installedPlugin">
        ///     The plugin to be uninstalled.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the uninstallation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin is successfully unistalled. <c>false</c> otherwise.
        /// </returns>
        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                return await this.pluginRegistry.UnregisterPluginAsync(installedPlugin);
            }
        }

        /// <summary>
        ///     Verifies the given installed plugin matches the reocrd in the plugin registry.
        /// </summary>
        /// <param name="installedPlugin">
        ///     An installed plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the installed plugin matches the record in the plugin registry. <c>false</c> otherwise.
        /// </returns>
        public async Task<bool> VerifyInstalledAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            Func<InstalledPlugin, bool> predicate = plugin => plugin.Equals(installedPlugin);
            InstalledPlugin matchingPlugin = await CheckInstalledCoreAsync(predicate, cancellationToken);
            return matchingPlugin != null;
        }

        /// <summary>
        ///     Checks if any plugin with the given ID has been installed to the plugin registry.
        /// </summary>
        /// <param name="pluginId">
        ///     A plugin identifier.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin is currently installed. <c>false</c> otherwise.
        /// </returns>
        public async Task<InstalledPlugin> GetInstalledPluginIfExistsAsync(
            string pluginId,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            Func<InstalledPlugin, bool> predicate = plugin => plugin.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase);
            return await CheckInstalledCoreAsync(predicate, cancellationToken);
        }

        private async Task<InstalledPlugin> CheckInstalledCoreAsync(
            Func<InstalledPlugin, bool> predicate,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(predicate, nameof(predicate));

            IReadOnlyCollection<InstalledPlugin> installedPlugins = await this.pluginRegistry.GetInstalledPlugins();
            InstalledPlugin matchingPlugin = installedPlugins.SingleOrDefault(p => predicate(p));

            return matchingPlugin;
        }

        private async Task<bool> ValidateInstalledPlugin(InstalledPlugin plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            string installPath = plugin.InstallPath;
            string checksum = await HashUtils.GetDirectoryHash(installPath);

            return checksum.Equals(plugin.Checksum);
        }
    }
}

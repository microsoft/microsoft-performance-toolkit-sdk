// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Responsible for the installation and unistallation of plugins to/from a
    ///     specific plugin registry.
    /// </summary>
    public sealed class PluginInstaller
    {
        private readonly PluginRegistry pluginRegistry;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of a <see cref="PluginInstaller"/>.
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The <see cref="PluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        public PluginInstaller(PluginRegistry pluginRegistry)
            : this(pluginRegistry, Logger.Create<PluginInstaller>())
        {
        }

        public PluginInstaller(PluginRegistry pluginRegistry, ILogger logger)
        {
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNull(logger, nameof(logger));

            this.pluginRegistry = pluginRegistry;
            this.logger = logger;          
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
                List<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetInstalledPlugins();
                
                var tasks = Task.WhenAll(installedPlugins.Select(p => CreateInstalledPluginAsync(p, cancellationToken)));
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch
                {
                    throw tasks.Exception;
                }

                return tasks.Result;
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
                InstalledPluginInfo installedPlugin = await GetInstalledPluginIfExistsAsync(pluginPackage.Id, cancellationToken);
                bool needsUninstall = false;

                if (installedPlugin != null)
                {
                    if (installedPlugin.Version.Equals(pluginPackage.Version) && await ValidateInstalledPluginAsync(installedPlugin))
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
                    var pluginToInstall = new InstalledPluginInfo(
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
                if (await VerifyInstalledAsync(installedPlugin.PluginInfo, cancellationToken))
                {
                    return await this.pluginRegistry.UnregisterPluginAsync(installedPlugin.PluginInfo);
                }

                return false;
            }
        }

        /// <summary>
        ///     Verifies the given installed plugin info matches the reocrd in the plugin registry.
        /// </summary>
        /// <param name="installedPluginInfo">
        ///     An installed plugin.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the installed plugin matches the record in the plugin registry. <c>false</c> otherwise.
        /// </returns>
        private async Task<bool> VerifyInstalledAsync(
            InstalledPluginInfo installedPluginInfo,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPluginInfo, nameof(installedPluginInfo));

            Func<InstalledPluginInfo, bool> predicate = plugin => plugin.Equals(installedPluginInfo);
            InstalledPluginInfo matchingPlugin = await CheckInstalledCoreAsync(predicate, cancellationToken);
            return matchingPlugin != null;
        }

        /// <summary>
        ///     Checks if any plugin with the given ID has been installed to the plugin registry
        ///     and returns the matching record.
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
        private async Task<InstalledPluginInfo> GetInstalledPluginIfExistsAsync(
            string pluginId,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginId, nameof(pluginId));

            Func<InstalledPluginInfo, bool> predicate = plugin => plugin.Id.Equals(pluginId, StringComparison.OrdinalIgnoreCase);
            return await CheckInstalledCoreAsync(predicate, cancellationToken);
        }

        private async Task<InstalledPluginInfo> CheckInstalledCoreAsync(
            Func<InstalledPluginInfo, bool> predicate,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(predicate, nameof(predicate));

            IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetInstalledPlugins();
            InstalledPluginInfo matchingPlugin = installedPlugins.SingleOrDefault(p => predicate(p));

            return matchingPlugin;
        }

        private async Task<bool> ValidateInstalledPluginAsync(InstalledPluginInfo plugin)
        {
            Guard.NotNull(plugin, nameof(plugin));

            string installPath = plugin.InstallPath;
            string checksum = await HashUtils.GetDirectoryHash(installPath);

            return checksum.Equals(plugin.Checksum);
        }

        private async Task<InstalledPlugin> CreateInstalledPluginAsync(
            InstalledPluginInfo installedPlugin,
            CancellationToken cancellationToken)
        {
            if (!await VerifyInstalledAsync(installedPlugin, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Plugin {installedPlugin.Id}-{installedPlugin.Version} is no longer installed");
            }

            string metadataFileName = Path.Combine(installedPlugin.InstallPath, PluginPackage.PluginMetadataFileName);
           
            PluginMetadata pluginMetadata = null;
            using (var stream = new FileStream(metadataFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    pluginMetadata = await SerializationUtils.ReadFromStreamAsync<PluginMetadata>(stream, this.logger);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException($"Failed to read metadata from {metadataFileName}: {e.Message}");
                }
            }

            return new InstalledPlugin(installedPlugin, pluginMetadata);
        }
    }
}

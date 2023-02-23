// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Result;

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

        /// <summary>
        ///     Creates an instance of a <see cref="PluginInstaller"/> with a <see cref="ILogger"/> object.
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The <see cref="PluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        /// <param name="logger">
        ///     Used to log messages.
        /// </param>
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
        ///     A <see cref="InstalledPluginsResults"/> instance containing all valid and invalid installed plugins.
        /// </returns>
        public async Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllInstalledPlugins();
                InstalledPluginInfo[] installedPluginsArr = installedPlugins.ToArray();

                Task<InstalledPlugin>[] tasks = installedPluginsArr
                    .Select(p => CreateInstalledPluginAsync(p, cancellationToken))
                    .ToArray();
                
                
                var task = Task.WhenAll(installedPluginsArr.Select(p => CreateInstalledPluginAsync(p, cancellationToken)));

                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    this.logger.Info($"Request for getting all installed plugins is cancelled.");
                    throw;
                }
                catch
                {
                }

                if (!task.IsFaulted && !task.IsCanceled)
                {
                    return new InstalledPluginsResults(
                        task.Result.AsReadOnly(),
                        new List<InstalledPluginInfo>().AsReadOnly());
                }

                var results = new List<InstalledPlugin>();
                var invalidPluginsInfo = new List<InstalledPluginInfo>();
                for (int i = 0; i < tasks.Length; i++)
                {
                    Task<InstalledPlugin> t = tasks[i];
                    InstalledPluginInfo plugin = installedPluginsArr[i];
                    if (t.IsFaulted)
                    {
                        foreach (Exception ex in t.Exception.Flatten().InnerExceptions)
                        {
                            invalidPluginsInfo.Add(plugin);
                            this.logger.Error(ex, $"An error occured when reading installed plugin {plugin}");
                        }
                    }
                    else if (t.Status == TaskStatus.RanToCompletion)
                    {
                        results.Add(t.Result);
                    }
                }

                return new InstalledPluginsResults(results, invalidPluginsInfo);
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
        public async Task<InstalledPluginInfo> InstallPluginAsync(
            PluginPackage pluginPackage,
            string installationRoot,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                // Check if any version of this plugin is already installed.
                InstalledPluginInfo installedPlugin = await this.pluginRegistry.TryGetInstalledPluginByIdAsync(pluginPackage.Id);
                bool needsUninstall = false;

                if (installedPlugin != null)
                {
                    if (!installedPlugin.Version.Equals(pluginPackage.Version))
                    {
                        needsUninstall = true;
                    }
                    else
                    {
                        if (await ValidateInstalledPluginAsync(installedPlugin))
                        {
                            this.logger.Warn($"Attempted to install an already installed and valid plugin.");
                            return null;
                        }

                        this.logger.Warn($"Installer is going to reinstall {installedPlugin} because it is tampered.");
                        needsUninstall = true;
                    }                    
                };

                string installationDir = Path.GetFullPath(Path.Combine(installationRoot, $"{pluginPackage.Id}-{pluginPackage.Version}"));
                try
                {
                    // TODO: #245 This could overwrite binaries that have been unregistered but have not been cleaned up.
                    // We need to revist this code once we have a mechanism for checking whether individual plugin are in use by plugin consumers.
                    this.logger.Info($"Extracting content of plugin {pluginPackage} to {installationDir}");
                    await pluginPackage.ExtractPackageAsync(installationDir, cancellationToken, progress);
                    this.logger.Info("Extraction completed.");

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
                        await this.pluginRegistry.UpdatePluginAsync(installedPlugin, pluginToInstall);
                    }
                    else
                    {
                        await this.pluginRegistry.RegisterPluginAsync(pluginToInstall);
                    }

                    return pluginToInstall;
                }
                catch (Exception e)
                {
                    cleanUpExtractedFiles(installationDir);
                    
                    if (e is OperationCanceledException)
                    {
                        this.logger.Info($"Request to install {pluginPackage} is cancelled.");
                    }
                    else
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} to {installationDir}.");
                    }
                    
                    throw;
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
                if (!await this.pluginRegistry.IsPluginRegisteredAsync(installedPlugin.PluginInfo))
                {
                    this.logger.Warn($"Unable to uninstall plugin {installedPlugin.PluginInfo} because it is not currently registered.");
                    return false;
                }

                await this.pluginRegistry.UnregisterPluginAsync(installedPlugin.PluginInfo);
                return true;
            }
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
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            if (!await this.pluginRegistry.IsPluginRegisteredAsync(installedPlugin))
            {
                throw new InvalidOperationException(
                    $"Plugin {installedPlugin} is no longer registered in the plugin registry");
            }

            if (!await ValidateInstalledPluginAsync(installedPlugin))
            {
                throw new InstalledPluginCorruptedOrMissingException(
                    $"Plugin {installedPlugin} is corrupted or missing.",
                    installedPlugin);
            }

            string metadataFileName = Path.Combine(installedPlugin.InstallPath, PluginPackage.PluginMetadataFileName);
            PluginMetadata pluginMetadata;
            using (var fileStream = new FileStream(metadataFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                pluginMetadata = await JsonSerializer.DeserializeAsync<PluginMetadata>(
                    fileStream,
                    SerializationConfig.PluginsManagerSerializerDefaultOptions,
                    cancellationToken);
            }

            return new InstalledPlugin(installedPlugin, pluginMetadata);
        }
    }
}

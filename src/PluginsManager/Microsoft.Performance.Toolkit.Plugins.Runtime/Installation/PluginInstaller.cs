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
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
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
        ///     Returns all plugins that are registered to the registry.
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
        public async Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllInstalledPlugins();
                InstalledPluginInfo[] installedPluginsArray = installedPlugins.ToArray();

                Task<InstalledPlugin>[] tasks = installedPluginsArray
                    .Select(p => CreateInstalledPluginAsync(p, cancellationToken))
                    .ToArray();


                var task = Task.WhenAll(tasks);

                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    this.logger.Info($"The request to get all installed plugins is cancelled.");
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
                    InstalledPluginInfo plugin = installedPluginsArray[i];
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        results.Add(t.Result);
                    }
                    else
                    {
                        if ((t.IsFaulted))
                        {
                            this.logger.Error($"Failed to load installed plugin {plugin}.", t.Exception);
                        }
                        
                        invalidPluginsInfo.Add(plugin);
                    };
                }

                return new InstalledPluginsResults(results, invalidPluginsInfo);
            }  
        }

        /// <summary>
        ///     Installs a plugin package to a given directory and register it to the plugin registry.
        ///     If a different version of this plugin is already installed, it will be uninstalled first.
        ///     If a same version of this plugin is already installed:
        ///        - If the plugin is valid, returns null without attempting to install.
        ///        - If the plugin is corrupted or missing, it will be uninstalled and reinstalled.
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
        /// <exception cref="ArgumentNullException">
        ///      Throws when <paramref name="pluginPackage"/> or <paramref name="installationRoot"/> or
        ///      <paramref name="sourceUri"/> is null.
        /// </exception>
        /// <exception cref="PluginRegistryException">
        ///     Throws when something is wrong with the plugin registry.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
        public async Task<InstalledPluginInfo> InstallPluginAsync(
            PluginPackage pluginPackage,
            string installationRoot,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackage, nameof(pluginPackage));
            Guard.NotNull(installationRoot, nameof(installationRoot));
            Guard.NotNull(sourceUri, nameof(sourceUri));

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
                        this.logger.Info($"{installedPlugin} is updated to {pluginToInstall} in the plugin registry.");
                    }
                    else
                    {
                        await this.pluginRegistry.RegisterPluginAsync(pluginToInstall);
                        this.logger.Info($"{pluginToInstall} is registered in the plugin registry.");
                    }

                    return pluginToInstall;
                }
                catch (Exception e)
                {
                    if (e is OperationCanceledException)
                    {
                        this.logger.Info($"Request to install {pluginPackage} is cancelled.");
                    }
                    else if (e is PluginRegistryException)
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} to {installationDir} due to a failure in the plugin registry.");
                    }
                    else if (e is PluginPackageException)
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} to {installationDir} due to a failure in the plugin package.");
                    }
                    else
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} to {installationDir}.");
                    }

                    this.logger.Info($"Cleaning up extraction folder: {installationDir}");
                    cleanUpExtractedFiles(installationDir);

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
        /// <exception cref="ArgumentNullException">
        ///     Throws when <paramref name="installedPlugin"/> is null.
        /// </exception>
        /// <exception cref="PluginRegistryException">
        ///     Throws when something is wrong with the plugin registry.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Throws when the operation was cancelled.
        /// </exception>
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

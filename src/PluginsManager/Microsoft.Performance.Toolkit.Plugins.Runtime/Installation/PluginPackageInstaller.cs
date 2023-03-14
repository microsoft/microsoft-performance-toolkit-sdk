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
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a <see cref="IPluginInstaller"/> that installs plugins from a <see cref="PluginPackage"/> stream.
    /// </summary>
    public sealed class PluginPackageInstaller
        : IPluginInstaller<DirectoryInfo>
    {
        private readonly IPluginRegistry<DirectoryInfo> pluginRegistry;
        private readonly IDataReaderFromFileAndStream<PluginMetadata> metadataReader;
        private readonly IDirectoryAccessor directoryAccessor;

        private IChecksumProvider<DirectoryInfo> checksumProvider;
        private readonly ILogger logger;

        ///// <summary>
        /////     Creates an instance of a <see cref="PluginPackageInstaller"/>.
        ///// </summary>
        ///// <param name="pluginRegistry">
        /////     The <see cref="IPluginRegistry"/> this installer register/unregister plugin records to.
        ///// </param>
        //public PluginPackageInstaller(IPluginRegistry pluginRegistry)
        //    : this(
        //          pluginRegistry,

        //          SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>())
        //{
        //}

        /// <summary>
        ///     Creates an instance of a <see cref="PluginPackageInstaller"/> with a <see cref="ISerializer{PluginMetadata}"/> object
        ///     and a <see cref="ILogger"/> object.
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The <see cref="IPluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        /// <param name="logger">
        ///     Used to log messages.
        /// </param>
        public PluginPackageInstaller(
            IPluginRegistry<DirectoryInfo> pluginRegistry,
            IDataReaderFromFileAndStream<PluginMetadata> metadataReader,
            IDirectoryAccessor directoryAccessor,
            IChecksumProvider<DirectoryInfo> checksumProvider,
            ILogger logger)
        {
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNull(logger, nameof(logger));

            this.pluginRegistry = pluginRegistry;
            this.metadataReader = metadataReader;
            this.directoryAccessor = directoryAccessor;
            this.checksumProvider = checksumProvider;
            this.logger = logger;
        }

        public PluginPackageInstaller(
            IPluginRegistry<DirectoryInfo> pluginRegistry)
            : this(
                pluginRegistry,
                new DataReaderFromFileAndStream<PluginMetadata>(SerializationUtils.JsonSerializerWithDefaultOptions),
                new DirectoryAccessor(),
                new SHA256DiretoryChecksumProvider(),
                Logger.Create<PluginPackageInstaller>())
        {
        }

        /// <inheritdoc/>
        public async Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(
            CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllAsync(cancellationToken);

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
                    // Exceptions from each tasks are handled in the loop below.
                }

                if (!task.IsFaulted && !task.IsCanceled)
                {
                    return new InstalledPluginsResults(
                        task.Result.AsReadOnly(),
                        Array.Empty<(InstalledPluginInfo, Exception)>().AsReadOnly());
                }

                var results = new List<InstalledPlugin>();
                var failedResults = new List<(InstalledPluginInfo, Exception)>();
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
                        if (t.IsFaulted)
                        {
                            this.logger.Error($"Failed to load installed plugin {plugin}.", t.Exception);
                        }

                        failedResults.Add((plugin, t.Exception));
                    };
                }

                return new InstalledPluginsResults(results, failedResults);
            }
        }

        /// <remarks>
        ///     Installs a plugin package to a given directory and register it to the plugin registry.
        ///     If a different version of this plugin is already installed, it will be uninstalled first.
        ///     If a same version of this plugin is already installed:
        ///        - If the plugin is valid, returns null without attempting to install.
        ///        - If the plugin is corrupted or missing, it will be uninstalled and reinstalled.
        /// </remarks>
        /// <inheritdoc/>
        public async Task<InstalledPluginInfo> InstallPluginAsync(
            Stream stream,
            DirectoryInfo installationRoot,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(stream, nameof(stream));
            Guard.NotNull(installationRoot, nameof(installationRoot));
            Guard.NotNull(sourceUri, nameof(sourceUri));

            if (!PluginPackage.TryCreate(
                stream,
                this.metadataReader,
                this.directoryAccessor,
                false,
                out PluginPackage pluginPackage))
            {
                throw new PluginPackageCreationException(
                      $"Failed to create plugin package out of a package stream from {sourceUri}");
            }

            using (pluginPackage)
            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                // Check if any version of this plugin is already installed.
                InstalledPluginInfo installedPlugin = await this.pluginRegistry.TryGetByIdAsync(
                    pluginPackage.Id,
                    cancellationToken);

                bool needsUninstall = false;
                if (installedPlugin != null)
                {
                    if (!installedPlugin.Version.Equals(pluginPackage.Version))
                    {
                        needsUninstall = true;
                    }
                    else
                    {
                        if (await ValidateInstalledPlugin(installedPlugin))
                        {
                            this.logger.Warn($"Attempted to install an already installed and valid plugin.");
                            return null;
                        }

                        this.logger.Warn($"Installer is going to reinstall {installedPlugin} because it is tampered.");
                        needsUninstall = true;
                    }
                };

                string installationDir = Path.GetFullPath(Path.Combine(installationRoot.FullName, $"{pluginPackage.Id}-{pluginPackage.Version}"));
                try
                {
                    // TODO: #245 This could overwrite binaries that have been unregistered but have not been cleaned up.
                    // We need to revist this code once we have a mechanism for checking whether individual plugin are in use by plugin consumers.
                    this.logger.Info($"Extracting content of plugin {pluginPackage} to {installationDir}");
                    await pluginPackage.ExtractPackageAsync(installationDir, cancellationToken, progress);
                    this.logger.Info("Extraction completed.");

                    string checksum = await this.checksumProvider.GetChecksumAsync(new DirectoryInfo(installationDir));
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
                        await this.pluginRegistry.UpdateAsync(installedPlugin, pluginToInstall, cancellationToken);
                        this.logger.Info($"{installedPlugin} is updated to {pluginToInstall} in the plugin registry.");
                    }
                    else
                    {
                        await this.pluginRegistry.AddAsync(pluginToInstall, cancellationToken);
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
                    else if (e is RepositoryException)
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
                    this.directoryAccessor.CleanData(new DirectoryInfo(installationDir));

                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                if (!await this.pluginRegistry.ExistsAsync(installedPlugin.PluginInfo, cancellationToken))
                {
                    this.logger.Warn($"Unable to uninstall plugin {installedPlugin.PluginInfo} because it is not currently registered.");
                    return false;
                }

                await this.pluginRegistry.DeleteAsync(installedPlugin.PluginInfo, cancellationToken);
                return true;
            }
        }

        /// <inheritdoc/>
        public async Task CleanupObsoletePluginsAsync(DirectoryInfo installationDir, CancellationToken cancellationToken)
        {
            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllAsync(cancellationToken);
                IEnumerable<string> registeredInstallDirs = installedPlugins.Select(p => Path.GetFullPath(p.InstallPath));

                IEnumerable<DirectoryInfo> deletedDirs = this.directoryAccessor.CleanDataAt(
                    installationDir,
                    dir => registeredInstallDirs.Any(d => d.Equals(dir.FullName, StringComparison.OrdinalIgnoreCase)),
                    cancellationToken);
            }
        }

        private async Task<InstalledPlugin> CreateInstalledPluginAsync(
            InstalledPluginInfo installedPlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            if (!await this.pluginRegistry.ExistsAsync(installedPlugin, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Plugin {installedPlugin} is no longer registered in the plugin registry");
            }

            if (!await ValidateInstalledPlugin(installedPlugin))
            {
                throw new InstalledPluginCorruptedOrMissingException(
                    $"Plugin {installedPlugin} is corrupted or missing.",
                    installedPlugin);
            }

            string metadataFileName = Path.Combine(installedPlugin.InstallPath, PluginPackage.PluginMetadataFileName);
            PluginMetadata pluginMetadata = await this.metadataReader.ReadDataAsync(new FileInfo(metadataFileName), cancellationToken);

            return new InstalledPlugin(installedPlugin, pluginMetadata);
        }

        private async Task<bool> ValidateInstalledPlugin(InstalledPluginInfo installedPluginInfo)
        {
            string calculatedChecksum = await this.checksumProvider.GetChecksumAsync(new DirectoryInfo(installedPluginInfo.InstallPath));
            return calculatedChecksum.Equals(installedPluginInfo.Checksum);
        }
    }
}

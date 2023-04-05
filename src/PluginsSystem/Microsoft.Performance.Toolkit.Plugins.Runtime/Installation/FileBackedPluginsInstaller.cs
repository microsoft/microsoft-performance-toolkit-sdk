﻿// Copyright (c) Microsoft Corporation.
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
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a <see cref="IPluginsInstaller"/> that installs plugins from a <see cref="PluginPackage"/> stream to a file system.
    /// </summary>
    public sealed class FileBackedPluginsInstaller
        : IPluginsInstaller
    {
        private readonly string installationRoot;
        private readonly IPluginRegistry pluginRegistry;
        private readonly ISerializer<PluginMetadata> pluginMetadataSerializer;
        private readonly IInstalledPluginValidator installedPluginValidator;
        private readonly IInstalledPluginStorage installedPluginStorage;
        private readonly IPluginPackageReader pluginPackageReader;

        private readonly ILogger logger;
        private readonly Func<Type, ILogger> loggerFactory;

        /// <summary>
        ///     Creates an instance of a <see cref="FileBackedPluginsInstaller"/> with a default logger factory.
        /// </summary>
        /// <param name="installationRoot">
        ///     The root directory where plugins are installed.
        /// </param>
        /// <param name="pluginRegistry">
        ///     The <see cref="IPluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        /// <param name="metadataSerializer">
        ///     The <see cref="ISerializer{PluginMetadata}"/> used to deserialize plugin metadata.
        /// </param>
        /// <param name="installedPluginValidator">
        ///     The <see cref="IInstalledPluginValidator"/> used to validate installed plugins.
        /// </param>
        /// <param name="packageReader">
        ///     The <see cref="IPluginPackageReader"/> used to read plugin packages.
        /// </param>
        public FileBackedPluginsInstaller(
            string installationRoot,
            IPluginRegistry pluginRegistry,
            ISerializer<PluginMetadata> metadataSerializer,
            IInstalledPluginValidator installedPluginValidator,
            IInstalledPluginStorage installedPluginStorage,
            IPluginPackageReader packageReader)
            : this(
                  installationRoot,
                  pluginRegistry,
                  metadataSerializer,
                  installedPluginValidator,
                  installedPluginStorage,
                  packageReader,
                  Logger.Create)
        {
        }

        /// <summary>
        ///     Creates an instance of a <see cref="FileBackedPluginsInstaller"/>.
        /// </summary>
        /// <param name="installationRoot">
        ///     The root directory where plugins are installed.
        /// </param>
        /// <param name="pluginRegistry">
        ///     The <see cref="IPluginRegistry"/> this installer register/unregister plugin records to.
        /// </param>
        /// <param name="metadataSerializer">
        ///     The <see cref="ISerializer{PluginMetadata}"/> used to deserialize plugin metadata.
        /// </param>
        /// <param name="installedPluginValidator">
        ///     The <see cref="IInstalledPluginValidator"/> used to validate installed plugins.
        /// </param>
        /// <param name="installedPluginStorage">
        ///     The <see cref="IInstalledPluginStorage"/> used to store installed plugins.
        /// </param>
        /// <param name="packageReader">
        ///     The <see cref="IPluginPackageReader"/> used to read plugin packages.
        /// </param>
        /// <param name="loggerFactory">
        ///     The <see cref="Func{Type, ILogger}"/> used to create a logger.
        /// </param>
        public FileBackedPluginsInstaller(
            string installationRoot,
            IPluginRegistry pluginRegistry,
            ISerializer<PluginMetadata> metadataSerializer,
            IInstalledPluginValidator installedPluginValidator,
            IInstalledPluginStorage installedPluginStorage,
            IPluginPackageReader packageReader,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNullOrWhiteSpace(installationRoot, nameof(installationRoot));
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNull(metadataSerializer, nameof(metadataSerializer));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.installationRoot = Path.GetFullPath(installationRoot);

            this.pluginRegistry = pluginRegistry;
            this.pluginMetadataSerializer = metadataSerializer;
            this.installedPluginValidator = installedPluginValidator;
            this.installedPluginStorage = installedPluginStorage;
            this.pluginPackageReader = packageReader;

            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(FileBackedPluginsInstaller));
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
        public async Task<InstalledPlugin> InstallPluginAsync(
            Stream stream,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(stream, nameof(stream));
            Guard.NotNull(sourceUri, nameof(sourceUri));

            PluginPackage pluginPackage = await this.pluginPackageReader.TryReadPackageAsync(stream, cancellationToken);
            if (pluginPackage == null)
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
                        if (await this.installedPluginValidator.ValidateInstalledPluginAsync(installedPlugin))
                        {
                            this.logger.Warn($"Attempted to install an already installed and valid plugin.");
                            return null;
                        }

                        this.logger.Warn($"Installer is going to reinstall {installedPlugin} because it is tampered.");
                        needsUninstall = true;
                    }
                };

                string installationDir = PathUtils.GetPluginInstallDirectory(
                    this.installationRoot,
                    pluginPackage.PluginIdentity);

                try
                {
                    // TODO: #245 This could overwrite binaries that have been unregistered but have not been cleaned up.
                    // We need to revist this code once we have a mechanism for checking whether individual plugin are in use by plugin consumers.
                    this.logger.Info($"Extracting content of plugin {pluginPackage} to {installationDir}");
                    await this.installedPluginStorage.AddAsync(pluginPackage, cancellationToken, progress);
                    this.logger.Info("Extraction completed.");

                    string checksum = await HashUtils.GetDirectoryHashAsync(installationDir);
                    var pluginToInstall = new InstalledPluginInfo(
                        pluginPackage.Id,
                        pluginPackage.Version,
                        sourceUri,
                        pluginPackage.DisplayName,
                        pluginPackage.Description,
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

                    return new InstalledPlugin(pluginToInstall, pluginPackage.PluginMetadata);
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

        /// <summary>
        ///     Attempts to clean up all obsolete (unregistered) plugin files.
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
        public async Task CleanupObsoletePluginsAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(this.installationRoot))
            {
                return;
            }

            using (await this.pluginRegistry.AquireLockAsync(cancellationToken, null))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllAsync(cancellationToken);
                IEnumerable<string> registeredInstallDirs = installedPlugins.Select(
                    p => PathUtils.GetPluginInstallDirectory(this.installationRoot, new PluginIdentity(p.Id, p.Version)));

                foreach (DirectoryInfo dir in new DirectoryInfo(this.installationRoot).GetDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!registeredInstallDirs.Any(d => d.Equals(Path.GetFullPath(dir.FullName), StringComparison.OrdinalIgnoreCase)))
                    {
                        this.logger.Info($"Deleting obsolete plugin files in {dir.FullName}");
                        dir.Delete(true);
                    }
                }
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

            if (!await this.installedPluginValidator.ValidateInstalledPluginAsync(installedPlugin))
            {
                throw new InstalledPluginCorruptedOrMissingException(
                    $"Plugin {installedPlugin} is corrupted or missing.",
                    installedPlugin);
            }

            string installPath = PathUtils.GetPluginInstallDirectory(
                this.installationRoot,
                new PluginIdentity(installedPlugin.Id, installedPlugin.Version));

            string metadataFileName = Path.Combine(installPath, PackageConstants.PluginMetadataFileName);

            PluginMetadata pluginMetadata;
            using (var fileStream = new FileStream(metadataFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                pluginMetadata = await this.pluginMetadataSerializer.DeserializeAsync(
                    fileStream,
                    cancellationToken);
            }

            return new InstalledPlugin(installedPlugin, pluginMetadata);
        }
    }
}

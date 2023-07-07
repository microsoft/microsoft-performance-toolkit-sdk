﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a <see cref="IPluginsInstaller"/> that installs plugins from a <see cref="PluginPackage"/> stream to a file system.
    /// </summary>
    public sealed class FileBackedPluginsInstaller
        : IPluginsInstaller
    {
        private readonly IPluginRegistry pluginRegistry;
        private readonly IInstalledPluginValidator installedPluginValidator;
        private readonly IInstalledPluginStorage installedPluginStorage;
        private readonly IPluginPackageReader pluginPackageReader;
        private readonly ILogger logger;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly IPluginValidator validator;
        private readonly IInvalidPluginsGate invalidGate;

        /// <summary>
        ///     Creates an instance of a <see cref="FileBackedPluginsInstaller"/>.
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The <see cref="IPluginRegistry"/> this installer register/unregister plugin records to.
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
        /// <param name="validator">
        ///     The <see cref="IPluginValidator"/> for validating a plugin that is attempting to be installed.
        /// </param>
        /// <param name="invalidGate">
        ///     The <see cref="IInvalidPluginsGate"/> to bypass plugin validation errors and continue
        ///     plugin installation.
        /// </param>
        /// <param name="loggerFactory">
        ///     The <see cref="Func{Type, ILogger}"/> used to create a logger.
        /// </param>
        public FileBackedPluginsInstaller(
            IPluginRegistry pluginRegistry,
            IInstalledPluginValidator installedPluginValidator,
            IInstalledPluginStorage installedPluginStorage,
            IPluginPackageReader packageReader,
            IPluginValidator validator,
            IInvalidPluginsGate invalidGate,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNull(installedPluginValidator, nameof(installedPluginValidator));
            Guard.NotNull(installedPluginStorage, nameof(installedPluginStorage));
            Guard.NotNull(packageReader, nameof(packageReader));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.pluginRegistry = pluginRegistry;
            this.installedPluginValidator = installedPluginValidator;
            this.installedPluginStorage = installedPluginStorage;
            this.pluginPackageReader = packageReader;
            this.validator = validator;
            this.loggerFactory = loggerFactory;
            this.invalidGate = invalidGate;
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
                    }
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

            ErrorInfo[] errors = this.validator.GetValidationErrors(pluginPackage.Metadata);
            if (errors?.Any() ?? false)
            {
                if (!await this.invalidGate.ShouldProceedDespiteFailures(
                        PluginsSystemOperation.Install,
                        new[] { new PluginValidationFailures(pluginPackage.Metadata.Identity, errors) }))
                {
                    this.logger.Error($"Plugin {pluginPackage.Metadata.Identity} is invalid and will not be installed.");
                    return null;
                }
            }

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
                    pluginPackage.Metadata.Identity.Id,
                    cancellationToken);

                bool needsUninstall = false;
                if (installedPlugin != null)
                {
                    if (!installedPlugin.Metadata.Identity.Version.Equals(pluginPackage.Metadata.Identity.Version))
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
                }

                try
                {
                    // TODO: #245 This could overwrite binaries that have been unregistered but have not been cleaned up.
                    // We need to revist this code once we have a mechanism for checking whether individual plugin are in use by plugin consumers.
                    this.logger.Info($"Extracting content of plugin {pluginPackage}");
                    string checksum = await this.installedPluginStorage.AddAsync(pluginPackage, cancellationToken, progress);
                    this.logger.Info("Extraction completed.");

                    var pluginToInstall = new InstalledPluginInfo(
                        pluginPackage.Metadata,
                        sourceUri,
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

                    return new InstalledPlugin(pluginToInstall, pluginPackage.ContentsMetadata);
                }
                catch (Exception e)
                {
                    if (e is OperationCanceledException)
                    {
                        this.logger.Info($"Request to install {pluginPackage} is cancelled.");
                    }
                    else if (e is RepositoryException)
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} due to a failure in the plugin registry.");
                    }
                    else if (e is PluginPackageException)
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage} to due to a failure in the plugin package.");
                    }
                    else
                    {
                        this.logger.Error(e, $"Failed to install plugin {pluginPackage}.");
                    }

                    this.logger.Info($"Cleaning up extraction folder of {pluginPackage.Metadata.Identity}");

                    try
                    {
                        await this.installedPluginStorage.RemoveAsync(pluginPackage.Metadata.Identity, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(ex, $"Failed to clean up extraction folder of {pluginPackage.Metadata.Identity}");
                    }

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
                if (!await this.pluginRegistry.ExistsAsync(installedPlugin.Info, cancellationToken))
                {
                    this.logger.Warn($"Unable to uninstall plugin {installedPlugin.Info} because it is not currently registered.");
                    return false;
                }

                await this.pluginRegistry.DeleteAsync(installedPlugin.Info, cancellationToken);
                return true;
            }
        }

        private async Task<InstalledPlugin> CreateInstalledPluginAsync(
            InstalledPluginInfo installedPluginInfo,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(installedPluginInfo, nameof(installedPluginInfo));

            if (!await this.pluginRegistry.ExistsAsync(installedPluginInfo, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Plugin {installedPluginInfo} is no longer registered in the plugin registry");
            }

            if (!await this.installedPluginValidator.ValidateInstalledPluginAsync(installedPluginInfo))
            {
                throw new InstalledPluginCorruptedOrMissingException(
                    $"Plugin {installedPluginInfo} is corrupted or missing.",
                    installedPluginInfo);
            }

            PluginContentsMetadata contentsMetadata = await this.installedPluginStorage.TryGetPluginContentsMetadataAsync(
                installedPluginInfo.Metadata.Identity,
                cancellationToken);

            Debug.Assert(contentsMetadata != null);

            return new InstalledPlugin(installedPluginInfo, contentsMetadata);
        }
    }
}

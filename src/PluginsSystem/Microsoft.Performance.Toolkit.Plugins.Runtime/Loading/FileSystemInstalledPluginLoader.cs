// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Loading
{
    /// <summary>
    ///     Loads plugins from the file system.
    /// </summary>
    public sealed class FileSystemInstalledPluginLoader
        : IInstalledPluginLoader
    {
        private readonly IPluginsStorageDirectory pluginsStorageDirectory;
        private readonly IInstalledPluginValidator installedPluginValidator;
        private readonly Func<string, Task<bool>> loadPluginFromDirectory;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly ILogger logger;
        private readonly IPluginValidator pluginMetadataValidator;
        private readonly IInvalidPluginsGate invalidGate;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystemInstalledPluginLoader"/>
        /// </summary>
        /// <param name="pluginsStorageDirectory">
        ///     The directory where plugins are stored.
        /// </param>
        /// <param name="installedPluginValidator">
        ///     The validator to use to validate installed plugins.
        /// </param>
        /// <param name="loadPluginFromDirectory">
        ///     The function to use to load a plugin from a directory.
        /// </param>
        /// <param name="pluginMetadataValidator">
        ///     The <see cref="IPluginValidator"/> for validating a plugin that is being loaded.
        /// </param>
        /// <param name="invalidGate">
        ///     The <see cref="IInvalidPluginsGate"/> to bypass plugin validation errors and continue
        ///     plugin loading.
        /// </param>
        /// <param name="loggerFactory">
        ///     The logger factory to use to create loggers.
        /// </param>
        public FileSystemInstalledPluginLoader(
            IPluginsStorageDirectory pluginsStorageDirectory,
            IInstalledPluginValidator installedPluginValidator,
            Func<string, Task<bool>> loadPluginFromDirectory,
            IPluginValidator pluginMetadataValidator,
            IInvalidPluginsGate invalidGate,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginsStorageDirectory, nameof(pluginsStorageDirectory));
            Guard.NotNull(installedPluginValidator, nameof(installedPluginValidator));
            Guard.NotNull(loadPluginFromDirectory, nameof(loadPluginFromDirectory));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.pluginsStorageDirectory = pluginsStorageDirectory;
            this.loadPluginFromDirectory = loadPluginFromDirectory;
            this.installedPluginValidator = installedPluginValidator;
            this.loggerFactory = loggerFactory;
            this.pluginMetadataValidator = pluginMetadataValidator;
            this.invalidGate = invalidGate;
            this.logger = loggerFactory(typeof(FileSystemInstalledPluginLoader));
        }

        /// <inheritdoc/>
        public async Task<bool> LoadPluginAsync(InstalledPlugin installedPlugin)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            PluginIdentity pluginIdentity = installedPlugin.Info.Metadata.Identity;
            string pluginDirectory = this.pluginsStorageDirectory.GetContentDirectory(pluginIdentity);

            if (!await this.installedPluginValidator.ValidateInstalledPluginAsync(installedPlugin.Info))
            {
                this.logger.Error($"Plugin {pluginIdentity} is not valid installed plugin. It will not be loaded. Has it been corrupted?");
                return false;
            }

            if (!this.pluginMetadataValidator.IsValid(installedPlugin.Info.Metadata, out ErrorInfo[] errors))
            {
                if (!await this.invalidGate.ShouldProceedDespiteFailures(
                        PluginsSystemOperation.Load,
                        new[] { new PluginValidationFailures(installedPlugin.Info.Metadata.Identity, errors) }))
                {
                    this.logger.Error($"Plugin {pluginIdentity} is not valid. It will not be loaded.");
                    return false;
                }
            }

            return await this.loadPluginFromDirectory(pluginDirectory);
        }
    }
}

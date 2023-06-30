// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

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
        /// <param name="loggerFactory">
        ///     The logger factory to use to create loggers.
        /// </param>
        public FileSystemInstalledPluginLoader(
            IPluginsStorageDirectory pluginsStorageDirectory,
            IInstalledPluginValidator installedPluginValidator,
            Func<string, Task<bool>> loadPluginFromDirectory,
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
                this.logger.Error($"Plugin {pluginIdentity} is not valid. It will not be loaded.");
                return false;
            }

            return await this.loadPluginFromDirectory(pluginDirectory);
        }
    }
}

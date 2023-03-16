// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager
        : IPluginsManager
    {
        private readonly IPluginsDiscoveryManager pluginsDiscovery;
        private readonly IPluginsInstaller pluginsInstaller;
        private readonly IPluginsFetchingManager pluginFetchingManager;
        private readonly ILogger logger;

        ///// <summary>
        /////     Initializes a plugin manager instance with a default <see cref="ILogger"/>.
        ///// </summary>
        ///// <param name="discovererProviders">
        /////     A collection of discoverer providers.
        ///// </param>
        ///// <param name="fetchers">
        /////     A collection of fetchers.
        ///// </param>
        ///// <param name="pluginInstaller">
        /////     A <see cref="IPluginsInstaller"/> this plugin manager uses to install/unintall plugins.
        ///// </param>
        ///// <param name="installationDir">
        /////     The directory where the plugins will be installed to.
        ///// </param>
        //public PluginsManager(
        //    IEnumerable<IPluginDiscovererProvider> discovererProviders,
        //    IEnumerable<IPluginFetcher> fetchers,
        //    IPluginsInstaller pluginInstaller)
        //    : this(
        //          discovererProviders,
        //          fetchers,
        //          new PluginSourceRepo(),
        //          pluginInstaller,
        //          Logger.Create<PluginsManager>())
        //{
        //}

        /// <param name="pluginsDiscoveryManager">
        ///     A <see cref="IPluginsDiscoveryManager"/> this plugin manager uses to discover plugins.
        /// </param>
        /// <param name="pluginInstaller">
        ///     A <see cref="IPluginsInstaller"/> this plugin manager uses to install/unintall plugins.
        /// </param>
        /// <param name="pluginFetchingManager">
        ///     A <see cref="IPluginsFetchingManager"/> this plugin manager uses to fetch plugins.
        /// </param>
        /// <param name="logger">
        ///     A logger used to log messages.
        /// </param>
        public PluginsManager(
            IPluginsDiscoveryManager pluginsDiscoveryManager,
            IPluginsInstaller pluginsInstaller,
            IPluginsFetchingManager pluginFetchingManager,
            ILogger logger)
        {
            Guard.NotNull(pluginsInstaller, nameof(pluginsInstaller));
            Guard.NotNull(logger, nameof(logger));

            this.pluginsDiscovery = pluginsDiscoveryManager;
            this.pluginsInstaller = pluginsInstaller;
            this.pluginFetchingManager = pluginFetchingManager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured
        {
            add
            {
                this.pluginsDiscovery.PluginSourceErrorOccured += value;
            }

            remove
            {
                this.pluginsDiscovery.PluginSourceErrorOccured -= value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.pluginsDiscovery.PluginSources;
            }
        }

        // TODO: #271 Re-enable when we start to support loading additional resources.
        ///// <inheritdoc />
        //public bool LoadAdditionalPluginResources(string directory)
        //{
        //    return this.resourceLoader.TryLoad(directory);
        //}

        /// <inheritdoc />
        public Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            return this.pluginsDiscovery.GetAvailablePluginsLatestAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource pluginSource,
            CancellationToken cancellationToken)
        {
            return this.pluginsDiscovery.GetAvailablePluginsLatestFromSourceAsync(pluginSource, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            return this.pluginsDiscovery.GetAllVersionsOfPluginAsync(pluginIdentity, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginFromSourceAsync(
            PluginSource source,
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            return this.pluginsDiscovery.GetAllVersionsOfPluginFromSourceAsync(source, pluginIdentity, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<InstalledPluginInfo> TryInstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            Stream stream;
            try
            {
                stream = await this.pluginFetchingManager.FetchPluginStream(availablePlugin, cancellationToken, progress);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info("Request to fetch plugin package is cancelled.");
                throw;
            }
            catch (Exception e)
            {
                string errorMsg = $"Fails to fetch plugin {availablePlugin.AvailablePluginInfo.Identity} " +
                    $"from {availablePlugin.AvailablePluginInfo.PluginPackageUri}";

                this.logger.Error(e, errorMsg);

                throw new PluginFetchingException(errorMsg, availablePlugin.AvailablePluginInfo, e);
            }

            using (stream)
            {
                return await InstallPluginAsync(
                    stream,
                    availablePlugin.AvailablePluginInfo.PluginPackageUri,
                    cancellationToken,
                    progress);
            }
        }

        /// <inheritdoc />
        /// TODO: Remove and just let client load the stream?
        public async Task<InstalledPluginInfo> TryInstallLocalPluginAsync(
            string pluginPackagePath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackagePath, nameof(pluginPackagePath));

            string packageFullPath = Path.GetFullPath(pluginPackagePath);
            using (var stream = new FileStream(pluginPackagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return await this.pluginsInstaller.InstallPluginAsync(
                    stream,
                    new Uri(packageFullPath),
                    cancellationToken,
                    progress);
            }
        }

        /// <inheritdoc/>
        public Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(CancellationToken cancellationToken)
        {
            return this.pluginsInstaller.GetAllInstalledPluginsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<InstalledPluginInfo> InstallPluginAsync(
            Stream pluginStream,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return this.pluginsInstaller.InstallPluginAsync(
                pluginStream,
                sourceUri,
                cancellationToken,
                progress);
        }

        /// <inheritdoc />
        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken)
        {
            return await this.pluginsInstaller.UninstallPluginAsync(installedPlugin, cancellationToken);
        }
    }
}

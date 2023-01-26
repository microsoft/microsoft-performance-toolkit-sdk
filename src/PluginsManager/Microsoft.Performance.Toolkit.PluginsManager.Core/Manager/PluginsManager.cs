// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Extensibility;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Installation;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Registry;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Transport;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager : IPluginsManager
    {
        private readonly IPluginManagerResourceLoader resourceLoader;

        private readonly ResourceRepository<IPluginDiscovererProvider> discovererProviderRepository;
        private readonly ResourceRepository<IPluginFetcher> pluginFetcherRepository;

        private readonly DiscoverersManager discoverersManager;

        private readonly PluginInstaller pluginInstaller;

        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviders">
        ///     A collection of discoverer providers.
        /// </param>
        /// <param name="pluginFetchers">
        ///     A collection of plugin fetchers.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginManagerResource"/>s at run time.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> pluginFetchers,
            IPluginManagerResourceLoader resourceLoader,
            PluginRegistry pluginRegistry)
        {
            this.resourceLoader = resourceLoader;
            
            this.discovererProviderRepository = 
                new ResourceRepository<IPluginDiscovererProvider>(discovererProviders);

            this.discoverersManager = new DiscoverersManager(
                this.discovererProviderRepository,
                new DiscoverersFactory());


            this.pluginFetcherRepository = new ResourceRepository<IPluginFetcher>(pluginFetchers);

            this.resourceLoader.Subscribe(this.discovererProviderRepository);
            this.resourceLoader.Subscribe(this.pluginFetcherRepository);

            this.pluginInstaller = new PluginInstaller(pluginRegistry);
        }

        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        { 
            get
            {
                return this.discoverersManager.PluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.discoverersManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public void AddPluginSources(IEnumerable<PluginSource> sources)
        {
            this.discoverersManager.AddPluginSources(sources);
        }

        /// <inheritdoc />
        public void LoadAdditionalPluginResources(string directory)
        {
            this.resourceLoader.TryLoad(directory);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            var results = new List<AvailablePlugin>();

            foreach (PluginSource pluginSource in this.PluginSources)
            {
                IReadOnlyCollection<AvailablePlugin> plugins = await GetAvailablePluginsLatestFromSourceAsync(pluginSource, cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));

            if (!this.PluginSources.Contains(source))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            var results = new List<AvailablePlugin>();
            foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(source).AsQueryable())
            {
                IReadOnlyCollection<AvailablePlugin> plugins = await discoverer.DiscoverPluginsLatestAsync(cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPlugin(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            if (this.PluginSources.Contains(availablePlugin.PluginSource))
            {
                foreach (IPluginDiscoverer discoverer in this.discoverersManager.GetDiscoverersFromSource(availablePlugin.PluginSource).AsQueryable())
                {
                    IReadOnlyCollection<AvailablePlugin> plugins = await discoverer.DiscoverAllVersionsOfPlugin(
                        availablePlugin.Identity,
                        cancellationToken);

                    if (plugins.Any())
                    {
                        return plugins;
                    }
                }
            }

            return Array.Empty<AvailablePlugin>();
        }

        /// <summary>
        ///     This is just an example of how a fetchers will be used. Will be moved to the installer.
        /// </summary>
        /// <param name="availablePlugin"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public async Task<Stream> GetPluginPackageStreamAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            foreach (IPluginFetcher fetcher in this.pluginFetcherRepository.PluginResources)
            {
                if (fetcher.TypeId == availablePlugin.FetcherTypeId && await fetcher.IsSupportedAsync(availablePlugin))
                {
                    return await fetcher.GetPluginStreamAsync(availablePlugin, cancellationToken, progress);
                }
            }

            throw new InvalidOperationException("No supported fetcher found");
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<InstalledPlugin>> GetInstalledPluginsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool> InstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            string installationDir,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            IPluginFetcher pluginFetcher = await GetPluginFetcher(availablePlugin);
            
            using (Stream stream = await pluginFetcher.GetPluginStreamAsync(availablePlugin, cancellationToken, progress))
            using (var pluginPackage = new PluginPackage(stream))
            {
                return await this.pluginInstaller.InstallPluginAsync(
                    pluginPackage,
                    installationDir,
                    availablePlugin.PluginPackageUri,
                    cancellationToken,
                    progress);
            }
        }

        /// <inheritdoc />
        public async Task<bool> InstallLocalPluginAsync(
            string pluginPackagePath,
            string installationDir,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackagePath, nameof(pluginPackagePath));
            Guard.NotNull(installationDir, nameof(installationDir));

            string packageFullPath = Path.GetFullPath(pluginPackagePath);

            using (var pluginPackage = new PluginPackage(pluginPackagePath))
            {
                await this.pluginInstaller.InstallPluginAsync(
                    pluginPackage,
                    installationDir,
                    new Uri(packageFullPath),
                    cancellationToken,
                    progress);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> UninstallPluginAsync(
            InstalledPlugin installedPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            return await this.pluginInstaller.UninstallPluginAsync(installedPlugin, cancellationToken, progress);
        }

        /// <inheritdoc />
        public async Task<bool> UpdatePluginAsync(
            InstalledPlugin installedPlugin,
            AvailablePlugin targetPlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(installedPlugin, nameof(installedPlugin));
            Guard.NotNull(targetPlugin, nameof(targetPlugin));

            if (installedPlugin.Id != targetPlugin.Identity.Id || installedPlugin.Version == targetPlugin.Identity.Version)
            {
                throw new ArgumentException("Target plugin must have a same id but different version.");
            }

            IPluginFetcher pluginFetcher = await GetPluginFetcher(targetPlugin);

            using (Stream stream = await pluginFetcher.GetPluginStreamAsync(targetPlugin, cancellationToken, progress))
            using (var pluginPackage = new PluginPackage(stream))
            {
                return await this.pluginInstaller.UpdatePluginAsync(
                    installedPlugin,
                    pluginPackage,
                    targetPlugin.PluginSource.Uri,
                    cancellationToken,
                    progress);
            }
        }
        private async Task<IPluginFetcher> GetPluginFetcher(AvailablePlugin availablePlugin)
        {
            foreach (IPluginFetcher fetcher in this.pluginFetcherRepository.PluginResources)
            {
                if (fetcher.TypeId == availablePlugin.FetcherTypeId && await fetcher.IsSupportedAsync(availablePlugin))
                {
                    return fetcher;
                }
            }

            throw new InvalidOperationException("No supported fetcher found");
        }
    }
}


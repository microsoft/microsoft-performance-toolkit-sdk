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
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Result;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager
        : IPluginsManager
    {
        private readonly IPluginManagerResourceLoader resourceLoader;

        private readonly PluginManagerResourceRepository<IPluginDiscovererProvider> discovererRepository;
        private readonly PluginManagerResourceRepository<IPluginFetcher> fetcherRepository;

        private readonly DiscovererSourcesManager discovererSourcesManager;
        
        private readonly PluginInstaller pluginInstaller;
        private readonly PluginRegistry pluginRegistry;

        private readonly string installationDir;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a plugin manager instance with a default <see cref="ILogger"/>.
        /// </summary>
        /// <param name="discovererProviders">
        ///     A collection of discoverer providers.
        /// </param>
        /// <param name="fetchers">
        ///     A collection of fetchers.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginManagerResource"/>s at run time.
        /// </param>
        /// <param name="pluginRegistry">
        ///     A plugin registry this plugin manager will interact with.
        /// </param>
        /// <param name="installationDir">
        ///     The directory where the plugins will be installed to.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> fetchers,
            IPluginManagerResourceLoader resourceLoader,
            PluginRegistry pluginRegistry,
            string installationDir) 
            : this(discovererProviders, fetchers, resourceLoader, pluginRegistry, installationDir, Logger.Create<PluginsManager>())
        {
        }

        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviders">
        ///     A collection of discoverer providers.
        /// </param>
        /// <param name="fetchers">
        ///     A collection of fetchers.
        /// </param>
        /// <param name="resourceLoader">
        ///     A loader that can load additional <see cref="IPluginManagerResource"/>s at run time.
        /// </param>
        /// <param name="pluginRegistry">
        ///     A plugin registry this plugin manager will interact with.
        /// </param>
        /// <param name="installationDir">
        ///     The directory where the plugins will be installed to.
        /// </param>
        /// <param name="logger">
        ///     A logger used to log messages.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> fetchers,
            IPluginManagerResourceLoader resourceLoader,
            PluginRegistry pluginRegistry,
            string installationDir,
            ILogger logger)
        {
            Guard.NotNull(discovererProviders, nameof(discovererProviders));
            Guard.NotNull(fetchers, nameof(fetchers));
            Guard.NotNull(resourceLoader, nameof(resourceLoader));
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNullOrWhiteSpace(installationDir, nameof(installationDir));
            Guard.NotNull(logger, nameof(logger));

            this.resourceLoader = resourceLoader;
            this.discovererRepository = new PluginManagerResourceRepository<IPluginDiscovererProvider>(discovererProviders);
            this.fetcherRepository = new PluginManagerResourceRepository<IPluginFetcher>(fetchers);

            this.discovererSourcesManager = new DiscovererSourcesManager(this.discovererRepository, new DiscoverersFactory());

            this.resourceLoader.Subscribe(this.discovererRepository);
            this.resourceLoader.Subscribe(this.fetcherRepository);

            this.pluginRegistry = pluginRegistry;
            this.pluginInstaller = new PluginInstaller(pluginRegistry);
            this.installationDir = installationDir;
            this.logger = logger;
        }
        
        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.discovererSourcesManager.PluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.discovererSourcesManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public void AddPluginSources(IEnumerable<PluginSource> sources)
        {
            this.discovererSourcesManager.AddPluginSources(sources);
        }

        /// <inheritdoc />
        public bool LoadAdditionalPluginResources(string directory)
        {
            return this.resourceLoader.TryLoad(directory);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            var tasks = Task.WhenAll(this.PluginSources.Select(s => GetAvailablePluginsLatestFromSourceAsync(s, cancellationToken)));
            try
            {
                await tasks.ConfigureAwait(false);
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from all plugin sources.
            // For each discovered plugin, the latest version across all sources will be returned.
            // For now, we assume:
            //      1. Duplicates (plugins with same id and version) maybe be discovered from different sources.
            //      2. In the case when duplicated "lastest" are discovered, only one of the duplicates will be returned.
            var results = new Dictionary<string, AvailablePlugin>();
            foreach (IReadOnlyCollection<AvailablePlugin> taskResult in tasks.Result)
            {
                IEnumerable<KeyValuePair<string, AvailablePlugin>> kvps = taskResult
                    .Select(p => new KeyValuePair<string, AvailablePlugin>(p.AvailablePluginInfo.Identity.Id, p));

                results = results.Union(kvps)
                       .GroupBy(g => g.Key)
                       .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value)
                                                       .OrderByDescending(x => x.AvailablePluginInfo.Identity.Version)
                                                       .ThenBy(x => x.AvailablePluginInfo.PluginSource.Uri)
                                                       .First());
            }

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestFromSourceAsync(
            PluginSource source,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));

            if (!this.PluginSources.Contains(source))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            IPluginDiscoverer[] discoverers = this.discovererSourcesManager.GetDiscoverersFromSource(source).ToArray();

            var tasks = Task.WhenAll(discoverers.Select(d => d.DiscoverPluginsLatestAsync(cancellationToken)));
            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from the same plugin source but by different discoverers.
            // If more than one available plugin with the same identity are discovered, the first of them will be returned.
            var results = new Dictionary<string, AvailablePlugin>();
            for (int i = 0; i < tasks.Result.Length; i++)
            {
                IPluginDiscoverer discoverer = discoverers[i];

                foreach (KeyValuePair<string, AvailablePluginInfo> kvp in tasks.Result[i])
                {
                    string id = kvp.Key;
                    AvailablePluginInfo pluginInfo = kvp.Value;

                    if (!results.TryGetValue(id, out AvailablePlugin availablePlugin) ||
                        availablePlugin.AvailablePluginInfo.Identity.Version < pluginInfo.Identity.Version)
                    {

                        IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                        var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                        results[id] = newPlugin;
                    }
                    else if (availablePlugin.AvailablePluginInfo.Identity.Equals(pluginInfo.Identity))
                    {
                        // TODO: Log a warning for discovering duplicates.
                    }
                }
            }

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            var tasks = Task.WhenAll(this.PluginSources.Select(s => GetAllVersionsOfPluginFromSourceAsync(s, pluginIdentity, cancellationToken)));
            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines versions of plugin discovered from all plugin sources.
            // For now, we assume:
            //      1. Duplicates (same version) maybe discovered from different sources.
            //      2. In the case when duplicated versions are discovered, only one of them will be returned.
            var results = tasks.Result.SelectMany(x => x)
                                .GroupBy(x => x.AvailablePluginInfo.Identity)
                                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.AvailablePluginInfo.PluginSource.Uri)
                                                                .First());

            return results.Values;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginFromSourceAsync(
            PluginSource source,
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            IPluginDiscoverer[] discoverers = this.discovererSourcesManager.GetDiscoverersFromSource(source).ToArray();
            var tasks = Task.WhenAll(discoverers.Select(d => d.DiscoverAllVersionsOfPlugin(pluginIdentity, cancellationToken)));

            try
            {
                await tasks;
            }
            catch
            {
                throw tasks.Exception;
            }

            // The below code combines plugins discovered from the same plugin source but by different discoverers.
            // If more than one available plugin with the same identity are discovered, the first of them will be returned.
            var results = new Dictionary<PluginIdentity, AvailablePlugin>();
            for (int i = 0; i < discoverers.Length; i++)
            {
                IPluginDiscoverer discoverer = discoverers[i];
                foreach (AvailablePluginInfo pluginInfo in tasks.Result[i])
                {
                    if (results.ContainsKey(pluginInfo.Identity))
                    {
                        // TODO: Log a warning for discovering duplicates.
                        continue;
                    }

                    IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                    var availablePlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[pluginInfo.Identity] = availablePlugin;
                }
            }

            return results.Values;
        }

        /// <inheritdoc />
        public Task<InstalledPluginsResults> GetInstalledPluginsAsync(CancellationToken cancellationToken)
        {
            return this.pluginInstaller.GetAllInstalledPluginsAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<InstalledPluginInfo> InstallAvailablePluginAsync(
            AvailablePlugin availablePlugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(availablePlugin, nameof(availablePlugin));

            Stream stream;
            try
            {
                stream = await availablePlugin.GetPluginPackageStream(cancellationToken, progress);
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
                if (!PluginPackage.TryCreate(stream, out PluginPackage pluginPackage))
                {
                    throw new PluginPackageCreationException(
                        $"Failed to create plugin package from discovered plugin {availablePlugin.AvailablePluginInfo.Identity}");
                }

                using (pluginPackage)
                {
                    return await this.pluginInstaller.InstallPluginAsync(
                        pluginPackage,
                        this.installationDir,
                        availablePlugin.AvailablePluginInfo.PluginPackageUri,
                        cancellationToken,
                        progress);
                }
            }
        }

        /// <inheritdoc />
        public async Task<InstalledPluginInfo> InstallLocalPluginAsync(
            string pluginPackagePath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackagePath, nameof(pluginPackagePath));

            string packageFullPath = Path.GetFullPath(pluginPackagePath);
            
            if (!PluginPackage.TryCreate(pluginPackagePath, out PluginPackage pluginPackage))
            {
                throw new PluginPackageCreationException($"Failed to create plugin package from {packageFullPath}");
            }

            using (pluginPackage)
            {
                return await this.pluginInstaller.InstallPluginAsync(
                    pluginPackage,
                    this.installationDir,
                    new Uri(packageFullPath),
                    cancellationToken,
                    progress);
            }
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
        public async Task CleanupObsoletePluginsAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(this.installationDir))
            {
                return;
            }

            using (await this.pluginRegistry.UseLock(cancellationToken))
            {
                IReadOnlyCollection<InstalledPluginInfo> installedPlugins = await this.pluginRegistry.GetAllInstalledPlugins();
                IEnumerable<string> registeredInstallDirs = installedPlugins.Select(p => Path.GetFullPath(p.InstallPath));
                var toRemove = new List<string>();
                foreach (DirectoryInfo dir in new DirectoryInfo(this.installationDir).GetDirectories())
                {
                    if (!registeredInstallDirs.Any(d => d.Equals(Path.GetFullPath(dir.FullName), StringComparison.OrdinalIgnoreCase)))
                    {
                        dir.Delete(true);
                    }
                }
            }
        }

        private async Task<IPluginFetcher> GetPluginFetcher(AvailablePluginInfo availablePluginInfo)
        {
            foreach (IPluginFetcher fetcher in this.fetcherRepository.Resources)
            {
                if (fetcher.TryGetGuid() == availablePluginInfo.FetcherResourceId &&
                    await fetcher.IsSupportedAsync(availablePluginInfo, Logger.Create(fetcher.GetType())))
                {
                    return fetcher;
                }
            }

            throw new InvalidOperationException("No supported fetcher found");
        }
    }
}

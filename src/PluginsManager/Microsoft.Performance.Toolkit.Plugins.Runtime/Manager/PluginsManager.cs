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
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Result;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager
        : IPluginsManager
    {
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
        /// <param name="pluginRegistry">
        ///     A plugin registry this plugin manager will interact with.
        /// </param>
        /// <param name="installationDir">
        ///     The directory where the plugins will be installed to.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> fetchers,
            PluginRegistry pluginRegistry,
            string installationDir) 
            : this(discovererProviders, fetchers, pluginRegistry, installationDir, Logger.Create<PluginsManager>())
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
            PluginRegistry pluginRegistry,
            string installationDir,
            ILogger logger)
        {
            Guard.NotNull(discovererProviders, nameof(discovererProviders));
            Guard.NotNull(fetchers, nameof(fetchers));
            Guard.NotNull(pluginRegistry, nameof(pluginRegistry));
            Guard.NotNullOrWhiteSpace(installationDir, nameof(installationDir));
            Guard.NotNull(logger, nameof(logger));

            this.discovererRepository = new PluginManagerResourceRepository<IPluginDiscovererProvider>(discovererProviders);
            this.fetcherRepository = new PluginManagerResourceRepository<IPluginFetcher>(fetchers);

            this.discovererSourcesManager = new DiscovererSourcesManager(this.discovererRepository, new DiscoverersFactory());

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

        // TODO: Re-enable when we start to support loading additional resources.
        ///// <inheritdoc />
        //public bool LoadAdditionalPluginResources(string directory)
        //{
        //    return this.resourceLoader.TryLoad(directory);
        //}

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            PluginSource[] pluginSources = this.PluginSources.ToArray();
            Task<IReadOnlyCollection<AvailablePlugin>>[] tasks = this.PluginSources
                .Select(s => GetAvailablePluginsLatestFromSourceAsync(s, cancellationToken))
                .ToArray() ;
            
            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get lastest available plugins is cancelled.");
                throw;
            }
            catch
            {
            }

            // The below code combines plugins discovered from all plugin sources.
            // For each discovered plugin, the latest version across all sources will be returned.
            // For now, we assume:
            //      1. Duplicates (plugins with same id and version) maybe be discovered from different sources.
            //      2. In the case when duplicated "lastest" are discovered, only one of the duplicates will be returned.
            var discoveredAvailablePlugins = new List<IReadOnlyCollection<AvailablePlugin>>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyCollection<AvailablePlugin>> t = tasks[i];
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Successfully discovered {t.Result.Count} available plugins from source {pluginSources[i].Uri}.");
                    discoveredAvailablePlugins.Add(t.Result);
                }          
                else if (t.IsFaulted)
                {
                    this.logger.Error($"Failed to get available plugins from source {pluginSources[i].Uri}. Skipping.", t.Exception);
                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"The request to get lastest available plugins from source {pluginSources[i].Uri} is cancelled.");
                    continue;
                }
                else
                {
                    continue;
                }
            }

            var results = new Dictionary<string, AvailablePlugin>();
            foreach (IReadOnlyCollection<AvailablePlugin> taskResult in discoveredAvailablePlugins)
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
            if (discoverers.Length == 0)
            {
                this.logger.Warn($"No discoverer is available for plugin source {source.Uri}.");
                return new AvailablePlugin[0];
            }

            Task<IReadOnlyDictionary<string, AvailablePluginInfo>>[] tasks = discoverers.Select(d => d.DiscoverPluginsLatestAsync(cancellationToken)).ToArray();
            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get available plugins from {source.Uri} is cancelled.");
                throw;
            }
            catch
            {
            }

            var results = new Dictionary<string, (AvailablePlugin, IPluginDiscoverer)>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyDictionary<string, AvailablePluginInfo>> t = tasks[i];
                IPluginDiscoverer discoverer = discoverers[i];
                string discovererTypeStr = discoverer.GetType().Name;

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} discovered {t.Result.Count} plugins from {source.Uri}.");

                    // Combines plugins discovered from the same plugin source but by different discoverers.
                    // If more than one available plugin with the same identity are discovered, the first of them will be returned.
                    await ProcessDiscoverAllResult(task.Result[i], discoverer, source, results);
                }
                else if (t.IsFaulted)
                {
                    this.logger.Error($"Discoverer {discovererTypeStr} failed to discover plugins from {source.Uri}.", t.Exception);
                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} cancelled the discovery of plugins from {source.Uri}.");
                    continue;
                }
                else
                {
                    continue;
                }
            }

            AvailablePlugin[] availablePlugins = results.Values.Select(tuple => tuple.Item1).ToArray();
            return availablePlugins.AsReadOnly();
        }
        
        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            Task<IReadOnlyCollection<AvailablePlugin>>[] tasks = this.PluginSources.Select(s => GetAllVersionsOfPluginFromSourceAsync(s, pluginIdentity, cancellationToken)).ToArray();
            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get all versions of plugin {pluginIdentity} is cancelled.");
                throw;
            }
            catch
            {
            }

            // The below code combines versions of plugin discovered from all plugin sources.
            // For now, we assume:
            //      1. Duplicates (same version) maybe discovered from different sources.
            //      2. In the case when duplicated versions are discovered, only one of them will be returned.
            var results = task.Result.SelectMany(x => x)
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
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            IPluginDiscoverer[] discoverers = this.discovererSourcesManager.GetDiscoverersFromSource(source).ToArray();
            if (discoverers.Length == 0)
            {
                this.logger.Warn($"No discoverer is available for plugin source {source.Uri}.");
                return new AvailablePlugin[0];
            }

            Task<IReadOnlyCollection<AvailablePluginInfo>>[] tasks = discoverers.Select(d => d.DiscoverAllVersionsOfPlugin(pluginIdentity, cancellationToken)).ToArray();

            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get all versions of plugin {pluginIdentity} from {source.Uri} is cancelled.");
            }
            catch
            {
            }

            var results = new Dictionary<PluginIdentity, (AvailablePlugin, IPluginDiscoverer)>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyCollection<AvailablePluginInfo>> t = tasks[i];
                IPluginDiscoverer discoverer = discoverers[i];
                string discovererTypeStr = discoverer.GetType().Name;

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} discovered {t.Result.Count} plugins from {source.Uri}.");

                    // Combines plugins discovered from the same plugin source but by different discoverers.
                    // If more than one available plugin with the same identity are discovered, the first of them will be returned.
                    await ProcessDiscoverAllVersionsResult(t.Result, discoverer, source, results);
                }
                else if (t.IsFaulted)
                {
                    this.logger.Error($"Discoverer {discovererTypeStr} failed to discover plugins from {source.Uri}.", t.Exception);
                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} cancelled the discovery of plugins from {source.Uri}.");
                    continue;
                }
                else
                {
                    continue;
                }
            }

            AvailablePlugin[] availablePlugins = results.Values.Select(tuple => tuple.Item1).ToArray();
            return availablePlugins.AsReadOnly();
        }

        /// <inheritdoc />
        public Task<InstalledPluginsResults> GetInstalledPluginsAsync(CancellationToken cancellationToken)
        {
            return this.pluginInstaller.GetAllInstalledPluginsAsync(cancellationToken);
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
                if (PluginPackage.TryCreate(stream, out PluginPackage pluginPackage))
                {
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
                else
                {
                    throw new PluginPackageCreationException(
                       $"Failed to create plugin package from discovered plugin {availablePlugin.AvailablePluginInfo.Identity}");
                }
            }
        }

        /// <inheritdoc />
        public async Task<InstalledPluginInfo> TryInstallLocalPluginAsync(
            string pluginPackagePath,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Guard.NotNull(pluginPackagePath, nameof(pluginPackagePath));

            string packageFullPath = Path.GetFullPath(pluginPackagePath);
            using (var stream = new FileStream(pluginPackagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (PluginPackage.TryCreate(stream, out PluginPackage pluginPackage))
                {
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
                else
                {
                    throw new PluginPackageCreationException($"Failed to create plugin package from {packageFullPath}");
                }
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

                foreach (DirectoryInfo dir in new DirectoryInfo(this.installationDir).GetDirectories())
                {
                    if (!registeredInstallDirs.Any(d => d.Equals(Path.GetFullPath(dir.FullName), StringComparison.OrdinalIgnoreCase)))
                    {
                        this.logger.Info($"Deleting obsolete plugin files in {dir.FullName}");
                        dir.Delete(true);
                    }
                }
            }
        }

        private async Task<IPluginFetcher> GetPluginFetcher(AvailablePluginInfo availablePluginInfo)
        {
            IPluginFetcher fetcherToUse = this.fetcherRepository.Resources
                .SingleOrDefault(fetcher => fetcher.TryGetGuid() == availablePluginInfo.FetcherResourceId);

            if (fetcherToUse == null)
            {
                this.logger.Error($"Fetcher with ID {availablePluginInfo.FetcherResourceId} is not found.");
                return null;
            }

            Type fetcherType = fetcherToUse.GetType();
            bool isSupported = await fetcherToUse.IsSupportedAsync(availablePluginInfo, Logger.Create(fetcherType));
            if (!isSupported)
            {
                this.logger.Error($"Fetcher {fetcherType.Name} doesn't support fetching from {availablePluginInfo.PluginPackageUri}");
                return null;
            }

            return fetcherToUse ;
        }

        private async Task ProcessDiscoverAllResult(
            IReadOnlyDictionary<string, AvailablePluginInfo> discoveryResult,
            IPluginDiscoverer discoverer,
            PluginSource source,
            IDictionary<string, (AvailablePlugin, IPluginDiscoverer)> results)
        {
            foreach (KeyValuePair<string, AvailablePluginInfo> kvp in discoveryResult)
            {
                string id = kvp.Key;
                AvailablePluginInfo pluginInfo = kvp.Value;

                if (!results.TryGetValue(id, out (AvailablePlugin, IPluginDiscoverer) tuple) ||
                    tuple.Item1.AvailablePluginInfo.Identity.Version < pluginInfo.Identity.Version)
                {
                    IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                    if (fetcher == null)
                    {
                        this.logger.Error($"No fetcher is found for plugin {pluginInfo.Identity}. Skipping.");
                    }

                    var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[id] = (newPlugin, discoverer);
                }
                else if (tuple.Item1.AvailablePluginInfo.Identity.Equals(pluginInfo.Identity))
                {
                    this.logger.Warn($"Duplicate plugin {pluginInfo.Identity} is discovered from {source.Uri} by {discoverer.GetType().Name}." +
                        $"Using the first found discoverer: {tuple.Item2.GetType().Name}.");
                }
            }
        }

        private async Task ProcessDiscoverAllVersionsResult(
            IReadOnlyCollection<AvailablePluginInfo> discoveryResult,
            IPluginDiscoverer discoverer,
            PluginSource source,
            IDictionary<PluginIdentity, (AvailablePlugin, IPluginDiscoverer)> results)
        {
            foreach (AvailablePluginInfo pluginInfo in discoveryResult)
            {
                if (!results.TryGetValue(pluginInfo.Identity, out (AvailablePlugin, IPluginDiscoverer) tuple))
                {
                    IPluginFetcher fetcher = await GetPluginFetcher(pluginInfo);
                    if (fetcher == null)
                    {
                        this.logger.Error($"No fetcher is found for plugin {pluginInfo.Identity}. Skipping.");
                    }

                    var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[pluginInfo.Identity] = (newPlugin, discoverer);
                }
                else
                {
                    this.logger.Warn($"Duplicate plugin {pluginInfo.Identity} is discovered from {source.Uri} by {discoverer.GetType().Name}." +
                       $"Using the first found discoverer: {tuple.Item2.GetType().Name}.");
                } 
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager
        : IPluginsManager
    {
        private readonly PluginsManagerResourceRepository<IPluginDiscovererProvider> discovererRepository;
        private readonly PluginsManagerResourceRepository<IPluginFetcher> fetcherRepository;
        private readonly IReadonlyRepository<PluginSource> sourcesRepo;
        private readonly DiscovererSourcesManager discovererSourcesManager;

        private readonly ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;

        private readonly IPluginsInstaller pluginInstaller;
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
        /// <param name="pluginInstaller">
        ///     A <see cref="IPluginsInstaller"/> this plugin manager uses to install/unintall plugins.
        /// </param>
        /// <param name="installationDir">
        ///     The directory where the plugins will be installed to.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IEnumerable<IPluginFetcher> fetchers,
            IPluginsInstaller pluginInstaller)
            : this(
                  discovererProviders,
                  fetchers,
                  new PluginSourceRepo(),
                  pluginInstaller,
                  Logger.Create<PluginsManager>())
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
        /// <param name="pluginInstaller">
        ///     A <see cref="IPluginsInstaller"/> this plugin manager uses to install/unintall plugins.
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
            IReadonlyRepository<PluginSource> sourcesRepo,
            IPluginsInstaller pluginInstaller,
            ILogger logger)
        {
            Guard.NotNull(discovererProviders, nameof(discovererProviders));
            Guard.NotNull(fetchers, nameof(fetchers));
            Guard.NotNull(pluginInstaller, nameof(pluginInstaller));
            Guard.NotNull(logger, nameof(logger));

            this.discovererRepository = new PluginsManagerResourceRepository<IPluginDiscovererProvider>(discovererProviders);
            this.fetcherRepository = new PluginsManagerResourceRepository<IPluginFetcher>(fetchers);

            this.sourcesRepo = sourcesRepo;
            this.sourcesRepo.ItemsModified += OnSourcesChanged;
            this.discovererRepository.ResourcesAdded += OnNewProvidersAdded;

            this.pluginInstaller = pluginInstaller;
            this.logger = logger;
        }

        private async void OnNewProvidersAdded(object sender, NewResourcesEventArgs<IPluginDiscovererProvider> e)
        {
            foreach (KeyValuePair<PluginSource, List<IPluginDiscoverer>> kvp in this.sourceToDiscoverers)
            {
                kvp.Value.AddRange(await CreateDiscoverers(kvp.Key, e.NewPluginsManagerResources));
            }
        }

        private async void OnSourcesChanged(object sender, EventArgs e)
        {
            this.sourceToDiscoverers.Clear();
            foreach (PluginSource source in this.sourcesRepo.Items)
            {
                IEnumerable<IPluginDiscoverer> discoverers = await CreateDiscoverers(
                    source,
                    this.discovererRepository.Resources);

                this.sourceToDiscoverers.TryAdd(source, discoverers.ToList());
            }
        }
        /// <summary>
        ///     Creates discoverer instances for a plugin source given a collection of providers.
        /// </summary>
        /// <param name="pluginSource">
        ///     A plugin source.
        /// </param>
        /// <param name="providers">
        ///     A collection of discoverer providers.
        /// </param>
        /// <returns>
        ///     A collection of discoverers that can discover plugins from the given plugin source.
        /// </returns>
        private async Task<IEnumerable<IPluginDiscoverer>> CreateDiscoverers(
           PluginSource pluginSource,
           IEnumerable<IPluginDiscovererProvider> providers)
        {
            IList<IPluginDiscoverer> results = new List<IPluginDiscoverer>();
            foreach (IPluginDiscovererProvider provider in providers)
            {
                try
                {
                    bool isSupported = await provider.IsSupportedAsync(pluginSource);
                    if (!isSupported)
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    string errorMsg = $"Error occurred when checking if {pluginSource} is supported by discoverer provider {provider.GetType().Name}.";
                    var errorInfo = new ErrorInfo(
                        ErrorCodes.PLUGINS_MANAGER_PluginSourceException,
                        errorMsg);

                    PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo, e));

                    this.logger.Error($"{errorMsg} Skipping creating discoverers from this provider.");

                    continue;
                }

                IPluginDiscoverer discoverer = null;
                try
                {
                    discoverer = provider.CreateDiscoverer(pluginSource);
                }
                catch (Exception e)
                {
                    string errorMsg = $"Error occurred when creating discoverer for {pluginSource}.";
                    var errorInfo = new ErrorInfo(
                       ErrorCodes.PLUGINS_MANAGER_PluginSourceException,
                       errorMsg);

                    PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo, e));

                    this.logger.Error($"{errorMsg} Skipping creating discoverers from this provider.");

                    continue;
                }

                Debug.Assert(discoverer != null);

                discoverer.SetLogger(Logger.Create(discoverer.GetType()));
                results.Add(discoverer);
            }

            this.logger.Info($"{results.Count} discoverers are created for plugin source {pluginSource}");
            return results;
        }

        /// <inheritdoc />
        public event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;

        /// <inheritdoc />
        public IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.sourcesRepo.Items;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.discovererSourcesManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public async Task AddPluginSourcesAsync(IEnumerable<PluginSource> sources)
        {
            await this.discovererSourcesManager.AddPluginSourcesAsync(sources);
        }

        // TODO: #271 Re-enable when we start to support loading additional resources.
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
                .ToArray();

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

            // The below code combines plugins discovered from all plugin sources.
            // For each discovered plugin, the latest version across all sources will be returned.
            // For now, we assume:
            //      1. Duplicates (plugins with same id and version) maybe be discovered from different sources.
            //      2. In the case when duplicated "lastest" are discovered, only one of the duplicates will be returned.
            var discoveredAvailablePlugins = new List<IReadOnlyCollection<AvailablePlugin>>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyCollection<AvailablePlugin>> t = tasks[i];
                PluginSource pluginSource = pluginSources[i];

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Successfully discovered {t.Result.Count} available plugins from source {pluginSource}.");
                    discoveredAvailablePlugins.Add(t.Result);
                }
                else if (t.IsFaulted)
                {
                    this.logger.Error($"Failed to get available plugins from source {pluginSource}. Skipping.", t.Exception);
                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"The request to get lastest available plugins from source {pluginSource} is cancelled.");
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
            PluginSource pluginSource,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginSource, nameof(pluginSource));

            if (!this.PluginSources.Contains(pluginSource))
            {
                throw new InvalidOperationException("Plugin source needs to be added to the manager before being performed discovery on.");
            }

            IPluginDiscoverer[] discoverers = this.sourceToDiscoverers[pluginSource].ToArray();
            if (!discoverers.Any())
            {
                HandleResourceNotFoundError(
                    pluginSource,
                    $"No available {typeof(IPluginDiscoverer).Name} found supporting plugin source {pluginSource}.");
                return Array.Empty<AvailablePlugin>();
            }

            Task<IReadOnlyDictionary<string, AvailablePluginInfo>>[] tasks = discoverers
                .Select(d => d.DiscoverPluginsLatestAsync(cancellationToken))
                .ToArray();

            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get available plugins from {pluginSource} is cancelled.");
                throw;
            }
            catch
            {
                // Exeptions from each tasks are handled in the loop below.
            }

            var results = new Dictionary<string, (AvailablePlugin, IPluginDiscoverer)>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyDictionary<string, AvailablePluginInfo>> t = tasks[i];
                IPluginDiscoverer discoverer = discoverers[i];
                string discovererTypeStr = discoverer.GetType().Name;

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} discovered {t.Result.Count} plugins from {pluginSource}.");

                    // Combines plugins discovered from the same plugin source but by different discoverers.
                    // If more than one available plugin with the same identity are discovered, the first of them will be returned.
                    await ProcessDiscoverAllResult(task.Result[i], discoverer, pluginSource, results);
                }
                else if (t.IsFaulted)
                {
                    HandlePluginSourceException(
                        pluginSource,
                        $"Discoverer {discovererTypeStr} failed to discover plugins from {pluginSource}.",
                        t.Exception);

                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} cancelled the discovery of plugins from {pluginSource}.");
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

            Task<IReadOnlyCollection<AvailablePlugin>>[] tasks = this.PluginSources
                .Select(s => GetAllVersionsOfPluginFromSourceAsync(s, pluginIdentity, cancellationToken))
                .ToArray();

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

            IPluginDiscoverer[] discoverers = this.sourceToDiscoverers[source].ToArray();
            if (!discoverers.Any())
            {
                HandleResourceNotFoundError(
                    source,
                    $"No available {typeof(IPluginDiscoverer).Name} found supporting the plugin source {source}.");

                return Array.Empty<AvailablePlugin>();
            }

            Task<IReadOnlyCollection<AvailablePluginInfo>>[] tasks = discoverers.Select(d => d.DiscoverAllVersionsOfPluginAsync(pluginIdentity, cancellationToken)).ToArray();

            var task = Task.WhenAll(tasks);
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                this.logger.Info($"The request to get all versions of plugin {pluginIdentity} from {source} is cancelled.");
            }
            catch
            {
                // Exceptions from each tasks are handled in the loop below.
            }

            var results = new Dictionary<PluginIdentity, (AvailablePlugin, IPluginDiscoverer)>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyCollection<AvailablePluginInfo>> t = tasks[i];
                IPluginDiscoverer discoverer = discoverers[i];
                string discovererTypeStr = discoverer.GetType().Name;

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} discovered {t.Result.Count} plugins from {source}.");

                    // Combines plugins discovered from the same plugin source but by different discoverers.
                    // If more than one available plugin with the same identity are discovered, the first of them will be returned.
                    await ProcessDiscoverAllVersionsResult(t.Result, discoverer, source, results);
                }
                else if (t.IsFaulted)
                {
                    HandlePluginSourceException(
                        source,
                        $"Discoverer {discovererTypeStr} failed to discover plugins from {source}.",
                        t.Exception);

                    continue;
                }
                else if (t.IsCanceled)
                {
                    this.logger.Info($"Discoverer {discovererTypeStr} cancelled the discovery of plugins from {source}.");
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
                return await this.pluginInstaller.InstallPluginAsync(
                    stream,
                    availablePlugin.AvailablePluginInfo.PluginPackageUri,
                    cancellationToken,
                    progress);
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
                return await this.pluginInstaller.InstallPluginAsync(
                    stream,
                    new Uri(packageFullPath),
                    cancellationToken,
                    progress);
            }
        }

        /// <inheritdoc/>
        public Task<InstalledPluginsResults> GetAllInstalledPluginsAsync(CancellationToken cancellationToken)
        {
            return this.pluginInstaller.GetAllInstalledPluginsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<InstalledPluginInfo> InstallPluginAsync(
            Stream pluginStream,
            Uri sourceUri,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            return this.pluginInstaller.InstallPluginAsync(
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
            Guard.NotNull(installedPlugin, nameof(installedPlugin));

            return await this.pluginInstaller.UninstallPluginAsync(installedPlugin, cancellationToken);
        }


        private async Task<IPluginFetcher> TryGetPluginFetcher(AvailablePluginInfo availablePluginInfo)
        {
            IPluginFetcher fetcherToUse = this.fetcherRepository.Resources
                .SingleOrDefault(fetcher => fetcher.TryGetGuid() == availablePluginInfo.FetcherResourceId);

            if (fetcherToUse == null)
            {
                this.logger.Error($"Fetcher with ID {availablePluginInfo.FetcherResourceId} is not found.");
                return null;
            }

            // Validate that the found fetcher actually supports fetching the given plugin.
            Type fetcherType = fetcherToUse.GetType();
            try
            {
                bool isSupported = await fetcherToUse.IsSupportedAsync(availablePluginInfo);
                if (!isSupported)
                {
                    this.logger.Error($"Fetcher {fetcherType.Name} doesn't support fetching from {availablePluginInfo.PluginPackageUri}");
                    return null;
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"Error occurred when checking if plugin {availablePluginInfo.Identity} is supported by {fetcherType.Name}.", e);
                return null;
            }

            return fetcherToUse;
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
                    IPluginFetcher fetcher = await TryGetPluginFetcher(pluginInfo);
                    if (fetcher == null)
                    {
                        HandleResourceNotFoundError(
                            source,
                            $"No fetcher is found that supports fetching plugin {pluginInfo.Identity}.");
                        continue;
                    }

                    var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[id] = (newPlugin, discoverer);
                }
                else if (tuple.Item1.AvailablePluginInfo.Identity.Equals(pluginInfo.Identity))
                {
                    this.logger.Warn($"Duplicate plugin {pluginInfo.Identity} is discovered from {source} by {discoverer.GetType().Name}." +
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
                    IPluginFetcher fetcher = await TryGetPluginFetcher(pluginInfo);
                    if (fetcher == null)
                    {
                        HandleResourceNotFoundError(
                            source,
                            $"No fetcher is found that supports fetching plugin {pluginInfo.Identity}.");
                        continue;
                    }
                    var newPlugin = new AvailablePlugin(pluginInfo, discoverer, fetcher);
                    results[pluginInfo.Identity] = (newPlugin, discoverer);
                }
                else
                {
                    this.logger.Warn($"Duplicate plugin {pluginInfo.Identity} is discovered from {source} by {discoverer.GetType().Name}." +
                       $"Using the first found discoverer: {tuple.Item2.GetType().Name}.");
                }
            }
        }


        private void HandleResourceNotFoundError(PluginSource pluginSource, string errorMsg)
        {
            var errorInfo = new ErrorInfo(ErrorCodes.PLUGINS_MANAGER_PluginsManagerResourceNotFound, errorMsg);
            PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo));

            this.logger.Error(errorMsg);
        }

        private void HandlePluginSourceException(PluginSource pluginSource, string errorMsg, Exception exception)
        {
            var errorInfo = new ErrorInfo(ErrorCodes.PLUGINS_MANAGER_PluginSourceException, errorMsg);
            PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo, exception));

            this.logger.Error(errorMsg, exception);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery
{
    /// <inheritdoc/>
    public sealed class PluginsDiscoveryOrchestrator
        : IPluginsDiscoveryOrchestrator
    {
        private readonly IRepositoryRO<IPluginDiscovererProvider> discovererProviderRepo;
        private readonly IRepositoryRO<IPluginFetcher> fetcherRepo;
        private readonly IRepositoryRO<PluginSource> pluginSourceRepo;

        private readonly ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;
        private bool requiresDiscoverersRefresh = true;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger logger;
        private readonly Func<Type, ILogger> loggerFactory;
        private readonly IPluginValidator validator;

        /// <summary>
        ///     Creates an instance of <see cref="PluginsDiscoveryOrchestrator"/> with the given logger factory.
        /// </summary>
        /// <param name="pluginSourceRepo">
        ///     A repository containing all available <see cref="PluginSource"/>s.
        /// </param>
        /// <param name="fetcherRepo">
        ///     A repository containing all available <see cref="IPluginFetcher"/>s.
        /// </param>
        /// <param name="discovererProviderRepo">
        ///     A repository containing all available <see cref="IPluginDiscovererProvider"/>s.
        /// </param>
        /// <param name="validator">
        ///     The <see cref="IPluginValidator"/> for validating fetched plugins.
        /// </param>
        /// <param name="loggerFactory">
        ///     A factory that creates loggers for the given type.
        /// </param>
        public PluginsDiscoveryOrchestrator(
            IRepositoryRO<PluginSource> pluginSourceRepo,
            IRepositoryRO<IPluginFetcher> fetcherRepo,
            IRepositoryRO<IPluginDiscovererProvider> discovererProviderRepo,
            IPluginValidator validator,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNull(pluginSourceRepo, nameof(pluginSourceRepo));
            Guard.NotNull(fetcherRepo, nameof(fetcherRepo));
            Guard.NotNull(discovererProviderRepo, nameof(discovererProviderRepo));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.pluginSourceRepo = pluginSourceRepo;
            this.discovererProviderRepo = discovererProviderRepo;
            this.fetcherRepo = fetcherRepo;

            this.sourceToDiscoverers = new ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>>();
            this.pluginSourceRepo.CollectionChanged += OnResourcesOrPluginSourcesChanged;
            this.validator = validator;
            this.discovererProviderRepo.CollectionChanged += OnResourcesOrPluginSourcesChanged;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory(typeof(PluginsDiscoveryOrchestrator));
        }

        /// <inheritdoc />
        public event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;

        /// <inheritdoc />
        private IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.pluginSourceRepo.Items;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            await RefereshDiscoverersIfNeededAsync(cancellationToken);

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
            //      2. In the case when duplicated "latest" are discovered, only one of the duplicates will be returned.
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
                    this.logger.Info($"The request to get latest available plugins from source {pluginSource} is cancelled.");
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
                    .Select(p => new KeyValuePair<string, AvailablePlugin>(p.Info.Metadata.Identity.Id, p));

                results = results.Union(kvps)
                       .GroupBy(g => g.Key)
                       .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value)
                                                       .OrderByDescending(x => x.Info.Metadata.Identity.Version)
                                                       .ThenBy(x => x.Info.Source.Uri)
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

            await RefereshDiscoverersIfNeededAsync(cancellationToken);

            IPluginDiscoverer[] discoverers = this.sourceToDiscoverers[pluginSource].ToArray();
            if (!discoverers.Any())
            {
                HandleResourceNotFoundError(
                    pluginSource,
                    $"No available {typeof(IPluginDiscoverer).Name} found supporting plugin source {pluginSource}.");
                return Array.Empty<AvailablePlugin>();
            }

            Task<IReadOnlyCollection<AvailablePluginInfo>>[] tasks = discoverers
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

            var results = new Dictionary<string, (AvailablePlugin plugin, IPluginDiscoverer)>();
            for (int i = 0; i < tasks.Length; i++)
            {
                Task<IReadOnlyCollection<AvailablePluginInfo>> t = tasks[i];
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

            AvailablePlugin[] availablePlugins = results.Values.Select(tuple => tuple.plugin).ToArray();
            return availablePlugins.AsReadOnly();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersionsOfPluginAsync(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            await RefereshDiscoverersIfNeededAsync(cancellationToken);

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
                                .GroupBy(x => x.Info.Metadata.Identity)
                                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Info.Source.Uri)
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

            await RefereshDiscoverersIfNeededAsync(cancellationToken);

            IPluginDiscoverer[] discoverers = this.sourceToDiscoverers[source].ToArray();
            if (!discoverers.Any())
            {
                HandleResourceNotFoundError(
                    source,
                    $"No available {typeof(IPluginDiscoverer).Name} found supporting the plugin source {source}.");

                return Array.Empty<AvailablePlugin>();
            }

            Task<IReadOnlyCollection<AvailablePluginInfo>>[] tasks = discoverers.Select(
                d => d.DiscoverAllVersionsOfPluginAsync(pluginIdentity, cancellationToken)).ToArray();

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

            var results = new Dictionary<PluginIdentity, (AvailablePlugin plugin, IPluginDiscoverer)>();
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
                    await ProcessDiscoverAllVersionsResult(pluginIdentity.Id, t.Result, discoverer, source, results);
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

            AvailablePlugin[] availablePlugins = results.Values.Select(tuple => tuple.plugin).ToArray();
            return availablePlugins.AsReadOnly();
        }

        private void OnResourcesOrPluginSourcesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.semaphore.Wait();
            this.requiresDiscoverersRefresh = true;
            this.semaphore.Release();
        }

        private async Task RefreshDiscoverersAsync(CancellationToken cancellationToken)
        {
            this.sourceToDiscoverers.Clear();
            foreach (PluginSource source in this.pluginSourceRepo.Items)
            {
                IEnumerable<IPluginDiscoverer> discoverers = await CreateDiscoverers(
                    source,
                    this.discovererProviderRepo.Items,
                    cancellationToken);

                this.sourceToDiscoverers.TryAdd(source, discoverers.ToList());
            }
        }

        private async Task RefereshDiscoverersIfNeededAsync(CancellationToken cancellationToken)
        {
            await this.semaphore.WaitAsync(cancellationToken);
            if (this.requiresDiscoverersRefresh)
            {
                try
                {
                    await RefreshDiscoverersAsync(cancellationToken);
                }
                finally
                {
                    this.requiresDiscoverersRefresh = false;
                    this.semaphore.Release();
                }
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
           IEnumerable<IPluginDiscovererProvider> providers,
           CancellationToken cancellationToken)
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
                        ErrorCodes.PLUGINS_SYSTEM_PluginSourceException,
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
                       ErrorCodes.PLUGINS_SYSTEM_PluginSourceException,
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

        private async Task ProcessDiscoverAllResult(
            IReadOnlyCollection<AvailablePluginInfo> discoveryResult,
            IPluginDiscoverer discoverer,
            PluginSource source,
            IDictionary<string, (AvailablePlugin, IPluginDiscoverer)> results)
        {
            // Make sure only the latest version of each plugin is used.
            IEnumerable<AvailablePluginInfo> distinctPluginInfos = discoveryResult
                .GroupBy(availablePluginInfo => availablePluginInfo.Metadata.Identity.Id)
                .Select(group => group.OrderByDescending(pluginInfo => pluginInfo.Metadata.Identity.Version).First());

            foreach (AvailablePluginInfo availablePluginInfo in distinctPluginInfos)
            {
                string id = availablePluginInfo.Metadata.Identity.Id;
                if (!results.TryGetValue(id, out (AvailablePlugin plugin, IPluginDiscoverer discoverer) tuple) ||
                    tuple.plugin.Info.Metadata.Identity.Version < availablePluginInfo.Metadata.Identity.Version)
                {
                    IPluginFetcher fetcher = await TryGetPluginFetcher(availablePluginInfo);
                    if (fetcher == null)
                    {
                        HandleResourceNotFoundError(
                            source,
                            $"No fetcher is found that supports fetching plugin {availablePluginInfo.Metadata.Identity}.");
                        continue;
                    }

                    this.validator.IsValid(availablePluginInfo.Metadata, out ErrorInfo[] errors);

                    var newPlugin = new AvailablePlugin(availablePluginInfo, discoverer, fetcher, errors ?? Array.Empty<ErrorInfo>());
                    results[id] = (newPlugin, discoverer);
                }
                else if (tuple.plugin.Info.Metadata.Identity.Equals(availablePluginInfo.Metadata.Identity))
                {
                    this.logger.Warn($"Duplicate plugin {availablePluginInfo.Metadata.Identity} is discovered from {source} by {discoverer.GetType().Name}." +
                        $"Using the first found discoverer: {tuple.discoverer.GetType().Name}.");
                }
            }
        }

        private async Task ProcessDiscoverAllVersionsResult(
            string pluginId,
            IReadOnlyCollection<AvailablePluginInfo> discoveryResult,
            IPluginDiscoverer discoverer,
            PluginSource source,
            IDictionary<PluginIdentity, (AvailablePlugin, IPluginDiscoverer discoverer)> results)
        {
            // Make sure the result contains only the specified plugin and no duplicates are included.
            IEnumerable<AvailablePluginInfo> distinctPluginInfos = discoveryResult
                .Where(p => p.Metadata.Identity.Id.Equals(pluginId))
                .GroupBy(availablePluginInfo => availablePluginInfo.Metadata.Identity)
                .Select(group => group.First());

            foreach (AvailablePluginInfo availablePluginInfo in distinctPluginInfos)
            {
                if (!results.TryGetValue(availablePluginInfo.Metadata.Identity, out (AvailablePlugin, IPluginDiscoverer discoverer) tuple))
                {
                    IPluginFetcher fetcher = await TryGetPluginFetcher(availablePluginInfo);
                    if (fetcher == null)
                    {
                        HandleResourceNotFoundError(
                            source,
                            $"No fetcher is found that supports fetching plugin {availablePluginInfo.Metadata.Identity}.");
                        continue;
                    }

                    this.validator.IsValid(availablePluginInfo.Metadata, out ErrorInfo[] errors);

                    var newPlugin = new AvailablePlugin(availablePluginInfo, discoverer, fetcher, errors ?? Array.Empty<ErrorInfo>());
                    results[availablePluginInfo.Metadata.Identity] = (newPlugin, discoverer);
                }
                else
                {
                    this.logger.Warn($"Duplicate plugin {availablePluginInfo.Metadata.Identity} is discovered from {source} by {discoverer.GetType().Name}." +
                       $"Using the first found discoverer: {tuple.discoverer.GetType().Name}.");
                }
            }
        }

        private async Task<IPluginFetcher> TryGetPluginFetcher(AvailablePluginInfo availablePluginInfo)
        {
            IPluginFetcher fetcherToUse = this.fetcherRepo.Items
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
                    this.logger.Error($"Fetcher {fetcherType.Name} doesn't support fetching from {availablePluginInfo.PackageUri}");
                    return null;
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"Error occurred when checking if plugin {availablePluginInfo.Metadata.Identity} is supported by {fetcherType.Name}.", e);
                return null;
            }

            return fetcherToUse;
        }

        private void HandleResourceNotFoundError(PluginSource pluginSource, string errorMsg)
        {
            var errorInfo = new ErrorInfo(ErrorCodes.PLUGINS_SYSTEM_PluginsSystemResourceNotFound, errorMsg);
            PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo));

            this.logger.Error(errorMsg);
        }

        private void HandlePluginSourceException(PluginSource pluginSource, string errorMsg, Exception exception)
        {
            var errorInfo = new ErrorInfo(ErrorCodes.PLUGINS_SYSTEM_PluginSourceException, errorMsg);
            PluginSourceErrorOccured?.Invoke(this, new PluginSourceErrorEventArgs(pluginSource, errorInfo, exception));

            this.logger.Error(errorMsg, exception);
        }
    }
}

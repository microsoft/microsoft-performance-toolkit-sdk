// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery
{
    /// <summary>
    ///     Manages a mapping from plugins sources to plugin discoverers.
    /// </summary>
    internal sealed class DiscovererSourcesManager
    {
        private readonly IPluginsManagerResourceRepository<IPluginDiscovererProvider> discovererProviderRepository;
        private readonly ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of <see cref="DiscovererSourcesManager"/> with a logger;
        /// </summary>
        /// <param name="discovererProviderRepository">
        ///     A repository containing all available <see cref="IPluginDiscovererProvider" />s.
        /// </param>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        public DiscovererSourcesManager(
            IPluginsManagerResourceRepository<IPluginDiscovererProvider> discovererProviderRepository,
            ILogger logger)
        {
            Guard.NotNull(discovererProviderRepository, nameof(discovererProviderRepository));

            this.discovererProviderRepository = discovererProviderRepository;
            this.discovererProviderRepository.ResourcesAdded += OnNewProvidersAdded;

            this.sourceToDiscoverers = new ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>>();
            this.logger = logger;
        }

        /// <summary>
        ///     Creates an instance of <see cref="DiscovererSourcesManager"/>.
        /// </summary>
        /// <param name="discovererProviderRepository">
        ///     A repository containing all available <see cref="IPluginDiscovererProvider" />s.
        /// </param>
        public DiscovererSourcesManager(
            IPluginsManagerResourceRepository<IPluginDiscovererProvider> discovererProviderRepository)
            : this(discovererProviderRepository, Logger.Create<DiscovererSourcesManager>())
        {
        }

        /// <summary>
        ///     Gets all plugin sources.
        /// </summary>
        public IEnumerable<PluginSource> PluginSources
        {
            get
            {
                return this.sourceToDiscoverers.Keys;
            }
        }

        /// <summary>
        ///    Raised when an error occurs interacting with a paticular <see cref="PluginSource"/>.
        /// </summary>
        public event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;

        /// <summary>
        ///     Returns a collection of discoverers associated with a given plugin source.
        /// </summary>
        /// <param name="pluginSource">
        ///     A plugin source.
        /// </param>
        /// <returns>
        ///     A collection of discoverers that are capable of discovering plugins for the given <paramref name="pluginSource"/>.
        /// </returns>
        public IEnumerable<IPluginDiscoverer> GetDiscoverersFromSource(PluginSource pluginSource)
        {
            Guard.NotNull(pluginSource, nameof(pluginSource));

            if (this.sourceToDiscoverers.TryGetValue(pluginSource, out List<IPluginDiscoverer> disoverers))
            {
                return disoverers;
            }

            return Array.Empty<IPluginDiscoverer>();
        }

        /// <summary>
        ///     Clears all plugin sources and their discoverers.
        /// </summary>
        public void ClearPluginSources()
        {
            this.sourceToDiscoverers.Clear();
        }

        /// <summary>
        ///     Adds a collection of plugin sources to this discoverers manager and creates discoverers for each source.
        /// </summary>
        /// <param name="pluginSources">
        ///     The plugin sources to be added.
        /// </param>
        /// <returns>
        ///     A task that completes when all plugin sources have been added.
        /// </returns>
        public async Task AddPluginSources(IEnumerable<PluginSource> pluginSources)
        {
            Guard.NotNull(pluginSources, nameof(pluginSources));

            foreach (PluginSource source in pluginSources)
            {
                if (this.sourceToDiscoverers.ContainsKey(source))
                {
                    continue;
                }

                IEnumerable<IPluginDiscoverer> discoverers = await CreateDiscoverers(
                    source,
                    this.discovererProviderRepository.Resources);

                this.sourceToDiscoverers.TryAdd(source, discoverers.ToList());
            }
        }

        /// <summary>
        ///     An event handler that is called to create new <see cref="IPluginDiscoverer">s when 
        ///     new <see cref="IPluginDiscovererProvider"> are added to the repository. 
        /// </summary>
        /// <param name="sender">
        ///     The object that raises the event.
        /// </param>
        /// <param name="e">
        ///     Event args containing the newly added discoverer providers.
        /// </param>
        private async void OnNewProvidersAdded(object sender, NewResourcesEventArgs<IPluginDiscovererProvider> e)
        {
            foreach (KeyValuePair<PluginSource, List<IPluginDiscoverer>> kvp in this.sourceToDiscoverers)
            {
                kvp.Value.AddRange(await CreateDiscoverers(kvp.Key, e.NewPluginsManagerResources));
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
    }
}

﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery
{
    /// <summary>
    ///     Manages a mapping from plugins sources to plugin discoverers.
    /// </summary>
    public sealed class DiscoverersManager : IDisposable
    {
        private readonly ResourceRepository<IPluginDiscovererProvider> repository;
        private readonly DiscoverersFactory discoverersFactory;
        private readonly ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>> sourceToDiscoverers;

        /// <summary>
        ///     Creates an instance of <see cref="DiscoverersManager"/>.
        /// </summary>
        /// <param name="discovererProviderRepository">
        ///     A repository containing all available <see cref="IPluginDiscovererProvider" />s.
        /// </param>
        /// <param name="discoverersFactory">
        ///     A factory for creating <see cref="IPluginDiscoverer" /> instances.
        /// </param>
        public DiscoverersManager(
            ResourceRepository<IPluginDiscovererProvider> discovererProviderRepository,
            DiscoverersFactory discoverersFactory) 
        {
            this.repository = discovererProviderRepository;
            this.repository.ResourcesAdded += OnNewProvidersAdded;
            this.discoverersFactory = discoverersFactory;
            this.sourceToDiscoverers = new ConcurrentDictionary<PluginSource, List<IPluginDiscoverer>>();
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
        ///     Returns a collection of discoverers associated with a given plugin source.
        /// </summary>
        /// <param name="source">
        ///     A plugin source.
        /// </param>
        /// <returns>
        ///     A collection of discoverers that are capable of discovering plugins for the given <paramref name="source"/>.
        /// </returns>
        public IEnumerable<IPluginDiscoverer> GetDiscoverersFromSource(PluginSource source)
        {
            Guard.NotNull(source, nameof(source));

            if (this.sourceToDiscoverers.TryGetValue(source, out List<IPluginDiscoverer> disoverers))
            {
                return disoverers;
            }

            return Array.Empty<IPluginDiscoverer>();
        }

        /// <summary>
        ///     Gets a single discoverer (if any) associated with a given plugin source and resource ID.
        /// </summary>
        /// <param name="source">
        ///     A plugin source.
        /// </param>
        /// <param name="resourceId">
        ///     A resource ID that identifies a unique <see cref="IPluginManagerResource"/>.
        /// </param>
        /// <returns>
        ///     A single discoverer if there is a match or <c>null</c> otherwise.
        /// </returns>
        public IPluginDiscoverer GetDiscovererFromSourceAndId(PluginSource source, Guid resourceId)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(resourceId, nameof(resourceId));

            return GetDiscoverersFromSource(source).SingleOrDefault(d => d.DiscovererResourceId == resourceId);
        }

        /// <summary>
        ///     Clears all plugin sources and their discoverers.
        /// </summary>
        public void ClearPluginSources()
        {
            this.sourceToDiscoverers.Clear();
        }

        /// <summary>
        ///     Adds a collection of plugin sources to this discoverers manager.
        /// </summary>
        /// <param name="sources">
        ///     The plugin sources to be added.
        /// </param>
        public async void AddPluginSources(IEnumerable<PluginSource> sources)
        {
            Guard.NotNull(sources, nameof(sources));

            foreach (PluginSource source in sources)
            {
                if (this.sourceToDiscoverers.ContainsKey(source))
                {
                    continue;
                }

                IEnumerable<IPluginDiscoverer> discoverers = await this.discoverersFactory.CreateDiscoverers(source, this.repository.PluginResources);
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
                kvp.Value.AddRange(await this.discoverersFactory.CreateDiscoverers(kvp.Key, e.NewPluginResources));
            }
        }

        /// <summary>
        ///     Dispose resources held by this class.
        /// </summary>
        public void Dispose()
        {
            this.repository.ResourcesAdded -= OnNewProvidersAdded;
        }
    }
}

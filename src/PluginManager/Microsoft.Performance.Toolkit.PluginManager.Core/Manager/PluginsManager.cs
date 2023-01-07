// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager : IPluginsManager
    {
        private readonly DiscovererSourceCollection discovererSourceCollection;
        private readonly ISet<IPluginSource> pluginSources;
        private readonly Dictionary<IPluginDiscovererSource, List<IPluginDiscoverer>> discoverers;
        private readonly IEnumerable<ICredentialProvider> credentialProviders;

        public PluginsManager(
            DiscovererSourceCollection pluginDiscovererSources,
            IEnumerable<ICredentialProvider> credentialProviders)
        {
            this.credentialProviders = credentialProviders;
            this.pluginSources = new HashSet<IPluginSource>();
            this.discoverers = new Dictionary<IPluginDiscovererSource, List<IPluginDiscoverer>>();
            this.discovererSourceCollection = new DiscovererSourceCollection();

            foreach (IPluginDiscovererSource<IPluginSource> discovererSource in pluginDiscovererSources.AllPluginDiscovererSources)
            {
                discovererSource.SetupCredentialService(credentialProviders);
                this.discoverers[discovererSource] = new List<IPluginDiscoverer>();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IPluginSource> PluginSources
        { 
            get
            {
                return this.pluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.pluginSources.Clear();

            foreach (List<IPluginDiscoverer> discoveres in this.discoverers.Values)
            {
                discoveres.Clear();
            }
        }

        /// <inheritdoc />
        public void AddPluginSource<TSource>(TSource source) where TSource : class, IPluginSource
        {
            Guard.NotNull(source, nameof(source));

            this.pluginSources.Add(source);

            foreach (IPluginDiscovererSource<TSource> discovererSource in  this.discovererSourceCollection.Get<TSource>())
            {
                if (discovererSource.IsSourceSupported(source))
                {
                    this.discoverers[discovererSource].Add(discovererSource.CreateDiscoverer(source));
                }
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            var results = new List<IAvailablePlugin>();

            foreach (IPluginDiscoverer discovererEndpoint in this.discoverers.Values.SelectMany(x => x))
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await discovererEndpoint.DiscoverPluginsLatestAsync(cancellationToken);
                results.AddRange(plugins);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            foreach (IPluginDiscoverer discovererEndpoint in this.discoverers.Values.SelectMany(x => x))
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await discovererEndpoint.DiscoverAllVersionsOfPlugin(
                    pluginIdentity,
                    cancellationToken);

                if (plugins.Any())
                {
                    return plugins;
                }
            }

            return Array.Empty<IAvailablePlugin>();
        }
    }
}

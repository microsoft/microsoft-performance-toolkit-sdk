// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManagerImpl : IPluginsManager
    {
        private readonly ISet<UriPluginSource> pluginSources;
        private readonly Dictionary<IPluginDiscoverer<UriPluginSource>, List<IDiscovererEndpoint>> discovererToEndpoints;
        private readonly ICrendentialProvider crendentialProvider;

        public PluginsManagerImpl(ISet<IPluginDiscoverer<UriPluginSource>> pluginDiscoverers)
        {
            this.crendentialProvider = new MockCredentialProvider();
            this.pluginSources = new HashSet<UriPluginSource>();
            this.discovererToEndpoints = new Dictionary<IPluginDiscoverer<UriPluginSource>, List<IDiscovererEndpoint>>();

            foreach (IPluginDiscoverer<UriPluginSource> discoverer in pluginDiscoverers)
            {
                this.discovererToEndpoints[discoverer] = new List<IDiscovererEndpoint>();
            }
        }

        public IEnumerable<UriPluginSource> PluginSources { get { return this.pluginSources; } }

        public void SetPluginSources(IEnumerable<UriPluginSource> pluginSources)
        {
            this.pluginSources.Clear();
            this.pluginSources.UnionWith( pluginSources );

            AssignSourcesToDiscoverer();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestAsync()
        {
            var results = new List<IAvailablePlugin>();

            foreach (IDiscovererEndpoint discovererEndpoint in this.discovererToEndpoints.Values.SelectMany(x => x))
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await discovererEndpoint.DiscoverPluginsLatestAsync();
                results.AddRange(plugins);
            }

            return results;
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            foreach (IDiscovererEndpoint discovererEndpoint in this.discovererToEndpoints.Values.SelectMany(x => x))
            {
                IReadOnlyCollection<IAvailablePlugin> plugins = await discovererEndpoint.DiscoverAllVersionsOfPlugin(pluginIdentity);
                if (plugins.Any())
                {
                    return plugins;
                }
            }

            return Array.Empty<IAvailablePlugin>();
        }

        private void AssignSourcesToDiscoverer()
        {
            foreach (IPluginDiscoverer<UriPluginSource> discoverer in this.discovererToEndpoints.Keys)
            {
                List<IDiscovererEndpoint> endpoints = this.discovererToEndpoints[discoverer];
                endpoints.Clear();

                foreach (UriPluginSource source in this.pluginSources)
                {
                    if (discoverer.IsSourceSupported(source))
                    {
                        IDiscovererEndpoint endpoint = discoverer.CreateDiscovererEndpoint(source, this.crendentialProvider);
                        endpoints.Add(endpoint);
                    }
                }
            }
        }
    }
}

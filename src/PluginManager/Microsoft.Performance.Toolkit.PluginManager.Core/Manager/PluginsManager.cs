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
    /// <summary>
    ///     TODO: Update - right now is just a simplified plugin manager to showcase how discoveres are orchestrated.
    ///     This only supports discovering <see cref="UriPluginSource"/>. We should probably just get rid of generic.
    ///     Crendetials can be ignored.
    /// </summary>
    public sealed class PluginsManagerImpl : IPluginsManager
    {
        private readonly ISet<UriPluginSource> pluginSources;
        private readonly Dictionary<IPluginDiscovererSource<UriPluginSource>, List<IPluginDiscoverer>> discoverers;
        private readonly IEnumerable<ICredentialProvider> credentialProviders;

        public PluginsManagerImpl(
            ISet<IPluginDiscovererSource<UriPluginSource>> pluginDiscovererSources,
            IEnumerable<ICredentialProvider> credentialProviders)
        {
            this.credentialProviders = credentialProviders;
            this.pluginSources = new HashSet<UriPluginSource>();
            this.discoverers = new Dictionary<IPluginDiscovererSource<UriPluginSource>, List<IPluginDiscoverer>>();

            foreach (IPluginDiscovererSource<UriPluginSource> discovererSource in pluginDiscovererSources)
            {
                discovererSource.SetupCredentialService(credentialProviders);
                this.discoverers[discovererSource] = new List<IPluginDiscoverer>();
            }
        }

        public IEnumerable<UriPluginSource> PluginSources { get { return this.pluginSources; } }

        public void SetPluginSources(IEnumerable<UriPluginSource> pluginSources)
        {
            this.pluginSources.Clear();
            this.pluginSources.UnionWith( pluginSources );

            CreateDiscoverers();
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

        private void CreateDiscoverers()
        {
            foreach (IPluginDiscovererSource<UriPluginSource> discovererSource in this.discoverers.Keys)
            {
                List<IPluginDiscoverer> discoverer = this.discoverers[discovererSource];
                discoverer.Clear();

                foreach (UriPluginSource source in this.pluginSources)
                {
                    if (discovererSource.IsSourceSupported(source))
                    {
                        IPluginDiscoverer endpoint = discovererSource.CreateDiscoverer(source);
                        discoverer.Add(endpoint);
                    }
                }
            }
        }
    }
}

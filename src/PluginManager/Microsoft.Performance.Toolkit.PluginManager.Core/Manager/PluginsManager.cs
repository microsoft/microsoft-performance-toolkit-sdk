// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Manager
{
    /// <inheritdoc />
    public sealed class PluginsManager : IPluginsManager
    {
        private readonly DiscoveryManager discoveryManager;
        public PluginsManager(
            IEnumerable<IPluginDiscovererSource> discovererSources,
            IDiscovererSourceLoader discovererSourceLoader)
        {
            this.discoveryManager = new DiscoveryManager(discovererSources, discovererSourceLoader);
        }

        /// <inheritdoc />
        public IEnumerable<IPluginSource> PluginSources
        { 
            get
            {
                return this.discoveryManager.PluginSources;
            }
        }

        /// <inheritdoc />
        public void ClearPluginSources()
        {
            this.discoveryManager.ClearPluginSources();
        }

        /// <inheritdoc />
        public void AddPluginSource<TSource>(TSource source) where TSource : class, IPluginSource
        {
            this.discoveryManager.AddPluginSource(source);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IAvailablePlugin>> GetAvailablePluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            return this.discoveryManager.GetAllAvailablePluginsLatestAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<IAvailablePlugin>> GetAllVersionsOfPlugin(
            IAvailablePlugin availablePlugin,
            CancellationToken cancellationToken)
        {
            return this.discoveryManager.GetAllVersionsOfPlugin(availablePlugin, cancellationToken);
        }

        public void LoadAdditionalDiscovererSources(string directory)
        {
            this.discoveryManager.LoadAdditionalDiscovererSource(directory);
        }
    }
}

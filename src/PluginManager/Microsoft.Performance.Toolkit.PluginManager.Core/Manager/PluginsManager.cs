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
        
        /// <summary>
        ///     Initializes a plugin manager instance.
        /// </summary>
        /// <param name="discovererProviders">
        ///     The known providers that have already been loaded or intialized directly.
        /// </param>
        /// <param name="discovererSourceLoader">
        ///     A loader that can load additional discoverer providers at run time.
        /// </param>
        public PluginsManager(
            IEnumerable<IPluginDiscovererProvider> discovererProviders,
            IDiscovererProvidersLoader discovererSourceLoader)
        {
            this.discoveryManager = new DiscoveryManager(discovererProviders, discovererSourceLoader);
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

        /// <inheritdoc />
        public void LoadAdditionalDiscovererSources(string directory)
        {
            this.discoveryManager.LoadAdditionalDiscovererSource(directory);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery
{
    /// <summary>
    ///    Creates discoverer instances for a plugin source given a collection of providers.
    /// </summary>
    public sealed class DiscoverersFactory
    {
        private readonly ILogger logger;

        /// <summary>
        ///     Creates an instance of<see cref="DiscoverersFactory"/>.
        /// </summary>
        public DiscoverersFactory()
            : this(Logger.Create<DiscoverersFactory>())
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="DiscoverersFactory"/> with a <see cref="ILogger"/>.
        /// </summary>
        /// <param name="logger"></param>
        public DiscoverersFactory(ILogger logger)
        {
            Guard.NotNull(logger, nameof(logger));

            this.logger = logger;
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
        public async Task<IEnumerable<IPluginDiscoverer>> CreateDiscoverers(
            PluginSource pluginSource,
            IEnumerable<IPluginDiscovererProvider> providers)
        {
            IList<IPluginDiscoverer> results = new List<IPluginDiscoverer>();
            foreach (IPluginDiscovererProvider provider in providers)
            {
                try
                {
                    if (await provider.IsSupportedAsync(pluginSource))
                    {
                        IPluginDiscoverer discoverer = provider.CreateDiscoverer(pluginSource);
                        discoverer.SetLogger(Logger.Create(discoverer.GetType()));
                        results.Add(discoverer);
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error($"Error occured from discoverer provider {provider.GetType().Name}. Skipping creating discoverers from this provider.", e);
                    continue;
                }
            }

            this.logger.Info($"{results.Count} discoverers are created for plugin source {pluginSource.Uri}");
            return results;
        }
    }
}

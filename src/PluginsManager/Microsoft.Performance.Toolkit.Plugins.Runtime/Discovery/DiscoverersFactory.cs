﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery
{
    /// <summary>
    ///    Creates discoverer instances for a plugin source given a collection of providers.
    /// </summary>
    public sealed class DiscoverersFactory
    {
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
                if (await provider.IsSupportedAsync(pluginSource))
                {
                    IPluginDiscoverer discoverer = provider.CreateDiscoverer(pluginSource);
                    discoverer.SetLogger(Logger.Create(provider.GetType()));
                    results.Add(discoverer);
                }
            }

            return results;
        }
    }
}

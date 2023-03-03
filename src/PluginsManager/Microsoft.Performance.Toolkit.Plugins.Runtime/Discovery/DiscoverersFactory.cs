// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Events;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery
{
    /// <summary>
    ///    Creates discoverer instances for a plugin source given a collection of providers.
    /// </summary>
    internal sealed class DiscoverersFactory
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
        ///    Raised when an error occurs interacting with a paticular <see cref="PluginSource"/>.
        /// </summary>
        public event EventHandler<PluginSourceErrorEventArgs> PluginSourceErrorOccured;

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

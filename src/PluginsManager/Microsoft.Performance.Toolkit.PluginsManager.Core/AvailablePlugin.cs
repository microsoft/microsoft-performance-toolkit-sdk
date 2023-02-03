// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Transport;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core
{
    /// <summary>
    ///     Represents a discovered plugin that is available for installation.
    /// </summary>
    public sealed class AvailablePlugin
    {
        private readonly IPluginDiscoverer pluginDiscoverer;
        private readonly IPluginFetcher pluginFetcher;

        /// <summary>
        ///     Creates an instance of <see cref="AvailablePlugin"/>
        /// </summary>
        /// <param name="pluginInfo">
        ///     The <see cref="AvailablePluginInfo"/> object cotaining information about this plugin.
        /// </param>
        /// <param name="discoverer">
        ///     The <see cref="IPluginDiscoverer"/> this plugin is discovered by.
        /// </param>
        /// <param name="fetcher">
        ///     The <see cref="IPluginFetcher"/> this plugin uses to fetch plugin content.
        /// </param>
        public AvailablePlugin(
            AvailablePluginInfo pluginInfo,
            IPluginDiscoverer discoverer,
            IPluginFetcher fetcher)
        {
            this.AvailablePluginInfo = pluginInfo;
            this.pluginDiscoverer = discoverer;
            this.pluginFetcher = fetcher;
        }

        /// <summary>
        ///     Gets the <see cref="AvailablePluginInfo"/> associated with this plugin.
        /// </summary>
        public AvailablePluginInfo AvailablePluginInfo { get; }


        /// <summary>
        ///     Gets the metadata of this plugin.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The metadata of a plugin.
        /// </returns>
        public Task<PluginMetadata> GetPluginMetadata(CancellationToken cancellationToken)
        {
            return this.pluginDiscoverer.GetPluginMetadataAsync(this.AvailablePluginInfo.Identity, cancellationToken);
        }

        /// <summary>
        ///     Gets all available versions of this plugin.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///      A collection of available plugins.
        /// </returns>
        public async Task<IReadOnlyCollection<AvailablePlugin>> GetAllVersions(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<AvailablePluginInfo> pluginInfos = await this.pluginDiscoverer.DiscoverAllVersionsOfPlugin(
                this.AvailablePluginInfo.Identity, cancellationToken);

            return pluginInfos.Select(info => new AvailablePlugin(info, this.pluginDiscoverer, this.pluginFetcher)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin installation.
        /// </param>
        /// <returns></returns>
        public Task<Stream> GetPluginPackageStream(CancellationToken cancellationToken, IProgress<int> progress)
        {
            return this.pluginFetcher.GetPluginStreamAsync(this.AvailablePluginInfo, cancellationToken, progress);
        }
    }
}

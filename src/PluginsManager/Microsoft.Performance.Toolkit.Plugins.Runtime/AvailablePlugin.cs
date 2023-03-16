// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
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

        public AvailablePlugin(
            AvailablePluginInfo pluginInfo,
            IPluginDiscoverer discoverer)
        {
            this.AvailablePluginInfo = pluginInfo;
            this.pluginDiscoverer = discoverer;
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

        internal Task<Stream> GetPluginPackageStream(CancellationToken cancellationToken, IProgress<int> progress)
        {
            return this.pluginFetcher.GetPluginStreamAsync(
                this.AvailablePluginInfo,
                cancellationToken,
                progress);
        }
    }
}

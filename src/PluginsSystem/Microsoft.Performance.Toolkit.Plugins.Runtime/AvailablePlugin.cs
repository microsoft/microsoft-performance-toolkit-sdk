﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a discovered plugin that is available for installation.
    /// </summary>
    public sealed class AvailablePlugin
    {
        private readonly IPluginDiscoverer discoverer;
        private readonly IPluginFetcher fetcher;

        /// <summary>
        ///     Creates an instance of <see cref="AvailablePlugin"/>
        /// </summary>
        /// <param name="info">
        ///     The <see cref="Info"/> object cotaining information about this plugin.
        /// </param>
        /// <param name="discoverer">
        ///     The <see cref="IPluginDiscoverer"/> this plugin is discovered by.
        /// </param>
        /// <param name="fetcher">
        ///     The <see cref="IPluginFetcher"/> this plugin uses to fetch plugin content.
        /// </param>
        internal AvailablePlugin(
            AvailablePluginInfo info,
            IPluginDiscoverer discoverer,
            IPluginFetcher fetcher)
        {
            this.Info = info;
            this.discoverer = discoverer;
            this.fetcher = fetcher;
        }

        /// <summary>
        ///     Gets the <see cref="Core.AvailablePluginInfo"/> associated with this plugin.
        /// </summary>
        public AvailablePluginInfo Info { get; }

        /// <summary>
        ///     Gets the contents metadata of this plugin.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     The contents metadata of a plugin.
        /// </returns>
        public Task<PluginContentsMetadata> GetContentsMetadata(CancellationToken cancellationToken)
        {
            return this.discoverer.GetPluginContentsMetadataAsync(this.Info.Metadata.Identity, cancellationToken);
        }

        /// <summary>
        ///     Gets stream to this plugin that can be installed via
        ///     <see cref="Installation.IPluginsInstaller.InstallPluginAsync(Stream, Uri, CancellationToken, IProgress{int})"/>.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of plugin package fetching.
        /// </param>
        /// <returns>
        ///     The stream of the plugin package file.
        /// </returns>
        public Task<Stream> GetPluginPackageStream(CancellationToken cancellationToken, IProgress<int> progress)
        {
            return this.fetcher.GetPluginStreamAsync(
                this.Info,
                cancellationToken,
                progress);
        }
    }
}

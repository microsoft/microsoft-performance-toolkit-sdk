﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging;
using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata;
using NuGet.Common;
using NuGet.Protocol.Core.Types;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public sealed class NuGetAvailablePlugin : IAvailablePlugin
    {
        private readonly AsyncLazy<NuGetDownloader> downloader;
        private readonly ILogger logger;
        private PluginMetadata pluginMetadata;
        private PluginPackage pluginPackage;

        public NuGetAvailablePlugin(
            PluginIdentity identity,
            string displayName,
            string description,
            Uri sourceUri,
            SourceRepository sourceRepository,
            ILogger logger)
        {
            this.Identity = identity;
            this.Info = new PluginInfo()
            {
                DisplayName = displayName,
                Description = description,
                SourceUri = sourceUri,
            };
            
            this.downloader = new AsyncLazy<NuGetDownloader>(
                async () =>
                {
                    DownloadResource downloadResource = await sourceRepository.GetResourceAsync<DownloadResource>();
                    return new NuGetDownloader(downloadResource, logger);
                });
            this.logger = logger;
        }

        public PluginIdentity Identity { get; }

        public PluginInfo Info { get; }

        public async Task<PluginMetadata> GetPluginMetadataAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken,
            bool force = false)
        {
            if (this.pluginMetadata != null && !force)
            {
                return this.pluginMetadata;
            }

            await GetPluginPackageStreamAsync(progress, cancellationToken);

            return this.pluginMetadata;
        }

        public async Task<PluginPackage> GetPluginPackageAsync(
           IProgress<int> progress,
           CancellationToken cancellationToken,
           bool force = false)
        {

            if (this.pluginPackage != null && !force)
            {
                return this.pluginPackage;
            }

            await GetPluginPackageStreamAsync(progress, cancellationToken);

            return this.pluginPackage;
        }

        public async Task<Stream> GetPluginPackageStreamAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            NuGetDownloader downloader = await this.downloader;
            Stream downloadedStream = await downloader.DownloadPluginAsync(this.Identity, progress, cancellationToken);

            if (downloadedStream != null)
            {
                this.pluginPackage = new PluginPackage(downloadedStream);
                this.pluginMetadata = this.pluginPackage.PluginMetadata;
            }

            return downloadedStream;
        }
    }
}

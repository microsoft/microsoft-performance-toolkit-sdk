﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Packaging.Metadata;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ILogger = NuGet.Common.ILogger;
using IPluginDiscoverer = Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery.IPluginDiscoverer;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.NuGet
{
    public sealed class NuGetPluginDiscoverer : IPluginDiscoverer
    {
        private readonly PluginSource pluginSource;
        private readonly PackageSource packageSource;
        private readonly SourceRepository repository;
        private readonly ILogger logger;
        private readonly Lazy<ICredentialProvider> credentialProvider;

        public static readonly Guid FetcherResourceId = Guid.Parse(PluginManagerConstants.NuGetFetcherId);
        public static readonly int PageCount = 20;

        public NuGetPluginDiscoverer(
            PluginSource pluginSource,
            Lazy<ICredentialProvider> credentialProvider)
        {
            this.pluginSource = pluginSource;
            this.packageSource = new PackageSource(pluginSource.Uri.ToString());
            this.logger = NullLogger.Instance;
            this.repository = Repository.Factory.GetCoreV3(this.packageSource.Source);
            this.credentialProvider = credentialProvider;
        }

        public async Task<IReadOnlyCollection<AvailablePluginInfo>> DiscoverAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            PackageMetadataResource metadataResource = await this.repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            var result = new List<AvailablePluginInfo>();

            using (var sourceCacheContext = new SourceCacheContext())
            {
                IEnumerable<IPackageSearchMetadata> versions = await metadataResource?.GetMetadataAsync(
                    pluginIdentity.Id,
                    false,
                    false,
                    sourceCacheContext,
                    this.logger,
                    cancellationToken
                );

                versions = versions.OrderByDescending(p => p.Identity.Version, VersionComparer.Default);
                foreach (IPackageSearchMetadata packageMetadata in versions)
                {
                    var pluginId = new PluginIdentity(packageMetadata.Identity.Id, packageMetadata.Identity.Version.Version);

                    // TODO: #249 serialize/deserialize - fetcher resource id and package uri should be provided by nuget
                    var plugin = new AvailablePluginInfo(
                        pluginId,
                        this.pluginSource,
                        packageMetadata.Title,
                        packageMetadata.Description,
                        this.pluginSource.Uri,
                        FetcherResourceId);

                    result.Add(plugin);
                }
            }

            return result;
        }

        public async Task<IReadOnlyDictionary<string, AvailablePluginInfo>> DiscoverPluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            PackageSearchResource searchResource = await this.repository.GetResourceAsync<PackageSearchResource>(cancellationToken);

            var result = new Dictionary<string, AvailablePluginInfo>();
            IEnumerable<IPackageSearchMetadata> packages = await searchResource.SearchAsync(null, new SearchFilter(false), 0, PageCount, this.logger, cancellationToken);

            int sourceSearchPackageCount = 0;
            foreach (IPackageSearchMetadata packageMetadata in packages)
            {
                sourceSearchPackageCount++;

                if (result.Count >= PageCount)
                {
                    break;
                }

                var pluginIdentity = new PluginIdentity(packageMetadata.Identity.Id, packageMetadata.Identity.Version.Version);

                // TODO: #249 serialize/deserialize - fetcher resource id and package uri should be provided by nuget
                var plugin = new AvailablePluginInfo(
                        pluginIdentity,
                        this.pluginSource,
                        packageMetadata.Title,
                        packageMetadata.Description,
                        this.pluginSource.Uri,
                        FetcherResourceId);

                result.Add(pluginIdentity.Id, plugin);
            }

            return result;
        }

        public async Task<PluginMetadata> GetPluginMetadataAsync(PluginIdentity pluginIdentity, CancellationToken cancellationToken)
        {
            // TODO: #249
            // Cannot get the metadata from nuget discoverer without downloading the package.
            // Returns empty metadata for now
            return new PluginMetadata();
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ICredentialService = Microsoft.Performance.Toolkit.PluginManager.Core.Credential.ICredentialService;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public sealed class NuGetPluginDiscoverer : IPluginDiscoverer
    {
        private readonly UriPluginSource pluginSource;
        private readonly PackageSource packageSource;
        private readonly ICredentialService credentialService;
        private readonly SourceRepository repository;
        private readonly ILogger logger;

        public static readonly int PageCount = 20;

        public NuGetPluginDiscoverer(UriPluginSource pluginSource) 
            : this(pluginSource, null)
        {
        }

        public NuGetPluginDiscoverer(UriPluginSource pluginSource, ICredentialService credentialService)
        {
            this.pluginSource = pluginSource;
            this.credentialService = credentialService;
            this.logger = new NullLogger();
            this.repository = Repository.Factory.GetCoreV3(this.packageSource.Source);
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverAllVersionsOfPlugin(
            PluginIdentity pluginIdentity,
            CancellationToken cancellationToken)
        {
            PackageMetadataResource metadataResource = await this.repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            var result = new List<IAvailablePlugin>();

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

                    var plugin = new NuGetAvailablePlugin(
                        pluginId,
                        packageMetadata.Title,
                        packageMetadata.Description,
                        this.pluginSource.Uri,
                        this.repository,
                        this.logger);

                    result.Add(plugin);
                }
            }

            return result;
        }

        public async Task<IReadOnlyCollection<IAvailablePlugin>> DiscoverPluginsLatestAsync(
            CancellationToken cancellationToken)
        {
            PackageSearchResource searchResource = await this.repository.GetResourceAsync<PackageSearchResource>(cancellationToken);

            var result = new List<IAvailablePlugin>();
            IEnumerable<IPackageSearchMetadata> packages = await searchResource.SearchAsync(null, new SearchFilter(false), 0, PageCount, this.logger, cancellationToken);

            int sourceSearchPackageCount = 0;
            foreach (IPackageSearchMetadata packageMetadata in packages)
            {
                sourceSearchPackageCount++;

                if (result.Count >= PageCount)
                {
                    break;
                }

                //TODO: Validate the package contains a plugin

                var pluginIdentity = new PluginIdentity(packageMetadata.Identity.Id, packageMetadata.Identity.Version.Version);

                var plugin = new NuGetAvailablePlugin(
                    pluginIdentity,
                    packageMetadata.Title,
                    packageMetadata.Description,
                    this.pluginSource.Uri,
                    this.repository,
                    this.logger);

                result.Add(plugin);
            }

            return result;
        }
    }
}

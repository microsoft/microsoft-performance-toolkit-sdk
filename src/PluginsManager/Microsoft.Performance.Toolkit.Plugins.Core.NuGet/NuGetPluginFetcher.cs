// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Core.NuGet
{
    [PluginManagerResource(PluginsManagerConstants.NuGetFetcherId)]
    public sealed class NuGetPluginFetcher
        : IPluginFetcher
    {
        public async Task<bool> IsSupportedAsync(AvailablePluginInfo plugin, SDK.Processing.ILogger logger)
        {
            Guard.NotNull(plugin, nameof(plugin));

            var nugetSource = new PackageSource(plugin.PluginSource.Uri.ToString());

            // Support http V3 and local feed as of of now
            bool isSupported =  IsHttpV3Feed(nugetSource) || nugetSource.IsLocal;

            return isSupported;
        }

        public async Task<Stream> GetPluginStreamAsync(
            AvailablePluginInfo plugin,
            SDK.Processing.ILogger logger,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            PluginIdentity pluginIdentity = plugin.Identity;
            PluginSource pluginSource = plugin.PluginSource;

            var nugetSource = new PackageSource(pluginSource.Uri.ToString());
            SourceRepository sourceRepository = Repository.Factory.GetCoreV3(nugetSource);
            DownloadResource downloadResource = await sourceRepository.GetResourceAsync<DownloadResource>();


            var nugetPackageIdentity = new PackageIdentity(pluginIdentity.Id, new NuGetVersion(pluginIdentity.Version));
            Stream pluginPackageStream = null;

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var context = new PackageDownloadContext(sourceCacheContext, Path.GetTempPath(), true);
                using (DownloadResourceResult result = await downloadResource.GetDownloadResourceResultAsync(nugetPackageIdentity, context, string.Empty, NullLogger.Instance, cancellationToken))
                {
                    if (result.Status == DownloadResourceResultStatus.Cancelled)
                    {
                        throw new OperationCanceledException();
                    }
                    else if (result.Status == DownloadResourceResultStatus.NotFound)
                    {
                        throw new InvalidOperationException($"Package '{pluginIdentity.Id}-v{pluginIdentity.Version}' not found");
                    }

                    PackageReaderBase reader = result.PackageReader;
                    IEnumerable<string> packages = await reader.GetFilesAsync(cancellationToken);
                    string pluginFilePath = packages.FirstOrDefault(p => Path.GetExtension(p) == ".plugin");

                    if (pluginFilePath == null)
                    {
                        return null;
                    }

                    Stream stream = await reader.GetStreamAsync(pluginFilePath, cancellationToken);
                    pluginPackageStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                    stream.CopyTo(pluginPackageStream);
                    pluginPackageStream.Position = 0;
                }

                return pluginPackageStream;
            }
        }

        private static bool IsHttpV3Feed(PackageSource packageSource)
        {
            return packageSource.IsHttp &&
              (packageSource.Source.EndsWith("index.json", StringComparison.OrdinalIgnoreCase)
              || packageSource.ProtocolVersion == 3);
        }
    }
}
  
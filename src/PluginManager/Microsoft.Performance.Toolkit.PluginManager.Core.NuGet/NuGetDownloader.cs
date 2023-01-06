using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Transport;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public class NuGetDownloader : IPluginDownloader
    {
        private readonly DownloadResource downloadResource;
        private readonly ILogger logger;

        public NuGetDownloader(DownloadResource downloadResource, ILogger logger)
        {
            this.downloadResource = downloadResource;
        }

        public async Task<Stream> DownloadPluginAsync(
            PluginIdentity identity,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var nugetPackageIdentity = new PackageIdentity(identity.Id, new NuGetVersion(identity.Version));
            Stream pluginPackageStream = null;

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var context = new PackageDownloadContext(sourceCacheContext, Path.GetTempPath(), true);
                using (DownloadResourceResult result = await this.downloadResource.GetDownloadResourceResultAsync(nugetPackageIdentity, context, string.Empty, new NullLogger(), cancellationToken))
                {
                    if (result.Status == DownloadResourceResultStatus.Cancelled)
                    {
                        throw new OperationCanceledException();
                    }
                    else if (result.Status == DownloadResourceResultStatus.NotFound)
                    {
                        throw new InvalidOperationException($"Package '{identity.Id}-v{identity.Version}' not found");
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
    }
}

using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Transport
{
    public interface IDownloader
    {
        Task<Stream> DownloadPluginAsync(
            PluginIdentity identity,
            IProgress<int> progress,
            CancellationToken cancellationToken);
    }
}

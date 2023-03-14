using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    public class LocalPluginLoader
        : IPluginStreamLoader<FileInfo>
    {
        public Task<Stream> GetPluginStreamAsync(FileInfo pluginInfo, CancellationToken cancellationToken, IProgress<int> progress)
        {
            return Task.FromResult<Stream>(File.OpenRead(pluginInfo.FullName));
        }

        public Task<bool> IsSupportedAsync(FileInfo pluginInfo)
        {
            return Task.FromResult(pluginInfo.Exists);
        }
    }
}

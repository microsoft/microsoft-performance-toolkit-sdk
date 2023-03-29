using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    public interface IPluginPackageReader
    {
        Task<PluginPackage> TryReadPackageAsync(
            Stream stream,
            CancellationToken cancellationToken);
    }
}
    
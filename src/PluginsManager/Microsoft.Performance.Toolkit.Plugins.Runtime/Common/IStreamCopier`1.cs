using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IStreamCopier<TDestination>
    {
        Task CopyStreamAsync(
            TDestination destination,
            Stream stream,
            CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataWriterAsync<TTarget, TEntity>
    {
        Task WriteDataAsync(TTarget target, TEntity entity, CancellationToken cancellationToken);
    }
}

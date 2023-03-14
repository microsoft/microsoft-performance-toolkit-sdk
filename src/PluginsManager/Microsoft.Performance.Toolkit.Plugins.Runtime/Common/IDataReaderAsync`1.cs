using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataReaderAsync<TSource, TEntity>
    {
        Task<TEntity> ReadDataAsync(TSource source, CancellationToken cancellationToken);

        bool CanReadData(TSource source);
    }

}
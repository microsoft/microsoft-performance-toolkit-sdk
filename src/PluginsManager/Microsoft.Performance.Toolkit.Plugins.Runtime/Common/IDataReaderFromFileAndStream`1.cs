using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataReaderFromFileAndStream<TEntity>
        : IDataReader<Stream, TEntity>,
          IDataReaderAsync<Stream, TEntity>,
          IDataReader<FileInfo, TEntity>,
          IDataReaderAsync<FileInfo, TEntity>
    {
    }
}

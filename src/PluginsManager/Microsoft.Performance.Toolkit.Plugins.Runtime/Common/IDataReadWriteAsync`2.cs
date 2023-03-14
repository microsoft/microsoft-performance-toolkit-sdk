namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataReadWriteAsync<T, TEntity>
        : IDataReaderAsync<T, TEntity>,
          IDataWriterAsync<T, TEntity>
    {
    }
}

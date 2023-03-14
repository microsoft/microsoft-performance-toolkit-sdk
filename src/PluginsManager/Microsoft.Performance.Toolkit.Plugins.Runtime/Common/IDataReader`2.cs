namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataReader<TSource, TEntity>
    {
        TEntity ReadData(TSource source);

        bool CanReadData(TSource source);
    }
}

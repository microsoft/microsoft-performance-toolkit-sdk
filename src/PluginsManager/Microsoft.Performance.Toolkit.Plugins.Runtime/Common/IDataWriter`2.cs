namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDataWriter<TTarget, TEntity>
    {
        void WriteData(TTarget target, TEntity entity);
    }

}

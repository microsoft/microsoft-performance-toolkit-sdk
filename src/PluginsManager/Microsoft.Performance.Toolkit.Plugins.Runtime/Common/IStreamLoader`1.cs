using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IStreamLoader<T>
        : IDataReader<T, Stream>
    {
    }
}

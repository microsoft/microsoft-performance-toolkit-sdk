using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public class FileStreamLoader
        : IStreamLoader<FileInfo>
    {
        public bool CanReadData(FileInfo source)
        {
            return source.Exists;
        }

        public Stream ReadData(FileInfo source)
        {
            return source.OpenRead();
        }
    }
}

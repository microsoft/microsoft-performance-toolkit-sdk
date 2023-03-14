using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public interface IDirectoryAccessor
        : IStreamCopier<FileInfo>,
          IDataCleaner<DirectoryInfo>
    {
    }
}

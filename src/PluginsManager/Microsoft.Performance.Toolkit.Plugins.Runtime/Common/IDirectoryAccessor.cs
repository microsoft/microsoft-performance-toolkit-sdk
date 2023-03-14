// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///    Provides access to a directory for dropping files into and removing files out of.
    /// </summary>
    public interface IDirectoryAccessor
        : IStreamCopier<FileInfo>,
          IDataCleaner<DirectoryInfo, DirectoryInfo>
    {
    }
}

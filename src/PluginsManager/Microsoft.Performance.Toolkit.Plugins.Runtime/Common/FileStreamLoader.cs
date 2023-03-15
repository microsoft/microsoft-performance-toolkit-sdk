// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a loader that can load stream from a file.
    /// </summary>
    public class FileStreamLoader
        : IStreamLoader<FileInfo>
    {
        /// <inheritdoc/>
        public bool CanReadData(FileInfo fileInfo)
        {
            return fileInfo.Exists;
        }

        /// <inheritdoc>/>
        public Stream ReadData(FileInfo fileInfo)
        {
            return fileInfo.OpenRead();
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK;

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
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Exists;
        }

        /// <inheritdoc>/>
        public Stream ReadData(FileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.OpenRead();
        }
    }
}

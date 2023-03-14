// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    public class FileReadWriteAsync<T>
        : IDataReadWriteAsync<FileInfo, T>
    {
        private readonly ISerializer serializer;

        public FileReadWriteAsync(ISerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        public bool CanReadData(FileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Exists;
        }

        public Task<T> ReadDataAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenRead())
            {
                return this.serializer.DeserializeAsync<T>(stream, cancellationToken);
            }
        }

        public Task WriteDataAsync(FileInfo fileInfo, T entity, CancellationToken cancellationToken)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenWrite())
            {
                return this.serializer.SerializeAsync(stream, entity, cancellationToken);
            }
        }
    }
}

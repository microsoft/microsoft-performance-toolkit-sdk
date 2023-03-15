// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Common
{
    /// <summary>
    ///     Represents a data reader and writer that can read and write data to a file.
    /// </summary>
    public class FileReadWriteAsync<TEntity>
        : IDataReadWriteAsync<FileInfo, TEntity>
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

        public Task<TEntity> ReadDataAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenRead())
            {
                return this.serializer.DeserializeAsync<TEntity>(stream, cancellationToken);
            }
        }

        public Task WriteDataAsync(FileInfo fileInfo, TEntity entity, CancellationToken cancellationToken)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenWrite())
            {
                return this.serializer.SerializeAsync(stream, entity, cancellationToken);
            }
        }
    }
}

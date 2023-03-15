// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
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
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileReadWriteAsync{TEntity}"/> class.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use when reading and writing data.
        /// </param>
        public FileReadWriteAsync(ISerializer serializer)
            : this(serializer, Logger.Create<FileReadWriteAsync<TEntity>>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileReadWriteAsync{TEntity}"/> class.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use when reading and writing data.
        /// </param>
        /// <param name="logger">
        ///     The logger to use to log messages.
        /// </param>
        public FileReadWriteAsync(ISerializer serializer, ILogger logger)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(logger, nameof(logger));

            this.serializer = serializer;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public bool CanReadData(FileInfo fileInfo)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return fileInfo.Exists;
        }

        /// <inheritdoc/>
        public Task<TEntity> ReadDataAsync(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            Guard.NotNull(fileInfo, nameof(fileInfo));

            using (FileStream stream = fileInfo.OpenRead())
            {
                return this.serializer.DeserializeAsync<TEntity>(stream, cancellationToken);
            }
        }

        /// <inheritdoc/>
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

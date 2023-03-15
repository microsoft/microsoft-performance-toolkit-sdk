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
    ///     Represents a data reader that can read data from a file or a stream using a <see cref="ISerializer"/>.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of data entity.
    /// </typeparam>
    public class DataReaderFromFileAndStream<TEntity>
        : IDataReaderFromFileAndStream<TEntity>
    {
        private readonly ISerializer serializer;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataReaderFromFileAndStream{TEntity}"/> class.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use to read data.
        /// </param>
        public DataReaderFromFileAndStream(ISerializer serializer)
            : this(serializer, Logger.Create<DataReaderFromFileAndStream<TEntity>>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataReaderFromFileAndStream{TEntity}"/> class.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use to read data.
        /// </param>
        /// <param name="logger">
        ///     The logger to use to log messages.
        /// </param>
        public DataReaderFromFileAndStream(ISerializer serializer, ILogger logger)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(logger, nameof(logger));

            this.serializer = serializer;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public bool CanReadData(Stream sourceStream)
        {
            Guard.NotNull(sourceStream, nameof(sourceStream));

            return sourceStream.CanRead;
        }

        /// <inheritdoc/>
        public bool CanReadData(FileInfo sourceFile)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));

            return File.Exists(sourceFile.FullName);
        }

        /// <inheritdoc/>
        public TEntity ReadData(Stream sourceStream)
        {
            Guard.NotNull(sourceStream, nameof(sourceStream));

            return this.serializer.Deserialize<TEntity>(sourceStream);
        }
        
        /// <inheritdoc/>
        public TEntity ReadData(FileInfo sourceFile)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));

            using (var stream = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadData(stream);
            }
        }

        /// <inheritdoc/>
        public Task<TEntity> ReadDataAsync(Stream sourceStream, CancellationToken cancellationToken)
        {
            Guard.NotNull(sourceStream, nameof(sourceStream));
            
            return this.serializer.DeserializeAsync<TEntity>(sourceStream, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TEntity> ReadDataAsync(FileInfo sourceFile, CancellationToken cancellationToken)
        {
            Guard.NotNull(sourceFile, nameof(sourceFile));

            using (var stream = new FileStream(sourceFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadDataAsync(stream, cancellationToken);
            }
        }
    }
}

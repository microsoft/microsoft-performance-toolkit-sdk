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
    ///     Provides a default implementation of <see cref="IDataReadWriteAsync{TSource, TTarget}"/> using
    ///     a <see cref="ISerializer"/> for data serialization and deserialization.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of data to read and write.
    /// </typeparam>
    public class StreamDataReadWriteAsync<TEntity>
        : IDataReadWriteAsync<Stream, TEntity>
    {
        private readonly ISerializer serializer;
        private readonly ILogger logger;

        /// <summary>
        ///     Creates a new instance of <see cref="StreamDataReadWriteAsync{TEntity}"/>.
        /// </summary>
        /// <param name="serializer"></param>
        public StreamDataReadWriteAsync(ISerializer serializer)
            : this(serializer, Logger.Create<StreamDataReadWriteAsync<TEntity>>())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="StreamDataReadWriteAsync{TEntity}"/>.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use for data serialization and deserialization.
        /// </param>
        /// <param name="logger">
        ///     The logger to use for logging.
        /// </param>
        public StreamDataReadWriteAsync(
            ISerializer serializer,
            ILogger logger)
        {
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(logger, nameof(logger));

            this.serializer = serializer;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public bool CanReadData(Stream source)
        {
            Guard.NotNull(source, nameof(source));

            return source.CanRead;
        }

        /// <inheritdoc/>
        public Task<TEntity> ReadDataAsync(Stream source, CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));

            return this.serializer.DeserializeAsync<TEntity>(source, cancellationToken);
        }

        /// <inheritdoc/>
        public Task WriteDataAsync(Stream target, TEntity entity, CancellationToken cancellationToken)
        {
            Guard.NotNull(target, nameof(target));

            return this.serializer.SerializeAsync(target, entity, cancellationToken);
        }
    }
}

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
    ///     Provides a default implementation of <see cref="IDataReadWriteAsync{TSource, TTarget}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StreamDataReadWriteAsync<T>
        : IDataReadWriteAsync<Stream, T>
    {
        private readonly ISerializer serializer;

        public StreamDataReadWriteAsync(ISerializer serializer)
        {
            this.serializer = serializer;
        }
        
        public bool CanReadData(Stream source)
        {
            Guard.NotNull(source, nameof(source));

            return source.CanRead;
        }

        public Task<T> ReadDataAsync(Stream source, CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));
            
            return this.serializer.DeserializeAsync<T>(source, cancellationToken);
        }

        public Task WriteDataAsync(Stream target, T entity, CancellationToken cancellationToken)
        {
            Guard.NotNull(target, nameof(target));

            return this.serializer.SerializeAsync(target, entity, cancellationToken);
        }
    }
}

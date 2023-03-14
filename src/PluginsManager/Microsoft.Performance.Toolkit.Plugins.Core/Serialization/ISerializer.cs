// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///     Represents a serializer that can serialize and deserialize objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of object that can be serialized and deserialized.
    /// </typeparam>
    public interface ISerializer
    {
        /// <summary>
        ///     Asynchronously deserializes an object of type <typeparamref name="T"/> from the given stream.
        /// </summary>
        /// <param name="sourceStream">
        ///     The stream to deserialize from.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A task that completes when the object has been deserialized. The task result is the deserialized object.
        /// </returns>
        Task<T> DeserializeAsync<T>(Stream sourceStream, CancellationToken cancellationToken);

        /// <summary>
        ///     Deserializes an object of type <typeparamref name="T"/> from the given stream.
        /// </summary>
        /// <param name="sourceStream">
        ///     The stream to deserialize from.
        /// </param>
        /// <returns>
        ///     The deserialized object.
        /// </returns>
        T Deserialize<T>(Stream sourceStream);

        /// <summary>
        ///     Asynchronously serializes the given object of type <typeparamref name="T"/> to the given stream.
        /// </summary>
        /// <param name="targetStream">
        ///     The stream to serialize to.
        /// </param>
        /// <param name="obj">
        ///     The object to serialize.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     A task that completes when the object has been serialized.
        /// </returns>
        Task SerializeAsync<T>(Stream targetStream, T obj, CancellationToken cancellationToken);

        /// <summary>
        ///     Serializes the given object of type <typeparamref name="T"/> to the given stream.
        /// </summary>
        /// <param name="targetStream">
        ///     The stream to serialize to.
        /// </param>
        /// <param name="obj">
        ///     The object to serialize.
        /// </param>
        void Serialize<T>(Stream targetStream, T obj);
    }
}

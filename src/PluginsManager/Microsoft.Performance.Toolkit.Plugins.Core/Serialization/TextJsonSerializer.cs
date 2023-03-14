// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///    Serializes and deserializes objects using the JSON format.
    /// </summary>
    public sealed class TextJsonSerializer
        : ISerializer
    {
        private readonly JsonSerializerOptions serializerOptions;

        /// <summary>
        ///    Creates an instance of <see cref="JsonSerializer"/> with the given <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="options">
        ///    The options to use when serializing or deserializing.
        /// </param>
        public TextJsonSerializer(JsonSerializerOptions options)
        {
            this.serializerOptions = options;
        }

        /// <summary>
        ///    Creates an instance of <see cref="JsonSerializer"/> with the default <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public TextJsonSerializer()
            : this(null)
        {
        }

        /// <inheritdoc>/>
        public Task<T> DeserializeAsync<T>(Stream sourceStream, CancellationToken cancellationToken)
        {
            return JsonSerializer.DeserializeAsync<T>(
                sourceStream,
                this.serializerOptions,
                cancellationToken).AsTask();
        }

        /// <inheritdoc>/>
        public Task SerializeAsync<T>(Stream targetStrteam, T obj, CancellationToken cancellationToken)
        {
            return JsonSerializer.SerializeAsync<T>(
                targetStrteam,
                obj,
                this.serializerOptions,
                cancellationToken);
        }

        /// <inheritdoc>/>
        public T Deserialize<T>(Stream sourceStream)
        {
            return JsonSerializer.Deserialize<T>(
                sourceStream,
                this.serializerOptions);
        }

        /// <inheritdoc>/>
        public void Serialize<T>(Stream targetStream, T obj)
        {
            JsonSerializer.Serialize<T>(
                targetStream,
                obj,
                this.serializerOptions);
        }
    }
}

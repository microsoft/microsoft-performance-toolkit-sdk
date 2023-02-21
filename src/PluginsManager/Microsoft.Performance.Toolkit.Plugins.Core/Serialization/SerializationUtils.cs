// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    public static class SerializationUtils
    {
        public static bool WriteToStream<T>(
            Stream stream,
            T obj,
            JsonSerializerOptions options,
            ILogger logger)
        {
            try
            {
                JsonSerializer.Serialize(stream, obj, options);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> WriteToStreamAsync<T>(
           Stream stream,
           T obj,
           JsonSerializerOptions options,
           ILogger logger)
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, obj, options);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool WriteToFile<T>(
            string fileName,
            T obj,
            JsonSerializerOptions options,
            ILogger logger)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return WriteToStream<T>(fileStream, obj, options, logger);
            }
        }

        /// <summary>
        ///     Write to file with options.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of object to write.
        /// </typeparam>
        /// <param name="fileName">
        ///     Name of file to write to.
        /// </param>
        /// <param name="obj">
        ///     Object to write.
        /// </param>
        /// <param name="options">
        ///     Options to use when writing.
        /// </param>
        /// <param name="logger">
        ///     Logger to use.
        /// </param>
        /// <returns>
        ///     True if successful, false otherwise.
        /// </returns>
        public static async Task<bool> WriteToFileAsync<T>(
            string fileName,
            T obj,
            JsonSerializerOptions options,
            ILogger logger)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return await WriteToStreamAsync<T>(fileStream, obj, options, logger);
            }
        }
        

        public static T ReadFromStream<T>(
            Stream stream,
            JsonSerializerOptions options,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            try
            {
                return JsonSerializer.Deserialize<T>(stream, options);
            }
            catch (JsonException ex)
            {
                return default;
            }
        }

        public static async Task<T> ReadFromStreamAsync<T>(
            Stream stream,
            JsonSerializerOptions options,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, options);
            }
            catch (JsonException ex)
            {
                return default;
            }
        }

        public static T ReadFromFile<T>(
            string fileName,
            JsonSerializerOptions options,
            ILogger logger)
        {
            Guard.NotNull(fileName, nameof(fileName));
            Guard.NotNull(logger, nameof(logger));

            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return ReadFromStream<T>(fileStream, options, logger);
                }
            }
            catch (IOException ex)
            {
                return default;
            }
        }

        public static async Task<T> ReadFromFileAsync<T>(
            string fileName,
            JsonSerializerOptions options,
            ILogger logger)
        {
            Guard.NotNull(fileName, nameof(fileName));

            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return await ReadFromStreamAsync<T>(fileStream, options, logger);
                }
            }
            catch (IOException ex)
            {
                return default;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Serialization
{
    public static class SerializationUtils
    {
        public static readonly JsonSerializerOptions PluginsManagerSerializerDefaultOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new VersionStringConverter(),
            }
        };

        internal class VersionStringConverter
            : JsonConverter<Version>
        {
            public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string stringValue = reader.GetString();
                    return new Version(stringValue);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        public static bool WriteToStream<T>(Stream stream, T obj, ILogger logger)
        {
            return WriteToStreamWithOptions<T>(stream, obj, PluginsManagerSerializerDefaultOptions);
        }

        public static bool WriteToStreamWithOptions<T>(Stream stream, T obj, JsonSerializerOptions options)
        {
            try
            {
                JsonSerializer.Serialize(stream, obj, PluginsManagerSerializerDefaultOptions);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool WriteToFile<T>(string fileName, T obj, ILogger logger)
        {
            return WriteToFileWithOptions<T>(fileName, obj, PluginsManagerSerializerDefaultOptions, logger);
        }

        public static bool WriteToFileWithOptions<T>(string fileName, T obj, JsonSerializerOptions options, ILogger logger)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return WriteToStreamWithOptions<T>(fileStream, obj, options);
            }
        }

        public static Task<bool> WriteToStreamAsyc<T>(Stream stream, T obj, ILogger logger)
        {
            return WriteToStreamWithOptionsAsync<T>(stream, obj, PluginsManagerSerializerDefaultOptions);
        }

        public static async Task<bool> WriteToStreamWithOptionsAsync<T>(Stream stream, T obj, JsonSerializerOptions options)
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, obj, PluginsManagerSerializerDefaultOptions);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static Task<bool> WriteToFileAsync<T>(string fileName, T obj, ILogger logger)
        {
            return WriteToFileWithOptionsAsync<T>(fileName, obj, PluginsManagerSerializerDefaultOptions, logger);
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
        public static async Task<bool> WriteToFileWithOptionsAsync<T>(string fileName, T obj, JsonSerializerOptions options, ILogger logger)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return await WriteToStreamWithOptionsAsync<T>(fileStream, obj, options);
            }
        }
        
        public static T ReadFromFile<T>(
            string fileName,
            ILogger logger)
        {
            return ReadFromFileWithOptions<T>(fileName, PluginsManagerSerializerDefaultOptions, logger);
        }

        public static T ReadFromStream<T>(
            Stream stream,
            ILogger logger)
        {
            return ReadFromStreamWithOptions<T>(stream, PluginsManagerSerializerDefaultOptions, logger);
        }

        public static T ReadFromFileWithOptions<T>(
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
                    return ReadFromStream<T>(fileStream, logger);
                }
            }
            catch (IOException ex)
            {
                return default;
            }
        }

        public static T ReadFromStreamWithOptions<T>(
            Stream stream,
            JsonSerializerOptions options,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));
            Guard.NotNull(logger, nameof(logger));

            try
            {
                return JsonSerializer.Deserialize<T>(stream, PluginsManagerSerializerDefaultOptions);
            }
            catch (JsonException ex)
            {
                return default;
            }
        }

        public static async Task<T> ReadFromStreamAsync<T>(
            Stream stream,
            ILogger logger)
        {
            Guard.NotNull(stream, nameof(stream));

            return await ReadFromStreamWithOptionsAsync<T>(stream, PluginsManagerSerializerDefaultOptions, logger);
        }

        public static async Task<T> ReadFromFileAsync<T>(
            string fileName,
            ILogger logger)
        {
            Guard.NotNull(fileName, nameof(fileName));
            Guard.NotNull(logger, nameof(logger));

            return await ReadFromFilWithOptionsAsync<T>(fileName, PluginsManagerSerializerDefaultOptions, logger);
        }

        public static async Task<T> ReadFromStreamWithOptionsAsync<T>(
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

        public static async Task<T> ReadFromFilWithOptionsAsync<T>(
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
                    return await ReadFromStreamWithOptionsAsync<T>(fileStream, options, logger);
                }
            }
            catch (IOException ex)
            {
                return default;
            }
        }
    }
}

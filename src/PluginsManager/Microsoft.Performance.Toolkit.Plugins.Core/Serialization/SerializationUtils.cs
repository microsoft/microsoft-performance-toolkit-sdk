// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///     Provides utility methods and default configuration for serialization in the plugins system.
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>
        ///     Creates a <see cref="ISerializer{T}"/> that uses the default serialization options for the plugins system.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of object to serialize.
        /// </typeparam>
        /// <returns>
        ///     A <see cref="ISerializer{T}"/> that uses the default serialization options for the plugins system.
        /// </returns>
        public static ISerializer<T> GetJsonSerializerWithDefaultOptions<T>()
            where T : class
        {
            return new JsonSerializer<T>(PluginsSystemSerializerDefaultOptions);
        }

        /// <summary>
        ///     Creates a <see cref="ISerializer{T}"/> that uses the specified serialization options.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of object to serialize.
        /// </typeparam>
        /// <param name="serializerOptions">
        ///     The serialization options to use.
        /// </param>
        /// <returns>
        ///     A <see cref="ISerializer{T}"/> that uses the specified serialization options.
        /// </returns>
        public static ISerializer<T> GetJsonSerializer<T>(JsonSerializerOptions serializerOptions)
            where T : class
        {
            return new JsonSerializer<T>(serializerOptions);
        }

        /// <summary>
        ///     Gets the default serialization options for the plugins system.
        /// </summary>
        public static readonly JsonSerializerOptions PluginsSystemSerializerDefaultOptions = new JsonSerializerOptions
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
    }
}

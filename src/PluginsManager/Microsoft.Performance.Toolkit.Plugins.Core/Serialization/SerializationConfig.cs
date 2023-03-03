// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///     Contains the default serialization options for the PluginsManager.
    /// </summary>
    public static class SerializationConfig
    {
        /// <summary>
        ///     Gets the default serialization options for the PluginsManager.
        /// </summary>
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
    }
}

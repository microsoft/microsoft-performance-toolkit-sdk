// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///     Converts a <see cref="PluginVersion"/> to and from JSON.
    /// </summary>
    public class PluginVersionJsonConverter : JsonConverter<PluginVersion>
    {
        /// <inheritdoc/>
        public override PluginVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {            
            var versionString = reader.GetString();
            return versionString is null ? null : PluginVersion.Parse(versionString);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, PluginVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

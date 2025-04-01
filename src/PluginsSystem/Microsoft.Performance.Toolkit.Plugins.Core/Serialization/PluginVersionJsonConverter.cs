// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    public class PluginVersionJsonConverter : JsonConverter<PluginVersion>
    {
        public override PluginVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var versionString = reader.GetString();
            return versionString is null ? null : PluginVersion.Parse(versionString);
        }
        public override void Write(Utf8JsonWriter writer, PluginVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

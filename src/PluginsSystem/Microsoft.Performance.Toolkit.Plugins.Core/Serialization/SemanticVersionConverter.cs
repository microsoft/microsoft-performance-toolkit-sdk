// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NuGet.Versioning;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Serialization
{
    /// <summary>
    ///     Converts a <see cref="SemanticVersion"/> to and from JSON.
    /// </summary>
    public class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
    {
        /// <inheritdoc/>
        public override SemanticVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var versionString = reader.GetString();
            return versionString is null ? null : SemanticVersion.Parse(versionString);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
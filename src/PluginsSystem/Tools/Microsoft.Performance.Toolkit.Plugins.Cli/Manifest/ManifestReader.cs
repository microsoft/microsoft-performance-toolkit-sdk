// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public sealed class ManifestReader
        : IManifestFileReader
    {
        private readonly ISerializer<PluginManifest> serializer;
        private readonly ILogger logger;

        public ManifestReader(
            ISerializer<PluginManifest> serializer,
            ILogger<ManifestReader> logger)
        {
            this.serializer = serializer;
            this.logger = logger;
        }

        public bool TryRead(string manifestFilePath, out PluginManifest? pluginManifest)
        {
            pluginManifest = null;
            try
            {
                using (FileStream manifestStream = File.OpenRead(manifestFilePath))
                {
                    pluginManifest = this.serializer.Deserialize(manifestStream);
                }
            }
            catch (IOException ex)
            {
                this.logger.LogError($"Failed to read {manifestFilePath} due to an IO exception {ex.Message}");
            }
            catch (JsonException ex)
            {
                this.logger.LogError($"Failed to deserialize {manifestFilePath} due to an JSON exception {ex.Message}.");
            }


            return true;
        }
    }
}

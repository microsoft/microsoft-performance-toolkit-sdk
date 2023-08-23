// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
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

        public PluginManifest Read(string manifestFilePath)
        {
            PluginManifest pluginManifest;
            try
            {
                using (FileStream manifestStream = File.OpenRead(manifestFilePath))
                {
                    pluginManifest = this.serializer.Deserialize(manifestStream);
                }
            }
            catch (IOException ex)
            {
                this.logger.LogDebug(ex, $"IO exception thrown when reading {manifestFilePath}.");
                throw new ConsoleRuntimeException($"Failed to read {manifestFilePath} due to an IO exception.", ex);
            }
            catch (JsonException ex)
            {
                this.logger.LogDebug(ex, $"Json exception thrown when deserializing {manifestFilePath}");
                throw new InvalidManifestException($"Invalid manifest file content: {ex.Message}. Failed to deserialize.", ex);
            }


            return pluginManifest;
        }
    }
}

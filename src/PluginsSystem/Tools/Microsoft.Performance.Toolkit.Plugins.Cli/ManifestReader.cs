// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Interfaces;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    public class ManifestReader
        : IManifestReader
    {
        private readonly ISerializer<PluginManifest> serializer;
        private readonly IPluginManifestFileValidator manifestValidator;
        private readonly ILogger logger;

        public ManifestReader(
            ISerializer<PluginManifest> serializer,
            IPluginManifestFileValidator manifestValidator,
            ILogger<ManifestReader> logger)
        {
            this.serializer = serializer;
            this.manifestValidator = manifestValidator;
            this.logger = logger;
        }

        public PluginManifest? TryReadInteractively()
        {
            throw new NotImplementedException();
        }

        public PluginManifest? TryReadFromFile(string manifestFilePath)
        {
            if (!this.manifestValidator.Validate(manifestFilePath))
            {
                this.logger.LogError($"Invaid manifest file {manifestFilePath}");
                return null;
            }

            PluginManifest? pluginManifest = null;
            try
            {
                using (FileStream manifestStream = File.OpenRead(manifestFilePath))
                {
                    pluginManifest = this.serializer.Deserialize(manifestStream);
                }
            }
            catch (IOException ex)
            {
                this.logger.LogDebug(ex, $"IO exception thrown when reading {manifestFilePath}");
                this.logger.LogError($"Failed to read {manifestFilePath}");
            }
            catch (JsonException ex)
            {
                this.logger.LogDebug(ex, $"Json exception thrown when deserializing {manifestFilePath}");
                this.logger.LogError($"Failed to deserialize {manifestFilePath}");
            }
            

            return pluginManifest;
        }
    }
}

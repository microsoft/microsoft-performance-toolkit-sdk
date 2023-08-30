// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     A reader for plugin manifests that reads from a file.
    /// </summary>
    internal sealed class ManifestReader
        : IManifestFileReader
    {
        private readonly ISerializer<PluginManifest> serializer;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManifestReader"/>
        /// </summary>
        /// <param name="serializer">
        ///     The serializer to use.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        public ManifestReader(
            ISerializer<PluginManifest> serializer,
            ILogger<ManifestReader> logger)
        {
            this.serializer = serializer;
            this.logger = logger;
        }

        /// <inheritdoc />
        public bool TryRead(string manifestFilePath, [NotNullWhen(true)] out PluginManifest? pluginManifest)
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
                return false;
            }
            catch (JsonException ex)
            {
                this.logger.LogError($"Failed to deserialize {manifestFilePath} due to an JSON exception {ex.Message}.");
                return false;
            }

            return true;
        }
    }
}

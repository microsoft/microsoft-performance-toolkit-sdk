// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public sealed class LocalManifestSchemaLoader
        : IJsonSchemaLoader
    {
        private readonly ILogger<LocalManifestSchemaLoader> logger;

        public LocalManifestSchemaLoader(ILogger<LocalManifestSchemaLoader> logger)
        {
            this.logger = logger;
        }

        public JSchema LoadSchema()
        {
            try
            {
                string schemaFilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Constants.ManifestSchemaFilePath);

                string fileText = File.ReadAllText(schemaFilePath);
                var schema = JSchema.Parse(fileText);

                return schema;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to load manifest schema from {Constants.ManifestSchemaFilePath}");
                throw;
            }
        }
    }
}

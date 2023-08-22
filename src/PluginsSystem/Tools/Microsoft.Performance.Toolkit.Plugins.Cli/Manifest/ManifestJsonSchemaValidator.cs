// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public sealed class ManifestJsonSchemaValidator
        : IManifestFileValidator
    {
        private readonly IJsonSchemaLoader schemaLoader;
        private readonly ILogger logger;
        private JSchema? schema = null;

        public ManifestJsonSchemaValidator(IJsonSchemaLoader schemaLoader, ILogger<ManifestJsonSchemaValidator> logger)
        {
            this.schemaLoader = schemaLoader;
            this.logger = logger;
        }

        public bool IsValid(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                this.logger.LogError($"Plugin manifest file {manifestPath} does not exist.");
                return false;
            }

            JToken? parsedManifest;
            try
            {
                string? fileText = File.ReadAllText(manifestPath);
                parsedManifest = JToken.Parse(fileText);
            }
            catch (IOException ex)
            {
                this.logger.LogError($"Failed to read manifest file {manifestPath} due to an IO exception: {ex}.");
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to parse {manifestPath} as a json token.");
                throw;
            }

            if (this.schema == null)
            {
                try
                {
                    this.schema = this.schemaLoader.LoadSchema();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Failed to load json schema.");
                    throw;
                }

            }

            bool isValid = parsedManifest.IsValid(this.schema, out IList<string> errors);
            if (!isValid)
            {
                foreach (string error in errors)
                {
                    this.logger.LogWarning($"Plugin manifest validation error: {error}");
                }
            }

            return isValid;
        }
    }
}

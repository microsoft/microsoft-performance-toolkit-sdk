// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    public sealed class ManifestJsonSchemaValidator
        : IManifestFileValidator
    {
        private readonly IJsonSchemaLoader schemaLoader;
        private readonly ILogger logger;
        private readonly Lazy<JSchema> schema;

        public ManifestJsonSchemaValidator(IJsonSchemaLoader schemaLoader, ILogger<ManifestJsonSchemaValidator> logger)
        {
            this.schemaLoader = schemaLoader;
            this.schema = new Lazy<JSchema>(this.schemaLoader.LoadSchema);
            this.logger = logger;
        }

        public bool IsValid(string manifestPath, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            string? fileText;
            JToken? parsedManifest;
            try
            {
                fileText = File.ReadAllText(manifestPath);
            }
            catch (IOException ex)
            {
                this.logger.LogDebug(ex, $"IO exception thrown when reading {manifestPath}.");
                throw new ConsoleRuntimeException($"Failed to read {manifestPath} due to an IO exception.", ex);
            }

            try
            {
                parsedManifest = JToken.Parse(fileText);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Exception thrown when processing {manifestPath} as JSON.");
                throw new ConsoleRuntimeException($"Failed to parse {manifestPath} as JSON when validating manifest.", ex);
            }

            bool isValid = parsedManifest.IsValid(this.schema.Value, out IList<string> errors);
            if (!isValid)
            {
                errorMessages.AddRange(errors);
            }

            return isValid;
        }
    }
}

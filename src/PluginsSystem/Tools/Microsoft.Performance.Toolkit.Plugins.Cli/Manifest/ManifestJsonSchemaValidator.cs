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

            fileText = File.ReadAllText(manifestPath);
            
            parsedManifest = JToken.Parse(fileText);
            bool isValid = parsedManifest.IsValid(this.schema.Value, out IList<string> errors);
            if (!isValid)
            {
                errorMessages.AddRange(errors);
            }

            return isValid;
        }
    }
}

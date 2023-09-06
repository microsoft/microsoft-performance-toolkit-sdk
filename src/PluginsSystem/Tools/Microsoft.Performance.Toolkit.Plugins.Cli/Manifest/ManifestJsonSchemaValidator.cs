// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Validates a manifest file against the schema.
    /// </summary>
    internal sealed class ManifestJsonSchemaValidator
        : IManifestFileValidator
    {
        private readonly IJsonSchemaLoader schemaLoader;
        private readonly ILogger logger;
        private readonly Lazy<JSchema> schema;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManifestJsonSchemaValidator"/>
        /// </summary>
        /// <param name="schemaLoader">
        ///     The schema loader to use.
        /// </param>
        /// <param name="logger">
        ///     The logger to use.
        /// </param>
        public ManifestJsonSchemaValidator(IJsonSchemaLoader schemaLoader, ILogger<ManifestJsonSchemaValidator> logger)
        {
            this.schemaLoader = schemaLoader;
            this.schema = new Lazy<JSchema>(this.schemaLoader.LoadSchema);
            this.logger = logger;
        }

        /// <inheritdoc />
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

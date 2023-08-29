// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Loads the manifest schema from a local file.
    /// </summary>
    internal sealed class LocalManifestSchemaLoader
        : IJsonSchemaLoader
    {
        private readonly ILogger<LocalManifestSchemaLoader> logger;
        private readonly Lazy<string> schemaFilePath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalManifestSchemaLoader"/>
        /// </summary>
        /// <param name="logger">
        ///     Logger to use.
        /// </param>
        public LocalManifestSchemaLoader(ILogger<LocalManifestSchemaLoader> logger)
        {
            this.logger = logger;
            this.schemaFilePath = new Lazy<string>(() => Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Constants.ManifestSchemaFilePath));
        }

        /// <inheritdoc />
        public JSchema LoadSchema()
        {
            string? fileText = File.ReadAllText(this.schemaFilePath.Value);
            var schema = JSchema.Parse(fileText);
            
            return schema;
        }
    }
}

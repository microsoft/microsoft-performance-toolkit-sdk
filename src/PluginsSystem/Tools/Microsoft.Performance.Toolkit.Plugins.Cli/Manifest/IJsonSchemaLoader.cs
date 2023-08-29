// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json.Schema;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Manifest
{
    /// <summary>
    ///     Loads the schema for the plugin manifest.
    /// </summary>
    internal interface IJsonSchemaLoader
    {
        /// <summary>
        ///     Loads the schema for the plugin manifest.
        /// </summary>
        /// <returns>
        ///     The json schema.
        /// </returns>
        JSchema LoadSchema();
    }
}

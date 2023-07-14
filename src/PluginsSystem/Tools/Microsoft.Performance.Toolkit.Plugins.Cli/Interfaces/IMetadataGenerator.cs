// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal interface IMetadataGenerator
    {
        public bool TryCreateMetadata(string assemblyDir, out ExtractedMetadata pluginMetadata);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginManager.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Packaging
{
    /// <summary>
    ///     Represents a read-only plugin package.
    /// </summary>
    /// TODO: #236
    public sealed class PluginPackage
    {
        public PluginMetadata PluginMetadata { get; }
    }
}

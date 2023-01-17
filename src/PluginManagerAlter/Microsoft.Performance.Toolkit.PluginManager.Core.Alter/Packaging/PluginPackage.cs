// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Packaging
{
    /// <summary>
    /// Represents a read-only plugin package
    /// </summary>
    public sealed class PluginPackage
    {
        public PluginPackage(Stream stream)
        {

        }

        public PluginMetadata PluginMetadata { get; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Package
{
    public abstract class PluginPackageEntry
    {
        public abstract string RelativePath { get; }

        /// <summary>
        ///     Opens the entry from the plugin package.
        /// </summary>
        /// <returns>
        ///     The stream that represents the contents of the entry.
        /// </returns>
        public abstract Stream Open();
    }
}

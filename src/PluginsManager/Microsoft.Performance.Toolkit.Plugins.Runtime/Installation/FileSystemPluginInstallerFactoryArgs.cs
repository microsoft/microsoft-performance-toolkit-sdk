// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///    Arguments for <see cref="FileSystemPluginInstallerFactory"/>.
    /// </summary>
    public class FileSystemPluginInstallerFactoryArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileSystemPluginInstallerFactoryArgs"/>
        /// </summary>
        /// <param name="pluginRegistry">
        ///     The plugin registry to use.
        /// </param>
        /// <param name="pluginMetadataReader">
        ///     The plugin metadata reader to use.
        /// </param>
        public FileSystemPluginInstallerFactoryArgs(
            IPluginRegistry<DirectoryInfo> pluginRegistry,
            IDataReaderFromFileAndStream<PluginMetadata> pluginMetadataReader)
        {
            this.PluginRegistry = pluginRegistry;
            this.PluginMetadataReader = pluginMetadataReader;
        }

        /// <summary>
        ///     Gets the plugin registry to use.
        /// </summary>
        public IPluginRegistry<DirectoryInfo> PluginRegistry { get; }

        /// <summary>
        ///     Gets the plugin metadata reader to use.
        /// </summary>
        public IDataReaderFromFileAndStream<PluginMetadata> PluginMetadataReader { get; }
    }
}

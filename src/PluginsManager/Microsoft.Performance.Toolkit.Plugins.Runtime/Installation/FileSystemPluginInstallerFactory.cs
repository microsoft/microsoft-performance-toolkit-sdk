// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <inheritdoc/>
    public class FileSystemPluginInstallerFactory
        : IFactory<IPluginInstaller<DirectoryInfo>, FileSystemPluginInstallerFactoryArgs>
    {
        /// <inheritdoc/>
        public IPluginInstaller<DirectoryInfo> Create(FileSystemPluginInstallerFactoryArgs args)
        {
            Guard.NotNull(args, nameof(args));

            return new FileSystemPluginInstaller(
                args.PluginRegistry,
                args.PluginMetadataReader);
        }
    }
}

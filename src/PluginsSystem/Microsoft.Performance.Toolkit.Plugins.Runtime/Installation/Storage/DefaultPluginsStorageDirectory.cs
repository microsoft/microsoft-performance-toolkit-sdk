﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Installation
{
    /// <summary>
    ///     Represents a default implementation of <see cref="IPluginsStorageDirectory"/>.
    /// </summary>
    public sealed class DefaultPluginsStorageDirectory
        : IPluginsStorageDirectory
    {
        private const string pluginInfoFileName = "plugininfo.json";
        private const string pluginContentsInfoFileName = "plugincontents.json";
        private const string pluginContentFolderName = "plugin/";

        private readonly string rootDirectory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultPluginsStorageDirectory"/>
        /// </summary>
        /// <param name="rootDirectory">
        ///     The root directory where plugins are installed.
        /// </param>
        internal DefaultPluginsStorageDirectory(string rootDirectory)
        {
            Guard.NotNullOrWhiteSpace(rootDirectory, nameof(rootDirectory));

            this.rootDirectory = Path.GetFullPath(rootDirectory);
        }

        /// <inheritdoc/>
        public string GetPluginContentDirectory(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string directory = GetPluginRootDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, pluginContentFolderName));
        }

        /// <inheritdoc/>
        public string GetPluginInfoFilePath(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string directory = GetPluginRootDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, pluginInfoFileName));
        }

        /// <inheritdoc/>
        public string GetPluginContentsInfoFilePath(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            string directory = GetPluginRootDirectory(pluginIdentity);
            return Path.GetFullPath(Path.Combine(directory, pluginContentsInfoFileName));
        }

        /// <inheritdoc/>
        public string GetPluginRootDirectory(PluginIdentity pluginIdentity)
        {
            Guard.NotNull(pluginIdentity, nameof(pluginIdentity));

            return Path.GetFullPath(Path.Combine(this.rootDirectory, $"{pluginIdentity.Id}-{pluginIdentity.Version}"));
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAllPluginRootDirectories()
        {
            return Directory.GetDirectories(this.rootDirectory);
        }
    }
}

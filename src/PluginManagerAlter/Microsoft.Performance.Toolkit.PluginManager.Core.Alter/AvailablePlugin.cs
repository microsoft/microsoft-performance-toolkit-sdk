// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter
{
    /// <summary>
    ///     A DTO that represents a discovered plugin that is available for installation.
    /// </summary>
    public sealed class AvailablePlugin
    {
        /// <summary>
        ///     Initializes an available plugin.
        /// </summary>
        public AvailablePlugin(
            PluginIdentity pluginIdentity,
            PluginSource pluginSource,
            string displayName,
            string description,
            Uri packageUri,
            Guid fetcherTypeId) 
        {
            this.Identity = pluginIdentity;
            this.PluginSource = pluginSource;
            this.DisplayName = displayName;
            this.Description = description;
            this.PluginPackageUri = packageUri;
            this.FetcherTypeId = fetcherTypeId;
        }

        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; }
        
        /// <summary>
        ///     Gets the souce where this plugin is discovered.
        /// </summary>
        public PluginSource PluginSource { get; }

        /// <summary>
        ///     Gets the human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     Gets the user friendly description of this plugin.
        /// </summary>   
        public string Description { get; }

        /// <summary>
        ///     Gets the URI where the plugin package can be fetched.
        /// </summary>
        public Uri PluginPackageUri { get; }

        /// <summary>
        ///     Gets the Guid which identifies the platform (NuGet, Github etc.) the plugin package is hosted on.
        /// </summary>
        public Guid FetcherTypeId { get; }
    }
}

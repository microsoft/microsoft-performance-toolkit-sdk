// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter
{
    public sealed class AvailablePlugin
    {
        /// <summary>
        ///     Gets the identity of this plugin.
        /// </summary>
        public PluginIdentity Identity
        {
            get
            {
                return this.Info.Identity;
            }
        }

        public PluginSource PluginSource { get; set; }


        /// <summary>
        ///     Gets the basic information of this plugin.
        /// </summary>
        public PluginInfo Info { get; set; }

        /// <summary>
        ///     Gets the metadata of this plugin.
        /// </summary>
        public PluginMetadata Metadata { get; set;}


        /// <summary>
        ///     Gets the URI where the plugin package can be fetched.
        /// </summary>
        public Uri PluginPackageUri { get; set; }

        /// <summary>
        ///     Gets the Guid which identifies the platform type (NuGet, Github etc.)the plugin package is hosted on.
        /// </summary>
        public Guid HostTypeId { get; set; }
    }
}

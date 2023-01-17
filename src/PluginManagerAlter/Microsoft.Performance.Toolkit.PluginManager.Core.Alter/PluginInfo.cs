// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core
{
    /// <summary>
    ///     Basic information of a plugin.
    /// </summary>
    public sealed class PluginInfo
    {
        /// <summary>
        ///     The identity of this plugin.
        /// </summary>
        public PluginIdentity Identity { get; set; }

        /// <summary>
        ///     The human-readable name of this plugin.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     A user friendly description of this plugin.
        /// </summary>   
        public string Description { get; set; }

        /// <summary>
        ///     The URI of the source of this plugin.
        /// </summary>
        public Uri SourceUri { get; set; }
    }
}

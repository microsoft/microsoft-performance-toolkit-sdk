// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     A common set of plugin information shared by available plugins and installed plugins.
    /// </summary>
    public interface IPluginInfo
    {
        /// <summary>
        ///     The identity of this plugin.
        /// </summary>
        PluginIdentity Identity { get; }

        /// <summary>
        ///     The human-readable name of this plugin.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     A user friendly description of this plugin.
        /// </summary>   
        string Description { get; }

        /// <summary>
        ///     The URI of the source of this plugin.
        /// </summary>
        Uri SourceUri { get; }
    }
}

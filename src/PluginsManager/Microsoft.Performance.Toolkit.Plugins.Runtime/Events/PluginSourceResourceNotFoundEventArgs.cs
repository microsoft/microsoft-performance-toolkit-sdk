// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Events
{
    /// <summary>
    ///     Event arguments for when no available <see cref="Core.Extensibility.IPluginManagerResource"/>
    ///     is found to perform requests against a <see cref="PluginSource"/>. 
    /// </summary>
    public class PluginSourceResourceNotFoundEventArgs
        : PluginSourceErrorEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginSourceResourceNotFoundEventArgs"/> class.
        /// </summary>
        /// <param name="pluginSource">
        ///     The <see cref="PluginSource"/> this event is for.
        /// </param>
        /// <param name="resourceType">
        ///     The type of <see cref="Core.Extensibility.IPluginManagerResource"/> that could not be found.
        /// </param>
        public PluginSourceResourceNotFoundEventArgs(PluginSource pluginSource, Type resourceType)
            : base(pluginSource, $"No available {resourceType.Name} are found for plugin source {pluginSource.Uri}.")
        {
            this.ResourceType = resourceType;
        }

        /// <summary>
        ///     Gets the type of <see cref="Core.Extensibility.IPluginManagerResource"/> that could not be found.
        /// </summary>
        public Type ResourceType { get; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Process a collection of <see cref="PluginManagerResourceReference"/> provided by the
    ///     <see cref="IPluginManagerResourceLoader"/>.
    /// </summary>
    public interface IPluginManagerResourcesConsumer
    {
        /// <summary>
        ///    Called when new resources are loaded.
        /// </summary>
        /// <param name="loadedResources">
        ///    The newly loaded <see cref="PluginManagerResourceReference"/> from the <see cref="IPluginManagerResourceLoader"/>.
        /// </param>
        void OnNewResourcesLoaded(IEnumerable<PluginManagerResourceReference> loadedResources);
    }
}

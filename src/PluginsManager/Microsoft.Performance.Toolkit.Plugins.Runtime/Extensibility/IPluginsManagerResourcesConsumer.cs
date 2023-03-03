// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Runtime;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Process a collection of <see cref="PluginsManagerResourceReference"/> provided by the
    ///     <see cref="IPluginsManagerResourceLoader"/>.
    /// </summary>
    public interface IPluginsManagerResourcesConsumer
    {
        /// <summary>
        ///    Called when new resources are loaded.
        /// </summary>
        /// <param name="loadedResources">
        ///    The newly loaded <see cref="PluginsManagerResourceReference"/> from the <see cref="IPluginsManagerResourceLoader"/>.
        /// </param>
        void OnNewResourcesLoaded(IEnumerable<PluginsManagerResourceReference> loadedResources);
    }
}

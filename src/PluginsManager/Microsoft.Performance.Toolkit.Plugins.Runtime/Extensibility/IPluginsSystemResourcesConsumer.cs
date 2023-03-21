// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Process a collection of <see cref="PluginsSystemResourceReference"/> provided by the
    ///     <see cref="IPluginsSystemResourceDirectoryLoader"/>.
    /// </summary>
    public interface IPluginsManagerResourcesReferenceConsumer
    {
        /// <summary>
        ///    Called when new resources are loaded.
        /// </summary>
        /// <param name="loadedResources">
        ///    The newly loaded <see cref="PluginsSystemResourceReference"/> from the <see cref="IPluginsSystemResourceDirectoryLoader"/>.
        /// </param>
        void OnNewResourcesLoaded(IEnumerable<PluginsSystemResourceReference> loadedResources);
    }
}

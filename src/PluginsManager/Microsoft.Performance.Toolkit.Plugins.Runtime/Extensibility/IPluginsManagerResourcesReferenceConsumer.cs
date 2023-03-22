// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <summary>
    ///     Process a collection of <see cref="PluginsManagerResourceReference"/> provided by the
    ///     <see cref="IPluginsManagerResourceDirectoryLoader"/>.
    /// </summary>
    public interface IPluginsManagerResourcesReferenceConsumer
    {
        /// <summary>
        ///    Called when new resources are loaded.
        /// </summary>
        /// <param name="loadedResources">
        ///    The newly loaded <see cref="PluginsManagerResourceReference"/> from the <see cref="IPluginsManagerResourceDirectoryLoader"/>.
        /// </param>
        void OnNewResourcesLoaded(IEnumerable<PluginsManagerResourceReference> loadedResources);
    }
}

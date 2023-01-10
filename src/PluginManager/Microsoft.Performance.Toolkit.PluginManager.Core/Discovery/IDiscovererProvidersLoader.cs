// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Loads discoverer providers.
    /// </summary>
    public interface IDiscovererProvidersLoader
    {
        /// <summary>
        ///     Tries to load <see cref="IPluginDiscovererProvider"/>s from the specified directory.
        /// </summary>
        /// <param name="directory">
        ///     The directoty to load providers from.
        /// </param>
        /// <param name="discovererProviders">
        ///     The loaded providers.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="discovererProviders"/> are successfully loaded. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool TryLoad(string directory, out IEnumerable<IPluginDiscovererProvider> discovererProviders);
    }
}

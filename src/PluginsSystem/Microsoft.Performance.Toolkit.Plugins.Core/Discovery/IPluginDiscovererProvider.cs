// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Discovery
{
    /// <summary>
    ///     Represents a provider that creates <see cref="IPluginDiscoverer"/> for supported <see cref="PluginSource"/>s.
    /// </summary>
    public interface IPluginDiscovererProvider
        : IPluginsSystemResource
    {
        /// <summary>
        ///     Checks if the given <paramref name="pluginSource"/> is supported by this discoverer.
        /// </summary>
        /// <param name="pluginSource">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="pluginSource"/> is supported by this discoverer. <c>false</c>
        ///     otherwise.
        /// </returns>
        Task<bool> IsSupportedAsync(PluginSource pluginSource);

        /// <summary>
        ///     Creates a discoverer for the specified plugin source.
        /// </summary>
        /// <param name="pluginSource">
        ///     A plugin source.
        /// </param>
        /// <returns>
        ///     A plugin discoverer.
        /// </returns>
        IPluginDiscoverer CreateDiscoverer(
            PluginSource pluginSource);
    }
}

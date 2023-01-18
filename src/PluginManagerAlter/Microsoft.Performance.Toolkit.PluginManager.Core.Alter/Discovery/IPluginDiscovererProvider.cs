// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Extensibility;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery
{
    /// <summary>
    ///     Represents a provider that creates <see cref="IPluginDiscoverer"/> for supported <see cref="PluginSource"/>s.
    /// </summary>
    public interface IPluginDiscovererProvider : IPluginManagerResource
    {
        /// <summary>
        ///     Checks if the given <paramref name="source"/> is supported by this discoverer.
        /// </summary>
        /// <param name="source">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="source"/> is supported by this discoverer. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool IsSupported(PluginSource source);

        /// <summary>
        ///     Creates a discoverer for the specified plugin source.
        /// </summary>
        /// <param name="source">
        ///     A plugin source.
        /// </param>
        /// <returns>
        ///     A plugin discoverer.
        /// </returns>
        IPluginDiscoverer CreateDiscoverer(PluginSource source);
    }
}

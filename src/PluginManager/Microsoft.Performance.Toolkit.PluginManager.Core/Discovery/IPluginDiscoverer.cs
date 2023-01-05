// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     A non-generic interface that exposes the type of the <see cref="IPluginSource"/> 
    ///     a discoverer discovers plugins from,
    /// </summary>
    public interface IPluginDiscoverer
    {
        /// <summary>
        ///     The <see cref="Type"/> of the <see cref="IPluginSource"/> this discover discovers plugins from.
        /// </summary>
        Type PluginSourceType { get; }
    }
}
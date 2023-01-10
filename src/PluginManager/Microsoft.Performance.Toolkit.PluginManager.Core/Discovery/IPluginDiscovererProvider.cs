// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Represents a provider that creates discoverers for discovering from <see cref="IPluginSource"/>s of type <see cref="PluginSourceType"/>.
    /// </summary>
    public interface IPluginDiscovererProvider
    {
        /// <summary>
        ///     The type of the <see cref="IPluginSource"/> this provider supports.
        /// </summary>
        Type PluginSourceType { get; }
    }
}

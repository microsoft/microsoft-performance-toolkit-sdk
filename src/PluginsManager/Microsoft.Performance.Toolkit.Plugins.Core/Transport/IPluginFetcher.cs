// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Transport
{
    /// <summary>
    ///     Represents a fetcher that is capable of fetching the plugin package from a supported <see cref="AvailablePluginInfo"/>.
    ///     A fetcher is meant to be stateless.
    /// </summary>
    public interface IPluginFetcher
        : IPluginStreamLoader<AvailablePluginInfo>,
          IPluginsManagerResource
    {
    }
}

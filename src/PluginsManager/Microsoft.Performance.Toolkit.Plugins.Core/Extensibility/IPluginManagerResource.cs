// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Represents a resource that is used by the plugin manager to perform operations like plugin discovering and fetching.
    ///     This interface is meant to be the root extension point of plugin managers resources (discoverer providers, fetechers etc.)
    /// </summary>
    public interface IPluginManagerResource
    {
        /// <summary>
        ///     Provides the <see cref="IPluginManagerResource"/> an application-appropriate logging mechanism.
        /// </summary>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        void SetLogger(ILogger logger);
    }
}

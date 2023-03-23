// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Core.Extensibility
{
    /// <summary>
    ///     Represents a resource that is used by the plugin system to perform operations like plugin discovering and fetching.
    ///     This interface is meant to be the root extension point of plugin system resources (discoverer providers, fetechers etc.)
    /// </summary>
    /// <remarks>
    ///     Instances of this interface are intended to be shared resources that can be concurrently accessed by multiple threads.
    ///     Thread safety needs to be ensured by the implementer.
    /// </remarks>
    public interface IPluginsSystemResource
    {
        /// <summary>
        ///     Provides the <see cref="IPluginsSystemResource"/> an application-appropriate logging mechanism.
        /// </summary>
        /// <param name="logger">
        ///     Used to log information.
        /// </param>
        /// <remarks>
        ///     Ensure that this method is called and is called only once before the resource is used.
        ///     Ensure that the logger is thread-safe.
        /// </remarks>
        void SetLogger(ILogger logger);
    }
}

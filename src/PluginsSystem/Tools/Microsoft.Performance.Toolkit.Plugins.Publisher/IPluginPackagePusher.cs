// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher
{
    /// <summary>
    ///     Represents a component that can push a plugin package and register it with a plugin source.
    /// </summary>
    public interface IPluginPackagePusher
    {
        /// <summary>
        ///     Pushes the given plugin package to the given plugin source.
        /// </summary>
        /// <param name="source">
        ///     The plugin source to push the plugin package to.
        /// </param>
        /// <param name="pluginPackageStream">
        ///     The stream of the plugin package to push.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the plugin package was successfully pushed to the plugin source. <c>false</c>
        /// </returns>
        Task<bool> PushPackage(
            PluginSource source,
            Stream pluginPackageStream,
            CancellationToken cancellationToken);
    }
}

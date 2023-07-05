// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Validates that a plugin can be used.
    /// </summary>
    public interface IPluginValidator
    {
        /// <summary>
        ///     Determines whether or not a plugin can be used.
        /// </summary>
        /// <param name="pluginMetadata">
        ///     The metadata for the plugin attempting to be used.
        /// </param>
        /// <param name="errorInfos">
        ///     <see cref="ErrorInfo"/>s describing why <paramref name="pluginMetadata"/> is invalid.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="pluginMetadata"/> is valid, <c>false</c> otherwise.
        /// </returns>
        bool IsValid(PluginMetadata pluginMetadata, out ErrorInfo[] errorInfos);
    }
}
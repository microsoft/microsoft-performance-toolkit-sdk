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
        ///     Validates a plugin based on its metadata, returning <see cref="ErrorInfo"/>s for
        ///     any validation issues found.
        /// </summary>
        /// <param name="pluginMetadata">
        ///     The metadata for the plugin to validate.
        /// </param>
        /// <returns>
        ///     <see cref="ErrorInfo"/>s describing why <paramref name="pluginMetadata"/> is invalid.
        /// </returns>
        ErrorInfo[] ValidationErrors(PluginMetadata pluginMetadata);
    }
}

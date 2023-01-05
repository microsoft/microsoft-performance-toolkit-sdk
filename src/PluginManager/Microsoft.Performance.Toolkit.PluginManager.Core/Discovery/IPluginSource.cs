// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Discovery
{
    /// <summary>
    ///     Represents a plugin source (e.g. a NuGet feed, a atom feed, a path to a text file).
    /// </summary>
    public interface IPluginSource
    {
        /// <summary>
        ///     Gets the name of the plugin source.
        /// </summary>
        string Name { get; }
    }
}

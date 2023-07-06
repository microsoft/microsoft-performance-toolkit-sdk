// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Represents an operation within the plugins system that requires plugin validation.
    /// </summary>
    public enum PluginsSystemOperation
    {
        /// <summary>
        ///     One or more plugins are attempting to be installed.
        /// </summary>
        Install,

        /// <summary>
        ///  One or more plugins are attempting to be loaded.
        /// </summary>
        Load,
    }
}

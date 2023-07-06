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
        ///     The plugins system is attempting to install one or more plugins.
        /// </summary>
        Install,

        /// <summary>
        ///  The plugins system is attempting to load one or more plugins.
        /// </summary>
        Load,
    }
}

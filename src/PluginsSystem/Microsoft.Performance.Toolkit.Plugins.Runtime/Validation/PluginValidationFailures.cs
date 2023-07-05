// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Validation
{
    /// <summary>
    ///     Represents errors that occurred while validating a specific plugin.
    /// </summary>
    public class PluginValidationFailures
    {
        public PluginValidationFailures(PluginIdentity invalidPlugin, ErrorInfo[] invalidReasons)
        {
            InvalidPlugin = invalidPlugin;
            InvalidReasons = invalidReasons;
        }

        /// <summary>
        ///     Gets the <see cref="PluginIdentity"/> of the invalid plugin.
        /// </summary>
        public PluginIdentity InvalidPlugin { get; }

        /// <summary>
        ///     Gets the <see cref="ErrorInfo"/>s representing the reason <see cref="InvalidPlugin"/> is invalid.
        /// </summary>
        public ErrorInfo[] InvalidReasons { get; }
    }
}
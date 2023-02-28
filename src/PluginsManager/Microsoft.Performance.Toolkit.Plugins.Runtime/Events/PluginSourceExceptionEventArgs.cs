// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Events
{
    /// <summary>
    ///     Event arguments for when an operation interacting with a plugin source throws an exception.
    /// </summary>
    public class PluginSourceExceptionEventArgs
        : PluginSourceErrorEventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginSourceExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="pluginSource">
        ///     The plugin source that the exception results from.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown.
        /// </param>
        public PluginSourceExceptionEventArgs(PluginSource pluginSource, Exception exception)
            : base(pluginSource, exception.Message)
        {
            this.Exception = exception;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginSourceExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="pluginSource">
        ///     The plugin source that the exception results from.
        /// </param>
        /// <param name="message">
        ///     The error message.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown.
        /// </param>
        public PluginSourceExceptionEventArgs(PluginSource pluginSource, string message, Exception exception)
            : base(pluginSource, message)
        {
            this.Exception = exception;
        }

        /// <summary>
        ///     Gets the exception that was thrown.
        /// </summary>
        public Exception Exception { get; }
    }
}

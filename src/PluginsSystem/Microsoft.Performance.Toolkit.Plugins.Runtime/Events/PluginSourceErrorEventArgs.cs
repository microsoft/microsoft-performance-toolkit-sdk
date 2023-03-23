// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.ExceptionServices;
using Microsoft.Performance.SDK;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Events
{
    /// <summary>
    ///    Event arguments type for when a plugin source encounters an error.
    /// </summary>
    public sealed class PluginSourceErrorEventArgs
        : EventArgs
    {
        /// <summary>
        ///    Initialize a new instance of the <see cref="PluginSourceErrorEventArgs"/> class.
        /// </summary>
        /// <param name="pluginSource">
        ///     The plugin source that the error results from.
        /// </param>
        /// <param name="error">
        ///     The <see cref="SDK.ErrorInfo"/> instance containing the error information.
        /// </param>
        public PluginSourceErrorEventArgs(
            PluginSource pluginSource,
            ErrorInfo error)
        {
            this.PluginSource = pluginSource;
            this.ErrorInfo = error;
        }

        /// <summary>
        ///     Initialize a new instance of the <see cref="PluginSourceErrorEventArgs"/> class with an <see cref="Exception"/>.
        /// </summary>
        /// <param name="pluginSource">
        ///     The plugin source that the error results from.
        /// </param>
        /// <param name="error">
        ///     The <see cref="SDK.ErrorInfo"/> instance containing the error information.
        /// </param>
        /// <param name="exception">
        ///     The exception that was thrown.
        /// </param>
        public PluginSourceErrorEventArgs(
            PluginSource pluginSource,
            ErrorInfo error,
            Exception exception)
            : this(pluginSource, error)
        {
            this.Exception = exception;
        }

        /// <summary>
        ///     Initialize a new instance of the <see cref="PluginSourceErrorEventArgs"/> class with an <see cref="ExceptionDispatchInfo"/>.
        /// </summary>
        /// <param name="pluginSource">
        ///     The plugin source that the error results from.
        /// </param>
        /// <param name="error">
        ///     The <see cref="SDK.ErrorInfo"/> instance containing the error information.
        /// </param>
        /// <param name="exceptionDispatchInfo">
        ///     The exception dispatch info.
        /// </param>
        public PluginSourceErrorEventArgs(
            PluginSource pluginSource,
            ErrorInfo error,
            ExceptionDispatchInfo exceptionDispatchInfo)
            : this(pluginSource, error)
        {
            this.Exception = exceptionDispatchInfo.SourceException;
            this.ExceptionDispatchInfo = exceptionDispatchInfo;
        }

        /// <summary>
        ///    Gets the plugin source that the error results from.
        /// </summary>
        public PluginSource PluginSource { get; }

        /// <summary>
        ///     Gets the <see cref="SDK.ErrorInfo"/> instance.
        /// </summary>
        public ErrorInfo ErrorInfo { get; }

        /// <summary>
        ///     Gets the exception that was thrown if any.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///     Gets the exception dispatch info.
        /// </summary>
        public ExceptionDispatchInfo ExceptionDispatchInfo { get; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Events
{
    /// <summary>
    ///    Base event arguments type for when a plugin source encounters an error.
    /// </summary>
    public abstract class PluginSourceErrorEventArgs
        : EventArgs
    {
        protected PluginSourceErrorEventArgs(PluginSource pluginSource)
            : this(pluginSource, $"Error occurred when interacting with plugin source: {pluginSource.Uri}")
        {
        }

        protected PluginSourceErrorEventArgs(PluginSource pluginSource, string errorMessage)
        {
            this.PluginSource = pluginSource;
            this.ErrorMessage = errorMessage;
        }
        
        /// <summary>
        ///     Gets the plugin source that encountered the error.
        /// </summary>
        public PluginSource PluginSource { get; }

        /// <summary>
        ///     Gets the error message. 
        /// </summary>
        public string ErrorMessage { get; }
    }
}

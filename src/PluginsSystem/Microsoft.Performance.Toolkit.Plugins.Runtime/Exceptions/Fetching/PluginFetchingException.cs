// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     Exception that occurs when the plugin cannot be fetched.
    /// </summary>
    public class PluginFetchingException
       : PluginsSystemException
    {
        public PluginFetchingException()
        {
        }

        public PluginFetchingException(string message)
            : base(message)
        {
        }

        public PluginFetchingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PluginFetchingException(string message, AvailablePluginInfo pluginInfo)
            : this(message)
        {
            this.PluginInfo = pluginInfo;
        }

        public PluginFetchingException(string message, AvailablePluginInfo pluginInfo, Exception innerException)
            : this(message, innerException)
        {
            this.PluginInfo = pluginInfo;
        }

        public AvailablePluginInfo PluginInfo { get; }
    }
}

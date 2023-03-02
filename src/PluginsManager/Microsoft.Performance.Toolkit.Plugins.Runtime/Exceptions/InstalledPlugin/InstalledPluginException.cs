// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Base exception type for installed plugins.
    /// </summary>
    public abstract class InstalledPluginException
        : PluginsManagerException
    {
        protected InstalledPluginException()
        {
        }

        protected InstalledPluginException(string message)
            : base(message)
        {
        }

        protected InstalledPluginException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InstalledPluginException(string message, InstalledPluginInfo pluginInfo)
            : this(message)
        {
            this.PluginInfo = pluginInfo;
        }

        protected InstalledPluginInfo PluginInfo { get; }
    }
}

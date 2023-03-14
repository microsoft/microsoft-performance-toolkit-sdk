// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///    Represents an exception that occurs when a plugin package cannot be loaded.
    /// </summary>
    public class PluginLocalLoadingException
        : PluginsManagerException
    {
        public PluginLocalLoadingException()
        {
        }

        public PluginLocalLoadingException(string message)
            : base(message)
        {
        }

        public PluginLocalLoadingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

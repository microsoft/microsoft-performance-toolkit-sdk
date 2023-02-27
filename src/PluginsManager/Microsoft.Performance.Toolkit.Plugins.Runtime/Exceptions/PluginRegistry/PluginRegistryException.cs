// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     Base class for all exceptions that occur while interacting with the plugin registry.
    /// </summary>
    public abstract class PluginRegistryException
        : PluginsManagerException
    {
        protected PluginRegistryException()
        {
        }

        protected PluginRegistryException(string message)
            : base(message)
        {
        }

        protected PluginRegistryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PluginRegistryException(string message, string registryFilePath)
            : this(message)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        protected PluginRegistryException(string message, string registryFilePath, Exception innerException)
            : this(message, innerException)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        public string PluginRegistryPath { get; }
    }
}

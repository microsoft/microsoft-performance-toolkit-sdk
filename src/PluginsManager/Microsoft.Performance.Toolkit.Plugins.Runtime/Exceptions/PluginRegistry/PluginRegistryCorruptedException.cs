// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     Exception that occurs when the plugin registry file is corrupted.
    /// </summary>
    public class PluginRegistryCorruptedException
        : PluginRegistryException
    {
        public PluginRegistryCorruptedException()
        {
        }

        public PluginRegistryCorruptedException(string message)
            : base(message)
        {
        }

        public PluginRegistryCorruptedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PluginRegistryCorruptedException(string message, string registryFilePath)
            : base(message, registryFilePath)
        {
        }

        public PluginRegistryCorruptedException(string message, string registryFilePath, Exception innerException)
            : base(message, registryFilePath, innerException)
        {
        }
    }
}

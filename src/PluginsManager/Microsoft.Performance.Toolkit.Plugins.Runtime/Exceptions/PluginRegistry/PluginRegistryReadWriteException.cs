// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions
{
    /// <summary>
    ///     Exception that occurs when the plugin registry file cannot be read or written.
    /// </summary>
    public class PluginRegistryReadWriteException
       : PluginRegistryException
    {
        public PluginRegistryReadWriteException()
        {
        }

        public PluginRegistryReadWriteException(string message)
            : base(message)
        {
        }

        public PluginRegistryReadWriteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PluginRegistryReadWriteException(string message, string registryFilePath)
            : base(message, registryFilePath)
        {
        }

        public PluginRegistryReadWriteException(string message, string registryFilePath, Exception innerException)
            : base(message, registryFilePath, innerException)
        {
        }
    }
}

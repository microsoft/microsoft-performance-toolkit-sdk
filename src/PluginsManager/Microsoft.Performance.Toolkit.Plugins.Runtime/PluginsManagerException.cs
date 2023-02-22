using System;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    public class PluginsManagerException
        : Exception
    {
        public PluginsManagerException()
        {
        }

        public PluginsManagerException(string message)
            : base(message)
        {
        }

        public PluginsManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class PluginRegistryCorruptedException
        : PluginsManagerException
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
            : this(message)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        public string PluginRegistryPath { get; }
    }
}

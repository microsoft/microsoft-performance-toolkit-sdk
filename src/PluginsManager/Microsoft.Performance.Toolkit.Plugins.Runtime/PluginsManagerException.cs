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

    public class PluginRegistrySerializationException
        : PluginsManagerException
    {
        public PluginRegistrySerializationException()
        {
        }

        public PluginRegistrySerializationException(string message)
            : base(message)
        {
        }

        public PluginRegistrySerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PluginRegistrySerializationException(string message, string registryFilePath, Exception innerException)
            : this(message, innerException)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        public string PluginRegistryPath { get; }
    }

    public class InstalledPluginCorruptedOrMissingException
        : PluginsManagerException
    {
        public InstalledPluginCorruptedOrMissingException()
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message)
            : base(message)
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstalledPluginCorruptedOrMissingException(string message, InstalledPluginInfo pluginInfo)
            : this(message)
        {
            this.PluginInfo = pluginInfo;
        }

        public InstalledPluginInfo PluginInfo { get; }
    }
}

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

    public class PluginRegistryReadWriteException
        : PluginsManagerException
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

        public PluginRegistryReadWriteException(string message, string registryFilePath, Exception innerException)
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
    
    public class PluginPackageCreationException
        : PluginsManagerException
    {
        public PluginPackageCreationException()
        {
        }

        public PluginPackageCreationException(string message)
            : base(message)
        {
        }

        public PluginPackageCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class PluginPackageExtractionException
        : PluginsManagerException
    {
        public PluginPackageExtractionException()
        {
        }

        public PluginPackageExtractionException(string message)
            : base(message)
        {
        }

        public PluginPackageExtractionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class MalformedPluginPackageException
        : PluginsManagerException
    {
        public MalformedPluginPackageException()
        {
        }

        public MalformedPluginPackageException(string message)
            : base(message)
        {
        }

        public MalformedPluginPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class PluginInstallationException
        : PluginsManagerException
    {
        public PluginInstallationException()
        {
        }

        public PluginInstallationException(string message)
            : base(message)
        {
        }

        public PluginInstallationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

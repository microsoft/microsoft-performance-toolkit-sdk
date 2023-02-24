using System;
using Microsoft.Performance.Toolkit.Plugins.Core;

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

    //
    // PluginRegistry Exception Types
    //
    
    public class PluginRegistryException
        : PluginsManagerException
    {
        public PluginRegistryException()
        {
        }

        public PluginRegistryException(string message)
            : base(message)
        {
        }

        public PluginRegistryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PluginRegistryException(string message, string registryFilePath)
            : this(message)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        public PluginRegistryException(string message, string registryFilePath, Exception innerException)
            : this(message, innerException)
        {
            this.PluginRegistryPath = registryFilePath;
        }

        public string PluginRegistryPath { get; }
    }

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


    //
    // PluginPackage Exception Types
    //

    public class PluginPackageException
        : PluginsManagerException
    {
        public PluginPackageException()
        {
        }

        public PluginPackageException(string message)
            : base(message)
        {
        }

        public PluginPackageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class PluginPackageCreationException
        : PluginPackageException
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
        : PluginPackageException
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
        : PluginPackageException
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

    //
    // InstalledPlugin Exception Types
    //

    public class InstalledPluginException
        : PluginsManagerException
    {
        public InstalledPluginException()
        {
        }

        public InstalledPluginException(string message)
            : base(message)
        {
        }

        public InstalledPluginException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstalledPluginException(string message, InstalledPluginInfo pluginInfo)
            : this(message)
        {
            this.PluginInfo = pluginInfo;
        }

        public InstalledPluginInfo PluginInfo { get; }
    }

    public class InstalledPluginCorruptedOrMissingException
        : InstalledPluginException
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
            : base(message, pluginInfo)
        {
        }
    }

    //
    // Fetching Exception Types
    //

    public class PluginFetchingException
        : PluginsManagerException
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

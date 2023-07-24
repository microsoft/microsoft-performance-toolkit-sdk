// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Common;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Installation;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Loading;
using Microsoft.Performance.Toolkit.Plugins.Package;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Validation;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///    Represents a plugins system that can be used to discover and install plugins.
    /// </summary>
    public sealed class PluginsSystem
    {
        /// <summary>
        ///     Creates an instance of the <see cref="PluginsSystem"/>.
        /// </summary>
        /// <param name="installer">
        ///     The installer to use.
        /// </param>
        /// <param name="discoverer">
        ///     The discoverer to use.
        /// </param>
        /// <param name="pluginSourceRepository">
        ///     The repository of plugin sources.
        /// </param>
        /// <param name="pluginDiscovererProviderLoader">
        ///     The resource loader for <see cref="IPluginDiscovererProvider"/>s.
        /// </param>
        /// <param name="pluginFetcherLoader">
        ///     The resource loader for <see cref="IPluginFetcher"/>s.
        /// </param>
        /// <param name="obsoletePluginsRemover">
        ///     The obsolete plugins remover.
        /// </param>
        /// <param name="installedPluginLoader">
        ///     The installed plugin loader.
        /// </param>
        public PluginsSystem(
            IPluginsInstaller installer,
            IPluginsDiscoveryOrchestrator discoverer,
            IRepository<PluginSource> pluginSourceRepository,
            IPluginsSystemResourceLoader<IPluginDiscovererProvider> pluginDiscovererProviderLoader,
            IPluginsSystemResourceLoader<IPluginFetcher> pluginFetcherLoader,
            IObsoletePluginsRemover obsoletePluginsRemover,
            IInstalledPluginLoader installedPluginLoader)
        {
            Guard.NotNull(installer, nameof(installer));
            Guard.NotNull(discoverer, nameof(discoverer));
            Guard.NotNull(pluginSourceRepository, nameof(pluginSourceRepository));
            Guard.NotNull(pluginDiscovererProviderLoader, nameof(pluginDiscovererProviderLoader));
            Guard.NotNull(pluginFetcherLoader, nameof(pluginFetcherLoader));
            Guard.NotNull(obsoletePluginsRemover, nameof(obsoletePluginsRemover));
            Guard.NotNull(installedPluginLoader, nameof(installedPluginLoader));

            this.Installer = installer;
            this.Discoverer = discoverer;
            this.PluginSourceRepository = pluginSourceRepository;
            this.DiscovererProviderResourceLoader = pluginDiscovererProviderLoader;
            this.FetcherResourceLoader = pluginFetcherLoader;
            this.ObsoletePluginsRemover = obsoletePluginsRemover;
            this.InstalledPluginLoader = installedPluginLoader;
        }

        /// <summary>
        ///     Creates a file based plugins system.
        /// </summary>
        /// <param name="root">
        ///     The root directory of the registry and installed plugins.
        /// </param>
        /// <param name="loadPluginFromDirectory">
        ///     A function that loads a plugin from a given directory.
        /// </param>
        /// <param name="options">
        ///     General options for the created <see cref="PluginsSystem"/>.
        /// </param>
        /// <param name="loggerFactory">
        ///     Used to create a logger for the given type.
        /// </param>
        /// <returns>
        ///     The created plugins system.
        /// </returns>
        public static PluginsSystem CreateFileBasedPluginsSystem(
            string root,
            Func<string, Task<bool>> loadPluginFromDirectory,
            PluginsSystemCreationOptions options,
            Func<Type, ILogger> loggerFactory)
        {
            Guard.NotNullOrWhiteSpace(root, nameof(root));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            string pluginsSystemRoot = Path.GetFullPath(root);

            List<IPluginValidator> validatorsToUse = new List<IPluginValidator>();

            if (options.ValidateSdkVersion)
            {
                validatorsToUse.Add(new SdkVersionValidator(loggerFactory(typeof(SdkVersionValidator))));
            }

            validatorsToUse.AddRange(options.AdditionalValidators);

            IPluginValidator compositePluginValidator = new CompositePluginValidator(validatorsToUse);

            // Initializes components for plugins discovery
            var pluginSourceRepo = new PluginSourceRepository(loggerFactory);
            var fetcherRepo = new PluginsSystemResourceRepository<IPluginFetcher>(loggerFactory);
            var discovererProviderRepo = new PluginsSystemResourceRepository<IPluginDiscovererProvider>(loggerFactory);

            var discoverer = new PluginsDiscoveryOrchestrator(
                pluginSourceRepo,
                fetcherRepo,
                discovererProviderRepo,
                compositePluginValidator,
                loggerFactory);

            // Initializes components for plugins installation
            var registry = new FileBackedPluginRegistry(
                pluginsSystemRoot,
                SerializationUtils.GetJsonSerializerWithDefaultOptions<List<InstalledPluginInfo>>(),
                loggerFactory);

            ISerializer<PluginMetadata> metadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginContentsMetadata>();

            var packageReader = new ZipPluginPackageReader(
                metadataSerializer,
                contentsMetadataSerializer,
                loggerFactory);

            var storageDirectory = new DefaultPluginsStorageDirectory(pluginsSystemRoot);

            var checsumCalculator = new SHA256DirectoryChecksumCalculator();

            var installedPluginStorage = new FileSystemInstalledPluginStorage(
                storageDirectory,
                contentsMetadataSerializer,
                checsumCalculator,
                loggerFactory);

            var validator = new InstalledPluginDirectoryChecksumValidator(storageDirectory, checsumCalculator, loggerFactory(typeof(InstalledPluginDirectoryChecksumValidator)));

            var installer = new FileBackedPluginsInstaller(
                registry,
                validator,
                installedPluginStorage,
                packageReader,
                compositePluginValidator,
                options.InvalidPluginsGate,
                loggerFactory);

            var obsoletePluginsRemover = new FileSystemObsoletePluginsRemover(
                registry,
                storageDirectory,
                loggerFactory);

            var pluginLoader = new FileSystemInstalledPluginLoader(
                storageDirectory,
                validator,
                loadPluginFromDirectory,
                compositePluginValidator,
                options.InvalidPluginsGate,
                loggerFactory);

            return new PluginsSystem(
                installer,
                discoverer,
                pluginSourceRepo,
                discovererProviderRepo,
                fetcherRepo,
                obsoletePluginsRemover,
                pluginLoader);
        }

        /// <summary>
        ///     Gets the installer.
        /// </summary>
        public IPluginsInstaller Installer { get; }

        /// <summary>
        ///     Gets the discoverer.
        /// </summary>
        public IPluginsDiscoveryOrchestrator Discoverer { get; }

        /// <summary>
        ///     Gets the loader of fetchers.
        /// </summary>
        public IPluginsSystemResourceLoader<IPluginFetcher> FetcherResourceLoader { get; }

        /// <summary>
        ///     Gets the loader of discoverer providers.
        /// </summary>
        public IPluginsSystemResourceLoader<IPluginDiscovererProvider> DiscovererProviderResourceLoader { get; }

        /// <summary>
        ///     Gets the repository of plugin sources.
        /// </summary>
        public IRepository<PluginSource> PluginSourceRepository { get; }

        /// <summary>
        ///     Gets the remover of obsolete plugins.
        /// </summary>
        public IObsoletePluginsRemover ObsoletePluginsRemover { get; }

        /// <summary>
        ///     Gets the loader of installed plugins.
        /// </summary>
        public IInstalledPluginLoader InstalledPluginLoader { get; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.DTO;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Provides the entry point for programmatically manipulating, cooking,
    ///     and accessing data.
    /// </summary>
    public abstract class Engine
        : IDisposable
    {
        private readonly List<ProcessingSourceReference> processingSourceReferences;
        private readonly ReadOnlyCollection<ProcessingSourceReference> processingSourceReferencesRO;

        private readonly List<DataCookerPath> sourceDataCookers;
        private readonly ReadOnlyCollection<DataCookerPath> sourceDataCookersRO;

        private readonly List<DataCookerPath> compositeDataCookers;
        private readonly ReadOnlyCollection<DataCookerPath> compositeDataCookersRO;

        private readonly Dictionary<Guid, TableDescriptor> tableGuidToDescriptor;
        private readonly List<TableDescriptor> allTables;
        private readonly ReadOnlyCollection<TableDescriptor> allTablesRO;

        private readonly List<DataProcessorId> dataProcessors;

        private readonly Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess;

        private readonly List<IDataSource> freeDataSources;
        private readonly ReadOnlyCollection<IDataSource> freeDataSourcesRO;

        private readonly List<DataCookerPath> enabledCookers;
        private readonly ReadOnlyCollection<DataCookerPath> enabledCookersRO;

        private readonly List<TableDescriptor> enabledTables;
        private readonly ReadOnlyCollection<TableDescriptor> enabledTablesRO;

        private readonly Func<Type, ILogger> loggerFactory;

        private DataExtensionFactory factory;
        private ExtensionRoot extensionRoot;

        private IReadOnlyCollection<string> extensionDirectories;
        private bool isProcessed;
        private IEnumerable<ErrorInfo> creationErrors;

        private IApplicationEnvironment applicationEnvironment;
        private IAssemblyLoader loader;

        private RuntimeExecutionResults runtimeExecutionResult;

        private bool isDisposed;

        internal Engine(Func<Type, ILogger> loggerFactory)
        {
            this.processingSourceReferences = new List<ProcessingSourceReference>();
            this.processingSourceReferencesRO = new ReadOnlyCollection<ProcessingSourceReference>(this.processingSourceReferences);

            this.sourceDataCookers = new List<DataCookerPath>();
            this.sourceDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.sourceDataCookers);

            this.compositeDataCookers = new List<DataCookerPath>();
            this.compositeDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.compositeDataCookers);

            this.tableGuidToDescriptor = new Dictionary<Guid, TableDescriptor>();
            this.allTables = new List<TableDescriptor>();
            this.allTablesRO = new ReadOnlyCollection<TableDescriptor>(this.allTables);

            this.dataProcessors = new List<DataProcessorId>();

            this.dataSourcesToProcess = new Dictionary<ProcessingSourceReference, List<List<IDataSource>>>();

            this.freeDataSources = new List<IDataSource>();
            this.freeDataSourcesRO = new ReadOnlyCollection<IDataSource>(this.freeDataSources);

            this.enabledCookers = new List<DataCookerPath>();
            this.enabledCookersRO = new ReadOnlyCollection<DataCookerPath>(this.enabledCookers);

            this.enabledTables = new List<TableDescriptor>();
            this.enabledTablesRO = new ReadOnlyCollection<TableDescriptor>(this.enabledTables);

            this.loggerFactory = loggerFactory;

            this.isDisposed = false;
        }

        /// <summary>
        ///     Gets the directories that the engine will scan for extensions.
        ///     These directories are scanned during <see cref="Create"/> when
        ///     creating an instance of the engine.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyCollection<string> ExtensionDirectories
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDirectories;
            }
            private set
            {
                Debug.Assert(!this.isDisposed);
                this.ThrowIfDisposed();
                this.extensionDirectories = value;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has completed processing.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool IsProcessed
        {
            get
            {
                this.ThrowIfDisposed();
                return this.isProcessed;
            }
            private set
            {
                Debug.Assert(!this.isDisposed);
                this.ThrowIfDisposed();
                this.isProcessed = value;
            }
        }

        /// <summary>
        ///     Gets the collection of source parsers that the engine will use to 
        ///     cook data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<IProcessingSource> ProcessingSources
        {
            get
            {
                this.ThrowIfDisposed();
                return this.processingSourceReferencesRO.Select(x => x.Instance);
            }
        }

        /// <summary>
        ///     Gets the collection of paths to all source cookers that the engine
        ///     has discovered.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> SourceDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.sourceDataCookersRO;
            }
        }

        /// <summary>
        ///     Gets the collection of paths to all composite cookers that the engine
        ///     has discovered.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> CompositeDataCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.compositeDataCookersRO;
            }
        }

        /// <summary>
        ///     Gets the collection of all cookers that the engine has discovered. This is the union
        ///     of <see cref="this.SourceDataCookers"/> and <see cref="this.CompositeDataCookers"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> AllCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.SourceDataCookers.Concat(this.CompositeDataCookers);
            }
        }

        /// <summary>
        ///     Gets the collection of Data Sources to process. These Data Sources will be processed in whatever
        ///     <see cref="ICustomDataProcessorWithSourceParser"/> are able to handle the Data Source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<IDataSource> FreeDataSourcesToProcess
        {
            get
            {
                this.ThrowIfDisposed();
                return this.freeDataSourcesRO;
            }
        }

        /// <summary>
        ///     Gets the collection of Data Sources that are to be processed by a specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<IProcessingSource, IReadOnlyList<IReadOnlyList<IDataSource>>> DataSourcesToProcess
        {
            get
            {
                this.ThrowIfDisposed();
                return this.dataSourcesToProcess.ToDictionary(
                    x => x.Key.Instance,
                    x => (IReadOnlyList<IReadOnlyList<IDataSource>>)x.Value
                        .Select(v => (IReadOnlyList<IDataSource>)v.AsReadOnly())
                        .ToList()
                        .AsReadOnly());
            }
        }

        /// <summary>
        ///     Gets the collection of all enabled cookers.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<DataCookerPath> EnabledCookers
        {
            get
            {
                this.ThrowIfDisposed();
                return this.enabledCookersRO;
            }
        }

        /// <summary>
        ///     Gets any non-fatal errors that occured during engine
        ///     creation. Because these errors are not fatal, they are
        ///     reported here rather than raising an exception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<ErrorInfo> CreationErrors
        {
            get
            {
                this.ThrowIfDisposed();
                return this.creationErrors;
            }
            private set
            {
                Debug.Assert(!this.isDisposed);
                this.ThrowIfDisposed();
                this.creationErrors = value;
            }
        }

        /// <summary>
        ///     Gets the collection of all enabled tables
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception> 
        public IEnumerable<TableDescriptor> EnabledTables
        {
            get
            {
                this.ThrowIfDisposed();
                return this.enabledTablesRO;
            }
        }

        /// <summary>
        ///     Gets the collection of available tables
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IEnumerable<TableDescriptor> AvailableTables
        {
            get
            {
                this.ThrowIfDisposed();
                return this.allTablesRO;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="EngineException">
        ///     An fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create()
        {
            return Create(new EngineCreateInfo());
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="createInfo">
        ///     The parameters to use for creating a new engine instance.
        /// </param>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="createInfo"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="EngineException">
        ///     An fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create(
            EngineCreateInfo createInfo)
        {
            Guard.NotNull(createInfo, nameof(createInfo));

            try
            {
                return CreateCore(createInfo);
            }
            catch (Exception e)
            {
                throw new EngineException(
                    "Unable to create the engine. See the inner exception for details.",
                    e);
            }
        }

        /// <summary>
        ///     Adds the given file to this instance for processing.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="dataSource"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public void AddDataSource(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();

            if (!this.TryAddDataSource(dataSource))
            {
                throw new UnsupportedDataSourceException(dataSource);
            }
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing.
        /// </summary>
        /// <param name="filePath">
        ///     The Data Source to process.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source has been added for processing;
        ///     <c>false</c> if the Data Source is not valid, cannot be processed,
        ///     or the instance has already been processed. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource)
        {
            this.ThrowIfDisposed();
            if (dataSource is null)
            {
                return false;
            }

            if (this.IsProcessed)
            {
                return false;
            }

            if (!this.processingSourceReferences.Any(x => x.Supports(dataSource)))
            {
                return false;
            }

            try
            {
                this.freeDataSources.Add(dataSource);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Adds the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public void AddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            this.AddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process <paramref name="dataSource"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the <see cref="IDataSource"/> has been added for processing by the <see cref="IProcessingSource"/>;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource, Type processingSourceType)
        {
            return this.TryAddDataSources(new[] { dataSource, }, processingSourceType);
        }

        /// <summary>
        ///     Adds the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        public void AddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processingSourceType, nameof(processingSourceType));
            if (dataSources.Any(x => x is null))
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            this.ThrowIfDisposed();
            this.ThrowIfProcessed();

            this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferences, this.dataSourcesToProcess, this.TypeIs);
        }

        /// <summary>
        ///     Attempts to add the given data sources to this instance for processing by
        ///     the specific <see cref="IProcessingSource"/>. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSources(IEnumerable<IDataSource> dataSources, Type processingSourceType)
        {
            this.ThrowIfDisposed();

            if (dataSources is null ||
                dataSources.Any(x => x is null) ||
                processingSourceType is null ||
                this.IsProcessed)
            {
                return false;
            }

            try
            {
                this.AddDataSourcesCore(dataSources, processingSourceType, this.processingSourceReferences, this.dataSourcesToProcess, this.TypeIs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Enables the given cooker for processing when <see cref="Process"/>
        ///     is called.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The cooker to enable.
        /// </param>
        /// <exception cref="CookerNotFoundException">
        ///     <paramref name="dataCookerPath"/> is not known by this instance.
        ///     See <see cref="AllCookers"/>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void EnableCooker(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();

            if (!this.TryEnableCooker(dataCookerPath))
            {
                throw new CookerNotFoundException(dataCookerPath);
            }
        }

        /// <summary>
        ///     Attempts to enable the given cooker for processing when <see cref="Process"/>
        ///     is called.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The cooker to enable.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the cooker exists and can be enabled;
        ///     <c>false</c> otherwise. Note that <c>false</c> is
        ///     always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryEnableCooker(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();
            if (this.IsProcessed)
            {
                return false;
            }

            if (!this.AllCookers.Contains(dataCookerPath))
            {
                return false;
            }

            this.enabledCookers.Add(dataCookerPath);
            return true;
        }

        /// <summary>
        ///     Enables the given table for processing when <see cref="Process"/>
        ///     is called.
        /// </summary>
        /// <param name="descriptor">
        ///     The table to enable.
        /// </param>
        /// <exception cref="TableNotFoundException">
        ///     A table cannot be found for the given <paramref name="tableDescriptor"/>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="descriptor"/> cannot be null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void EnableTable(TableDescriptor descriptor)
        {
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();
            Guard.NotNull(descriptor, nameof(descriptor));

            if (!this.TryEnableTable(descriptor))
            {
                throw new TableNotFoundException(descriptor);
            }
        }

        /// <summary>
        ///     Attempts to enable the given cooker for processing when <see cref="Process"/>
        ///     is called.
        /// </summary>
        /// <param name="descriptor">
        ///     The table to enable.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the table exists and can be enabled;
        ///     <c>false</c> otherwise. Note that <c>false</c> is
        ///     always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="descriptor"/> cannot be null.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryEnableTable(TableDescriptor descriptor)
        {
            this.ThrowIfDisposed();
            Guard.NotNull(descriptor, nameof(descriptor));

            if (this.IsProcessed)
            {
                return false;
            }

            if (!this.allTables.Contains(descriptor))
            {
                return false;
            }

            if (this.enabledTables.Contains(descriptor))
            {
                return true;
            }

            this.enabledTables.Add(descriptor);
            return true;
        }

        /// <summary>
        ///     Processes all of the files given to this instance using all
        ///     of the enabled cookers.
        /// </summary>
        /// <returns>
        ///     The results of processing.
        /// </returns>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///   <see cref="Process"/> has already been called on this instance.
        /// </exception>
        /// <exception cref="EngineException">
        ///     An unexpected error occurred.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public RuntimeExecutionResults Process()
        {
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();
            try
            {
                return this.ProcessCore();
            }
            catch (Exception e)
            {
                throw new EngineException("An unexpected error occurred.", e);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Disoses all resources held by this class.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to dispose both managed and unmanaged
        ///     resources; <c>false</c> to dispose only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.runtimeExecutionResult.SafeDispose();
                this.extensionRoot.SafeDispose();

                this.applicationEnvironment = null;
                this.creationErrors = null;
                this.extensionDirectories = null;
                this.extensionRoot = null;
                this.factory = null;
                this.loader = null;
            }

            this.isDisposed = true;
        }

        /// <summary>
        ///     Throws an exception if this instance has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        protected void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(Engine));
            }
        }

        /// <summary>
        ///     Throws an exception if this instance has already processed.
        /// </summary>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already processed.
        /// </exception>
        protected void ThrowIfProcessed()
        {
            if (this.IsProcessed)
            {
                throw new InstanceAlreadyProcessedException();
            }
        }

        /// <summary>
        ///     Creates a logger for the given type.
        /// </summary>
        /// <param name="type">
        ///     The <c>Type</c> for which the logger is created.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="ILogger"/>.
        /// </returns>
        protected ILogger CreateLogger(Type type)
        {
            Debug.Assert(!this.isDisposed);
            this.ThrowIfDisposed();
            return loggerFactory?.Invoke(type) ?? Logger.Create(type);
        }

        private static Engine CreateCore(
            EngineCreateInfo createInfo)
        {
            Debug.Assert(createInfo != null);

            IPlugInCatalog catalog = null;
            IDataExtensionRepositoryBuilder repo = null;
            Engine instance = null;
            try
            {
                var extensionDirectories = createInfo.ExtensionDirectories.ToList().AsReadOnly();
                Debug.Assert(extensionDirectories.Any());

                var assemblyLoader = createInfo.AssemblyLoader ?? new AssemblyLoader();

                var assemblyDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, _ => new NullValidator());

                catalog = new ReflectionPlugInCatalog(assemblyDiscovery);

                var factory = new DataExtensionFactory();
                repo = factory.CreateDataExtensionRepository();
                var reflector = new DataExtensionReflector(
                    assemblyDiscovery,
                    repo);

                assemblyDiscovery.ProcessAssemblies(extensionDirectories, out var discoveryError);

                repo.FinalizeDataExtensions();

                var repoTuple = Tuple.Create(factory, repo);

                Debug.Assert(repoTuple != null);

                instance = new EngineImpl(createInfo.LoggerFactory);

                instance.ExtensionDirectories = extensionDirectories;
                instance.CreationErrors = new[] { discoveryError, };
                instance.processingSourceReferences.AddRange(catalog.PlugIns);
                instance.sourceDataCookers.AddRange(repoTuple.Item2.SourceDataCookers);
                instance.compositeDataCookers.AddRange(repoTuple.Item2.CompositeDataCookers);
                instance.extensionRoot = new ExtensionRoot(catalog, repo);

                instance.dataProcessors.AddRange(repoTuple.Item2.DataProcessors);

                instance.factory = repoTuple.Item1;

                string runtimeName = !string.IsNullOrWhiteSpace(createInfo.RuntimeName)
                    ? createInfo.RuntimeName
                    : "Microsoft.Performance.Toolkit.Engine";

                string applicationName = !string.IsNullOrWhiteSpace(createInfo.ApplicationName)
                    ? createInfo.ApplicationName
                    : string.Empty;

                instance.applicationEnvironment = new ApplicationEnvironment(
                    applicationName: applicationName,
                    runtimeName: runtimeName,
                    new RuntimeTableSynchronizer(),
                    new TableConfigurationsSerializer(),
                    instance.extensionRoot,
                    instance.factory.CreateSourceSessionFactory(),
                    new RuntimeMessageBox());

                instance.loader = assemblyLoader;

                foreach (var cds in instance.processingSourceReferences)
                {
                    try
                    {
                        cds.Instance.SetApplicationEnvironment(instance.applicationEnvironment);
                        cds.Instance.SetLogger(instance.CreateLogger(cds.Instance.GetType()));
                    }
                    catch (Exception e)
                    {
                        //
                        // todo: log
                        //

                        Console.Error.WriteLine(e);
                    }
                }

                var allTables = new HashSet<TableDescriptor>();
                foreach (var tableId in repoTuple.Item2.TablesById)
                {
                    allTables.Add(tableId.Value.TableDescriptor);
                    instance.tableGuidToDescriptor[tableId.Key] = tableId.Value.TableDescriptor;
                }

                foreach (var descriptor in catalog.PlugIns.SelectMany(x => x.AvailableTables))
                {
                    allTables.Add(descriptor);
                    instance.tableGuidToDescriptor[descriptor.Guid] = descriptor;
                }

                instance.allTables.AddRange(allTables);

                instance.IsProcessed = false;

                return instance;
            }
            catch
            {
                instance.SafeDispose();
                repo.SafeDispose();
                catalog.SafeDispose();
                throw;
            }
        }

        private void AddDataSourcesCore(
            IEnumerable<IDataSource> dataSources,
            Type processingSourceType,
            List<ProcessingSourceReference> processingSourceReferences,
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Func<Type, Type, bool> typeIs)
        {
            Debug.Assert(dataSources != null);
            Debug.Assert(processingSourceType != null);
            Debug.Assert(processingSourceReferences != null);
            Debug.Assert(dataSourcesToProcess != null);
            Debug.Assert(typeIs != null);

            Debug.Assert(!this.IsProcessed);

            var cdsr = processingSourceReferences.FirstOrDefault(x => typeIs(x.Instance.GetType(), processingSourceType));
            if (cdsr is null)
            {
                throw new UnsupportedProcessingSourceException(processingSourceType);
            }

            var atLeastOneDataSourceProvided = false;
            foreach (var dataSource in dataSources)
            {
                Debug.Assert(dataSource != null);

                atLeastOneDataSourceProvided = true;
                if (!cdsr.Supports(dataSource))
                {
                    throw new UnsupportedDataSourceException(dataSource, processingSourceType);
                }
            }

            if (!atLeastOneDataSourceProvided)
            {
                throw new ArgumentException("The Data Source collection cannot be empty.", nameof(dataSources));
            }

            if (!dataSourcesToProcess.TryGetValue(cdsr, out var list))
            {
                list = new List<List<IDataSource>>();
                dataSourcesToProcess[cdsr] = list;
            }

            list.Add(dataSources.ToList());
        }

        private RuntimeExecutionResults ProcessCore()
        {
            this.IsProcessed = true;

            var allDataSourceAssociations = GroupAllDataSourcesToProcessingSources(
                this.processingSourceReferences,
                this.freeDataSources,
                this.dataSourcesToProcess);

            // This contains the set of composite cookers that will be used by the system of data processors available
            // to this RuntimeExecutionResults.
            var compositeCookers = new ProcessingSystemCompositeCookers(this.extensionRoot);

            var executors = CreateExecutors(allDataSourceAssociations, compositeCookers);

            var extendedTables = new HashSet<ITableExtensionReference>();
            var processorTables = new Dictionary<TableDescriptor, ICustomDataProcessor>();

            foreach (var table in this.enabledTables)
            {
                if (this.extensionRoot.TablesById.TryGetValue(table.Guid, out ITableExtensionReference reference))
                {
                    extendedTables.Add(reference);
                }
                else
                {
                    var executor = executors.Single(x => x.Context.ProcessingSource.AvailableTables.Contains(table));
                    executor.Processor.EnableTable(table);
                    processorTables.Add(table, executor.Processor);
                }
            }

            var processors = this.extensionRoot.EnableDataCookers(
                executors.Select(x => x.Processor as ICustomDataProcessorWithSourceParser).Where(x => !(x is null)),
                new HashSet<DataCookerPath>(this.enabledCookers));

            if (extendedTables.Any())
            {
                var processorForTable = this.extensionRoot.EnableSourceDataCookersForTables(
                executors.Select(x => x.Processor as ICustomDataProcessorWithSourceParser).Where(x => !(x is null)),
                extendedTables);

                processors.UnionWith(processorForTable);
            }

            var executionResults = new List<ExecutionResult>(executors.Count);
            foreach (var executor in executors)
            {
                try
                {
                    var executionResult = executor.ExecuteAsync(CancellationToken.None)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                    executionResults.Add(executionResult);
                }
                catch (Exception e)
                {
                    //
                    // todo: log
                    //

                    Console.Error.WriteLine(e);
                }
            }

            var sourceCookerDataRetrieval = this.factory.CreateCrossParserSourceDataCookerRetrieval(processors);
            var dataRetrievalFactory = new DataExtensionRetrievalFactory(
                sourceCookerDataRetrieval,
                compositeCookers,
                this.extensionRoot);

            compositeCookers.Initialize(dataRetrievalFactory);

            this.runtimeExecutionResult = new RuntimeExecutionResults(
                sourceCookerDataRetrieval,
                compositeCookers,
                dataRetrievalFactory,
                this.extensionRoot,
                processorTables);

            return this.runtimeExecutionResult;
        }

        private List<ProcessingSourceExecutor> CreateExecutors(
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> allDataSourceAssociations,
            ProcessingSystemCompositeCookers compositeCookers)
        {
            var executors = new List<ProcessingSourceExecutor>();
            foreach (var kvp in allDataSourceAssociations)
            {
                var processingSource = kvp.Key;
                var dataSourceLists = kvp.Value;

                foreach (var dataSources in dataSourceLists)
                {
                    try
                    {
                        var executionContext = new SDK.Runtime.ExecutionContext(
                            new DataProcessorProgress(),
                            x => ConsoleLogger.Create(x.GetType()),
                            processingSource,
                            dataSources,
                            processingSource.Instance.DataTables.Concat(processingSource.Instance.MetadataTables),
                            new RuntimeProcessorEnvironment(this.extensionRoot, compositeCookers, this.CreateLogger),
                            ProcessorOptions.Default);

                        var executor = new ProcessingSourceExecutor();
                        executor.InitializeCustomDataProcessor(executionContext);

                        executors.Add(executor);
                    }
                    catch (Exception e)
                    {
                        //
                        // todo: log
                        //

                        Console.Error.WriteLine(e);
                        //this.logger.Error(
                        //    "Error processing dataSources {0} on processing source {1}: {2}",
                        //    dataSources.ToString(),
                        //    processingSource.Name,
                        //    e);
                    }
                }
            }

            return executors;
        }

        private static Dictionary<ProcessingSourceReference, List<List<IDataSource>>> GroupAllDataSourcesToProcessingSources(
            IEnumerable<ProcessingSourceReference> processingSources,
            IEnumerable<IDataSource> freeDataSourcesToProcess,
            IReadOnlyDictionary<ProcessingSourceReference, List<List<IDataSource>>> dataSourcesToProcess)
        {
            var allDataSourceAssociations = new Dictionary<ProcessingSourceReference, List<List<IDataSource>>>();

            var freeDataSourceAssociations = DataSourceResolver.Assign(freeDataSourcesToProcess, processingSources);

            foreach (var kvp in dataSourcesToProcess)
            {
                var cds = kvp.Key;
                var dataSourceLists = kvp.Value;

                if (!allDataSourceAssociations.TryGetValue(cds, out List<List<IDataSource>> existingDataSourceLists))
                {
                    existingDataSourceLists = new List<List<IDataSource>>();
                    allDataSourceAssociations[cds] = existingDataSourceLists;
                }

                foreach (var list in dataSourceLists)
                {
                    existingDataSourceLists.Add(list.ToList());
                }
            }

            foreach (var kvp in freeDataSourceAssociations)
            {
                if (!allDataSourceAssociations.TryGetValue(kvp.Key, out List<List<IDataSource>> existingDataSourceLists))
                {
                    existingDataSourceLists = new List<List<IDataSource>>();
                    allDataSourceAssociations[kvp.Key] = existingDataSourceLists;
                }

                foreach (var file in kvp.Value)
                {
                    existingDataSourceLists.Add(new List<IDataSource> { file, });
                }
            }

            foreach (var kvp in allDataSourceAssociations.ToArray())
            {
                if (kvp.Value.Count == 0)
                {
                    allDataSourceAssociations.Remove(kvp.Key);
                }
            }

            return allDataSourceAssociations;
        }

        private bool TypeIs(Type first, Type second)
        {
            Debug.Assert(first != null);
            Debug.Assert(second != null);

            if (this.loader.SupportsIsolation)
            {
                return first.GUID == second.GUID &&
                       first.AssemblyQualifiedName == second.AssemblyQualifiedName;
            }
            else
            {
                return first.Is(second);
            }
        }

        private sealed class EngineImpl
            : Engine
        {
            internal EngineImpl(Func<Type, ILogger> loggerFactory)
                : base(loggerFactory)
            {
            }
        }

        private sealed class RuntimeProcessorEnvironment
            : IProcessorEnvironment
        {
            private readonly ProcessingSystemCompositeCookers compositeCookers;
            private readonly IDataExtensionRepository repository;
            private readonly Func<Type, ILogger> loggerFactory;
            private readonly object loggerLock = new object();

            private ILogger logger;
            private Type processorType;

            public RuntimeProcessorEnvironment(
                IDataExtensionRepository repository,
                ProcessingSystemCompositeCookers compositeCookers,
                Func<Type, ILogger> loggerFactory)
            {
                Debug.Assert(repository != null);
                Debug.Assert(compositeCookers != null);
                Debug.Assert(loggerFactory != null);

                this.compositeCookers = compositeCookers;
                this.repository = repository;
                this.loggerFactory = loggerFactory;
            }

            public IDataProcessorExtensibilitySupport CreateDataProcessorExtensibilitySupport(
                ICustomDataProcessorWithSourceParser processor)
            {
                return new CustomDataProcessorExtensibilitySupport(
                    processor,
                    this.repository,
                    this.compositeCookers);
            }

            public ILogger CreateLogger(Type processorType)
            {
                Guard.NotNull(processorType, nameof(processorType));

                lock (this.loggerLock)
                {
                    if (logger != null)
                    {
                        if (this.processorType != processorType)
                        {
                            throw new ArgumentException(
                                $"{nameof(CreateLogger)} cannot be called with multiple types in a single instance.",
                                nameof(processorType));
                        }

                        return this.logger;
                    }

                    this.processorType = processorType;
                    this.logger = this.loggerFactory(processorType);
                    return this.logger;
                }
            }

            public IDynamicTableBuilder RequestDynamicTableBuilder(
                TableDescriptor descriptor)
            {
                return null;
            }
        }

        private sealed class RuntimeTableSynchronizer
            : ITableDataSynchronization
        {
            public void SubmitColumnChangeRequest(
                IEnumerable<Guid> columns,
                Action onReadyForChange,
                Action onChangeComplete,
                bool requestInitialFilterReevaluation = false)
            {
                onReadyForChange?.Invoke();
                onChangeComplete?.Invoke();
            }

            public void SubmitColumnChangeRequest(
                Func<IProjectionDescription, bool> predicate,
                Action onReadyForChange,
                Action onChangeComplete,
                bool requestInitialFilterReevaluation = false)
            {
                onReadyForChange?.Invoke();
                onChangeComplete?.Invoke();
            }
        }

        private sealed class RuntimeMessageBox
            : IMessageBox
        {
            public void Show(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                string format,
                params object[] args)
            {
                var message = string.Format(formatProvider, format, args);

                Console.Error.WriteLine(
                    string.Concat("[", icon.ToString(), "]: ", message));
            }

            public ButtonResult Show(MessageBoxIcon icon, IFormatProvider formatProvider, Buttons buttons, string caption, string format, params object[] args)
            {
                var message = string.Format(formatProvider, format, args);

                Console.Error.WriteLine(
                    string.Concat("[", icon.ToString(), "]: ", message));

                // todo: how do we want to handle this?
                // User prompts? Default?
                return ButtonResult.None;
            }
        }

        private sealed class NullValidator
            : IPreloadValidator
        {
            public bool IsAssemblyAcceptable(string fullPath, out ErrorInfo error)
            {
                error = ErrorInfo.None;
                return true;
            }

            public void Dispose()
            {
            }
        }
    }
}

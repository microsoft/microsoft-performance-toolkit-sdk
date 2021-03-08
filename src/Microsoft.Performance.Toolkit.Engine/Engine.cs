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
        private List<CustomDataSourceReference> customDataSourceReferences;
        private ReadOnlyCollection<CustomDataSourceReference> customDataSourceReferencesRO;

        private List<DataCookerPath> sourceDataCookers;
        private ReadOnlyCollection<DataCookerPath> sourceDataCookersRO;

        private List<DataCookerPath> compositeDataCookers;
        private ReadOnlyCollection<DataCookerPath> compositeDataCookersRO;

        private readonly Dictionary<Guid, TableDescriptor> tableGuidToDescriptor;
        private readonly List<TableDescriptor> allTables;
        private readonly ReadOnlyCollection<TableDescriptor> allTablesRO;

        private List<DataProcessorId> dataProcessors;

        private Dictionary<CustomDataSourceReference, List<List<IDataSource>>> dataSourcesToProcess;

        private List<IDataSource> freeDataSources;
        private ReadOnlyCollection<IDataSource> freeDataSourcesRO;

        private List<DataCookerPath> enabledCookers;
        private ReadOnlyCollection<DataCookerPath> enabledCookersRO;

        // TRGIBEAU: Maybe make this a hashset?
        private readonly List<TableDescriptor> enabledTables;
        private readonly ReadOnlyCollection<TableDescriptor> enabledTablesRO;

        private IDataExtensionRepositoryBuilder repository;
        private DataExtensionFactory factory;
        private ExtensionRoot extensionRoot;

        private string extensionDirectory;
        private bool isProcessed;
        private IEnumerable<ErrorInfo> creationErrors;

        private IApplicationEnvironment applicationEnvironment;
        private IAssemblyLoader loader;
        private bool isDisposed;

        internal Engine()
        {
            this.customDataSourceReferences = new List<CustomDataSourceReference>();
            this.customDataSourceReferencesRO = new ReadOnlyCollection<CustomDataSourceReference>(this.customDataSourceReferences);

            this.sourceDataCookers = new List<DataCookerPath>();
            this.sourceDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.sourceDataCookers);

            this.compositeDataCookers = new List<DataCookerPath>();
            this.compositeDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.compositeDataCookers);

            this.tableGuidToDescriptor = new Dictionary<Guid, TableDescriptor>();
            this.allTables = new List<TableDescriptor>();
            this.allTablesRO = new ReadOnlyCollection<TableDescriptor>(this.allTables);

            this.dataProcessors = new List<DataProcessorId>();

            this.dataSourcesToProcess = new Dictionary<CustomDataSourceReference, List<List<IDataSource>>>();

            this.freeDataSources = new List<IDataSource>();
            this.freeDataSourcesRO = new ReadOnlyCollection<IDataSource>(this.freeDataSources);

            this.enabledCookers = new List<DataCookerPath>();
            this.enabledCookersRO = new ReadOnlyCollection<DataCookerPath>(this.enabledCookers);

            this.enabledTables = new List<TableDescriptor>();
            this.enabledTablesRO = new ReadOnlyCollection<TableDescriptor>(this.enabledTables);

            this.isDisposed = false;
        }

        /// <summary>
        ///     Gets the directory that the engine will scan for extensions.
        ///     This directory is scanned during <see cref="Create"/> when
        ///     creating an instance of the engine. See <see cref="SetExtensionDirectory(string)"/>
        ///     and <see cref="ResetExtensionDirectory"/> for manipulating this property.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public string ExtensionDirectory
        {
            get
            {
                this.ThrowIfDisposed();
                return this.extensionDirectory;
            }
            private set
            {
                Debug.Assert(!this.isDisposed);
                this.ThrowIfDisposed();
                this.extensionDirectory = value;
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
        public IEnumerable<ICustomDataSource> CustomDataSources
        {
            get
            {
                this.ThrowIfDisposed();
                return this.customDataSourceReferencesRO.Select(x => x.Instance);
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
        ///     Gets the collection of Data Sources that are to be processed by a specific Custom Data Source.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public IReadOnlyDictionary<ICustomDataSource, IReadOnlyList<IReadOnlyList<IDataSource>>> DataSourcesToProcess
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
        public IEnumerable<TableDescriptor> EnableTables => this.enabledTablesRO;

        /// <summary>
        ///     Gets the collection of available tables
        /// </summary>
        public IEnumerable<TableDescriptor> AllTables => this.allTablesRO;

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

            if (!this.customDataSourceReferences.Any(x => x.Supports(dataSource)))
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
        ///     the specific Custom Data Source.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="customDataSourceType">
        ///     The Custom Data Source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedCustomDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> is unknown.
        /// </exception>
        public void AddDataSource(IDataSource dataSource, Type customDataSourceType)
        {
            this.AddDataSources(new[] { dataSource, }, customDataSourceType);
        }

        /// <summary>
        ///     Attempts to add the given Data Source to this instance for processing by
        ///     the specific Custom Data Source.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="customDataSourceType">
        ///     The Custom Data Source to use to process <paramref name="dataSource"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Data Source has been added for processing by the Custom Data Source;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSource(IDataSource dataSource, Type customDataSourceType)
        {
            return this.TryAddDataSources(new[] { dataSource, }, customDataSourceType);
        }

        /// <summary>
        ///     Adds the given data sources to this instance for processing by
        ///     the specific Custom Data Source. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="customDataSourceType">
        ///     The Custom Data Source to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="customDataSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedCustomDataSourceException">
        ///     The specified <paramref name="customDataSourceType"/> is unknown.
        /// </exception>
        public void AddDataSources(IEnumerable<IDataSource> dataSources, Type customDataSourceType)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(customDataSourceType, nameof(customDataSourceType));
            if (dataSources.Any(x => x is null))
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            this.ThrowIfDisposed();
            this.ThrowIfProcessed();

            this.AddDataSourcesCore(dataSources, customDataSourceType, this.customDataSourceReferences, this.dataSourcesToProcess, this.TypeIs);
        }

        /// <summary>
        ///     Attempts to add the given data sources to this instance for processing by
        ///     the specific Custom Data Source. All of the files will be processed
        ///     by the same instance of the Custom Data Processor. Use <see cref="AddDataSource(IDataSource, Type)"/>
        ///     to ensure each Data Source is processed by a different instance, or
        ///     use multiple calls to <see cref="AddDataSources(IEnumerable{IDataSource}, Type)"/>.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="customDataSourceType">
        ///     The Custom Data Source to use to process the <paramref name="dataSources"/>.
        /// </param>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryAddDataSources(IEnumerable<IDataSource> dataSources, Type customDataSourceType)
        {
            this.ThrowIfDisposed();

            if (dataSources is null ||
                dataSources.Any(x => x is null) ||
                customDataSourceType is null ||
                this.IsProcessed)
            {
                return false;
            }

            try
            {
                this.AddDataSourcesCore(dataSources, customDataSourceType, this.customDataSourceReferences, this.dataSourcesToProcess, this.TypeIs);
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

        public void EnableTable(TableDescriptor descriptor)
        {
            this.ThrowIfProcessed();

            if (!this.TryEnableTable(descriptor))
            {
                throw new TableNotFoundException(descriptor);
            }
        }

        public bool TryEnableTable(TableDescriptor descriptor)
        {
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
                this.extensionRoot.SafeDispose();

                this.allTables = null;
                this.applicationEnvironment = null;
                this.compositeDataCookers = null;
                this.creationErrors = null;
                this.customDataSourceReferences = null;
                this.dataProcessors = null;
                this.dataSourcesToProcess = null;
                this.enabledCookers = null;
                this.extensionDirectory = null;
                this.extensionRoot = null;
                this.factory = null;
                this.freeDataSources = null;
                this.loader = null;
                this.sourceDataCookers = null;
                this.tableGuidToDescriptor = null;
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

        private static Engine CreateCore(
            EngineCreateInfo createInfo)
        {
            Debug.Assert(createInfo != null);

            IPlugInCatalog catalog = null;
            IDataExtensionRepositoryBuilder repo = null;
            Engine instance = null;
            try
            {

                var extensionDirectory = createInfo.ExtensionDirectory;
                Debug.Assert(extensionDirectory != null);

                var assemblyLoader = createInfo.AssemblyLoader ?? new AssemblyLoader();

                var assemblyDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, _ => new NullValidator());

                catalog = new ReflectionPlugInCatalog(assemblyDiscovery);

                var factory = new DataExtensionFactory();
                repo = factory.CreateDataExtensionRepository();
                var reflector = new DataExtensionReflector(
                    assemblyDiscovery,
                    repo);

                assemblyDiscovery.ProcessAssemblies(extensionDirectory, out var discoveryError);

                repo.FinalizeDataExtensions();

                var repoTuple = Tuple.Create(factory, repo);

                Debug.Assert(repoTuple != null);

                instance = new EngineImpl();

                instance.ExtensionDirectory = extensionDirectory;
                instance.CreationErrors = new[] { discoveryError, };
                instance.customDataSourceReferences.AddRange(catalog.PlugIns);
                instance.sourceDataCookers.AddRange(repoTuple.Item2.SourceDataCookers);
                instance.compositeDataCookers.AddRange(repoTuple.Item2.CompositeDataCookers);
                instance.extensionRoot = new ExtensionRoot(catalog, repo);

                // TRGIBEAU: Do we need anything else off of the Reference?
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

                instance.dataProcessors.AddRange(repoTuple.Item2.DataProcessors);

                instance.factory = repoTuple.Item1;

                instance.applicationEnvironment = new ApplicationEnvironment(
                    applicationName: string.Empty,
                    runtimeName: "Microsoft.Performance.Toolkit.Engine",
                    new RuntimeTableSynchronizer(),
                    new TableConfigurationsSerializer(),
                    instance.extensionRoot,
                    instance.factory.CreateSourceSessionFactory(),
                    new RuntimeMessageBox());

                instance.loader = assemblyLoader;

                foreach (var cds in instance.customDataSourceReferences)
                {
                    try
                    {
                        cds.Instance.SetApplicationEnvironment(instance.applicationEnvironment);

                        // todo: CreateLogger func should be passed in from the EngineCreateInfo
                        cds.Instance.SetLogger(Logger.Create(cds.Instance.GetType()));
                    }
                    catch (Exception e)
                    {
                        //
                        // todo: log
                        //

                        Console.Error.WriteLine(e);
                    }
                }

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
            Type customDataSourceType,
            List<CustomDataSourceReference> customDataSourceReferences,
            Dictionary<CustomDataSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Func<Type, Type, bool> typeIs)
        {
            Debug.Assert(dataSources != null);
            Debug.Assert(customDataSourceType != null);
            Debug.Assert(customDataSourceReferences != null);
            Debug.Assert(dataSourcesToProcess != null);
            Debug.Assert(typeIs != null);

            Debug.Assert(!this.IsProcessed);

            var cdsr = customDataSourceReferences.FirstOrDefault(x => typeIs(x.Instance.GetType(), customDataSourceType));
            if (cdsr is null)
            {
                throw new UnsupportedCustomDataSourceException(customDataSourceType);
            }

            var atLeastOneDataSourceProvided = false;
            foreach (var dataSource in dataSources)
            {
                Debug.Assert(dataSource != null);

                atLeastOneDataSourceProvided = true;
                if (!cdsr.Supports(dataSource))
                {
                    throw new UnsupportedDataSourceException(dataSource, customDataSourceType);
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

            var allDataSourceAssociations = GroupAllDataSourcesToCustomDataSources(
                this.customDataSourceReferences,
                this.freeDataSources,
                this.dataSourcesToProcess);

            var executors = CreateExecutors(allDataSourceAssociations);

            var extendedTables = new HashSet<ITableExtensionReference>();
            var processorTables = new Dictionary<TableDescriptor, ICustomDataProcessor>();

            foreach (var table in this.enabledTables)
            {
                foreach (var executor in executors.Where(x => x.Context.CustomDataSource.DataTables.Contains(table)))
                {
                    executor.Processor.EnableTable(table);
                    processorTables.Add(table, executor.Processor);
                }

                if (this.repository.TablesById.TryGetValue(table.Guid, out ITableExtensionReference reference))
                {
                    extendedTables.Add(reference);
                }
            }

            var processors = this.extensionRoot.EnableDataCookers(
                executors.Select(x => x.Processor as ICustomDataProcessorWithSourceParser).Where(x => !(x is null)),
                new HashSet<DataCookerPath>(this.enabledCookers));


            if (extendedTables.Any())
            {
                var processorForTable = this.repository.EnableSourceDataCookersForTables(
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

            var retrieval = this.factory.CreateCrossParserSourceDataCookerRetrieval(processors);
            var retrievalFactory = new DataExtensionRetrievalFactory(retrieval, this.extensionRoot);

            var results = new RuntimeExecutionResults(
                retrieval,
                retrievalFactory,
                this.extensionRoot,
                executors);

            return results;
        }

        private List<CustomDataSourceExecutor> CreateExecutors(
            Dictionary<CustomDataSourceReference, List<List<IDataSource>>> allDataSourceAssociations)
        {
            var executors = new List<CustomDataSourceExecutor>();
            foreach (var kvp in allDataSourceAssociations)
            {
                try
                {
                    var cds = kvp.Key;
                    var dataSourceLists = kvp.Value;

                    foreach (var dataSources in dataSourceLists)
                    {
                        var executionContext = new SDK.Runtime.ExecutionContext(
                            new DataProcessorProgress(),
                            x => ConsoleLogger.Create(x.GetType()),
                            cds,
                            dataSources,
                            cds.Instance.DataTables.Concat(cds.Instance.MetadataTables),
                            new RuntimeProcessorEnvironment(this.extensionRoot),
                            ProcessorOptions.Default);

                        var executor = new CustomDataSourceExecutor();
                        executor.InitializeCustomDataProcessor(executionContext);

                        executors.Add(executor);
                    }
                }
                catch (Exception e)
                {
                    //
                    // todo: log
                    //

                    Console.Error.WriteLine(e);
                }
            }

            return executors;
        }

        private static Dictionary<CustomDataSourceReference, List<List<IDataSource>>> GroupAllDataSourcesToCustomDataSources(
            IEnumerable<CustomDataSourceReference> customDataSources,
            IEnumerable<IDataSource> freeDataSourcesToProcess,
            IReadOnlyDictionary<CustomDataSourceReference, List<List<IDataSource>>> dataSourcesToProcess)
        {
            var allDataSourceAssociations = new Dictionary<CustomDataSourceReference, List<List<IDataSource>>>();

            var freeDataSourceAssociations = DataSourceResolver.Assign(freeDataSourcesToProcess, customDataSources);

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
        }

        private sealed class RuntimeProcessorEnvironment
            : IProcessorEnvironment
        {
            private readonly IDataExtensionRepository repository;
            private readonly object loggerLock = new object();

            private ILogger logger;
            private Type processorType;

            public RuntimeProcessorEnvironment(
                IDataExtensionRepository repository)
            {
                Debug.Assert(repository != null);

                this.repository = repository;
            }

            public IDataProcessorExtensibilitySupport CreateDataProcessorExtensibilitySupport(
                ICustomDataProcessorWithSourceParser processor)
            {
                return new CustomDataProcessorExtensibilitySupport(processor, this.repository);
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
                    this.logger = Logger.Create(processorType);
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
            }

            public void SubmitColumnChangeRequest(
                Func<IProjectionDescription, bool> predicate,
                Action onReadyForChange,
                Action onChangeComplete,
                bool requestInitialFilterReevaluation = false)
            {
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

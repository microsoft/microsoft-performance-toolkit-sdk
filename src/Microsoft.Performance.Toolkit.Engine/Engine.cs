// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
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

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Provides the entry point for programmatically manipulating, cooking,
    ///     and accessing data.
    /// </summary>
    public abstract class Engine
    {
        private readonly List<CustomDataSourceReference> customDataSourceReferences;
        private readonly ReadOnlyCollection<CustomDataSourceReference> customDataSourceReferencesRO;

        private readonly List<DataCookerPath> sourceDataCookers;
        private readonly ReadOnlyCollection<DataCookerPath> sourceDataCookersRO;

        private readonly List<DataCookerPath> compositeDataCookers;
        private readonly ReadOnlyCollection<DataCookerPath> compositeDataCookersRO;

        private readonly Dictionary<Guid, TableDescriptor> tableGuidToDescriptor;
        private readonly List<Guid> allTables;

        private readonly List<DataProcessorId> dataProcessors;

        private readonly Dictionary<CustomDataSourceReference, List<List<IDataSource>>> dataSourcesToProcess;

        private readonly List<IDataSource> freeDataSources;
        private readonly ReadOnlyCollection<IDataSource> freeDataSourcesRO;

        private readonly List<DataCookerPath> enabledCookers;
        private readonly ReadOnlyCollection<DataCookerPath> enabledCookersRO;

        private IDataExtensionRepositoryBuilder repository;
        private DataExtensionFactory factory;

        private IApplicationEnvironment applicationEnvironment;
        private IAssemblyLoader loader;

        internal Engine()
        {
            this.customDataSourceReferences = new List<CustomDataSourceReference>();
            this.customDataSourceReferencesRO = new ReadOnlyCollection<CustomDataSourceReference>(this.customDataSourceReferences);

            this.sourceDataCookers = new List<DataCookerPath>();
            this.sourceDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.sourceDataCookers);

            this.compositeDataCookers = new List<DataCookerPath>();
            this.compositeDataCookersRO = new ReadOnlyCollection<DataCookerPath>(this.compositeDataCookers);

            this.tableGuidToDescriptor = new Dictionary<Guid, TableDescriptor>();
            this.allTables = new List<Guid>();

            this.dataProcessors = new List<DataProcessorId>();

            this.dataSourcesToProcess = new Dictionary<CustomDataSourceReference, List<List<IDataSource>>>();

            this.freeDataSources = new List<IDataSource>();
            this.freeDataSourcesRO = new ReadOnlyCollection<IDataSource>(this.freeDataSources);

            this.enabledCookers = new List<DataCookerPath>();
            this.enabledCookersRO = new ReadOnlyCollection<DataCookerPath>(this.enabledCookers);
        }

        /// <summary>
        ///     Gets the directory that the engine will scan for extensions.
        ///     This directory is scanned during <see cref="Create"/> when
        ///     creating an instance of the engine. See <see cref="SetExtensionDirectory(string)"/>
        ///     and <see cref="ResetExtensionDirectory"/> for manipulating this property.
        /// </summary>
        public string ExtensionDirectory { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance has completed processing.
        /// </summary>
        public bool IsProcessed { get; private set; }

        /// <summary>
        ///     Gets the collection of source parsers that the engine will use to 
        ///     cook data.
        /// </summary>
        public IEnumerable<ICustomDataSource> CustomDataSources => this.customDataSourceReferencesRO.Select(x => x.Instance);

        /// <summary>
        ///     Gets the collection of paths to all source cookers that the engine
        ///     has discovered.
        /// </summary>
        public IEnumerable<DataCookerPath> SourceDataCookers => this.sourceDataCookersRO;

        /// <summary>
        ///     Gets the collection of paths to all composite cookers that the engine
        ///     has discovered.
        /// </summary>
        public IEnumerable<DataCookerPath> CompositeDataCookers => this.compositeDataCookersRO;

        /// <summary>
        ///     Gets the collection of all cookers that the engine has discovered. This is the union
        ///     of <see cref="this.SourceDataCookers"/> and <see cref="this.CompositeDataCookers"/>.
        /// </summary>
        public IEnumerable<DataCookerPath> AllCookers => this.SourceDataCookers.Concat(this.CompositeDataCookers);

        /// <summary>
        ///     Gets the collection of Data Sources to process. These Data Sources will be processed in whatever
        ///     <see cref="ICustomDataProcessorWithSourceParser"/> are able to handle the Data Source.
        /// </summary>
        public IEnumerable<IDataSource> FreeDataSourcesToProcess => this.freeDataSourcesRO;

        /// <summary>
        ///     Gets the collection of Data Sources that are to be processed by a specific Custom Data Source.
        /// </summary>
        public IReadOnlyDictionary<ICustomDataSource, IReadOnlyList<IReadOnlyList<IDataSource>>> DataSourcesToProcess =>
            this.dataSourcesToProcess.ToDictionary(
                x => x.Key.Instance,
                x => (IReadOnlyList<IReadOnlyList<IDataSource>>)x.Value.Select(v => (IReadOnlyList<IDataSource>)v.AsReadOnly()).ToList().AsReadOnly());

        /// <summary>
        ///     Gets the collection of all enabled cookers.
        /// </summary>
        public IEnumerable<DataCookerPath> EnabledCookers => this.enabledCookersRO;

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="EngineException">
        ///     An error occurred while creating the engine.
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
        ///     An error occurred while creating the engine.
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
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="dataSource"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public void AddDataSource(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));
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
        public bool TryAddDataSource(IDataSource dataSource)
        {
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

            this.ThrowIfProcessed();

            if (!this.AddDataSourcesCore(dataSources, customDataSourceType, this.customDataSourceReferences, this.dataSourcesToProcess, this.TypeIs, out var e))
            {
                Debug.Assert(e != null);
                e.Throw();
            }
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
        public bool TryAddDataSources(IEnumerable<IDataSource> dataSources, Type customDataSourceType)
        {
            if (dataSources is null ||
                dataSources.Any(x => x is null) ||
                customDataSourceType is null ||
                this.IsProcessed)
            {
                return false;
            }

            return this.AddDataSourcesCore(dataSources, customDataSourceType, this.customDataSourceReferences, this.dataSourcesToProcess, this.TypeIs, out _);
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
        public void EnableCooker(DataCookerPath dataCookerPath)
        {
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
        public bool TryEnableCooker(DataCookerPath dataCookerPath)
        {
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
        public RuntimeExecutionResults Process()
        {
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

        private static Engine CreateCore(
            EngineCreateInfo createInfo)
        {
            Debug.Assert(createInfo != null);

            var extensionDirectory = createInfo.ExtensionDirectory;
            Debug.Assert(extensionDirectory != null);

            var assemblyLoader = createInfo.AssemblyLoader ?? new AssemblyLoader();

            var assemblyDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, _ => new NullValidator());

            var catalog = new ReflectionPlugInCatalog(assemblyDiscovery);

            var factory = new DataExtensionFactory();
            var repoBuilder = factory.CreateDataExtensionRepository();
            var reflector = new DataExtensionReflector(
                assemblyDiscovery,
                repoBuilder);

            assemblyDiscovery.ProcessAssemblies(extensionDirectory, out _);

            repoBuilder.FinalizeDataExtensions();

            var repoTuple = Tuple.Create(factory, repoBuilder);

            Debug.Assert(repoTuple != null);

            var instance = new EngineImpl();

            instance.ExtensionDirectory = extensionDirectory;
            instance.customDataSourceReferences.AddRange(catalog.PlugIns);
            instance.sourceDataCookers.AddRange(repoTuple.Item2.SourceDataCookers);
            instance.compositeDataCookers.AddRange(repoTuple.Item2.CompositeDataCookers);

            var allTables = new HashSet<Guid>();
            foreach (var tableId in repoTuple.Item2.TablesById)
            {
                allTables.Add(tableId.Key);
            }

            foreach (var descriptor in catalog.PlugIns.SelectMany(x => x.AvailableTables))
            {
                allTables.Add(descriptor.Guid);
                instance.tableGuidToDescriptor[descriptor.Guid] = descriptor;
            }

            instance.allTables.AddRange(allTables);

            instance.dataProcessors.AddRange(repoTuple.Item2.DataProcessors);

            instance.repository = repoTuple.Item2;
            instance.factory = repoTuple.Item1;

            instance.applicationEnvironment = new ApplicationEnvironment(
                applicationName: string.Empty,
                runtimeName: "Microsoft.Performance.Toolkit.Engine",
                new RuntimeTableSynchronizer(),
                new TableConfigurationsSerializer(),
                instance.repository,
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

        private bool AddDataSourcesCore(
            IEnumerable<IDataSource> dataSources,
            Type customDataSourceType,
            List<CustomDataSourceReference> customDataSourceReferences,
            Dictionary<CustomDataSourceReference, List<List<IDataSource>>> dataSourcesToProcess,
            Func<Type, Type, bool> typeIs,
            out ExceptionDispatchInfo error)
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
                error = ExceptionDispatchInfo.Capture(new UnsupportedCustomDataSourceException(customDataSourceType));
                return false;
            }

            var atLeastOneDataSourceProvided = false;
            foreach (var dataSource in dataSources)
            {
                Debug.Assert(dataSource != null);

                atLeastOneDataSourceProvided = true;
                if (!cdsr.Supports(dataSource))
                {
                    error = ExceptionDispatchInfo.Capture(new UnsupportedDataSourceException(dataSource, customDataSourceType));
                    return false;
                }
            }

            if (!atLeastOneDataSourceProvided)
            {
                error = ExceptionDispatchInfo.Capture(new ArgumentException("The Data Source collection cannot be empty.", nameof(dataSources)));
                return false;
            }

            if (!dataSourcesToProcess.TryGetValue(cdsr, out var list))
            {
                list = new List<List<IDataSource>>();
                dataSourcesToProcess[cdsr] = list;
            }

            list.Add(dataSources.ToList());

            error = null;
            return true;
        }

        private RuntimeExecutionResults ProcessCore()
        {
            this.IsProcessed = true;

            var allDataSourceAssociations = GroupAllDataSourcesToCustomDataSources(
                this.customDataSourceReferences,
                this.freeDataSources,
                this.dataSourcesToProcess);

            var executors = CreateExecutors(allDataSourceAssociations);
            var processors = this.repository.EnableDataCookers(
                executors.Select(x => x.Processor as ICustomDataProcessorWithSourceParser).Where(x => !(x is null)),
                new HashSet<DataCookerPath>(this.enabledCookers));

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
            var retrievalFactory = new DataExtensionRetrievalFactory(retrieval, this.repository);

            var results = new RuntimeExecutionResults(
                retrieval,
                retrievalFactory,
                this.repository);

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
                    var cds = kvp.Key.Instance;
                    var dataSourceLists = kvp.Value;

                    foreach (var dataSources in dataSourceLists)
                    {
                        var executionContext = new SDK.Runtime.ExecutionContext(
                            new DataProcessorProgress(),
                            x => ConsoleLogger.Create(x.GetType()),
                            cds,
                            dataSources,
                            cds.DataTables.Concat(cds.MetadataTables),
                            new RuntimeProcessorEnvironment(this.repository),
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

        private void ThrowIfProcessed()
        {
            if (this.IsProcessed)
            {
                throw new InstanceAlreadyProcessedException();
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

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

        private readonly Dictionary<CustomDataSourceReference, List<List<string>>> filesToProcess;

        private readonly List<string> freeFilesToProcess;
        private readonly ReadOnlyCollection<string> freeFilesToProcessRO;

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

            this.filesToProcess = new Dictionary<CustomDataSourceReference, List<List<string>>>();

            this.freeFilesToProcess = new List<string>();
            this.freeFilesToProcessRO = new ReadOnlyCollection<string>(this.freeFilesToProcess);

            this.enabledCookers = new List<DataCookerPath>();
            this.enabledCookersRO = new ReadOnlyCollection<DataCookerPath>(this.enabledCookers);
        }

        /// <summary>
        ///     Gets the directory that the runtime will scan for extensions.
        ///     This directory is scanned during <see cref="Create"/> when
        ///     creating an instance of the runtime. See <see cref="SetExtensionDirectory(string)"/>
        ///     and <see cref="ResetExtensionDirectory"/> for manipulating this property.
        /// </summary>
        public string ExtensionDirectory { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance has completed processing.
        /// </summary>
        public bool IsProcessed { get; private set; }

        /// <summary>
        ///     Gets the collection of source parsers that the runtime will use to 
        ///     cook data.
        /// </summary>
        public IEnumerable<ICustomDataSource> CustomDataSources => this.customDataSourceReferencesRO.Select(x => x.Instance);

        /// <summary>
        ///     Gets the collection of paths to all source cookers that the runtime
        ///     has discovered.
        /// </summary>
        public IEnumerable<DataCookerPath> SourceDataCookers => this.sourceDataCookersRO;

        /// <summary>
        ///     Gets the collection of paths to all composite cookers that the runtime
        ///     has discovered.
        /// </summary>
        public IEnumerable<DataCookerPath> CompositeDataCookers => this.compositeDataCookersRO;

        /// <summary>
        ///     Gets the collection of all cookers that the runtime has discovered. This is the union
        ///     of <see cref="this.SourceDataCookers"/> and <see cref="this.CompositeDataCookers"/>.
        /// </summary>
        public IEnumerable<DataCookerPath> AllCookers => this.SourceDataCookers.Concat(this.CompositeDataCookers);

        /// <summary>
        ///     Gets the collection of files to process. These files will be processed in whatever
        ///     <see cref="ICustomDataProcessorWithSourceParser"/> are able to handle the file.
        /// </summary>
        public IEnumerable<string> FreeFilesToProcess => this.freeFilesToProcessRO;

        /// <summary>
        ///     Gets the collection of files that are to be processed by a specific data source.
        /// </summary>
        public IReadOnlyDictionary<ICustomDataSource, IReadOnlyList<IReadOnlyList<string>>> FilesToProcess =>
            this.filesToProcess.ToDictionary(
                x => x.Key.Instance,
                x => (IReadOnlyList<IReadOnlyList<string>>)x.Value.Select(v => (IReadOnlyList<string>)v.AsReadOnly()).ToList().AsReadOnly());

        /// <summary>
        ///     Gets the collection of all cookers
        /// </summary>
        public IEnumerable<DataCookerPath> EnabledCookers => this.enabledCookersRO;

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <returns>
        ///     The created runtime environment.
        /// </returns>
        /// <exception cref="EngineException">
        ///     An error occurred while creating the runtime.
        /// </exception>
        public static Engine Create()
        {
            return Create(new EngineCreateInfo());
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="createInfo">
        ///     The parameters to use for creating a new runtime instance.
        /// </param>
        /// <returns>
        ///     The created runtime environment.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="createInfo"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="EngineException">
        ///     An error occurred while creating the runtime.
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
                    "Unable to create the runtime. See the inner exception for details.",
                    e);
            }
        }

        /// <summary>
        ///     Adds the given file to this instance for processing.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filePath"/> is whitespace.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="UnsupportedFileException">
        ///     <paramref name="filePath"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        public void AddFile(string filePath)
        {
            Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));

            this.ThrowIfProcessed();

            if (!this.TryAddFile(filePath))
            {
                throw new UnsupportedFileException(filePath);
            }
        }

        /// <summary>
        ///     Attempts to add the given file to this instance for processing.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the file has been added for processing;
        ///     <c>false</c> if the file is not valid, cannot be processed,
        ///     or the instance has already been processed. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        public bool TryAddFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            if (this.IsProcessed)
            {
                return false;
            }

            if (!this.DoesAnyDataSourceSupport(filePath))
            {
                return false;
            }

            try
            {
                this.freeFilesToProcess.Add(Path.GetFullPath(filePath));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Adds the given file to this instance for processing by
        ///     the specific source.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <param name="dataSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filePath"/> is whitespace.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="UnsupportedFileException">
        ///     The specified <paramref name="dataSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="dataSourceType"/> is unknown.
        /// </exception>
        public void AddFile(string filePath, Type dataSourceType)
        {
            Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Guard.NotNull(dataSourceType, nameof(dataSourceType));

            this.ThrowIfProcessed();

            foreach (var s in this.customDataSourceReferences)
            {
                if (TypeIs(s.Instance.GetType(), dataSourceType))
                {
                    if (DoesDataSourceSupport(filePath, s))
                    {
                        if (!this.filesToProcess.TryGetValue(s, out var list))
                        {
                            list = new List<List<string>>();
                            this.filesToProcess[s] = list;
                        }

                        list.Add(new List<string> { filePath, });
                        return;
                    }

                    throw new UnsupportedFileException(filePath, dataSourceType);
                }
            }

            throw new UnsupportedDataSourceException(dataSourceType);
        }

        /// <summary>
        ///     Attempts to add the given file to this instance for processing by
        ///     the specific source.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the file to process.
        /// </param>
        /// <param name="dataSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the file has been added for processing by this data source;
        ///     <c>false</c> otherwise. Note that <c>false</c>
        ///     is always returned when <see cref="IsProcessed"/> is <c>true</c>.
        /// </returns>
        public bool TryAddFile(string filePath, Type dataSourceType)
        {
            try
            {
                this.AddFile(filePath, dataSourceType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Adds the given files to this instance for processing by
        ///     the specific source. All of the files will be processed
        ///     by the same instance of data processor. Use <see cref="AddFile(string, Type)"/>
        ///     to ensure each file is processed by a different instance, or
        ///     use multiple calls to <see cref="AddFiles(IEnumerable{string}, Type)"/>.
        /// </summary>
        /// <param name="filePaths">
        ///     The path to the files to process.
        /// </param>
        /// <param name="dataSourceType">
        ///     The data source to use to process the file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="filePath"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filePath"/> is whitespace.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="UnsupportedFileException">
        ///     The specified <paramref name="dataSourceType"/> cannot handle
        ///     the given file.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="dataSourceType"/> is unknown.
        /// </exception>
        public void AddFiles(IEnumerable<string> filePaths, Type dataSourceType)
        {
            Guard.NotNull(filePaths, nameof(filePaths));
            Guard.NotNull(dataSourceType, nameof(dataSourceType));
            this.ThrowIfProcessed();

            foreach (var s in this.customDataSourceReferences)
            {
                if (TypeIs(s.Instance.GetType(), dataSourceType))
                {
                    var atLeastOnefileSupported = false;
                    string firstUnsupportedFile = null;
                    foreach (var file in filePaths)
                    {
                        Guard.NotNullOrWhiteSpace(file, nameof(filePaths));

                        if (!DoesDataSourceSupport(file, s))
                        {
                            if (firstUnsupportedFile is null)
                            {
                                firstUnsupportedFile = file;
                            }
                            else if (atLeastOnefileSupported)
                            {
                                throw new UnsupportedFileException(file, dataSourceType);
                            }
                        }
                        else
                        {
                            if (firstUnsupportedFile != null)
                            {
                                throw new UnsupportedFileException(file, dataSourceType);
                            }

                            atLeastOnefileSupported = true;
                        }
                    }

                    if (!atLeastOnefileSupported &&
                        firstUnsupportedFile is null)
                    {
                        //
                        // the file collection was empty
                        //

                        throw new ArgumentException("The file path collection cannot be empty", nameof(filePaths));
                    }

                    if (firstUnsupportedFile != null)
                    {
                        throw new UnsupportedFileException(firstUnsupportedFile, dataSourceType);
                    }

                    if (atLeastOnefileSupported)
                    {
                        Debug.Assert(firstUnsupportedFile is null);
                        if (!this.filesToProcess.TryGetValue(s, out var list))
                        {
                            list = new List<List<string>>();
                            this.filesToProcess[s] = list;
                        }

                        list.Add(filePaths.ToList());
                        return;
                    }
                }
            }

            throw new UnsupportedDataSourceException(dataSourceType);
        }

        /// <summary>
        ///     Attempts to add the given files to this instance. All of
        ///     the files will be processed by the same instance of 
        ///     data processor. Use <see cref="AddFile"/> to ensure
        ///     each file is processed by a different instance.
        /// </summary>
        /// <param name="filePaths">
        ///     The paths to the files to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the files are added;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryAddFiles(IEnumerable<string> filePaths, Type dataSourceType)
        {
            try
            {
                this.AddFiles(filePaths, dataSourceType);
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

            var assemblyDiscovery = new AssemblyExtensionDiscovery(assemblyLoader);

            var catalog = new ReflectionPlugInCatalog(assemblyDiscovery);

            var factory = new DataExtensionFactory();
            var repoBuilder = factory.CreateDataExtensionRepository();
            var reflector = new DataExtensionReflector(
                assemblyDiscovery,
                repoBuilder);

            assemblyDiscovery.ProcessAssemblies(extensionDirectory);

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

        private RuntimeExecutionResults ProcessCore()
        {
            this.IsProcessed = true;

            var allFileAssociations = GroupAllFilesToDataSources(
                this.CustomDataSources,
                this.FreeFilesToProcess,
                this.FilesToProcess);

            var executors = CreateExecutors(allFileAssociations);
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
            Dictionary<ICustomDataSource, List<List<string>>> allFileAssociations)
        {
            var executors = new List<CustomDataSourceExecutor>();
            foreach (var kvp in allFileAssociations)
            {
                try
                {
                    var cds = kvp.Key;
                    var fileLists = kvp.Value;

                    foreach (var files in fileLists)
                    {
                        var executionContext = new SDK.Runtime.ExecutionContext(
                            new DataProcessorProgress(),
                            x => ConsoleLogger.Create(x.GetType()),
                            cds,
                            files.Select(x => new FileDataSource(x)),
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

        private static Dictionary<ICustomDataSource, List<List<string>>> GroupAllFilesToDataSources(
            IEnumerable<ICustomDataSource> customDataSources,
            IEnumerable<string> freeFilesToProcess,
            IReadOnlyDictionary<ICustomDataSource, IReadOnlyList<IReadOnlyList<string>>> filesToProcess)
        {
            var allFileAssociations = new Dictionary<ICustomDataSource, List<List<string>>>();

            var freeFileAssociations = AllocateFiles(customDataSources, freeFilesToProcess);

            foreach (var kvp in filesToProcess)
            {
                var cds = kvp.Key;
                var fileLists = kvp.Value;

                if (!allFileAssociations.TryGetValue(cds, out List<List<string>> existingFileLists))
                {
                    existingFileLists = new List<List<string>>();
                    allFileAssociations[cds] = existingFileLists;
                }

                foreach (var list in fileLists)
                {
                    existingFileLists.Add(list.ToList());
                }
            }

            foreach (var kvp in freeFileAssociations)
            {
                if (!allFileAssociations.TryGetValue(kvp.Key, out List<List<string>> existingFileLists))
                {
                    existingFileLists = new List<List<string>>();
                    allFileAssociations[kvp.Key] = existingFileLists;
                }

                foreach (var file in kvp.Value)
                {
                    existingFileLists.Add(new List<string> { file, });
                }
            }

            foreach (var kvp in allFileAssociations.ToArray())
            {
                if (kvp.Value.Count == 0)
                {
                    allFileAssociations.Remove(kvp.Key);
                }
            }

            return allFileAssociations;
        }

        private static Dictionary<ICustomDataSource, List<string>> AllocateFiles(
            IEnumerable<ICustomDataSource> allDataSources,
            IEnumerable<string> files)
        {
            var cdsToFiles = new Dictionary<ICustomDataSource, List<string>>();

            foreach (var cds in allDataSources)
            {
                cdsToFiles[cds] = files.Where(cds.Supports).ToList();
            }

            return cdsToFiles;
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

        private bool DoesAnyDataSourceSupport(string file)
        {
            foreach (var s in this.customDataSourceReferences)
            {
                if (DoesDataSourceSupport(file, s))
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoesDataSourceSupport(string file, CustomDataSourceReference cds)
        {
            try
            {
                if (cds.Instance.IsFileSupported(file))
                {
                    return true;
                }
            }
            catch
            {
                // obviously can't support the file if it can't
                // make a decision
            }

            return false;
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
    }
}

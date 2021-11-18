// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

// TODO::support concurrency in the Engine

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Provides the entry point for programmatically manipulating, cooking,
    ///     and accessing data.
    /// </summary>
    /// <remarks>
    ///     This class is not thread safe.
    /// </remarks>
    public abstract class Engine
        : IDisposable
    {
        private readonly Dictionary<Guid, TableDescriptor> tableGuidToDescriptor;
        private readonly List<TableDescriptor> allTables;
        private readonly ReadOnlyCollection<TableDescriptor> allTablesRO;

        private readonly List<DataCookerPath> enabledCookers;
        private readonly ReadOnlyCollection<DataCookerPath> enabledCookersRO;

        private readonly Func<Type, ILogger> loggerFactory;

        private readonly EngineCreateInfo createInfo;

        private ILogger logger;
        private List<ProcessingSourceExecutor> executors;
        private ReadOnlyDataSourceSet workingDataSourceSet;
        private DataSourceSet internalDataSourceSet;

        private TableExtensionSelector tableExtensionSelector;

        private bool isProcessed;

        private IApplicationEnvironment applicationEnvironment;

        private RuntimeExecutionResults runtimeExecutionResult;

        private ProcessingSystemCompositeCookers compositeCookers;

        private Dictionary<TableDescriptor, ICustomDataProcessor> tablesToProcessors;

        private bool isDisposed;

        internal Engine(EngineCreateInfo createInfo, DataSourceSet internalDataSourceSet)
        {
            //
            // The internalDataSourceSet is used by the helper methods
            // that take DataSources and return a new engine. For those
            // methods, we create a new DataSourceSet under the hood to
            // reference those data sources, and so need to keep track
            // of that set in order to dispose it when the engine is
            // disposed. This is because the user is unaware that a new
            // DataSourceSet was created and thus there is no way that
            // they could be responsible for freeing it.
            //

            this.workingDataSourceSet = createInfo.DataSources;
            this.internalDataSourceSet = internalDataSourceSet;

            this.loggerFactory = createInfo.LoggerFactory;

            this.tableGuidToDescriptor = new Dictionary<Guid, TableDescriptor>();
            this.allTables = new List<TableDescriptor>();
            this.allTablesRO = new ReadOnlyCollection<TableDescriptor>(this.allTables);

            this.enabledCookers = new List<DataCookerPath>();
            this.enabledCookersRO = new ReadOnlyCollection<DataCookerPath>(this.enabledCookers);

            this.createInfo = createInfo;

            // This contains the set of composite cookers that will be used by the system of data processors available
            // to this RuntimeExecutionResults.
            this.compositeCookers = new ProcessingSystemCompositeCookers(this.Extensions);

            // Scenario: A processing source supports multiple data processors, and a TableDescriptor is valid for
            // multiple data processors. To support this, we'll need to update this data structure, the way we enable
            // tables, as well as the way we query for enabled tables.
            // TODO: can we add telemetry for this scenario so we know how often this happens so we can prioritize it?
            // TODO: support the above scenario
            this.tablesToProcessors = new Dictionary<TableDescriptor, ICustomDataProcessor>();

            this.isDisposed = false;
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
        ///     Gets the collection of all data sources to process in this
        ///     engine instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public ReadOnlyDataSourceSet DataSourcesToProcess
        {
            get
            {
                this.ThrowIfDisposed();
                return this.workingDataSourceSet;
            }
        }

        /// <summary>
        ///     Gets the collection of all plugins loaded into this engine instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public PluginSet Plugins => this.DataSourcesToProcess.Plugins;

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
                return this.tablesToProcessors.Keys.AsEnumerable();
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
        ///     Gets the parameters used to create this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public EngineCreateInfo CreateInfo
        {
            get
            {
                this.ThrowIfDisposed();
                return this.createInfo;
            }
        }

        private IEnumerable<ProcessingSourceReference> ProcessingSourceReferences => this.Plugins.ProcessingSourceReferences;

        private bool ArePluginsIsolated => this.Plugins.ArePluginsIsolated;

        private ExtensionRoot Extensions => this.Plugins.Extensions;

        private DataExtensionFactory Factory => this.Plugins.Factory;

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="EngineException">
        ///     A fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     <paramref name="dataSource"/> cannot be processed by any
        ///     discovered extensions.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create(IDataSource dataSource)
        {
            Guard.NotNull(dataSource, nameof(dataSource));

            DataSourceSet dataSourceSet = null;
            try
            {
                dataSourceSet = DataSourceSet.Create();
                dataSourceSet.AddDataSource(dataSource);

                var info = new EngineCreateInfo(dataSourceSet.AsReadOnly());
                return Create(info);
            }
            catch
            {
                dataSourceSet.SafeDispose();
                throw;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="dataSource">
        ///     The Data Source to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process <paramref name="dataSource"/>.
        /// </param>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSource"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="EngineException">
        ///     A fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create(
            IDataSource dataSource,
            Type processingSourceType)
        {
            Guard.NotNull(dataSource, nameof(dataSource));
            Guard.NotNull(processingSourceType, nameof(processingSourceType));

            DataSourceSet dataSourceSet = null;
            try
            {
                dataSourceSet = DataSourceSet.Create();
                dataSourceSet.AddDataSource(dataSource, processingSourceType);

                var info = new EngineCreateInfo(dataSourceSet.AsReadOnly());
                return CreateCore(info, dataSourceSet);
            }
            catch
            {
                dataSourceSet.SafeDispose();
                throw;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="dataSources">
        ///     The Data Sources to process.
        /// </param>
        /// <param name="processingSourceType">
        ///     The <see cref="IProcessingSource"/> to use to process <paramref name="dataSources"/>.
        /// </param>
        /// <returns>
        ///     The created engine environment.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="dataSources"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="processingSourceType"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="UnsupportedDataSourceException">
        ///     The specified <paramref name="processingSourceType"/> cannot handle
        ///     the given Data Source.
        /// </exception>
        /// <exception cref="UnsupportedProcessingSourceException">
        ///     The specified <paramref name="processingSourceType"/> is unknown.
        /// </exception>
        /// <exception cref="EngineException">
        ///     A fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create(
            IEnumerable<IDataSource> dataSources,
            Type processingSourceType)
        {
            Guard.NotNull(dataSources, nameof(dataSources));
            Guard.NotNull(processingSourceType, nameof(processingSourceType));

            DataSourceSet dataSourceSet = null;
            try
            {
                dataSourceSet = DataSourceSet.Create();
                dataSourceSet.AddDataSources(dataSources, processingSourceType);

                var info = new EngineCreateInfo(dataSourceSet.AsReadOnly());
                return CreateCore(info, dataSourceSet);
            }
            catch
            {
                dataSourceSet.SafeDispose();
                throw;
            }
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="Engine" /> class.
        /// </summary>
        /// <param name="createInfo">
        ///     Provides details on how to create the engine as well as what 
        ///     the engine should process.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="createInfo"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="EngineException">
        ///     A fatal error occurred while creating the engine.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public static Engine Create(EngineCreateInfo createInfo)
        {
            Guard.NotNull(createInfo, nameof(createInfo));

            try
            {
                return CreateCore(createInfo, null);
            }
            catch (Exception e)
            {
                throw new EngineException(
                    "Unable to create the engine. See the inner exception for details.",
                    e);
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
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="NoDataSourceException">
        ///     There are inadequate data sources in <see cref="DataSourcesToProcess"/>
        ///     in order for the specified cooker to participate in processing.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public void EnableCooker(DataCookerPath dataCookerPath)
        {
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();

            if (!this.Plugins.AllCookers.Contains(dataCookerPath))
            {
                throw new CookerNotFoundException(dataCookerPath);
            }

            if (!this.CouldCookerHaveData(dataCookerPath))
            {
                throw new NoDataSourceException(dataCookerPath);
            }

            this.Extensions.EnableDataCookers(
                GetExtensibleProcessors(),
                new HashSet<DataCookerPath>() { dataCookerPath });

            this.enabledCookers.Add(dataCookerPath);
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

            if (!this.Plugins.AllCookers.Contains(dataCookerPath))
            {
                return false;
            }

            if (!this.CouldCookerHaveData(dataCookerPath))
            {
                return false;
            }

            try
            {
                EnableCooker(dataCookerPath);

                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        ///     Enables the given table for processing when <see cref="Process"/>
        ///     is called.
        /// </summary>
        /// <param name="descriptor">
        ///     The table to enable.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="descriptor"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The processing sources referenced by the table could
        ///     not be found.
        /// </exception>
        /// <exception cref="InstanceAlreadyProcessedException">
        ///     This instance has already been processed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        /// <exception cref="TableException">
        ///     A table is not available for the given <paramref name="descriptor"/>.
        /// </exception>
        /// <exception cref="TableNotFoundException">
        ///     A table cannot be found for the given <paramref name="descriptor"/>.
        /// </exception>
        public void EnableTable(TableDescriptor descriptor)
        {
            this.ThrowIfDisposed();
            this.ThrowIfProcessed();
            Guard.NotNull(descriptor, nameof(descriptor));

            if (this.tablesToProcessors.ContainsKey(descriptor))
            {
                return;
            }

            if (!this.allTables.Contains(descriptor))
            {
                throw new TableNotFoundException(descriptor);
            }

            ITableExtensionReference tableReference = null;
            IEnumerable<ProcessingSourceExecutor> executorsWithTable = null;

            executorsWithTable = this.executors.Where(
                x => x.Context.ProcessingSource.AvailableTables.Contains(descriptor));

            if (!executorsWithTable.Any())
            {
                if (this.Extensions.TablesById.TryGetValue(descriptor.Guid, out tableReference))
                {
                    Debug.Assert(
                        tableReference != null,
                        "If ExtensionRoot.TablesById contains the table descriptor, then it should also contain a " +
                        "non-null table reference.");
                    Debug.Assert(tableReference.BuildTableAction != null);

                    if (tableReference.Availability != DataExtensionAvailability.Available)
                    {
                        throw new TableException($"The requested table is not available: {tableReference.Availability}.");
                    }
                }
                else
                {
                    throw new ArgumentException(
                        "No data source found for the given table.",
                        paramName: nameof(descriptor));
                }
            }

            EnableTableCore(descriptor, tableReference, executorsWithTable);
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

            if (this.tablesToProcessors.ContainsKey(descriptor))
            {
                return true;
            }

            try
            {
                EnableTable(descriptor);
            }
            catch (Exception)
            {
                return false;
            }

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

        /// <summary>
        ///     Returns custom data processors that derive from <see cref="ICustomDataProcessorWithSourceParser"/>.
        /// </summary>
        /// <returns>
        ///     Custom data processors created by the engine that derive from
        ///     <see cref="ICustomDataProcessorWithSourceParser"/>.
        /// </returns>
        public IEnumerable<ICustomDataProcessorWithSourceParser> GetExtensibleProcessors()
        {
            Debug.Assert(this.executors != null);
            return this.executors.Select(e => e.Processor).OfType<ICustomDataProcessorWithSourceParser>();
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
                //
                // we diposes the original set only if we own the data sources.
                // this only occurs when the user uses one of the convenience
                // methods that creates a DataSource internally, e.g. Create(IDataSource).
                //

                this.internalDataSourceSet.SafeDispose();

                this.compositeCookers.SafeDispose();

                this.runtimeExecutionResult.SafeDispose();
            }

            this.applicationEnvironment = null;
            this.workingDataSourceSet = null;
            this.internalDataSourceSet = null;
            this.applicationEnvironment = null;
            this.tablesToProcessors = null;
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

        private static Engine CreateCore(EngineCreateInfo createInfo, DataSourceSet internalDataSourceSet)
        {
            Debug.Assert(createInfo != null);

            var loggerFactory = createInfo.LoggerFactory ?? Logger.Create;

            var logger = loggerFactory(typeof(Engine));

            Engine instance = null;
            try
            {

                instance = new EngineImpl(createInfo, internalDataSourceSet);
                instance.logger = logger;
                instance.tableExtensionSelector = new TableExtensionSelector(createInfo.DataSources.Plugins.Extensions.Repository);

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
                    instance.Extensions,
                    instance.Factory.CreateSourceSessionFactory(),
                    createInfo.IsInteractive
                        ? (IMessageBox)new InteractiveRuntimeMessageBox(instance.logger)
                        : (IMessageBox)new NonInteractiveMessageBox(instance.logger));

                foreach (var cds in instance.ProcessingSourceReferences)
                {
                    try
                    {
                        cds.Instance.SetApplicationEnvironment(instance.applicationEnvironment);
                        cds.Instance.SetLogger(instance.CreateLogger(cds.Instance.GetType()));
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Failed to initialize processing source '{cds.Name}': {e}");
                    }
                }

                var allTables = new HashSet<TableDescriptor>();
                foreach (var tableId in createInfo.DataSources.Plugins.TablesById)
                {
                    allTables.Add(tableId.Value.TableDescriptor);
                    instance.tableGuidToDescriptor[tableId.Key] = tableId.Value.TableDescriptor;
                }

                foreach (var descriptor in createInfo.DataSources.Plugins.ProcessingSourceReferences.SelectMany(x => x.AvailableTables))
                {
                    allTables.Add(descriptor);
                    instance.tableGuidToDescriptor[descriptor.Guid] = descriptor;
                }

                instance.allTables.AddRange(allTables);

                var allDataSourceAssociations = GroupAllDataSourcesToProcessingSources(
                    instance.Plugins.ProcessingSourceReferences,
                    instance.DataSourcesToProcess.FreeDataSourcesToProcess,
                    instance.DataSourcesToProcess.DataSourcesToProcess);

                instance.executors = instance.CreateExecutors(allDataSourceAssociations);

                instance.IsProcessed = false;

                return instance;
            }
            catch (Exception)
            {
                instance.SafeDispose();
                throw;
            }
        }

        private bool CouldCookerHaveData(DataCookerPath dataCookerPath)
        {
            if (dataCookerPath.DataCookerType == DataCookerType.CompositeDataCooker)
            {
                //
                // Composite cookers are always enabled, so it doesn't make sense to
                // determine whether they might have data. Whether they might have data
                // is determined by which cookers they depend on having data. Ultimately,
                // if the source cookers that ultimately feed into the composite cooker
                // are enabled, then the composite could have data. Thus, this method
                // will only check if it is feasible that a source cooker could have data.
                // This is sufficient because if the source cooker cannot have have data,
                // then by definition any composite cookers that depend on it could not
                // have data, and when the source cooker is enabled, this method would
                // return false, thus there is no situation where a composite cooker
                // could have data but one of the required source cookers did not.
                //

                return true;
            }

            var found = this.Plugins.Extensions.TryGetDataCookerReference(dataCookerPath, out var cooker);
            Debug.Assert(found);
            Debug.Assert(cooker != null);

            var sourceParsers = this.executors.Select(x => (x, x.Processor))
                .Where(x => x.Processor is ICustomDataProcessorWithSourceParser)
                .Select(x => (x.x, (ICustomDataProcessorWithSourceParser)x.Processor))
                .Where(x => x.Item2.SourceParserId == dataCookerPath.SourceParserId)
                .ToList();

            return sourceParsers.Count > 0;
        }

        private void EnableTableCore(
            TableDescriptor descriptor,
            ITableExtensionReference tableReference,
            IEnumerable<ProcessingSourceExecutor> executorsWithTable)
        {
            Debug.Assert(!this.IsProcessed);
            Debug.Assert(!this.isDisposed);

            Debug.Assert(descriptor != null);

            if (this.tablesToProcessors.ContainsKey(descriptor))
            {
                // this table has already been enabled
                return;
            }

            if (tableReference != null)
            {
                // extended tables generally aren't tied to a processor
                this.tablesToProcessors.Add(descriptor, null);

                this.Extensions.EnableSourceDataCookersForTables(
                    GetExtensibleProcessors(),
                    new HashSet<ITableExtensionReference>() { tableReference });
            }
            else
            {
                foreach (var executor in executorsWithTable)
                {
                    if (this.tablesToProcessors.TryGetValue(descriptor, out var processor))
                    {
                        if (processor != executor.Processor)
                        {
                            var message = string.Format(
                                "Unable to map table {0} to processor {1}: " +
                                "The SDK.Engine does not support tables used by multiple data processors.",
                                descriptor.Guid,
                                executor.Context.ProcessingSource);

                            this.logger.Warn("{0}", message);

                            continue;
                        }

                        // this table was already added for this processor
                        continue;
                    }

                    try
                    {
                        // TODO: Because a source processor can support multiple custom data processors, would it be better
                        // to add a SupportsTable method to each processor? This code is awkward. If there was a
                        // SupportsTable method, we could throw an exception for support by multiple processors before
                        // enabling the table on any processor.
                        //

                        executor.Processor.EnableTable(descriptor);
                        this.tablesToProcessors.Add(descriptor, executor.Processor);
                    }
                    catch (Exception e)
                    {
                        var message = string.Format("Unable to enable table {0} on processor {1}: {2}",
                            descriptor.Guid,
                            executor.Context.ProcessingSource,
                            e);

                        this.logger.Warn("{0}", message);

                        throw new TableException(message, e);
                    }
                }
            }

            Debug.Assert(this.tablesToProcessors.ContainsKey(descriptor));
        }

        private RuntimeExecutionResults ProcessCore()
        {
            this.IsProcessed = true;

            var executionResults = new List<ExecutionResult>(this.executors.Count);
            var errors = new List<ProcessingError>(this.executors.Count);
            foreach (var executor in this.executors)
            {
                try
                {
                    var executionResult = executor.ExecuteAsync(CancellationToken.None)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();
                    executionResults.Add(executionResult);
                    if (executionResult.ProcessorFault != null)
                    {
                        errors.Add(new ProcessingError(executionResult));
                    }
                }
                catch (Exception e)
                {
                    //
                    // todo: better error message
                    //

                    errors.Add(
                        new ProcessingError(
                            executor.Context.ProcessingSource.Guid,
                            executor.Processor,
                            executor.Context.DataSources,
                            e));

                    this.logger.Error($"Failed to process: {e}");
                }
            }

            var sourceCookerDataRetrieval = this.Factory.CreateCrossParserSourceDataCookerRetrieval(
                GetExtensibleProcessors());
            var dataRetrievalFactory = new DataExtensionRetrievalFactory(
                sourceCookerDataRetrieval,
                this.compositeCookers,
                this.Extensions);

            this.compositeCookers.Initialize(dataRetrievalFactory);

            this.runtimeExecutionResult = new RuntimeExecutionResults(
                sourceCookerDataRetrieval,
                this.compositeCookers,
                dataRetrievalFactory,
                this.Extensions,
                this.tablesToProcessors,
                errors);

            return this.runtimeExecutionResult;
        }

        private List<ProcessingSourceExecutor> CreateExecutors(
            Dictionary<ProcessingSourceReference, List<List<IDataSource>>> allDataSourceAssociations)
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
                            processingSource.Instance.MetadataTables,
                            new RuntimeProcessorEnvironment(this.Extensions, this.compositeCookers, this.CreateLogger),
                            ProcessorOptions.Default);

                        var executor = new ProcessingSourceExecutor();
                        executor.InitializeCustomDataProcessor(executionContext);

                        executors.Add(executor);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(
                           "Error processing dataSources {0} on processing source {1}: {2}",
                           dataSources.ToString(),
                           processingSource.Name,
                           e);
                    }
                }
            }

            return executors;
        }

        private static Dictionary<ProcessingSourceReference, List<List<IDataSource>>> GroupAllDataSourcesToProcessingSources(
            IEnumerable<ProcessingSourceReference> processingSources,
            IEnumerable<IDataSource> freeDataSourcesToProcess,
            IReadOnlyDictionary<ProcessingSourceReference, IReadOnlyList<IReadOnlyList<IDataSource>>> dataSourcesToProcess)
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

            if (this.ArePluginsIsolated)
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
            internal EngineImpl(EngineCreateInfo createInfo, DataSourceSet internalDataSourceSet)
                : base(createInfo, internalDataSourceSet)
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

        private abstract class RuntimeMessageBox
            : IMessageBox
        {
            internal RuntimeMessageBox(
                ILogger logger)
            {
                this.Logger = logger;
            }

            protected ILogger Logger { get; }

            public void Show(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                string format,
                params object[] args)
            {
                var message = FormatMessage(icon, formatProvider, format, args);
                this.Logger.Error(message);
            }

            public abstract ButtonResult Show(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                Buttons buttons,
                string caption,
                string format,
                params object[] args);

            protected static string FormatMessage(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                string format,
                params object[] args)
            {
                var message = string.Format(formatProvider, format, args);
                return string.Concat("[", icon.ToString("G"), "]: ", message);
            }
        }

        private sealed class NonInteractiveMessageBox
            : RuntimeMessageBox
        {
            internal NonInteractiveMessageBox(ILogger logger)
                : base(logger)
            {
            }

            public override ButtonResult Show(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                Buttons buttons,
                string caption,
                string format,
                params object[] args)
            {
                throw new InvalidOperationException("This engine was created with IsInteractive set to false. User interaction with plugins is no allowed in this mode.");
            }
        }

        private sealed class InteractiveRuntimeMessageBox
           : RuntimeMessageBox
        {
            internal InteractiveRuntimeMessageBox(ILogger logger)
                : base(logger)
            {
            }

            public override ButtonResult Show(
                MessageBoxIcon icon,
                IFormatProvider formatProvider,
                Buttons buttons,
                string caption,
                string format,
                params object[] args)
            {
                var message = FormatMessage(icon, formatProvider, format, args);
                switch (icon)
                {
                    case MessageBoxIcon.Error:
                        this.Logger.Error("{0}", message);
                        break;

                    case MessageBoxIcon.Information:
                        this.Logger.Info("{0}", message);
                        break;

                    case MessageBoxIcon.Warning:
                        this.Logger.Warn("{0}", message);
                        break;

                    default:
                        this.Logger.Info(message);
                        break;
                }

                return GetUserInput(buttons);
            }

            private static ButtonResult GetUserInput(Buttons buttons)
            {
                switch (buttons)
                {
                    case Buttons.OK:
                        {
                            Console.Out.WriteLine("Press [Enter] to Continue.");
                            Console.In.ReadLine();
                            return ButtonResult.OK;
                        }

                    case Buttons.OKCancel:
                        {
                            var c = GetCharFromUser(
                                "O(kay) / C(ancel) ? ",
                                'O', 'o', 'C', 'c');
                            switch (c)
                            {
                                case 'O':
                                case 'o':
                                    return ButtonResult.OK;

                                case 'C':
                                case 'c':
                                    return ButtonResult.Cancel;

                                default:
                                    Debug.Assert(false);
                                    throw new InvalidOperationException();
                            }
                        }

                    case Buttons.YesNo:
                        {
                            var c = GetCharFromUser(
                                "Y(es) / N(o) ? ",
                                'Y', 'y', 'N', 'n');
                            switch (c)
                            {
                                case 'Y':
                                case 'y':
                                    return ButtonResult.Yes;

                                case 'N':
                                case 'n':
                                    return ButtonResult.No;

                                default:
                                    Debug.Assert(false);
                                    throw new InvalidOperationException();
                            }
                        }

                    case Buttons.YesNoCancel:
                        {
                            var c = GetCharFromUser(
                               "Y(es) / N(o) / C(ancel) ? ",
                               'Y', 'y', 'N', 'n', 'C', 'c');
                            switch (c)
                            {
                                case 'Y':
                                case 'y':
                                    return ButtonResult.Yes;

                                case 'N':
                                case 'n':
                                    return ButtonResult.No;

                                case 'C':
                                case 'c':
                                    return ButtonResult.Cancel;

                                default:
                                    Debug.Assert(false);
                                    throw new InvalidOperationException();
                            }
                        }

                    default:
                        {
                            throw new InvalidEnumArgumentException(
                                nameof(buttons),
                                (int)buttons,
                                typeof(Buttons));
                        }
                }
            }

            private static char GetCharFromUser(
                string prompt,
                params char[] valid)
            {

                char c = '\0';
                do
                {
                    Console.Out.Write(prompt);

                    var line = Console.In.ReadLine();
                    if (line.Length > 0)
                    {
                        c = line[0];
                    }
                } while (!valid.Contains(c));

                return c;
            }
        }
    }
}

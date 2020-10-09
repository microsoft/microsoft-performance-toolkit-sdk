// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// This is a helper class that handles some of the logic required for a custom data processor that
    /// implements <see cref="ICustomDataProcessorWithSourceParser{T,TContext,TKey}"/>.
    /// </summary>
    /// <typeparam name="T">Type of data from the source to be processed</typeparam>
    /// <typeparam name="TContext">Type that contains context about the data from the source</typeparam>
    /// <typeparam name="TKey">Type that will be used to identify data from the source that is relevant to this extension</typeparam>
    public abstract class CustomDataProcessorBaseWithSourceParser<T, TContext, TKey>
        : CustomDataProcessorBase,
          ICustomDataProcessorWithSourceParser<T, TContext, TKey> 
          where T : IKeyedDataType<TKey>
    {
        private readonly IDataProcessorExtensibilitySupport extensibilitySupport;

        /// <summary>
        /// This constructor will setup the data processor so that it can use the data extension framework - allowing
        /// table and data cookers both internally and external to this plug-in.
        /// </summary>
        /// <param name="sourceParser">The source data parser</param>
        /// <param name="options">Processor options</param>
        /// <param name="applicationEnvironment">Application environment</param>
        /// <param name="processorEnvironment">Processor environment</param>
        /// <param name="allTablesMapping">Maps table descriptors to possible build actions</param>
        /// <param name="metadataTables">Metadata tables</param>
        protected CustomDataProcessorBaseWithSourceParser(
            ISourceParser<T, TContext, TKey> sourceParser,
            ProcessorOptions options,
            IApplicationEnvironment applicationEnvironment,
            IProcessorEnvironment processorEnvironment,
            IReadOnlyDictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>> allTablesMapping,
            IEnumerable<TableDescriptor> metadataTables) 
            : base(options, applicationEnvironment, processorEnvironment, allTablesMapping, metadataTables)
        {
            Guard.NotNull(sourceParser, nameof(sourceParser));

            this.SourceParser = sourceParser;
            this.SourceProcessingSession = this.ApplicationEnvironment.SourceSessionFactory.CreateSourceSession(this);
            this.extensibilitySupport = this.ProcessorEnvironment.CreateDataProcessorExtensibilitySupport(this);

            foreach (var metadataTable in metadataTables)
            {
                ProcessTableForExtensibility(metadataTable);
            }
        }

        /// <summary>
        /// This constructor is intended for use within the unified scenario - where a data processor might need to
        /// spawn additional data processors. Data from the unified data processor will be copied to the spawned
        /// data processor as appropriate.
        /// </summary>
        /// <param name="other">An existing custom data processor</param>
        protected CustomDataProcessorBaseWithSourceParser(CustomDataProcessorBaseWithSourceParser<T, TContext, TKey> other)
            : this(
                  other.SourceParser,
                  other.Options,
                  other.ApplicationEnvironment,
                  other.ProcessorEnvironment,
                  other.TableDescriptorToBuildAction,
                  other.EnabledTables)
        {
            foreach (var td in other.extensibilitySupport.GetAllRequiredTables())
            {
                EnableTable(td);
            }
        }

        /// <inheritdoc cref="ICustomDataProcessorWithSourceParser{T,TContext,TKey}"/>
        public ISourceParser<T, TContext, TKey> SourceParser { get; }

        /// <inheritdoc cref="ICustomDataProcessorWithSourceParser{T,TContext,TKey}"/>
        public ISourceProcessingSession<T, TContext, TKey> SourceProcessingSession { get; }

        /// <inheritdoc cref="ICustomDataProcessorWithSourceParser"/>
        public string SourceParserId => this.SourceParser.Id;

        /// <summary>
        /// Returns the DataSourceInfo from the source parser.
        /// </summary>
        /// <returns>Information regarding the processed source</returns>
        public override DataSourceInfo GetDataSourceInfo()
        {
            return this.SourceParser.DataSourceInfo;
        }

        /// <summary>
        /// Enables a source data cooker on the <see cref="SourceProcessingSession"/>.
        /// </summary>
        /// <param name="sourceDataCookerFactory">Source data cooker factory</param>
        public void EnableCooker(ISourceDataCookerFactory sourceDataCookerFactory)
        {
            Guard.NotNull(sourceDataCookerFactory, nameof(sourceDataCookerFactory));

            if (this.SourceProcessingSession == null)
            {
                return;
            }

            EnableCookerCore(sourceDataCookerFactory);

            if (sourceDataCookerFactory.CreateInstance() is ISourceDataCooker<T, TContext, TKey> cooker)
            {
                if (!StringComparer.Ordinal.Equals(cooker.Path.SourceParserId, this.SourceParserId))
                {
                    Debug.Assert(false, "Attempting to enable a source data cooker on the wrong source parser.");
                }
                else
                {
                    this.SourceProcessingSession.RegisterSourceDataCooker(cooker);
                    this.OnDataCookerEnabled(cooker);
                }
            }
            else
            {
                throw new ArgumentException("The given cooker reference is not applicable to this data processor.");
            }
        }

        /// <summary>
        /// Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </summary>
        /// <typeparam name="TOutput">Data output type</typeparam>
        /// <param name="dataOutputPath">Path to the data output</param>
        /// <returns>Data output of the specified type</returns>
        public TOutput QueryOutput<TOutput>(DataOutputPath dataOutputPath)
        {
            if (!StringComparer.Ordinal.Equals(dataOutputPath.SourceParserId, SourceParserId))
            {
                throw new ArgumentException(
                    $"The source Id in {nameof(dataOutputPath)} does not match this data processor.");
            }

            var dataCooker = this.SourceProcessingSession.GetSourceDataCooker(dataOutputPath.CookerPath);
            return dataCooker.QueryOutput<TOutput>(dataOutputPath);
        }

        /// <summary>
        /// Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </summary>
        /// <param name="dataOutputPath">Path to the data output</param>
        /// <returns>Data output as an <see cref="object"/></returns>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            if (!StringComparer.Ordinal.Equals(dataOutputPath.SourceParserId, SourceParserId))
            {
                throw new ArgumentException(
                    $"The source Id in {nameof(dataOutputPath)} does not match this data processor.");
            }

            var dataCooker = this.SourceProcessingSession.GetSourceDataCooker(dataOutputPath.CookerPath);
            return dataCooker.QueryOutput(dataOutputPath);
        }

        /// <inheritdoc cref="CustomDataProcessorBase"/>
        /// <summary>
        /// Adds to the base class functionality, validating that the <see cref="SourceParser"/> and
        /// <see cref="SourceProcessingSession"/> have been initialized, and then uses the <see cref="SourceProcessingSession"/>
        /// to process the source.
        /// </summary>
        protected override Task ProcessAsyncCore(
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(progress, nameof(progress));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            if (this.SourceParser == null)
            {
                Debug.Assert(false);
                throw new InvalidOperationException(
                    $"ProcessAsync may not be called on this class without a valid {nameof(SourceParser)}.");
            }

            if (this.SourceProcessingSession == null)
            {
                Debug.Assert(false);
                throw new InvalidOperationException(
                    $"ProcessAsync may not be called on this class without a valid {nameof(this.SourceProcessingSession)}.");
            }

            this.extensibilitySupport.FinalizeTables();

            EnableRequiredSourceDataCookers();

            this.SourceProcessingSession.ProcessSource(this.Logger, progress, cancellationToken);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override void BuildTableCore(
            TableDescriptor tableDescriptor,
            Action<ITableBuilder, IDataExtensionRetrieval> createTable,
            ITableBuilder tableBuilder)
        {
            Guard.NotNull(tableDescriptor, nameof(tableDescriptor));
            Guard.NotNull(createTable, nameof(createTable));
            Guard.NotNull(tableBuilder, nameof(tableBuilder));

            var dataRetrieval = GetDataExtensionRetrieval(tableDescriptor);

            if (dataRetrieval != null)
            {
                createTable(tableBuilder, dataRetrieval);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Builds on top of the functionality provided by the base class by enabling the data cookers required
        /// by the given table descriptor.
        /// </summary>
        protected override void OnTableEnabled(TableDescriptor tableDescriptor)
        {
            ProcessTableForExtensibility(tableDescriptor);
        }

        /// <summary>
        /// This returns an instance of <see cref="IDataExtensionRetrieval"/> that is specific to a given
        /// <see cref="TableDescriptor"/>.
        /// </summary>
        /// <param name="tableDescriptor">Table descriptor</param>
        /// <returns>This can be used to retrieve data with which to build the given table.</returns>
        protected IDataExtensionRetrieval GetDataExtensionRetrieval(TableDescriptor tableDescriptor)
        {
            return this.extensibilitySupport.GetDataExtensionRetrieval(tableDescriptor);
        }

        /// <summary>
        /// Enables an implementation to perform additional processing during a call to <see cref="EnableCooker"/>.
        /// </summary>
        /// <param name="sourceDataCookerFactory">Source data cooker factory</param>
        protected virtual void EnableCookerCore(ISourceDataCookerFactory sourceDataCookerFactory)
        {
        }

        /// <summary>
        /// Override to be notified when a source data cooker has been enabled for the session.
        /// </summary>
        /// <param name="sourceDataCooker">The cooker that was enabled.</param>
        protected virtual void OnDataCookerEnabled(ISourceDataCooker<T, TContext, TKey> sourceDataCooker)
        {
        }

        /// <summary>
        /// Override to be notified when all cookers have been enabled for the session.
        /// </summary>
        protected virtual void OnAllCookersEnabled()
        {
        }

        private void ProcessTableForExtensibility(TableDescriptor tableDescriptor)
        {
            if (tableDescriptor.RequiredDataCookers.Any() || tableDescriptor.RequiredDataProcessors.Any())
            {
                if (!this.extensibilitySupport.AddTable(tableDescriptor))
                {
                    throw new InvalidOperationException($"Unable to process table {tableDescriptor.Name} for dependencies.");
                }
            }
        }

        private void EnableRequiredSourceDataCookers()
        {
            var requiredCookers = this.extensibilitySupport.GetAllRequiredSourceDataCookers();

            foreach (var dataCookerPath in requiredCookers)
            {
                ISourceDataCookerFactory cookerFactory
                    = this.ApplicationEnvironment.SourceDataCookerFactoryRetrieval.GetSourceDataCookerFactory(dataCookerPath);
                EnableCooker(cookerFactory);
            }

            this.OnAllCookersEnabled();
        }
    }
}

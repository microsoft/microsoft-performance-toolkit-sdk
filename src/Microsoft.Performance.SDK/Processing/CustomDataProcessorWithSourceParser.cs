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
using Microsoft.Performance.SDK.Extensibility.Exceptions;
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
    public abstract class CustomDataProcessorWithSourceParser<T, TContext, TKey>
        : CustomDataProcessor,
          ICustomDataProcessorWithSourceParser<T, TContext, TKey>
          where T : IKeyedDataType<TKey>
    {
        private readonly IDataProcessorExtensibilitySupport extensibilitySupport;

        /// <summary>
        /// This constructor will setup the data processor so that it can use the data extension framework - allowing
        /// table and data cookers both internally and external to this plugin.
        /// </summary>
        /// <param name="sourceParser">The source data parser</param>
        /// <param name="options">Processor options</param>
        /// <param name="applicationEnvironment">Application environment</param>
        /// <param name="processorEnvironment">Processor environment</param>
        /// <param name="allTablesMapping">Maps table descriptors to possible build actions</param>
        /// <param name="metadataTables">Metadata tables</param>
        protected CustomDataProcessorWithSourceParser(
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

            EnableExtensionMetadataTables(metadataTables);
        }

        /// <summary>
        /// This constructor is intended for use within the unified scenario - where a data processor might need to
        /// spawn additional data processors. Data from the unified data processor will be copied to the spawned
        /// data processor as appropriate.
        /// </summary>
        /// <param name="other">An existing custom data processor</param>
        protected CustomDataProcessorWithSourceParser(CustomDataProcessorWithSourceParser<T, TContext, TKey> other)
            : this(
                  other.SourceParser,
                  other.Options,
                  other.ApplicationEnvironment,
                  other.ProcessorEnvironment,
                  other.TableDescriptorToBuildAction,
                  other.EnabledTables)
        {
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

        /// <inheritdoc/>
        public void EnableCooker(ISourceDataCookerFactory sourceDataCookerFactory)
        {
            Guard.NotNull(sourceDataCookerFactory, nameof(sourceDataCookerFactory));

            Debug.Assert(this.SourceProcessingSession != null);

            EnableCookerCore(sourceDataCookerFactory);

            if (sourceDataCookerFactory.CreateInstance() is ISourceDataCooker<T, TContext, TKey> cooker)
            {
                if (!StringComparer.Ordinal.Equals(cooker.Path.SourceParserId, this.SourceParserId))
                {
                    throw new ArgumentException(
                        "The given cooker is not applicable to this data processor.");
                }
                else
                {
                    this.SourceProcessingSession.RegisterSourceDataCooker(cooker);
                    this.OnDataCookerEnabled(cooker);
                }
            }
            else
            {
                throw new ArgumentException("The given cooker is not applicable to this data processor.");
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataOutputPath"/> does not target the data processor.
        ///     - or -
        ///     <paramref name="dataOutputPath"/>'s data cooker is not available.
        /// </exception>
        public TOutput QueryOutput<TOutput>(DataOutputPath dataOutputPath)
        {
            if (!StringComparer.Ordinal.Equals(dataOutputPath.SourceParserId, SourceParserId))
            {
                throw new ArgumentException(
                    message: $"{nameof(dataOutputPath)} does not target this data processor.",
                    paramName: nameof(dataOutputPath));
            }

            var dataCooker = this.SourceProcessingSession.GetSourceDataCooker(dataOutputPath.CookerPath);
            if (dataCooker == null)
            {
                throw new ArgumentException(
                    message: $"Requested data cooker is not available: {dataOutputPath.CookerPath}",
                    paramName: nameof(dataOutputPath));
            }

            return dataCooker.QueryOutput<TOutput>(dataOutputPath);
        }

        /// <inheritdoc/>
        /// <remarks>
        ///     Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataOutputPath"/> does not target the data processor.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The data cooker referenced by <paramref name="dataOutputPath"/> is not enabled on the data processor.
        /// </exception>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            if (!StringComparer.Ordinal.Equals(dataOutputPath.SourceParserId, SourceParserId))
            {
                throw new ArgumentException(
                    $"{nameof(dataOutputPath)} does not target this data processor.");
            }

            var dataCooker = this.SourceProcessingSession.GetSourceDataCooker(dataOutputPath.CookerPath);
            if (dataCooker == null)
            {
                throw new InvalidOperationException(string.Format(
                    "The specified data cooker '{0}' is not enabled on this data processor.",
                    dataOutputPath.CookerPath));
            }

            return dataCooker.QueryOutput(dataOutputPath);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </remarks>
        public bool TryQueryOutput<TOutput>(DataOutputPath identifier, out TOutput result)
        {
            bool success = TryQueryOutput(identifier, out var baseResult);
            if (success)
            {
                try
                {
                    result = (TOutput)baseResult;
                    return true;
                }
                catch (Exception)
                {
                }
            }

            result = default;
            return false;
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Returns data from a source data cooker registered with <see cref="SourceProcessingSession"/>.
        /// </remarks>
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object result)
        {
            result = default;

            if (!StringComparer.Ordinal.Equals(dataOutputPath.SourceParserId, SourceParserId))
            {
                return false;
            }

            try
            {
                var dataCooker = this.SourceProcessingSession.GetSourceDataCooker(dataOutputPath.CookerPath);
                if (dataCooker is null)
                {
                    return false;
                }

                result = dataCooker.QueryOutput(dataOutputPath);
                return true;
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        ///     This implementation does nothing because this object isn't finished initializing
        ///     when this is called by the base class.
        /// </summary>
        /// <param name="metadataTables">
        ///     Metadata tables.
        /// </param>
        protected override void EnableMetadataTables(IEnumerable<TableDescriptor> metadataTables)
        {
            base.EnableMetadataTables(metadataTables.Where(td => !td.RequiresDataExtensions()));
        }

        /// <inheritdoc cref="CustomDataProcessor"/>
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

        /// <inheritdoc/>
        /// <exception cref="ExtensionTableException">
        ///     The requested table cannot be enabled.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The <see cref="IDataProcessorExtensibilitySupport"/> has already been finalized.
        /// </exception>
        protected override void OnBeforeEnableTable(TableDescriptor tableDescriptor)
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
            if (!tableDescriptor.RequiresDataExtensions())
            {
                // there's nothing that needs to be done to prepare for this table by this processor
                return;
            }

            // Processors need to be able to get an IDataExtensionRetrieval object to build
            // internal tables. Calling this here will enable this as well as any required
            // source data cookers.
            //
            this.extensibilitySupport.EnableTable(tableDescriptor);
        }

        private void EnableRequiredSourceDataCookers()
        {
            var requiredCookers = this.extensibilitySupport.GetRequiredSourceDataCookers();

            foreach (var dataCookerPath in requiredCookers)
            {
                ISourceDataCookerFactory cookerFactory
                    = this.ApplicationEnvironment.SourceDataCookerFactoryRetrieval.GetSourceDataCookerFactory(dataCookerPath);
                EnableCooker(cookerFactory);
            }

            this.OnAllCookersEnabled();
        }

        /// <summary>
        ///     This method enabled the source data cookers required by extension internal tables, only metadata tables for now.
        /// </summary>
        /// <param name="tables"></param>
        private void EnableExtensionMetadataTables(IEnumerable<TableDescriptor> tables)
        {
            foreach (var table in tables.Where(td => td.RequiresDataExtensions()))
            {
                try
                {
                    EnableTable(table);
                }
                catch (WrongProcessorTableException e)
                {
                    // An IProcessingSource might pass in tables that this processor doesn't handle.
                    this.Logger?.Verbose(string.Format("{0}", e));
                }
                catch (InternalTableReferencesMultipleSourceParsersException e)
                {
                    // This invalid table should be logged but shouldn't prevent this processor from executing.
                    this.Logger?.Warn(string.Format("{0}", e));
                }
                catch (Exception e)
                {
                    this.Logger.Warn(
                        "Failed to enable internal table {0} on processor {1}: {2}.",
                        table.Type,
                        this.GetType().FullName,
                        e);

                    throw;
                }
            }
        }
    }
}

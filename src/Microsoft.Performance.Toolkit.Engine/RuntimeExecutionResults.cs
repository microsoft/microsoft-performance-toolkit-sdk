// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents the results of processing data using the <see cref="Engine"/>.
    /// </summary>
    public sealed class RuntimeExecutionResults
        : ICookedDataRetrieval,
          IDisposable
    {
        private readonly ICompositeCookerRepository compositeCookerData;

        // The following fields aren't 'readonly' because they're set to null when disposing.
        private ICookedDataRetrieval sourceCookerData;
        private IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap;

        private IDataExtensionRetrievalFactory retrievalFactory;
        private IDataExtensionRepository repository;
        private IEnumerable<DataCookerPath> sourceCookerPaths;

        private bool disposedValue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeExecutionResults"/>
        ///     class.
        /// </summary>
        /// <param name="sourceCookerData">
        ///     The retrieval interface for getting to source cooker data.
        /// </param>
        /// <param name="compositeCookerData">
        ///     The retrieval interface for getting to composite cooker data.
        /// </param>
        /// <param name="retrievalFactory">
        ///     The factory for creating retrievals for composite cookers.
        /// </param>
        /// <param name="repository">
        ///     The repository that was used to process the data.
        /// </param>
        /// <param name="tableToProcessorMap">
        ///     The map of a <see cref="TableDescriptor"/> to the <see cref="ICustomDataProcessor"/> that produces
        ///     that table.
        /// </param>
        /// <param name="errors">
        ///     The collection of errors, if any, that were encountered during processing.
        /// </param>
        /// <param name="logger">
        ///     The logger to use for logging.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="sourceCookerData"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="compositeCookerData"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="retrievalFactory"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="repository"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="tableToProcessorMap"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="errors"/> is <c>null</c>.
        /// </exception>
        /// TODO: Going to move RuntimeExecutionResults to internal and expose calls via new interface
        ///       1 - 1 with the Engine when calling Process
        internal RuntimeExecutionResults(
            ICookedDataRetrieval sourceCookerData,
            ICompositeCookerRepository compositeCookerData,
            IDataExtensionRetrievalFactory retrievalFactory,
            IDataExtensionRepository repository,
            IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap,
            IEnumerable<ProcessingError> errors)
        {
            Guard.NotNull(sourceCookerData, nameof(sourceCookerData));
            Guard.NotNull(compositeCookerData, nameof(compositeCookerData));
            Guard.NotNull(retrievalFactory, nameof(retrievalFactory));
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(tableToProcessorMap, nameof(tableToProcessorMap));
            Guard.NotNull(errors, nameof(errors));

            this.sourceCookerData = sourceCookerData;
            this.compositeCookerData = compositeCookerData;
            this.retrievalFactory = retrievalFactory;
            this.repository = repository;
            this.sourceCookerPaths = new HashSet<DataCookerPath>(this.repository.SourceDataCookers);
            this.tableToProcessorMap = tableToProcessorMap;

            this.ProcessingErrors = errors.OfType<ProcessingError>().ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets the collection of errors, if any, that occurred during processing.
        /// </summary>
        public IReadOnlyCollection<ProcessingError> ProcessingErrors { get; }

        /// <summary>
        ///     Gets the direct cooker retrieval for the specified
        ///     cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <returns>
        ///     The object to query for cooked data from the specified cooker.
        /// </returns>
        /// <exception cref="CookerNotFoundException">
        ///     <paramref name="cookerPath"/> does not represent a known cooker.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public CookerOutput GetCookedData(DataCookerPath cookerPath)
        {
            this.ThrowIfDisposed();

            return new CookerOutput(cookerPath, GetRetrieval(cookerPath));
        }

        /// <summary>
        ///     Attempts to get the data retrieval for the specified cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <param name="retrieval">
        ///     The object to query for cooked data, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the cooker can be queried;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryGetCookedData(DataCookerPath cookerPath, out CookerOutput retrieval)
        {
            this.ThrowIfDisposed();

            try
            {
                retrieval = this.GetCookedData(cookerPath);
                return true;
            }
            catch
            {
                retrieval = null;
                return false;
            }
        }

        /// <summary>
        ///     Queries for processed data from the given path.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being queried.
        /// </typeparam>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <returns>
        ///     The cooked data.
        /// </returns>
        /// <exception cref="DataOutputNotFoundException">
        ///     No processed data could be found for the given <paramref name="dataOutputPath"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public T QueryOutput<T>(DataOutputPath dataOutputPath)
        {
            return (T)this.QueryOutput(dataOutputPath);
        }

        /// <summary>
        ///     Queries for processed data from the given path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <returns>
        ///     The cooked data.
        /// </returns>
        /// <exception cref="DataOutputNotFoundException">
        ///     No processed data could be found for the given <paramref name="dataOutputPath"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            this.ThrowIfDisposed();

            try
            {
                var retrieval = this.GetRetrieval(dataOutputPath.CookerPath);
                return retrieval.QueryOutput(dataOutputPath);
            }
            catch (Exception e)
            {
                throw new DataOutputNotFoundException(dataOutputPath, e);
            }
        }

        /// <summary>
        ///     Attempts to query for processed data from the given path.
        /// </summary>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <param name="data">
        ///     The retrieved data, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if data at the given path was found;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object data)
        {
            this.ThrowIfDisposed();

            try
            {
                data = this.QueryOutput(dataOutputPath);
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        /// <summary>
        ///     Attempts to query for processed data from the given path.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being queried.
        /// </typeparam>
        /// <param name="dataOutputPath">
        ///     The fully qualified output path to the data to retrieve.
        /// </param>
        /// <param name="data">
        ///     The retrieved data, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if data at the given path was found;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryQueryOutput<T>(DataOutputPath dataOutputPath, out T data)
        {
            if (this.TryQueryOutput(dataOutputPath, out object dataRaw))
            {
                data = (T)dataRaw;
                return true;
            }

            data = default(T);
            return false;
        }

        /// <summary>
        ///     Checks if the given <paramref name="tableDescriptor"/> has data available.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The table to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> the table has data available;
        ///     <c>false</c> otherwise.
        ///     <c>null</c> the table does not support checking if data is available.
        /// </returns>
        /// <remarks>
        ///     If this method returns <c>null</c> and you must check if the given 
        ///     descriptor has data, you can check if the <see cref="ITableBuilder"/> 
        ///     returned from <see cref="BuildTable(TableDescriptor)"/> 
        ///     has a row count greater than 0."
        /// </remarks>
        /// <exception cref="TableException">
        ///     A exception occured when calling IsDataAvailable on the specified <paramref name="tableDescriptor"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool? IsTableDataAvailable(TableDescriptor tableDescriptor)
        {
            this.ThrowIfDisposed();

            try
            {
                if (!this.tableToProcessorMap.TryGetValue(tableDescriptor, out ICustomDataProcessor processor))
                {
                    return null;
                }
                else if (processor is null)
                {
                    bool result = ExecuteOnRepositoryIfContained(
                        tableDescriptor,
                        (reference, tableRetrieval) => reference.IsDataAvailableFunc?.Invoke(tableRetrieval),
                        out bool? repoIsDataAvail);

                    Debug.Assert(result, "Table was enabled but unable to query IsDataAvailable.");
                    return repoIsDataAvail;
                }

                return processor.DoesTableHaveData(tableDescriptor);
            }
            catch (Exception inner)
            {
                throw new TableException(
                    $"An exception was thrown while calling IsDataAvailable for the {tableDescriptor}.",
                    tableDescriptor,
                    inner);
            }
        }

        /// <summary>
        ///     Builds a table with the given <paramref name="tableDescriptor"/>.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The table to build.
        /// </param>
        /// <returns>
        ///     The built table.
        /// </returns>
        /// <exception cref="TableException">
        ///     A table cannot be built for the given <paramref name="tableDescriptor"/>.
        /// </exception>
        /// <exception cref="TableNotEnabledException">
        ///     A table for the given <paramref name="tableDescriptor"/> was not enabled.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public ITableResult BuildTable(TableDescriptor tableDescriptor)
        {
            this.ThrowIfDisposed();

            if (!this.tableToProcessorMap.TryGetValue(tableDescriptor, out ICustomDataProcessor processor))
            {
                throw new TableNotEnabledException(tableDescriptor);
            }

            try
            {
                var tableBuilder = new TableBuilder();

                if (processor is null)
                {
                    bool result = ExecuteOnRepositoryIfContained(
                        tableDescriptor,
                        (reference, tableRetrieval) => { reference.BuildTableAction(tableBuilder, tableRetrieval); return tableBuilder; },
                        out ITableResult repoTableResult);

                    Debug.Assert(result, "Table was enabled but unable to BuildTable.");
                    return repoTableResult;
                }

                processor.BuildTable(tableDescriptor, tableBuilder);
                return tableBuilder;
            }
            catch (Exception inner)
            {
                throw new TableException(
                    $"An exception was thrown while calling BuildTable for {tableDescriptor}.",
                    tableDescriptor,
                    inner);
            }
        }

        /// <summary>
        ///     Attempts to build a table with the given <paramref name="tableDescriptor"/>.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The table to build.
        /// </param>
        /// <param name="filledTableBulder">
        ///     The built table, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the table for the given <paramref name="tableDescriptor"/> was built;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        public bool TryBuildTable(TableDescriptor tableDescriptor, out ITableResult filledTableBulder)
        {
            this.ThrowIfDisposed();

            try
            {
                filledTableBulder = BuildTable(tableDescriptor);
                return true;
            }
            catch (TableException)
            {
                filledTableBulder = null;
                return false;
            }
        }

        private ICookedDataRetrieval GetRetrieval(DataCookerPath cookerPath)
        {
            if (this.sourceCookerPaths.Contains(cookerPath))
            {
                return this.sourceCookerData;
            }

            if (cookerPath.DataCookerType == DataCookerType.SourceDataCooker)
            {
                // This source cooker path wasn't in the list of available source data cookers, so throw.
                throw new CookerNotFoundException(cookerPath);
            }

            try
            {
                return this.compositeCookerData.GetCookerOutput(cookerPath);
            }
            catch (Exception e)
            {
                throw new CookerNotFoundException(cookerPath, e);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Will perform the Func on the Repository if the table is contained, else false.
        private bool ExecuteOnRepositoryIfContained<TResult>(
            TableDescriptor tableDescriptor,
            Func<ITableExtensionReference, IDataExtensionRetrieval, TResult> func,
            out TResult result)
        {
            this.ThrowIfDisposed();

            result = default;

            if (this.repository.TablesById.TryGetValue(tableDescriptor.Guid, out ITableExtensionReference reference))
            {
                var tableRetrieval = this.retrievalFactory.CreateDataRetrievalForTable(tableDescriptor.Guid);

                result = func(reference, tableRetrieval);

                return true;
            }

            return false;
        }

        // Will perform the Func on the Processor if the table is contained, else false.
        private bool ExecuteOnProcessorIfContained<TResult>(
            TableDescriptor tableDescriptor,
            Func<ICustomDataProcessor, TResult> func,
            out TResult result)
        {
            this.ThrowIfDisposed();
            result = default;

            if (this.tableToProcessorMap.TryGetValue(tableDescriptor, out ICustomDataProcessor processor))
            {
                Debug.Assert(processor != null);
                result = func(processor);

                return true;
            }

            return false;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.compositeCookerData.SafeDispose();
                }

                this.sourceCookerData = null;
                this.tableToProcessorMap = null;
                this.retrievalFactory = null;
                this.repository = null;
                this.sourceCookerPaths = null;
                disposedValue = true;
            }
        }

        private sealed class TableBuilder
            : ITableBuilder,
              ITableBuilderWithRowCount,
              ITableResult
        {
            private readonly List<TableConfiguration> builtInTableConfigurations;
            private readonly ReadOnlyCollection<TableConfiguration> builtInTableConfigurationsRO;

            private readonly List<IDataColumn> columns;
            private readonly ReadOnlyCollection<IDataColumn> columnsRO;

            private readonly ColumnVariantsRegistrar variantsRegistrar;

            private readonly Dictionary<string, TableCommandCallback> tableCommands;
            private readonly ReadOnlyDictionary<string, TableCommandCallback> tableCommandsRO;

            internal TableBuilder()
            {
                this.columns = new List<IDataColumn>();
                this.columnsRO = new ReadOnlyCollection<IDataColumn>(this.columns);

                this.builtInTableConfigurations = new List<TableConfiguration>();
                this.builtInTableConfigurationsRO = new ReadOnlyCollection<TableConfiguration>(this.builtInTableConfigurations);

                this.tableCommands = new Dictionary<string, TableCommandCallback>();
                this.tableCommandsRO = new ReadOnlyDictionary<string, TableCommandCallback>(this.tableCommands);
                this.variantsRegistrar = new ColumnVariantsRegistrar();
            }

            //
            // ITableBuilder
            //

            public IEnumerable<TableConfiguration> BuiltInTableConfigurations => this.builtInTableConfigurationsRO;

            public TableConfiguration DefaultConfiguration { get; private set; }

            /// <inheritdoc />
            public IReadOnlyDictionary<IDataColumn, IReadOnlyDictionary<ColumnVariantIdentifier, IDataColumn>> ColumnVariants
            {
                get
                {
                    return this.variantsRegistrar.GetAllVariants();
                }
            }

            public IReadOnlyDictionary<string, TableCommandCallback> TableCommands => this.tableCommandsRO;

            public Func<int, IEnumerable<TableRowDetailEntry>> TableRowDetailsGenerator { get; private set; }

            public ITableBuilder AddTableCommand(string commandName, TableCommandCallback callback)
            {
                this.tableCommands.TryAdd(commandName, callback);
                return this;
            }

            public ITableBuilder AddTableConfiguration(TableConfiguration configuration)
            {
                this.builtInTableConfigurations.Add(configuration);
                return this;
            }

            public ITableBuilder SetDefaultTableConfiguration(TableConfiguration configuration)
            {
                this.DefaultConfiguration = configuration;
                return this;
            }

            public ITableBuilderWithRowCount SetRowCount(int numberOfRows)
            {
                this.RowCount = numberOfRows;
                return this;
            }

            public int RowCount { get; private set; }

            /// <inheritdoc />
            public IReadOnlyCollection<IDataColumn> Columns => this.columnsRO;

            public ITableBuilderWithRowCount AddColumn(IDataColumn column)
            {
                return this.AddColumnWithVariants(column, null);
            }

            /// <inheritdoc />
            public ITableBuilderWithRowCount AddColumnWithVariants(
                IDataColumn column,
                Action<RootColumnBuilder> options)
            {
                Guard.NotNull(column, nameof(column));

                this.columns.Add(column);

                if (options != null)
                {
                    var processor = new BuiltColumnVariantsRegistrant(column, this.variantsRegistrar);
                    var builder = new EmptyColumnBuilder(
                        processor,
                        column);

                    options(builder);
                }

                return this;
            }

            public ITableBuilderWithRowCount ReplaceColumn(IDataColumn oldColumn, IDataColumn newColumn)
            {
                this.columns.Remove(oldColumn);
                this.columns.Add(newColumn);
                return this;
            }

            public ITableBuilderWithRowCount SetTableRowDetailsGenerator(Func<int, IEnumerable<TableRowDetailEntry>> generator)
            {
                this.TableRowDetailsGenerator = generator;
                return this;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Represents the results of processing data using the <see cref="Engine"/>.
    /// </summary>
    public sealed class RuntimeExecutionResults
    {
        private readonly ICookedDataRetrieval cookedDataRetrieval;
        private readonly IDataExtensionRetrievalFactory retrievalFactory;
        private readonly IDataExtensionRepository repository;
        private readonly IEnumerable<DataCookerPath> sourceCookers;
        private readonly IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RuntimeExecutionResults"/>
        ///     class.
        /// </summary>
        /// <param name="cookedDataRetrieval">
        ///     The retrieval interface for getting to cooked data.
        /// </param>
        /// <param name="retrievalFactory">
        ///     The factory for creating retrievals for composite cookers.
        /// </param>
        /// <param name="repository">
        ///     The repository that was used to process the data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="cookedDataRetrieval"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="retrievalFactory"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="repository"/> is <c>null</c>.
        /// </exception>
        /// TODO: Going to move RuntimeExecutionResults to internal and expose calls via new interface
        ///       1 - 1 with the Engine when calling Process
        public RuntimeExecutionResults(
            ICookedDataRetrieval cookedDataRetrieval,
            IDataExtensionRetrievalFactory retrievalFactory,
            IDataExtensionRepository repository,
            IDictionary<TableDescriptor, ICustomDataProcessor> tableToProcessorMap)
        {
            Guard.NotNull(cookedDataRetrieval, nameof(cookedDataRetrieval));
            Guard.NotNull(retrievalFactory, nameof(retrievalFactory));
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(tableToProcessorMap, nameof(tableToProcessorMap));

            this.cookedDataRetrieval = cookedDataRetrieval;
            this.retrievalFactory = retrievalFactory;
            this.repository = repository;
            this.sourceCookers = new HashSet<DataCookerPath>(this.repository.SourceDataCookers);
            this.tableToProcessorMap = tableToProcessorMap;
        }

        /// <summary>
        ///     Gets the direct cooker retrieval for the specified
        ///     cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <returns>
        ///     The interface to query for cooked data from said cooker.
        /// </returns>
        /// <exception cref="CookerNotFoundException">
        ///     <paramref name="cookerPath"/> does not represent a known cooker.
        /// </exception>
        public ICookedDataRetrieval GetCookedData(DataCookerPath cookerPath)
        {
            if (this.sourceCookers.Contains(cookerPath))
            {
                return this.cookedDataRetrieval;
            }

            try
            {
                var cooker = this.repository.GetCompositeDataCookerReference(cookerPath);
                var retrieval = this.retrievalFactory.CreateDataRetrievalForCompositeDataCooker(cookerPath);
                return cooker.GetOrCreateInstance(retrieval);
            }
            catch (Exception e)
            {
                throw new CookerNotFoundException(cookerPath, e);
            }
        }

        /// <summary>
        ///     Attempts to get the direct cooker retrieval for the specified
        ///     cooker.
        /// </summary>
        /// <param name="cookerPath">
        ///     The path to the cooker to retrieve.
        /// </param>
        /// <param name="retrieval">
        ///     The found retrieval, if any.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the cooker can be queried;
        ///     <c>false</c> otherwise.
        /// </returns>
        public bool TryGetCookedData(DataCookerPath cookerPath, out ICookedDataRetrieval retrieval)
        {
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
        public object QueryOutput(DataOutputPath dataOutputPath)
        {
            try
            {
                var cooker = this.GetCookedData(dataOutputPath.CookerPath);
                return cooker.QueryOutput(dataOutputPath);
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
        public bool TryQueryOutput(DataOutputPath dataOutputPath, out object data)
        {
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
        ///     has a <see cref="ITableBuilder.RowCount"/> greater than 0."
        /// </remarks>
        /// <exception cref="TableException">
        ///     A exception occured when calling IsDataAvailable on the specified <paramref name="tableDescriptor"/>.
        /// </exception>
        public bool? IsTableDataAvailable(TableDescriptor tableDescriptor)
        {
            try
            {
                if (FuncOnRepositoryIfContained(tableDescriptor, (reference, tableRetrieval) => reference.IsDataAvailableFunc?.Invoke(tableRetrieval), out bool? repoIsDataAvail))
                {
                    return repoIsDataAvail;
                }

                if (FuncOnProcessorIfContained(tableDescriptor, (processor) => processor.DoesTableHaveData(tableDescriptor), out bool processorIsDataAvail))
                {
                    return processorIsDataAvail;
                }
            }
            catch (Exception inner)
            {
                throw new TableException(tableDescriptor, inner);
            }

            return null;
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
        public ITableResult BuildTable(TableDescriptor tableDescriptor)
        {
            var tableBuilder = new TableBuilder();

            try
            {
                if (FuncOnRepositoryIfContained(tableDescriptor, (reference, tableRetrieval) => { reference.BuildTableAction(tableBuilder, tableRetrieval); return tableBuilder; }, out ITableResult repoTableResult))
                {
                    return repoTableResult;
                }

                if (FuncOnProcessorIfContained(tableDescriptor, (processor) => { processor.BuildTable(tableDescriptor, tableBuilder); return tableBuilder; }, out ITableResult processorTableResult))
                {
                    return processorTableResult;
                }
            }
            catch (Exception inner)
            {
                throw new TableException(tableDescriptor, inner);
            }

            throw new TableException(tableDescriptor);
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
        public bool TryBuildTable(TableDescriptor tableDescriptor, out ITableResult filledTableBulder)
        {
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

        // Will perform the Func on the Repository if the table is contained, else false.
        private bool FuncOnRepositoryIfContained<TResult>(TableDescriptor tableDescriptor, Func<ITableExtensionReference, IDataExtensionRetrieval, TResult> func, out TResult result)
        {
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
        private bool FuncOnProcessorIfContained<TResult>(TableDescriptor tableDescriptor, Func<ICustomDataProcessor, TResult> func, out TResult result)
        {
            result = default;

            if (this.tableToProcessorMap.TryGetValue(tableDescriptor, out ICustomDataProcessor processor))
            {
                result = func(processor);                    

                return true;
            }

            return false;
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
            }

            //
            // ITableBuilder
            //

            public IEnumerable<TableConfiguration> BuiltInTableConfigurations => this.builtInTableConfigurationsRO;

            public TableConfiguration DefaultConfiguration { get; private set; }

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

            public IReadOnlyCollection<IDataColumn> Columns => this.columnsRO;

            public ITableBuilderWithRowCount AddColumn(IDataColumn column)
            {
                this.columns.Add(column);
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

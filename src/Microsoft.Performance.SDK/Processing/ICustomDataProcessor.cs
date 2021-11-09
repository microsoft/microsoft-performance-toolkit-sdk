// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Extensibility.Exceptions;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Implementations of this interface process a data source
    ///     in order to create tables showing the data in the source.
    ///     This is the core of how data is transformed into graphable /
    ///     analyzable data.
    /// </summary>
    public interface ICustomDataProcessor
    {
        /// <summary>
        ///     Instructs the processor that a table is being
        ///     requested by the user. This means that the processor
        ///     should do whatever is necessary in <see cref="ProcessAsync(IProgress{int}, CancellationToken)"/>
        ///     to make sure the table can be used.
        ///     <para />
        ///     This method must be thread-safe.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The <see cref="TableDescriptor"/> that instructs the
        ///     processor as to which table is being requested.
        /// </param>
        /// <exception cref="ExtensionTableException">
        ///     The requested table cannot be enabled.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The <see cref="IDataProcessorExtensibilitySupport"/> has already been finalized.
        /// </exception>

        void EnableTable(TableDescriptor tableDescriptor);

        /// <summary>
        ///     Instructs the processor that a table is being
        ///     requested by the user. This means that the processor
        ///     should do whatever is necessary in ProcessAsync to
        ///     make sure the table can be used.
        ///     <para />
        ///     This method must be thread-safe.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     The <see cref="TableDescriptor"/> that instructs the
        ///     processor as to which table is being requested.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the table was enabled; <c>false</c> otherwise.
        /// </returns>
        bool TryEnableTable(TableDescriptor tableDescriptor);

        /// <summary>
        ///     Asynchronously processes the data source.
        /// </summary>
        /// <param name="progress">
        ///     Provides a method of updating the application as to this
        ///     processor's progress.
        /// </param>
        /// <param name="cancellationToken">
        ///     A means of the application signaling to the processor that
        ///     it should abort processing.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the asynchronous operation.
        /// </returns>
        Task ProcessAsync(
            IProgress<int> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Gets information about the processed data source.
        ///     This method will be called after <see cref="ProcessAsync(IProgress{int}, CancellationToken)"/>
        ///     has completed successfully.
        /// </summary>
        /// <returns>
        ///     Information about the data source that was processed.
        /// </returns>
        DataSourceInfo GetDataSourceInfo();

        /// <summary>
        ///     Instructs the processor to build the requested table
        ///     into an actual structure.
        ///     <para />
        ///     This method must be thread-safe.
        /// </summary>
        /// <param name="table">
        ///     The table to build.
        /// </param>
        /// <param name="tableBuilder">
        ///     The builder to use to build the table.
        /// </param>
        void BuildTable(TableDescriptor table, ITableBuilder tableBuilder);

        /// <summary>
        ///     Instructs the processor to return the appropriate service
        ///     for the given table, if any.
        /// </summary>
        /// <param name="table">
        ///     The table whose service is to be created.
        /// </param>
        /// <returns>
        ///     The created service, if the table has a service. <c>null</c> otherwise.
        /// </returns>
        ITableService CreateTableService(TableDescriptor table);

        /// <summary>
        ///     This method is used to communicate whether the given table has any
        ///     data to show as a result of processing. This method is never called
        ///     before <see cref="ProcessAsync(IProgress{int}, CancellationToken)"/>. If this method returns <c>false</c>,
        ///     then the table will not be exposed.
        /// </summary>
        /// <param name="table">
        ///     The table being interrogated.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the table has at least one row of data;
        ///     <c>false</c> otherwise.
        /// </returns>
        bool DoesTableHaveData(TableDescriptor table);

        /// <summary>
        ///     Gets the tables that have been enabled on this processor.
        /// </summary>
        /// <returns>
        ///     The <see cref="TableDescriptor"/> of tables that have been enabled on this processor.
        /// </returns>
        IEnumerable<TableDescriptor> GetEnabledTables();
    }
}

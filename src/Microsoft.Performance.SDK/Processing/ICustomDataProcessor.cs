// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

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
        ///     Instructs the processor to that a table is being
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
        void EnableTable(TableDescriptor tableDescriptor);

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
        ///     This method will be called after <see cref="ProcessAsync(ILogger, IProgress{int}, CancellationToken)"/>
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
        ///     Instructs the processor to build all (if any) tables
        ///     that contain metadata about the data that has been processed.
        ///     <para />
        ///     This method must be thread-safe.
        /// </summary>
        /// <param name="metadataTableBuilderFactory">
        ///     A factory that the processor can use to get <see cref="ITableBuilder"/>s
        ///     to build metadata tables.
        /// </param>
        void BuildMetadataTables(
            IMetadataTableBuilderFactory metadataTableBuilderFactory);

        /// <summary>
        ///     This method is used to communicate whether the given table has any
        ///     data to show as a result of processing. This method is never called
        ///     before <see cref="nameof(ICustomDataProcessor.ProcessAsync)"/>. If
        ///     this method returns <c>false</c>, then the table will not be exposed.
        /// </summary>
        /// <param name="table">
        ///     The table being interrogated.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the table has at least one row of data;
        ///     <c>false</c> otherwise.
        /// </returns>
        bool DoesTableHaveData(TableDescriptor table);
    }
}

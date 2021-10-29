// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility.Exceptions;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Adds methods to support a custom data processor that supports data extensions.
    /// </summary>
    public interface IDataProcessorExtensibilitySupport
    {
        /// <summary>
        ///     This should be called for every extended table internal to an <see cref="IProcessingSource"/> that will need 
        ///     access to an <see cref="IDataExtensionRetrieval"/> object in order to build the table.
        ///     <para/>
        ///     If the table isn't internal, the data sources from the associated <see cref="ICustomDataProcessor"/>
        ///     will be enabled on that processor so that the table can be built later.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     Identifies the table to add.
        /// </param>
        /// <exception cref="ExtensionTableException">
        ///     The requested table cannot be enabled.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The <see cref="IDataProcessorExtensibilitySupport"/> has already been finalized.
        /// </exception>
        void EnableTable(TableDescriptor tableDescriptor);

        /// <summary>
        ///     This should be called for every extended table internal to an <see cref="IProcessingSource"/> that will need 
        ///     access to an <see cref="IDataExtensionRetrieval"/> object in order to build the table.
        ///     <para/>
        ///     If the table isn't internal, the data sources from the associated <see cref="ICustomDataProcessor"/>
        ///     will be enabled on that processor so that the table can be built later.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     Identifies the table to add.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a reference to the table was successfully enabled; otherwise false.
        /// </returns>
        bool TryEnableTable(TableDescriptor tableDescriptor);

        /// <summary>
        ///     This prepares the object for processing and must be called before
        ///     <see cref="ICustomDataProcessor.ProcessAsync(System.IProgress{int}, System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <remarks>
        ///     Additional tables may not be enabled after this method has executed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        ///     <see cref="FinalizeTables"/> has already been called.
        /// </exception>
        void FinalizeTables();

        /// <summary>
        ///     Creates an object that can be used to generate an <see cref="IDataExtensionRetrieval"/> required by a
        ///     data extension in the custom data processor.
        /// <para/>
        ///     This is used by a custom data processor to generate an IDataExtensionRetrieval for an internal
        ///     data extension.
        /// <para/>
        ///     The data available from a generated IDataExtensionRetrieval instance will be limited to the
        ///     data provided by the <see cref="ICustomDataProcessorWithSourceParser"/>.
        /// </summary>
        /// <returns>
        ///     An object that can be used to generate an IDataExtensionRetrieval, or null if the table descriptor is not
        ///     supported. This could happen if <see cref="TryEnableTable"/> returned false, or if <see cref="FinalizeTables"/>
        ///     filtered out the table.
        /// </returns>
        /// <remarks>
        ///     This is commonly used to generate an IDataExtensionRetrieval for an internal table, such as a
        ///     metadata table.
        /// </remarks>
        IDataExtensionRetrieval GetDataExtensionRetrieval(TableDescriptor tableDescriptor);

        /// <summary>
        ///     Retrieves a set of all source data cooker paths required by the tables that were added through a call to
        ///     <see cref="TryEnableTable(TableDescriptor)"/>.
        /// </summary>
        /// <returns>
        ///     A set of source data cooker paths.
        /// </returns>
        ISet<DataCookerPath> GetRequiredSourceDataCookers();

        /// <summary>
        ///     Retrieves an enumerable of internal tables that were added through a call to
        ///     <see cref="TryEnableTable(TableDescriptor)"/>.
        /// </summary>
        /// <returns>
        ///     An enumerable of tables.
        /// </returns>
        IEnumerable<TableDescriptor> GetEnabledInternalTables();
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Adds methods to support a custom data processor that supports data extensions. These methods make it
    ///     possible to create the <see cref="IDataExtensionRetrieval"/> object that is necessary for retrieving
    ///     data required by a data extension table.
    /// </summary>
    public interface IDataProcessorExtensibilitySupport
    {
        /// <summary>
        ///     This should be called for every table internal to a custom data source that will need access to an
        ///     <see cref="IDataExtensionRetrieval"/> object in order to build the table.
        /// </summary>
        /// <param name="tableDescriptor">
        ///     Identifies the table to add.
        /// </param>
        /// <returns>
        ///     True if a reference to the table was added successfully, otherwise false.
        /// </returns>
        bool AddTable(TableDescriptor tableDescriptor);

        /// <summary>
        ///     This processes all of the dependencies for tables internal to a custom data source, such as metadata 
        ///     tables. It should be called before processing a data source, after all data cookers have been enabled
        ///     on the custom data source.
        ///     </summary>
        /// <remarks>
        ///     A table that was added successfully through <see cref="AddTable"/> maybe be filtered out during this call.
        ///     This may happen because a required data extension is not available, or because the table requires a
        ///     source data cooker that is not associated with the source parser in the custom data processor.
        /// </remarks>
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
        ///     supported. This could happen if <see cref="AddTable"/> returned false, or if <see cref="FinalizeTables"/>
        ///     filtered out the table.
        /// </returns>
        /// <remarks>
        ///     This is commonly used to generate an IDataExtensionRetrieval for an internal table, such as a
        ///     metadata table.
        /// </remarks>
        IDataExtensionRetrieval GetDataExtensionRetrieval(TableDescriptor tableDescriptor);

        /// <summary>
        ///     Retrieves a set of all source data cooker paths required by the tables that were added through a call to
        ///     <see cref="AddTable(TableDescriptor)"/>.
        /// </summary>
        /// <returns>
        ///     A set of source data cooker paths.
        /// </returns>
        ISet<DataCookerPath> GetAllRequiredSourceDataCookers();

        /// <summary>
        ///     Retrieves an enumerable of tables that were added through a call to
        ///     <see cref="AddTable(TableDescriptor)"/>.
        /// </summary>
        /// <returns>
        ///     An enumerable of tables.
        /// </returns>
        IEnumerable<TableDescriptor> GetAllRequiredTables();
    }
}

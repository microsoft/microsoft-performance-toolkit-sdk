// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Exposes methods used to generate data required by a data extension. This data will be limited to
    ///     the data extensions marked as required by the given data extension.
    /// </summary>
    public interface IDataExtensionRetrievalFactory
    {
        /// <summary>
        ///     A composite cooker has access to source data cookers, as well as other composite data cookers.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     Identifies the composite data cooker.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this composite data cooker.
        /// </returns>
        IDataExtensionRetrieval CreateDataRetrievalForCompositeDataCooker(DataCookerPath dataCookerPath);

        /// <summary>
        ///     A data processor has access to source data cookers, composite data cookers, as well as other
        ///     data processors.
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Identifies the data processor.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this data processor.
        /// </returns>
        IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(DataProcessorId dataProcessorId);

        /// <summary>
        ///     A table has access to source data cookers, composite data cookers, and data processors.
        /// </summary>
        /// <param name="tableId">
        ///     Identifies the table.
        /// </param>
        /// <returns>
        ///     A set of data uniquely tailored to this table.
        /// </returns>
        IDataExtensionRetrieval CreateDataRetrievalForTable(Guid tableId);
    }
}

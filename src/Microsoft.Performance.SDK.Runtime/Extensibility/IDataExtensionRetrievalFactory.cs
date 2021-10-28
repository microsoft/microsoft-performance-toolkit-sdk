// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     Used to generate an <see cref="IDataExtensionRetrieval"/> unique to a given data extension. The
    ///     <see cref="IDataExtensionRetrieval"/> has access only to data that the data extension has identified as
    ///     required.
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

        // TODO: __SDK_DP__
        // Redesign Data Processor API
        /////// <summary>
        ///////     A data processor has access to source data cookers, composite data cookers, as well as other
        ///////     data processors.
        /////// </summary>
        /////// <param name="dataProcessorId">
        ///////     Identifies the data processor.
        /////// </param>
        /////// <returns>
        ///////     A set of data uniquely tailored to this data processor.
        /////// </returns>
        ////IDataExtensionRetrieval CreateDataRetrievalForDataProcessor(DataProcessorId dataProcessorId);

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

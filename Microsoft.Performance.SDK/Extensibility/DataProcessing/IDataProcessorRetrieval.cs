// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataProcessing
{
    /// <summary>
    ///     Offers access to data processors.
    /// </summary>
    public interface IDataProcessorRetrieval
    {
        /// <summary>
        ///     Query for a data processor by Id.
        /// </summary>
        /// <param name="dataProcessorId">
        ///     Data processor Id.
        /// </param>
        /// <returns>
        ///     Data processor.
        /// </returns>
        object QueryDataProcessor(DataProcessorId dataProcessorId);
    }
}

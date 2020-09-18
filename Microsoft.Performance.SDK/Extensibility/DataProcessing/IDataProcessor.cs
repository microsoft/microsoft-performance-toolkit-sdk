// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataProcessing
{
    /// <summary>
    ///     Should be implemented on a data processor data extension.
    /// </summary>
    public interface IDataProcessor
        : IDataProcessorDescriptor,
          IDataCookerDependent,
          IDataProcessorDependent
    {
        /// <summary>
        ///     Initializes a data processor extension with its required data.
        /// </summary>
        /// <param name="dataExtensionRetrieval">
        ///     Required data.
        /// </param>
        void OnDataAvailable(IDataExtensionRetrieval dataExtensionRetrieval);
    }
}

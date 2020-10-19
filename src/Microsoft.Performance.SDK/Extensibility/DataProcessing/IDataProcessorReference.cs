// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataProcessing
{
    /// <summary>
    ///     An interface to describe and instantiate a given data cooker.
    /// </summary>
    public interface IDataProcessorCreator
    {
        /// <summary>
        ///     Create an instance of the described data processor.
        /// </summary>
        /// <param name="requiredData">
        ///     Data required by the data processor.
        /// </param>
        /// <returns>
        ///     A data processor.
        /// </returns>
        IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData);
    }
}

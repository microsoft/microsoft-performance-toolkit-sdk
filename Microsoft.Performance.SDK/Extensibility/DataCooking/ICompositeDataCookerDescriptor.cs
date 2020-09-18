// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     A data cooker that does not directly take part in data source processing
    ///     should implement this interface.
    /// </summary>
    public interface ICompositeDataCookerDescriptor
        : IDataCooker
    {
        /// <summary>
        ///     When all required data becomes available, this method will be called
        ///     so that the data cooker may retrieve data it requires.
        /// </summary>
        /// <param name="requiredData">
        ///     Provides access to data required by the cooker.
        /// </param>
        void OnDataAvailable(IDataExtensionRetrieval requiredData);
    }
}

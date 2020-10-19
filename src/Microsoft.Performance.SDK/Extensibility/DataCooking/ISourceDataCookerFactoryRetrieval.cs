// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     Adds a method to retrieve a source data cooker factory for a given data cooker path.
    /// </summary>
    public interface ISourceDataCookerFactoryRetrieval
    {
        /// <summary>
        ///     Returns a factory to create a source data cooker for the given data cooker path.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     The path to the data cooker for which to create a factory.
        /// </param>
        /// <returns>
        ///     A factory to create a data cooker instance.
        /// </returns>
        ISourceDataCookerFactory GetSourceDataCookerFactory(DataCookerPath dataCookerPath);
    }
}

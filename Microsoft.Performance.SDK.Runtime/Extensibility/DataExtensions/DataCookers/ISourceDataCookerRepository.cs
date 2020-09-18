// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     This interface provides access to a collection of data cookers.
    /// </summary>
    public interface ISourceDataCookerRepository
        : ISourceDataCookerFactoryRetrieval
    {
        /// <summary>
        ///     Gets the paths for all source data cookers in this repository.
        /// </summary>
        IEnumerable<DataCookerPath> SourceDataCookers { get; }

        /// <summary>
        ///     Returns a reference to a given data cooker.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     Path to a data cooker.
        /// </param>
        /// <returns>
        ///     A data cooker reference.
        /// </returns>
        ISourceDataCookerReference GetSourceDataCookerReference(DataCookerPath dataCookerPath);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     Exposes methods to find paths and references to composite data cookers from a data cooker repository.
    /// </summary>
    public interface ICompositeDataCookerRepository
    {
        /// <summary>
        ///     Gets the paths for all composite data cookers in this repository.
        /// </summary>
        IEnumerable<DataCookerPath> CompositeDataCookers { get; }

        /// <summary>
        ///     Returns a reference to a given data cooker.
        /// </summary>
        /// <param name="dataCookerPath">
        ///     Path to a data cooker.
        /// </param>
        /// <returns>
        ///     A composite data cooker reference.
        /// </returns>
        ICompositeDataCookerReference GetCompositeDataCookerReference(DataCookerPath dataCookerPath);
    }
}

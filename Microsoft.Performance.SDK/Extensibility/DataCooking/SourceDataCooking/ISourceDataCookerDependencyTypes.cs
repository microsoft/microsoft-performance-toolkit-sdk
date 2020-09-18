// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     This interface is used to define specific dependency behaviors for each of the data cookers required
    ///     by this data cooker.
    /// </summary>
    public interface ISourceDataCookerDependencyTypes
        : IDataCookerDependent,
          ISourceDataCookerDescriptor
    {
        /// <summary>
        ///     Gets the definition of the dependency type for each source data cooker required
        ///     by another source data cooker.
        /// </summary>
        IReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> DependencyTypes { get; }
    }
}

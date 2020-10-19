// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency
{
    /// <summary>
    ///     Describes the direct data extension requirements for a given data extension.
    /// </summary>
    public interface IDataExtensionDependencies
    {
        /// <summary>
        ///     Gets the source data cookers that the given data extension depends on.
        /// </summary>
        IReadOnlyCollection<DataCookerPath> RequiredSourceDataCookerPaths { get; }

        /// <summary>
        ///     Gets the composite data cookers that the given data extension depends on.
        /// </summary>
        IReadOnlyCollection<DataCookerPath> RequiredCompositeDataCookerPaths { get; }

        /// <summary>
        ///     Gets the data processors that the given data extension depends on.
        /// </summary>
        IReadOnlyCollection<DataProcessorId> RequiredDataProcessorIds { get; }
    }
}

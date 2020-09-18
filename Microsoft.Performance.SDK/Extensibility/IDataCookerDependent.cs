// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Should be applied to any data extension type that may require data cookers.
    /// </summary>
    public interface IDataCookerDependent
    {
        /// <summary>
        ///     Gets the required data cooker identifiers.
        /// </summary>
        IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; }
    }
}

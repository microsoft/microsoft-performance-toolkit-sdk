// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     This unifies the cooker data access for a given processing system.
    /// </summary>
    /// <remarks>
    ///     This must be implemented to return data for both Source Data Cookers as well as Composite Data Cookers.
    /// </remarks>
    public interface IProcessingSystemCookerData
        : ICompositeCookerRepository
    {
        ICookedDataRetrieval SourceCookerData { get; }
    }
}

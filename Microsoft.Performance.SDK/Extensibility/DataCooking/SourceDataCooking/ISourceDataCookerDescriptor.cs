// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     Information about a source data cooker that is not dependent on templates.
    /// </summary>
    public interface ISourceDataCookerDescriptor
    {
        /// <summary>
        ///     Gets the definition for how data is produced for this data cooker.
        /// </summary>
        DataProductionStrategy DataProductionStrategy { get; }
    }
}

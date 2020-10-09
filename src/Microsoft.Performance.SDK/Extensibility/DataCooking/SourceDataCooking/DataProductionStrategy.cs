// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking
{
    /// <summary>
    ///     This defines how data is cooked on a SourceDataCooker, indicating
    ///     when it is generally safe for consumption by other data cookers.
    /// </summary>
    public enum DataProductionStrategy
    {
        /// <summary>
        ///     Does not finish cooking until all records have been received.
        ///     Data is not ready for consumption until this point.
        /// </summary>
        PostSourceParsing,

        /// <summary>
        ///     Data will be ready for consumption when cooking the
        ///     individual record has finished.
        /// </summary>
        AsConsumed,

        /// <summary>
        ///     A source data cooker with this value will run in every stage
        ///     in which a dependent source data cooker runs.
        /// <para/>
        ///     This is particularly useful for streaming source data cookers,
        ///     where the data is not stored.
        /// </summary>
        AsRequired,
    }
}

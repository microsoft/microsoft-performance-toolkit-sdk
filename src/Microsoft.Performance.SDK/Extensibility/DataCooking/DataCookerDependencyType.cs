// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     The interaction between source data cookers determines the order in which source
    ///     data cookers receive source data to cook.
    /// <para/>
    ///     The default consumption strategy will be determined by the
    ///     <see cref="SourceDataCooking.DataProductionStrategy"/>
    ///     defined on the target SourceDataCooker.
    /// <para/>
    ///     Overriding this means the dependent knows internals of the target SourceDataCooker
    ///     and that it is safe to do so.
    /// </summary>
    public enum DataCookerDependencyType
    {
        /// <summary>
        ///     Default behavior: the dependency type will be determined by the data production strategy on
        ///     the target source data cooker.
        /// </summary>
        AlignedWithProductionStrategy,

        /// <summary>
        ///     This data cooker may receive source data during the same iteration as the target
        ///     data cooker, but only after the target data cooker has already had a chance to
        ///     cook the same data record.
        /// <para/>
        ///     <see cref="ISourceDataCooker{T, TContext, TKey}.EndDataCooking"/> will be called based on
        ///     this dependency ordering.
        /// </summary>
        AsConsumed,

        /// <summary>
        ///     This data cooker may receive source data anytime during the same parsing pass
        ///     as the target source data cooker.
        /// </summary>
        SamePass,
    }
}

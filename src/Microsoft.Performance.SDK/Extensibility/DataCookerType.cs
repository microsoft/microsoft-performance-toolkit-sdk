// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     The type of a <see cref="IDataCooker"/>.
    /// </summary>
    public enum DataCookerType
    {
        /// <summary>
        ///     An <see cref="IDataCooker"/> that directly receives input from an
        ///     <see cref="ISourceParser{T, TContext, TKey}"/> and/or one or more
        ///     <see cref="SourceDataCooker"/>s targeting the same <see cref="ISourceParser{T, TContext, TKey}"/>.
        /// </summary>
        SourceDataCooker,

        /// <summary>
        ///     An <see cref="IDataCooker"/> that consumes events from one or more
        ///     other <see cref="IDataCooker"/>.
        /// </summary>
        CompositeDataCooker,
    }
}
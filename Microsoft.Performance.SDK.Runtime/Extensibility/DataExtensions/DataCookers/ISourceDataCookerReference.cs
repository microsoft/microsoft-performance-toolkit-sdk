// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     A wrapper around a type that is a source data cooker.
    /// </summary>
    public interface ISourceDataCookerReference
        : ISourceDataCookerFactory,
          IDataCookerReference
    {
    }
}

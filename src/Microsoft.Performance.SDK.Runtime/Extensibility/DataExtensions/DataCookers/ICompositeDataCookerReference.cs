// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers
{
    /// <summary>
    ///     A wrapper around a type that is a composite data cooker.
    /// </summary>
    public interface ICompositeDataCookerReference
        : ICompositeDataCookerRetrieval,
          IDataCookerReference
    {
    }
}

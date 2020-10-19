// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     A data cooker.
    /// </summary>
    public interface IDataCooker
        : IDataCookerDescriptor,
          ICookedDataSet,
          IDataCookerDependent
    {
    }
}

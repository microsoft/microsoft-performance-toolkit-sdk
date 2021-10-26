// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     This interface is used to query cooked data or data processors.
    /// </summary>
    public interface IDataExtensionRetrieval
        : ICookedDataRetrieval //,
          // TODO: __SDK_DP__
          // IDataProcessorRetrieval
    {
    }
}

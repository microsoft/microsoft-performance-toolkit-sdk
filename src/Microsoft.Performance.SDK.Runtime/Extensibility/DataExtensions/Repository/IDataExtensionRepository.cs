// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataCookers;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository
{
    /// <summary>
    ///     Provides access to a set of data extensions.
    /// </summary>
    public interface IDataExtensionRepository
        : ISourceDataCookerRepository,
          ICompositeDataCookerRepository,
          IDataTableRepository,
          // TODO: __SDK_DP__
          // IDataProcessorRepository,
          IDisposable
    {
    }
}

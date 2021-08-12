// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions
{
    /// <summary>
    ///     Provides access to extensions data associated with either a single or a grouped set of 
    ///     <see cref="ICustomDataProcessor"/>s.
    /// </summary>
    public interface IProcessingSystemData
        : ICookedDataRetrieval,
          ICompositeCookerRetrieval,
          IMapTablesToProcessors,
          IDataExtensionRetrievalFactory,
          IDisposable
    {
    }
}

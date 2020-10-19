// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Extensibility.DataCooking
{
    /// <summary>
    ///     Implement this interface to expose output from the data cooker.
    /// </summary>
    public interface ICookedDataSet
        : ICookedDataRetrieval
    {
        /// <summary>
        ///     Gets each piece of data output from the extension has a unique name associated with it.
        /// </summary>
        IReadOnlyCollection<DataOutputPath> OutputIdentifiers { get; }
    }
}

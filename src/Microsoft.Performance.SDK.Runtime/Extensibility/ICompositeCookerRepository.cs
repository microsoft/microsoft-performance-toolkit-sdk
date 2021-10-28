// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     Provides access to <see cref="ICookedDataRetrieval"/> for composite cookers.
    /// </summary>
    public interface ICompositeCookerRepository
        : IDisposable
    {
        /// <summary>
        ///     Gets data output from a composite cooker.
        /// </summary>
        /// <remarks>
        ///     The composite cooker might need to be created and processed in order to return the result.
        /// </remarks>
        /// <param name="cookerPath">
        ///     Uniquely identifies a composite data cooker.
        /// </param>
        /// <returns>
        ///     The data output of the identified composite cooker.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="cookerPath"/> is not available.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        ICookedDataRetrieval GetCookerOutput(DataCookerPath cookerPath);
    }
}

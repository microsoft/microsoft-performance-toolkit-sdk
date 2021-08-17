// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Extensibility
{
    /// <summary>
    ///     This class holds references to composite cookerse. It offers basic functionality to retrieve a cooker's
    ///     <see cref="ICookedDataRetrieval"/>, possibly creating new cooker instances to do so.
    /// </summary>
    public interface ICompositeCookerRepository
        : IDisposable
    {
        /// <summary>
        ///     Gets or creates a composite <see cref="IDataCooker"/>.
        /// </summary>
        /// <param name="cookerPath">
        ///     Uniquely identifies a composite data cooker.
        /// </param>
        /// <returns>
        ///     An instance of a composite data cooker.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     <paramref name="cookerPath"/> is not available.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="createDataRetrieval"/> is <c>null</c> when creating a composite data cooker.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     This instance is disposed.
        /// </exception>
        ICookedDataRetrieval GetOrCreateCompositeCooker(DataCookerPath cookerPath);
    }
}

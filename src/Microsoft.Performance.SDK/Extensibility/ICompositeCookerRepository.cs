// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Extensibility
{
    /// <summary>
    ///     Provides access to composite <see cref="ICookedDataRetrieval"/>s, creating them when necessary.
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
        /// <param name="createDataRetrieval">
        ///     Function to create an <see cref="IDataExtensionRetrieval"/> for the composite cooker.
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
        ICookedDataRetrieval GetOrCreateCompositeCooker(
            DataCookerPath cookerPath,
            Func<DataCookerPath, IDataExtensionRetrieval> createDataRetrieval);
    }
}

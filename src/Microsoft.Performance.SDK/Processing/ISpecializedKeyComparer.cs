// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    /// Adds a method to retrieve a specialized key comparer.
    /// </summary>
    /// <typeparam name="TKey">The type to be compared</typeparam>
    public interface ISpecializedKeyComparer<TKey>
    {
        /// <summary>
        /// Provides a set of modes that may provide specialized comparisons.
        /// </summary>
        IReadOnlyList<string> ProjectionModes { get; }

        /// <summary>
        /// Returns an <see cref="IComparer{T}"/> type based on the mode passed in. Return <c>null</c> if no
        /// specialized comparer exists for the given mode.
        /// </summary>
        /// <param name="mode">Used to determine the return value</param>
        /// <returns>A specialized comparer, or <c>null</c> to use a default comparer.</returns>
        IComparer<TKey> GetSpecializedKeyComparer(string mode);
    }
}

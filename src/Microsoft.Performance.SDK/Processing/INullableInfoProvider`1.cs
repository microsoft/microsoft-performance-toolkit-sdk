// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Encapsulates null checks against instances of a
    ///     <see cref="System.Type"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="System.Type"/> of values.
    /// </typeparam>
    public interface INullableInfoProvider<T>
    {
        /// <summary>
        ///     Determines whether the given instance is
        ///     considered to be <c>null</c>.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value"/> is considered
        ///     to be <c>null</c>; false otherwise.
        /// </returns>
        bool IsNull(T value);
    }
}

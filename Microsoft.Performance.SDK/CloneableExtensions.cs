// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides static (Shared in Visual Basic) methods for
    ///     interacting with <see cref="ICloneable{T}"/> instances.
    /// </summary>
    public static class CloneableExtensions
    {
        /// <summary>
        ///     Performs a typed clone of the given instance.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="System.Type"/> of instance returned by the clone
        ///     method.
        /// </typeparam>
        /// <param name="self">
        ///     The instance being cloned.
        /// </param>
        /// <returns>
        ///     A clone of <paramref name="self"/> of type <typeparamref name="T"/>.
        /// </returns>
        public static T Clone<T>(this ICloneable<T> self)
        {
            return self.CloneT();
        }
    }
}

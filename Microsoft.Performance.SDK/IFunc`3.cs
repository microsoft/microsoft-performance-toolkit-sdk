// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a function taking two parameters
    ///     and returning a result.
    /// </summary>
    /// <typeparam name="T1">
    ///     The first parameter <see cref="System.Type"/>.
    /// </typeparam>
    /// <typeparam name="T2">
    ///    The second parameter <see cref="System.Type"/>.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The <see cref="System.Type"/> of the return value.
    /// </typeparam>
    public interface IFunc<in T1, in T2, out TResult>
    {
        /// <summary>
        ///     Invokes the given function.
        /// </summary>
        /// <param name="t1">
        ///     The first argument.
        /// </param>
        /// <param name="t2">
        ///     The second argument.
        /// </param>
        /// <returns>
        ///     The result.
        /// </returns>
        TResult Invoke(T1 t1, T2 t2);
    }
}

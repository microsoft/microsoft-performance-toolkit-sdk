// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a function taking one parameter
    ///     and returning a result.
    /// </summary>
    /// <typeparam name="T">
    ///     The parameter <see cref="System.Type"/>.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The <see cref="System.Type"/> of the return value.
    /// </typeparam>
    public interface IFunc<in T, out TResult>
    {
        /// <summary>
        ///     Invokes the function.
        /// </summary>
        /// <param name="t1">
        ///     The first argument.
        /// </param>
        /// <returns>
        ///     The result.
        /// </returns>
        TResult Invoke(T t1);
    }
}

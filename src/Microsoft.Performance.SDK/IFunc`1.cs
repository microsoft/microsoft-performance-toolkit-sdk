// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Represents a function taking no parameters
    ///     and returning a result.
    /// </summary>
    /// <typeparam name="TResult">
    ///     The <see cref="System.Type"/> of the return value.
    /// </typeparam>
    public interface IFunc<out TResult>
    {
        /// <summary>
        ///     Invokes this function.
        /// </summary>
        /// <returns>
        ///     The return value.
        /// </returns>
        TResult Invoke();
    }
}

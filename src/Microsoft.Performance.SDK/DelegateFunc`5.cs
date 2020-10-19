// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Wraps a <see cref="Func{T1, T2, T3, T4, TResult}"/> into an
    ///     <see cref="IFunc{T1, T2, T3, T4, TResult}"/>.
    /// </summary>
    /// <typeparam name="T1">
    ///     The <see cref="Type"/> of the first parameter.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The <see cref="Type"/> of the second parameter.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The <see cref="Type"/> of the third parameter.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The <see cref="Type"/> of the fourth parameter.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The return <see cref="Type"/>.
    /// </typeparam>
    public struct DelegateFunc<T1, T2, T3, T4, TResult>
        : IFunc<T1, T2, T3, T4, TResult>
    {
        private readonly Func<T1, T2, T3, T4, TResult> func;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFunc{T1, T2, T3, T4, TResult}"/>
        ///     class.
        /// </summary>
        /// <param name="func">
        ///     The <see cref="Func{T1, T2, T3, T4, TResult}"/> to wrap.
        /// </param>
        public DelegateFunc(Func<T1, T2, T3, T4, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            this.func = func;
        }

        /// <inheritdoc />
        public TResult Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return this.func(arg1, arg2, arg3, arg4);
        }
    }
}

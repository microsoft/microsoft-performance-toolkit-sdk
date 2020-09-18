// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Wraps a <see cref="Func{T, TResult}"/> into an <see cref="IFunc{T, TResult}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of the parameter.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The return <see cref="Type"/>.
    /// </typeparam>
    public struct DelegateFunc<T, TResult>
        : IFunc<T, TResult>
    {
        private readonly Func<T, TResult> func;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFunc{T, TResult}"/>
        ///     class.
        /// </summary>
        /// <param name="func">
        ///     The <see cref="Func{T, TResult}"/> to wrap.
        /// </param>
        public DelegateFunc(Func<T, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            this.func = func;
        }

        /// <inheritdoc />
        public TResult Invoke(T arg)
        {
            return this.func(arg);
        }
    }
}

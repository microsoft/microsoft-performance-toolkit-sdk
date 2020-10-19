// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{   
     /// <summary>
     ///     Wraps a <see cref="Func{T1, T2, TResult}"/> into an
     ///     <see cref="IFunc{T1, T2, TResult}"/>.
     /// </summary>
     /// <typeparam name="T1">
     ///     The <see cref="Type"/> of the first parameter.
     /// </typeparam>
     /// <typeparam name="T2">
     ///     The <see cref="Type"/> of the second parameter.
     /// </typeparam>
     /// <typeparam name="TResult">
     ///     The return <see cref="Type"/>.
     /// </typeparam>
    public struct DelegateFunc<T1, T2, TResult>
        : IFunc<T1, T2, TResult>
    {
        private readonly Func<T1, T2, TResult> func;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFunc{T1, T2, T3, TResult}"/>
        ///     class.
        /// </summary>
        /// <param name="func">
        ///     The <see cref="Func{T1, T2, T3, TResult}"/> to wrap.
        /// </param>
        public DelegateFunc(Func<T1, T2, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            this.func = func;
        }

        /// <inheritdoc />
        public TResult Invoke(T1 arg1, T2 arg2)
        {
            return this.func(arg1, arg2);
        }
    }
}

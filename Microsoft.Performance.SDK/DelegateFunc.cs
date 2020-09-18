// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides a means of decorating <see cref="Func{T, TResult}"/>s with the IFunc interface.
    /// </summary>
    public static class DelegateFunc
    {
        /// <summary>
        ///     Converts the given <see cref="Func{T, TResult}"/> into a <see cref="DelegateFunc{T,TResult}"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of the parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="Type"/>of the result of <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">
        ///     The <see cref="Func{T, TResult}"/> to convert.
        /// </param>
        /// <returns>
        ///     A <see cref="DelegateFunc{T, TResult}"/> wrapper for <paramref name="func"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="func"/> is <c>null</c>.
        /// </exception>
        public static DelegateFunc<T, TResult> From<T, TResult>(Func<T, TResult> func)
        {
            return new DelegateFunc<T, TResult>(func);
        }

        /// <summary>
        ///     Converts the given <see cref="Func{T1, T2, TResult}"/> into a <see cref="DelegateFunc{T1, T2, TResult}"/>.
        /// </summary>
        /// <typeparam name="T1">
        ///     The <see cref="Type"/> of the first parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The <see cref="Type"/> of the second parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="Type"/>of the result of <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">
        ///     The <see cref="Func{T1, T2, TResult}"/> to convert.
        /// </param>
        /// <returns>
        ///     A <see cref="DelegateFunc{T1, T2, TResult}"/> wrapper for <paramref name="func"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="func"/> is <c>null</c>.
        /// </exception>
        public static DelegateFunc<T1, T2, TResult> From<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return new DelegateFunc<T1, T2, TResult>(func);
        }

        /// <summary>
        ///     Converts the given <see cref="Func{T1, T2, T3, TResult}"/> into a <see cref="DelegateFunc{T1, T2, T3, TResult}"/>.
        /// </summary>
        /// <typeparam name="T1">
        ///     The <see cref="Type"/> of the first parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The <see cref="Type"/> of the second parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The <see cref="Type"/> of the third parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="Type"/>of the result of <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">
        ///     The <see cref="Func{T1, T2, T3, TResult}"/> to convert.
        /// </param>
        /// <returns>
        ///     A <see cref="DelegateFunc{T1, T2, T3, TResult}"/> wrapper for <paramref name="func"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="func"/> is <c>null</c>.
        /// </exception>
        public static DelegateFunc<T1, T2, T3, TResult> From<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            return new DelegateFunc<T1, T2, T3, TResult>(func);
        }

        /// <summary>
        ///     Converts the given <see cref="Func{T1, T2, T3, T4, TResult}"/> into a <see cref="DelegateFunc{T1, T2, T3, T4, TResult}"/>.
        /// </summary>
        /// <typeparam name="T1">
        ///     The <see cref="Type"/> of the first parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T2">
        ///     The <see cref="Type"/> of the second parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T3">
        ///     The <see cref="Type"/> of the third parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="T4">
        ///     The <see cref="Type"/> of the fourth parameter to <paramref name="func"/>.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The <see cref="Type"/>of the result of <paramref name="func"/>.
        /// </typeparam>
        /// <param name="func">
        ///     The <see cref="DelegateFunc{T1, T2, T3, T4, TResult}"/> to convert.
        /// </param>
        /// <returns>
        ///     A <see cref="DelegateFunc{T1, T2, T3, T4, TResult}"/> wrapper for <paramref name="func"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="func"/> is <c>null</c>.
        /// </exception>
        public static DelegateFunc<T1, T2, T3, T4, TResult> From<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            return new DelegateFunc<T1, T2, T3, T4, TResult>(func);
        }
    }
}

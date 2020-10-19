// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Encapsulates a method that has four parameters and returns a value of the <see cref="Type"/>
    ///     specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">
    ///     The <see cref="Type"/> of the first parameter of the method that this delegate encapsulates.
    /// </typeparam>
    /// <typeparam name="T2">
    ///     The <see cref="Type"/> of the second parameter of the method that this delegate encapsulates.
    /// </typeparam>
    /// <typeparam name="T3">
    ///     The <see cref="Type"/> of the third parameter of the method that this delegate encapsulates.
    /// </typeparam>
    /// <typeparam name="T4">
    ///     The <see cref="Type"/> of the fourth parameter of the method that this delegate encapsulates.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The <see cref="Type"/> of the return value of the method that this delegate encapsulates.
    /// </typeparam>
    /// <param name="arg1">
    ///     The first parameter of the method that this delegate encapsulates.
    /// </param>
    /// <param name="arg2">
    ///     The second parameter of the method that this delegate encapsulates.
    /// </param>
    /// <param name="arg3">
    ///     The third parameter of the method that this delegate encapsulates.
    /// </param>
    /// <param name="arg4">
    ///     The fourth parameter of the method that this delegate encapsulates.
    /// </param>
    /// <returns>
    ///     The return value of the method that this delegate encapsulates.
    /// </returns>
    public delegate TResult StaticFunc<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}

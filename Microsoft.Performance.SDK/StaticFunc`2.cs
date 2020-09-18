// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Encapsulates a method that has one parameter and returns a value of the <see cref="Type"/>
    ///     specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of the parameter of the method that this delegate encapsulates.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The <see cref="Type"/> of the return value of the method that this delegate encapsulates.
    /// </typeparam>
    /// <param name="arg">
    ///     The parameter of the method that this delegate encapsulates.
    /// </param>
    /// <returns>
    ///     The return value of the method that this delegate encapsulates.
    /// </returns>
    public delegate TResult StaticFunc<in T, out TResult>(T arg);
}

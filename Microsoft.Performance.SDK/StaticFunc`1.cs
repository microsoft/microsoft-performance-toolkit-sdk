// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Encapsulates a method that has no parameters and returns a value of the <see cref="Type"/>
    ///     specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="TResult">
    ///     The <see cref="Type"/> of the return value of the method that this delegate encapsulates.
    /// </typeparam>
    /// <returns>
    ///     The return value of the method that this delegate encapsulates.
    /// </returns>
    public delegate TResult StaticFunc<out TResult>();
}

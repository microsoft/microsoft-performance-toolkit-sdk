// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Auth;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Extends <see cref="IApplicationEnvironment"/> to provide additional functionality.
    /// </summary>
    [Obsolete("This interface will be removed in version 2.0 of the SDK. It is OK to use this interface in version 1.x of the SDK.")]
    public interface IApplicationEnvironmentV2
        : IApplicationEnvironment
    {
        /// <summary>
        ///     Attempts to get an <see cref="IAuthProvider{TAuth, TResult}"/> that can provide authentication
        ///     for <see cref="IAuthMethod{TResult}"/> of type <typeparamref name="TAuth"/>.
        /// </summary>
        /// <param name="provider">
        ///     The found provider, or <c>null</c> if no registered provider can provide authentication for
        ///     <typeparamref name="TAuth"/>.
        /// </param>
        /// <typeparam name="TAuth">
        ///     The type of the <see cref="IAuthMethod{TResult}"/> for which to attempt to get a provider.
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the result of a successful authentication for <typeparamref name="TAuth"/>.
        /// </typeparam>
        /// <returns>
        ///     <c>true</c> if a provider was found; <c>false</c> otherwise. If <c>false</c> is returned,
        ///     <paramref name="provider"/> will be <c>null</c>.
        /// </returns>
        bool TryGetAuthProvider<TAuth, TResult>(out IAuthProvider<TAuth, TResult> provider)
            where TAuth : IAuthMethod<TResult>;
    }
}
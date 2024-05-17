// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Performance.SDK.Auth
{
    /// <summary>
    ///     Provides authentication for a given <see cref="IAuthMethod{TResult}"/>.
    /// </summary>
    /// <typeparam name="TAuth">
    ///     The type of the <see cref="IAuthMethod{TResult}"/> this provider can authenticate.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The type of the result of a successful authentication.
    /// </typeparam>
    public interface IAuthProvider<in TAuth, TResult>
        where TAuth : IAuthMethod<TResult>
    {
        /// <summary>
        ///     Attempts to authenticate the given <see cref="IAuthMethod{TResult}"/>.
        /// </summary>
        /// <param name="authRequest">
        ///     The <see cref="IAuthMethod{TResult}"/> to authenticate.
        /// </param>
        /// <returns>
        ///     An awaitable task that, upon completion, will contain the result of the authentication request.
        ///     The result of the authentication will either be an instance of <typeparamref name="TResult"/> if
        ///     the authentication was successful, or <c>null</c> if the authentication failed.
        /// </returns>
        Task<TResult> TryGetAuth(TAuth authRequest);
    }
}
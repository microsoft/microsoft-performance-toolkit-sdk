// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Auth
{
    /// <summary>
    ///     Represents an authentication method that can be used to authenticate to a service.
    /// </summary>
    /// <typeparam name="TResult">
    ///     The type of the result of a successful authentication.
    /// </typeparam>
    public interface IAuthMethod<TResult>
    {
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Auth
{
    /// <summary>
    ///     An <see cref="IAuthMethod{TResult}"/> that acquires bearer tokens for accessing Azure services.
    /// </summary>
    public sealed class AzureBearerTokenAuth
        : IAuthMethod<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureBearerTokenAuth"/> class.
        /// </summary>
        /// <param name="appId">
        ///     The <see cref="Guid"/> of the Microsoft Entra ID (formerly Azure Active Directory) of the
        ///     application to authenticate to.
        /// </param>
        /// <param name="scopes">
        ///     The required scopes that the returned bearer token must have.
        /// </param>
        /// <param name="allowCache">
        ///     A value indicating whether or not to allow the acquired token to come from a cache. This value may
        ///     be <c>false</c> if a token for these scopes was previously acquired, but the token does not have access
        ///     to the required resources. In this case, a different token (e.g. a token for a different user) may be
        ///     required.
        /// </param>
        /// <param name="appName">
        ///     A human-readable display name for the application the user is logging into.
        /// </param>
        /// <param name="authReason">
        ///     An optional human-readable description to describe why the authentication is needed. For example,
        ///     "retrieve data from Azure DevOps".
        /// </param>
        /// <param name="tenant">
        ///     The optional tenant the authenticated principal must be logged into. May be <c>null</c>.
        /// </param>
        /// <param name="preferredAccountDomain">
        ///     The optional domain name of accounts which should be preferred for this authentication request.
        ///     May be <c>null</c>.
        /// </param>
        public AzureBearerTokenAuth(
            Guid appId,
            string[] scopes,
            bool allowCache,
            string appName,
            string authReason,
            Guid? tenant,
            string preferredAccountDomain)
        {
            AppId = appId;
            Scopes = scopes;
            AllowCache = allowCache;
            AppName = appName;
            AuthReason = authReason;
            Tenant = tenant;
            PreferredAccountDomain = preferredAccountDomain;
        }

        /// <summary>
        ///     Gets the <see cref="Guid"/> of the Microsoft Entra ID (formerly Azure Active Directory) of the
        ///     application to authenticate to.
        /// </summary>
        public Guid AppId { get; }

        /// <summary>
        ///     Gets the required scopes that the returned bearer token must have.
        /// </summary>
        public string[] Scopes { get; }

        /// <summary>
        ///     Gets a value indicating whether or not to allow the acquired token to come from a cache. This value may
        ///     be <c>false</c> if a token for these scopes was previously acquired, but the token does not have access
        ///     to the required resources. In this case, a different token (e.g. a token for a different user) may be
        ///     required.
        /// </summary>
        public bool AllowCache { get; }

        /// <summary>
        ///     Gets a human-readable display name for the application the user is logging into.
        /// </summary>
        public string AppName { get; }

        /// <summary>
        ///     Gets an optional human-readable description to describe why the authentication is needed. For example,
        ///     "retrieve data from Azure DevOps".
        /// </summary>
        public string AuthReason { get; }

        /// <summary>
        ///     Gets the optional tenant the authenticated principal must be logged into. May be <c>null</c>.
        /// </summary>
        public Guid? Tenant { get; }

        /// <summary>
        ///     Gets the optional domain name of accounts which should be preferred for this authentication request.
        ///     May be <c>null</c>.
        /// </summary>
        public string PreferredAccountDomain { get; }
    }
}
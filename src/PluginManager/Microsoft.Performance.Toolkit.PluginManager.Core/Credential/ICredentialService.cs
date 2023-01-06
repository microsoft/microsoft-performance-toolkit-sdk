// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using System;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public interface ICredentialService
    {
        Task<ICredentials> GetCredentialsAsync(Uri uri, CancellationToken cancellationToken);
    }
}

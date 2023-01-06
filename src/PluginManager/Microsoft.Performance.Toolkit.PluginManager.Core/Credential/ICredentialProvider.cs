// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System;
using System.Threading;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    public interface ICredentialProvider
    {
        Task<ICredentials> GetAsync(Uri uri, CancellationToken cancellationToken);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Credential
{
    // TODO: #235 Authentication
    public interface ICredentialProvider
    {
        Task<ICredentials> GetAsync(PluginSource source, CancellationToken cancellationToken);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using System.Threading;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Credential
{
    public interface ICredentialProvider
    {
        Task<ICredentials> GetAsync(PluginSource source, CancellationToken cancellationToken);
    }
}

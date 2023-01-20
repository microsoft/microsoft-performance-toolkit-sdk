// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Credential;
using Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.NuGet
{
    public class NuGetCredentialProvider : ICredentialProvider
    {
        // TODO: #235 Authentication
        public async Task<ICredentials> GetAsync(PluginSource pluginSource, CancellationToken cancellationToken)
        {
            return new NuGetCredentials();
        }
    }
}

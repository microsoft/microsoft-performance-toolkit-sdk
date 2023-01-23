// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.NuGet
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

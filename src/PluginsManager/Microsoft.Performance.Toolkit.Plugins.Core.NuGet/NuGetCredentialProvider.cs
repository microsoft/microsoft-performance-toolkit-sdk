// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.Plugins.Core.Credential;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Core.NuGet
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

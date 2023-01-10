// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public class NuGetCredentialProvider : ICredentialProvider<UriPluginSource>
    {
        public Task<ICredentials> GetAsync(UriPluginSource pluginSource, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

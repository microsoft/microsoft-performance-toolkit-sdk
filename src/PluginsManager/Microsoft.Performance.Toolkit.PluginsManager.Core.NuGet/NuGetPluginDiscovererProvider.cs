// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Discovery;
using Microsoft.Performance.Toolkit.PluginsManager.Core.Extensibility;
using NuGet.Configuration;

namespace Microsoft.Performance.Toolkit.PluginsManager.Core.NuGet
{
    [PluginManagerResource(PluginManagerConstants.NuGetDiscovererProviderId)]
    public class NuGetPluginDiscovererProvider 
        : IPluginDiscovererProvider
    {
        private Lazy<ICredentialProvider> credentialProvider =
            new Lazy<ICredentialProvider>(() => new NuGetCredentialProvider());

        public Lazy<ICredentialProvider> CredentialProvider
        {
            get
            {
                return this.credentialProvider;
            }
        }

        public IPluginDiscoverer CreateDiscoverer(PluginSource source)
        {
            return new NuGetPluginDiscoverer(source, this.CredentialProvider);
        }

        public async Task<bool> IsSupportedAsync(PluginSource source)
        {
            if (source == null)
            {
                return false;
            }

            var nugetSource = new PackageSource(source.Uri.ToString());

            // Support http V3 and local feed as of of now
            bool isSupported = IsHttpV3Feed(nugetSource) || nugetSource.IsLocal;

            return isSupported;
        }

        private static bool IsHttpV3Feed(PackageSource packageSource)
        {
            return packageSource.IsHttp &&
              (packageSource.Source.EndsWith("index.json", StringComparison.OrdinalIgnoreCase)
              || packageSource.ProtocolVersion == 3);
        }
    }
}

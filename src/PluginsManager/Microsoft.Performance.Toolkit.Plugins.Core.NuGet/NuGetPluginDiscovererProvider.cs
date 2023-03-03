// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Credential;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using NuGet.Configuration;

namespace Microsoft.Performance.Toolkit.Plugins.Core.NuGet
{
    [PluginsManagerResource(PluginsManagerConstants.NuGetDiscovererProviderId)]
    public class NuGetPluginDiscovererProvider 
        : IPluginDiscovererProvider
    {
        private ILogger logger;
        private Lazy<ICredentialProvider> credentialProvider =
            new Lazy<ICredentialProvider>(() => new NuGetCredentialProvider());

        public Lazy<ICredentialProvider> CredentialProvider
        {
            get
            {
                return this.credentialProvider;
            }
        }

        public IPluginDiscoverer CreateDiscoverer(PluginSource pluginSource)
        {
            return new NuGetPluginDiscoverer(pluginSource, this.CredentialProvider);
        }

        public async Task<bool> IsSupportedAsync(PluginSource pluginSource)
        {
            if (pluginSource == null)
            {
                return false;
            }

            var nugetSource = new PackageSource(pluginSource.Uri.ToString());

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

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }
    }
}

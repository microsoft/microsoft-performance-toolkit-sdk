// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.Toolkit.PluginManager.Core.Credential;
using Microsoft.Performance.Toolkit.PluginManager.Core.Discovery;
using NuGet.Configuration;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.NuGet
{
    public class NuGetPluginDiscovererSource : PluginDiscovererSource<UriPluginSource>
    {
        private CredentialService credentialService;

        public override IPluginDiscoverer CreateDiscoverer(UriPluginSource source)
        {
            return new NuGetPluginDiscoverer(source, this.credentialService);
        }

        public override bool IsSourceSupported(UriPluginSource source)
        {
            if (source == null)
            {
                return false;
            }

            var nugetSource = new PackageSource(source.Uri.ToString());

            // Support http V3 and local feed as of of now
            return IsHttpV3Feed(nugetSource) || nugetSource.IsLocal;
        }

        public override void SetupCredentialService(IEnumerable<ICredentialProvider> credentialProviders)
        {
            this.credentialService = new CredentialService(credentialProviders);
        }

        private static bool IsHttpV3Feed(PackageSource packageSource)
        {
            return packageSource.IsHttp &&
              (packageSource.Source.EndsWith("index.json", StringComparison.OrdinalIgnoreCase)
              || packageSource.ProtocolVersion == 3);
        }
    }
}

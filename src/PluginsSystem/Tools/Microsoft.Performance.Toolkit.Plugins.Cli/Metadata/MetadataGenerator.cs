// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Validation;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Metadata
{
    public sealed class MetadataGenerator
        : IMetadataGenerator
    {
        private readonly ILogger<MetadataGenerator> logger;

        public MetadataGenerator(ILogger<MetadataGenerator> logger)
        {
            this.logger = logger;
        }

        public PluginMetadata Generate(ProcessedPluginDirectory pluginDirectory, PluginManifest manifest)
        {
            PluginIdentity identity = new(manifest.Identity.Id, manifest.Identity.Version);
            ulong installedSize = (ulong)pluginDirectory.PluginSize;
            IEnumerable<PluginOwnerInfo> owners = manifest.Owners.Select(
                o => new PluginOwnerInfo(o.Name, o.Address, o.EmailAddresses.ToArray(), o.PhoneNumbers.ToArray()));
            Version sdkVersion = pluginDirectory.SdkVersion;

            PluginMetadata metadata = new(
                identity,
                installedSize,
                manifest.DisplayName,
                manifest.Description,
                sdkVersion,
                manifest.ProjectUrl,owners);

            return metadata;
        }
    }
}

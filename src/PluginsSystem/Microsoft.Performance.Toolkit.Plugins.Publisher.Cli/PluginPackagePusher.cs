// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    public sealed class PluginPackagePusher
        : IPluginPackagePusher
    {
        private readonly IPluginPackageReader pluginPackageReader;
        private readonly ILogger logger;

        public PluginPackagePusher(
            IPluginPackageReader pluginPackageReader,
            Func<Type, ILogger> loggerFactory)
        {
            this.pluginPackageReader = pluginPackageReader;
            this.logger = loggerFactory(typeof(PluginPackagePusher));
        }

        public async Task<bool> PushPackage(
            PluginSource source,
            Stream pluginPackageStream,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(pluginPackageStream, nameof(pluginPackageStream));

            PluginPackage package = await this.pluginPackageReader.TryReadPackageAsync(pluginPackageStream, cancellationToken);
            if (package == null)
            {
                this.logger.Error($"Failed to read plugin package from stream.");
                return false;
            }

            // TODO: Validate package

            // TODO: Push package to source

            // TODO: Create AvailablePluginInfo and add to PluginSource

            return false;
        }
    }
}

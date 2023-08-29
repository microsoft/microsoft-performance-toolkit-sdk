using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands.Common
{
    internal abstract class PackGenCommonCommand<TArgs>
        : ICommand<TArgs>
        where TArgs : PackGenCommonArgs
    {
        protected readonly IManifestLocatorFactory manifestLocatorFactory;
        protected readonly IPluginArtifactsProcessor artifactsProcessor;
        protected readonly ILogger<PackGenCommonCommand<TArgs>> logger;

        protected PackGenCommonCommand(
            IManifestLocatorFactory manifestLocatorFactory,
            IPluginArtifactsProcessor artifactsProcessor,
            ILogger<PackGenCommonCommand<TArgs>> logger)
        {
            this.manifestLocatorFactory = manifestLocatorFactory;
            this.artifactsProcessor = artifactsProcessor;
            this.logger = logger;
        }

        public int Run(TArgs args)
        {
            if (!TryGetProcessedPluginResult(args, out ProcessedPluginResult? processedSource))
            {
                return 1;
            }

            return RunCore(args, processedSource!);
        }

        protected abstract int RunCore(TArgs args, ProcessedPluginResult processedSource);

        protected bool TryGetProcessedPluginResult(PackGenCommonArgs args, out ProcessedPluginResult? processedPluginResult)
        {
            processedPluginResult = null;
            IManifestLocator manifestLocator = this.manifestLocatorFactory.Create(args);
            if (!manifestLocator.TryLocate(out string? manifestFilePath))
            {
                this.logger.LogError("Failed to locate manifest file.");
                return false;
            }

            var artifacts = new PluginArtifacts(args.SourceDirectoryFullPath, manifestFilePath!);
            if (!this.artifactsProcessor.TryProcess(artifacts, out processedPluginResult))
            {
                this.logger.LogError("Failed to process plugin artifacts.");
                return false;
            }

            return true;
        }
    }
}

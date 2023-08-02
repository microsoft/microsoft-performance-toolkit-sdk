// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    internal class PluginSourceFilesValidator
        : IPluginSourceFilesValidator
    {
        private readonly ILogger logger;

        public PluginSourceFilesValidator(Func<Type, ILogger> loggerFactory)
        {
            this.logger = loggerFactory(typeof(PluginSourceFilesValidator));
        }

        public bool Validate(string pluginSourceDir, bool manifestIncluded)
        {
            Guard.NotNull(pluginSourceDir, nameof(pluginSourceDir));

            if (!Directory.Exists(pluginSourceDir))
            {
                this.logger.Error($"Plugin source directory does not exist: {pluginSourceDir}");
                return false;
            }

            if (!Directory.EnumerateFiles(pluginSourceDir, "*.dll", SearchOption.AllDirectories).Any())
            {
                this.logger.Error($"Plugin source directory does not contain any DLLs: {pluginSourceDir}");
                return false;
            }

            if (manifestIncluded)
            {
                if (!File.Exists(Path.Combine(pluginSourceDir, Constants.BundledManifestName)))
                {
                    this.logger.Error($"Plugin source directory does not contain an bundled manifest: {Constants.BundledManifestName}");
                    return false;
                }
            }

            // SDK should not be present in the plugin source directory
            // All dlls should depend on a single SDK version

            // TODO: Add more validation here
            // e.g. Ensure only one SDK assembly is present?
            return true;
        }
    }
}

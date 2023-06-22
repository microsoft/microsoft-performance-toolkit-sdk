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

        public bool Validate(string pluginSourceDir)
        {
            Guard.NotNull(pluginSourceDir, nameof(pluginSourceDir));

            if (!Directory.Exists(pluginSourceDir))
            {
                this.logger.Error($"Plugin source directory does not exist: {pluginSourceDir}");
                return false;
            }

            if (!Directory.EnumerateFiles(pluginSourceDir, "*.dll").Any())
            {
                this.logger.Error($"Plugin source directory does not contain any DLLs: {pluginSourceDir}");
                return false;
            }

            return true;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Validation
{
    /// <summary>
    ///     Scans the source directory for plugin files and processes them by running validation and other checks.
    /// </summary>
    public interface ISourceFilesProcessor
    {
        ProcessedPluginDirectory Process(string pluginSourceDir, bool manifestEmbedded);
    }
}

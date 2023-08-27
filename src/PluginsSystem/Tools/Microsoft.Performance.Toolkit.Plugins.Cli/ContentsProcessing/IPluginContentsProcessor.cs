// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.ContentsProcessing
{
    internal interface IPluginContentsProcessor
    {
        public ProcessedPluginContents Process(PackGenCommonArgs options);
    }
}

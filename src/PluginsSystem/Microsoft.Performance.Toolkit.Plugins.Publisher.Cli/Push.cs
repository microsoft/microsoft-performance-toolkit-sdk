// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    [DisplayName("push")]
    [Description("Push the plugin package to a plugin source")]
    public sealed class Push : Command
    {
        [DisplayName("pluginSourceFeed")]
        [Description("The source feed to push the plugin package to")]
        [PositionalArgument(0)]
        [ExpandPath]
        public string PluginSourceFeed { get; set; }

        [DisplayName("pluginFilePath")]
        [Description("The file path of the .plugin package")]
        [PositionalArgument(1)]
        [ExpandPath]
        public string PluginFilePath { get; set; }


        public override Task<int> RunAsync(CancellationToken cancellationToken)
        {
        }
    }
}

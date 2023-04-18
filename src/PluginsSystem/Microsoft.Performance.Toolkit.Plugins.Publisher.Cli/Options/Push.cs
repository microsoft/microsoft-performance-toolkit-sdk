// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    [DisplayName("push")]
    [Description("Push the plugin package to a plugin source")]
    internal sealed class Push
        : Command
    {
        [DisplayName("feed")]
        [Description("The source feed to push the plugin package to")]
        [PositionalArgument(0)]
        [ExpandPath]
        public string? PluginSourceFeed { get; set; }

        [DisplayName("package")]
        [Description("The file path of the .ptpck package")]
        [PositionalArgument(1)]
        [ExpandPath]
        public string? PluginPackagePath { get; set; }

        public override async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            var uri = new Uri(this.PluginSourceFeed);
            var source = new PluginSource(uri);
            var packagePath = Path.GetFullPath(this.PluginPackagePath);

            if (!File.Exists(packagePath))
            {
                Console.Error.WriteLine($"The package '{packagePath}' does not exist.");
                return 1;
            }

            var metadataReader = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            Func<Type, ILogger> loggerFactory = x => ConsoleLogger.Create(x.GetType());

            var zipPluginPackageReader = new ZipPluginPackageReader(
                metadataReader,
                loggerFactory);

            var pusher = new PluginPackagePusher(
                zipPluginPackageReader,
                loggerFactory);
            
            using FileStream package = File.OpenRead(packagePath);

            Console.WriteLine($"Pushing package is not yet implemented.");
            return 1;

            // TODO: Implement push and uncomment the following line
            //bool success = await pusher.PushPackage(source, package, cancellationToken);

            //return success ? 0 : 1;
        }
    }
}

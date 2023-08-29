// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Commands;
using Microsoft.Performance.Toolkit.Plugins.Cli.Console;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;

namespace Microsoft.Performance.Toolkit.Plugins.Cli
{
    /// <summary>
    ///     Represents the entry point for the CLI.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        ///     Main entry point.
        /// </summary>
        /// <param name="args">
        ///     The command line arguments.
        /// </param>
        /// <returns>
        ///     A task whose result is the exit code. 0 on success; otherwise, non-zero.
        /// </returns>
        public static int Main(string[] args)
        {
            return CreatPluginsCli().Run(args);
        }

        private static PluginsCli CreatPluginsCli()
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<PluginsCli>()
                .AddSingleton<ICommand<PackArgs>, PackCommand>()
                .AddSingleton<ICommand<MetadataGenArgs>, MetadataGenCommand>()
                .AddSingleton<IPluginArtifactsProcessor, PluginArtifactsProcessor>()
                .AddSingleton<IPackageBuilder, ZipPluginPackageBuilder>()
                .AddSingleton<ISerializer<PluginManifest>>(SerializationUtils.GetJsonSerializer<PluginManifest>(new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }))
                .AddSingleton<ISerializer<PluginMetadata>>(SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>())
                .AddSingleton<ISerializer<PluginContentsMetadata>>(SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginContentsMetadata>())
                .AddSingleton<IManifestFileReader, ManifestReader>()
                .AddSingleton<IManifestFileValidator, ManifestJsonSchemaValidator>()
                .AddSingleton<IJsonSchemaLoader, LocalManifestSchemaLoader>()
                .AddSingleton<IManifestLocatorFactory, ManifestLocatorFactory>()
                .BuildServiceProvider();

             return serviceProvider.GetRequiredService<PluginsCli>();
        }
    }
}

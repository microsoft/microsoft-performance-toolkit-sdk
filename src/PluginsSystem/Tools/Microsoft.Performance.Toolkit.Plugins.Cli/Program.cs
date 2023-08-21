// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.Toolkit.Plugins.Cli.Interfaces;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Package;

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
        public static async Task<int> Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);

            string schemaFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Constants.ManifestSchemaFilePath);

            string manifestSchema = File.ReadAllText(schemaFilePath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var manifestSerializer = SerializationUtils.GetJsonSerializer<PluginManifest>(options);
            var metadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            var contentsMetadataSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginContentsMetadata>();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<IMetadataGenerator, MetadataGenerator>()
                .AddSingleton<IPluginSourceFilesValidator, PluginSourceFilesValidator>()
                .AddSingleton<ISerializer<PluginManifest>>(manifestSerializer)
                .AddSingleton<ISerializer<PluginMetadata>>(metadataSerializer)
                .AddSingleton<ISerializer<PluginContentsMetadata>>(contentsMetadataSerializer)
                .AddSingleton<IManifestReader, ManifestReader>()
                .AddSingleton<IPluginManifestFileValidator>(serviceProvider => new PluginManifestJsonValidator(
                    File.ReadAllText(Constants.ManifestSchemaFilePath),
                    serviceProvider.GetRequiredService<ILogger<PluginManifestJsonValidator>>()))
                .BuildServiceProvider();

            return result.MapResult(
                (PackOptions opts) => RunPack(opts, serviceProvider),
                (MetadataGenOptions opts) => RunMetadataGen(opts, serviceProvider),
                errs => HandleParseError(errs, serviceProvider));
        }

        private static int HandleParseError(IEnumerable<Error> errs, ServiceProvider serviceProvider)
        {
            ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            string errors = string.Join(Environment.NewLine, errs.Select(x => x.ToString()));
            logger.LogError($"Failed to parse command line arguments: \n{errors}");

            return 1;
        }

        private static int RunPack(PackOptions packOptions, ServiceProvider serviceProvider)
        {
            IPluginSourceFilesValidator validator = serviceProvider.GetRequiredService<IPluginSourceFilesValidator>();
            IManifestReader manifestReader = serviceProvider.GetRequiredService<IManifestReader>();
            IMetadataGenerator metadataGenerator = serviceProvider.GetRequiredService<IMetadataGenerator>();
            ISerializer<PluginMetadata> metadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginMetadata>>();
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginContentsMetadata>>();

            bool shouldInclude = packOptions.ManifestFilePath == null;

            if (!validator.Validate(packOptions.SourceDirectory, shouldInclude, out ValidatedPluginDirectory? validatedPluginDirectory))
            {
                return 1;
            }

            string? manifestFilePath = shouldInclude ? validatedPluginDirectory.ManifestFilePath : packOptions.ManifestFilePath;
            PluginManifest? manifest = manifestReader.TryReadFromFile(manifestFilePath);

            if (manifest == null)
            {
                return 1;
            }

            AllMetadata? allMetadata = metadataGenerator.TryGen(validatedPluginDirectory, manifest);
            PluginMetadata metadata = allMetadata.Metadata;
            PluginContentsMetadata contentsMetadata = allMetadata.ContentsMetadata;


            string tmpPath = Path.GetTempFileName();
            using (var builder = new PluginPackageBuilder(tmpPath, metadataSerializer, contentsMetadataSerializer))
            {
                builder.AddMetadata(metadata);
                builder.AddContentsMetadata(contentsMetadata);
                builder.AddContent(
                    packOptions.SourceDirectory,
                    s => (packOptions.ManifestBundled && s.Equals(Path.Combine(packOptions.SourceDirectory, Constants.BundledManifestName)))
                            || (packOptions.ManifestFilePath != null && s.Equals(Path.GetFullPath(packOptions.ManifestFilePath), StringComparison.OrdinalIgnoreCase)),
                    CancellationToken.None);
            }

            string packageFileName = $"{metadata.Identity}{PackageConstants.PluginPackageExtension}";
            string targetDirectory = Path.GetFullPath(packOptions.TargetDirectory ?? Environment.CurrentDirectory);
            string targetFileName = Path.Combine(targetDirectory, packageFileName);
            Directory.CreateDirectory(targetDirectory);

            File.Delete(targetFileName);
            File.Move(tmpPath, targetFileName);

            return 0;
        }

        private static int RunMetadataGen(MetadataGenOptions metadataGenOptions, ServiceProvider serviceProvider)
        {
            IPluginSourceFilesValidator validator = serviceProvider.GetRequiredService<IPluginSourceFilesValidator>();
            IManifestReader manifestReader = serviceProvider.GetRequiredService<IManifestReader>();
            IMetadataGenerator metadataGenerator = serviceProvider.GetRequiredService<IMetadataGenerator>();
            ISerializer<PluginMetadata> metadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginMetadata>>();
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginContentsMetadata>>();

            bool shouldInclude = metadataGenOptions.ManifestFilePath == null;

            if (!validator.Validate(metadataGenOptions.SourceDirectory, shouldInclude, out ValidatedPluginDirectory? validatedPluginDirectory))
            {
                return 1;
            }

            string? manifestFilePath = shouldInclude ? validatedPluginDirectory.ManifestFilePath : metadataGenOptions.ManifestFilePath;
            PluginManifest? manifest = manifestReader.TryReadFromFile(manifestFilePath);

            if (manifest == null)
            {
                return 1;
            }

            AllMetadata? allMetadata = metadataGenerator.TryGen(validatedPluginDirectory, manifest);
            if (allMetadata == null)
            {
                return 1;
            }


            PluginMetadata metadata = allMetadata.Metadata;
            PluginContentsMetadata contentsMetadata = allMetadata.ContentsMetadata;    

            string fileName = $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}";
            string contentsFileName = $"{metadata.Identity}-{PackageConstants.PluginContentsMetadataFileName}";

            try
            {
                string targetDirectory = Path.GetFullPath(metadataGenOptions.TargetDirectory ?? Environment.CurrentDirectory);
                Directory.CreateDirectory(targetDirectory);


                string targetFileName = Path.Combine(targetDirectory, fileName);
                using (FileStream fileStream = File.Open(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    metadataSerializer.Serialize(fileStream, metadata);
                }

                string contentsTargetFileName = Path.Combine(metadataGenOptions.TargetDirectory ?? Environment.CurrentDirectory, contentsFileName);
                using (FileStream fileStream = File.Open(contentsTargetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    contentsMetadataSerializer.Serialize(fileStream, contentsMetadata);
                }
            }
            catch
            {
                return 1;
            }

            return 0;
        }
    }
}
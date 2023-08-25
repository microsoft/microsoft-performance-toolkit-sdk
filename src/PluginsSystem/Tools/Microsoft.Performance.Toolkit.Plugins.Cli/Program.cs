// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options.Validation;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
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
        public static async Task<int> Main(string[] args)
        {
            ParserResult<object> result = Parser.Default.ParseArguments<PackOptions, MetadataGenOptions>(args);

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddLogging(x => x.AddConsole())
                .AddSingleton<IPluginContentsProcessor, SourceProcessor>()
                .AddSingleton<IOptionsValidator<PackOptions, TransformedPackOptions>, PackOptionsValidator>()
                .AddSingleton<IOptionsValidator<MetadataGenOptions, TransformedMetadataGenOptions>, MetadataGenOptionsValidator>()
                .AddSingleton<IPack, Pack>()
                .AddSingleton<IMetadataGen, MetadataGen>()
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
                .BuildServiceProvider();

            ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                return result.MapResult(
                    (PackOptions opts) => RunPack(opts, serviceProvider),
                    (MetadataGenOptions opts) => RunMetadataGen(opts, serviceProvider),
                    errs => HandleParseError(errs, serviceProvider));
            }
            catch (ArgumentValidationException ex)
            {
                logger.LogError("Invalid arguments. {0}", ex.Message);
            }
            catch (InvalidPluginContentException ex)
            {
                logger.LogError("Invalid plugin content. {0}", ex.Message);
            }
            catch (InvalidManifestException ex)
            {
                logger.LogError("Invalid manifest file. {0}", ex.Message);
            }
            catch (ConsoleRuntimeException ex)
            {
                logger.LogError("A runtime error occurred: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError("Unhandled exception occurred: {0}", ex.Message);
            }

            return 1;
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
            IPack pack = serviceProvider.GetRequiredService<IPack>();

            return pack.Run(packOptions);
        }

        private static int RunMetadataGen(MetadataGenOptions metadataGenOptions, ServiceProvider serviceProvider)
        {
            IMetadataGen metadataGen = serviceProvider.GetRequiredService<IMetadataGen>();

            return metadataGen.Run(metadataGenOptions);
        }

        // To utils
        public static string GetValidDestFileName(string file)
        {
            string? directory = Path.GetDirectoryName(file);
            string name = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);

            string destFileName = file;

            int fileCount = 1;
            while (File.Exists(destFileName))
            {
                destFileName = Path.Combine(directory!, $"{name}_({fileCount++}){extension}");
            }

            return destFileName;
        }
    }
}

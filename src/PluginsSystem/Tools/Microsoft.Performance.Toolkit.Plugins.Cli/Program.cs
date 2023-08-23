// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Cli.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Cli.Options;
using Microsoft.Performance.Toolkit.Plugins.Cli.Packaging;
using Microsoft.Performance.Toolkit.Plugins.Cli.Validation;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

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
                .AddSingleton<IMetadataGenerator, MetadataGenerator>()
                .AddSingleton<ISourceFilesProcessor, PluginSourceFilesProcessor>()
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
            packOptions.Validate();
            
            PackageGenCommon(packOptions, serviceProvider, out ProcessedPluginDirectory processedDir, out PluginMetadata metadata);

            IPackageBuilder packageBuilder = serviceProvider.GetRequiredService<IPackageBuilder>();
            ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            string? destFilePath = packOptions.OutputFileFullPath;
            if (destFilePath == null || !packOptions.Overwrite)
            {
                string destFileName = destFilePath == null ?
                    Path.Combine(Environment.CurrentDirectory, metadata.Identity.ToString()) :
                    Path.GetFileNameWithoutExtension(destFilePath);

                destFilePath = GetValidDestFileName(destFileName, PackageConstants.PluginPackageExtension);
            }

            string tmpFile = Path.GetTempFileName();
            try
            {
                packageBuilder.Build(processedDir, metadata, tmpFile);
                File.Move(tmpFile, destFilePath, packOptions.Overwrite);

                logger.LogInformation($"Successfully created plugin package at {destFilePath}");
            }
            finally
            {
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }
            }

            return 0;
        }

        private static int RunMetadataGen(MetadataGenOptions metadataGenOptions, ServiceProvider serviceProvider)
        {
            metadataGenOptions.Validate();
            PackageGenCommon(metadataGenOptions, serviceProvider, out ProcessedPluginDirectory processedDir, out PluginMetadata metadata);

            ISerializer<PluginMetadata> metadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginMetadata>>();
            ISerializer<PluginContentsMetadata> contentsMetadataSerializer = serviceProvider.GetRequiredService<ISerializer<PluginContentsMetadata>>();
            ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            bool outputSpecified = metadataGenOptions.OutputDirectory != null;
            string? outputDirectory = outputSpecified ? metadataGenOptions.OutputDirectoryFullPath : Environment.CurrentDirectory;
            
            string destMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{Constants.MetadataFileName}");
            string validDestMetadataFileName = (outputSpecified && metadataGenOptions.Overwrite) ?
                $"{destMetadataFileName}{Constants.MetadataFileExtension}" : GetValidDestFileName(destMetadataFileName, Constants.MetadataFileExtension);

            string destContentsMetadataFileName = Path.Combine(outputDirectory!, $"{metadata.Identity}-{Constants.ContentsMetadataFileName}");
            string validDestContentsMetadataFileName = (outputSpecified && metadataGenOptions.Overwrite) ?
                $"{destContentsMetadataFileName}{Constants.MetadataFileExtension}" : GetValidDestFileName(destContentsMetadataFileName, Constants.MetadataFileExtension);

            try
            {
                using (FileStream fileStream = File.Open(validDestMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    metadataSerializer.Serialize(fileStream, metadata);
                }
            }
            catch (IOException ex)
            {
                logger.LogDebug(ex, $"IO exception when writing to {validDestMetadataFileName}.");
                throw new ConsoleRuntimeException($"Failed to create plugin metadata at {validDestMetadataFileName}.", ex);
            }

            logger.LogInformation($"Successfully created plugin metadata at {validDestMetadataFileName}.");
            
            try
            {
                using (FileStream fileStream = File.Open(validDestContentsMetadataFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    contentsMetadataSerializer.Serialize(fileStream, processedDir.ContentsMetadata);
                }
            }
            catch (IOException ex)
            {
                logger.LogDebug(ex, $"IO exception when writing to {validDestContentsMetadataFileName}.");
                throw new ConsoleRuntimeException($"Failed to create plugin contents metadata at {validDestContentsMetadataFileName}.", ex);
            }

            logger.LogInformation($"Successfully created plugin contents metadata at {validDestContentsMetadataFileName}.");
            
            return 0;
        }

        private static void PackageGenCommon(PackageGenCommonOptions options, ServiceProvider serviceProvider, out ProcessedPluginDirectory processedDir, out PluginMetadata metadata)
        {
            ISourceFilesProcessor processor = serviceProvider.GetRequiredService<ISourceFilesProcessor>();
            IManifestFileValidator manifestValidator = serviceProvider.GetRequiredService<IManifestFileValidator>();
            IManifestFileReader manifestReader = serviceProvider.GetRequiredService<IManifestFileReader>();
            IMetadataGenerator metadataGenerator = serviceProvider.GetRequiredService<IMetadataGenerator>();
            ILogger logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            bool shouldInclude = options.ManifestFilePath == null;
            processedDir = processor.Process(options.SourceDirectoryFullPath, shouldInclude);

            string? manifestFilePath = shouldInclude ? processedDir.ManifestFilePath : options.ManifestFileFullPath;
            if (!manifestValidator.IsValid(manifestFilePath!, out List<string> validationMessages))
            {
                string errors = string.Join(Environment.NewLine, validationMessages);
                logger.LogWarning($"Manifest file failed some json schema format validation checks: \n{errors}");
                logger.LogWarning("Continuing with packing process but it is recommended to fix the validation errors and repack before publishing the plugin.");
            }

            PluginManifest manifest = manifestReader.Read(manifestFilePath!);
            metadata = metadataGenerator.Generate(processedDir, manifest);

        }

        private static string GetValidDestFileName(string filename, string extension)
        {
            string destFileName = $"{filename}{extension}";

            int fileCount = 1;
            while (File.Exists(destFileName))
            {
                destFileName = $"{filename}_({fileCount++}){extension}";
            }

            return destFileName;
        }
    }
}
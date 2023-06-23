// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using SystemVersion = System.Version;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("metadata-gen", HelpText = $"Generates a {PackageConstants.PluginMetadataFileName} file from the specified source directory.")]
    internal class MetadataGenOptions
    {
        public MetadataGenOptions(
            string sourceDirectory,
            string targetDirectory,
            string manifestFilePath,
            string id,
            string version,
            string pluginDisplayName,
            string pluginDescription)
        {
            this.SourceDirectory = sourceDirectory;
            this.TargetDirectory = targetDirectory;
            this.ManifestFilePath = manifestFilePath;
            this.Id = id;
            this.Version = version;
            this.PluginDisplayName = pluginDisplayName;
            this.PluginDescription = pluginDescription;
        }

        [Option(
            's',
            "source",
            Required = true,
            HelpText = "The directory containing the plugin binaries.")]
        public string SourceDirectory { get; }

        [Option(
            't',
            "target",
            Required = false,
            HelpText = "Directory where the metadata file will be created. If not specified, the current directory will be used.")]
        public string TargetDirectory { get; }

        [Option(
            'm',
            "manifest",
            SetName = "manifest",
            HelpText = "Path to the plugin manifest file. If not specified, command line arguments will be used.")]
        public string? ManifestFilePath { get; }

        [Option(
            "id",
            SetName = "no-manifest",
            HelpText = "Id of the plugin.")]
        public string? Id { get; }

        [Option(
            'v',
            "version",
            SetName = "no-manifest",
            HelpText = "Version of the packaged plugin. Must be a valid System.Version string.")]
        public string? Version { get; }

        [Option(
            "displayName",
            SetName = "no-manifest",
            HelpText = "Display name of the plugin")]
        public string? PluginDisplayName { get; }

        [Option(
            "description",
            SetName = "no-manifest",
            HelpText = "Description of the plugin")]
        public string? PluginDescription { get; }

        public int Run(
            Func<Type, ILogger> loggerFactory,
            IPluginSourceFilesValidator sourceDirValidator,
            IPluginManifestValidator manifestValidator,
            IMetadataGenerator metadataGenerator)
        {

            ILogger logger = loggerFactory(typeof(MetadataGenOptions));

            if (!sourceDirValidator.Validate(this.SourceDirectory))
            {
                logger.Error($"Invalid plugin source files in: {this.SourceDirectory}");
                return 1;
            }

            PluginMetadataInit metadataInit = new();
            if (!metadataGenerator.TryCreateMetadata(this.SourceDirectory, out metadataInit))
            {
                logger.Error("Failed to extract metadata from assembly.");
                return 1;
            }

            PluginManifest? pluginManifest = null;
            if (this.ManifestFilePath != null)
            {
                if (!manifestValidator.Validate(this.ManifestFilePath))
                {
                    logger.Error($"Invalid plugin manifest file: {this.ManifestFilePath}");
                    return 1;
                }

                // If a manifest file was specified, use it to generate the metadata file
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                using (FileStream manifestStream = File.OpenRead(this.ManifestFilePath))
                {
                    pluginManifest = JsonSerializer.Deserialize<PluginManifest>(manifestStream, options: options);
                }
            }
            else
            {
                if (this.Id == null
                    || this.Version == null
                    || this.PluginDisplayName == null
                    || this.PluginDescription == null)
                {
                    logger.Error($"Missing required arguments. Must specify either a manifest file or all of the following: --id, --version, --displayName, --description");
                    return 1;
                }

                if (!SystemVersion.TryParse(this.Version, out SystemVersion? version))
                {
                    Console.Error.WriteLine($"The version '{this.Version}' is not a valid System.Version string.");
                    return 1;
                }

                pluginManifest = new PluginManifest(this.Id, version, this.PluginDisplayName, this.PluginDescription, null, null);
            }

            metadataInit.Id = pluginManifest.Id;
            metadataInit.Version = pluginManifest.Version;
            metadataInit.Description = pluginManifest.Description;
            metadataInit.DisplayName = pluginManifest.DisplayName;
            metadataInit.Owners = pluginManifest.Owners;

            var metadata = metadataInit.ToPluginMetadata();

            string fileName = $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}";
            string targetFileName = Path.Combine(this.TargetDirectory ?? Environment.CurrentDirectory, fileName);
            string? targetDirectory = Path.GetDirectoryName(targetFileName);
            Directory.CreateDirectory(targetDirectory);

            ISerializer<PluginMetadata> serializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            using (FileStream fileStream = File.Open(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fileStream, metadata);
            }

            return 0;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Manifest;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Package;
using SystemVersion = System.Version;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("pack", HelpText = $"Creates a new {PackageConstants.PluginPackageExtension} package using specified metadata and source directory.")]
    internal class PackOptions
    {
        public PackOptions(
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
            HelpText = $"Directory where the {PackageConstants.PluginPackageExtension} file will be created. If not specified, the current directory will be used.")]
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

            if (!metadataGenerator.TryCreateMetadata(this.SourceDirectory, out ExtractedMetadata extracted))
            {
                logger.Error("Failed to extract metadata from assembly.");
                return 1;
            }

            PluginMetadataInit metadataInit = new();
            if (this.ManifestFilePath != null)
            {
                if (!File.Exists(this.ManifestFilePath))
                {
                    logger.Error($"Plugin manifest file does not exist: {this.ManifestFilePath}");
                    return 1;
                }
                
                PluginManifest? pluginManifest = null;
                try
                {
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


                    if (!SystemVersion.TryParse(pluginManifest.Identity.Version, out SystemVersion? version))
                    {
                        Console.Error.WriteLine($"The version '{this.Version}' is not a valid System.Version string.");
                        return 1;
                    }

                    metadataInit.Identity = new PluginIdentity(pluginManifest.Identity.Id, version);
                    metadataInit.DisplayName = pluginManifest.DisplayName;
                    metadataInit.Description = pluginManifest.Description;
                    metadataInit.Owners = pluginManifest.Owners.Select(o => new PluginOwnerInfo(o.Name, o.Address, o.EmailAddresses, o.PhoneNumbers));
                    Uri.TryCreate(pluginManifest.ProjectUrl, UriKind.Absolute, out Uri? projectUrl);
                    metadataInit.ProjectUrl = projectUrl;
                }
                catch (JsonException ex)
                {
                    logger.Error($"Invalid plugin manifest file: {this.ManifestFilePath}");
                    logger.Error(ex.Message);
                    return 1;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to read plugin manifest file: {this.ManifestFilePath}");
                    logger.Error(ex.Message);
                    return 1;
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

                var id = new PluginIdentity(this.Id, version);
                metadataInit.Identity = id;
                metadataInit.DisplayName = this.PluginDisplayName;
                metadataInit.Description = this.PluginDescription;
            }

            metadataInit.SdkVersion = extracted.SdkVersion;
            metadataInit.InstalledSize = extracted.InstalledSize;
            metadataInit.ProcessingSources = extracted.ProcessingSources;
            metadataInit.DataCookers = extracted.DataCookers;
            metadataInit.ExtensibleTables = extracted.ExtensibleTables;


            //if (!manifestValidator.Validate(pluginManifest))
            //{
            //    logger.Error("Invalid plugin manifest. See errors above.");
            //    return 1;
            //}


            var metadata = metadataInit.ToPluginMetadata();
            var metadataContents = metadataInit.ToPluginContentsMetadata();

            string relativePackageFileName = $"{metadata.Identity}{PackageConstants.PluginPackageExtension}";
            string targetFileName = Path.Combine(this.TargetDirectory ?? Environment.CurrentDirectory, relativePackageFileName);
            string tmpPath = Path.GetTempFileName();
            ISerializer<PluginMetadata> serializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            ISerializer<PluginContentsMetadata> contentsSerializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginContentsMetadata>();

            using (var builder = new PluginPackageBuilder(tmpPath, serializer, contentsSerializer))
            {
                builder.AddMetadata(metadata);
                builder.AddContentsMetadata(metadataContents);
                builder.AddContent(this.SourceDirectory, CancellationToken.None);
            }

            string? targetDirectory = Path.GetDirectoryName(targetFileName);
            Directory.CreateDirectory(targetDirectory);

            File.Delete(targetFileName);
            File.Move(tmpPath, targetFileName);

            logger.Info($"Created plugin package: {targetFileName}");
            return 0;
        }
    }
}

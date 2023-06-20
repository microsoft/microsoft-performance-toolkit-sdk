// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;
using SystemVersion = System.Version;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    [Verb("metadata-gen", HelpText = "Generates a pluginspec.json file from the specified source directory.")]
    internal class MetadataGenOptions
    {
        [Option(
            's',
            "source",
            Required = true,
            HelpText = "The directory containing the plugin binaries.")]
        public string? SourceDirectory { get; set; }

        [Option(
            't',
            "target",
            Required = false,
            HelpText = "Directory where the metadata file will be created. If not specified, the current directory will be used.")]
        public string? TargetDirectory { get; set; }

        [Option(
            "id",
            Required = true,
            HelpText = "Id of the packed plugin.")]
        public string? Id { get; set; }

        [Option(
            'v',
            Required = true,
            HelpText = "Version of the packed plugin. Must be a valid System.Version string.")]
        public string? Version { get; set; }

        [Option(
            "displayName",
            Required = false,
            HelpText = "Display name of the packed plugin")]
        public string? PluginDisplayName { get; set; }

        [DisplayName("description")]
        [Description("Description of the packed plugin")]
        [Option(
            "description",
            Required = false,
            HelpText = "Description of the packed plugin")]
        public string? PluginDescription { get; set; }

        public int Run()
        {
            if (!Directory.Exists(this.SourceDirectory))
            {
                Console.Error.WriteLine($"The source directory '{this.SourceDirectory}' does not exist.");
                return 1;
            }

            PluginMetadata? metadata = null;
            ISerializer<PluginMetadata> serializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();
            
            PluginMetadataInit metadataInit = metadata == null ? new PluginMetadataInit() : PluginMetadataInit.FromPluginMetadata(metadata);

            // Override metadata with command line arguments if specified
            if (!string.IsNullOrEmpty(this.Id))
            {
                metadataInit.Id = this.Id;
            }

            if (!string.IsNullOrEmpty(this.Version))
            {
                if (!SystemVersion.TryParse(this.Version, out SystemVersion? version))
                {
                    Console.Error.WriteLine($"The version '{this.Version}' is not a valid System.Version string.");
                    return 1;
                }
                metadataInit.Version = version;
            }

            if (!string.IsNullOrEmpty(this.PluginDisplayName))
            {
                metadataInit.DisplayName = this.PluginDisplayName;
            }

            if (!string.IsNullOrEmpty(this.PluginDescription))
            {
                metadataInit.Description = this.PluginDescription;
            }

            if (!MetadataGenerator.TryExtractFromAssembly(this.SourceDirectory, metadataInit))
            {
                Console.Error.WriteLine("Failed to extract metadata from assembly.");
                return 1;
            }

            metadata = metadataInit.ToPluginMetadata();

            
            var fileName = $"{metadata.Identity}-{PackageConstants.PluginMetadataFileName}";
            string targetFileName = Path.Combine(this.TargetDirectory ?? Environment.CurrentDirectory, fileName);

            string? targetDirectory = Path.GetDirectoryName(targetFileName);
            Directory.CreateDirectory(targetDirectory);

            using (var fileStream = File.Open(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fileStream, metadata);
            }

            return 0;
        }
    }
}

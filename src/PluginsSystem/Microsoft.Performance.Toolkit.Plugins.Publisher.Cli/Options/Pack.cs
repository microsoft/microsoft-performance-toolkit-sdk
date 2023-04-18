// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;
using Microsoft.Performance.Toolkit.Plugins.Core.Serialization;
using SystemVersion = System.Version;

namespace Microsoft.Performance.Toolkit.Plugins.Publisher.Cli
{
    [DisplayName("pack")]
    [Description("Creates a new .ptpck package using specified metadata and source directory.")]
    internal sealed class Pack
        : Command
    {
        [DisplayName("source")]
        [Description("The directory containing the plugin binaries.")]
        [PositionalArgument(0)]
        [ExpandPath]
        public string? SourceDirectory { get; set; }

        [DisplayName("target")]
        [Description("Directory where the .ptpck file will be created. If not specified, the current directory will be used.")]
        [ExtraArgument]
        [ExpandPath]
        public string? TargetDirectory { get; set; }

        [DisplayName("metadata")]
        [Description("Path to the pluginspec.json file. If not specified, the metadata will be generated from the binaries in the source directory.")]
        [ExtraArgument]
        [ExpandPath]
        public string? MetadataPath { get; set; }

        [DisplayName("id")]
        [Description("Id of the packed plugin.")]
        [ExtraArgument]
        public string? Id { get; set; }

        [DisplayName("version")]
        [Description("Version of the packed plugin. Must be a valid System.Version string.")]
        [ExtraArgument]
        public string? Version { get; set; }

        [DisplayName("displayName")]
        [Description("Display name of the packed plugin")]
        [ExtraArgument]
        public string? PluginDisplayName { get; set; }

        [DisplayName("description")]
        [Description("Description of the packed plugin")]
        [ExtraArgument]
        public string? PluginDescription { get; set; }

        public override async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(this.SourceDirectory))
            {
                Console.Error.WriteLine($"The source directory '{this.SourceDirectory}' does not exist.");
                return 1;
            }

            PluginMetadata? metadata = null;
            ISerializer<PluginMetadata> serializer = SerializationUtils.GetJsonSerializerWithDefaultOptions<PluginMetadata>();

            if (!string.IsNullOrEmpty(this.MetadataPath))
            {
                if (!File.Exists(this.MetadataPath))
                {
                    Console.Error.WriteLine($"The metadata file '{this.MetadataPath}' does not exist.");
                    return 1;
                }

                try
                {
                    string path = Path.GetFullPath(this.MetadataPath);
                    using (FileStream stream = File.OpenRead(path))
                    {
                        metadata = serializer.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to read metadata file '{this.MetadataPath}': {ex.Message}");
                    return 1;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(this.Id) || string.IsNullOrEmpty(this.Version))
                {
                    Console.Error.WriteLine("When no metadata file is specified, both id and version must be specified.");
                    return 1;
                }
            }

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
            string relativePackageFileName = $"{metadata.Identity}.ptpck";
            string targetFileName = Path.Combine(this.TargetDirectory ?? Environment.CurrentDirectory, relativePackageFileName);

            string tmpPath = Path.GetTempFileName();
            using (var builder = new PluginPackageBuilder(tmpPath, serializer))
            {
                builder.AddMetadata(metadata);
                builder.AddContent(this.SourceDirectory, cancellationToken);
            }

            string? targetDirectory = Path.GetDirectoryName(targetFileName);
            Directory.CreateDirectory(targetDirectory);
            
            File.Delete(targetFileName);
            File.Move(tmpPath, targetFileName);

            Console.WriteLine($"{metadata.Identity} packed to {targetFileName}");

            return 0;
        }
    }
}

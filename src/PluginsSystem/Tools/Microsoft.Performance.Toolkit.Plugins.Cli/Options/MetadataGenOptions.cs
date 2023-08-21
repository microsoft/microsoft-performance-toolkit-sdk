// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("metadata-gen", HelpText = $"Generates a {PackageConstants.PluginMetadataFileName} file from the specified source directory.")]
    internal class MetadataGenOptions
    {
        public MetadataGenOptions(
            string sourceDirectory,
            string targetDirectory,
            bool manifestBundled,
            string manifestFilePath,
            string id,
            string version,
            string pluginDisplayName,
            string pluginDescription)
        {
            this.SourceDirectory = sourceDirectory;
            this.TargetDirectory = targetDirectory;
            this.ManifestFilePath = manifestFilePath;
            this.ManifestBundled = manifestBundled;
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
            'b',
            "manifest-bundled",
            SetName = "manifest-bundled",
            HelpText = "Indicates that the plugin manifest file should be included in the package source directory.")]
        public bool ManifestBundled { get; }

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
    }
}

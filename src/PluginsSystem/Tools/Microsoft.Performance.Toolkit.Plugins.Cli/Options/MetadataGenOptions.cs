// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("metadata-gen", HelpText = $"Generates a {PackageConstants.PluginMetadataFileName} file from the specified source directory.")]
    internal class MetadataGenOptions
    {
        public MetadataGenOptions(
            string sourceDirectory,
            string outputDirectory,
            bool overwrite,
            string manifestFilePath)
        {
            this.SourceDirectory = sourceDirectory;
            this.OutputDirectory = outputDirectory;
            this.Overwrite = overwrite;
            this.ManifestFilePath = manifestFilePath;
        }

        [Option(
            's',
            "source",
            Required = true,
            HelpText = "The directory containing the plugin binaries.")]
        public string SourceDirectory { get; }

        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "Directory where the metadata file(s) will be created. If not specified, the current directory will be used.")]
        public string? OutputDirectory { get; }

        [Option(
            'w',
            "overwrite",
            Required = false,
            HelpText = "Indicates that the destination file(s) should be overwritten if they already exist.")]
        public bool Overwrite { get; } = false;

        [Option(
            'm',
            "manifest",
            Required = false,
            HelpText = "Path to the plugin manifest file. If not specified, the program will attempt to find the manifest in the source directory.")]
        public string? ManifestFilePath { get; }
    }
}

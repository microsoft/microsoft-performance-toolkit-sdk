// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("pack", HelpText = $"Creates a new {PackageConstants.PluginPackageExtension} package using specified metadata and source directory.")]
    internal class PackOptions
    {
        public PackOptions(
            string sourceDirectory,
            string outputFilePath,
            bool overwrite,
            string manifestFilePath)
        {
            this.SourceDirectory = sourceDirectory;
            this.OutputFilePath = outputFilePath;
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
            HelpText = "The path to the output package file. If not specified, a file will be created in the current directory.")]
        public string? OutputFilePath { get; }

        [Option(
            'w',
            "overwrite",
            Required = false,
            HelpText = "Indicates that the destination file should be overwritten if it already exists.")]
        public bool Overwrite { get; } = false;

        [Option(
            'm',
            "manifest",
            Required = false,
            HelpText = "Path to the plugin manifest file. If not specified, the program will attempt to find the manifest in the source directory.")]
        public string? ManifestFilePath { get; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.SourceDirectory))
            {
                // throw new ProcessingException("Source directory must be specified.");
            }

            if (string.IsNullOrWhiteSpace(this.OutputFilePath))
            {
                // throw new ProcessingException("Destination file must be specified.");
            }

            if (Path.GetExtension(this.OutputFilePath) != PackageConstants.PluginPackageExtension)
            {
                // throw new ProcessingException($"Destination file must have extension '{PackageConstants.PluginPackageExtension}'.");
            }

            // Check if dest directory exists
        }
    }
}

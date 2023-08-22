// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("pack", HelpText = $"Creates a new {PackageConstants.PluginPackageExtension} package using specified metadata and source directory.")]
    internal class PackOptions
        : PackageGenCommonOptions
    {
        public PackOptions(
            string sourceDirectory,
            string outputFilePath,
            bool overwrite,
            string manifestFilePath)
            : base(sourceDirectory, overwrite, manifestFilePath)
        {
            this.OutputFilePath = outputFilePath;
        }

        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "The path to the output package file. If not specified, a file will be created in the current directory.")]
        public string? OutputFilePath { get; }

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

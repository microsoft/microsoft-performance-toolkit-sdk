// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
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

        internal string? OutputFileFullPath { get; private set; }

        public override void Validate()
        {
            base.Validate();

            if (this.OutputFilePath == null && this.Overwrite)
            {
                throw new ArgumentValidationException("Cannot overwrite output file when output file is not specified.");
            }

            if (this.OutputFilePath != null)
            {
                if (!Path.GetExtension(this.OutputFilePath).Equals(PackageConstants.PluginPackageExtension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentValidationException($"Output file must have extension '{PackageConstants.PluginPackageExtension}'.");
                }

                try
                {
                    this.OutputFileFullPath = Path.GetFullPath(this.OutputFilePath);
                }
                catch (Exception ex)
                {
                    throw new ArgumentValidationException("Unable to get full path to output file.", ex);
                }

                string? outputDir = Path.GetDirectoryName(this.OutputFileFullPath);
                if (!Directory.Exists(outputDir))
                {
                    throw new ArgumentValidationException($"The directory '{outputDir}' does not exist. Please provide a valid directory path or create the directory and try again.");
                }
            }

        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    [Verb("metadata-gen", HelpText = $"Generates a {PackageConstants.PluginMetadataFileName} file from the specified source directory.")]
    internal class MetadataGenOptions
        : PackageGenCommonOptions
    {
        public MetadataGenOptions(
            string sourceDirectory,
            string outputDirectory,
            bool overwrite,
            string manifestFilePath)
            : base(sourceDirectory, overwrite, manifestFilePath)
        {
            this.OutputDirectory = outputDirectory;
        }

        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "Directory where the metadata file(s) will be created. If not specified, the current directory will be used.")]
        public string? OutputDirectory { get; }

        internal string? OutputDirectoryFullPath { get; private set; }


        public override void Validate()
        {
            base.Validate();

            if (this.OutputDirectory != null)
            {
                if (!Directory.Exists(this.OutputDirectory))
                {
                    throw new ArgumentValidationException($"Output directory '{this.OutputDirectory}' does not exist. Please create it or omit the --output option to use the current directory.");
                }

                this.OutputDirectoryFullPath = Path.GetFullPath(this.OutputDirectory);
            }
        }
    }
}

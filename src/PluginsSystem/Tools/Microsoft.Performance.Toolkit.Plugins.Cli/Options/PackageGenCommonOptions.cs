// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.Toolkit.Plugins.Cli.Exceptions;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Options
{
    internal abstract class PackageGenCommonOptions
    {
        protected PackageGenCommonOptions(
            string sourceDirectory,
            bool overwrite,
            string? manifestFilePath)
        {
            this.SourceDirectory = sourceDirectory;
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

        internal string SourceDirectoryFullPath { get; private set; }

        internal string? ManifestFileFullPath { get; private set; }
        
        public virtual void Validate()
        {
            // Validate source directory
            if (string.IsNullOrWhiteSpace(this.SourceDirectory))
            {
                throw new ArgumentValidationException("Source directory must be specified. Use --source <path> or -s <path>.");
            }

            if (!Directory.Exists(this.SourceDirectory))
            {
                throw new ArgumentValidationException($"Source directory '{this.SourceDirectory}' does not exist.");
            }
            
            this.SourceDirectoryFullPath = Path.GetFullPath(this.SourceDirectory);

            // Validate manifest file path
            if (this.ManifestFilePath != null)
            {
                if (!File.Exists(this.ManifestFilePath))
                {
                    throw new ArgumentValidationException($"Manifest file '{this.ManifestFilePath}' does not exist.");
                }

                this.ManifestFileFullPath = Path.GetFullPath(this.ManifestFilePath);
            }
        }
    }
}

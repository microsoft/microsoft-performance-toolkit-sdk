// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs
{
    /// <summary>
    ///     Common options for the pack and metadata-gen verbs.
    /// </summary>
    internal abstract class PackGenCommonOptions
    {
        protected PackGenCommonOptions(
            string sourceDirectory,
            bool overwrite,
            string? manifestFilePath)
        {
            this.SourceDirectory = sourceDirectory;
            this.Overwrite = overwrite;
            this.ManifestFilePath = manifestFilePath;
        }

        /// <summary>
        ///     Gets the directory containing the plugin binaries.
        /// </summary>
        [Option(
           's',
           "source",
           Required = true,
           HelpText = "The directory containing the plugin binaries.")]
        public string SourceDirectory { get; }

        /// <summary>
        ///     Gets whether the destination file should be overwritten if it already exists.
        /// </summary>
        [Option(
            'w',
            "overwrite",
            Required = false,
            HelpText = "Indicates that the destination file should be overwritten if it already exists.")]
        public bool Overwrite { get; } = false;

        /// <summary>
        ///     Gets the path to the plugin manifest file.
        /// </summary>
        [Option(
            'm',
            "manifest",
            Required = false,
            HelpText = "Path to the plugin manifest file. If not specified, the program will attempt to find the manifest in the source directory.")]
        public string? ManifestFilePath { get; }
    }
}

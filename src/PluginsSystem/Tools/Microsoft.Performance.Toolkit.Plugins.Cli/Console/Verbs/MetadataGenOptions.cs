// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs
{
    /// <summary>
    ///     Options for the metadata-gen verb.
    /// </summary>
    [Verb("metadata-gen", HelpText = $"Generates a {PackageConstants.PluginMetadataFileName} and a {PackageConstants.PluginContentsMetadataFileName} file from the specified plugin source directory.")]
    internal class MetadataGenOptions
        : PackGenCommonOptions
    {
        /// <summary>
        ///     Creates a new instance of <see cref="MetadataGenOptions"/>.
        /// </summary>
        /// <param name="sourceDirectory">
        ///     The directory containing the plugin binaries.
        /// </param>
        /// <param name="outputDirectory">
        ///     The directory where the metadata file(s) will be created.
        /// </param>
        /// <param name="overwrite">
        ///     Indicates that the destination file should be overwritten if it already exists.
        /// </param>
        /// <param name="manifestFilePath">
        ///     Path to the plugin manifest file.
        /// </param>
        public MetadataGenOptions(
            string sourceDirectory,
            string outputDirectory,
            bool overwrite,
            string manifestFilePath)
            : base(sourceDirectory, overwrite, manifestFilePath)
        {
            this.OutputDirectory = outputDirectory;
        }

        /// <summary>
        ///     Gets the directory where the metadata file(s) will be created.
        /// </summary>
        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "Directory where the metadata file(s) will be created. If not specified, the current directory will be used.")]
        public string? OutputDirectory { get; }
    }
}

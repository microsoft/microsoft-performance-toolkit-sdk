// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Package;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Console.Verbs
{
    /// <summary>
    ///     Options for the pack verb.
    /// </summary>
    [Verb("pack", HelpText = $"Creates a new {PackageConstants.PluginPackageExtension} package using specified metadata and source directory.")]
    internal class PackOptions
        : PackGenCommonOptions
    {
        /// <summary>
        ///     Creates a new instance of <see cref="PackOptions"/>.
        /// </summary>
        /// <param name="sourceDirectory">
        ///     The directory containing the plugin binaries.
        /// </param>
        /// <param name="outputFilePath">
        ///     The path to write the package file to.
        /// </param>
        /// <param name="overwrite">
        ///     Indicates that the destination file should be overwritten if it already exists.
        /// </param>
        /// <param name="manifestFilePath"></param>
        public PackOptions(
            string sourceDirectory,
            string outputFilePath,
            bool overwrite,
            string manifestFilePath)
            : base(sourceDirectory, overwrite, manifestFilePath)
        {
            this.OutputFilePath = outputFilePath;
        }

        /// <summary>
        ///     Gets the path to write the package file to.
        /// </summary>
        [Option(
            'o',
            "output",
            Required = false,
            HelpText = "The path to the output package file. If not specified, a file will be created in the current directory.")]
        public string? OutputFilePath { get; }
    }
}

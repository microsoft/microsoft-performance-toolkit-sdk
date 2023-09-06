// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    /// <summary>
    ///     Represents the arguments for the <see cref="PackCommand"/>.
    /// </summary>
    /// <param name="SourceDirectoryFullPath">
    ///     The full path to the directory containing the plugin to pack.
    /// </param>
    /// <param name="ManifestFileFullPath">
    ///     The full path to the manifest file.
    /// </param>
    /// <param name="OutputFileFullPath">
    ///     The full path to where the output package file should be written.
    /// </param>
    /// <param name="OverWrite">
    ///     Whether or not to overwrite the output file if it already exists.
    /// </param>
    internal record PackArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputFileFullPath,
        bool OverWrite)
        : PackGenCommonArgs(SourceDirectoryFullPath, ManifestFileFullPath, OverWrite)
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PackArgs"/>
        /// </summary>
        /// <param name="commonArgs">
        ///     The common arguments.
        /// </param>
        /// <param name="outputFileFullPath">
        ///     The full path to the output package file.
        /// </param>
        public PackArgs(PackGenCommonArgs commonArgs, string? outputFileFullPath)
            : this(commonArgs.SourceDirectoryFullPath, commonArgs.ManifestFileFullPath, outputFileFullPath, commonArgs.Overwrite)
        {
        }
    }
}

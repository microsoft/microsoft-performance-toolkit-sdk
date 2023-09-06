// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    /// <summary>
    ///     Represents the arguments for the <see cref="MetadataGenCommand"/>.
    /// </summary>
    /// <param name="SourceDirectoryFullPath">
    ///     The full path to the directory containing the source code for the plugin.
    /// </param>
    /// <param name="ManifestFileFullPath">
    ///     The full path to the manifest file for the plugin.
    /// </param>
    /// <param name="OutputDirectoryFullPath">
    ///     The full path to the directory to write the generated metadata to.
    /// </param>
    /// <param name="Overwrite">
    ///     Whether or not to overwrite existing files in the output directory.
    /// </param>
    internal record MetadataGenArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        string? OutputDirectoryFullPath,
        bool Overwrite)
        : PackGenCommonArgs(
            SourceDirectoryFullPath,
            ManifestFileFullPath,
            Overwrite)
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MetadataGenArgs"/> class.
        /// </summary>
        /// <param name="commonArgs">
        ///     The common arguments.
        /// </param>
        /// <param name="outputDirectoryFullPath">
        ///     The full path to the directory to write the generated metadata.
        /// </param>
        public MetadataGenArgs(PackGenCommonArgs commonArgs, string? outputDirectoryFullPath)
            : this(commonArgs.SourceDirectoryFullPath, commonArgs.ManifestFileFullPath, outputDirectoryFullPath, commonArgs.Overwrite)
        {
        }
    }
}

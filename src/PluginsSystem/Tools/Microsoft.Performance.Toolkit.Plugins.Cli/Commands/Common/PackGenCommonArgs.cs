// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Commands
{
    /// <summary>
    ///     Common arguments for packgen commands.
    /// </summary>
    /// <param name="SourceDirectoryFullPath">
    ///     The full path to the directory containing the plugin.
    /// </param>
    /// <param name="ManifestFileFullPath">
    ///     The full path to the manifest file to use.
    /// </param>
    /// <param name="Overwrite">
    ///     Whether or not to overwrite an existing output file.
    /// </param>    
    internal record PackGenCommonArgs(
        string SourceDirectoryFullPath,
        string? ManifestFileFullPath,
        bool Overwrite);
}

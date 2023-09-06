// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Performance.Toolkit.Plugins.Cli.Processing
{
    /// <summary>
    ///     Represents a processor that can process a <see cref="PluginArtifacts"/> instance.
    /// </summary>
    internal interface IPluginArtifactsProcessor
    {
        /// <summary>
        ///     Attempts to process the given <see cref="PluginArtifacts"/> instance into a <see cref="ProcessedPluginResult"/>.
        /// </summary>
        /// <param name="artifacts">
        ///     The <see cref="PluginArtifacts"/> instance to process.
        /// </param>
        /// <param name="processedPlugin">
        ///     The created <see cref="ProcessedPluginResult"/> instance, if processing was successful.
        /// </param>
        /// <returns>
        ///     <c>true</c> if processing was successful; <c>false</c> otherwise.
        /// </returns>
        public bool TryProcess(PluginArtifacts artifacts, [NotNullWhen(true)] out ProcessedPluginResult? processedPlugin);
    }
}

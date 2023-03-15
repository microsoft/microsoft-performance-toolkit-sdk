// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime
{
    /// <summary>
    ///     Represents a package extractor that can extract a package to a destination.
    /// </summary>
    /// <typeparam name="TDestination">
    ///     The type of destination to which the package will be extracted.
    /// </typeparam>
    public interface IPluginPackageExtractor<TDestination>
    {
        /// <summary>
        ///     Extracts all files in this package.
        /// </summary>
        /// <param name="extractLocation">
        ///     The location to which the files will be extracted.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the extraction.
        /// </param>
        /// <returns>
        ///     An await-able task that completes when the extraction is complete.
        /// </returns>
        Task ExtractPackageAsync(
            PluginPackage pluginPackage,
            TDestination extractLocation,
            CancellationToken cancellationToken,
            IProgress<int> progress);

        /// <summary>
        ///     Extract certain entires from the package.
        /// </summary>
        /// <param name="extractPath">
        ///     The location to which the files will be extracted.
        /// </param>
        /// <param name="predicate">
        ///     A function whose result indicates whether an entry should be extracted.
        /// </param>
        /// <param name="cancellationToken">
        ///     Signals that the caller wishes to cancel the operation.
        /// </param>
        /// <param name="progress">
        ///     Indicates the progress of the extraction.
        /// </param>
        /// <returns>
        ///     An await-able task that completes when the extraction is complete.
        /// </returns>
        Task ExtractEntriesAsync(
            PluginPackage pluginPackage,
            TDestination extractPath,
            Func<PluginPackageEntry, bool> predicate,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}

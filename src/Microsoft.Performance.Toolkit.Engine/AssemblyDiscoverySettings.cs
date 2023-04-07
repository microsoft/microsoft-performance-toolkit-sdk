// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Configures how plugins and plugin extensions are discovered.
    /// </summary>
    public sealed class AssemblyDiscoverySettings
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        public AssemblyDiscoverySettings(bool includeSubdirectories)
            : this(includeSubdirectories, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns used to find files to process. If <c>null</c> or empty, defaults to "*.dll" and 
        ///     "*.exe".
        /// </param>
        public AssemblyDiscoverySettings(
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns)
            : this(includeSubdirectories, searchPatterns, null, MatchCasing.CaseInsensitive)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns used to find files to process. If <c>null</c> or empty, defaults to "*.dll" and 
        ///     "*.exe".
        /// </param>
        /// <param name="exclusionPatterns">
        ///     Files paths that match these patterns will not be processed.
        ///     May be <c>null</c>.
        ///     e.g. { "excludeMe*.dll", "blob.dll" }
        /// </param>
        /// <param name="fileNameCaseSensitivity">
        ///     Indicates how case sensitivity will be applied to
        ///     <paramref name="searchPatterns"/> and <paramref name="exclusionPatterns"/>.
        /// </param>
        public AssemblyDiscoverySettings(
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionPatterns,
            MatchCasing fileNameCaseSensitivity)
        {
            IncludeSubdirectories = includeSubdirectories;
            SearchPatterns = searchPatterns;
            ExclusionPatterns = exclusionPatterns;
            FileNameCaseSensitivity = fileNameCaseSensitivity;
        }

        /// <summary>
        ///     Gets a value indicating whether subdirectories will be searched.
        /// </summary>
        public bool IncludeSubdirectories { get; }

        /// <summary>
        ///     Gets the search patterns to use. If null or empty, defaults to "*.dll" and "*.exe".
        /// </summary>
        public IEnumerable<string> SearchPatterns { get; }

        /// <summary>
        ///     Gets patterns to exclude matching file names from being processed.
        ///     May be <c>null</c>.
        ///     e.g. { "excludeMe*.dll", "blob.dll" }
        /// </summary>
        public IEnumerable<string> ExclusionPatterns { get; }

        /// <summary>
        ///     Gets a value that indicates whether files name pattern matching should be case sensitive.
        ///     <seealso cref="SearchPatterns"/>
        ///     <seealso cref="ExclusionPatterns"/>
        /// </summary>
        public MatchCasing FileNameCaseSensitivity { get; }
    }
}

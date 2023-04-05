// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

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
            : this(includeSubdirectories, null, null, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns to use. If <c>null</c> or empty, defaults to "*.dll" and "*.exe".
        /// </param>
        public AssemblyDiscoverySettings(
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns)
            : this(includeSubdirectories, searchPatterns, null, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="includeSubdirectories">
        ///     Indicates whether subdirectories will be searched.
        /// </param>
        /// <param name="searchPatterns">
        ///     The search patterns to use. If <c>null</c> or empty, defaults to "*.dll" and "*.exe".
        /// </param>
        /// <param name="exclusionFileNames">
        ///     A set of files names to exclude from the search. May be <c>null</c>.
        /// </param>
        /// <param name="exclusionsAreCaseSensitive">
        ///     Indicates whether exclusion file names should be treated as case sensitive.
        /// </param>
        public AssemblyDiscoverySettings(
            bool includeSubdirectories,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> exclusionFileNames,
            bool exclusionsAreCaseSensitive)
        {
            IncludeSubdirectories = includeSubdirectories;
            SearchPatterns = searchPatterns;
            ExclusionFileNames = exclusionFileNames;
            ExclusionsAreCaseSensitive = exclusionsAreCaseSensitive;
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
        ///     Gets a set of files names to exclude from the search. May be <c>null</c>.
        /// </summary>
        public IEnumerable<string> ExclusionFileNames { get; }

        /// <summary>
        ///     Gets indicating whether exclusion file names should be treated as case sensitive.
        /// </summary>
        public bool ExclusionsAreCaseSensitive { get; }
    }
}

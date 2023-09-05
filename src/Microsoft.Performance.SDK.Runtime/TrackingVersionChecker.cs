// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     A <see cref="VersionChecker"/> that keeps track of the versions it has checked and whether they are supported.
    /// </summary>
    public class TrackingVersionChecker
        : VersionChecker
    {
        private readonly ConcurrentDictionary<SemanticVersion, bool> checkedVersion;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingVersionChecker"/>
        /// </summary>
        public TrackingVersionChecker()
        {
            this.checkedVersion = new ConcurrentDictionary<SemanticVersion, bool>();
        }

        /// <summary>
        ///     Gets the list of versions that have been checked by this instance.
        /// </summary>
        public IReadOnlyDictionary<SemanticVersion, bool> CheckedVersions
        {
            get
            {
                return this.checkedVersion;
            }
        }

        /// <summary>
        ///     Overrides the base implementation to keep track of the versions that have been checked.
        /// </summary>
        /// <param name="candidateVersion">
        ///     The version to check.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the version is supported; <c>false</c> otherwise.
        /// </returns>
        public override bool IsVersionSupported(SemanticVersion candidateVersion)
        {
            bool supported = base.IsVersionSupported(candidateVersion);
            this.checkedVersion.TryAdd(candidateVersion, supported);

            return supported;
        }
    }
}

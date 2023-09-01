// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     A <see cref="VersionChecker"/> that keeps track of the versions it has verified to be supported.
    /// </summary>
    public class TrackingVersionChecker
        : VersionChecker
    {
        private readonly HashSet<SemanticVersion> verifiedVersions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrackingVersionChecker"/>
        /// </summary>
        public TrackingVersionChecker()
        {
            this.verifiedVersions = new HashSet<SemanticVersion>();
        }

        /// <summary>
        ///     Gets the list of versions that have been verified to be supported.
        /// </summary>
        public IReadOnlyList<SemanticVersion> VerifiedVersions
        {
            get
            {
                return this.verifiedVersions.ToList();
            }
        }

        /// <summary>
        ///     Overrides the base implementation to keep track of the versions that have been verified.
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
            if (supported)
            {
                this.verifiedVersions.Add(candidateVersion);
            }

            return supported;
        }
    }
}

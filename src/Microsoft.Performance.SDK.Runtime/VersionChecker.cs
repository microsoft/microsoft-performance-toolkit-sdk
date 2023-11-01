// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     This class is used to check compatibility of different
    ///     versions of the SDK. This class is only meant to be used
    ///     from the hosting process. The hosting process's version of
    ///     the SDK is used as the reference for all compatibility checks.
    /// </summary>
    public class VersionChecker
    {
        private static readonly SemanticVersion SdkVersion;

        private static readonly SemanticVersion LowestSupportedSdkVersion;

        /// <summary>
        ///     Initializes the static members of the <see cref="VersionChecker"/>
        ///     class.
        /// </summary>
        static VersionChecker()
        {
            SdkVersion = SdkAssembly.Assembly.GetSemanticVersion();

            //
            // This is the lowest version of the SDK that is
            // compatible with SdkVersion. If any breaking changes occur
            // before the 1.0 release, then this value will be updated.
            //

            LowestSupportedSdkVersion = new SemanticVersion(0, 109, 0);
        }

        protected VersionChecker()
        {
        }

        /// <summary>
        ///     Gets the Version of the SDK that is being hosted.
        /// </summary>
        public virtual SemanticVersion Sdk => SdkVersion;

        /// <summary>
        ///     Gets the minimum supported Version of the SDK.
        /// </summary>
        public virtual SemanticVersion LowestSupportedSdk => LowestSupportedSdkVersion;

        /// <summary>
        ///     Creates a new <see cref="VersionChecker"/>.
        /// </summary>
        /// <returns>
        ///     A new instance of the <see cref="VersionChecker"/> class.
        /// </returns>
        public static VersionChecker Create()
        {
            return new VersionChecker();
        }

        /// <summary>
        ///     Determines whether the given Version of the SDK is
        ///     supported by the host.
        /// </summary>
        /// <param name="candidateVersion">
        ///     The version of the SDK being used by the Plugin.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the Plugin's version of the SDK is
        ///     compatible with this version of the SDK; <c>false</c>
        ///     otherwise.
        /// </returns>
        public virtual bool IsVersionSupported(SemanticVersion candidateVersion)
        {
            Guard.NotNull(candidateVersion, nameof(candidateVersion));

            return BaselineCheck(this.LowestSupportedSdk, candidateVersion) &&
                MajorCheck(this.Sdk, candidateVersion) &&
                MinorCheck(this.Sdk, candidateVersion) &&
                PatchCheck(this.Sdk, candidateVersion);
        }

        /// <summary>
        ///     Determines if the given <see cref="Assembly"/> references
        ///     the SDK, and if so, returns the <see cref="SemanticVersion"/> of the
        ///     referenced SDK. If the given Assembly does not reference
        ///     the SDK, then <c>null</c> is returned.
        /// </summary>
        /// <param name="candidate">
        ///     The <see cref="Assembly"/> to check for a referenced SDK.
        /// </param>
        /// <returns>
        ///     The <see cref="SemanticVersion"/> of the referenced SDK, if it
        ///     exists; <c>null</c> if the SDK is not referenced.
        /// </returns>
        public virtual SemanticVersion FindReferencedSdkVersion(Assembly candidate)
        {
            Guard.NotNull(candidate, nameof(candidate));

            AssemblyName referencedSdk = candidate
                .GetReferencedAssemblies()
                .SingleOrDefault(x => x.Name.Equals(SdkAssembly.Assembly.GetName().Name));

            return referencedSdk?.GetSemanticVersion();
        }

        private static bool BaselineCheck(
            SemanticVersion baseline,
            SemanticVersion candidate)
        {
            Debug.Assert(baseline != null);
            Debug.Assert(candidate != null);

            return candidate >= baseline;
        }

        private static bool MajorCheck(
            SemanticVersion sdk,
            SemanticVersion candidate)
        {
            Debug.Assert(sdk != null);
            Debug.Assert(candidate != null);

            return candidate.Major == sdk.Major;
        }

        private static bool MinorCheck(
            SemanticVersion sdk,
            SemanticVersion candidate)
        {
            Debug.Assert(sdk != null);
            Debug.Assert(candidate != null);
            Debug.Assert(sdk.Major == candidate.Major);

            return candidate.Minor <= sdk.Minor;
        }

        private static bool PatchCheck(
            SemanticVersion sdk,
            SemanticVersion candidate)
        {
            Debug.Assert(sdk != null);
            Debug.Assert(candidate != null);
            Debug.Assert(candidate.Major == sdk.Major);
            Debug.Assert(candidate.Minor <= sdk.Minor);

            if (sdk.Major == 0)
            {
                return candidate.Patch <= sdk.Patch;
            }

            return true;
        }
    }
}

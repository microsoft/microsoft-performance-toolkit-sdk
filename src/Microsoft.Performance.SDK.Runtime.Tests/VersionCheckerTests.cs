// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class VersionCheckerTests
    {
        [TestMethod]
        public void BaseLineIs109()
        {
            var sut = CreateSut();

            Assert.AreEqual(new SemanticVersion(0, 109, 0), sut.LowestSupportedSdk);
        }

        [TestMethod]
        public void VersionLessThanBaselineRejected()
        {
            var baseline = new SemanticVersion(1, 2, 3);
            var version = new SemanticVersion(1, 2, 2);

            var sut = CreateSut(baseline: baseline);

            Assert.IsFalse(sut.IsVersionSupported(version));
        }

        [TestMethod]
        [DataRow("0.109.0", "0.109.0", "0.109.0", true)]

        // major being zero (0), anything between lowest version
        // and current version is acceptable.
        [DataRow("0.112.0", "0.109.0", "0.109.0", true)]
        [DataRow("0.112.0", "0.109.0", "0.111.0", true)]
        [DataRow("0.112.0", "0.109.0", "0.112.0", true)]
        [DataRow("0.112.0", "0.109.0", "0.112.1", false)]
        [DataRow("0.112.0", "0.109.0", "0.113.0", false)]

        // major being non-zero, must have matching major
        // and minor being <=.
        // Patches do not change the interface, so are
        // irrelevent to the checks.
        [DataRow("1.0.0", "0.109.0", "0.109.0", false)]
        [DataRow("1.0.0", "0.109.0", "0.201.0", false)]
        [DataRow("1.0.0", "0.109.0", "1.0.0", true)]
        [DataRow("1.0.0", "0.109.0", "1.0.1", true)]
        [DataRow("1.0.0", "0.109.0", "1.1.0", false)]

        [DataRow("1.3.0", "0.109.0", "1.2.0", true)]
        [DataRow("1.3.0", "0.109.0", "1.3.0", true)]
        [DataRow("1.3.0", "0.109.0", "1.3.1", true)]
        [DataRow("1.3.0", "0.109.0", "1.4.0", false)]
        [DataRow("1.3.0", "0.109.0", "2.0.0", false)]
        public void VersionSupportedTests(
            string sdkVersion,
            string baselineVersion,
            string candidateVersion,
            bool expected)
        {
            var sdkSemVer = SemanticVersion.Parse(sdkVersion);
            var baselineSemVer = SemanticVersion.Parse(baselineVersion);
            var candidateSemVer = SemanticVersion.Parse(candidateVersion);

            var sut = CreateSut(sdkVersion: sdkSemVer, baseline: baselineSemVer);
            var result = sut.IsVersionSupported(candidateSemVer);

            Assert.AreEqual(
                expected,
                result,
                "{0} SHOULD{1} HAVE BEEN SUPPORTED BY {2}",
                candidateSemVer,
                expected ? string.Empty : " NOT",
                sdkSemVer);
        }

        private static VersionChecker CreateSut(
            SemanticVersion sdkVersion = null,
            SemanticVersion baseline = null)
        {
            return new ConfigurableVersionChecker
            {
                SdkSetter = sdkVersion,
                BaselineSetter = baseline,
            };
        }
    }
}

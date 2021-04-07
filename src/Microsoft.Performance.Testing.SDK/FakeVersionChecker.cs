// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Performance.SDK.Runtime;
using NuGet.Versioning;

namespace Microsoft.Performance.Testing.SDK
{
    public class FakeVersionChecker
        : VersionChecker
    {
        public override SemanticVersion Sdk { get; } = new SemanticVersion(1, 0, 0);

        public override SemanticVersion LowestSupportedSdk { get; } = new SemanticVersion(1, 0, 0);

        public override SemanticVersion FindReferencedSdkVersion(Assembly candidate)
        {
            return this.Sdk;
        }

        public override bool IsVersionSupported(SemanticVersion candidateVersion)
        {
            return true;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;
using NuGet.Versioning;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests;

internal static class FakeMetadata
{
    public static PluginMetadata GetFakeMetadataWithOnlyIdentityAndSdkVersion()
    {
        return new PluginMetadata(
            new PluginIdentity("fake_id", SemanticVersion.Parse("1.0.0")),
            0,
            null,
            null,
            SemanticVersion.Parse("1.0.0"),
            null,
            null);
    }
}

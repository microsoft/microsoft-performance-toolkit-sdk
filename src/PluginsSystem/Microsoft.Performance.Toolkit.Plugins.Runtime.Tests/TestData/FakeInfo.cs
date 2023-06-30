// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests;

internal static class FakeInfo
{
    public static PluginMetadata GetFakePluginInfoWithOnlyIdentityAndSdkVersion()
    {
        return new PluginMetadata(
            new PluginIdentity("fake_id", new Version("1.0.0")),
            0,
            null,
            null,
            new Version("1.0.0"),
            null);
    }
}
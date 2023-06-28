// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests;

internal static class FakeInfo
{
    public static PluginInfo GetFakePluginInfoWithOnlyIdentityAndSdkVersion()
    {
        return new PluginInfo(
            new PluginIdentity("fake_id", new Version("1.0.0")),
            0,
            null,
            null,
            new Version("1.0.0"),
            null);
    }
}
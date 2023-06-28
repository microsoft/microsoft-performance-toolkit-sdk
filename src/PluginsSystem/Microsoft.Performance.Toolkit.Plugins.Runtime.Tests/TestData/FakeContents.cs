// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Packaging.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    internal static class FakeContents
    {
        public static PluginContents GetFakeEmptyPluginContents()
        {
            return new PluginContents(
                null,
                null,
                null);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Package.Tests
{
    internal static class FakeContentsMetadata
    {
        public static PluginContentsMetadata GetFakeEmptyPluginContentsMetadata()
        {
            return new PluginContentsMetadata(
                null,
                null,
                null);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Metadata;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    internal static class FakeContents
    {
        public static PluginContentsMetadata GetFakeEmptyPluginContentsInfo()
        {
            return new PluginContentsMetadata(
                null,
                null,
                null);
        }
    }
}

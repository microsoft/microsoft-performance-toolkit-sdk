// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Mocks
{
    public static class FakeFetcher
    {
        public static Mock<IPluginFetcher> Create()
        {
            var mockFetcher = new Mock<IPluginFetcher>();
            return mockFetcher;
        }

        public static Mock<IPluginFetcher> Create(bool isSupported)
        {
            var mockFetcher = new Mock<IPluginFetcher>();
            mockFetcher.Setup(x => x.IsSupportedAsync(It.IsAny<AvailablePluginInfo>()).Result).Returns(isSupported);

            return mockFetcher;
        }
    }
}

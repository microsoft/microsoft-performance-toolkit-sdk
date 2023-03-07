// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Moq;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests.Mocks
{
    public static class FakeDiscovererProvider
    {
        public static Mock<IPluginDiscovererProvider> Create()
        {
            var mockDiscovererProvider = new Mock<IPluginDiscovererProvider>();

            return mockDiscovererProvider;
        }

        public static Mock<IPluginDiscovererProvider> Create(
           bool isSupported)
        {
            return Create(new Mock<IPluginDiscoverer>(), isSupported);
        }


        public static Mock<IPluginDiscovererProvider> Create(
            Mock<IPluginDiscoverer> mockPluginDiscoverer,
            bool isSupported = true)
        {
            var mockDiscovererProvider = new Mock<IPluginDiscovererProvider>();
            
            mockDiscovererProvider.Setup(
                x => x.IsSupportedAsync(It.IsAny<PluginSource>()).Result).Returns(isSupported);
            
            mockDiscovererProvider.Setup(
                x => x.CreateDiscoverer(It.IsAny<PluginSource>())).Returns(mockPluginDiscoverer.Object);

            return mockDiscovererProvider;
        }
    }
}

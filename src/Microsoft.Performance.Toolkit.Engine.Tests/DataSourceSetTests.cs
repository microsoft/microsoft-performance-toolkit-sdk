// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    public class DataSourceSetTests
        : EngineFixture
    {
        [TestMethod]
        public void Create_DoesOwns_Disposes()
        {
            using (var plugins = PluginSet.Load())
            {
                using (var sut = DataSourceSet.Create(plugins))
                {

                }
            }

            using (var plugins = PluginSet.Load())
            {
                using (var sut = DataSourceSet.Create(plugins, true))
                {

                }
            }
        }

        [TestMethod]
        public void Create_DoesNotOwn_DoesNotDispose()
        {
            using (var plugins = PluginSet.Load())
            {
                using (var sut = DataSourceSet.Create(plugins, false))
                {

                }
            }
        }
    }
}

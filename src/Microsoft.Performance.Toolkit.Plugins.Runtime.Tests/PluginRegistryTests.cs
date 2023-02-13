// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class PluginRegistryTests
    {
        private static string? registryRoot;

        private PluginRegistry? Sut { get; set; }

        [TestInitialize]
        public void Setup()
        {
            registryRoot = Path.GetTempFileName();
            this.Sut = new PluginRegistry(registryRoot);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                File.Delete(registryRoot);
            }
            catch
            {
            }
        }

        [TestMethod]
        [UnitTest]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_PathNotRooted_Throws()
        {
            string withoutRoot = registryRoot.Substring(Path.GetPathRoot(registryRoot).Length);
            new PluginRegistry(withoutRoot);
        }
    }
}

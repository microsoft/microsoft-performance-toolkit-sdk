// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class ToolkitEngineCreateInfoTests
    {
        private static readonly string ScratchDir = Path.Combine(
            Environment.CurrentDirectory, nameof(EngineCreateInfo) + "_SCRATCH");

        private EngineCreateInfo Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Directory.CreateDirectory(ScratchDir);
            this.Sut = new EngineCreateInfo();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(ScratchDir, true);
        }

        [TestMethod]
        [IntegrationTest]
        public void Ctor_ExtensionDirectory_IsCurrent()
        {
            Assert.AreEqual(Environment.CurrentDirectory, this.Sut.ExtensionDirectory);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_Null_SetToCurrent()
        {
            this.Sut.ExtensionDirectory = ScratchDir;
            this.Sut.ExtensionDirectory = null;

            Assert.AreEqual(Environment.CurrentDirectory, this.Sut.ExtensionDirectory);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_ExistingDirectory_Sets()
        {
            this.Sut.ExtensionDirectory = ScratchDir;

            Assert.AreEqual(ScratchDir, this.Sut.ExtensionDirectory);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathIsAFile_Throws()
        {
            var file = Path.Combine(ScratchDir, "test.txt");
            File.WriteAllText(file, "test");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(
                () => this.Sut.ExtensionDirectory = file);
            Assert.AreEqual(file, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathDoesNotExist_Throws()
        {
            var doesNotExist = Path.Combine(ScratchDir, "doesNotExist");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(
                () => this.Sut.ExtensionDirectory = doesNotExist);
            Assert.AreEqual(doesNotExist, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_RelativePath_Expands()
        {
            var relative = Path.GetFileName(ScratchDir);

            this.Sut.ExtensionDirectory = relative;

            Assert.AreEqual(ScratchDir, this.Sut.ExtensionDirectory);
        }
    }
}

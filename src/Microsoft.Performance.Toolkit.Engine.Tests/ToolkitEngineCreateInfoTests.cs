// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
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

            var dir1 = Path.Combine(ScratchDir, "Dir1");
            var dir2 = Path.Combine(ScratchDir, "Dir2");
            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);
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
            this.Sut = new EngineCreateInfo();
            Assert.AreEqual(1, this.Sut.ExtensionDirectories.Count());
            Assert.AreEqual(Environment.CurrentDirectory, this.Sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_ExistingDirectory_Sets()
        {
            this.Sut = new EngineCreateInfo(ScratchDir);

            Assert.AreEqual(1, this.Sut.ExtensionDirectories.Count());
            Assert.AreEqual(ScratchDir, this.Sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathIsAFile_Throws()
        {
            var file = Path.Combine(ScratchDir, "test.txt");
            File.WriteAllText(file, "test");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(
                () => new EngineCreateInfo(file));
            Assert.AreEqual(file, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathDoesNotExist_Throws()
        {
            var doesNotExist = Path.Combine(ScratchDir, "doesNotExist");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(
                () => new EngineCreateInfo(doesNotExist));
            Assert.AreEqual(doesNotExist, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_RelativePath_Expands()
        {
            var relative = Path.GetFileName(ScratchDir);

            this.Sut = new EngineCreateInfo(relative);

            Assert.AreEqual(1, this.Sut.ExtensionDirectories.Count());
            Assert.AreEqual(ScratchDir, this.Sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectories_MultipePaths_Sets()
        {
            var dir1 = Path.Combine(ScratchDir, "Dir1");
            var dir2 = Path.Combine(ScratchDir, "Dir2");

            this.Sut = new EngineCreateInfo(new[] { dir1, dir2 });

            Assert.AreEqual(2, this.Sut.ExtensionDirectories.Count());
            Assert.IsTrue(this.Sut.ExtensionDirectories.Contains(dir1));
            Assert.IsTrue(this.Sut.ExtensionDirectories.Contains(dir2));
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectories_AnInvalidPath_Throws()
        {
            var dir1 = Path.Combine(ScratchDir, "Dir1");
            var dirDoesNotExist = Path.Combine(ScratchDir, "doesNotExist");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(
                () => new EngineCreateInfo(new[] { dir1, dirDoesNotExist }));
            Assert.AreEqual(dirDoesNotExist, e.Path);
        }
    }
}

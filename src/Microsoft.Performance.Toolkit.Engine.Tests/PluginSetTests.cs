﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class PluginSetTests
        : EngineFixture
    {
        [TestMethod]
        [IntegrationTest]
        public void Ctor_ExtensionDirectory_IsCurrent()
        {
            using var sut = PluginSet.Load();

            Assert.AreEqual(1, sut.ExtensionDirectories.Count());
            Assert.AreEqual(Environment.CurrentDirectory, sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_ExistingDirectory_Sets()
        {
            using var sut = PluginSet.Load(this.Scratch.FullName);

            Assert.AreEqual(1, sut.ExtensionDirectories.Count());
            Assert.AreEqual(this.Scratch.FullName, sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathIsAFile_Throws()
        {
            var file = Path.Combine(this.Scratch.FullName, "test.txt");
            File.WriteAllText(file, "test");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(() => PluginSet.Load(file));
            Assert.AreEqual(file, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_PathDoesNotExist_Throws()
        {
            var doesNotExist = Path.Combine(this.Scratch.FullName, "doesNotExist");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(() => PluginSet.Load(doesNotExist));
            Assert.AreEqual(doesNotExist, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_RelativePath_Expands()
        {
            var relative = "Relative";
            var relativeFull = Path.GetFullPath(relative);
            try
            {
                Directory.CreateDirectory(relative);

                using var sut = PluginSet.Load(relative);

                Assert.AreEqual(1, sut.ExtensionDirectories.Count());
                Assert.AreEqual(relativeFull, sut.ExtensionDirectories.First());
            }
            finally
            {
                Directory.Delete(relativeFull, true);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectories_MultipePaths_Sets()
        {
            var dir1 = this.Scratch.CreateSubdirectory("Dir1");
            var dir2 = this.Scratch.CreateSubdirectory("Dir2");


            using var sut = PluginSet.Load(new[] { dir1.FullName, dir2.FullName });

            Assert.AreEqual(2, sut.ExtensionDirectories.Count());
            Assert.IsTrue(sut.ExtensionDirectories.Contains(dir1.FullName));
            Assert.IsTrue(sut.ExtensionDirectories.Contains(dir2.FullName));
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectories_AnInvalidPath_Throws()
        {
            var dir1 = this.Scratch.CreateSubdirectory("Dir1");
            var dirDoesNotExist = Path.Combine(this.Scratch.FullName, "doesNotExist");

            var e = Assert.ThrowsException<InvalidExtensionDirectoryException>(() => PluginSet.Load(new[] { dir1.FullName, dirDoesNotExist }));
            Assert.AreEqual(dirDoesNotExist, e.Path);
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_LoadsFromExtensionPath()
        {
            var expectedSourceCookerPath = Source1DataCooker.DataCookerPath;

            using var sut = PluginSet.Load();

            Assert.IsTrue(sut.ProcessingSourceReferences.Any());
            Assert.IsTrue(sut.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));
            Assert.IsTrue(sut.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_MultipleCallsWithDifferentPath_LoadsFromExtensionPath()
        {
            var expectedSourceCookerPath = Source1DataCooker.DataCookerPath;

            using var firstEngine = PluginSet.Load();
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));
            Assert.IsTrue(firstEngine.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
            var firstInstances = firstEngine.ProcessingSourceReferences.Where(x => x.Instance is Source123DataSource).ToList();

            var tempDir = this.Scratch.CreateSubdirectory(nameof(Load_MultipleCallsWithDifferentPath_LoadsFromExtensionPath));

            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            CopyAssemblyContainingType(typeof(FakePlugin), tempDir);

            // we loaded from an assembly in a different folder, so the type is 'technically' different, so do a name
            // compare this time.
            using var newEngine = PluginSet.Load(tempDir.FullName);

            Assert.IsTrue(newEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(newEngine.ProcessingSourceReferences.Any(x => x.Instance.GetType().Name == typeof(Source123DataSource).Name));
            Assert.IsTrue(newEngine.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
            var secondInstances = newEngine.ProcessingSourceReferences.Where(x => x.Instance.GetType().Name == typeof(Source123DataSource).Name).ToList();

            foreach (var instance in firstInstances)
            {
                Assert.IsFalse(secondInstances.Any(x => ReferenceEquals(x, instance)));
            }

            foreach (var instance in secondInstances)
            {
                Assert.IsFalse(firstInstances.Any(x => ReferenceEquals(x, instance)));
            }
        }

        [ProcessingSource(
            "{645AB037-A325-45EC-9DB0-B8086A83B528}",
            nameof(FakePlugin),
            "Source for Tests")]
        [FileDataSource(Extension)]
        private sealed class FakePlugin
            : ProcessingSource
        {
            public const string Extension = ".txt";

            protected override ICustomDataProcessor CreateProcessorCore(
                IEnumerable<IDataSource> dataSources,
                IProcessorEnvironment processorEnvironment,
                ProcessorOptions options)
            {
                throw new NotImplementedException();
            }

            protected override bool IsDataSourceSupportedCore(IDataSource dataSource)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(
                    Extension,
                    Path.GetExtension(dataSource.Uri.LocalPath));
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestPluginAssembly1;
using TestPluginAssembly2;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class PluginSetTests
        : EngineFixture
    {
        private static string TestMethodName
        {
            get
            {
                var stackTrace = new StackTrace();
                return stackTrace.GetFrame(1).GetMethod().Name;
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Ctor_ExtensionDirectory_IsCurrent()
        {
            var sut = PluginSet.Load();

            Assert.AreEqual(1, sut.ExtensionDirectories.Count());
            Assert.AreEqual(Environment.CurrentDirectory, sut.ExtensionDirectories.First());
        }

        [TestMethod]
        [IntegrationTest]
        public void ExtensionDirectory_ExistingDirectory_Sets()
        {
            var sut = PluginSet.Load(this.Scratch.FullName);

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

                var sut = PluginSet.Load(relative);

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


            var sut = PluginSet.Load(new[] { dir1.FullName, dir2.FullName });

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

            var tempDir = this.Scratch.CreateSubdirectory(TestMethodName);

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

        [TestMethod]
        [IntegrationTest]
        public void Load_SubDirectoryIncluded_LoadsFromExtensionPath()
        {
            DirectoryInfo includedDir = this.Scratch.CreateSubdirectory(TestMethodName + @"\Included");
            DirectoryInfo targetDir = Directory.GetParent(includedDir.FullName);

            CopyAssemblyContainingType(typeof(FakePlugin1), includedDir);

            var discoverySettings = new AssemblyDiscoverySettings(true, null, null, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_SubDirectoryNotIncluded_SkipsSubDirectory()
        {
            DirectoryInfo notIncludedDir = this.Scratch.CreateSubdirectory(TestMethodName + @"\NotIncluded");
            DirectoryInfo targetDir = Directory.GetParent(notIncludedDir.FullName);

            CopyAssemblyContainingType(typeof(FakePlugin1), notIncludedDir);

            var discoverySettings = new AssemblyDiscoverySettings(false, null, null, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsFalse(firstEngine.ProcessingSourceReferences.Any());
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_SearchPatternExe_SkipsDll()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);

            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);
            string plugin2ExePath = plugin2Path.Replace(".dll", ".exe");
            File.Move(plugin2Path, plugin2ExePath);

            var discoverySettings = new AssemblyDiscoverySettings(true, new[] { "*.exe" }, null, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
            Assert.IsFalse(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_SearchPatternDll_LoadsFromExtensionPath()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);

            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);
            string plugin2ExePath = plugin2Path.Replace(".dll", ".exe");
            File.Move(plugin2Path, plugin2ExePath);

            var discoverySettings = new AssemblyDiscoverySettings(true, new[] { "*.dll" }, null, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
            Assert.IsFalse(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_SearchPatternDllExe_LoadsFromExtensionPath()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);

            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);
            string plugin2ExePath = plugin2Path.Replace(".dll", ".exe");
            File.Move(plugin2Path, plugin2ExePath);

            var discoverySettings = new AssemblyDiscoverySettings(true, new[] { "*.dll", "*.exe" }, null, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_ExcludeFakePlugin2_ExcludesFakePlugin2()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);
            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);

            var discoverySettings = new AssemblyDiscoverySettings(true, null, new[] { Path.GetFileName(plugin2Path) }, MatchCasing.CaseInsensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
            Assert.IsFalse(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_ExcludeCaseSensitive_ExcludesFakePlugin2()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);
            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);

            var discoverySettings = new AssemblyDiscoverySettings(true, null, new[] { Path.GetFileName(plugin2Path) }, MatchCasing.CaseSensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
            Assert.IsFalse(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
        }

        [TestMethod]
        [IntegrationTest]
        public void Load_ExcludeCaseSensitive_LoadsFakePlugin2()
        {
            DirectoryInfo targetDir = this.Scratch.CreateSubdirectory(TestMethodName);

            CopyAssemblyContainingType(typeof(FakePlugin1), targetDir);
            string plugin2Path = CopyAssemblyContainingType(typeof(FakePlugin2), targetDir);

            var discoverySettings = new AssemblyDiscoverySettings(true, null, new[] { Path.GetFileName(plugin2Path).ToUpper() }, MatchCasing.CaseSensitive);

            using var firstEngine = PluginSet.Load(new[] { targetDir.FullName }, null, discoverySettings, null);
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any());
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin1));
            Assert.IsTrue(firstEngine.ProcessingSourceReferences.Any(x => x.Instance is FakePlugin2));
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

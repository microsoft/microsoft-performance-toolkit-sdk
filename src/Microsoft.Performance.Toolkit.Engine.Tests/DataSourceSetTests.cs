// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class DataSourceSetTests
        : EngineFixture
    {
        private DataSourceSet Sut { get; set; }

        public override void OnInitialize()
        {
            base.OnInitialize();

            this.Sut = DataSourceSet.Create();
        }

        public override void OnCleanup()
        {
            this.Sut?.Dispose();

            base.OnCleanup();
        }

        #region AddDataSource

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_NullFile_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSource(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_NullType_Throws()
        {
            var file = CreateTestFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSource(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_NoInstancesOfDataSourceLoaded_Throws()
        {
            var typeAssembly = typeof(Source123DataSource).Assembly;
            var engineAssemblies = System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies.Where(x =>
                x.GetName().Name.StartsWith("Microsoft.Performance.Toolkit.Engine"));
            var processingSources = this.Sut.Plugins.ProcessingSourceReferences.ToList();
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedProcessingSourceException>(() => this.Sut.AddDataSource(file, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedProcessingSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_DataSourceDoesNotSupportFile_Throws()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSource(file, typeof(Source123DataSource)));
            Assert.AreEqual(file.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source123DataSource).FullName, e.RequestedProcessingSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file1, typeof(Source123DataSource))
                    .AddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSourceOnly_Null_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSource(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSourceOnly_NoCookersOrDataSourcesSupport_Throws()
        {
            var tempDir = this.Scratch.CreateSubdirectory(nameof(AddDataSourceOnly_NoCookersOrDataSourcesSupport_Throws));

            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            using (var plugins = PluginSet.Load(tempDir.FullName))
            using (var sut = DataSourceSet.Create(plugins))
            {

                var file = CreateTestFile(".380298502");
                var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSource(file));
                Assert.AreEqual(file.Uri.ToString(), e.DataSource);
                Assert.IsNull(e.RequestedProcessingSource);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSourceOnly_AtLeastOnSourceSupports_Adds()
        {
            var file = CreateTestFile(Source123DataSource.Extension);
            this.Sut.AddDataSource(file);

            Assert.AreEqual(1, this.Sut.FreeDataSourcesToProcess.Count());
            Assert.AreEqual(file, this.Sut.FreeDataSourcesToProcess.ElementAt(0));
        }

        #endregion

        #region TryAddDataSource

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_Null_False()
        {
            Assert.IsFalse(this.Sut.TryAddDataSource(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_NoCookersOrDataSourcesSupport_False()
        {
            var tempDir = this.Scratch.CreateSubdirectory(nameof(TryAddDataSourceOnly_NoCookersOrDataSourcesSupport_False));
            
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            using (var plugins = PluginSet.Load(tempDir.FullName))
            using (var sut = DataSourceSet.Create(plugins))
            {
                var file = CreateTestFile(".380298502");
                Assert.IsFalse(sut.TryAddDataSource(file));
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_AtLeastOnSourceSupports_Adds()
        {
            var file = CreateTestFile(Source123DataSource.Extension);
            this.Sut.TryAddDataSource(file);

            Assert.AreEqual(1, this.Sut.FreeDataSourcesToProcess.Count());
            Assert.AreEqual(file, this.Sut.FreeDataSourcesToProcess.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_AtLeastOnSourceSupports_True()
        {
            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsTrue(this.Sut.TryAddDataSource(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NullFile_False()
        {
            Assert.IsFalse(this.Sut.TryAddDataSource(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NullType_False()
        {
            var file = CreateTestFile(".txt");
            Assert.IsFalse(this.Sut.TryAddDataSource(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NoInstancesOfDataSourceLoaded_False()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSource(file, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_DataSourceDoesNotSupportFile_False()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(".380298502");
            Assert.IsFalse(this.Sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedFile = file;
            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(expectedFile, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_True()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsTrue(this.Sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);

            this.Sut.TryAddDataSource(file1, typeof(Source123DataSource));
            this.Sut.TryAddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1][0]);
        }

        #endregion

        #region AddDataSources

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NullFiles_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSources(null, typeof(Source123DataSource)));
        }


        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_EmptyFiles_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => this.Sut.AddDataSources(new IDataSource[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_ContainsNullFiles_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSources(new[] { (IDataSource)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NullType_Throws()
        {
            var file = CreateTestFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => this.Sut.AddDataSources(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NoInstancesOfDataSourceLoaded_Throws()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedProcessingSourceException>(() => this.Sut.AddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedProcessingSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_DataSourceDoesNotSupportFile_Throws()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source4DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));

            Assert.AreEqual(file1.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source4DataSource).FullName, e.RequestedProcessingSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var files = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.AddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(files.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0].Count);

            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var files1 = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            var files2 = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.AddDataSources(files1, typeof(Source123DataSource))
                    .AddDataSources(files2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, this.Sut.AssignedDataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        #endregion

        #region TryAddDataSources

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NullFiles_False()
        {
            Assert.IsFalse(this.Sut.TryAddDataSources(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NoFiles_False()
        {
            Assert.IsFalse(this.Sut.TryAddDataSources(new IDataSource[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_ContainsNullFiles_False()
        {
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { (IDataSource)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NullType_False()
        {
            var file = CreateTestFile(".txt");
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NoInstancesOfDataSourceLoaded_False()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_DataSourceDoesNotSupportFile_False()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var files = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.TryAddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FileSupportedByAtLeastOneDataSource_True()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var files = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            Assert.IsTrue(this.Sut.TryAddDataSources(files, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            Assert.IsTrue(this.Sut.Plugins.ProcessingSourceReferences.Any(x => x.Instance is Source123DataSource));

            var files1 = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            var files2 = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.TryAddDataSources(files1, typeof(Source123DataSource));
            this.Sut.TryAddDataSources(files2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.Plugins.ProcessingSourceReferences.Single(x => x.Instance is Source123DataSource);

            Assert.AreEqual(1, this.Sut.AssignedDataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.AssignedDataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, this.Sut.AssignedDataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], this.Sut.AssignedDataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        #endregion

        private IDataSource CreateTestFile(string extension)
        {
            var path = Any.FileOnDisk(extension, this.Scratch.FullName);
            return new FileDataSource(path);
        }
    }
}

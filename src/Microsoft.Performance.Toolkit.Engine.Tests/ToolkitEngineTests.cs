// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source123;
using Microsoft.Performance.Toolkit.Engine.Tests.TestCookers.Source4;
using Microsoft.Performance.Toolkit.Engine.Tests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests
{
    [TestClass]
    public class ToolkitEngineTests
    {
        private static readonly string ScratchDirectory =
            Path.GetFullPath(nameof(ToolkitEngineTests) + "_SCRATCH");

        public TestContext TestContext { get; set; }

        private Engine Sut { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            try
            {
                Directory.Delete(ScratchDirectory, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception e)
            {
                this.TestContext.WriteLine("cannot delete {0}: {1}", ScratchDirectory, e);
            }

            try
            {
                Directory.CreateDirectory(ScratchDirectory);
            }
            catch (Exception e)
            {
                this.TestContext.WriteLine("cannot delete {0}: {1}", ScratchDirectory, e);
                Assert.Fail("Unable to initialize the test directory.");
            }

            this.Sut = Engine.Create(
                new EngineCreateInfo
                {
                    Versioning = new FakeVersionChecker(),
                });
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                Directory.Delete(ScratchDirectory, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception e)
            {
                this.TestContext.WriteLine("cannot delete {0}: {1}", ScratchDirectory, e);
            }
            finally
            {
                this.Sut.Dispose();
            }
        }

        #region Create

        [TestMethod]
        [IntegrationTest]
        public void Create_NoParameters_UsesCurrentDirectory()
        {
            Assert.AreEqual(Environment.CurrentDirectory, this.Sut.ExtensionDirectory);
        }

        [TestMethod]
        [UnitTest]
        public void Create_NullCreateInfo_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Engine.Create(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_IsProcessed_False()
        {
            Assert.IsFalse(this.Sut.IsProcessed);
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_LoadsFromExtensionPath()
        {
            RunTestInDomain<Serializable_Create_LoadsFromExtensionPath>();
        }

        [Serializable]
        private sealed class Serializable_Create_LoadsFromExtensionPath
            : SerializableTest
        {
            protected override void RunCore()
            {
                var expectedSourceCookerPath = new DataCookerPath("SourceId", "CookerId");

                var engine = Engine.Create();

                Assert.IsTrue(engine.CustomDataSources.Any());
                Assert.IsTrue(engine.CustomDataSources.Any(x => x is Source123DataSource));
                Assert.IsTrue(engine.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Create_MultipleCallsWithDifferentPath_LoadsFromExtensionPath()
        {
            RunTestInDomain<Serializable_Create_MultipleCallsWithDifferentPath_LoadsFromExtensionPath>();
        }

        [Serializable]
        private sealed class Serializable_Create_MultipleCallsWithDifferentPath_LoadsFromExtensionPath
            : SerializableTest
        {
            protected override void RunCore()
            {
                var expectedSourceCookerPath = Source1DataCooker.DataCookerPath;


                var engine = Engine.Create();
                Assert.IsTrue(engine.CustomDataSources.Any());
                Assert.IsTrue(engine.CustomDataSources.Any(x => x is Source123DataSource));
                Assert.IsTrue(engine.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
                var firstInstances = engine.CustomDataSources.Where(x => x is Source123DataSource).ToList();

                var tempDir = Path.Combine(ScratchDirectory, nameof(Serializable_Create_MultipleCallsWithDifferentPath_LoadsFromExtensionPath));
                Directory.CreateDirectory(tempDir);

                CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
                CopyAssemblyContainingType(typeof(FakeCustomDataSource), tempDir);

                // we loaded from an assembly in a different folder, so the type is 'technically' different, so do a name
                // compare this time.
                engine = Engine.Create(
                    new EngineCreateInfo
                    {
                        ExtensionDirectory = tempDir,
                        Versioning = new FakeVersionChecker(),
                    });
                Assert.IsTrue(engine.CustomDataSources.Any());
                Assert.IsTrue(engine.CustomDataSources.Any(x => x.GetType().Name == typeof(Source123DataSource).Name));
                Assert.IsTrue(engine.SourceDataCookers.Any(x => x == expectedSourceCookerPath));
                var secondInstances = engine.CustomDataSources.Where(x => x.GetType().Name == typeof(Source123DataSource).Name).ToList();

                foreach (var instance in firstInstances)
                {
                    Assert.IsFalse(secondInstances.Any(x => ReferenceEquals(x, instance)));
                }

                foreach (var instance in secondInstances)
                {
                    Assert.IsFalse(firstInstances.Any(x => ReferenceEquals(x, instance)));
                }
            }
        }

        #endregion Create

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
            var customDataSources = this.Sut.CustomDataSources.ToList();
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedCustomDataSourceException>(() => this.Sut.AddDataSource(file, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_DataSourceDoesNotSupportFile_Throws()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSource(file, typeof(Source123DataSource)));
            Assert.AreEqual(file.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source123DataSource).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file, this.Sut.DataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file1, typeof(Source123DataSource));
            this.Sut.AddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, this.Sut.DataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, this.Sut.DataSourcesToProcess[expectedDataSource][1][0]);
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
            var tempDir = Path.Combine(ScratchDirectory, "Serializable_AddDataSourceOnly_NoCookersOrDataSourcesSupport_Throws");
            Directory.CreateDirectory(tempDir);
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            var sut = Engine.Create(
                new EngineCreateInfo
                {
                    ExtensionDirectory = tempDir,
                    Versioning = new FakeVersionChecker(),
                });

            var file = CreateTestFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSource(file));
            Assert.AreEqual(file.Uri.ToString(), e.DataSource);
            Assert.IsNull(e.RequestedCustomDataSource);
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

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_AlreadyProcessed_Throws()
        {
            this.Sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => this.Sut.AddDataSource(Any.DataSource()));
            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => this.Sut.AddDataSource(Any.DataSource(), this.Sut.CustomDataSources.First().GetType()));
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
            var tempDir = Path.Combine(ScratchDirectory, "TryAddDataSourceOnly_NoCookersOrDataSourcesSupport_False");
            Directory.CreateDirectory(tempDir);
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            using (var sut = Engine.Create(
                new EngineCreateInfo
                {
                    ExtensionDirectory = tempDir,
                    Versioning = new FakeVersionChecker(),
                }))
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
        public void TryAddDataSourceOnly_AlreadyProcessed_False()
        {
            this.Sut.Process();

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsFalse(this.Sut.TryAddDataSource(file));
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
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSource(file, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_DataSourceDoesNotSupportFile_False()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(".380298502");
            Assert.IsFalse(this.Sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            this.Sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedFile = file;
            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(expectedFile, this.Sut.DataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_True()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsTrue(this.Sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);

            this.Sut.TryAddDataSource(file1, typeof(Source123DataSource));
            this.Sut.TryAddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, this.Sut.DataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, this.Sut.DataSourcesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_AlreadyProcessed_False()
        {
            this.Sut.Process();

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsFalse(this.Sut.TryAddDataSource(file, typeof(Source123DataSource)));
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
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedCustomDataSourceException>(() => this.Sut.AddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_DataSourceDoesNotSupportFile_Throws()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source4DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => this.Sut.AddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));

            Assert.AreEqual(file1.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source4DataSource).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.AddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(files.Length, this.Sut.DataSourcesToProcess[expectedDataSource][0].Count);

            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], this.Sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

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

            this.Sut.AddDataSources(files1, typeof(Source123DataSource));
            this.Sut.AddDataSources(files2, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, this.Sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, this.Sut.DataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], this.Sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, this.Sut.DataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], this.Sut.DataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_AlreadyProcessed_Throws()
        {
            this.Sut.Process();

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => this.Sut.AddDataSources(new[] { file, }, typeof(Source123DataSource)));
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
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_DataSourceDoesNotSupportFile_False()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = CreateTestFile(Source123DataSource.Extension);
            var file2 = CreateTestFile(Source123DataSource.Extension);
            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
                CreateTestFile(Source123DataSource.Extension),
            };

            this.Sut.TryAddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(1, this.Sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files.Length, this.Sut.DataSourcesToProcess[expectedDataSource][0].Count); 
            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], this.Sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FileSupportedByAtLeastOneDataSource_True()
        {
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

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
            Assert.IsTrue(this.Sut.CustomDataSources.Any(x => x is Source123DataSource));

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

            var expectedDataSource = this.Sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, this.Sut.DataSourcesToProcess.Count);
            Assert.IsTrue(this.Sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, this.Sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, this.Sut.DataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], this.Sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, this.Sut.DataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], this.Sut.DataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_AlreadyProcessed_False()
        {
            this.Sut.Process();

            var file = CreateTestFile(Source123DataSource.Extension);

            Assert.IsFalse(this.Sut.TryAddDataSources(new[] { file, }, typeof(Source123DataSource)));
        }

        #endregion

        #region Enable Cooker

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_Known_Enables()
        {
            var cooker = this.Sut.AllCookers.FirstOrDefault();

            this.Sut.EnableCooker(cooker);

            Assert.AreEqual(1, this.Sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, this.Sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_NotKnown_Throws()
        {
            var cooker = new DataCookerPath("not-there-id");

            var e = Assert.ThrowsException<CookerNotFoundException>(() => this.Sut.EnableCooker(cooker));
            Assert.AreEqual(cooker, e.DataCookerPath);
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_AlreadyProcessed_Throws()
        {
            this.Sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => this.Sut.EnableCooker(this.Sut.AllCookers.First()));
        }

        #endregion Enable Cooker

        #region TryEnableCooker

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_Enables()
        {
            var cooker = this.Sut.AllCookers.FirstOrDefault();

            this.Sut.TryEnableCooker(cooker);

            Assert.AreEqual(1, this.Sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, this.Sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_True()
        {
            var cooker = this.Sut.AllCookers.FirstOrDefault();

            Assert.IsTrue(this.Sut.TryEnableCooker(cooker));

            Assert.AreEqual(1, this.Sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, this.Sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_NotKnown_False()
        {
            var cooker = new DataCookerPath("not-there-id");

            Assert.IsFalse(this.Sut.TryEnableCooker(cooker));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_AlreadyProcessed_False()
        {
            this.Sut.Process();

            Assert.IsFalse(this.Sut.TryEnableCooker(this.Sut.AllCookers.First()));
        }

        #endregion TryEnableCooker

        #region Process

        [TestMethod]
        [FunctionalTest]
        public void Process_WhenComplete_IsProcessedSetToTrue()
        {
            this.Sut.Process();

            Assert.IsTrue(this.Sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_NothingEnabled_DoesNothing()
        {
            var results = this.Sut.Process();

            Assert.IsNotNull(results);
            Assert.IsTrue(this.Sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_AlreadyProcessed_Throws()
        {
            this.Sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => this.Sut.Process());
        }

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/ProcessTestSuite.json")]
        [DynamicData(nameof(ProcessTestData), DynamicDataSourceType.Method)]
        public void Process_WhenComplete_DataWasProcessed(
            EngineProcessTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }

            foreach (var cooker in testCase.CookersToEnable)
            {
                var cookerPath = DataCookerPath.Parse(cooker);
                Assert.IsTrue(this.Sut.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var file in testCase.FilePaths)
            {
                this.Sut.AddDataSource(new FileDataSource(file));
            }

            var results = this.Sut.Process();

            foreach (var expectedData in testCase.ExpectedOutputs)
            {
                var dataOutputPathRaw = expectedData.Key;
                var expectedDataPoints = expectedData.Value;
                var dataOutputPath = DataOutputPath.Create(dataOutputPathRaw);

                Assert.IsTrue(
                    results.TryQueryOutput(dataOutputPath, out object data), "Output for {0} not found.", dataOutputPathRaw);
                Assert.IsNotNull(data, "output for {0} was null ???", dataOutputPathRaw);

                var enumerableData = data as IEnumerable;
                Assert.IsNotNull(
                    enumerableData,
                    "Test output data must implement IEnumerable<> (type wasn't enumerable): '{0}'",
                    data.GetType());
                var enumerableType = enumerableData.GetType();
                var eInterface = enumerableType.GetInterface(typeof(IEnumerable<>).Name);
                Assert.IsNotNull(
                    eInterface,
                    "Test output data must implement IEnumerable<> (interface wasn't found): {0}",
                    string.Join(", ", data.GetType().GetInterfaces().Select(x => x.FullName)));
                var collectionType = eInterface.GetGenericArguments()[0];
                Assert.IsNotNull(collectionType, "Unable to retrieve collection type for {0}", data.GetType());

                var enumeratedData = new List<object>();
                foreach (var o in enumerableData)
                {
                    enumeratedData.Add(o);
                }

                Assert.AreEqual(
                    expectedDataPoints.Length, 
                    enumeratedData.Count,
                    "The processor did not process the correct amount of data: {0}",
                    dataOutputPath);

                var properties = collectionType.GetProperties()
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                for (var i = 0; i < expectedDataPoints.Length; ++i)
                {
                    var expectedObject = expectedDataPoints[i];
                    var actualObject = enumeratedData[i];
                    foreach (var kvp in expectedObject)
                    {
                        var propertyName = kvp.Key;
                        var expectedValue = kvp.Value;

                        Assert.IsTrue(properties.TryGetValue(propertyName, out PropertyInfo property));
                        var actualValue = property.GetValue(actualObject)?.ToString();
                        Assert.AreEqual(expectedValue, actualValue, propertyName);
                    }
                }
            }

            foreach (var dataOutputPathRaw in testCase.ThrowingOutputs)
            {
                var dataOutputPath = DataOutputPath.Create(dataOutputPathRaw);
                Assert.IsFalse(
                    results.TryQueryOutput(dataOutputPath, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
            }
        }

        private static IEnumerable<object[]> ProcessTestData()
        {
            var suite = EngineProcessTestsLoader.Load("TestData/ProcessTestSuite.json");
            foreach (var testCase in suite.TestCases)
            {
                yield return new[] { testCase, };
            }
        }

        #endregion

        #region Isolation

        [TestMethod]
        [FunctionalTest]
        [DeploymentItem(@"TestData/source123_test_data.s123d")]
        [DeploymentItem(@"TestData/source4_test_data.s4d")]
        [DeploymentItem(@"TestData/source5_test_data.s5d")]
        [DeploymentItem(@"TestData/ProcessTestSuite.json")]
        [DynamicData(nameof(ProcessTestData), DynamicDataSourceType.Method)]
        public void Process_Isolated_WhenComplete_DataWasProcessed(
            EngineProcessTestCaseDto testCase)
        {
            if (testCase.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }

            var runtime = Engine.Create(
                new EngineCreateInfo
                {
                    AssemblyLoader = new IsolationAssemblyLoader(),
                    Versioning = new FakeVersionChecker(),
                });

            foreach (var cooker in testCase.CookersToEnable)
            {
                var cookerPath = DataCookerPath.Parse(cooker);
                Assert.IsTrue(runtime.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var file in testCase.FilePaths)
            {
                runtime.AddDataSource(new FileDataSource(file));
            }

            var results = runtime.Process();

            foreach (var expectedData in testCase.ExpectedOutputs)
            {
                var dataOutputPathRaw = expectedData.Key;
                var expectedDataPoints = expectedData.Value;
                var dataOutputPath = DataOutputPath.Create(dataOutputPathRaw);

                Assert.IsTrue(
                    results.TryQueryOutput(dataOutputPath, out object data), "Output for {0} not found.", dataOutputPathRaw);
                Assert.IsNotNull(data, "output for {0} was null ???", dataOutputPathRaw);

                var enumerableData = data as IEnumerable;
                Assert.IsNotNull(
                    enumerableData,
                    "Test output data must implement IEnumerable<> (type wasn't enumerable): '{0}'",
                    data.GetType());
                var enumerableType = enumerableData.GetType();
                var eInterface = enumerableType.GetInterface(typeof(IEnumerable<>).Name);
                Assert.IsNotNull(
                    eInterface,
                    "Test output data must implement IEnumerable<> (interface wasn't found): {0}",
                    string.Join(", ", data.GetType().GetInterfaces().Select(x => x.FullName)));
                var collectionType = eInterface.GetGenericArguments()[0];
                Assert.IsNotNull(collectionType, "Unable to retrieve collection type for {0}", data.GetType());

                var enumeratedData = new List<object>();
                foreach (var o in enumerableData)
                {
                    enumeratedData.Add(o);
                }

                Assert.AreEqual(
                    expectedDataPoints.Length,
                    enumeratedData.Count,
                    "The processor did not process the correct amount of data: {0}",
                    dataOutputPath);

                var properties = collectionType.GetProperties()
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

                for (var i = 0; i < expectedDataPoints.Length; ++i)
                {
                    var expectedObject = expectedDataPoints[i];
                    var actualObject = enumeratedData[i];
                    foreach (var kvp in expectedObject)
                    {
                        var propertyName = kvp.Key;
                        var expectedValue = kvp.Value;

                        Assert.IsTrue(properties.TryGetValue(propertyName, out PropertyInfo property));
                        var actualValue = property.GetValue(actualObject)?.ToString();
                        Assert.AreEqual(expectedValue, actualValue, propertyName);
                    }
                }
            }

            foreach (var dataOutputPathRaw in testCase.ThrowingOutputs)
            {
                var dataOutputPath = DataOutputPath.Create(dataOutputPathRaw);
                Assert.IsFalse(
                    results.TryQueryOutput(dataOutputPath, out var _),
                    "Output should not have been available: {0}",
                    dataOutputPathRaw);
            }
        }

        #endregion

        #region Dispose

        [TestMethod]
        [UnitTest]
        public void WhenDisposed_EverythingThrows()
        {
            this.Sut.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.SourceDataCookers);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AllCookers);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.CompositeDataCookers);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.CreationErrors);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.CustomDataSources);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.DataSourcesToProcess);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.ExtensionDirectory);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.FreeDataSourcesToProcess);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.IsProcessed);
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.SourceDataCookers);

            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddDataSource(Any.DataSource()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddDataSource(Any.DataSource(), typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddDataSources(new[] { Any.DataSource(), }, typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddFile(Any.FilePath()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddFile(Any.FilePath(), typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.AddFiles(new[] { Any.FilePath(), }, typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.EnableCooker(Any.DataCookerPath()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.Process());
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddDataSource(Any.DataSource()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddDataSource(Any.DataSource(), typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddDataSources(new[] { Any.DataSource(), }, typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddFile(Any.FilePath()));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddFile(Any.FilePath(), typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryAddFiles(new[] { Any.FilePath(), }, typeof(FakeCustomDataSource)));
            Assert.ThrowsException<ObjectDisposedException>(() => this.Sut.TryEnableCooker(Any.DataCookerPath()));
        }

        [TestMethod]
        [UnitTest]
        public void CanDisposeMultipleTimes()
        {
            this.Sut.Dispose();
            this.Sut.Dispose();
            this.Sut.Dispose();
        }

        #endregion

        private static void CopyAssemblyContainingType(Type type, string destDir)
        {
            Assert.IsNotNull(type);

            var assemblyFile = type.Assembly.GetCodeBaseAsLocalPath();
            var assemblyFileName = Path.GetFileName(assemblyFile);
            File.Copy(assemblyFile, Path.Combine(destDir, assemblyFileName), true);
        }

        private static IDataSource CreateTestFile(string extension)
        {
            var path = Any.FileOnDisk(extension, ScratchDirectory);
            return new FileDataSource(path);
        }

        private static void RunTestInDomain<TestType>()
            where TestType : SerializableTest
        {
            // TODO: Update to use .net "domains"
        }

        [Serializable]
        private abstract class SerializableTest
            : MarshalByRefObject
        {
            public bool TestPassed { get; private set; }

            public string FailureReason { get; private set; }

            public override object InitializeLifetimeService()
            {
                return null;
            }

            public void Run()
            {
                try
                {
                    this.RunCore();
                    this.TestPassed = true;
                }
                catch (Exception e)
                {
                    this.TestPassed = false;
                    this.FailureReason = e.ToString();
                }
            }

            protected abstract void RunCore();
        }

        [CustomDataSource(
            "{645AB037-A325-45EC-9DB0-B8086A83B528}",
            nameof(FakeCustomDataSource),
            "Source for Tests")]
        [FileDataSource(Extension)]
        private sealed class FakeCustomDataSource
            : CustomDataSourceBase
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

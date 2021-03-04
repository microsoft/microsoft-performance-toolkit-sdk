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
            }
        }

        #region Create

        [TestMethod]
        [IntegrationTest]
        public void Create_NoParameters_UsesCurrentDirectory()
        {
            var sut = CreateEngine();

            Assert.AreEqual(Environment.CurrentDirectory, sut.ExtensionDirectory);
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
            var sut = CreateEngine();

            Assert.IsFalse(sut.IsProcessed);
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
            var sut = CreateEngine();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSource(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_NullType_Throws()
        {
            var sut = CreateEngine();

            var file = AnyFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSource(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_NoInstancesOfDataSourceLoaded_Throws()
        {
            var sut = CreateEngine();
            var typeAssembly = typeof(Source123DataSource).Assembly;
            var engineAssemblies = System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies.Where(x =>
                x.GetName().Name.StartsWith("Microsoft.Performance.Toolkit.Engine"));
            var customDataSources = sut.CustomDataSources.ToList();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedCustomDataSourceException>(() => sut.AddDataSource(file, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_DataSourceDoesNotSupportFile_Throws()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => sut.AddDataSource(file, typeof(Source123DataSource)));
            Assert.AreEqual(file.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source123DataSource).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file, sut.DataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);

            sut.AddDataSource(file1, typeof(Source123DataSource));
            sut.AddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, sut.DataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, sut.DataSourcesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSourceOnly_Null_Throws()
        {
            var sut = CreateEngine();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSource(null));
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

            var file = AnyFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => sut.AddDataSource(file));
            Assert.AreEqual(file.Uri.ToString(), e.DataSource);
            Assert.IsNull(e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSourceOnly_AtLeastOnSourceSupports_Adds()
        {
            var sut = CreateEngine();

            var file = AnyFile(Source123DataSource.Extension);
            sut.AddDataSource(file);

            Assert.AreEqual(1, sut.FreeDataSourcesToProcess.Count());
            Assert.AreEqual(file, sut.FreeDataSourcesToProcess.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSource_AlreadyProcessed_Throws()
        {
            var sut = CreateEngine();
            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddDataSource(AnyDataSource()));
            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddDataSource(AnyDataSource(), sut.CustomDataSources.First().GetType()));
        }

        #endregion

        #region TryAddDataSource

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_Null_False()
        {
            var sut = CreateEngine();

            Assert.IsFalse(sut.TryAddDataSource(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_NoCookersOrDataSourcesSupport_False()
        {
            var tempDir = Path.Combine(ScratchDirectory, "TryAddDataSourceOnly_NoCookersOrDataSourcesSupport_False");
            Directory.CreateDirectory(tempDir);
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            var sut = Engine.Create(
                new EngineCreateInfo
                {
                    ExtensionDirectory = tempDir,
                    Versioning = new FakeVersionChecker(),
                });

            var file = AnyFile(".380298502");
            Assert.IsFalse(sut.TryAddDataSource(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_AtLeastOnSourceSupports_Adds()
        {
            var sut = CreateEngine();

            var file = AnyFile(Source123DataSource.Extension);
            sut.TryAddDataSource(file);

            Assert.AreEqual(1, sut.FreeDataSourcesToProcess.Count());
            Assert.AreEqual(file, sut.FreeDataSourcesToProcess.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_AtLeastOnSourceSupports_True()
        {
            var sut = CreateEngine();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsTrue(sut.TryAddDataSource(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSourceOnly_AlreadyProcessed_False()
        {
            var sut = CreateEngine();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddDataSource(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NullFile_False()
        {
            var sut = CreateEngine();

            Assert.IsFalse(sut.TryAddDataSource(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NullType_False()
        {
            var sut = CreateEngine();

            var file = AnyFile(".txt");
            Assert.IsFalse(sut.TryAddDataSource(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_NoInstancesOfDataSourceLoaded_False()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddDataSource(file, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_DataSourceDoesNotSupportFile_False()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(".380298502");
            Assert.IsFalse(sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_Added()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            sut.AddDataSource(file, typeof(Source123DataSource));

            var expectedFile = file;
            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(expectedFile, sut.DataSourcesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSource_True()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsTrue(sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparately()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);

            sut.TryAddDataSource(file1, typeof(Source123DataSource));
            sut.TryAddDataSource(file2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, sut.DataSourcesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, sut.DataSourcesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSource_AlreadyProcessed_False()
        {
            var sut = CreateEngine();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddDataSource(file, typeof(Source123DataSource)));
        }

        #endregion

        #region AddDataSources

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NullFiles_Throws()
        {
            var sut = CreateEngine();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSources(null, typeof(Source123DataSource)));
        }


        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_EmptyFiles_Throws()
        {
            var sut = CreateEngine();

            Assert.ThrowsException<ArgumentException>(() => sut.AddDataSources(new IDataSource[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_ContainsNullFiles_Throws()
        {
            var sut = CreateEngine();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSources(new[] { (IDataSource)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NullType_Throws()
        {
            var sut = CreateEngine();

            var file = AnyFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => sut.AddDataSources(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_NoInstancesOfDataSourceLoaded_Throws()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedCustomDataSourceException>(() => sut.AddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_DataSourceDoesNotSupportFile_Throws()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source4DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => sut.AddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));

            Assert.AreEqual(file1.Uri.ToString(), e.DataSource);
            Assert.AreEqual(typeof(Source4DataSource).FullName, e.RequestedCustomDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.AddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(files.Length, sut.DataSourcesToProcess[expectedDataSource][0].Count);

            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files1 = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            var files2 = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.AddDataSources(files1, typeof(Source123DataSource));
            sut.AddDataSources(files2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, sut.DataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, sut.DataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], sut.DataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void AddDataSources_AlreadyProcessed_Throws()
        {
            var sut = CreateEngine();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddDataSources(new[] { file, }, typeof(Source123DataSource)));
        }

        #endregion

        #region TryAddDataSources

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NullFiles_False()
        {
            var sut = CreateEngine();

            Assert.IsFalse(sut.TryAddDataSources(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NoFiles_False()
        {
            var sut = CreateEngine();

            Assert.IsFalse(sut.TryAddDataSources(new IDataSource[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_ContainsNullFiles_False()
        {
            var sut = CreateEngine();

            Assert.IsFalse(sut.TryAddDataSources(new[] { (IDataSource)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NullType_False()
        {
            var sut = CreateEngine();

            var file = AnyFile(".txt");
            Assert.IsFalse(sut.TryAddDataSources(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_NoInstancesOfDataSourceLoaded_False()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddDataSources(new[] { file, }, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_DataSourceDoesNotSupportFile_False()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddDataSources(new[] { file1, file2, }, typeof(Source4DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FilesSupportedByAtLeastOneDataSource_Added()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.TryAddDataSources(files, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(1, sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files.Length, sut.DataSourcesToProcess[expectedDataSource][0].Count); 
            for (var i = 0; i < files.Length; ++i)
            {
                Assert.AreEqual(files[i], sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FileSupportedByAtLeastOneDataSource_True()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            Assert.IsTrue(sut.TryAddDataSources(files, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            var sut = CreateEngine();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files1 = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            var files2 = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.TryAddDataSources(files1, typeof(Source123DataSource));
            sut.TryAddDataSources(files2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.DataSourcesToProcess.Count);
            Assert.IsTrue(sut.DataSourcesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, sut.DataSourcesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, sut.DataSourcesToProcess[expectedDataSource][0].Count);
            for (var i = 0; i < files1.Length; ++i)
            {
                Assert.AreEqual(files1[i], sut.DataSourcesToProcess[expectedDataSource][0][i]);
            }

            Assert.AreEqual(files2.Length, sut.DataSourcesToProcess[expectedDataSource][1].Count);
            for (var i = 0; i < files2.Length; ++i)
            {
                Assert.AreEqual(files2[i], sut.DataSourcesToProcess[expectedDataSource][1][i]);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddDataSources_AlreadyProcessed_False()
        {
            var sut = CreateEngine();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddDataSources(new[] { file, }, typeof(Source123DataSource)));
        }

        #endregion

        #region Enable Cooker

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_Known_Enables()
        {
            var sut = CreateEngine();
            var cooker = sut.AllCookers.FirstOrDefault();

            sut.EnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_NotKnown_Throws()
        {
            var sut = CreateEngine();
            var cooker = new DataCookerPath("not-there-id");

            var e = Assert.ThrowsException<CookerNotFoundException>(() => sut.EnableCooker(cooker));
            Assert.AreEqual(cooker, e.DataCookerPath);
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_AlreadyProcessed_Throws()
        {
            var sut = CreateEngine();
            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.EnableCooker(sut.AllCookers.First()));
        }

        #endregion Enable Cooker

        #region TryEnableCooker

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_Enables()
        {
            var sut = CreateEngine();
            var cooker = sut.AllCookers.FirstOrDefault();

            sut.TryEnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_True()
        {
            var sut = CreateEngine();
            var cooker = sut.AllCookers.FirstOrDefault();

            Assert.IsTrue(sut.TryEnableCooker(cooker));

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_NotKnown_False()
        {
            var sut = CreateEngine();
            var cooker = new DataCookerPath("not-there-id");

            Assert.IsFalse(sut.TryEnableCooker(cooker));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_AlreadyProcessed_False()
        {
            var sut = CreateEngine();
            sut.Process();

            Assert.IsFalse(sut.TryEnableCooker(sut.AllCookers.First()));
        }

        #endregion TryEnableCooker

        #region Process

        [TestMethod]
        [FunctionalTest]
        public void Process_WhenComplete_IsProcessedSetToTrue()
        {
            var sut = CreateEngine();

            sut.Process();

            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_NothingEnabled_DoesNothing()
        {
            var sut = CreateEngine();

            var results = sut.Process();

            Assert.IsNotNull(results);
            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_AlreadyProcessed_Throws()
        {
            var sut = CreateEngine();

            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.Process());
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

            var runtime = CreateEngine();

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

        private static Engine CreateEngine()
        {
            return Engine.Create(
                new EngineCreateInfo
                {
                    Versioning = new FakeVersionChecker(),
                });
        }

        private static void CopyAssemblyContainingType(Type type, string destDir)
        {
            Assert.IsNotNull(type);

            var assemblyFile = type.Assembly.GetCodeBaseAsLocalPath();
            var assemblyFileName = Path.GetFileName(assemblyFile);
            File.Copy(assemblyFile, Path.Combine(destDir, assemblyFileName), true);
        }

        private static T CreateInstanceInDomain<T>(AppDomain domain)
            where T : class
        {
            var instance = domain.CreateInstanceAndUnwrap(
                typeof(T).Assembly.FullName,
                typeof(T).FullName) as T;
            return instance;
        }

        private static IDataSource AnyDataSource()
        {
            return AnyFile(".txt");
        }

        private static IDataSource AnyFile(string extension)
        {
            var file = Path.Combine(ScratchDirectory, Path.GetRandomFileName()) + extension;
            File.WriteAllText(file, "THIS IS A TEST FILE");
            return new FileDataSource(file);
        }

        private static void RunTestInDomain<TestType>()
            where TestType : SerializableTest
        {
            // TODO: Update to use .net "domains"
            //using (var domain = ScopedAppDomain.Create(typeof(TestType).Name))
            //{
            //    var test = CreateInstanceInDomain<TestType>(domain);

            //    test.Run();
            //    Assert.IsTrue(test.TestPassed, "Failed: {0}", test.FailureReason);
            //}
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

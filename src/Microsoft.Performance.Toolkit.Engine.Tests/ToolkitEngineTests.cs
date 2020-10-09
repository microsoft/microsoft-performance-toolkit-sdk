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
            var sut = Engine.Create();

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
            var sut = Engine.Create();

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

        #region AddFile

        [TestMethod]
        [IntegrationTest]
        public void AddFile_NullFile_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFile(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_WhitespaceFile_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentException>(() => sut.AddFile(string.Empty, typeof(Source123DataSource)));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFile(" ", typeof(Source123DataSource)));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFile("\t", typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_NullType_Throws()
        {
            var sut = Engine.Create();

            var file = AnyFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFile(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_NoInstancesOfDataSourceLoaded_Throws()
        {
            var sut = Engine.Create();
            var typeAssembly = typeof(Source123DataSource).Assembly;
            var engineAssemblies = System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies.Where(x =>
                x.GetName().Name.StartsWith("Microsoft.Performance.Toolkit.Engine"));
            var customDataSources = sut.CustomDataSources.ToList();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => sut.AddFile(file, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_DataSourceDoesNotSupportFile_Throws()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedFileException>(() => sut.AddFile(file, typeof(Source123DataSource)));
            Assert.AreEqual(file, e.FilePath);
            Assert.AreEqual(typeof(Source123DataSource).FullName, e.RequestedDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_FileSupportedByAtLeastOneDataSource_Added()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            sut.AddFile(file, typeof(Source123DataSource));

            var expectedFile = file;
            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(expectedFile, sut.FilesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparaetly()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);

            sut.AddFile(file1, typeof(Source123DataSource));
            sut.AddFile(file2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, sut.FilesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFileOnly_Null_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFile(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFileOnly_Whitespace_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentException>(() => sut.AddFile(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFile(" "));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFile("\t"));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFileOnly_NoCookersOrDataSourcesSupport_Throws()
        {
            var tempDir = Path.Combine(ScratchDirectory, "Serializable_AddFileOnly_NoCookersOrDataSourcesSupport_Throws");
            Directory.CreateDirectory(tempDir);
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            var sut = Engine.Create(
                new EngineCreateInfo
                {
                    ExtensionDirectory = tempDir,
                });

            var file = AnyFile(".380298502");
            var e = Assert.ThrowsException<UnsupportedFileException>(() => sut.AddFile(file));
            Assert.AreEqual(file, e.FilePath);
            Assert.IsNull(e.RequestedDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFileOnly_AtLeastOnSourceSupports_Adds()
        {
            var sut = Engine.Create();

            var file = AnyFile(Source123DataSource.Extension);
            sut.AddFile(file);

            Assert.AreEqual(1, sut.FreeFilesToProcess.Count());
            Assert.AreEqual(file, sut.FreeFilesToProcess.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFile_AlreadyProcessed_Throws()
        {
            var sut = Engine.Create();
            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddFile("any file"));
            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddFile("any file", sut.CustomDataSources.First().GetType()));
        }

        #endregion

        #region TryAddFile

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_Null_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFile(null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_Whitespace_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFile(string.Empty));
            Assert.IsFalse(sut.TryAddFile(" "));
            Assert.IsFalse(sut.TryAddFile(" \r  \r\n \t "));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_NoCookersOrDataSourcesSupport_False()
        {
            var tempDir = Path.Combine(ScratchDirectory, "TryAddFileOnly_NoCookersOrDataSourcesSupport_False");
            Directory.CreateDirectory(tempDir);
            CopyAssemblyContainingType(typeof(Source123DataSource), tempDir);
            var sut = Engine.Create(
                new EngineCreateInfo
                {
                    ExtensionDirectory = tempDir,
                });

            var file = AnyFile(".380298502");
            Assert.IsFalse(sut.TryAddFile(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_AtLeastOnSourceSupports_Adds()
        {
            var sut = Engine.Create();

            var file = AnyFile(Source123DataSource.Extension);
            sut.TryAddFile(file);

            Assert.AreEqual(1, sut.FreeFilesToProcess.Count());
            Assert.AreEqual(file, sut.FreeFilesToProcess.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_AtLeastOnSourceSupports_True()
        {
            var sut = Engine.Create();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsTrue(sut.TryAddFile(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFileOnly_AlreadyProcessed_False()
        {
            var sut = Engine.Create();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddFile(file));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_NullFile_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFile(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_WhitespaceFile_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFile(string.Empty, typeof(Source123DataSource)));
            Assert.IsFalse(sut.TryAddFile(" ", typeof(Source123DataSource)));
            Assert.IsFalse(sut.TryAddFile("\t", typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_NullType_False()
        {
            var sut = Engine.Create();

            var file = AnyFile(".txt");
            Assert.IsFalse(sut.TryAddFile(file, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_NoInstancesOfDataSourceLoaded_False()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddFile(file, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_DataSourceDoesNotSupportFile_False()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(".380298502");
            Assert.IsFalse(sut.TryAddFile(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_FileSupportedByAtLeastOneDataSource_Added()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            sut.AddFile(file, typeof(Source123DataSource));

            var expectedFile = file;
            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(expectedFile, sut.FilesToProcess[expectedDataSource][0][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_FileSupportedByAtLeastOneDataSource_True()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsTrue(sut.TryAddFile(file, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_FileSupportedByAtLeastOneDataSourceManyTimes_AddedSeparaetly()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);

            sut.TryAddFile(file1, typeof(Source123DataSource));
            sut.TryAddFile(file2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(file1, sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(file2, sut.FilesToProcess[expectedDataSource][1][0]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFile_AlreadyProcessed_False()
        {
            var sut = Engine.Create();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddFile(file, typeof(Source123DataSource)));
        }

        #endregion

        #region AddFiles

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_NullFiles_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFiles(null, typeof(Source123DataSource)));
        }


        [TestMethod]
        [IntegrationTest]
        public void AddFiles_EmptyFiles_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentException>(() => sut.AddFiles(new string[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_ContainsNullFiles_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFiles(new[] { (string)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_WhitespaceFile_Throws()
        {
            var sut = Engine.Create();

            Assert.ThrowsException<ArgumentException>(() => sut.AddFiles(new[] { string.Empty, }, typeof(Source123DataSource)));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFiles(new[] { " ", }, typeof(Source123DataSource)));
            Assert.ThrowsException<ArgumentException>(() => sut.AddFiles(new[] { "\t", }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_NullType_Throws()
        {
            var sut = Engine.Create();

            var file = AnyFile(".txt");
            Assert.ThrowsException<ArgumentNullException>(() => sut.AddFiles(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_NoInstancesOfDataSourceLoaded_Throws()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedDataSourceException>(() => sut.AddFiles(new[] { file, }, typeof(ToolkitEngineTests)));
            Assert.AreEqual(typeof(ToolkitEngineTests).FullName, e.RequestedDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_DataSourceDoesNotSupportFile_Throws()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source4DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);
            var e = Assert.ThrowsException<UnsupportedFileException>(() => sut.AddFiles(new[] { file1, file2, }, typeof(Source4DataSource)));

            Assert.AreEqual(file1, e.FilePath);
            Assert.AreEqual(typeof(Source4DataSource).FullName, e.RequestedDataSource);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_FilesSupportedByAtLeastOneDataSource_Added()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.AddFiles(files, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(files.Length, sut.FilesToProcess[expectedDataSource][0].Count);
            Assert.AreEqual(files[0], sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(files[1], sut.FilesToProcess[expectedDataSource][0][1]);
            Assert.AreEqual(files[2], sut.FilesToProcess[expectedDataSource][0][2]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            var sut = Engine.Create();
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

            sut.AddFiles(files1, typeof(Source123DataSource));
            sut.AddFiles(files2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, sut.FilesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, sut.FilesToProcess[expectedDataSource][0].Count);
            Assert.AreEqual(files1[0], sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(files1[1], sut.FilesToProcess[expectedDataSource][0][1]);
            Assert.AreEqual(files1[2], sut.FilesToProcess[expectedDataSource][0][2]);
            Assert.AreEqual(files2.Length, sut.FilesToProcess[expectedDataSource][1].Count);
            Assert.AreEqual(files2[0], sut.FilesToProcess[expectedDataSource][1][0]);
            Assert.AreEqual(files2[1], sut.FilesToProcess[expectedDataSource][1][1]);
        }

        [TestMethod]
        [IntegrationTest]
        public void AddFiles_AlreadyProcessed_Throws()
        {
            var sut = Engine.Create();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.AddFiles(new[] { file, }, typeof(Source123DataSource)));
        }

        #endregion

        #region TryAddFiles

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_NullFiles_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFiles(null, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_NoFiles_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFiles(new string[0], typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_ContainsNullFiles_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFiles(new[] { (string)null, }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_WhitespaceFile_False()
        {
            var sut = Engine.Create();

            Assert.IsFalse(sut.TryAddFiles(new[] { string.Empty, }, typeof(Source123DataSource)));
            Assert.IsFalse(sut.TryAddFiles(new[] { " ", }, typeof(Source123DataSource)));
            Assert.IsFalse(sut.TryAddFiles(new[] { "\t", }, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_NullType_False()
        {
            var sut = Engine.Create();

            var file = AnyFile(".txt");
            Assert.IsFalse(sut.TryAddFiles(new[] { file, }, null));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_NoInstancesOfDataSourceLoaded_False()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddFiles(new[] { file, }, typeof(ToolkitEngineTests)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_DataSourceDoesNotSupportFile_False()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var file1 = AnyFile(Source123DataSource.Extension);
            var file2 = AnyFile(Source123DataSource.Extension);
            Assert.IsFalse(sut.TryAddFiles(new[] { file1, file2, }, typeof(Source4DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_FilesSupportedByAtLeastOneDataSource_Added()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            sut.TryAddFiles(files, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(1, sut.FilesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files.Length, sut.FilesToProcess[expectedDataSource][0].Count);
            Assert.AreEqual(files[0], sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(files[1], sut.FilesToProcess[expectedDataSource][0][1]);
            Assert.AreEqual(files[2], sut.FilesToProcess[expectedDataSource][0][2]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_FileSupportedByAtLeastOneDataSource_True()
        {
            var sut = Engine.Create();
            Assert.IsTrue(sut.CustomDataSources.Any(x => x is Source123DataSource));

            var files = new[]
            {
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
                AnyFile(Source123DataSource.Extension),
            };

            Assert.IsTrue(sut.TryAddFiles(files, typeof(Source123DataSource)));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_FilesSupportedBySameDataSource_AddedAsSeparateCollection()
        {
            var sut = Engine.Create();
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

            sut.TryAddFiles(files1, typeof(Source123DataSource));
            sut.TryAddFiles(files2, typeof(Source123DataSource));

            var expectedDataSource = sut.CustomDataSources.Single(x => x is Source123DataSource);

            Assert.AreEqual(1, sut.FilesToProcess.Count);
            Assert.IsTrue(sut.FilesToProcess.ContainsKey(expectedDataSource));
            Assert.AreEqual(2, sut.FilesToProcess[expectedDataSource].Count);
            Assert.AreEqual(files1.Length, sut.FilesToProcess[expectedDataSource][0].Count);
            Assert.AreEqual(files1[0], sut.FilesToProcess[expectedDataSource][0][0]);
            Assert.AreEqual(files1[1], sut.FilesToProcess[expectedDataSource][0][1]);
            Assert.AreEqual(files1[2], sut.FilesToProcess[expectedDataSource][0][2]);
            Assert.AreEqual(files2.Length, sut.FilesToProcess[expectedDataSource][1].Count);
            Assert.AreEqual(files2[0], sut.FilesToProcess[expectedDataSource][1][0]);
            Assert.AreEqual(files2[1], sut.FilesToProcess[expectedDataSource][1][1]);
        }

        [TestMethod]
        [IntegrationTest]
        public void TryAddFiles_AlreadyProcessed_False()
        {
            var sut = Engine.Create();
            sut.Process();

            var file = AnyFile(Source123DataSource.Extension);

            Assert.IsFalse(sut.TryAddFiles(new[] { file, }, typeof(Source123DataSource)));
        }

        #endregion

        #region Enable Cooker

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_Known_Enables()
        {
            var sut = Engine.Create();
            var cooker = sut.AllCookers.FirstOrDefault();

            sut.EnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_NotKnown_Throws()
        {
            var sut = Engine.Create();
            var cooker = new DataCookerPath("not-there-id");

            var e = Assert.ThrowsException<CookerNotFoundException>(() => sut.EnableCooker(cooker));
            Assert.AreEqual(cooker, e.DataCookerPath);
        }

        [TestMethod]
        [IntegrationTest]
        public void EnableCooker_AlreadyProcessed_Throws()
        {
            var sut = Engine.Create();
            sut.Process();

            Assert.ThrowsException<InstanceAlreadyProcessedException>(() => sut.EnableCooker(sut.AllCookers.First()));
        }

        #endregion Enable Cooker

        #region TryEnableCooker

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_Enables()
        {
            var sut = Engine.Create();
            var cooker = sut.AllCookers.FirstOrDefault();

            sut.TryEnableCooker(cooker);

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_Known_True()
        {
            var sut = Engine.Create();
            var cooker = sut.AllCookers.FirstOrDefault();

            Assert.IsTrue(sut.TryEnableCooker(cooker));

            Assert.AreEqual(1, sut.EnabledCookers.Count());
            Assert.AreEqual(cooker, sut.EnabledCookers.ElementAt(0));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_NotKnown_False()
        {
            var sut = Engine.Create();
            var cooker = new DataCookerPath("not-there-id");

            Assert.IsFalse(sut.TryEnableCooker(cooker));
        }

        [TestMethod]
        [IntegrationTest]
        public void TryEnableCooker_AlreadyProcessed_False()
        {
            var sut = Engine.Create();
            sut.Process();

            Assert.IsFalse(sut.TryEnableCooker(sut.AllCookers.First()));
        }

        #endregion TryEnableCooker

        #region Process

        [TestMethod]
        [FunctionalTest]
        public void Process_WhenComplete_IsProcessedSetToTrue()
        {
            var sut = Engine.Create();

            sut.Process();

            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_NothingEnabled_DoesNothing()
        {
            var sut = Engine.Create();

            var results = sut.Process();

            Assert.IsNotNull(results);
            Assert.IsTrue(sut.IsProcessed);
        }

        [TestMethod]
        [FunctionalTest]
        public void Process_AlreadyProcessed_Throws()
        {
            var sut = Engine.Create();

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

            var runtime = Engine.Create();

            foreach (var cooker in testCase.CookersToEnable)
            {
                var cookerPath = DataCookerPath.Parse(cooker);
                Assert.IsTrue(runtime.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var file in testCase.FilePaths)
            {
                runtime.AddFile(file);
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
                });

            foreach (var cooker in testCase.CookersToEnable)
            {
                var cookerPath = DataCookerPath.Parse(cooker);
                Assert.IsTrue(runtime.TryEnableCooker(cookerPath), "Unable to enable cooker '{0}'", cookerPath);
            }

            foreach (var file in testCase.FilePaths)
            {
                runtime.AddFile(file);
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

        private static string AnyFile(string extension)
        {
            var file = Path.Combine(ScratchDirectory, Path.GetRandomFileName()) + extension;
            File.WriteAllText(file, "THIS IS A TEST FILE");
            return file;
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

            protected override bool IsFileSupportedCore(string path)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(
                    FileExtensionUtils.GetCanonicalExtension(path),
                    FileExtensionUtils.GetCanonicalExtension(Extension));
            }

            protected override void SetApplicationEnvironmentCore(IApplicationEnvironment applicationEnvironment)
            {
                throw new NotImplementedException();
            }
        }
    }
}

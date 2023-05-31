// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    internal class TestFindFiles
        : AssemblyExtensionDiscovery.IFindFiles
    {
        public Func<string, string, IEnumerable<string>> enumerateFiles;

        public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern)
        {
            if (enumerateFiles == null)
            {
                return new List<string>();
            }

            return this.enumerateFiles.Invoke(directoryPath, searchPattern);
        }

        public Func<string, IEnumerable<string>> enumerateFolders;

        public IEnumerable<string> EnumerateFolders(string directoryPath)
        {
            if (enumerateFolders == null)
            {
                return new List<string>();
            }

            return this.enumerateFolders.Invoke(directoryPath);
        }
    }

    internal class TestAssemblyLoader
        : IAssemblyLoader
    {
        public bool SupportsIsolation { get; set; }

        public ErrorInfo LoadAssemblyErrorInfo { get; set; } = ErrorInfo.None;

        public Func<string, bool> IsAssemblyFunc { get; set; }

        public Func<string, Assembly> LoadAssemblyFunc { get; set; }

        public bool IsAssembly(string path)
        {
            return this.IsAssemblyFunc?.Invoke(path) ?? true;
        }

        public Assembly LoadAssembly(string assemblyPath, out ErrorInfo error)
        {
            error = this.LoadAssemblyErrorInfo;
            return this.LoadAssemblyFunc?.Invoke(assemblyPath) ?? null;
        }
    }

    internal class TestExtensionObserver
        : IExtensionTypeObserver
    {
        public List<Type> ProcessTypes { get; } = new List<Type>();
        public Action<Type, string> OnProcessType { get; set; }
        public void ProcessType(Type type, string sourceName)
        {
            this.ProcessTypes.Add(type);
            OnProcessType?.Invoke(type, sourceName);
        }

        internal bool Completed { get; set; }

        public void DiscoveryStarted()
        {
            this.Completed = false;
        }

        public void DiscoveryComplete()
        {
            Assert.IsFalse(this.Completed);
            this.Completed = true;
        }
    }

    [TestClass]
    public class AssemblyExtensionDiscoveryTests
    {
        private TestAssemblyLoader Loader { get; set; }

        private FakeVersionChecker VersionChecker { get; set; }

        private AssemblyExtensionDiscovery Discovery { get; set; }

        private TestFindFiles FindFiles { get; set; }

        private List<TestExtensionObserver> Observers { get; set; }

        private string TestAssemblyDirectory { get; set; }

        private void RegisterObservers()
        {
            foreach (var observer in this.Observers)
            {
                this.Discovery.RegisterTypeConsumer(observer);
            }
        }

        [TestInitialize]
        public void Setup()
        {
            this.Loader = new TestAssemblyLoader();

            this.VersionChecker = new FakeVersionChecker();

            this.Discovery = new AssemblyExtensionDiscovery(this.Loader, _ => new FakePreloadValidator());

            this.FindFiles = new TestFindFiles();

            this.Discovery.FindFiles = this.FindFiles;

            this.Observers = new List<TestExtensionObserver>();

            var testAssembly = this.GetType().Assembly;
            this.TestAssemblyDirectory = Path.GetDirectoryName(testAssembly.GetCodeBaseAsLocalPath());
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_NullDirectoryPath()
        {
            Assert.ThrowsException<ArgumentNullException>(() => this.Discovery.ProcessAssemblies((string)null, out _));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_NoObservers()
        {
            this.FindFiles.enumerateFiles = (directory, searchPattern) =>
            {
                Assert.Fail("EnumerateFiles shouldn't be called if there are no observers.");
                return new List<string>();
            };

            this.FindFiles.enumerateFolders = (directory) =>
            {
                Assert.Fail("EnumerateFolders shouldn't be called if there are no observers.");
                return new List<string>();
            };

            var testAssembly = this.GetType().Assembly;
            var testAssemblyDirectory = Path.GetDirectoryName(testAssembly.GetCodeBaseAsLocalPath());

            this.Discovery.ProcessAssemblies(testAssemblyDirectory, out _);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecifyExclusions()
        {
            var exclusions = new[] { "Blawp.dll", };

            string configPath = Path.Combine(this.TestAssemblyDirectory, AssemblyExtensionDiscovery.ExclusionsFilename);

            try
            {
                {
                    using StreamWriter writer = File.CreateText(configPath);
                    writer.WriteLine(exclusions[0]);
                    writer.Flush();
                }

                this.FindFiles.enumerateFiles =
                    (directory, searchPattern) => new List<string>() { exclusions[0], };

                this.Observers.Add(new TestExtensionObserver());

                RegisterObservers();
                this.Loader.LoadAssemblyFunc =
                    s =>
                    {
                        Assert.Fail("LoadAssembly should not be called, the file should be excluded.");
                        return null;
                    };

                this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);
            }
            finally
            {
                File.Delete(configPath);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecifyExclusions_DifferingCase()
        {
            var exclusions = new[] { "Blawp.dll", };

            string configPath = Path.Combine(this.TestAssemblyDirectory, AssemblyExtensionDiscovery.ExclusionsFilename);

            try
            {
                {
                    using StreamWriter writer = File.CreateText(configPath);
                    writer.WriteLine(exclusions[0]);
                    writer.Flush();
                }

                // exclusions are case sensitive. using ToLower here should make it so that we still receive a callback
                this.FindFiles.enumerateFiles =
                    (directory, searchPattern) =>
                    {
                        if (searchPattern.EndsWith(".dll"))
                        {
                            return new List<string>() { exclusions[0].ToLower(), };
                        }

                        return new List<string>();
                    };

                var observer = new TestExtensionObserver();
                this.Observers.Add(observer);

                var assembly = new FakeAssembly
                {
                    TypesToReturn = new[]
                    {
                    typeof(int),
                    typeof(bool),
                    typeof(string),
                    },
                };

                int callbackCount = 0;
                RegisterObservers();
                this.Loader.LoadAssemblyFunc =
                    s =>
                    {
                        callbackCount++;
                        return assembly;
                    };

                this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);

                Assert.AreEqual(1, callbackCount);
                Assert.AreEqual(0, observer.ProcessTypes.Count);
            }
            finally
            {
                File.Delete(configPath);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecifyExclusions_Folder()
        {
            var exclusions = new[] { "SubFolder", };

            string configPath = Path.Combine(this.TestAssemblyDirectory, AssemblyExtensionDiscovery.ExclusionsFilename);
            string subFolderPath = Path.Combine(this.TestAssemblyDirectory, exclusions[0]);

            try
            {
                {
                    using StreamWriter writer = File.CreateText(configPath);
                    writer.WriteLine(exclusions[0]);
                    writer.Flush();
                }

                this.FindFiles.enumerateFiles =
                    (directory, searchPattern) =>
                    {
                        Assert.AreNotEqual(directory, subFolderPath);
                        return new List<string>();
                    };

                this.FindFiles.enumerateFolders =
                    (directory) =>
                    {
                        Assert.AreNotEqual(directory, subFolderPath);
                        return new List<string> { subFolderPath };
                    };

                this.Observers.Add(new TestExtensionObserver());

                RegisterObservers();
                this.Loader.LoadAssemblyFunc =
                    s =>
                    {
                        Assert.Fail("LoadAssembly should not be called, the folder should be excluded.");
                        return null;
                    };

                this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);
            }
            finally
            {
                File.Delete(configPath);
            }
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_ValidAssemblyThatDoesNotReferenceSdk_TypesNotEnumerated()
        {
            var files = new[] { "Blawp.dll", };
            this.FindFiles.enumerateFiles =
                (directory, searchPattern) =>
                    searchPattern.EndsWith(".dll")
                        ? new List<string>() { files[0].ToLower(), }
                        : new List<string>();

            var observer = new TestExtensionObserver();
            this.Observers.Add(observer);
            RegisterObservers();

            var assembly = new FakeAssembly
            {
                TypesToReturn = new[]
                {
                    typeof(int),
                    typeof(bool),
                    typeof(string),
                },
            };

            this.Loader.LoadAssemblyFunc = s => assembly;

            this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);

            Assert.AreEqual(0, observer.ProcessTypes.Count);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_ValidAssemblyThatReferencesSdk_TypesEnumerated()
        {
            var files = new[] { "Blawp.dll", };
            this.FindFiles.enumerateFiles =
                (directory, searchPattern) =>
                    searchPattern.EndsWith(".dll")
                        ? new List<string>() { files[0].ToLower(), }
                        : new List<string>();

            var observer = new TestExtensionObserver();
            this.Observers.Add(observer);
            RegisterObservers();

            var assembly = new FakeAssembly
            {
                ReferencedAssemblies = new[]
                {
                    SdkAssembly.Assembly.GetName(),
                },
                TypesToReturn = new[]
                {
                    typeof(int),
                    typeof(bool),
                    typeof(string),
                },
            };

            this.Loader.LoadAssemblyFunc = s => assembly;

            this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);

            CollectionAssert.AreEquivalent(assembly.TypesToReturn, observer.ProcessTypes);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_ValidAssemblyThatReferencesSdk_TypesEnumerated_SubFolder()
        {
            const string subFolderName = "SubFolder";

            string subFolderPath = Path.Combine(this.TestAssemblyDirectory, subFolderName);
            var files = new[] { "Blawp.dll", };

            this.FindFiles.enumerateFiles =
                (directory, searchPattern) =>
                {
                    if (directory == subFolderPath)
                    {
                        return searchPattern.EndsWith(".dll")
                            ? new List<string>() { files[0].ToLower(), }
                            : new List<string>();
                    }

                    return new List<string>();
                };

            this.FindFiles.enumerateFolders =
                (directory) =>
                {
                    if (directory == this.TestAssemblyDirectory)
                    {
                        return new List<string> { subFolderPath };
                    }

                    return new List<string>();
                };

            var observer = new TestExtensionObserver();
            this.Observers.Add(observer);
            RegisterObservers();

            var assembly = new FakeAssembly
            {
                ReferencedAssemblies = new[]
                {
                    SdkAssembly.Assembly.GetName(),
                },
                TypesToReturn = new[]
                {
                    typeof(int),
                    typeof(bool),
                    typeof(string),
                },
            };

            this.Loader.LoadAssemblyFunc = s => assembly;

            try
            {
                Directory.CreateDirectory(subFolderPath);
                this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, out _);
            }
            finally
            {
                Directory.Delete(subFolderPath, true);
            }

            CollectionAssert.AreEquivalent(assembly.TypesToReturn, observer.ProcessTypes);
        }
    }
}

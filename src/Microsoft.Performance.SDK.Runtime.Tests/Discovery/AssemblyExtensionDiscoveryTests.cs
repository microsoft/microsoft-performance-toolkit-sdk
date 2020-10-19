// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    internal class TestFindFiles
        : AssemblyExtensionDiscovery.IFindFiles
    {
        public Func<string, string, SearchOption, IEnumerable<string>> enumerateFiles;

        public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, SearchOption searchOption)
        {
            if (enumerateFiles == null)
            {
                return new List<string>();
            }

            return this.enumerateFiles.Invoke(directoryPath, searchPattern, searchOption);
        }
    }

    internal class TestAssemblyLoader
        : IAssemblyLoader
    {
        public bool SupportsIsolation { get; set; }

        public Func<string, Assembly> loadAssembly;

        public Assembly LoadAssembly(string assemblyPath)
        {
            return loadAssembly?.Invoke(assemblyPath);
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

            this.Discovery = new AssemblyExtensionDiscovery(this.Loader);

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
            Assert.ThrowsException<ArgumentNullException>(() => this.Discovery.ProcessAssemblies((string)null));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_NullDirectoryPath2()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => this.Discovery.ProcessAssemblies(
                    (string)null,
                    false,
                    null,
                    null,
                    false));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_EmptyDirectoryPath()
        {
            Assert.ThrowsException<ArgumentException>(
                () => this.Discovery.ProcessAssemblies(
                    string.Empty,
                    false,
                    null,
                    null,
                    false));
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_NoObservers()
        {
            this.FindFiles.enumerateFiles = (directory, searchPattern, searchOptions) =>
            {
                Assert.Fail("EnumerateFiles shouldn't be called if there are no observers.");
                return new List<string>();
            };

            var testAssembly = this.GetType().Assembly;
            var testAssemblyDirectory = Path.GetDirectoryName(testAssembly.GetCodeBaseAsLocalPath());

            this.Discovery.ProcessAssemblies(testAssemblyDirectory);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecificSearchPattern()
        {
            var searchPatterns = new[] { "*.elephant", "*.bananas" };
            var patternsSearched = new List<string>(searchPatterns.Length);

            this.FindFiles.enumerateFiles = (directory, searchPattern, searchOptions) =>
            {
                Assert.IsTrue(
                    searchPattern == searchPatterns[0] || searchPattern == searchPatterns[1]);
                patternsSearched.Add(searchPattern);
                return new List<string>();
            };

            this.Observers.Add(new TestExtensionObserver());

            RegisterObservers();

            this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, false, searchPatterns, null, false);

            Assert.AreEqual(searchPatterns.Length, patternsSearched.Count);
            foreach (var pattern in searchPatterns)
            {
                Assert.IsTrue(patternsSearched.Contains(pattern));
            }
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecifyExclusions()
        {
            var exclusions = new[] { "Blawp.dll", };

            this.FindFiles.enumerateFiles = 
                (directory, searchPattern, searchOptions) => new List<string>() {exclusions[0].ToLower(),};

            this.Observers.Add(new TestExtensionObserver());

            RegisterObservers();
            this.Loader.loadAssembly =
                s =>
                {
                    Assert.Fail("LoadAssembly should not be called, the file should be excluded.");
                    return null;
                };

            this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, false, null, exclusions, false);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_SpecifyCaseSensitiveExclusions()
        {
            var exclusions = new[] { "Blawp.dll", };

            this.FindFiles.enumerateFiles = 
                (directory, searchPattern, searchOptions) => new List<string>() {exclusions[0].ToLower(),};

            this.Observers.Add(new TestExtensionObserver());
            RegisterObservers();

            bool assemblyLoaded = false;
            this.Loader.loadAssembly = s =>
            {
                assemblyLoaded = true;
                return this.GetType().Assembly;
            };

            this.Discovery.ProcessAssemblies(this.TestAssemblyDirectory, false, null, exclusions, true);

            Assert.IsTrue(assemblyLoaded);
        }
    }
}

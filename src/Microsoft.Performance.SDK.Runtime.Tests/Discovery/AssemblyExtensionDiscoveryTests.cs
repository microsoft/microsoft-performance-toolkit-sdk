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
            this.FindFiles.enumerateFiles = (directory, searchPattern, searchOptions) =>
            {
                Assert.Fail("EnumerateFiles shouldn't be called if there are no observers.");
                return new List<string>();
            };

            var testAssembly = this.GetType().Assembly;
            var testAssemblyDirectory = Path.GetDirectoryName(testAssembly.GetCodeBaseAsLocalPath());

            this.Discovery.ProcessAssemblies(testAssemblyDirectory, out _);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessAssemblies_ValidAssemblyThatDoesNotReferenceSdk_TypesNotEnumerated()
        {
            var files = new[] { "Blawp.dll", };
            this.FindFiles.enumerateFiles =
                (directory, searchPattern, searchOptions) =>
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
                (directory, searchPattern, searchOptions) =>
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
    }
}

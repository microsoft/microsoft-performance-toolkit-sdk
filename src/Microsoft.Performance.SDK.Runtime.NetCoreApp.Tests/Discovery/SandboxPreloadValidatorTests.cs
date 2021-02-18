// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Versioning;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Discovery
{
    [TestClass]
    public class SandboxPreloadValidatorTests
    {
        [TestMethod]
        [UnitTest]
        public void AssemblyNotFound_ReturnsFalseWithError()
        {
            var path = "not_there.dll";

            var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                new FakeVersionChecker());

            Assert.IsFalse(sut.IsAssemblyAcceptable(path, out var error));
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCodes.AssemblyLoadFailed, error.Code);
            Assert.AreEqual(ErrorCodes.AssemblyLoadFailed.Description, error.Message);
            Assert.AreEqual(path, error.Target);
        }

        [TestMethod]
        [UnitTest]
        public void AssemblyFound_Valid_ReturnsTrue()
        {
            var path = this.GetType().Assembly.Location;

            var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                new FakeVersionChecker());

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out var error));
            Assert.AreEqual(ErrorInfo.None, error);
        }


        [TestMethod]
        [UnitTest]
        public void AssemblyFound_ValidationFails_ReturnsFalse()
        {
            var path = this.GetType().Assembly.Location;
            var checker = new ConfigurableVersionChecker()
            {
                LowestSupportedSdkSetter = new SemanticVersion(999, 999, 999),
                SdkSetter = new SemanticVersion(999, 999, 999),
            };

            var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                checker);

            Assert.IsFalse(sut.IsAssemblyAcceptable(path, out var error));
            Assert.IsNotNull(error);
            Assert.AreEqual(ErrorCodes.SdkVersionIncompatible, error.Code);
            Assert.AreEqual(ErrorCodes.SdkVersionIncompatible.Description, error.Message);
            Assert.AreEqual(path, error.Target);
        }

        [TestMethod]
        [UnitTest]
        public void AssemblyCanBeLoadedAfterValidation()
        {
            var path = this.GetType().Assembly.Location;
            var checker = new FakeVersionChecker();

            var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                checker);

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out var error));
            Assert.AreEqual(ErrorInfo.None, error);

            var pluginLoader = new PluginsLoader(
                new IsolationAssemblyLoader(),
                x => new SandboxPreloadValidator(x, checker));
            var pluginsLoaded = pluginLoader.TryLoadPlugins(
                new[] { Path.GetDirectoryName(path), },
                out var errors);

            Assert.IsTrue(pluginsLoaded);
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }
    }
}

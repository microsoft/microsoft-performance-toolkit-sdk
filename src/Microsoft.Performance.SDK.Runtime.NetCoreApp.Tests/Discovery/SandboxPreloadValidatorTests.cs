// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private DirectoryInfo TestScratchDir { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.TestScratchDir = new DirectoryInfo(nameof(SandboxPreloadValidatorTests));

            this.Cleanup();

            this.TestScratchDir.Create();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                this.TestScratchDir.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        [TestMethod]
        [UnitTest]
        public void AssemblyNotFound_ReturnsFalseWithError()
        {
            var path = "not_there.dll";

            using var sut = new SandboxPreloadValidator(
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

            using var sut = new SandboxPreloadValidator(
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

            using var sut = new SandboxPreloadValidator(
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

            using var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                checker);

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out var error));
            Assert.AreEqual(ErrorInfo.None, error);

            var pluginLoader = new PluginsLoader(
                new IsolationAssemblyLoader(),
                x => new SandboxPreloadValidator(x, checker),
                Logger.Null);
            var pluginsLoaded = pluginLoader.TryLoadPlugins(
                new[] { Path.GetDirectoryName(path), },
                out var errors);

            Assert.IsTrue(pluginsLoaded);
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        [UnitTest]
        public void AssemblyCanBeVerifiedMultipleTimes()
        {
            var path = this.GetType().Assembly.Location;

            using var sut = new SandboxPreloadValidator(
                new[] { this.GetType().Assembly.Location, },
                new FakeVersionChecker());

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out var error));
            Assert.AreEqual(ErrorInfo.None, error);

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out error));
            Assert.AreEqual(ErrorInfo.None, error);

            Assert.IsTrue(sut.IsAssemblyAcceptable(path, out error));
            Assert.AreEqual(ErrorInfo.None, error);
        }

        [TestMethod]
        [UnitTest]
        public void SameAssemblyWithDifferentMvidValidates()
        {
            var a = this.TestScratchDir.CreateSubdirectory("a");
            var b = this.TestScratchDir.CreateSubdirectory("b");

            var friendlyName = nameof(SameAssemblyWithDifferentMvidValidates);
            var version = new Version(1, 2, 3, 4);

            var assembly1Path = Path.Combine(a.FullName, friendlyName + ".dll");
            var assembly2Path = Path.Combine(b.FullName, friendlyName + ".dll");

            CreateAssembly(version, assembly1Path);
            CreateAssembly(version, assembly2Path);

            using var sut = new SandboxPreloadValidator(
                new[]
                {
                    assembly1Path,
                    assembly2Path,
                },
                new FakeVersionChecker());

            Assert.IsTrue(sut.IsAssemblyAcceptable(assembly1Path, out var error));
            Assert.AreEqual(ErrorInfo.None, error);

            Assert.IsTrue(sut.IsAssemblyAcceptable(assembly2Path, out error));
            Assert.AreEqual(ErrorInfo.None, error);
        }

        private static void CreateAssembly(
            Version assemblyVersion,
            string outPath)
        {
            //
            // we would use Reflection.Emit to create these, but AssemblyBuilder.Save is
            // not present in .NET Core.
            // https://github.com/dotnet/runtime/issues/15704
            //

            var code = $@"
using System.Reflection;

[assembly: AssemblyVersion(""{assemblyVersion}"")]
namespace Test
{{
    public static class TestClass
    {{
    }}
}}";
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            
            var systemRefLocation = typeof(object).GetTypeInfo().Assembly.Location;
            var systemReference = MetadataReference.CreateFromFile(systemRefLocation);
            
            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(outPath))
                .WithOptions(
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(systemReference)
                .AddSyntaxTrees(tree);

            var compilationResult = compilation.Emit(outPath);

            Assert.IsTrue(compilationResult.Success, 
                string.Join(
                    Environment.NewLine,
                    compilationResult.Diagnostics));

            Assert.IsTrue(File.Exists(outPath));
        }
    }
}

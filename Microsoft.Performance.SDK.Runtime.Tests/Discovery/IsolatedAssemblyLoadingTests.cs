// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.Performance.SDK.Runtime.Discovery;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Discovery;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Discovery
{
    [TestClass]
    public class IsolatedAssemblyLoadingTests
    {
        private DirectoryInfo TestDir { get; set; }

        private List<string> SharedAssemblyNames { get; set; }

        private IsolationAssemblyLoader IsolationLoader { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.TestDir = Directory.CreateDirectory("TestDlls");
            this.SharedAssemblyNames = new List<string>
            {
                "Microsoft.Performance.SDK.Runtime",
            };

            this.IsolationLoader = new IsolationAssemblyLoader();
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                IsolationAssemblyLoader.ClearLoadContexts();
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                this.TestDir?.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void SharedAssembliesLoadFromDefaultContext()
        {
            //
            // Place a DLL we know we need to share in another folder.
            //

            var runtimeDll = typeof(AssemblyLoader).Assembly;
            var runtimeDllName = runtimeDll.GetName().Name;
            var runtimeDllFileName = Path.GetFileName(runtimeDll.GetCodeBaseAsLocalPath());

            var pluginDllFilePath = Path.GetFullPath(Path.Combine(this.TestDir.Name, Path.GetFileName(runtimeDllFileName)));
            if (!File.Exists(pluginDllFilePath))
            {
                File.Copy(runtimeDllFileName, pluginDllFilePath, true);
            }

            //
            // And then load said folder.
            //

            var assembly = IsolationLoader.LoadAssembly(pluginDllFilePath);

            //
            // Making sure that the shared DLL did not load into the child context.
            //

            Assert.IsTrue(AssemblyLoadContext.Default.Assemblies.Any(x => x.GetName().Name == runtimeDllName));
            Assert.AreEqual(
                Path.GetFullPath(runtimeDllFileName),
                AssemblyLoadContext.Default.Assemblies.Single(x => x.GetName().Name == runtimeDllName).GetCodeBaseAsLocalPath());
            Assert.IsFalse(IsolationAssemblyLoader.LoadContexts.ContainsKey(this.TestDir.FullName));
        }

        [TestMethod]
        [IntegrationTest]
        public void UnsharedAssembliesLoadIntoGivenContext()
        {
            //
            // Place a DLL into a plugin folder.
            //

            var notSharedDll = this.GetType().Assembly;
            var notSharedDllName = notSharedDll.GetName().Name;
            var notSharedDllFileName = Path.GetFileName(notSharedDll.GetCodeBaseAsLocalPath());

            var notSharedDllFilePath = Path.GetFullPath(Path.Combine(this.TestDir.Name, notSharedDllFileName));
            if (!File.Exists(notSharedDllFilePath))
            {
                File.Copy(notSharedDllFileName, notSharedDllFilePath, true);
            }

            //
            // And load without anything shared.
            //

            var assembly = IsolationLoader.LoadAssembly(notSharedDllFilePath);

            //
            // Making sure that the DLL loaded into the specified context.
            //

            var loadedDllPath = assembly.GetCodeBaseAsLocalPath();
            Assert.AreEqual(notSharedDllFilePath, loadedDllPath);
            Assert.IsTrue(IsolationAssemblyLoader.LoadContexts.ContainsKey(this.TestDir.FullName));

            var isolatedLoadContext = IsolationAssemblyLoader.LoadContexts[this.TestDir.FullName];
            Assert.IsTrue(isolatedLoadContext.Assemblies.Any(x => x.GetName().Name == notSharedDllName));
        }
        
        [TestMethod]
        [IntegrationTest]
        public void SharedAndNotSharedAssembliesLoadIntoCorrectContexts()
        {
            //
            // Place a DLL we know we need to share in another folder.
            //

            var sharedDll = typeof(AssemblyLoader).Assembly;
            var sharedDllName = sharedDll.GetName().Name;
            var sharedDllFileName = Path.GetFileName(sharedDll.GetCodeBaseAsLocalPath());

            var sharedPluginDllFileName = Path.GetFullPath(Path.Combine(this.TestDir.Name, Path.GetFileName(sharedDllFileName)));
            if (!File.Exists(sharedPluginDllFileName))
            {
                File.Copy(sharedDllFileName, sharedPluginDllFileName, true);
            }

            //
            // Place a DLL that we are not sharing into the plugin folder.
            //

            var notSharedDll = this.GetType().Assembly;
            var notSharedDllName = notSharedDll.GetName().Name;
            var notSharedDllFileName = Path.GetFileName(notSharedDll.GetCodeBaseAsLocalPath());

            var notSharedPluginDllFilePath = Path.GetFullPath(Path.Combine(this.TestDir.Name, Path.GetFileName(notSharedDllFileName)));
            if (!File.Exists(notSharedPluginDllFilePath))
            {
                File.Copy(notSharedDllFileName, notSharedPluginDllFilePath, true);
            }

            //
            // And then load said folder.
            //

            var sharedAssembly = this.IsolationLoader.LoadAssembly(sharedPluginDllFileName);
            var notSharedAssembly = this.IsolationLoader.LoadAssembly(notSharedPluginDllFilePath);

            //
            // Making sure that the shared DLL did not load into the child context.
            //

            Assert.IsTrue(IsolationAssemblyLoader.LoadContexts.ContainsKey(this.TestDir.FullName));
            var isolatedLoadContext = IsolationAssemblyLoader.LoadContexts[this.TestDir.FullName];

            Assert.IsTrue(AssemblyLoadContext.Default.Assemblies.Any(x => x.GetName().Name == sharedDllName));
            Assert.AreEqual(
                Path.GetFullPath(sharedDllFileName),
                AssemblyLoadContext.Default.Assemblies.Single(x => x.GetName().Name == sharedDllName).GetCodeBaseAsLocalPath());
            Assert.IsFalse(isolatedLoadContext.Assemblies.Any(x => x.GetName().Name == sharedDllName));

            //
            // and Making sure that the shared DLL did not load into the child context.
            //

            Assert.IsTrue(isolatedLoadContext.Assemblies.Any(x => x.GetName().Name == notSharedDllName));
            Assert.AreEqual(
                notSharedPluginDllFilePath,
                isolatedLoadContext.Assemblies.Single(x => x.GetName().Name == notSharedDllName).GetCodeBaseAsLocalPath());
        }
    }
}

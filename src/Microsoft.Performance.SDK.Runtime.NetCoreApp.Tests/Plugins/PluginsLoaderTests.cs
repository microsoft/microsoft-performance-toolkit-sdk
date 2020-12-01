﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources;

namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Plugins.Tests
{
    /// <summary>
    ///     Tests logic for loading plugins with the <see cref="PluginsLoader"/>. Both
    ///     valid and invalid filesystem structures are tested, since the loader allows
    ///     loading custom data sources from invalid folder schemas.
    ///     <para/>
    ///     The layout of .\MockCustomDataSources is translated 1:1 to compiled binaries
    ///     inside of a temp folder. Each custom data sources is compiled into its own
    ///     DLL, and can be individually loaded by passing in its parent directory.
    /// </summary>
    [TestClass]
    public class PluginsLoaderTests
    {
        private const string Invalid = "InvalidFolderSchema";

        private const string Valid = "ValidFolderSchema";

        private static string tempPath;

        private const string AsmVersionAttrText = "using System.Reflection;\n" +
                                                  "[assembly: AssemblyVersion(\"1.0.0\")]\n" +
                                                  "[assembly: AssemblyInformationalVersion(\"1.0.0\")]\n" +
                                                  "[assembly: AssemblyFileVersion(\"1.0.0\")]";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            // Folder in temp directory to store generated DLLs
            tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);

            var rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "MockCustomDataSources");

            // All references plugins need to compile
            var systemReference = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
            var sdkReference = MetadataReference.CreateFromFile(typeof(Processing.TableRowDetailEntry).GetTypeInfo().Assembly.Location);
            var netstandardReference = MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location);
            var runtimeReference = MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.0.0.0").Location);
            var reflectionReference = MetadataReference.CreateFromFile(Assembly.Load("System.Reflection").Location);

            var plugins = Directory.EnumerateFiles(rootDir, "*.cs", new EnumerationOptions { RecurseSubdirectories = true });
            foreach (var plugin in plugins)
            {
                var filename = new FileInfo(Path.ChangeExtension(plugin, "dll")).Name;
                var dllFolder = Path.Combine(tempPath, Path.GetRelativePath(rootDir, new FileInfo(plugin).Directory.FullName));

                if (!Directory.Exists(dllFolder))
                {
                    Directory.CreateDirectory(dllFolder);
                }

                // Read source code
                var code = File.ReadAllText(plugin);

                // Place assembly version attribute right before "namespace"
                var namespaceStartIndex = code.IndexOf("namespace Microsoft.Performance.SDK.Runtime.NetCoreApp.Tests.Plugins.MockCustomDataSources");
                code = code.Insert(namespaceStartIndex - 1, AsmVersionAttrText);

                // Compile the plugin
                var tree = SyntaxFactory.ParseSyntaxTree(code);
                var compilation = CSharpCompilation.Create(filename)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(systemReference, sdkReference, netstandardReference, runtimeReference, reflectionReference)
                    .AddSyntaxTrees(tree);
                using (MemoryStream dllStream = new MemoryStream())
                using (MemoryStream pdbStream = new MemoryStream())
                using (Stream win32resStream = compilation.CreateDefaultWin32Resources(
                                                            versionResource: true,
                                                            noManifest: false,
                                                            manifestContents: null,
                                                            iconInIcoFormat: null))
                {
                    var result = compilation.Emit(
                                    peStream: dllStream,
                                    pdbStream: pdbStream,
                                    win32Resources: win32resStream);
                    if (!result.Success)
                    {
                        foreach (Diagnostic codeIssue in result.Diagnostics)
                        {
                            string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}," +
                                $"Location: { codeIssue.Location.GetLineSpan()}," +
                                $"Severity: { codeIssue.Severity}";
                            Console.WriteLine(issue);
                        }

                        // If this Assert is hit, the SDK was likely updated to break
                        // the custom data source definitions used to test this class
                        Assert.IsTrue(false);
                    }
                    else
                    {
                        File.WriteAllBytes(Path.Combine(dllFolder, filename), dllStream.ToArray());
                    }
                }
            }
        }
        
        //
        // Invalid folder schema tests
        //

        [TestMethod]
        public void SingletonInvalidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var success = loader.TryLoadPlugin(GetInvalidPath("InvalidA"));
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer);

            var expected = new Type[] { typeof(InvalidSchemaA) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer);
            AssertLoadedCDSs(expected, loader);
            AssertObservedCDSs(expected, consumer);
        }

        [TestMethod]
        public void MultipleInvalidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var success = loader.TryLoadPlugins(new string[] { GetInvalidPath("InvalidA"), GetInvalidPath("InvalidB") }, out _);
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer);

            var expected = new Type[] { typeof(InvalidSchemaA), typeof(InvalidSchemaB) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer);
            AssertLoadedCDSs(expected, loader);
            AssertObservedCDSs(expected, consumer);
        }

        [TestMethod]
        public void RepeatedCDSInvalidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            // Load same plugin twice
            var success = loader.TryLoadPlugin(GetInvalidPath("InvalidA"));
            Assert.IsTrue(success);

            success = loader.TryLoadPlugin(GetInvalidPath("InvalidA"));
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer);

            // Assert same CDS is loaded/observed only once
            var expected = new Type[] { typeof(InvalidSchemaA) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer);
            AssertLoadedCDSs(expected, loader);
            AssertObservedCDSs(expected, consumer);
        }

        [TestMethod]
        public void LoadRootInvalid()
        {
            (var loader, var consumer) = Setup(true);

            var success = loader.TryLoadPlugin(Path.Combine(tempPath, Invalid));
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer);

            var expected = new Type[] { typeof(InvalidSchemaA), typeof(InvalidSchemaB) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer);
            AssertLoadedCDSs(expected, loader);
            AssertObservedCDSs(expected, consumer);
        }

        //
        // Valid folder schema tests
        //

        [TestMethod]
        public void SingletonValidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var version = Version.Parse("1.0.0");
            var success = loader.TryLoadPlugin(GetValidPath("ValidA", version));
            Assert.IsTrue(success);

            var expectedPlugins = new List<Tuple<string, Version>>
            {
                new Tuple<string, Version>("ValidA", version),
            };
            AssertExpectedPlugins(expectedPlugins, consumer);

            var expectedCDSs = new Type[] { typeof(ValidSchemaA1) };
            AssertNumberCustomDataSourcesLoaded(expectedCDSs.Length, loader, consumer);
            AssertLoadedCDSs(expectedCDSs, loader);
            AssertObservedCDSs(expectedCDSs, consumer);
        }

        [TestMethod]
        public void MultipleValidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var versionA = Version.Parse("1.0.0");
            var versionB = Version.Parse("1.2.3");
            var success = loader.TryLoadPlugins(new string[] { GetValidPath("ValidA", versionA), GetValidPath("ValidB", versionB) }, out _);
            Assert.IsTrue(success);

            var expectedPlugins = new List<Tuple<string, Version>>
            {
                new Tuple<string, Version>("ValidA", versionA),
                new Tuple<string, Version>("ValidB", versionB),
            };
            AssertExpectedPlugins(expectedPlugins, consumer);

            var expectedCDSs = new Type[] { typeof(ValidSchemaA1), typeof(ValidSchemaB) };
            AssertNumberCustomDataSourcesLoaded(expectedCDSs.Length, loader, consumer);
            AssertLoadedCDSs(expectedCDSs, loader);
            AssertObservedCDSs(expectedCDSs, consumer);
        }

        [TestMethod]
        public void RepeatedValidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var version = Version.Parse("1.0.0");

            // Load plugin
            var success = loader.TryLoadPlugin(GetValidPath("ValidA", version));
            Assert.IsTrue(success);

            // Load same plugin again
            success = loader.TryLoadPlugin(GetValidPath("ValidA", version));
            Assert.IsTrue(success);

            // Assert loaded only has one copy
            var expectedPlugins = new List<Tuple<string, Version>>
            {
                new Tuple<string, Version>("ValidA", version),
            };
            AssertExpectedPlugins(expectedPlugins, consumer);

            var expectedCDSs = new Type[] { typeof(ValidSchemaA1) };
            AssertNumberCustomDataSourcesLoaded(expectedCDSs.Length, loader, consumer);
            AssertLoadedCDSs(expectedCDSs, loader);
            AssertObservedCDSs(expectedCDSs, consumer);
        }

        [TestMethod]
        public void DifferentVersionsValidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var version1 = Version.Parse("1.0.0");
            var version2 = Version.Parse("2.0.0");
            var success = loader.TryLoadPlugins(new string[] { GetValidPath("ValidA", version1), GetValidPath("ValidA", version2) }, out _);
            Assert.IsTrue(success);

            var expectedPlugins = new List<Tuple<string, Version>>
            {
                new Tuple<string, Version>("ValidA", version1),
                new Tuple<string, Version>("ValidA", version2),
            };
            AssertExpectedPlugins(expectedPlugins, consumer);

            var expectedCDSs = new Type[] { typeof(ValidSchemaA1), typeof(ValidSchemaA2) };
            AssertNumberCustomDataSourcesLoaded(expectedCDSs.Length, loader, consumer);
            AssertLoadedCDSs(expectedCDSs, loader);
            AssertObservedCDSs(expectedCDSs, consumer);
        }

        [TestMethod]
        public void DifferentVersionsRepeatCDSValidSchemaPlugin()
        {
            (var loader, var consumer) = Setup(true);

            var version2 = Version.Parse("2.0.0");
            var version3 = Version.Parse("3.0.0");
            var success = loader.TryLoadPlugins(new string[] { GetValidPath("ValidA", version2), GetValidPath("ValidA", version3) }, out _);
            Assert.IsTrue(success);

            var expectedPlugins = new List<Tuple<string, Version>>
            {
                new Tuple<string, Version>("ValidA", version2),
                new Tuple<string, Version>("ValidA", version3),
            };
            AssertExpectedPlugins(expectedPlugins, consumer);

            // Only one instance of A2 should be loaded, even though both v2 and v3 of ValidA
            // contain its definition.
            var expectedCDSs = new Type[] { typeof(ValidSchemaA2), typeof(ValidSchemaA3) };
            AssertNumberCustomDataSourcesLoaded(expectedCDSs.Length, loader, consumer);
            AssertLoadedCDSs(expectedCDSs, loader);
            AssertObservedCDSs(expectedCDSs, consumer);
        }

        /// <summary>
        ///     Checks that every expected plugin in <paramref name="expected"/>
        ///     was observed by <paramref name="consumer"/>
        /// </summary>
        private void AssertExpectedPlugins(List<Tuple<string, Version>> expected, MockPluginsConsumer consumer)
        {
            foreach (var plugin in expected)
            {
                Assert.IsTrue(consumer.ObservedDataSourcesByPluginName.ContainsKey(plugin.Item1));
                Assert.IsTrue(consumer.ObservedPluginVersionsByPluginName.ContainsKey(plugin.Item1));
                Assert.IsTrue(consumer.ObservedPluginVersionsByPluginName[plugin.Item1].Contains(plugin.Item2));
            }
        }

        //
        // Subscribe/Unsubscribe tests
        //

        [TestMethod]
        public void LateSubscribeTest()
        {
            // Defer subscribing consumer1
            (var loader, var consumer1) = Setup(false);

            // Load first plugin
            var success = loader.TryLoadPlugin(GetInvalidPath("InvalidA"));
            Assert.IsTrue(success);

            // Check loader processed plugin
            var expected = new Type[] { typeof(InvalidSchemaA) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader);
            AssertLoadedCDSs(expected, loader);

            // Subscribe first consumer (i.e. a "late" subscription)
            loader.Subscribe(consumer1);

            // Assert first plugin observed through subscribe
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer1);
            AssertObservedCDSs(expected, consumer1);

            // Subscribe a new consumer
            var consumer2 = new MockPluginsConsumer();
            loader.Subscribe(consumer2);

            // Assert new consumer was given first plugin
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer2);
            AssertObservedCDSs(expected, consumer2);

            // Load second plugin
            success = loader.TryLoadPlugin(GetInvalidPath("InvalidB"));
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer1);

            // Assert both plugins observed by both consumers
            expected = new Type[] { typeof(InvalidSchemaA), typeof(InvalidSchemaB) };
            AssertNumberCustomDataSourcesLoaded(expected.Length, loader, consumer1, consumer2);
            AssertLoadedCDSs(expected, loader);
            AssertObservedCDSs(expected, consumer1, consumer2);
        }

        [TestMethod]
        public void UnsubscribeTest()
        {
            (var loader, var consumer) = Setup(true);

            // Load first plugin
            var success = loader.TryLoadPlugin(GetInvalidPath("InvalidA"));
            Assert.IsTrue(success);

            // Assert first plugin processed correctly
            var expected1 = new Type[] { typeof(InvalidSchemaA) };
            AssertNumberCustomDataSourcesLoaded(expected1.Length, loader, consumer);
            AssertLoadedCDSs(expected1, loader);
            AssertObservedCDSs(expected1, consumer);

            // Unsubscribe consumer
            loader.Unsubscribe(consumer);

            // Load second plugin
            success = loader.TryLoadPlugin(GetInvalidPath("InvalidB"));
            Assert.IsTrue(success);

            AssertNoPluginNames(consumer);

            // Assert consumer did not see second plugin, but loader advertises it
            var expected2 = new Type[] { typeof(InvalidSchemaA), typeof(InvalidSchemaB) };
            AssertNumberCustomDataSourcesLoaded(expected2.Length, loader);

            Assert.AreEqual(expected1.Length, consumer.ObservedDataSources.Count);
            AssertLoadedCDSs(expected2, loader);
            AssertObservedCDSs(expected1, consumer);
        }

        /// <summary>
        ///     Creates the loader and consumer, subscribing the consumer if requested
        /// </summary>
        private static ValueTuple<PluginsLoader, MockPluginsConsumer> Setup(bool subscribe)
        {
            var loader = new PluginsLoader();
            var consumer = new MockPluginsConsumer();
            if (subscribe)
            {
                loader.Subscribe(consumer);
            }

            return (loader, consumer);
        }

        /// <summary>
        ///     Gets the path to a plugin inside the "InvalidFolderSchema" folder
        /// </summary>
        private static string GetInvalidPath(string pluginName)
        {
            return Path.Combine(tempPath, Invalid, pluginName);
        }

        /// <summary>
        ///     Gets the path to a plugin inside the "ValidFolderSchema" folder
        /// </summary>
        private static string GetValidPath(string pluginName, Version pluginVersion)
        {
            return Path.Combine(tempPath, Valid, "InstalledPlugins", "1.0.0", pluginName, pluginVersion.ToString(3));
        }

        /// <summary>
        ///     Checks that <paramref name="loader"/> has loaded and that each consumer in
        ///     <paramref name="consumers"/> has observed <paramref name="expected"/>
        ///     custom data sources 
        /// </summary>
        private static void AssertNumberCustomDataSourcesLoaded(int expected, PluginsLoader loader, params MockPluginsConsumer[] consumers)
        {
            Assert.AreEqual(expected, loader.LoadedCustomDataSources.Count());
            foreach (var consumer in consumers)
            {
                Assert.AreEqual(expected, consumer.ObservedDataSources.Count);
            }
        }

        /// <summary>
        ///     Checks that every custom data source defined in <paramref name="types"/>
        ///     was loaded by <paramref name="loader"/>
        /// </summary>
        private static void AssertLoadedCDSs(Type[] types, PluginsLoader loader)
        {
            AssertCDSs(types, loader.LoadedCustomDataSources);
        }

        /// <summary>
        ///     Checks that every custom data source defined in <paramref name="types"/>
        ///     was observed by each consumer in <paramref name="consumers"/>
        /// </summary>
        private static void AssertObservedCDSs(Type[] types, params MockPluginsConsumer[] consumers)
        {
            foreach (var consumer in consumers)
            {
                AssertCDSs(types, consumer.ObservedDataSources);
            }
        }

        /// <summary>
        ///     Checks that every custom data source defined in <paramref name="types"/>
        ///     is present in <paramref name="actualCDSs"/>
        /// </summary>
        private static void AssertCDSs(Type[] types, IEnumerable<CustomDataSourceReference> actualCDSs)
        {
            var expectedCDSs = types.Select(type => { CustomDataSourceReference.TryCreateReference(type, out var cds); return cds; });
            var expectedSorted = expectedCDSs.OrderBy(cdsr => cdsr.Guid).ToList();

            var actualSorted = actualCDSs.OrderBy(cdsr => cdsr.Guid).ToList();

            for (int i = 0; i < expectedSorted.Count; i++)
            {
                var expected = expectedSorted[i];
                var actual = actualSorted[i];

                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Guid, actual.Guid);
            }
        }

        /// <summary>
        ///     Checks that <paramref name="consumer"/> has not observed any plugin names.
        ///     <para/>
        ///     This means two invariants must be true:
        ///         1) <see cref="MockPluginsConsumer.ObservedDataSourcesByPluginName"/> only has one
        ///            entry for the empty string
        /// </summary>
        private void AssertNoPluginNames(MockPluginsConsumer consumer)
        {
            Assert.AreEqual(1, consumer.ObservedDataSourcesByPluginName.Count);
            Assert.AreEqual(0, consumer.ObservedPluginVersionsByPluginName.Count);
            Assert.IsTrue(consumer.ObservedDataSourcesByPluginName.Keys.First() == "");
        }
    }

    /// <summary>
    ///     A simple plugins consumer that just keeps track of what it has seen loaded
    /// </summary>
    internal class MockPluginsConsumer
    : IPluginsConsumer
    {
        public Dictionary<string, List<CustomDataSourceReference>> ObservedDataSourcesByPluginName { get; }

        public Dictionary<string, List<Version>> ObservedPluginVersionsByPluginName { get; }

        public List<CustomDataSourceReference> ObservedDataSources { get; }

        public MockPluginsConsumer()
        {
            this.ObservedDataSourcesByPluginName = new Dictionary<string, List<CustomDataSourceReference>>();
            this.ObservedPluginVersionsByPluginName = new Dictionary<string, List<Version>>();
            this.ObservedDataSources = new List<CustomDataSourceReference>();
        }

        public void OnCustomDataSourceLoaded(string pluginName, Version pluginVersion, CustomDataSourceReference customDataSource)
        {
            var name = (pluginName != null) ? pluginName : "";

            this.ObservedDataSources.Add(customDataSource);

            // Keep track of every CDSR loaded for a given plugin name,
            // even the ones loaded without a name
            if (!this.ObservedDataSourcesByPluginName.TryGetValue(name, out var loaded))
            {
                loaded = new List<CustomDataSourceReference>();
                this.ObservedDataSourcesByPluginName.Add(name, loaded);
            }
            loaded.Add(customDataSource);

            // If a name was provided, keep track of the version
            if (pluginName != null)
            {
                if (!this.ObservedPluginVersionsByPluginName.TryGetValue(name, out var versions))
                {
                    versions = new List<Version>();
                    this.ObservedPluginVersionsByPluginName.Add(name, versions);
                }
                versions.Add(pluginVersion);
            }
        }
    }
}
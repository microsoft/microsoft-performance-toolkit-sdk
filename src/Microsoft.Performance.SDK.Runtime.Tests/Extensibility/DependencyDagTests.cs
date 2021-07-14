// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DependencyDagTests
    {
        [TestMethod]
        [UnitTest]
        public void Create_NullParameters_Throw()
        {
            Assert.ThrowsException<ArgumentNullException>(() => DependencyDag.Create(null, new TestDataExtensionRepository()));
            Assert.ThrowsException<ArgumentNullException>(() => DependencyDag.Create(new TestPluginCatalog(), null));
        }

        [TestMethod]
        [UnitTest]
        public void Create_UnloadedCatalog_Throws()
        {
            var catalog = new TestPluginCatalog { IsLoaded = false, };
            var repo = new DataExtensionRepository();
            repo.FinalizeDataExtensions();

            Assert.ThrowsException<ArgumentException>(() => DependencyDag.Create(catalog, repo));
        }

        [TestMethod]
        [UnitTest]
        public void Create_NoDependencies_EverythingIsRoot()
        {
            var sourceCooker1 = new TestSourceDataCookerReference();
            var sourceCooker2 = new TestSourceDataCookerReference();
            var dataProcessor = new TestDataProcessorReference();
            var table = new TestTableExtensionReference();

            var cds1 = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.ProcessingSourceAttribute(),
                new HashSet<Processing.DataSourceAttribute>());

            var repo = new DataExtensionRepository();
            repo.TryAddReference(sourceCooker1);
            repo.TryAddReference(sourceCooker2);
            repo.AddDataProcessorReference(dataProcessor);
            repo.AddTableExtensionReference(table);
            repo.FinalizeDataExtensions();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;
            catalog.PlugIns = new[] { cds1, };

            var allReferences = new[]
            {
                DependencyDag.Reference.Create(sourceCooker1),
                DependencyDag.Reference.Create(sourceCooker2),
                DependencyDag.Reference.Create(dataProcessor),
                DependencyDag.Reference.Create(table),
                DependencyDag.Reference.Create(cds1),
            };

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Length, sut.All.Count);
            Assert.AreEqual(allReferences.Length, sut.Roots.Count);
            CollectionAssert.AreEquivalent(sut.All.ToList(), sut.Roots.ToList());
            CollectionAssert.AreEquivalent(sut.All.Select(x => x.Target).ToList(), allReferences);

            foreach (var n in sut.All)
            {
                Assert.AreEqual(0, n.Dependencies.Count);
                Assert.AreEqual(0, n.Dependents.Count);
            }
        }

        [TestMethod]
        [UnitTest]
        public void Create_SimpleDependencies_CreatesCorrectly()
        {
            //
            //    r0     r1    r2
            //   /   \     \  /
            //  d1   d2    d1
            //

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r1d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r2d1 = r1d1;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d1.Path);

            var allRoots = new IDataExtensionReference[]
            {
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d2,
                    r1d1,
                    r2d1,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r2d1);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r2d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d1));

            Assert.AreEqual(2, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));
            Assert.IsTrue(r0n.Dependencies.Contains(r0d2n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d1n));

            Assert.AreEqual(0, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));

            Assert.AreEqual(0, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependents.Contains(r0n));

            Assert.AreEqual(r1d1n, r2d1n);
            Assert.AreEqual(0, r1d1n.Dependencies.Count);
            Assert.AreEqual(2, r1d1n.Dependents.Count);
            Assert.IsTrue(r1d1n.Dependents.Contains(r1n));
            Assert.IsTrue(r1d1n.Dependents.Contains(r2n));
        }

        [TestMethod]
        [UnitTest]
        public void Create_DependencyChain_CreatesCorrectly()
        {
            //
            //    r0     r1    r2
            //   /   \     \  /
            //  d1   d2    d1
            //  |
            //  d3

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d1d3 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1d3"), };
            var r1d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r2d1 = r1d1;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d1d3.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d1.Path);

            var allRoots = new IDataExtensionReference[]
            {
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d2,
                    r0d1d3,
                    r1d1,
                    r2d1,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r0d1d3);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r2d1);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r0d1d3n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1d3));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r2d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d1));

            Assert.AreEqual(2, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));
            Assert.IsTrue(r0n.Dependencies.Contains(r0d2n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d1n));

            Assert.AreEqual(1, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));
            Assert.IsTrue(r0d1n.Dependencies.Contains(r0d1d3n));

            Assert.AreEqual(0, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependents.Contains(r0n));

            Assert.AreEqual(0, r0d1d3n.Dependencies.Count);
            Assert.AreEqual(1, r0d1d3n.Dependents.Count);
            Assert.IsTrue(r0d1d3n.Dependents.Contains(r0d1n));

            Assert.AreEqual(r1d1n, r2d1n);
            Assert.AreEqual(0, r1d1n.Dependencies.Count);
            Assert.AreEqual(2, r1d1n.Dependents.Count);
            Assert.IsTrue(r1d1n.Dependents.Contains(r1n));
            Assert.IsTrue(r1d1n.Dependents.Contains(r2n));
        }

        [TestMethod]
        [UnitTest]
        public void Create_DependencySkip_CreatesCorrectly()
        {
            //
            //    r0     r1   r2
            //   /   \    |  /  
            //  d1   d2  d1 /
            //            |/
            //           d2

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r1d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r1d1d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1d2"), };
            var r2d2 = r1d1d2;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d2.Path);
            r1d1.requiredDataCookers.Add(r1d1d2.Path);

            var allRoots = new IDataExtensionReference[]
            {
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d2,
                    r1d1,
                    r1d1d2,
                    r2d2,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r1d1d2);
            repo.TryAddReference(r2d2);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r1d1d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1d2));
            var r2d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d2));

            Assert.AreEqual(2, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));
            Assert.IsTrue(r0n.Dependencies.Contains(r0d2n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d2n));

            Assert.AreEqual(0, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));

            Assert.AreEqual(0, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependents.Contains(r0n));

            Assert.AreEqual(r1d1d2n, r2d2n);
            Assert.AreEqual(0, r1d1d2n.Dependencies.Count);
            Assert.AreEqual(2, r1d1d2n.Dependents.Count);
            Assert.IsTrue(r1d1d2n.Dependents.Contains(r1d1n));
            Assert.IsTrue(r1d1d2n.Dependents.Contains(r2n));
        }

        [TestMethod]
        [UnitTest]
        public void Create_TransitiveDependenciesPruned()
        {
            //
            //   r0 --       r0
            //  / \ \  \      | \
            // d1  | | d4 => d1 d4
            //  |\ / | |      |  |
            //  | d2 | |     d2 /
            //  | / /  |      |/
            // d3 -+--/      d3
            //

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d3 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d3"), };
            var r0d4 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d4"), };

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r0.requiredDataCookers.Add(r0d3.Path);
            r0.requiredDataCookers.Add(r0d4.Path);
            r0d1.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d3.Path);
            r0d2.requiredDataCookers.Add(r0d3.Path);
            r0d4.requiredDataCookers.Add(r0d3.Path);

            var allRoots = new[]
            {
                r0,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d2,
                    r0d3,
                    r0d4,
                }.Select(DependencyDag.Reference.Create))
                .ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r0d3);
            repo.TryAddReference(r0d4);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r0d3n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d3));
            var r0d4n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d4));

            Assert.AreEqual(2, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));
            Assert.IsTrue(r0n.Dependencies.Contains(r0d4n));

            Assert.AreEqual(1, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependencies.Contains(r0d2n));
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));

            Assert.AreEqual(1, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependencies.Contains(r0d3n));
            Assert.IsTrue(r0d2n.Dependents.Contains(r0d1n));

            Assert.AreEqual(0, r0d3n.Dependencies.Count);
            Assert.AreEqual(2, r0d3n.Dependents.Count);
            Assert.IsTrue(r0d3n.Dependents.Contains(r0d2n));
            Assert.IsTrue(r0d3n.Dependents.Contains(r0d4n));
        }

        [TestMethod]
        [UnitTest]
        public void CyclesNotAllowed()
        {
            // 
            //  r0 <-<-<
            //  |      |
            //  d1     ^
            //  |  \   |
            //  d2 d3 ->
            //

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d3 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d3"), };

            r0.requiredDataCookers.Add(r0d1.Path);
            r0d1.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d3.Path);
            r0d3.requiredDataCookers.Add(r0.Path);

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r0d3);
            repo.FinalizeDataExtensions();

            var e = Assert.ThrowsException<InvalidOperationException>(() => DependencyDag.Create(catalog, repo));

            var expectedCycleString = string.Format(
                "Cycle: {0} -> {1} -> {2} -> {0}",
                r0.Path,
                r0d1.Path,
                r0d3.Path);

            StringAssert.Contains(e.Message, expectedCycleString);
        }

        [TestMethod]
        [UnitTest]
        public void DeeperCyclesNotAllowed()
        {
            // 
            //  r0 
            //  |
            //  d1 <-<-<
            //  |  \   |
            //  d2 d3 ->
            //

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d3 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d3"), };

            r0.requiredDataCookers.Add(r0d1.Path);
            r0d1.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d3.Path);
            r0d3.requiredDataCookers.Add(r0d1.Path);

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r0d3);
            repo.FinalizeDataExtensions();

            var e = Assert.ThrowsException<InvalidOperationException>(() => DependencyDag.Create(catalog, repo));

            var expectedCycleString = string.Format(
                "Cycle: {0} -> {1} -> {2} -> {0}",
                r0.Path,
                r0d1.Path,
                r0d3.Path);

            StringAssert.Contains(e.Message, expectedCycleString);
        }

        [TestMethod]
        [UnitTest]
        public void Create_WhenProcessorsWithParsersPresent_SourcesDependOnCds()
        {
            //
            //     cc1
            //    /   \
            //  sc1    sc2  sc3
            //    \   /     |
            //      r ------/      sc4


            var sp = new TestSourceParser { Id = "sp", };
            var p = new TestCustomDataProcessor(
                sp,
                ProcessorOptions.Default,
                new TestApplicationEnvironment { SourceSessionFactory = new TestSourceSessionFactory(), },
                new TestProcessorEnvironment(),
                new Dictionary<TableDescriptor, Action<ITableBuilder, IDataExtensionRetrieval>>(),
                Array.Empty<TableDescriptor>());

            var cds = new FakeProcessingSource();
            var r = new ProcessingSourceReference(
                typeof(FakeProcessingSource), 
                () => cds, 
                Any.ProcessingSourceAttribute(), 
                new HashSet<Processing.DataSourceAttribute>());

            cds.CreateProcessorReturnValue = p;
            r.CreateProcessor(
                new[]
                {
                    Any.DataSource(),
                },
                Any.ProcessorEnvironment(), 
                ProcessorOptions.Default);

            var sc1 = new TestSourceDataCookerReference { Path = new DataCookerPath(p.SourceParserId, "sc1"), };
            var sc2 = new TestSourceDataCookerReference { Path = new DataCookerPath(p.SourceParserId, "sc2"), };
            var sc3 = new TestSourceDataCookerReference { Path = new DataCookerPath(p.SourceParserId, "sc3"), };
            var sc4 = new TestSourceDataCookerReference { Path = new DataCookerPath("not-there", "sc4"), };
            var cc1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("cc1"), };

            cc1.requiredDataCookers.Add(sc1.Path);
            cc1.requiredDataCookers.Add(sc2.Path);

            var catalog = new TestPluginCatalog
            {
                PlugIns = new[] { r, },
                IsLoaded = true,
            };

            var repo = new DataExtensionRepository();
            repo.TryAddReference(sc1);
            repo.TryAddReference(sc2);
            repo.TryAddReference(sc3);
            repo.TryAddReference(sc4);
            repo.TryAddReference(cc1);
            repo.FinalizeDataExtensions();

            var allRoots = new[]
            {
                DependencyDag.Reference.Create(cc1),
                DependencyDag.Reference.Create(sc3),
                DependencyDag.Reference.Create(sc4),
            }.ToSet();

            var allReferences = new[]
            {
                DependencyDag.Reference.Create(sc2),
                DependencyDag.Reference.Create(sc3),
            }
            .Concat(allRoots)
            .ToSet();

            var sut = DependencyDag.Create(catalog, repo);

            sut.DependentWalk(Console.WriteLine);

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            var rn = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r));
            var sc1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(sc1));
            var sc2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(sc2));
            var sc3n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(sc3));
            var sc4n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(sc4));
            var cc1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(cc1));

            Assert.AreEqual(0, rn.Dependencies.Count);
            Assert.AreEqual(3, rn.Dependents.Count);
            Assert.IsTrue(rn.Dependents.Contains(sc1n));
            Assert.IsTrue(rn.Dependents.Contains(sc2n));
            Assert.IsTrue(rn.Dependents.Contains(sc3n));

            Assert.AreEqual(1, sc1n.Dependencies.Count);
            Assert.IsTrue(sc1n.Dependencies.Contains(rn));
            Assert.AreEqual(1, sc1n.Dependents.Count);
            Assert.IsTrue(sc1n.Dependents.Contains(cc1n));

            Assert.AreEqual(1, sc2n.Dependencies.Count);
            Assert.IsTrue(sc2n.Dependencies.Contains(rn));
            Assert.AreEqual(1, sc2n.Dependents.Count);
            Assert.IsTrue(sc2n.Dependents.Contains(cc1n));

            Assert.AreEqual(1, sc3n.Dependencies.Count);
            Assert.IsTrue(sc3n.Dependencies.Contains(rn));
            Assert.AreEqual(0, sc3n.Dependents.Count);

            Assert.AreEqual(0, sc4n.Dependencies.Count);
            Assert.AreEqual(0, sc4n.Dependents.Count);

            Assert.AreEqual(2, cc1n.Dependencies.Count);
            Assert.IsTrue(cc1n.Dependencies.Contains(sc1n));
            Assert.IsTrue(cc1n.Dependencies.Contains(sc2n));
            Assert.AreEqual(0, cc1n.Dependents.Count);
        }

        [TestMethod]
        [UnitTest]
        public void Create_MissingDirect_CreatesCorrectly()
        {
            //
            //    r0        r1    r2
            //   /   \        \  /
            //  d1   d2 (X)    d1
            //  |
            //  d3

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d1d3 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1d3"), };
            var r1d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r2d1 = r1d1;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d1d3.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d1.Path);
            var allRoots = new IDataExtensionReference[]
            {
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d1d3,
                    r1d1,
                    r2d1,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d1d3);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r2d1);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d1d3n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1d3));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r2d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d1));

            Assert.AreEqual(1, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d1n));

            Assert.AreEqual(1, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));
            Assert.IsTrue(r0d1n.Dependencies.Contains(r0d1d3n));

            Assert.AreEqual(0, r0d1d3n.Dependencies.Count);
            Assert.AreEqual(1, r0d1d3n.Dependents.Count);
            Assert.IsTrue(r0d1d3n.Dependents.Contains(r0d1n));

            Assert.AreEqual(r1d1n, r2d1n);
            Assert.AreEqual(0, r1d1n.Dependencies.Count);
            Assert.AreEqual(2, r1d1n.Dependents.Count);
            Assert.IsTrue(r1d1n.Dependents.Contains(r1n));
            Assert.IsTrue(r1d1n.Dependents.Contains(r2n));
        }

        [TestMethod]
        [UnitTest]
        public void Create_MissingIndirect_CreatesCorrectly()
        {
            //
            //    r0     r1    r2
            //   /   \     \  /
            //  d1   d2    d1
            //  |
            //  d3 (X)

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d1d3 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1d3"), };
            var r1d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r2d1 = r1d1;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d1d3.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d1.Path);

            var allRoots = new IDataExtensionReference[]
            {
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d1,
                    r0d2,
                    r1d1,
                    r2d1,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d1);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r2d1);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r2d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d1));

            Assert.AreEqual(2, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d1n));
            Assert.IsTrue(r0n.Dependencies.Contains(r0d2n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d1n));

            Assert.AreEqual(0, r0d1n.Dependencies.Count);
            Assert.AreEqual(1, r0d1n.Dependents.Count);
            Assert.IsTrue(r0d1n.Dependents.Contains(r0n));

            Assert.AreEqual(0, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependents.Contains(r0n));

            Assert.AreEqual(r1d1n, r2d1n);
            Assert.AreEqual(0, r1d1n.Dependencies.Count);
            Assert.AreEqual(2, r1d1n.Dependents.Count);
            Assert.IsTrue(r1d1n.Dependents.Contains(r1n));
            Assert.IsTrue(r1d1n.Dependents.Contains(r2n));
        }

        [TestMethod]
        [UnitTest]
        public void Create_MissingThatHasChildrenPresent_CreatesCorrectly()
        {
            //
            //     r0      r1    r2         d3  r0  r1  r2
            //   /    \      \  /     =>         |   \  /
            //  d1 (X) d2     d1                d2    d1
            //  |
            //  d3
            //

            var r0 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r1"), };
            var r2 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r2"), };

            var r0d1 = new TestCompositeDataCookerReference { Path = new DataCookerPath("r0d1"), };
            var r0d2 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d2"), };
            var r0d1d3 = new TestSourceDataCookerReference { Path = new DataCookerPath("r0d1d3"), };
            var r1d1 = new TestSourceDataCookerReference { Path = new DataCookerPath("r1d1"), };
            var r2d1 = r1d1;

            r0.requiredDataCookers.Add(r0d1.Path);
            r0.requiredDataCookers.Add(r0d2.Path);
            r0d1.requiredDataCookers.Add(r0d1d3.Path);
            r1.requiredDataCookers.Add(r1d1.Path);
            r2.requiredDataCookers.Add(r2d1.Path);

            var allRoots = new IDataExtensionReference[]
            {
                r0d1d3,
                r0,
                r1,
                r2,
            }.Select(DependencyDag.Reference.Create).ToSet();

            var allReferences = allRoots.Concat(
                new IDataExtensionReference[]
                {
                    r0d2,
                    r1d1,
                    r2d1,
                }.Select(DependencyDag.Reference.Create)).ToSet();

            var catalog = new TestPluginCatalog();
            catalog.IsLoaded = true;

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(r2);
            repo.TryAddReference(r0d2);
            repo.TryAddReference(r0d1d3);
            repo.TryAddReference(r1d1);
            repo.TryAddReference(r2d1);
            repo.FinalizeDataExtensions();

            var sut = DependencyDag.Create(catalog, repo);

            Assert.AreEqual(allReferences.Count, sut.All.Count);
            CollectionAssert.AreEquivalent(allReferences.ToList(), sut.All.Select(x => x.Target).ToList());

            Assert.AreEqual(allRoots.Count, sut.Roots.Count);
            CollectionAssert.AreEquivalent(allRoots.ToList(), sut.Roots.Select(x => x.Target).ToList());

            foreach (var r in sut.Roots)
            {
                Assert.AreEqual(0, r.Dependents.Count, r.Target.Match(x => x.Name, x => x.Name));
            }

            var r0n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0));
            var r1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1));
            var r2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2));
            var r0d2n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d2));
            var r0d1d3n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r0d1d3));
            var r1d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r1d1));
            var r2d1n = sut.All.Single(x => x.Target == DependencyDag.Reference.Create(r2d1));

            Assert.AreEqual(1, r0n.Dependencies.Count);
            Assert.AreEqual(0, r0n.Dependents.Count);
            Assert.IsTrue(r0n.Dependencies.Contains(r0d2n));

            Assert.AreEqual(1, r1n.Dependencies.Count);
            Assert.AreEqual(0, r1n.Dependents.Count);
            Assert.IsTrue(r1n.Dependencies.Contains(r1d1n));

            Assert.AreEqual(1, r2n.Dependencies.Count);
            Assert.AreEqual(0, r2n.Dependents.Count);
            Assert.IsTrue(r2n.Dependencies.Contains(r2d1n));

            Assert.AreEqual(0, r0d2n.Dependencies.Count);
            Assert.AreEqual(1, r0d2n.Dependents.Count);
            Assert.IsTrue(r0d2n.Dependents.Contains(r0n));

            Assert.AreEqual(0, r0d1d3n.Dependencies.Count);
            Assert.AreEqual(0, r0d1d3n.Dependents.Count);

            Assert.AreEqual(r1d1n, r2d1n);
            Assert.AreEqual(0, r1d1n.Dependencies.Count);
            Assert.AreEqual(2, r1d1n.Dependents.Count);
            Assert.IsTrue(r1d1n.Dependents.Contains(r1n));
            Assert.IsTrue(r1d1n.Dependents.Contains(r2n));
        }
    }
}

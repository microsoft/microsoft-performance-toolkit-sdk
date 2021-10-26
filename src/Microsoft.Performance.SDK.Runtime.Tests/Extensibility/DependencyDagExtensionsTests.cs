// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DependencyDagExtensionsTests
    {
        [TestMethod]
        [UnitTest]
        public void DependentWalk_NullParameters_Throw()
        {
            var catalog = new TestProcessingSourceCatalog { IsLoaded = true, };
            var repo = new DataExtensionRepository();
            repo.FinalizeDataExtensions();

            var dag = DependencyDag.Create(catalog, repo);

            Assert.ThrowsException<ArgumentNullException>(() => DependencyDagExtensions.DependentWalk(null, x => { }));
            Assert.ThrowsException<ArgumentNullException>(() => dag.DependentWalk(null));
        }

        [TestMethod]
        [UnitTest]
        public void DependentWalk_NodesAreNotVisitedUntilTheirDependentsHaveBeenVisited()
        {
            //
            //        r0      r1
            //       / \     / \
            //      n1  n2  /   n3
            //     /   / \ /
            //    n5  /   n4
            //     \ /
            //      n6
            //

            // 0: r0 r1
            // 1: n1 n2 n3
            // 2: n5 n4
            // 3: n6

            var r0 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("r0"), };
            var r1 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("r1"), };
            var n1 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n1"), };
            var n2 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n2"), };
            var n3 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n3"), };
            var n4 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n4"), };
            var n5 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n5"), };
            var n6 = new TestCompositeDataCookerReference { Path = DataCookerPath.ForComposite("n6"), };

            r0.requiredDataCookers.Add(n1.Path);
            r0.requiredDataCookers.Add(n2.Path);
            r1.requiredDataCookers.Add(n4.Path);
            r1.requiredDataCookers.Add(n3.Path);
            n1.requiredDataCookers.Add(n5.Path);
            n2.requiredDataCookers.Add(n6.Path);
            n2.requiredDataCookers.Add(n4.Path);

            var catalog = new TestProcessingSourceCatalog { IsLoaded = true, };

            var repo = new DataExtensionRepository();
            repo.TryAddReference(r0);
            repo.TryAddReference(r1);
            repo.TryAddReference(n1);
            repo.TryAddReference(n2);
            repo.TryAddReference(n3);
            repo.TryAddReference(n4);
            repo.TryAddReference(n5);
            repo.TryAddReference(n6);
            repo.FinalizeDataExtensions();

            var allReferences = new[]
            {
                DependencyDag.Reference.Create(r0),
                DependencyDag.Reference.Create(r1),
                DependencyDag.Reference.Create(n1),
                DependencyDag.Reference.Create(n2),
                DependencyDag.Reference.Create(n3),
                DependencyDag.Reference.Create(n4),
                DependencyDag.Reference.Create(n5),
                DependencyDag.Reference.Create(n6),
            };

            var visited = new HashSet<DependencyDag.Reference>();

            var dag = DependencyDag.Create(catalog, repo);

            dag.DependentWalk(
                n =>
                {
                    visited.Add(n.Target);
                    Assert.IsTrue(
                        n.Dependents.All(x => visited.Contains(x.Target)),
                        "Failed on node: " + n.Target.Match(x => x.Name, x => x.Name));
                });

            CollectionAssert.AreEquivalent(allReferences, visited.ToList());
        }
    }
}

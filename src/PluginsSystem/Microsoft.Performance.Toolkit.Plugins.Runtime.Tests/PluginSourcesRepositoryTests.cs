// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Specialized;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Toolkit.Plugins.Core.Discovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Tests
{
    [TestClass]
    public class PluginSourcesRepositoryTests
    {
        [TestMethod]
        [UnitTest]
        public void Add_Single_Success()
        {
            var repo = new PluginSourceRepository();
            var source = new PluginSource(FakeUris.Uri1);

            bool success = repo.Add(source);

            Assert.IsTrue(success);
            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreSame(source, repo.Items.Single());
        }

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_Success()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri3);

            IEnumerable<PluginSource> addedItems = repo.Add(new PluginSource[] { source1, source2, source3 });

            Assert.AreEqual(3, addedItems.Count());
            Assert.IsTrue(addedItems.Contains(source1));
            Assert.IsTrue(addedItems.Contains(source2));
            Assert.IsTrue(addedItems.Contains(source3));

            Assert.AreEqual(3, repo.Items.Count());
            Assert.IsTrue(repo.Items.Contains(source1));
            Assert.IsTrue(repo.Items.Contains(source2));
            Assert.IsTrue(repo.Items.Contains(source3));
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_WithDuplicates_DuplicatesNotAdded()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);

            Assert.IsTrue(repo.Add(source1));
            Assert.IsFalse(repo.Add(source1));
            Assert.IsFalse(repo.Add(new PluginSource(FakeUris.Uri1)));

            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreSame(source1, repo.Items.Single());
        }

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_WithDuplicates_DuplicatesNotAdded()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri1);

            IEnumerable<PluginSource> addedItems = repo.Add(new PluginSource[] { source1, source2, source3, source1 });

            Assert.AreEqual(2, addedItems.Count());
            Assert.IsTrue(addedItems.Contains(source1));
            Assert.IsTrue(addedItems.Contains(source2));

            Assert.AreEqual(2, repo.Items.Count());
            Assert.IsTrue(repo.Items.Contains(source1));
            Assert.IsTrue(repo.Items.Contains(source2));
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_Success()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            repo.Add(source1);

            bool success = repo.Remove(source1);

            Assert.IsTrue(success);
            Assert.AreEqual(0, repo.Items.Count());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_Success()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri3);
            repo.Add(new PluginSource[] { source1, source2, source3 });

            var removedItems = repo.Remove(new PluginSource[] { source1, source3 });

            Assert.AreEqual(2, removedItems.Count());
            Assert.IsTrue(removedItems.Contains(source1));
            Assert.IsTrue(removedItems.Contains(source3));

            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreSame(source2, repo.Items.Single());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_NotFound_Noop()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            repo.Add(source1);

            bool success = repo.Remove(source2);

            Assert.IsFalse(success);
            Assert.AreEqual(1, repo.Items.Count());
            Assert.AreSame(source1, repo.Items.Single());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_NotFound_Noop()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri3);
            var source4 = new PluginSource(FakeUris.Uri1);
            repo.Add(new PluginSource[] { source1, source2, source3 });

            IEnumerable<PluginSource> removedItems = repo.Remove(new PluginSource[] { source1, source4 });

            Assert.AreEqual(1, removedItems.Count());
            Assert.AreSame(source1, removedItems.Single());

            Assert.AreEqual(2, repo.Items.Count());
            Assert.IsTrue(repo.Items.Contains(source2));
            Assert.IsTrue(repo.Items.Contains(source3));
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_CollectionChangedRaised()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                Assert.AreEqual(1, e.NewItems?.Count);
                Assert.AreSame(source1, e.NewItems?[0]);
            };

            repo.Add(source1);

            Assert.IsTrue(raised);
        }


        [TestMethod]
        [UnitTest]
        public void Add_Multiple_CollectionChangedRaised()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri3);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                Assert.AreEqual(3, e.NewItems?.Count);
                Assert.IsTrue(e.NewItems?.Contains(source1));
                Assert.IsTrue(e.NewItems?.Contains(source2));
                Assert.IsTrue(e.NewItems?.Contains(source3));
            };

            repo.Add(new PluginSource[] { source1, source2, source3 });

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_CollectionChangedRaised()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            repo.Add(source1);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.AreEqual(1, e.OldItems?.Count);
                Assert.AreSame(source1, e.OldItems?[0]);
            };

            repo.Remove(source1);

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_CollectionChangedRaised()
        {
            var repo = new PluginSourceRepository();
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source3 = new PluginSource(FakeUris.Uri3);
            repo.Add(new PluginSource[] { source1, source2, source3 });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.AreEqual(2, e.OldItems?.Count);
                Assert.IsTrue(e.OldItems?.Contains(source1));
                Assert.IsTrue(e.OldItems?.Contains(source3));
            };

            repo.Remove(new PluginSource[] { source1, source3 });

            Assert.IsTrue(raised);
        }
    }
}

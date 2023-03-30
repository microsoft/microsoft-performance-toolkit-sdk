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
        #region Add Single Plugin Source

        [TestMethod]
        [UnitTest]
        public void Add_Single_New_Added()
        {
            var repo = new PluginSourceRepository();
            var source = new PluginSource(FakeUris.Uri1);

            bool success = repo.Add(source);

            Assert.IsTrue(success);
            Assert.IsTrue(repo.Items.Contains(source));
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_Exisiting_NotAdded()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });

            bool success = repo.Add(source);

            Assert.IsFalse(success);
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_Duplicate_NotAdded()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });

            bool success = repo.Add(new PluginSource(FakeUris.Uri1));

            Assert.IsFalse(success);
        }


        [TestMethod]
        [UnitTest]
        public void Add_Single_CollectionChangedInvoked()
        {
            var repo = new PluginSourceRepository();
            var source = new PluginSource(FakeUris.Uri1);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Add(source);

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_Fails_CollectionChangedNotInvoked()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Add(source);

            Assert.IsFalse(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Add_Single_NullItem_ThrowsArgumentNullException()
        {
            var repo = new PluginSourceRepository();

            PluginSource? pluginSource = null;
            Assert.ThrowsException<ArgumentNullException>(() => repo.Add(pluginSource));
        }
        #endregion

        #region Add Multiple Plugin Sources

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_New_Added()
        {
            var repo = new PluginSourceRepository();
            var sources = new PluginSource[]
            {
                new PluginSource(FakeUris.Uri1),
                new PluginSource(FakeUris.Uri2)
            };

            IEnumerable<PluginSource> addedItems = repo.Add(sources);

            CollectionAssert.AreEquivalent(sources, addedItems.ToArray());
            CollectionAssert.AreEquivalent(sources, repo.Items.ToArray());
        }

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_Existing_NewItemsAdded()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var source1_duplicate = new PluginSource(FakeUris.Uri1);

            var repo = CreateSut(new PluginSource[] { source1 });

            IEnumerable<PluginSource> addedItems = repo.Add(new PluginSource[] { source1, source2, source1, source1_duplicate });

            CollectionAssert.AreEquivalent(new PluginSource[] { source2 }, addedItems.ToArray());
            CollectionAssert.AreEquivalent(new PluginSource[] { source1, source2 }, repo.Items.ToArray());
        }

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_CollectionChangedInvoked()
        {
            var repo = new PluginSourceRepository();
            var sources = new PluginSource[]
            {
                new PluginSource(FakeUris.Uri1),
                new PluginSource(FakeUris.Uri2)
            };
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Add(sources);

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Add_Multiple_NoneAdded_CollectionChangedNotInvoked()
        {
            var repo = CreateSut(Array.Empty<PluginSource>());
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Add(Array.Empty<PluginSource>());

            Assert.IsFalse(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Add_Mutiple_ThrowsArgumentNullException()
        {
            var repo = new PluginSourceRepository();

            PluginSource[]? pluginSources = null;
            Assert.ThrowsException<ArgumentNullException>(() => repo.Add(pluginSources));
        }

        #endregion

        #region Remove Single Plugin Source

        [TestMethod]
        [UnitTest]
        public void Remove_Single_Removed()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });

            bool success = repo.Remove(source);

            Assert.IsTrue(success);
            Assert.IsFalse(repo.Items.Contains(source));
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_Duplicate_Removed()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var source_duplicate = new PluginSource(FakeUris.Uri1);

            var repo = CreateSut(new PluginSource[] { source });

            bool success = repo.Remove(source_duplicate);

            Assert.IsTrue(success);
            Assert.IsFalse(repo.Items.Contains(source));
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_NoneExisting_NotRemoved()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });

            bool success = repo.Remove(new PluginSource(FakeUris.Uri2));

            Assert.IsFalse(success);
            Assert.IsTrue(repo.Items.Contains(source));
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_CollectionChangedInvoked()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Remove(source);

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_NoneRemoved_CollectionChangedNotInvoked()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new PluginSource[] { source });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Remove(new PluginSource(FakeUris.Uri2));

            Assert.IsFalse(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Single_ThrowsArgumentNullException()
        {
            var repo = new PluginSourceRepository();

            PluginSource? pluginSource = null;
            Assert.ThrowsException<ArgumentNullException>(() => repo.Remove(pluginSource));
        }

        #endregion

        #region Remove Multiple Plugin Sources

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_Removed()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var repo = CreateSut(new PluginSource[] { source1, source2 });

            IEnumerable<PluginSource> removedItems = repo.Remove(new PluginSource[] { source1, source2 });

            CollectionAssert.AreEquivalent(new PluginSource[] { source1, source2 }, removedItems.ToArray());
            Assert.IsFalse(repo.Items.Any());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_Duplicate_Removed()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source1_duplicate = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var repo = CreateSut(new PluginSource[] { source1, source2 });

            IEnumerable<PluginSource> removedItems = repo.Remove(new PluginSource[] { source1_duplicate, source2 });

            CollectionAssert.AreEquivalent(new PluginSource[] { source1_duplicate, source2 }, removedItems.ToArray());
            Assert.IsFalse(repo.Items.Any());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_NoneExisting_NotRemoved()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var repo = CreateSut(new PluginSource[] { source1 });

            IEnumerable<PluginSource> removedItems = repo.Remove(new PluginSource[] { source2 });

            Assert.IsFalse(removedItems.Any());
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_CollectionChangedInvoked()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var repo = CreateSut(new PluginSource[] { source1, source2 });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Remove(new PluginSource[] { source1, source2 });

            Assert.IsTrue(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_NoneRemoved_CollectionChangedNotInvoked()
        {
            var source1 = new PluginSource(FakeUris.Uri1);
            var source2 = new PluginSource(FakeUris.Uri2);
            var repo = CreateSut(new PluginSource[] { source1 });
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
            };

            repo.Remove(new PluginSource[] { source2 });

            Assert.IsFalse(raised);
        }

        [TestMethod]
        [UnitTest]
        public void Remove_Multiple_ThrowsArgumentNullException()
        {
            var repo = new PluginSourceRepository();

            PluginSource[]? pluginSources = null;
            Assert.ThrowsException<ArgumentNullException>(() => repo.Remove(pluginSources));
        }

        #endregion


        #region Collection Changed

        [TestMethod]
        [UnitTest]
        public void CollectionChanged_NewItem_Correct()
        {
            var repo = new PluginSourceRepository();
            var source = new PluginSource(FakeUris.Uri1);

            repo.CollectionChanged += (s, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                Assert.AreEqual(source, e.NewItems?.Cast<PluginSource>().Single());
            };

            repo.Add(source);
        }

        [TestMethod]
        [UnitTest]
        public void CollectionChanged_NewItems_Correct()
        {
            var repo = new PluginSourceRepository();
            var sources = new[]
            {
                new PluginSource(FakeUris.Uri1),
                new PluginSource(FakeUris.Uri2),
                new PluginSource(FakeUris.Uri3),
            };

            repo.CollectionChanged += (s, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                CollectionAssert.AreEquivalent(sources, e.NewItems?.Cast<PluginSource>().ToArray());
            };

            repo.Add(sources);
        }

        [TestMethod]
        [UnitTest]
        public void CollectionChanged_OldItem_Correct()
        {
            var source = new PluginSource(FakeUris.Uri1);
            var repo = CreateSut(new[] { source });

            repo.CollectionChanged += (s, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.AreEqual(source, e.OldItems?.Cast<PluginSource>().Single());
            };

            repo.Remove(source);
        }

        [TestMethod]
        [UnitTest]
        public void CollectionChanged_OldItems_Correct()
        {
            var sources = new[]
            {
                new PluginSource(FakeUris.Uri1),
                new PluginSource(FakeUris.Uri2),
                new PluginSource(FakeUris.Uri3),
            };
            var repo = CreateSut(sources);

            repo.CollectionChanged += (s, e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                CollectionAssert.AreEquivalent(sources, e.OldItems?.Cast<PluginSource>().ToArray());
            };

            repo.Remove(sources);
        }

        #endregion

        private PluginSourceRepository CreateSut(IEnumerable<PluginSource>? existingPluginSources)
        {
            var repo = new PluginSourceRepository();
            if (existingPluginSources != null && existingPluginSources.Any())
            {
                repo.Add(existingPluginSources);
            }

            return repo;
        }
    }
}

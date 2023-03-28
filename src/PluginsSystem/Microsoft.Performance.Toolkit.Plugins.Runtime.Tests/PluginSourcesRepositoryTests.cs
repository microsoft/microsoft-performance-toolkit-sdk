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
        [DynamicData(nameof(GetAddSingleTestCases), DynamicDataSourceType.Method)]
        public void Add_Single_Returns(PluginSourceRepoTestCase repoTestCase)
        {
            var sourceToAdd = repoTestCase?.PluginSourcesToChange?.Single();
            var repo = CreateSut(repoTestCase?.ExistingRepoSources);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                CollectionAssert.AreEquivalent(repoTestCase?.PluginSourcesToChange, e.NewItems?.Cast<PluginSource>().ToArray());
            };

            bool success = repo.Add(sourceToAdd);

            Assert.AreEqual(success, repoTestCase?.ExpectedResult);
            if (repoTestCase?.ExpectedResult == true)
            {
                Assert.IsTrue(repo.Items.Contains(sourceToAdd));
                Assert.IsTrue(raised);
            }
        }

        [TestMethod]
        [UnitTest]
        public void Add_SingleNullItem_ThrowsArgumentNullException()
        {
            var repo = new PluginSourceRepository();

            PluginSource? pluginSource = null;
            Assert.ThrowsException<ArgumentNullException>(() => repo.Add(pluginSource));
        }
        #endregion

        #region Add Multiple Plugin Sources

        [TestMethod]
        [UnitTest]
        [DynamicData(nameof(GetAddMultipleTestCases), DynamicDataSourceType.Method)]
        public void Add_Multiple_Returns(PluginSourceRepoTestCase repoTestCase)
        {
            var repo = CreateSut(repoTestCase.ExistingRepoSources);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                CollectionAssert.AreEquivalent(repoTestCase.ExpectedIChangedtems, e.NewItems?.Cast<PluginSource>().ToArray());
            };

            IEnumerable<PluginSource> addedItems = repo.Add(repoTestCase.PluginSourcesToChange);

            CollectionAssert.AreEquivalent(repoTestCase.ExpectedIChangedtems, addedItems.ToArray());
            CollectionAssert.IsSubsetOf(repoTestCase.ExpectedIChangedtems, repo.Items.ToArray());
            if (repoTestCase?.ExpectedIChangedtems?.Any() ?? false)
            {
                Assert.IsTrue(raised);
            }
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
        [DynamicData(nameof(GetRemoveSingleTestCases), DynamicDataSourceType.Method)]
        public void Remove_Single_Returns(PluginSourceRepoTestCase repoTestCase)
        {
            var sourceToRemove = repoTestCase?.PluginSourcesToChange?.Single();
            var repo = CreateSut(repoTestCase?.ExistingRepoSources);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                CollectionAssert.AreEquivalent(repoTestCase?.PluginSourcesToChange, e.OldItems?.Cast<PluginSource>().ToArray());
            };

            bool success = repo.Remove(sourceToRemove);

            Assert.AreEqual(success, repoTestCase?.ExpectedResult);
            if (repoTestCase?.ExpectedResult == true)
            {
                Assert.IsFalse(repo.Items.Contains(sourceToRemove));
                Assert.IsTrue(raised);
            }
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
        [DynamicData(nameof(GetRemoveMultipleTestCases), DynamicDataSourceType.Method)]
        public void Remove_Multiple_Returns(PluginSourceRepoTestCase repoTestCase)
        {
            var repo = CreateSut(repoTestCase.ExistingRepoSources);
            bool raised = false;

            repo.CollectionChanged += (s, e) =>
            {
                raised = true;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                CollectionAssert.AreEquivalent(repoTestCase.ExpectedIChangedtems, e.OldItems?.Cast<PluginSource>().ToArray());
            };

            IEnumerable<PluginSource> removedItems = repo.Remove(repoTestCase.PluginSourcesToChange);

            CollectionAssert.AreEquivalent(repoTestCase.ExpectedIChangedtems, removedItems.ToArray());
            CollectionAssert.DoesNotContain(repo.Items.ToArray(), repoTestCase.ExpectedIChangedtems);
            if (repoTestCase?.ExpectedIChangedtems?.Any() ?? false)
            {
                Assert.IsTrue(raised);
            }
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

        #region Test Data And Helper Methods

        public sealed class PluginSourceRepoTestCase
        {
            public PluginSource[]? ExistingRepoSources { get; set; }

            public PluginSource[]? PluginSourcesToChange { get; set; }

            public bool ExpectedResult { get; set; }

            public PluginSource[]? ExpectedIChangedtems { get; set; }
        }

        private static IEnumerable<object[]> GetAddSingleTestCases()
        {
            // Add new
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = Array.Empty<PluginSource>(),
                    PluginSourcesToChange = new PluginSource[] { new PluginSource(FakeUris.Uri1) },
                    ExpectedResult = true,
                },
            };

            // Duplicate
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        new PluginSource(FakeUris.Uri1),
                    },
                    PluginSourcesToChange = new PluginSource[] { new PluginSource(FakeUris.Uri1) },
                    ExpectedResult = false,
                },
            };
        }

        private static IEnumerable<object[]> GetAddMultipleTestCases()
        {
            var pluginSource1 = new PluginSource(FakeUris.Uri1);
            var pluginSource2 = new PluginSource(FakeUris.Uri2);
            var pluginSource1_duplicate = new PluginSource(FakeUris.Uri1);

            // Add new
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = Array.Empty<PluginSource>(),
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    ExpectedIChangedtems = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                },
            };

            // Duplicate
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource1_duplicate,
                        pluginSource2,
                    },
                    ExpectedIChangedtems = new PluginSource[]
                    {
                        pluginSource2,
                    },
                },
            };

            // Duplicate and new
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource2
                    },
                    ExpectedIChangedtems = Array.Empty<PluginSource>(),
                },
            };

            // Empty
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = Array.Empty<PluginSource>(),
                    PluginSourcesToChange =  Array.Empty<PluginSource>(),
                    ExpectedIChangedtems = Array.Empty<PluginSource>(),
                },
            };
        }

        private static IEnumerable<object[]> GetRemoveSingleTestCases()
        {
            var pluginSource1 = new PluginSource(FakeUris.Uri1);
            var pluginSource1_duplicate = new PluginSource(FakeUris.Uri1); // same as pluginSource1

            // Remove existing
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    ExpectedResult = true,
                },
            };

            // Remove existing duplicate
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1_duplicate,
                    },
                    ExpectedResult = true,
                },
            };

            // Remove non-existing
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = Array.Empty<PluginSource>(),
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    ExpectedResult = false,
                },
            };
        }

        private static IEnumerable<object[]> GetRemoveMultipleTestCases()
        {
            var pluginSource1 = new PluginSource(FakeUris.Uri1);
            var pluginSource2 = new PluginSource(FakeUris.Uri2);
            var pluginSource1_duplicate = new PluginSource(FakeUris.Uri1); // same as pluginSource1

            // Remove existing
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    ExpectedIChangedtems = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                },
            };

            // Remove existing duplicate
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1_duplicate,
                    },
                    ExpectedIChangedtems = new PluginSource[]
                    {
                        pluginSource1_duplicate,
                    },
                },
            };

            // Remove existing and  non-existing
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                        pluginSource2,
                    },
                    ExpectedIChangedtems = new PluginSource[]
                    {
                        pluginSource1,
                    },
                },
            };

            // Remove non-existing
            yield return new object[]
            {
                new PluginSourceRepoTestCase
                {
                    ExistingRepoSources = Array.Empty<PluginSource>(),
                    PluginSourcesToChange = new PluginSource[]
                    {
                        pluginSource1,
                    },
                    ExpectedIChangedtems = Array.Empty<PluginSource>(),
                },
            };
        }

        private PluginSourceRepository CreateSut(IEnumerable<PluginSource>? existingPluginSources)
        {
            var repo = new PluginSourceRepository();
            if (existingPluginSources != null && existingPluginSources.Any())
            {
                repo.Add(existingPluginSources);
            }

            return repo;
        }

        #endregion
    }
}

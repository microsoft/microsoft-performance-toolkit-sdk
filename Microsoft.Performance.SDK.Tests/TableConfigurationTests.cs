// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class TableConfigurationTests
    {
        private TableConfiguration Sut { get; set; }

        [TestInitialize]
        public void Setup()
        {
            this.Sut = new TableConfiguration();
        }

        [TestMethod]
        [UnitTest]
        public void AddingEmptyColumnSetIsOkay()
        {
            this.Sut.Columns = Array.Empty<ColumnConfiguration>();
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionMissingPivotIsOkay()
        {
            this.Sut.Columns = new[]
            {
                TableConfiguration.LeftFreezeColumn,
                TableConfiguration.RightFreezeColumn,
                TableConfiguration.GraphColumn,
            };
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionMissingLeftFreezeIsOkay()
        {
            this.Sut.Columns = new[]
            {
                TableConfiguration.PivotColumn,
                TableConfiguration.RightFreezeColumn,
                TableConfiguration.GraphColumn,
            };
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionMissingRightFreezeIsOkay()
        {
            this.Sut.Columns = new[]
            {
                TableConfiguration.PivotColumn,
                TableConfiguration.LeftFreezeColumn,
                TableConfiguration.GraphColumn,
            };
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionMissingGraphColumnIsOkay()
        {
            this.Sut.Columns = new[]
            {
                TableConfiguration.PivotColumn,
                TableConfiguration.LeftFreezeColumn,
                TableConfiguration.RightFreezeColumn,
            };
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionRightLeftOfLeftThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => this.Sut.Columns = new[]
                {
                    TableConfiguration.RightFreezeColumn,
                    TableConfiguration.LeftFreezeColumn,
                });
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionGraphLeftOfPivotThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(
                () => this.Sut.Columns = new[]
                {
                    TableConfiguration.GraphColumn,
                    TableConfiguration.PivotColumn,
                });
        }

        [TestMethod]
        [UnitTest]
        public void AddingColumnCollectionMetadataColumnSpecifiedMultipleTimesThrows()
        {
            foreach (var c in TableConfiguration.AllMetadataColumns)
            {
                var testCase = new[]
                {
                    c,
                    c,
                };

                Assert.ThrowsException<InvalidOperationException>(
                   () => this.Sut.Columns = testCase);
            }
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnRole_MetadataColumnThrows()
        {
            foreach (var c in TableConfiguration.AllMetadataColumns)
            {
                var r = ColumnRole.Duration;
                Assert.ThrowsException<InvalidOperationException>(() => this.Sut.AddColumnRole(r, c.Metadata.Guid));
            }
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnRole_InvalidRoleThrows()
        {
            var c = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c, };

            Assert.ThrowsException<InvalidEnumArgumentException>(
                () => this.Sut.AddColumnRole(ColumnRole.CountColumnMetadata, c.Metadata.Guid));
            Assert.ThrowsException<InvalidEnumArgumentException>(
                () => this.Sut.AddColumnRole(
                    (ColumnRole)((int)ColumnRole.CountColumnMetadata + 1), c.Metadata.Guid));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnRole_Adds()
        {
            var r = ColumnRole.Duration;
            var c = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c, };

            this.Sut.AddColumnRole(r, c.Metadata.Guid);

            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c.Metadata.Guid, this.Sut.ColumnRoles[r]);
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnRole_SameRoleOverwrites()
        {
            var r = ColumnRole.Duration;
            var c1 = Any.ColumnConfiguration();
            var c2 = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c1, c2, };

            this.Sut.AddColumnRole(r, c1.Metadata.Guid);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r]);

            this.Sut.AddColumnRole(r, c2.Metadata.Guid);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c2.Metadata.Guid, this.Sut.ColumnRoles[r]);
        }

        [TestMethod]
        [UnitTest]
        [Ignore("Disabled while we determine how to handle time columns, for example.")]
        public void AddColumnRole_DifferentRoleSameColumnThrows()
        {
            var r1 = ColumnRole.Duration;
            var r2 = ColumnRole.EndThreadId;
            var c = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c, };

            this.Sut.AddColumnRole(r1, c.Metadata.Guid);

            Assert.ThrowsException<InvalidOperationException>(() => this.Sut.AddColumnRole(r2, c.Metadata.Guid));
        }

        [TestMethod]
        [UnitTest]
        public void AddColumnRole_SameRoleSameColumnDoesNothing()
        {
            var r = ColumnRole.Duration;
            var c = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c, };

            this.Sut.AddColumnRole(r, c.Metadata.Guid);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c.Metadata.Guid, this.Sut.ColumnRoles[r]);

            this.Sut.AddColumnRole(r, c.Metadata.Guid);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c.Metadata.Guid, this.Sut.ColumnRoles[r]);
        }

        [TestMethod]
        [UnitTest]
        public void RemoveColumnRole_MissingDoesNothing()
        {
            var c1 = Any.ColumnConfiguration();
            var c2 = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c1, c2, };

            var r1 = ColumnRole.Duration;
            var r2 = ColumnRole.EndThreadId;
            this.Sut.AddColumnRole(r1, c1.Metadata.Guid);

            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r1]);
            Assert.IsFalse(this.Sut.ColumnRoles.ContainsKey(r2));

            this.Sut.RemoveColumnRole(r2);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r1]);
        }

        [TestMethod]
        [UnitTest]
        public void RemoveColumnRole_InvalidDoesNothing()
        {
            var c1 = Any.ColumnConfiguration();
            var c2 = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c1, c2, };

            var r1 = ColumnRole.Duration;
            var r2 = ColumnRole.CountColumnMetadata;
            this.Sut.AddColumnRole(r1, c1.Metadata.Guid);

            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r1]);
            Assert.IsFalse(this.Sut.ColumnRoles.ContainsKey(r2));

            this.Sut.RemoveColumnRole(r2);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r1]);
        }

        [TestMethod]
        [UnitTest]
        public void RemoveColumnRole_PresentIsRemoved()
        {
            var c = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c, };

            var r = ColumnRole.Duration;
            this.Sut.AddColumnRole(r, c.Metadata.Guid);

            Assert.AreEqual(c.Metadata.Guid, this.Sut.ColumnRoles[r]);

            this.Sut.RemoveColumnRole(r);
            Assert.AreEqual(0, this.Sut.ColumnRoles.Count);
        }

        [TestMethod]
        [UnitTest]
        public void RemoveColumnRole_PresentIsRemovedOthersAreLeft()
        {
            var c1 = Any.ColumnConfiguration();
            var c2 = Any.ColumnConfiguration();
            this.Sut.Columns = new[] { c1, c2, };

            var r1 = ColumnRole.Duration;
            var r2 = ColumnRole.EndThreadId;
            this.Sut.AddColumnRole(r1, c1.Metadata.Guid);
            this.Sut.AddColumnRole(r2, c2.Metadata.Guid);

            Assert.AreEqual(c1.Metadata.Guid, this.Sut.ColumnRoles[r1]);
            Assert.AreEqual(c2.Metadata.Guid, this.Sut.ColumnRoles[r2]);

            this.Sut.RemoveColumnRole(r1);
            Assert.AreEqual(1, this.Sut.ColumnRoles.Count);
            Assert.AreEqual(c2.Metadata.Guid, this.Sut.ColumnRoles[r2]);
        }
    }
}

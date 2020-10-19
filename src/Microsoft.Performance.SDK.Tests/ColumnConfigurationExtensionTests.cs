// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ColumnConfigurationExtensionTests
    {
        [TestMethod]
        [UnitTest]
        public void IsMetadataColumnReturnsFalseForNull()
        {
            Assert.IsFalse(ColumnConfigurationExtensions.IsMetadataColumn(null));
        }

        [TestMethod]
        [UnitTest]
        public void IsMetadataColumnReturnsTrueForMetadata()
        {
            var tests = new[]
            {
                TableConfiguration.GraphColumn,
                TableConfiguration.LeftFreezeColumn,
                TableConfiguration.PivotColumn,
                TableConfiguration.RightFreezeColumn
            };

            foreach (var test in tests)
            {
                Assert.IsTrue(test.IsMetadataColumn());
            }
        }

        [TestMethod]
        [UnitTest]
        public void IsMetadataColumnReturnsFalseForNonMetadataColumns()
        {
            var c = new ColumnConfiguration(new ColumnMetadata(Guid.NewGuid(), "Test"));

            Assert.IsFalse(c.IsMetadataColumn());
        }

        [TestMethod]
        [UnitTest]
        public void IsMetadataColumnReturnsTrueForEvenSameGuids()
        {
            var tests = TableConfiguration.AllMetadataColumns
                .Select(x => new ColumnConfiguration(x.Metadata));

            foreach (var test in tests)
            {
                Assert.IsTrue(test.IsMetadataColumn());
            }
        }
    }
}

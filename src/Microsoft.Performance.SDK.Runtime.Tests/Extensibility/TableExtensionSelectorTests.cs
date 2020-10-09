// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Tables;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class TableExtensionSelectorTests
    {
        // Source Cookers
        private static TestSourceDataCookerReference[] SourceCookers;

        private static readonly IDictionary<DataCookerPath, TestSourceDataCookerReference> SourceCookersByPath 
            = new Dictionary<DataCookerPath, TestSourceDataCookerReference>();

        // Composite cookers

        private static TestCompositeDataCookerReference[] CompositeCookers;

        private static readonly IDictionary<DataCookerPath, TestCompositeDataCookerReference> CompositeCookersByPath
            = new Dictionary<DataCookerPath, TestCompositeDataCookerReference>();

        private static readonly DataCookerPath Source1Cooker1Path = new DataCookerPath("Source1", "SourceCooker1");

        static TableExtensionSelectorTests()
        {
            SetupSourceCookers();
            SetupCompositeCookers();
        }

        private static void SetupSourceCookers()
        {
            SourceCookers = new[]
            {
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source1", "SourceCooker1"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source2", "SourceCooker0"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source2", "SourceCooker10"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source2", "SourceCooker13"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source1", "SourceCooker41"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source1", "SourceCooker356"),
                },
                new TestSourceDataCookerReference
                {
                    Path = new DataCookerPath("Source4", "SourceCooker_Woah!"),
                },
            };

            foreach (var sourceCooker in SourceCookers)
            {
                SourceCookersByPath.Add(sourceCooker.Path, sourceCooker);
            }
        }

        private static void SetupCompositeCookers()
        {
            CompositeCookers = new[]
            {
                new TestCompositeDataCookerReference
                {
                    Path = new DataCookerPath(string.Empty, "CompositeCooker1"),
                },
                new TestCompositeDataCookerReference
                {
                    Path = new DataCookerPath(string.Empty, "CompositeCooker2"),
                },
                new TestCompositeDataCookerReference
                {
                    Path = new DataCookerPath(string.Empty, "CompositeCooker3"),
                },
                new TestCompositeDataCookerReference
                {
                    Path = new DataCookerPath(string.Empty, "CompositeCooker33"),
                },
            };

            foreach (var cooker in CompositeCookers)
            {
                CompositeCookersByPath.Add(cooker.Path, cooker);
            }
        }

        private static void AddRequiredDataExtensions(
            TestTableExtensionReference table,
            TestDataExtensionRepository testRepo)
        {
            foreach (var cookerPath in table.RequiredDataCookers)
            {
                if (SourceCookersByPath.TryGetValue(cookerPath, out var sourceCookerReference))
                {
                    testRepo.sourceCookersByPath.Add(cookerPath, sourceCookerReference);
                }
                else if (CompositeCookersByPath.TryGetValue(cookerPath, out var compositeCookerReference))
                {
                    testRepo.compositeCookersByPath.Add(cookerPath, compositeCookerReference);
                }
            }

            if (table.DependencyState != null)
            {
                testRepo.FinalizeDataExtensions();
            }
        }

        /// <summary>
        /// Add a single table, make sure it and its category are added to the table selector
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void ConstructorTest1()
        {
            var table1 = new TestTableExtensionReference(false)
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{ADE569AF-4460-4DCC-A7C8-9748737AE592}"),
                    "Table1",
                    "Table1 Description",
                    "General",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    new[] { Source1Cooker1Path }),
                BuildTableAction = (tableBuilder, dataRetrieval) => {},
                availability = DataExtensionAvailability.Available,
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.tablesById.Add(table1.TableDescriptor.Guid, table1);

            var tableSelector = new TableExtensionSelector(testRepo);

            Assert.AreEqual(tableSelector.Tables.Count, 1);
            Assert.AreEqual(tableSelector.TableCategories.Count, 1);
            Assert.IsTrue(tableSelector.TableCategories.Contains(table1.TableDescriptor.Category));
        }

        /// <summary>
        /// Add a single, unavailable table.
        /// Make sure neither it nor its category are  added to the table selector
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void ConstructorTest2()
        {
            var table1 = new TestTableExtensionReference(false)
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{ADE569AF-4460-4DCC-A7C8-9748737AE592}"),
                    "Table1",
                    "Table1 Description",
                    "General",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    new[] { Source1Cooker1Path }),
                BuildTableAction = (tableBuilder, dataRetrieval) => {},
                availability = DataExtensionAvailability.MissingRequirement,
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.tablesById.Add(table1.TableDescriptor.Guid, table1);

            var tableSelector = new TableExtensionSelector(testRepo);

            Assert.AreEqual(tableSelector.Tables.Count, 0);
            Assert.AreEqual(tableSelector.TableCategories.Count, 0);
        }

        /// <summary>
        /// With multiple tables, make sure the categories show up correctly.
        /// All categories should be available.
        /// There should be no duplicate category names.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void ConstructorTest3()
        {
            var table1 = new TestTableExtensionReference(false)
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{ADE569AF-4460-4DCC-A7C8-9748737AE592}"),
                    "Table1",
                    "Table1 Description",
                    "General",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    new[] { Source1Cooker1Path }),
                BuildTableAction = (tableBuilder, dataRetrieval) => { },
                availability = DataExtensionAvailability.Available,
            };

            var table2 = new TestTableExtensionReference(false)
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{42F33CCB-D1D4-48CD-BAF0-853A508C638D}"),
                    "Table2",
                    "Table1 Description",
                    "Specific",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    new[] { Source1Cooker1Path }),
                BuildTableAction = (tableBuilder, dataRetrieval) => { },
                availability = DataExtensionAvailability.Available,
            };

            var table3 = new TestTableExtensionReference(false)
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{3A613426-6550-49AA-89F0-C6BBB6039EB4}"),
                    "Table3",
                    "Table1 Description",
                    "General",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    new[] { Source1Cooker1Path }),
                BuildTableAction = (tableBuilder, dataRetrieval) => { },
                availability = DataExtensionAvailability.Available,
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.tablesById.Add(table1.TableDescriptor.Guid, table1);
            testRepo.tablesById.Add(table2.TableDescriptor.Guid, table2);
            testRepo.tablesById.Add(table3.TableDescriptor.Guid, table3);

            var tableSelector = new TableExtensionSelector(testRepo);

            Assert.AreEqual(tableSelector.Tables.Count, 3);
            Assert.AreEqual(tableSelector.TableCategories.Count, 2);
            Assert.IsTrue(tableSelector.TableCategories.Contains(table2.TableDescriptor.Category));
            Assert.IsTrue(tableSelector.TableCategories.Contains(table1.TableDescriptor.Category));
        }

        /// <summary>
        /// Add a single table, with a single source cooker.
        /// Make sure the single source cooker is returned.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void GetSourceDataCookersForTablesTest1()
        {
            var requiredDataCookers = new List<DataCookerPath>
            {
                Source1Cooker1Path,
            };

            var table1 = new TestTableExtensionReference
            {
                TableDescriptor = new TableDescriptor(
                    Guid.Parse("{ADE569AF-4460-4DCC-A7C8-9748737AE592}"),
                    "Table1",
                    "Table1 Description",
                    "General",
                    false,
                    TableLayoutStyle.GraphAndTable,
                    requiredDataCookers),
                BuildTableAction = (tableBuilder, dataRetrieval) => { },
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.tablesById.Add(table1.TableDescriptor.Guid, table1);

            AddRequiredDataExtensions(table1, testRepo);

            var tableSelector = new TableExtensionSelector(testRepo);

            Assert.AreEqual(tableSelector.Tables.Count, 1);

            var requiredSourceDataCookers = tableSelector.GetSourceDataCookersForTables(new [] {table1.TableDescriptor.Guid });

            Assert.AreEqual(requiredSourceDataCookers.Count, 1);

            var sourceId = requiredDataCookers[0].SourceParserId;
            Assert.IsTrue(requiredSourceDataCookers.ContainsKey(sourceId));
            Assert.AreEqual(requiredSourceDataCookers[sourceId].Count(), 1);
        }
    }
}

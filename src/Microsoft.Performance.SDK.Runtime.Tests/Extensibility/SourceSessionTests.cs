// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class SourceSessionTests
    {
        /// <summary>
        ///     Ensure progress is reported incrementally
        /// </summary>
        private void CheckProgressReports(TestProgress progress, int numRecords)
        {
            int numPasses = (progress.ReportedValues.Count) / numRecords;
            int numIter = numRecords * numPasses;

            int pass = 0;
            int record = 0;
            for (int recordIdx = 0; recordIdx < progress.ReportedValues.Count - 1; recordIdx++)
            {
                int expectedVal = 100 * (recordIdx + 1) / numIter;
                int actualVal = progress.ReportedValues[recordIdx];
                Assert.AreEqual(expectedVal, actualVal);

                // iterate record and pass
                record++;
                if (record == numRecords)
                {
                    record = 0;
                    pass++;
                }
            }
        }

        /// <summary>
        ///     Create and register cooker with session.
        /// </summary>
        /// <returns>cooker</returns>
        private TestSourceDataCooker CreateCooker(
            SourceProcessingSession<TestRecord, TestParserContext, int> session,
            string id,
            string name,
            ReadOnlyHashSet<int> keys)
        {
            TestSourceDataCooker cooker = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(id, name),
                DataKeys = keys,
            };
            session.RegisterSourceDataCooker(cooker);
            return cooker;
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullSourceParser_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new SourceProcessingSession<TestRecord, TestParserContext, int>(null));
        }

        [TestMethod]
        [UnitTest]
        public void Constructor_NullSourceParserId_Throws()
        {
            var sourceParser = new TestSourceParser();

            Assert.ThrowsException<ArgumentNullException>(
                () => new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser));
        }

        [TestMethod]
        [UnitTest]
        public void Constructor()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            Assert.AreEqual(sourceSession.SourceParser, sourceParser);
        }

        [TestMethod]
        [UnitTest]
        public void RegisterSourceDataCooker_NullCooker_Throws()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            Assert.ThrowsException<ArgumentNullException>(
                () => sourceSession.RegisterSourceDataCooker(null));
        }

        [TestMethod]
        [UnitTest]
        public void RegisterSourceDataCooker()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var sourceDataCooker = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker")
            };

            sourceSession.RegisterSourceDataCooker(sourceDataCooker);

            Assert.AreEqual(sourceSession.RegisteredSourceDataCookers.Count, 1);

            var returnedSourceDataCooker = sourceSession.GetSourceDataCooker(sourceDataCooker.Path);

            Assert.AreEqual(sourceDataCooker, returnedSourceDataCooker);
        }

        [TestMethod]
        [UnitTest]
        public void ProcessSession()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var dataCookerContext = new TestSourceDataCookerContext();

            var sourceDataCooker1 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker2"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 113, 145, 167, 1000 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker2);

            var sourceDataCooker3 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker3"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 200, 250, 286, 1000 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker3);

            var sourceDataCooker4 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker4"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 145, 316, 315, 301 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker4);

            // Setup some dependencies so that not all cookers are in the same pass/block
            // Note that changing the required cookers after registering with the source
            // session should be fine, as the cookers aren't scheduled until ProcessSource
            // is called on the source session.
            //
            {
                // sourceDataCooker4: Pass=1, Block=0
                sourceDataCooker4.RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[] { sourceDataCooker3.Path });

                // sourceDataCooker2: Pass=1, Block=1
                sourceDataCooker2.RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new []
                    {
                        sourceDataCooker4.Path,
                        sourceDataCooker1.Path
                    });
                sourceDataCooker2.DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        {sourceDataCooker4.Path, DataCookerDependencyType.AsConsumed }
                    });
            }

            // data cooker order should be: { cooker1, cooker3 }, { cooker4, cooker2 }
            // cooker1 and cooker3 could be in any order in pass 0.
            // but cooker4 and cooker2 are ordered as such in pass 1.
            //

            // setup data for the TestParser to "parse"
            var testRecords = new List<TestRecord>
            {
                // delivered to cooker4 first and then cooker2 in Pass1
                new TestRecord {Key = 145, Value = "145"},

                // shouldn't be delivered to any cooker
                new TestRecord {Key = 500, Value = "500"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 250, Value = "250:1"},

                // delivered to cooker1 in Pass0
                new TestRecord {Key = 67, Value = "67"},

                // delivered to cooker3 in Pass0 and cooker2 in Pass1
                new TestRecord {Key = 1000, Value = "1000"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 250, Value = "250:2"},

                // delivered to cooker4 in Pass1
                new TestRecord {Key = 301, Value = "301"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 286, Value = "286"},

                // delivered to cooker4 in Pass1
                new TestRecord {Key = 315, Value = "315"},
            };

            sourceParser.TestRecords = testRecords;

            Assert.AreEqual(sourceSession.RegisteredSourceDataCookers.Count, 4);

            TestProgress progress = new TestProgress();
            sourceSession.ProcessSource(new NullLogger(), progress, CancellationToken.None);

            // Make sure each cooker received a BeginDataCooking call
            Assert.AreEqual(sourceDataCooker1.BeginDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker2.BeginDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker3.BeginDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker4.BeginDataCookingCallCount, 1);

            // Make sure each cooker received a EndDataCooking call
            Assert.AreEqual(sourceDataCooker1.EndDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker2.EndDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker3.EndDataCookingCallCount, 1);
            Assert.AreEqual(sourceDataCooker4.EndDataCookingCallCount, 1);

            // Check that each cooker received the proper number of data records
            Assert.AreEqual(sourceDataCooker1.CookDataElementCallCount, 1);
            Assert.AreEqual(sourceDataCooker2.CookDataElementCallCount, 2);
            Assert.AreEqual(sourceDataCooker3.CookDataElementCallCount, 4);
            Assert.AreEqual(sourceDataCooker4.CookDataElementCallCount, 3);

            // Check that each cooker received the expected data records
            Assert.IsTrue(sourceDataCooker1.ReceivedRecords.ContainsKey(testRecords[3]));
            Assert.IsTrue(sourceDataCooker2.ReceivedRecords.ContainsKey(testRecords[0]));
            Assert.IsTrue(sourceDataCooker2.ReceivedRecords.ContainsKey(testRecords[4]));
            Assert.IsTrue(sourceDataCooker3.ReceivedRecords.ContainsKey(testRecords[2]));
            Assert.IsTrue(sourceDataCooker3.ReceivedRecords.ContainsKey(testRecords[4]));
            Assert.IsTrue(sourceDataCooker3.ReceivedRecords.ContainsKey(testRecords[5]));
            Assert.IsTrue(sourceDataCooker3.ReceivedRecords.ContainsKey(testRecords[7]));
            Assert.IsTrue(sourceDataCooker4.ReceivedRecords.ContainsKey(testRecords[0]));
            Assert.IsTrue(sourceDataCooker4.ReceivedRecords.ContainsKey(testRecords[6]));
            Assert.IsTrue(sourceDataCooker4.ReceivedRecords.ContainsKey(testRecords[8]));

            // Make sure that cooker4 received test record 0 before cooker2
            Assert.IsTrue(sourceDataCooker4.ReceivedRecords[testRecords[0]] < sourceDataCooker2.ReceivedRecords[testRecords[0]]);

            // TestRecord 145 should be delivered twice
            // TestRecord 1000 should be delivered twice
            // TestRecord 500 should not ever be delivered
            int expectedReceivedCount = testRecords.Count + 2 - 1;
            Assert.AreEqual(expectedReceivedCount, dataCookerContext.CountOfTestRecordsReceived);

            CheckProgressReports(progress, testRecords.Count);
        }

        [TestMethod]
        [UnitTest]
        public void ProgressReportsTest()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);
            var dataCookerContext = new TestSourceDataCookerContext();

            // Create and populate cookers
            List<TestSourceDataCooker> cookers = new List<TestSourceDataCooker>();
            Dictionary<int, ReadOnlyHashSet<int>> cookerKeys = new Dictionary<int, ReadOnlyHashSet<int>>() {
                { 1, new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 })},
                { 2, new ReadOnlyHashSet<int>(new HashSet<int>() { 113, 145, 167, 1000 })},
                { 3, new ReadOnlyHashSet<int>(new HashSet<int>() { 200, 250, 286, 1000 })},
                { 4, new ReadOnlyHashSet<int>(new HashSet<int>() { 145, 316, 315, 301 })},
                { 5, new ReadOnlyHashSet<int>(new HashSet<int>() { 111, 222, 369 })},
                { 6, new ReadOnlyHashSet<int>(new HashSet<int>() { 167, 369, 1207 })},
                { 7, new ReadOnlyHashSet<int>(new HashSet<int>() { 167, 420, 1207 })}
            };

            for (int i = 1; i < 8; i++)
            {
                string name = "TestSourceDataCooker" + i.ToString();
                cookers.Add(CreateCooker(sourceSession, sourceParser.Id, name, cookerKeys[i]));
            }

            // data cooker order should be: { cooker1, cooker3 }, { cooker4 }, { cooker2 }, { cooker7 }, { cooker6 }, { cooker5 }
            // cooker1 and cooker3 could be in any order in pass 0.
            {
                // sourceDataCooker4: Pass=1, Block=0
                cookers[3].RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[] { cookers[2].Path });

                // sourceDataCooker2: Pass=2, Block=0
                cookers[1].RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[]
                    {
                        cookers[3].Path,
                        cookers[0].Path
                    });

                // sourceDataCooker7: Pass=3, Block=0
                cookers[6].RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[] { cookers[1].Path });

                // sourceDataCooker6: Pass=4, Block=0
                cookers[5].RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[]
                    {
                        cookers[1].Path,
                        cookers[6].Path
                    });

                // sourceDataCooker5: Pass=5, Block=0
                cookers[4].RequiredDataCookers = new ReadOnlyCollection<DataCookerPath>(
                    new[] { cookers[5].Path });
            }

            // setup data for the TestParser to "parse"
            var testRecords = new List<TestRecord>
            {
                // delivered to cooker4 in Pass1 and then cooker2 in Pass2
                new TestRecord {Key = 145, Value = "145"},

                // shouldn't be delivered to any cooker
                new TestRecord {Key = 500, Value = "500"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 250, Value = "250:1"},

                // delivered to cooker1 in Pass0
                new TestRecord {Key = 67, Value = "67"},

                // delivered to cooker3 in Pass0 and cooker2 in Pass2
                new TestRecord {Key = 1000, Value = "1000"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 250, Value = "250:2"},

                // delivered to cooker4 in Pass1
                new TestRecord {Key = 301, Value = "301"},

                // delivered to cooker3 in Pass0
                new TestRecord {Key = 286, Value = "286"},

                // delivered to cooker4 in Pass1
                new TestRecord {Key = 315, Value = "315"},

                // delivered to cooker2 in Pass2, cooker7 in Pass4, cooker6 in Pass5
                new TestRecord {Key = 167, Value="167"},

                // delivered to cooker6 in Pass5, cooker5 in Pass6
                new TestRecord {Key = 369, Value="369"},

                // delivered to cooker7 in Pass4, cooker6 in Pass5
                new TestRecord {Key = 1207, Value="1207"},

                // shouldn't be delivered to any cooker
                new TestRecord {Key = 1001, Value="1001"}

            };

            sourceParser.TestRecords = testRecords;
            Assert.AreEqual(sourceSession.RegisteredSourceDataCookers.Count, cookers.Count);

            TestProgress progress = new TestProgress();
            sourceSession.ProcessSource(new NullLogger(), progress, CancellationToken.None);

            CheckProgressReports(progress, testRecords.Count);
        }

        [TestMethod]
        [UnitTest]
        public void RegisteredDataKeysAreSentToSourceParser()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var sourceDataCooker1 = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 })
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker2"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 113, 145, 167, 1000 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker2);

            // setup data for the TestParser to "parse"
            var testRecords = new List<TestRecord>
            {
                // delivered to cooker4 first and then cooker2 in Pass1
                new TestRecord {Key = 145, Value = "145"},
            };

            sourceParser.TestRecords = testRecords;

            TestProgress progress = new TestProgress();
            sourceSession.ProcessSource(new NullLogger(), progress, CancellationToken.None);

            Assert.IsFalse(sourceParser.ReceivedAllEventsConsumed);
            Assert.IsTrue(
                sourceParser.RequestedDataKeys.Count ==
                sourceDataCooker1.DataKeys.Count + sourceDataCooker2.DataKeys.Count);

            CheckProgressReports(progress, testRecords.Count);
        }

        [TestMethod]
        [UnitTest]
        public void RegisteredDataKeysAreSentToSourceParserWithAllEventsTrue()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var sourceDataCooker1 = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 })
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker()
            {
                Path = DataCookerPath.ForSource(sourceParser.Id, "TestSourceDataCooker2"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { }),
                Options = SDK.Extensibility.DataCooking.SourceDataCooking.SourceDataCookerOptions.ReceiveAllDataElements,
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker2);

            // setup data for the TestParser to "parse"
            var testRecords = new List<TestRecord>
            {
                // delivered to cooker4 first and then cooker2 in Pass1
                new TestRecord {Key = 145, Value = "145"},
            };

            sourceParser.TestRecords = testRecords;

            TestProgress progress = new TestProgress();
            sourceSession.ProcessSource(new NullLogger(), progress, CancellationToken.None);

            Assert.IsTrue(sourceParser.ReceivedAllEventsConsumed);
            Assert.IsTrue(sourceParser.RequestedDataKeys.Count == sourceDataCooker1.DataKeys.Count);

            CheckProgressReports(progress, testRecords.Count);
        }
    }
}

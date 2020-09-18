// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class SourceSessionTests
    {
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
            var sourceParser = new TestSourceParser() {Id = "TestSourceParser"};
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
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker")
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
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker2"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 113, 145, 167, 1000 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker2);

            var sourceDataCooker3 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker3"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 200, 250, 286, 1000 }),
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker3);

            var sourceDataCooker4 = new TestSourceDataCooker(dataCookerContext)
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker4"),
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

            sourceSession.ProcessSource(new NullLogger(), new TestProgress(), CancellationToken.None);

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
        }

        [TestMethod]
        [UnitTest]
        public void RegisteredDataKeysAreSentToSourceParser()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var sourceDataCooker1 = new TestSourceDataCooker()
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 })
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker()
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker2"),
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

            sourceSession.ProcessSource(new NullLogger(), new TestProgress(), CancellationToken.None);

            Assert.IsFalse(sourceParser.ReceivedAllEventsConsumed);
            Assert.IsTrue(
                sourceParser.RequestedDataKeys.Count == 
                sourceDataCooker1.DataKeys.Count + sourceDataCooker2.DataKeys.Count);
        }
        
        [TestMethod]
        [UnitTest]
        public void RegisteredDataKeysAreSentToSourceParserWithAllEventsTrue()
        {
            var sourceParser = new TestSourceParser() { Id = "TestSourceParser" };
            var sourceSession = new SourceProcessingSession<TestRecord, TestParserContext, int>(sourceParser);

            var sourceDataCooker1 = new TestSourceDataCooker()
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker1"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() { 13, 45, 67 })
            };
            sourceSession.RegisterSourceDataCooker(sourceDataCooker1);

            var sourceDataCooker2 = new TestSourceDataCooker()
            {
                Path = new DataCookerPath(sourceParser.Id, "TestSourceDataCooker2"),
                DataKeys = new ReadOnlyHashSet<int>(new HashSet<int>() {}),
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

            sourceSession.ProcessSource(new NullLogger(), new TestProgress(), CancellationToken.None);

            Assert.IsTrue(sourceParser.ReceivedAllEventsConsumed);
            Assert.IsTrue(sourceParser.RequestedDataKeys.Count == sourceDataCooker1.DataKeys.Count);
        }
    }
}

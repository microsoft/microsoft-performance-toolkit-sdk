// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Testing;
using Microsoft.Performance.Testing.SDK;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataSourceAttribute = Microsoft.Performance.SDK.Processing.DataSourceAttribute;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    [DeploymentItem(TestCaseDataFileName)]
    public class DataSourceResolverTests
    {
        public const string TestCaseDataFileName = @"TestData/DataResolverTestCases.xml";

        [TestMethod]
        [UnitTest]
        public void Assign_NullDataSourcesThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => DataSourceResolver.Assign(null, Array.Empty<ProcessingSourceReference>()));
        }

        [TestMethod]
        [UnitTest]
        public void Assign_NullCustomDataSourcesThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => DataSourceResolver.Assign(Array.Empty<IDataSource>(), null));
        }

        [TestMethod]
        [UnitTest]
        public void Assign_UnsupportedTypesNotAssignedToCds()
        {
            var fakeCds = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.CustomDataSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".abc"),
                });

            var fakeDs = new FakeDataSource("fake");

            var assignment = DataSourceResolver.Assign(
                new[] { fakeDs, },
                new[] { fakeCds, });

            Assert.IsTrue(assignment.ContainsKey(fakeCds));
            Assert.IsTrue(assignment[fakeCds].Count() == 0);
        }

        [TestMethod]
        [UnitTest]
        public void Assign_FailedPreliminaryCheckNotAssignedToCds()
        {
            var fakeCds = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.CustomDataSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".abc"),
                    new FakeDataSourceAttribute()
                    {
                        PreliminaryCheckResult = false,
                    },
                });

            var fakeDs = new FakeDataSource("fake");

            var assignment = DataSourceResolver.Assign(
                new[] { fakeDs, },
                new[] { fakeCds, });

            Assert.IsTrue(assignment.ContainsKey(fakeCds));
            Assert.IsTrue(assignment[fakeCds].Count() == 0);
        }

        [TestMethod]
        [UnitTest]
        public void Assign_SucceededPreliminaryCheckFailedSupportsCheckNotAssignedToCds()
        {
            var fakeCds = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                Any.CustomDataSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".abc"),
                    new FakeDataSourceAttribute()
                    {
                        PreliminaryCheckResult = true,
                    },
                });

            var fakeDs = new FakeDataSource("fake");

            var assignment = DataSourceResolver.Assign(
                new[] { fakeDs, },
                new[] { fakeCds, });

            Assert.IsTrue(assignment.ContainsKey(fakeCds));
            Assert.IsTrue(assignment[fakeCds].Count() == 0);
        }

        [TestMethod]
        [UnitTest]
        public void Assign_SucceededPreliminaryCheckSucceededSupportsCheckAssignedToCds()
        {
            var cds = new FakeProcessingSource();
            var fakeCds = new ProcessingSourceReference(
                typeof(FakeProcessingSource),
                () => cds,
                Any.CustomDataSourceAttribute(),
                new HashSet<DataSourceAttribute>
                {
                    new FileDataSourceAttribute(".abc"),
                    new FakeDataSourceAttribute()
                    {
                        PreliminaryCheckResult = true,
                    },
                });

            var fakeDs = new FakeDataSource("fake");

            cds.IsDataSourceSupportedReturnValue[fakeDs] = true;

            var assignment = DataSourceResolver.Assign(
                new[] { fakeDs, },
                new[] { fakeCds, });

            Assert.IsTrue(assignment.ContainsKey(fakeCds));
            Assert.IsTrue(assignment[fakeCds].Count() == 1);
            Assert.AreEqual(fakeDs, assignment[fakeCds].Single());
        }

        [TestMethod]
        [UnitTest]
        [DynamicData(nameof(GetAssignTestCases), DynamicDataSourceType.Method)]
        public void AssignTests(AssignTestCase testCase)
        {
            Assert.IsNotNull(testCase);

            if (testCase.Debug)
            {
                Debugger.Break();
            }

            var actual = DataSourceResolver.Assign(testCase.DataSources, testCase.CustomDataSources);

            Assert.AreEqual(testCase.Expected.Count, actual.Count);
            foreach (var kvp in testCase.Expected)
            {
                var ecds = kvp.Key;
                var eds = kvp.Value.ToList();

                Assert.IsTrue(actual.ContainsKey(ecds), "'{0}' is not found in the assignment.", ecds);
                var ads = actual[ecds].ToList();

                CollectionAssert.AreEquivalent(eds, ads);
            }
        }

        public static IEnumerable<object[]> GetAssignTestCases()
        {
            var xml = XElement.Load(TestCaseDataFileName);
            var assignElement = xml.GetChild("Assign");
            var testCaseElements = assignElement.GetChildren("TestCase");
            foreach (var testCaseElement in testCaseElements)
            {
                yield return new[] { AssignTestCase.Parse(testCaseElement), };
            }
        }

        public class AssignTestCase
        {
            public AssignTestCase()
            {
                this.CustomDataSources = new List<ProcessingSourceReference>();
                this.DataSources = new List<IDataSource>();
                this.Expected = new Dictionary<ProcessingSourceReference, IEnumerable<IDataSource>>();
            }

            public bool Debug { get; set; }

            public string Description { get; set; }

            public string Id { get; set; }

            public List<ProcessingSourceReference> CustomDataSources { get; }

            public List<IDataSource> DataSources { get; }

            public Dictionary<ProcessingSourceReference, IEnumerable<IDataSource>> Expected { get; }

            public static AssignTestCase Parse(XElement testCaseElement)
            {
                Assert.IsNotNull(testCaseElement);

                var testCase = new AssignTestCase();
                if (testCaseElement.TryGetAttributeValueBool("Debug", out var debug))
                {
                    testCase.Debug = debug;
                }
                else
                {
                    testCase.Debug = false;
                }

                testCase.Description = testCaseElement.GetAttributeValue("Description");
                testCase.Id = testCaseElement.GetAttributeValue("Id");

                ParseParameters(
                    testCaseElement.GetChild("Parameters"),
                    testCase,
                    out var dataSourcesNameToImpl,
                    out var cdsNameToImpl);

                ParseExpected(
                    testCaseElement.GetChild("Expected"),
                    dataSourcesNameToImpl,
                    cdsNameToImpl,
                    testCase);

                return testCase;
            }

            public override string ToString()
            {
                return string.Concat(this.Id, ": ", this.Description);
            }

            private static void ParseParameters(
                XElement parametersElement,
                AssignTestCase testCaseToHydrate,
                out Dictionary<string, IDataSource> dataSourcesNameToImpl,
                out Dictionary<string, ProcessingSourceReference> cdsNameToImpl)
            {
                dataSourcesNameToImpl = ParseDataSources(
                    parametersElement.GetChild("DataSources"),
                    testCaseToHydrate);

                cdsNameToImpl = ParseCustomDataSources(
                    parametersElement.GetChild("CustomDataSources"),
                    dataSourcesNameToImpl,
                    testCaseToHydrate);
            }

            private static Dictionary<string, IDataSource> ParseDataSources(
                XElement dataSourcesElement,
                AssignTestCase testCaseToHydrate)
            {
                Assert.IsNotNull(dataSourcesElement);
                Assert.IsNotNull(testCaseToHydrate);

                var dataSourcesNameToImpl = new Dictionary<string, IDataSource>();

                var dataSourceElements = dataSourcesElement.GetChildren("DataSource");
                foreach (var dataSourceElement in dataSourceElements)
                {
                    var name = dataSourceElement.Value;
                    var value = new FakeDataSource(name);

                    testCaseToHydrate.DataSources.Add(value);

                    dataSourcesNameToImpl[name] = value;
                }

                return dataSourcesNameToImpl;
            }

            private static Dictionary<string, ProcessingSourceReference> ParseCustomDataSources(
                XElement customDataSourcesElement,
                Dictionary<string, IDataSource> dataSourcesNameToImpl,
                AssignTestCase testCaseToHydrate)
            {
                Assert.IsNotNull(customDataSourcesElement);
                Assert.IsNotNull(dataSourcesNameToImpl);
                Assert.IsNotNull(testCaseToHydrate);

                var cdsNameToImpl = new Dictionary<string, ProcessingSourceReference>();

                var cdsElements = customDataSourcesElement.GetChildren("CustomDataSource");
                foreach (var cdsElement in cdsElements)
                {
                    var cds = new FakeProcessingSource();

                    var name = cdsElement.GetChild("Name").Value;
                    var metadata = new ProcessingSourceAttribute(
                        Guid.NewGuid().ToString(),
                        name,
                        "Description");

                    if (cdsElement.TryGetChild("Supports", out var supportElement))
                    {
                        var supportedDataSourceElements = supportElement.GetChildren("DataSource");
                        foreach (var supportDataSource in supportedDataSourceElements)
                        {
                            var dsName = supportDataSource.Value;
                            if (!dataSourcesNameToImpl.TryGetValue(dsName, out var ds))
                            {
                                ds = new FakeDataSource(dsName);
                                dataSourcesNameToImpl[dsName] = ds;
                            }

                            cds.IsDataSourceSupportedReturnValue[ds] = true;
                        }
                    }

                    var fakeReference = new ProcessingSourceReference(
                        typeof(FakeProcessingSource),
                        () => cds,
                        metadata,
                        new HashSet<DataSourceAttribute>
                        {
                            new FakeDataSourceAttribute
                            {
                                PreliminaryCheckResult = true,
                            }
                        });

                    testCaseToHydrate.CustomDataSources.Add(fakeReference);
                    cdsNameToImpl[name] = fakeReference;
                }

                return cdsNameToImpl;
            }

            private static void ParseExpected(
                XElement expectedElement,
                Dictionary<string, IDataSource> dataSourcesNameToImpl,
                Dictionary<string, ProcessingSourceReference> cdsNameToImpl,
                AssignTestCase testCaseToHydrate)
            {
                Assert.IsNotNull(expectedElement);
                Assert.IsNotNull(dataSourcesNameToImpl);
                Assert.IsNotNull(cdsNameToImpl);
                Assert.IsNotNull(testCaseToHydrate);

                if (expectedElement.TryGetChild("CustomDataSourceAssignments", out var expectedMap))
                {
                    var assignments = expectedMap.GetChildren("Assignment");
                    foreach (var assignment in assignments)
                    {
                        var cdsName = assignment.GetChild("CustomDataSource").Value;

                        var dataSources = new List<IDataSource>();
                        var dataSourceElements = assignment.GetChild("DataSources").GetChildren("DataSource");
                        var seenDsNames = new HashSet<string>();
                        foreach (var dataSourceElement in dataSourceElements)
                        {
                            var dsName = dataSourceElement.Value;
                            if (seenDsNames.Add(dsName))
                            {
                                dataSources.Add(dataSourcesNameToImpl[dsName]);
                            }
                        }

                        testCaseToHydrate.Expected[cdsNameToImpl[cdsName]] = dataSources;
                    }
                }
            }
        }

        private sealed class FakeDataSource
            : IDataSource
        {
            public FakeDataSource(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public Uri Uri => new Uri("https://" + this.Name);

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var other = obj as FakeDataSource;
                return other != null &&
                    this.Name.Equals(other.Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }
        }

        private sealed class FakeDataSourceAttribute
            : DataSourceAttribute
        {
            public FakeDataSourceAttribute()
                : base(typeof(FakeDataSource))
            {
            }

            public bool PreliminaryCheckResult { get; set; } = true;

            protected override bool AcceptsCore(IDataSource dataSource)
            {
                return this.PreliminaryCheckResult;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    public class DataCookerWithoutDependencies
        : IDataCookerDescriptor
    {
        public virtual string DataCookerId => Path.DataCookerId;

        public string Description { get; set; }

        public virtual string SourceParserId => Path.SourceParserId;

        public DataCookerPath Path { get; set; }
    }

    public class ValidSourceDataCookerWithoutDependencies
        : DataCookerWithoutDependencies,
          ISourceDataCookerDependencyTypes
    {
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; set; }

        public IReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> DependencyTypes { get; set; }

        public DataProductionStrategy DataProductionStrategy { get; set; }
    }

    internal class TestScheduler : ISourceDataCookerScheduler
    {
        internal LinkedList<SchedulingPass> passes = new LinkedList<SchedulingPass>();

        internal TestScheduler()
        {
            SchedulingPass.CreatePass(0, this.passes);
        }

        public SchedulingPass Pass0 => this.passes.First.Value;

        public Dictionary<DataCookerPath, IDataCookerDescriptor> dataCookerPathsToDataCookers
            = new Dictionary<DataCookerPath, IDataCookerDescriptor>();

        public Dictionary<IDataCookerDescriptor, DataCookerSchedulingNode> cookersToNodes
            = new Dictionary<IDataCookerDescriptor, DataCookerSchedulingNode>();

        public DataCookerSchedulingNode GetSchedulingNode(DataCookerPath dataCookerPath)
        {
            if (this.dataCookerPathsToDataCookers.TryGetValue(dataCookerPath, out var dataCooker))
            {
                if (this.cookersToNodes.TryGetValue(dataCooker, out var schedulingNode))
                {
                    return schedulingNode;
                }
            }

            // it's a bug if we've reached this
            // cookers that don't have all their requirements met shouldn't be enabled on a source session.
            //
            throw new InvalidOperationException(
                $"A required cooker is not available for {dataCookerPath}");
        }

        public void AddNode(DataCookerSchedulingNode node)
        {
            cookersToNodes.Add(node.DataCooker, node);
            dataCookerPathsToDataCookers.Add(node.DataCooker.Path, node.DataCooker);
        }

        public SchedulingPass GetPass(int index)
        {
            SchedulingPass pass = this.Pass0;

            while (index > 0)
            {
                pass = pass.Next;
                index--;
            }

            return pass;
        }

        public int PassCount => this.passes.Count;
    }

    [TestClass]
    public class DataCookerSchedulingNodeTests
    {
        private ValidSourceDataCookerWithoutDependencies cookerWithNoDependencies;
        private DataCookerPath cookerWithNoDependenciesPath;

        private ValidSourceDataCookerWithoutDependencies cookerWithOneDependency;

        public static readonly string SourceParserId = "TestSourceParser";

        private readonly ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> emptyDependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
            new Dictionary<DataCookerPath, DataCookerDependencyType>());

        [TestInitialize]
        public void Initialize()
        {
            this.cookerWithNoDependencies = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerWithNoDependencies"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = this.emptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[] { }
            };

            this.cookerWithNoDependenciesPath = this.cookerWithNoDependencies.Path;

            this.cookerWithOneDependency = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerWithOneDependency"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = this.emptyDependencyTypes,
                RequiredDataCookers = new[]
                {
                    new DataCookerPath(SourceParserId, this.cookerWithNoDependencies.Path.DataCookerId),
                }
            };
        }

        [TestMethod]
        [UnitTest]
        public void ConstructorThrowsWithoutDependencySupportInDataCooker()
        {
            Assert.ThrowsException<ArgumentException>(
                () => DataCookerSchedulingNode.CreateSchedulingNode(new DataCookerWithoutDependencies()));
        }

        [TestMethod]
        [UnitTest]
        public void ConstructorSucceeds()
        {
            var node = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            Assert.IsTrue(node.DataCookerPath.Equals(this.cookerWithNoDependenciesPath));
            Assert.AreEqual(node.DataCooker, this.cookerWithNoDependencies);
            Assert.IsTrue(node.Status == SchedulingStatus.NotScheduled);
            Assert.IsTrue(node.CookerDependencies == this.cookerWithNoDependencies);
            Assert.IsTrue(node.SourceDataCookerDescriptor == this.cookerWithNoDependencies);
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCookers_NoDependencies()
        {
            var scheduler = new TestScheduler();
            var node = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            Assert.AreEqual(node.Status, SchedulingStatus.NotScheduled);

            node.Schedule(scheduler, null);

            Assert.AreEqual(node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(1, scheduler.GetPass(0).GetBlock(0).Nodes.Count);
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCookers_SingleDependency_DifferentStage()
        {
            var cookerWithNoDependenciesNode = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithOneDependency);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cookerWithNoDependenciesNode);
            scheduler.AddNode(node);

            node.Schedule(scheduler, null);

            Assert.AreEqual(node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cookerWithNoDependenciesNode.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(1, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cookerWithNoDependenciesNode));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCookers_CircularDependenciesThrows()
        {
            string cookerWithCircularDependency1Id = "CookerWithCircularDependency1";
            var cookerWithCircularDependency1Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency1Id);

            string cookerWithCircularDependency2Id = "CookerWithCircularDependency2";
            var cookerWithCircularDependency2Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency2Id);

            string cookerWithCircularDependency3Id = "CookerWithCircularDependency3";
            var cookerWithCircularDependency3Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency3Id);

            IDataCookerDescriptor cookerWithCircularDependency1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency1Id),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = this.emptyDependencyTypes,
                RequiredDataCookers = new[] { cookerWithCircularDependency2Path }
            };

            IDataCookerDescriptor cookerWithCircularDependency2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency2Id),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = this.emptyDependencyTypes,
                RequiredDataCookers = new[] { cookerWithCircularDependency3Path }
            };

            IDataCookerDescriptor cookerWithCircularDependency3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, cookerWithCircularDependency3Id),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = this.emptyDependencyTypes,
                RequiredDataCookers = new[] { cookerWithCircularDependency1Path }
            };

            var cookerWithCircularDependency1Node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithCircularDependency1);
            var cookerWithCircularDependency2Node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithCircularDependency2);
            var cookerWithCircularDependency3Node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithCircularDependency3);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cookerWithCircularDependency1Node);
            scheduler.AddNode(cookerWithCircularDependency2Node);
            scheduler.AddNode(cookerWithCircularDependency3Node);

            Assert.ThrowsException<InvalidOperationException>(
                () => cookerWithCircularDependency1Node.Schedule(scheduler, null));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_DependencyType_SameIteration_UsesSamePassAndSameBlock()
        {
            string cookerWithSameIterationDependencyTypeId = "Cooker_DependencyType_SameIteration";

            IDataCookerDescriptor cookerWithSameIterationDependencyType = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, cookerWithSameIterationDependencyTypeId),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.cookerWithNoDependenciesPath, DataCookerDependencyType.SamePass },
                    }),
                RequiredDataCookers = new[] { this.cookerWithNoDependenciesPath }
            };

            var cookerWithNoDependenciesNode = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithSameIterationDependencyType);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cookerWithNoDependenciesNode);

            node.Schedule(scheduler, null);

            Assert.AreEqual(cookerWithNoDependenciesNode.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(node.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cookerWithNoDependenciesNode));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_DependencyType_AsConsumed_UsesSamePassAndDifferentBlock()
        {
            string cookerWithAsConsumedDependencyTypeId = "Cooker_DependencyType_AsConsumed";

            IDataCookerDescriptor cookerWithAsConsumedDependencyType = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, cookerWithAsConsumedDependencyTypeId),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.cookerWithNoDependenciesPath, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { this.cookerWithNoDependenciesPath }
            };

            var cookerWithNoDependenciesNode = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithAsConsumedDependencyType);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cookerWithNoDependenciesNode);
            scheduler.AddNode(node);

            node.Schedule(scheduler, null);

            Assert.AreEqual(cookerWithNoDependenciesNode.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(node.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cookerWithNoDependenciesNode));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_NullDependencyTypes()
        {
            var cookerWithNoDependenciesNode = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var cookerWithAsConsumedDependencyType = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerWithNullDependencyTypes"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { this.cookerWithNoDependenciesPath }
            };

            var node = DataCookerSchedulingNode.CreateSchedulingNode(cookerWithAsConsumedDependencyType);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cookerWithNoDependenciesNode);
            scheduler.AddNode(node);

            node.Schedule(scheduler, null);

            Assert.AreEqual(node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cookerWithNoDependenciesNode.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(1, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cookerWithNoDependenciesNode));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_CheckBlockIndexWithChainedRequiredCookers()
        {
            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.cookerWithNoDependencies.Path, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path }
            };

            var cooker3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker3"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { cooker2.Path, DataCookerDependencyType.SamePass },
                    }),
                RequiredDataCookers = new[] { cooker2.Path }
            };

            var cooker1Node = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);
            var cooker2Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var cooker3Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker3);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cooker1Node);
            scheduler.AddNode(cooker2Node);
            scheduler.AddNode(cooker3Node);

            cooker3Node.Schedule(scheduler, null);

            Assert.AreEqual(cooker1Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker2Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker3Node.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cooker1Node));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(cooker2Node));

            // Even though Cooker3 is set to consume Cooker2 as "SameStage", Cooker2
            // consumes Cooker1... it must follow Cooker1, and therefore Cooker3 must
            // also follow Cooker1.
            //
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(cooker3Node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_DataProduction_AsConsumed_And_DependencyType_Default()
        {
            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = null,
                RequiredDataCookers = null
            };

            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { cooker1.Path }
            };

            var cooker1Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);
            var cooker2Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cooker1Node);
            scheduler.AddNode(cooker2Node);

            cooker2Node.Schedule(scheduler, null);

            Assert.AreEqual(cooker1Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker2Node.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cooker1Node));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(cooker2Node));
        }

        [TestMethod]
        [UnitTest]
        public void ScheduleDataCooker_MultipleDependencies()
        {
            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.cookerWithNoDependencies.Path, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path }
            };

            var cooker3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker3"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[]
                {
                    // this requirement should set: Pass=0, Block=1
                    cooker2.Path,

                    // this requirement should set: Pass=1, Block=0
                    this.cookerWithNoDependencies.Path
                }
            };

            var cooker4 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker4"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[]
                {
                    // this requirement should set: Pass=1, Block=0
                    this.cookerWithNoDependencies.Path,

                    // this requirement should not change the value
                    cooker2.Path,
                }
            };

            var cooker1Node = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);
            var cooker2Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var cooker3Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker3);
            var cooker4Node = DataCookerSchedulingNode.CreateSchedulingNode(cooker4);

            var scheduler = new TestScheduler();
            scheduler.AddNode(cooker1Node);
            scheduler.AddNode(cooker2Node);
            scheduler.AddNode(cooker3Node);
            scheduler.AddNode(cooker4Node);

            cooker3Node.Schedule(scheduler, null);
            cooker4Node.Schedule(scheduler, null);

            Assert.AreEqual(cooker1Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker2Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker3Node.Status, SchedulingStatus.Scheduled);
            Assert.AreEqual(cooker4Node.Status, SchedulingStatus.Scheduled);

            Assert.AreEqual(2, scheduler.PassCount);

            // An 'AreEqual' check fails here, because after scheduling the first required cooker for cooker3Node, it falls
            // into Pass 0, Block 3. After scheduling the second required cooker, it will be moved into Pass 1, Block 0, but
            // by then Pass 0, Block 3 is already created. It will be empty, so it's not a problem.
            //
            Assert.IsTrue(scheduler.GetPass(0).BlockCount >= 2);
            Assert.AreEqual(1, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(cooker1Node));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(cooker2Node));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(cooker3Node));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(cooker4Node));

            // See comment above - just double check that any extraneous blocks are empty.
            for (int x = 2; x < scheduler.GetPass(0).BlockCount; x++)
            {
                Assert.IsTrue(!scheduler.GetPass(0).GetBlock(x).Nodes.Any());
            }
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_ConstructedCorrectly()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var node = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);
            Assert.IsTrue(node is AsRequiredCookerSchedulingNode);
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_DirectScheduling_Nop()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredNode = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode);

            asRequiredNode.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.NotScheduled, asRequiredNode.Status);
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker.Path },
            };

            var asRequiredNode = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker);

            var scheduler = new TestScheduler();
            scheduler.AddNode(node);
            scheduler.AddNode(asRequiredNode);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode.Status);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_2()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker.Path, this.cookerWithNoDependencies.Path },
            };

            var asRequiredNode = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode);
            scheduler.AddNode(node);
            scheduler.AddNode(node2);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(2, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node2));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_3()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path, this.cookerWithNoDependencies.Path, asRequiredCooker2.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(node);
            scheduler.AddNode(node2);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode2.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(1, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(2, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node2));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_4()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };

            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path, this.cookerWithNoDependencies.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node1 = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var node3 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(node1);
            scheduler.AddNode(node2);
            scheduler.AddNode(node3);

            node1.Schedule(scheduler, null);
            node2.Schedule(scheduler, null);
            node3.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node3.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode2.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(2, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(node1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(node2));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_SingleDependency_1()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };
            
            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.NotScheduled, asRequiredNode2.Status);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_SingleDependency_2()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path },
            };
            
            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode2.Status);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(3, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(2).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_MultipleDependencies_1()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker3"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path, asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker3.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var asRequiredNode3 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker3);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(asRequiredNode3);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode3.Status);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(3, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(asRequiredNode3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(2).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_SingleDependent_MultipleDependencies_2()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };

            var asRequiredCooker3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker3"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path, asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker3.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var asRequiredNode3 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker3);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(asRequiredNode3);

            node.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode3.Status);

            Assert.AreEqual(1, scheduler.PassCount);
            Assert.AreEqual(4, scheduler.GetPass(0).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(2).Nodes.Contains(asRequiredNode3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(3).Nodes.Contains(node));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_MultipleDependents_1()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker1.Path },
            };

            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path, asRequiredCooker1.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var node1 = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var node3 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(node1);
            scheduler.AddNode(node2);
            scheduler.AddNode(node3);

            node1.Schedule(scheduler, null);
            node2.Schedule(scheduler, null);
            node3.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node3.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(2, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(2, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(node1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(node2));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_MultipleDependents_SingleDependency_1()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new [] { asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path },
            };

            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path, asRequiredCooker1.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node1 = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var node3 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(node1);
            scheduler.AddNode(node2);
            scheduler.AddNode(node3);

            node1.Schedule(scheduler, null);
            node2.Schedule(scheduler, null);
            node3.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node3.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(3, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(2, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(2).Nodes.Contains(node1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(node2));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_MultipleDependents_SingleDependency_2()
        {
            var asRequiredCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var asRequiredCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new [] { asRequiredCooker1.Path },
            };

            var cooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker2.Path },
            };

            var cooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path, asRequiredCooker2.Path },
            };

            var asRequiredNode1 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker1);
            var asRequiredNode2 = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker2);
            var node1 = DataCookerSchedulingNode.CreateSchedulingNode(cooker1);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(cooker2);
            var node3 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(asRequiredNode1);
            scheduler.AddNode(asRequiredNode2);
            scheduler.AddNode(node1);
            scheduler.AddNode(node2);
            scheduler.AddNode(node3);

            node1.Schedule(scheduler, null);
            node2.Schedule(scheduler, null);
            node3.Schedule(scheduler, null);

            Assert.AreEqual(SchedulingStatus.Scheduled, node1.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node2.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, node3.Status);
            Assert.AreEqual(SchedulingStatus.Scheduled, asRequiredNode1.Status);

            Assert.AreEqual(2, scheduler.PassCount);
            Assert.AreEqual(3, scheduler.GetPass(0).BlockCount);
            Assert.AreEqual(3, scheduler.GetPass(1).BlockCount);

            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(0).Nodes.Contains(node3));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(1).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(0).GetBlock(2).Nodes.Contains(node1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(0).Nodes.Contains(asRequiredNode1));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(1).Nodes.Contains(asRequiredNode2));
            Assert.IsTrue(scheduler.GetPass(1).GetBlock(2).Nodes.Contains(node2));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_DependentNotAligned_Throws()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new DataCookerPath[] { },
            };

            var cooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { asRequiredCooker.Path, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { asRequiredCooker.Path },
            };

            var asRequiredNode = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);
            var node = DataCookerSchedulingNode.CreateSchedulingNode(cooker);

            var scheduler = new TestScheduler();
            scheduler.AddNode(node);
            scheduler.AddNode(asRequiredNode);

            Assert.ThrowsException<InvalidOperationException>(() => node.Schedule(scheduler, null));
        }

        [TestMethod]
        [UnitTest]
        public void AsRequiredNode_NonAsRequiredDependency_Throws()
        {
            var asRequiredCooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "AsRequiredCooker"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsRequired,
                DependencyTypes = null,
                RequiredDataCookers = new[] { this.cookerWithNoDependencies.Path },
            };

            var cooker = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = null,
                RequiredDataCookers = new[] { asRequiredCooker.Path },
            };

            var asRequiredNode = DataCookerSchedulingNode.CreateSchedulingNode(asRequiredCooker);
            var node1 = DataCookerSchedulingNode.CreateSchedulingNode(cooker);
            var node2 = DataCookerSchedulingNode.CreateSchedulingNode(this.cookerWithNoDependencies);

            var scheduler = new TestScheduler();
            scheduler.AddNode(node1);
            scheduler.AddNode(node2);
            scheduler.AddNode(asRequiredNode);

            Assert.ThrowsException<InvalidOperationException>(() => node1.Schedule(scheduler, null));
        }
    }
}

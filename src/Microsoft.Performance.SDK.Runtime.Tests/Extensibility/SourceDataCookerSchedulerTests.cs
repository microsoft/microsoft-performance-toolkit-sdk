// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Scheduling;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class SourceDataCookerSchedulerTests
    {
        public static readonly string SourceParserId = "TestSourceParser";

        private static readonly ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType> EmptyDependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
            new Dictionary<DataCookerPath, DataCookerDependencyType>());

        private readonly List<IDataCookerDescriptor> expectedPass0Cookers = new List<IDataCookerDescriptor>();
        private readonly List<IDataCookerDescriptor> expectedPass1Cookers = new List<IDataCookerDescriptor>();

        private ValidSourceDataCookerWithoutDependencies dataCooker1;
        private ValidSourceDataCookerWithoutDependencies dataCooker2;
        private ValidSourceDataCookerWithoutDependencies dataCooker3;
        private ValidSourceDataCookerWithoutDependencies dataCooker4;
        private ValidSourceDataCookerWithoutDependencies dataCooker5;
        private ValidSourceDataCookerWithoutDependencies dataCooker6;
        private ValidSourceDataCookerWithoutDependencies dataCooker7;

        [TestInitialize]
        public void TestSetup()
        {
            // Pass: 0, Block: 0
            this.dataCooker1 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker1"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[] { }
            };

            // Pass: 0, Block: 0
            this.dataCooker4 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker4"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.dataCooker1.Path, DataCookerDependencyType.SamePass },
                    }),
                RequiredDataCookers = new[] { this.dataCooker1.Path }
            };

            // Pass: 0, Block: 1
            this.dataCooker3 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker3"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.dataCooker1.Path, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { this.dataCooker1.Path }
            };

            // Pass: 1, Block: 0
            this.dataCooker2 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker2"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new[] { this.dataCooker1.Path }
            };

            // Pass: 1, Block: 1
            this.dataCooker5 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker5"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new[] { this.dataCooker2.Path }
            };

            // Pass: 1, Block: 2
            this.dataCooker6 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker6"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.dataCooker5.Path, DataCookerDependencyType.AsConsumed },
                    }),
                RequiredDataCookers = new[] { this.dataCooker5.Path }
            };

            // Pass: 1, Block: 2
            this.dataCooker7 = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "Cooker7"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = new ReadOnlyDictionary<DataCookerPath, DataCookerDependencyType>(
                    new Dictionary<DataCookerPath, DataCookerDependencyType>()
                    {
                        { this.dataCooker6.Path, DataCookerDependencyType.SamePass },
                    }),
                RequiredDataCookers = new[] { this.dataCooker6.Path }
            };

            this.expectedPass0Cookers.Add(this.dataCooker1);
            this.expectedPass0Cookers.Add(this.dataCooker3);
            this.expectedPass0Cookers.Add(this.dataCooker4);
            this.expectedPass1Cookers.Add(this.dataCooker2);
            this.expectedPass1Cookers.Add(this.dataCooker5);
            this.expectedPass1Cookers.Add(this.dataCooker6);
            this.expectedPass1Cookers.Add(this.dataCooker7);
        }

        [TestMethod]
        [UnitTest]
        public void ConstructorTest()
        {
            var scheduler = new SourceDataCookerScheduler(SourceParserId);
            Assert.IsTrue(StringComparer.Ordinal.Equals(SourceParserId, scheduler.SourceParserId));
        }

        /// <summary>
        /// This is a general test where order of data cookers passed into
        /// <see cref="SourceDataCookerScheduler.ScheduleDataCookers"/> is not ordered specifically in a one direction.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void ScheduleDataCookers()
        {
            var testDataCookers = new List<ValidSourceDataCookerWithoutDependencies>
            {
                // Note: these are out-of-pass/block-order on purpose, as I didn't want them to be added in-order.

                this.dataCooker1,
                this.dataCooker2,
                this.dataCooker3,
                this.dataCooker4,
                this.dataCooker5,
                this.dataCooker6,
                this.dataCooker7
            };

            var scheduler = new SourceDataCookerScheduler(SourceParserId);
            scheduler.ScheduleDataCookers(testDataCookers);

            var cookersByPass = scheduler.DataCookersBySourcePass;

            // Expect that there are 2 passes
            Assert.AreEqual(cookersByPass.Count, 2);

            // Check the sizes of the 2 passes
            Assert.AreEqual(cookersByPass[0].Count, this.expectedPass0Cookers.Count);
            Assert.AreEqual(cookersByPass[1].Count, this.expectedPass1Cookers.Count);

            // Check the cookers are in their expected passes
            Assert.IsTrue(cookersByPass[0].Contains(this.dataCooker1));
            Assert.IsTrue(cookersByPass[0].Contains(this.dataCooker3));
            Assert.IsTrue(cookersByPass[0].Contains(this.dataCooker4));

            Assert.IsTrue(cookersByPass[1].Contains(this.dataCooker2));
            Assert.IsTrue(cookersByPass[1].Contains(this.dataCooker5));
            Assert.IsTrue(cookersByPass[1].Contains(this.dataCooker6));
            Assert.IsTrue(cookersByPass[1].Contains(this.dataCooker7));

            // Verify that necessary ordering withing a pass is met
            Assert.IsTrue(cookersByPass[0].IndexOf(this.dataCooker1) < cookersByPass[0].IndexOf(this.dataCooker3));
            Assert.IsTrue(cookersByPass[0].IndexOf(this.dataCooker4) < cookersByPass[0].IndexOf(this.dataCooker3));

            Assert.IsTrue(cookersByPass[1].IndexOf(this.dataCooker2) < cookersByPass[1].IndexOf(this.dataCooker5));
            Assert.IsTrue(cookersByPass[1].IndexOf(this.dataCooker2) < cookersByPass[1].IndexOf(this.dataCooker6));
            Assert.IsTrue(cookersByPass[1].IndexOf(this.dataCooker2) < cookersByPass[1].IndexOf(this.dataCooker7));
            Assert.IsTrue(cookersByPass[1].IndexOf(this.dataCooker5) < cookersByPass[1].IndexOf(this.dataCooker6));
            Assert.IsTrue(cookersByPass[1].IndexOf(this.dataCooker5) < cookersByPass[1].IndexOf(this.dataCooker7));
        }

        /// <summary>
        /// This is meant to test the case where a data cooker that requires other data cookers is passed into
        /// <see cref="SourceDataCookerScheduler.ScheduleDataCookers"/> before the required data cookers.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void ScheduleDependenciesAfterDependent()
        {
            var dataCookerA = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerA"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[] { }
            };

            var dataCookerB = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerB"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[] { }
            };

            var dataCookerC = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerC"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[] { }
            };

            var dataCookerD = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerD"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.AsConsumed,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[]
                {
                    dataCookerA.Path,
                    dataCookerB.Path,
                    dataCookerC.Path
                }
            };

            var dataCookerE = new ValidSourceDataCookerWithoutDependencies
            {
                Path = new DataCookerPath(SourceParserId, "CookerE"),
                Description = "Test data cooker",
                DataProductionStrategy = DataProductionStrategy.PostSourceParsing,
                DependencyTypes = EmptyDependencyTypes,
                RequiredDataCookers = new DataCookerPath[]
                {
                    dataCookerD.Path
                }
            };

            var sourceCookers = new List<ValidSourceDataCookerWithoutDependencies>()
            {
                dataCookerE,
                dataCookerD,
                dataCookerA,
                dataCookerB,
                dataCookerC
            };

            var scheduler = new SourceDataCookerScheduler(SourceParserId);
            scheduler.ScheduleDataCookers(sourceCookers);

            List<List<IDataCookerDescriptor>> cookersByPass = scheduler.DataCookersBySourcePass;

            // Expect that there are 2 passes
            Assert.AreEqual(cookersByPass.Count, 2);

            // Check the sizes of the 2 passes
            // Cookers A, B, C have no dependencies and should be in pass 0.
            // Cookers D & E have dependencies and should be in pass 1.
            Assert.AreEqual(cookersByPass[0].Count, 3);
            Assert.AreEqual(cookersByPass[1].Count, 2);

            // Check the cookers are in their expected passes
            Assert.IsTrue(cookersByPass[0].Contains(dataCookerA));
            Assert.IsTrue(cookersByPass[0].Contains(dataCookerB));
            Assert.IsTrue(cookersByPass[0].Contains(dataCookerC));

            Assert.IsTrue(cookersByPass[1].Contains(dataCookerD));
            Assert.IsTrue(cookersByPass[1].Contains(dataCookerE));

            // Verify that necessary ordering withing a pass is met
            Assert.IsTrue(cookersByPass[1].IndexOf(dataCookerD) < cookersByPass[1].IndexOf(dataCookerE));
        }
    }
}

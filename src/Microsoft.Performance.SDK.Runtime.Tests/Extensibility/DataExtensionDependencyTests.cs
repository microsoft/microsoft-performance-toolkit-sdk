// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility
{
    [TestClass]
    public class DataExtensionDependencyTests
    {
        [TestMethod]
        [UnitTest]
        public void ConstructorThrowsForNullDependencyTarget()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new DataExtensionDependencyState((IDataExtensionDependencyTarget)null));
        }

        [TestMethod]
        [UnitTest]
        public void ConstructorThrowsForNullOther()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new DataExtensionDependencyState((DataExtensionDependencyState)null));
        }

        [TestMethod]
        [UnitTest]
        public void AddError()
        {
            string testMessage = "Test Error Message";

            var target = new TestDataExtensionDependencyTarget();
            var targetDependency = new DataExtensionDependencyState(target);

            targetDependency.AddError(testMessage);

            Assert.AreEqual(targetDependency.Errors.Count, 1);
            Assert.IsTrue(targetDependency.Errors.First() == testMessage);
        }

        [TestMethod]
        [UnitTest]
        public void UpdateAvailability()
        {
            var target = new TestDataExtensionDependencyTarget();
            var targetDependency = new DataExtensionDependencyState(target);

            // Unavailable -> Available
            targetDependency.UpdateAvailability(DataExtensionAvailability.Available);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.Available);

            // Available -> Error
            targetDependency.UpdateAvailability(DataExtensionAvailability.Error);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.Error);

            // Error X->X Available
            targetDependency.UpdateAvailability(DataExtensionAvailability.Available);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.Error);

            // Error -> Undetermined
            targetDependency.AddError("Should go away");
            targetDependency.UpdateAvailability(DataExtensionAvailability.Undetermined);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.Undetermined);
            Assert.AreEqual(targetDependency.Errors.Count, 0);

            // Undetermined -> MissingIndirectRequirement
            targetDependency.UpdateAvailability(DataExtensionAvailability.MissingIndirectRequirement);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.MissingIndirectRequirement);

            // MissingIndirectRequirement -> MissingRequirement
            targetDependency.UpdateAvailability(DataExtensionAvailability.MissingRequirement);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.MissingRequirement);

            // MissingRequirement X->X MissingIndirectRequirement
            targetDependency.UpdateAvailability(DataExtensionAvailability.MissingIndirectRequirement);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.MissingRequirement);

            // MissingRequirement X->X IndirectError
            targetDependency.UpdateAvailability(DataExtensionAvailability.IndirectError);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.IndirectError);

            // IndirectError -> Error
            targetDependency.UpdateAvailability(DataExtensionAvailability.Error);
            Assert.AreEqual(targetDependency.Availability, DataExtensionAvailability.Error);
        }

        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_NoDependencies()
        {
            var testRepo = new TestDataExtensionRepository();

            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 0);
        }

        /// <summary>
        /// This test case adds one required source data cooker to the extension.
        /// The required source cooker has already been marked as available, so it is expected
        /// that the extension will also be marked as available.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_OneAvailableDataCooker()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            var cooker1 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker1"),
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.sourceCookersByPath.Add(cooker1.Path, cooker1);

            cooker0.requiredDataCookers.Add(new DataCookerPath(cooker1.Path));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 1);
        }

        /// <summary>
        /// This test case adds one required source data cooker to the extension.
        /// The required source cooker has been marked as error, so it is expected
        /// that the extension will also be marked with an indirect error.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_OneErroredDataCooker()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            var cooker1 = new TestSourceDataCookerReference(false)
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker1"),
                availability = DataExtensionAvailability.Error,
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.sourceCookersByPath.Add(cooker1.Path, cooker1);

            cooker0.requiredDataCookers.Add(new DataCookerPath(cooker1.Path));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.IndirectError);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 1);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 1);
        }

        /// <summary>
        /// This test case adds one required source data cooker (level 1) to the extension, and that
        /// data cooker has a requirement on a different data cooker (level 2).
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_RequiresCookers_TwoLevelsDeep()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            var cooker1 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker1"),
            };

            var cooker2 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker2"),
            };

            // the repository has all source cookers
            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.sourceCookersByPath.Add(cooker1.Path, cooker1);
            testRepo.sourceCookersByPath.Add(cooker2.Path, cooker2);

            // setup the requirements for the cookers
            cooker0.requiredDataCookers.Add(cooker1.Path);
            cooker1.requiredDataCookers.Add(cooker2.Path);

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker2.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker2.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker2.DependencyReferences.RequiredSourceDataCookerPaths.Count, 0);

            Assert.IsTrue(cooker1.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker1.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker1.DependencyReferences.RequiredSourceDataCookerPaths.Count, 1);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 2);
        }

        /// <summary>
        /// Source data cooker, Cooker0, depends on a source data cooker that is not available.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_MissingDependency()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            // the repository has all source cookers
            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);

            // setup the requirements for the cookers
            cooker0.requiredDataCookers.Add(new DataCookerPath("TestSourceParser", "Cooker1"));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.MissingRequirement);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyState.MissingDataCookers.Count, 1);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 0);
        }

        /// <summary>
        /// This test case adds one required source data cooker (Cooker1) to a source cooker (Cooker0).
        /// Cooker1 has a requirement on a different data cooker (Cooker2).
        /// Cooker2 has a dependency back to Cooker0, which should be caught and an error generated.
        /// Cooker0 should have an error for the circular dependency. The indirect error from Cooker1
        ///     won't be stored as IndirectError is a lower priority than Error.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_ThrowsOnCircularReferences()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            var cooker1 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker1"),
            };

            var cooker2 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker2"),
            };

            // the repository has all source cookers
            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.sourceCookersByPath.Add(cooker1.Path, cooker1);
            testRepo.sourceCookersByPath.Add(cooker2.Path, cooker2);

            // setup the requirements for the cookers
            cooker0.requiredDataCookers.Add(cooker1.Path);
            cooker1.requiredDataCookers.Add(cooker2.Path);
            cooker2.requiredDataCookers.Add(cooker0.Path);

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Error);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 1);
        }

        /// <summary>
        /// This test case adds one required source data cooker to the extension.
        /// Because the required cooker has a different source id than the base,
        /// we expect the additional validation method to result in an error.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_PerformAdditionalValidationIsCalled()
        {
            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser0", "Cooker0"),
            };

            var cooker1 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser1", "Cooker1"),
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.sourceCookersByPath.Add(cooker1.Path, cooker1);

            cooker0.requiredDataCookers.Add(new DataCookerPath(cooker1.Path));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Error);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 1);
        }

        /// <summary>
        /// This test case adds one required composite data cooker to the extension.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_AddOneCompositeDataCooker()
        {
            var cooker0 = new TestCompositeDataCookerReference
            {
                Path = new DataCookerPath("TestSourceParser", "Cooker0"),
            };

            var cooker1 = new TestCompositeDataCookerReference
            {
                Path = new DataCookerPath(DataCookerPath.EmptySourceParserId, "Cooker1"),
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.compositeCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.compositeCookersByPath.Add(cooker1.Path, cooker1);

            cooker0.requiredDataCookers.Add(new DataCookerPath(cooker1.Path));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker1.Availability == DataExtensionAvailability.Available);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Available);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredCompositeDataCookerPaths.Count, 1);
        }

        /// <summary>
        /// A composite data cooker requires a composite data cooker that isn't available.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_MissingCompositeDataCooker()
        {
            var cooker0 = new TestCompositeDataCookerReference
            {
                Path = new DataCookerPath(DataCookerPath.EmptySourceParserId, "Cooker0"),
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.compositeCookersByPath.Add(cooker0.Path, cooker0);

            // this doesn't exist
            cooker0.requiredDataCookers.Add(new DataCookerPath("Cooker1"));

            cooker0.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.MissingRequirement);
            Assert.AreEqual(cooker0.DependencyState.Errors.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredSourceDataCookerPaths.Count, 0);
            Assert.AreEqual(cooker0.DependencyReferences.RequiredCompositeDataCookerPaths.Count, 0);
            Assert.AreEqual(cooker0.DependencyState.MissingDataCookers.Count, 1);
        }

        /// <summary>
        /// A data processor requires a source data cooker and a composite data cooker,
        /// both which are available.
        /// </summary>
        [TestMethod]
        [UnitTest]
        public void EstablishAvailability_DataProcessorReliesOnAvailableDataCookers()
        {
            var dataProcessor = new TestDataProcessorReference
            {
                Id = "DataProcessor",
            };

            var cooker0 = new TestSourceDataCookerReference
            {
                Path = new DataCookerPath("TestSource0", "Cooker0"),
            };

            var cooker1 = new TestCompositeDataCookerReference
            {
                Path = new DataCookerPath(DataCookerPath.EmptySourceParserId, "Cooker0"),
            };

            var testRepo = new TestDataExtensionRepository();
            testRepo.sourceCookersByPath.Add(cooker0.Path, cooker0);
            testRepo.compositeCookersByPath.Add(cooker1.Path, cooker1);

            // setup the requirements
            dataProcessor.requiredDataCookers.Add(cooker0.Path);
            dataProcessor.requiredDataCookers.Add(cooker1.Path);

            dataProcessor.ProcessDependencies(testRepo);

            Assert.IsTrue(cooker0.Availability == DataExtensionAvailability.Available);
            Assert.IsTrue(cooker1.Availability == DataExtensionAvailability.Available);
            Assert.IsTrue(dataProcessor.Availability == DataExtensionAvailability.Available);

            Assert.AreEqual(dataProcessor.DependencyState.Errors.Count, 0);
            Assert.AreEqual(dataProcessor.DependencyReferences.RequiredSourceDataCookerPaths.Count, 1);
            Assert.AreEqual(dataProcessor.DependencyReferences.RequiredCompositeDataCookerPaths.Count, 1);
            Assert.AreEqual(dataProcessor.DependencyState.MissingDataCookers.Count, 0);
        }
    }
}

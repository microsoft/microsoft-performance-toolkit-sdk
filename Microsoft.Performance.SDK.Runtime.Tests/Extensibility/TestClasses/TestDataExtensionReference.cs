// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    internal abstract class TestDataExtensionReference
        : IDataExtensionReference
    {
        protected TestDataExtensionReference()
            : this(true)
        {
        }

        protected TestDataExtensionReference(bool useDataExtensionDependencyState)
        {
            if (useDataExtensionDependencyState)
            {
                this.DependencyState = new DataExtensionDependencyState(this);
            }
        }

        public HashSet<DataCookerPath> requiredDataCookers = new HashSet<DataCookerPath>();
        public virtual IReadOnlyCollection<DataCookerPath> RequiredDataCookers => this.requiredDataCookers;

        public HashSet<DataProcessorId> requiredDataProcessors = new HashSet<DataProcessorId>();
        public virtual IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => this.requiredDataProcessors;

        public virtual string Name { get; }

        public DataExtensionAvailability InitialAvailability { get; set; }

        public Action<IDataExtensionDependencyStateSupport, IDataExtensionReference> performAdditionalDataExtensionValidation;
        public virtual void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            this.performAdditionalDataExtensionValidation?.Invoke(dependencyStateSupport, requiredDataExtension);
        }

        public DataExtensionAvailability availability = DataExtensionAvailability.Undetermined;
        public DataExtensionAvailability Availability
        {
            get
            {
                if (this.DependencyState != null)
                {
                    return this.DependencyState.Availability;
                }

                return this.availability;
            }
        }

        public TestDataExtensionDependencies dependencyRetrieval = new TestDataExtensionDependencies();
        public IDataExtensionDependencies DependencyReferences
        {
            get
            {
                if (this.DependencyState != null)
                {
                    return this.DependencyState.DependencyReferences;
                }

                return this.dependencyRetrieval;
            }
        }

        public Action<IDataExtensionRepository> processDependencies;
        public void ProcessDependencies(
            IDataExtensionRepository availableDataExtensions)
        {
            if (this.processDependencies != null)
            {
                this.processDependencies.Invoke(availableDataExtensions);
            }
            else if (this.DependencyState != null)
            {
                this.DependencyState.ProcessDependencies(availableDataExtensions);
            }
        }

        public DataExtensionDependencyState DependencyState { get; set; }
    }
}

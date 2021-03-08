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
        private readonly bool useDataExtensionDependencyState;

        protected TestDataExtensionReference()
            : this(true)
        {
        }

        protected TestDataExtensionReference(bool useDataExtensionDependencyState)
        {
            this.useDataExtensionDependencyState = useDataExtensionDependencyState;
            this.DependencyState = new DataExtensionDependencyState(this);
        }

        ~TestDataExtensionReference()
        {
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
                if (this.useDataExtensionDependencyState &&
                    this.UseDependencyState())
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
                if (this.UseDependencyState())
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
            else if (this.UseDependencyState())
            {
                this.DependencyState.ProcessDependencies(availableDataExtensions);
            }
        }

        public IDataExtensionDependencyState DependencyState { get; set; }

        public int DisposeCalls { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return this.Name;
        }

        protected virtual void Dispose(bool disposing)
        {
            ++this.DisposeCalls;
        }

        protected bool UseDependencyState()
        {
            return this.useDataExtensionDependencyState &&
                this.DependencyState != null;
        }

        public void Release()
        {
            throw new NotImplementedException();
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Repository;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public abstract class TestDataExtensionReference
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
            DependencyState = new TestDataExtensionDependencyState(this);
        }

        protected TestDataExtensionReference(Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
            : this(true, createDependencyState)
        {
        }

        protected TestDataExtensionReference(
            bool useDataExtensionDependencyState,
            Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
        {
            this.useDataExtensionDependencyState = useDataExtensionDependencyState;
            DependencyState = createDependencyState(this);
        }

        public HashSet<DataCookerPath> requiredDataCookers = new HashSet<DataCookerPath>();
        public virtual IReadOnlyCollection<DataCookerPath> RequiredDataCookers => requiredDataCookers;

        public HashSet<DataProcessorId> requiredDataProcessors = new HashSet<DataProcessorId>();
        public virtual IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => requiredDataProcessors;

        public virtual string Name { get; }

        public DataExtensionAvailability InitialAvailability { get; set; }

        public Action<IDataExtensionDependencyStateSupport, IDataExtensionReference> performAdditionalDataExtensionValidation;
        public virtual void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataExtension)
        {
            performAdditionalDataExtensionValidation?.Invoke(dependencyStateSupport, requiredDataExtension);
        }

        public DataExtensionAvailability availability = DataExtensionAvailability.Undetermined;
        public DataExtensionAvailability Availability
        {
            get
            {
                if (useDataExtensionDependencyState &&
                    UseDependencyState())
                {
                    return DependencyState.Availability;
                }

                return availability;
            }
        }

        public TestDataExtensionDependencies dependencyRetrieval = new TestDataExtensionDependencies();
        public IDataExtensionDependencies DependencyReferences
        {
            get
            {
                if (UseDependencyState())
                {
                    return DependencyState.DependencyReferences;
                }

                return dependencyRetrieval;
            }
        }

        public Action<IDataExtensionRepository> processDependencies;

        public void ProcessDependencies(
            IDataExtensionRepository availableDataExtensions)
        {
            if (processDependencies != null)
            {
                processDependencies.Invoke(availableDataExtensions);
            }
            else if (UseDependencyState())
            {
                DependencyState.ProcessDependencies(availableDataExtensions);
            }
        }

        public IDataExtensionDependencyState DependencyState { get; set; }

        public int DisposeCalls { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            return Name;
        }

        protected virtual void Dispose(bool disposing)
        {
            ++DisposeCalls;
        }

        protected bool UseDependencyState()
        {
            return useDataExtensionDependencyState &&
                DependencyState != null;
        }
    }
}

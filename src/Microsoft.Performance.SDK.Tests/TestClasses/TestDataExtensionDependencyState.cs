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
    public class TestDataExtensionDependencyState
        : IDataExtensionDependencyState
    {
        public List<ErrorInfo> errors = new List<ErrorInfo>();
        public List<DataCookerPath> missingDataCookers = new List<DataCookerPath>();
        public List<DataProcessorId> missingDataProcessors = new List<DataProcessorId>();

        public TestDataExtensionDependencyState(IDataExtensionDependencyTarget dataDependencyTarget)
        {
            this.DataDependencyTarget = dataDependencyTarget;
        }

        public IDataExtensionDependencyTarget DataDependencyTarget { get; }

        public IReadOnlyCollection<ErrorInfo> Errors => this.errors;

        public IReadOnlyCollection<DataCookerPath> MissingDataCookers => this.missingDataCookers;

        public IReadOnlyCollection<DataProcessorId> MissingDataProcessors => this.missingDataProcessors;

        public DataExtensionAvailability Availability { get; set; }

        public IDataExtensionDependencies DependencyReferences { get; set; }

        public void AddError(ErrorInfo error)
        {
            errors.Add(error);
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void ProcessDependencies(IDataExtensionRepository availableDataExtensions)
        {
        }

        public void UpdateAvailability(DataExtensionAvailability availability)
        {
            this.Availability = availability;
        }
    }
}

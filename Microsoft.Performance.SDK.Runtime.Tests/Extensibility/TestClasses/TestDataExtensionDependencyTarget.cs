// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestDataExtensionDependencyTarget
        : IDataExtensionDependencyTarget
    {
        public List<DataCookerPath> requiredDataCookers = new List<DataCookerPath>();
        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers => new ReadOnlyCollection<DataCookerPath>(this.requiredDataCookers);


        public List<DataProcessorId> requiredDataProcessors = new List<DataProcessorId>();
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors => new ReadOnlyCollection<DataProcessorId>(this.requiredDataProcessors);

        public string Name { get; set; }

        public DataExtensionAvailability InitialAvailability { get; set; }

        public Action<IDataExtensionDependencyStateSupport, IDataExtensionReference> validationAction = null;
        public void PerformAdditionalDataExtensionValidation(
            IDataExtensionDependencyStateSupport dependencyStateSupport,
            IDataExtensionReference requiredDataCooker)
        {
            this.validationAction?.Invoke(dependencyStateSupport, requiredDataCooker);
        }
    }
}

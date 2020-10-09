// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public class TestDataExtensionDependencies
        : IDataExtensionDependencies
    {
        public TestDataExtensionDependencies()
        {
            this.requiredSourceDataCookersByPath = new HashSet<DataCookerPath>();
            this.requiredCompositeDataCookersByPath = new HashSet<DataCookerPath>();
            this.requiredDataProcessorsById = new HashSet<DataProcessorId>();
        }

        public HashSet<DataCookerPath> requiredSourceDataCookersByPath;
        public IReadOnlyCollection<DataCookerPath> RequiredSourceDataCookerPaths => this.requiredSourceDataCookersByPath;

        public HashSet<DataCookerPath> requiredCompositeDataCookersByPath;
        public IReadOnlyCollection<DataCookerPath> RequiredCompositeDataCookerPaths => this.requiredCompositeDataCookersByPath;

        public HashSet<DataProcessorId> requiredDataProcessorsById;
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessorIds => this.requiredDataProcessorsById;
    }
}

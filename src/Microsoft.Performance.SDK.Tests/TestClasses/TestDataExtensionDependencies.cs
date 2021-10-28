// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestDataExtensionDependencies
        : IDataExtensionDependencies
    {
        public TestDataExtensionDependencies()
        {
            requiredSourceDataCookersByPath = new HashSet<DataCookerPath>();
            requiredCompositeDataCookersByPath = new HashSet<DataCookerPath>();
            requiredDataProcessorsById = new HashSet<DataProcessorId>();
        }

        public HashSet<DataCookerPath> requiredSourceDataCookersByPath;
        public IReadOnlyCollection<DataCookerPath> RequiredSourceDataCookerPaths => requiredSourceDataCookersByPath;

        public HashSet<DataCookerPath> requiredCompositeDataCookersByPath;
        public IReadOnlyCollection<DataCookerPath> RequiredCompositeDataCookerPaths => requiredCompositeDataCookersByPath;

        public HashSet<DataProcessorId> requiredDataProcessorsById;
        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessorIds => requiredDataProcessorsById;
    }
}

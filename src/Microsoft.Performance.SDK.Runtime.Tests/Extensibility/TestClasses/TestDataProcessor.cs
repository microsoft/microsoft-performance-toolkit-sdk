// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    public sealed class TestDataProcessor
        : IDataProcessor
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public IReadOnlyCollection<DataCookerPath> RequiredDataCookers { get; set; }

        public IReadOnlyCollection<DataProcessorId> RequiredDataProcessors { get; set; }

        public void OnDataAvailable(IDataExtensionRetrieval dataExtensionRetrieval)
        {
        }
    }
}

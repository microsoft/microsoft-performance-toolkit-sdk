// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataProcessing;
using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    internal class TestDataProcessorReference
        : TestDataExtensionReference,
          IDataProcessorReference
    {
        public TestDataProcessorReference()
            : base()
        {
        }

        public TestDataProcessorReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState)
        {
        }

        public Func<IDataExtensionRetrieval, IDataProcessor> getOrCreateInstance;
        public IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData)
        {
            return this.getOrCreateInstance?.Invoke(requiredData);
        }

        public string Id { get; set; }

        public string Description { get; set; }

        public override string Name => this.Id;
    }
}

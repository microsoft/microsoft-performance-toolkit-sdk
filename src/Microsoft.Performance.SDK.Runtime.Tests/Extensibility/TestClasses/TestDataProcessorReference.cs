// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    // TODO: __SDK_DP__
    // Redesign Data Processor API
    ////internal class TestDataProcessorReference
    ////    : TestDataExtensionReference,
    ////      IDataProcessorReference
    ////{
    ////    public TestDataProcessorReference()
    ////        : base()
    ////    {
    ////        this.Id = Guid.NewGuid().ToString();
    ////    }

    ////    public TestDataProcessorReference(bool useDataExtensionDependencyState)
    ////        : base(useDataExtensionDependencyState)
    ////    {
    ////    }

    ////    public Func<IDataExtensionRetrieval, IDataProcessor> getOrCreateInstance;
    ////    public IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData)
    ////    {
    ////        return this.getOrCreateInstance?.Invoke(requiredData);
    ////    }

    ////    public string Id { get; set; }

    ////    public string Description { get; set; }

    ////    public override string Name => this.Id;
    ////}
}

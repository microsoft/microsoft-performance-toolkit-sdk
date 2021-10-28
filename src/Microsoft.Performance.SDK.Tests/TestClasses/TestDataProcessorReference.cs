// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// TODO: __SDK_DP__
// Redesign Data Processor API
// using System;
// using Microsoft.Performance.SDK.Extensibility;
// using Microsoft.Performance.SDK.Extensibility.DataProcessing;
// using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.DataProcessors;
// using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;

// namespace Microsoft.Performance.SDK.Tests.TestClasses
// {
//     public class TestDataProcessorReference
//         : TestDataExtensionReference,
//           IDataProcessorReference
//     {
//         public TestDataProcessorReference()
//             : base()
//         {
//             Id = Guid.NewGuid().ToString();
//         }

//         public TestDataProcessorReference(bool useDataExtensionDependencyState)
//             : base(useDataExtensionDependencyState)
//         {
//             Id = Guid.NewGuid().ToString();
//         }

//         public TestDataProcessorReference(
//             Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
//             : this(true, createDependencyState)
//         {
//         }

//         public TestDataProcessorReference(
//             bool useDataExtensionDependencyState,
//             Func<IDataExtensionDependencyTarget, IDataExtensionDependencyState> createDependencyState)
//             : base(useDataExtensionDependencyState, createDependencyState)
//         {
//             Id = Guid.NewGuid().ToString();
//         }

//         public Func<IDataExtensionRetrieval, IDataProcessor> getOrCreateInstance;
//         public IDataProcessor GetOrCreateInstance(IDataExtensionRetrieval requiredData)
//         {
//             return getOrCreateInstance?.Invoke(requiredData);
//         }

//         public string Id { get; set; }

//         public string Description { get; set; }

//         public override string Name => Id;
//     }
// }

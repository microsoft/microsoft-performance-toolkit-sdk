// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.Extensibility.DataExtensions.Dependency;
using Microsoft.Performance.SDK.Tests.TestClasses;

/// <summary>
///     Wrapper classes for extension references that use the runtime's <see cref="DataExtensionDependencyState"/>.
///     <para/>
///     This class is internal to the runtime, so the base test classes in the SDK tests is not able to directly access
///     this.
/// </summary>
namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.TestClasses
{
    internal class TestRuntimeCompositeCookerReference
        : TestCompositeDataCookerReference
    {
        public TestRuntimeCompositeCookerReference()
            : this(true)
        {
        }

        public TestRuntimeCompositeCookerReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState, (x) => new DataExtensionDependencyState(x))
        {
        }
    }

    internal class TestRuntimeSourceCookerReference
        : TestSourceDataCookerReference
    {
        public TestRuntimeSourceCookerReference()
            : this(true)
        {
        }

        public TestRuntimeSourceCookerReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState, (x) => new DataExtensionDependencyState(x))
        {
        }
    }

    internal class TestRuntimeTableExtensionReference
        : TestTableExtensionReference
    {
        public TestRuntimeTableExtensionReference()
            : this(true)
        {
        }

        public TestRuntimeTableExtensionReference(bool useDataExtensionDependencyState)
            : base(useDataExtensionDependencyState, (x) => new DataExtensionDependencyState(x))
        {
        }
    }

    // TODO: __SDK_DP__
    // Redesign Data Processor API
    // internal class TestRuntimeDataProcessorReference
    //     : TestDataProcessorReference
    // {
    //     public TestRuntimeDataProcessorReference()
    //         : this(true)
    //     {
    //     }

    //     public TestRuntimeDataProcessorReference(bool useDataExtensionDependencyState)
    //         : base(useDataExtensionDependencyState, (x) => new DataExtensionDependencyState(x))
    //     {
    //     }
    // }
}

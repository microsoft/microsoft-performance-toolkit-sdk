// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.Tests.Fixtures;

public class TestColumnVariantsProcessor
    : IColumnVariantsProcessor
{
    public void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes)
    {
        // NOOP
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking.SourceDataCooking;
using Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataTypes;

namespace Microsoft.Performance.SDK.Runtime.Tests.Extensibility.DataCookers
{
    public class ValidSourceDataCooker
        : BaseSourceDataCooker<TestDataElement, TestDataContext, int>
    {
        public ValidSourceDataCooker() 
            : base("SourceId", "CookerId")
        {
        }

        public override string Description { get; } = "Test Source Data Cooker";

        public override ReadOnlyHashSet<int> DataKeys { get; } = new ReadOnlyHashSet<int>(new HashSet<int>());

        public override DataProcessingResult CookDataElement(
            TestDataElement data, 
            TestDataContext context,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}

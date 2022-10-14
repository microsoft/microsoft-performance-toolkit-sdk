// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Tests.TestClasses
{
    public class TestTableAttribute
        : TableAttribute
    {
        public TestTableAttribute(string buildTableActionMethodName)
            : base(DefaultTableDescriptorPropertyName, buildTableActionMethodName, DefaultIsDataAvailableMethodName)
        {
        }
    }
}

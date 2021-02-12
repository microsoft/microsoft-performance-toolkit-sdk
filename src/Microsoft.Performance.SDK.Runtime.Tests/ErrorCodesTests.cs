// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    [TestClass]
    public class ErrorCodesTests
    {
        [TestMethod]
        [UnitTest]
        public void TestConversions()
        {
            foreach (var code in ErrorCodes.All)
            {
                string str = code;
                Assert.AreEqual(code.Code, str);
            }
        }
    }
}

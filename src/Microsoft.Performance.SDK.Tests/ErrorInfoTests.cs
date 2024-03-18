// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using Microsoft.Performance.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.SDK.Tests
{
    [TestClass]
    public class ErrorInfoTests
    {
        [TestMethod]
        [UnitTest]
        public void ToString_Default()
        {
            var info = new TestError("test_case_error")
            {
                Target = "ToString_Default",
                Details = new[]
                {
                    new ErrorInfo("detail_code", "Detail one")
                    {
                        Details = new[]
                        {
                            new ErrorInfo("nested_datail", "nesting"),
                        },
                        Target = "First",
                    },
                    new ErrorInfo("another_detail", "yet again"),
                },
                Data = "test data",
            };

            Console.Out.WriteLine(info.ToString("G", null));
        }

        [TestMethod]
        [UnitTest]
        public void TestSerialization()
        {
            var info = new TestError("test_case_error")
            {
                Target = "ToString_Default",
                Details = new[]
                {
                    new ErrorInfo("detail_code", "Detail one")
                    {
                        Details = new[]
                        {
                            new ErrorInfo("nested_datail", "nesting"),
                        },
                        Target = "First",
                    },
                    new ErrorInfo("another_detail", "yet again"),
                },
                Data = "test data",
            };

            using (var stream = new MemoryStream())
            {
                try
                {
                    JsonSerializer.Serialize(stream, info);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }

                stream.Seek(0, SeekOrigin.Begin);
                ErrorInfo deserialized = null;
                try
                {
                    deserialized = JsonSerializer.Deserialize<TestError>(stream);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }

                Assert.AreEqual(info.ToString(), deserialized.ToString());
            }
        }

        private sealed class TestError
            : ErrorInfo
        {
            public TestError(string code)
                : base(code, code)
            {
            }

            public string Data { get; set; }
        }
    }
}

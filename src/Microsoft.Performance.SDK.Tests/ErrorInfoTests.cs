// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.Serialization;
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
                var serializer = new DataContractSerializer(
                    typeof(ErrorInfo),
                    new[] { typeof(TestError), typeof(ErrorInfo[]) });
                serializer.WriteObject(stream, info);
                stream.Seek(0, SeekOrigin.Begin);
                var deserialized = (ErrorInfo)serializer.ReadObject(stream);

                Assert.AreEqual(info.ToString(), deserialized.ToString());
                Assert.IsInstanceOfType(deserialized, typeof(TestError));
                Assert.AreEqual(info.Data, ((TestError)deserialized).Data);
                Assert.AreEqual(info.Details[0].Details[0].Code, deserialized.Details[0].Details[0].Code);
            }
        }

        [Serializable]
        private sealed class TestError
            : ErrorInfo
        {
            public TestError(string code) 
                : base(code, code)
            {
            }

            private TestError(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                this.Data = info.GetString(nameof(Data));
            }

            public string Data { get; set; }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue(nameof(Data), this.Data);
            }
        }
    }
}

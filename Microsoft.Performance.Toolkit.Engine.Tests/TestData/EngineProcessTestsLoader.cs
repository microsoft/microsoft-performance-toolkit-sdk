// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestData
{
    public static class EngineProcessTestsLoader
    {
        public static EngineProcessTestSuiteDto Load(
            string path)
        {
            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
            };

            var serializer = new DataContractJsonSerializer(typeof(EngineProcessTestSuiteDto), settings);

            EngineProcessTestSuiteDto testSuite;
            using (var stream = File.OpenRead(path))
            {
                testSuite = (EngineProcessTestSuiteDto)serializer.ReadObject(stream);
            }

            Assert.IsNotNull(testSuite);

            return testSuite;
        }
    }
}

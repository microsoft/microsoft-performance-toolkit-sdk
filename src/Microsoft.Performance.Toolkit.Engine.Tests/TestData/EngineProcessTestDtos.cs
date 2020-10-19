// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestData
{
    [DataContract]
    public class EngineProcessTestSuiteDto
    {
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "testCases")]
        public EngineProcessTestCaseDto[] TestCases { get; set; }
    }

    [DataContract]
    public class EngineProcessTestCaseDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "comments")]
        public string[] Comments { get; set; }

        [DataMember(Name = "debugBreak")]
        public bool DebugBreak { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "filePaths")]
        public string[] FilePaths { get; set; }

        [DataMember(Name = "cookersToEnable")]
        public string[] CookersToEnable { get; set; }

        [DataMember(Name = "expectedOutputs")]
        public Dictionary<string, Dictionary<string, string>[]> ExpectedOutputs { get; set; }

        [DataMember(Name = "throwingOutputs")]
        public string[] ThrowingOutputs { get; set; }

        public override string ToString()
        {
            return this.Id + ": " + this.Description;
        }
    }
}

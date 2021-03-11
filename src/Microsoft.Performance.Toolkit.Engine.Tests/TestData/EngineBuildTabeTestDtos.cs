// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Performance.Toolkit.Engine.Tests.TestData
{
    [DataContract]
    public class EngineBuildTableTestSuiteDto
    {
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "testCases")]
        public EngineBuildTableTestCaseDto[] TestCases { get; set; }
    }

    [DataContract]
    public class EngineBuildTableTestCaseDto
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

        [DataMember(Name = "tablesToEnable")]
        public string[] TablesToEnable { get; set; }

        [DataMember(Name = "expectedOutputs")]
        public Dictionary<string, Dictionary<string, string>[]> ExpectedOutputs { get; set; }

        [DataMember(Name = "throwingTables")]
        public string[] ThrowingTables { get; set; }

        public override string ToString()
        {
            return this.Id + ": " + this.Description;
        }
    }
}

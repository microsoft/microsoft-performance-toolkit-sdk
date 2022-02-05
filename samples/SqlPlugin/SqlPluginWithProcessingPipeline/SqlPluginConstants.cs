// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace SqlPluginWithProcessingPipeline
{
    public static class SqlPluginConstants
    {
        // XML namespace for file we are parsing
        public const string SqlXmlNamespace = @"http://tempuri.org/TracePersistence.xsd";

        // ID for source parser
        public const string ParserId = "SqlSourceParser";

        // ID for example data cooker
        public const string CookerId = "SqlDataCooker";

        // Path from source parser to example data cooker. This is the path
        // that is used to programatically access the data cooker's data outputs,
        // and can be created by external binaries by just knowing the
        // parser and cooker IDs defined above
        public static readonly DataCookerPath CookerPath =
            DataCookerPath.ForSource(SqlPluginConstants.ParserId, SqlPluginConstants.CookerId);
    }
}

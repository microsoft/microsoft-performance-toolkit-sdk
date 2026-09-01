// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Extensibility;

namespace SimplePlugin
{
    public static class Constants
    {
        public const string ParserId = "YourParserId";

        public const string CookerId = "YourDataCookerId";

        public static readonly DataCookerPath CookerPath = DataCookerPath.ForSource(ParserId, CookerId);
    }
}

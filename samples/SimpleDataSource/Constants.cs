using Microsoft.Performance.SDK.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleAddIn
{
    public static class Constants
    {
        public const string ParserId = "YourParserId";

        public const string CookerId = "YourDataCookerId";

        public static readonly DataCookerPath CookerPath = DataCookerPath.ForSource(ParserId, CookerId);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Extensibility.DataCooking;

namespace Microsoft.Performance.SDK.Runtime.Tests
{
    public class TestCookedDataReflector : CookedDataReflector
    {
        public static readonly string ParserId = "TestCookerParser";
        public static readonly string CookerId = "TestDataReflector";

        public static readonly DataCookerPath DefaultCookerPath
            = new DataCookerPath(ParserId, CookerId);

        public TestCookedDataReflector(DataCookerPath cookerPath)
            : base(cookerPath)
        {
        }

        public TestCookedDataReflector()
            : base(DefaultCookerPath)
        {
        }

        [DataOutput]
        public bool HasData => true;

        [DataOutput]
        public Func<int, int, int> AddFunc => AddValues;

        private int AddValues(int x, int y)
        {
            return x + y;
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.Testing.SDK
{
    public static class FakeCustomDataSourceOptions
    {
        public static class Ids
        {
            public static readonly object One = new object();
            public static readonly object Two = new object();
            public static readonly object Three = new object();
        }

        public static readonly Option FakeOptionOne = new Option(Ids.One, "one", 1, 1);
        public static readonly Option FakeOptionTwo = new Option(Ids.Two, "two", 1, 1);
        public static readonly Option FakeOptionThree = new Option(Ids.Three, "three", 1, int.MaxValue);
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.DTO.Enums
{
    internal enum TableLayoutStyle
    {
        None = 0x00,
        Graph = 0x01,
        Table = 0x02,
        GraphAndTable = Graph | Table,
    }
}

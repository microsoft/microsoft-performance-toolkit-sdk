// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     How a table should be presented in the UI.
    /// </summary>
    public enum TableLayoutStyle
    {
        /// <summary>
        ///     Do not draw a graph or table.
        /// </summary>
        None = 0x00,

        /// <summary>
        ///     Draw only a graph of the data.
        /// </summary>
        Graph = 0x01,

        /// <summary>
        ///     Draw only a table of the data.
        /// </summary>
        Table = 0x02,

        /// <summary>
        ///     Draw a graph and a table of the data.
        /// </summary>
        GraphAndTable = Graph | Table,
    }
}

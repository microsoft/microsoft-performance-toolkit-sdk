// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    public interface ITableResult
    {
        public IEnumerable<TableConfiguration> BuiltInTableConfigurations { get; }

        public TableConfiguration DefaultConfiguration { get; }

        public int RowCount { get; }

        public IReadOnlyCollection<IDataColumn> Columns { get; }

        public IReadOnlyDictionary<string, TableCommandCallback> TableCommands { get; }

        public Func<int, IEnumerable<TableRowDetailEntry>> TableRowDetailsGenerator { get; }
    }
}

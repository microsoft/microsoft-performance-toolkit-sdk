﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace Microsoft.Performance.Toolkit.Engine
{
    public interface ITableResult
    {
        /// <inheritdoc cref="ITableBuilder.BuiltInTableConfigurations"/>
        IEnumerable<TableConfiguration> BuiltInTableConfigurations { get; }

        /// <inheritdoc cref="ITableBuilder.DefaultConfiguration"/>
        TableConfiguration DefaultConfiguration { get; }

        /// <inheritdoc cref="ITableBuilderWithRowCount.RowCount"/>
        int RowCount { get; }

        /// <summary>
        ///     Gets the collection of columns that have been added to the table.
        /// </summary>
        IReadOnlyCollection<IDataColumn> Columns { get; }

        /// <summary>
        ///     Gets the dictionary of table command name and <see cref="TableCommandCallback"/> that have been assigned to the table.
        /// </summary>
        IReadOnlyDictionary<string, TableCommandCallback> TableCommands { get; }

        /// <summary>
        ///     Gets the function to be used to generate details for a given row in the table.
        ///     <para/>
        ///     The function will take the row number being examined by the user, and should
        ///     return a collection of one or more entries given details for that row. This can
        ///     be used to support information on a row by row basis that does not make
        ///     sense in a columnar format.
        ///     <para/>
        ///     This method is not required to be set; it is perfectly acceptable
        ///     to not support details. This method may be called only zero (0) or one (1)
        ///     time(s) per instance.
        /// </summary>
        Func<int, IEnumerable<TableRowDetailEntry>> TableRowDetailsGenerator { get; }
    }
}

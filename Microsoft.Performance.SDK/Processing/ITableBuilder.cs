// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     ITableBuilder is the only way to get ITableBuilderWithRowCount to force the
    ///     custom data source to set a row count.
    /// </summary>
    public interface ITableBuilder
    {
        /// <summary>
        ///     Gets the collection of <see cref="TableConfiguration"/>s that
        ///     are already known for this table.
        /// </summary>
        IEnumerable<TableConfiguration> BuiltInTableConfigurations { get; }

        /// <summary>
        ///     Gets the default configuration for this table.
        /// </summary>
        TableConfiguration DefaultConfiguration { get; }

        /// <summary>
        ///     Registers a command that users can execute against this table.
        /// </summary>
        /// <param name="commandName">
        ///     The name of the command. This name ultimately shows up in the context
        ///     menu in the UI when the user right clicks a table. The name of the command
        ///     is *NOT* case sensitive, and must be unique for the table. Any whitespace
        ///     around the name is trimmed, so " name " will be stored as "name".
        /// </param>
        /// <param name="callback">
        ///     The callback to execute when the user chooses this command.
        /// </param>
        /// <returns>
        ///     This instance of the builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="commandName"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="callback"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="commandName"/> is whitespace.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     A command with <paramref name="commandName"/> has already been added
        ///     to this instance, irrespective of casing.
        /// </exception>
        ITableBuilder AddTableCommand(string commandName, TableCommandCallback callback);

        /// <summary>
        ///     Adds a configuration for this table to the builder.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration to add.
        /// </param>
        /// <returns>
        ///     This instance of the builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="configuration"/> is <c>null</c>.
        /// </exception>
        ITableBuilder AddTableConfiguration(TableConfiguration configuration);

        /// <summary>
        ///     Sets the given configuration as the default for this table.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration to set as the default.
        /// </param>
        /// <returns>
        ///     This instance of the builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="configuration"/> is <c>null</c>.
        /// </exception>
        ITableBuilder SetDefaultTableConfiguration(TableConfiguration configuration);

        /// <summary>
        ///     Sets the number of rows that are in the table.
        /// </summary>
        /// <param name="numberOfRows">
        ///     The number of rows in the table.
        /// </param>
        /// <returns>
        ///     A table builder that can be used to add columns.
        ///     The returned builder will reflect all other changes
        ///     made to this instance.
        /// </returns>
        ITableBuilderWithRowCount SetRowCount(int numberOfRows);
    }

    /// <summary>
    ///     Provides a means of adding columns to a data table.
    /// </summary>
    public interface ITableBuilderWithRowCount
    {
        /// <summary>
        ///     Gets the count of rows in the table.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        ///     Gets the collection of columns that have been added to
        ///     the builder instance.
        /// </summary>
        IReadOnlyCollection<IDataColumn> Columns { get; }

        /// <summary>
        ///     Adds a column to this builder instance.
        /// </summary>
        /// <param name="column">
        ///     The column to add.
        /// </param>
        /// <returns>
        ///     This instance of the builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="column"/> is <c>null</c>.
        /// </exception>
        ITableBuilderWithRowCount AddColumn(IDataColumn column);

        /// <summary>
        ///     Replaces the given column with another column.
        ///     If the column to replace does not exist, then the
        ///     other column is simply added.
        /// </summary>
        /// <param name="oldColumn">
        ///     The column to replace.
        /// </param>
        /// <param name="newColumn">
        ///     The column to use to replace <paramref name="oldColumn"/>.
        /// </param>
        /// <returns>
        ///     This instance of the builder.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="oldColumn"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="newColumn"/> is <c>null</c>.
        /// </exception>
        ITableBuilderWithRowCount ReplaceColumn(IDataColumn oldColumn, IDataColumn newColumn);

        /// <summary>
        ///     Sets the function to be used to generate details for a given row in the table.
        ///     <para/>
        ///     The function will take the row number being examined by the user, and should
        ///     return a collection of one or more entries given details for that row. This can
        ///     be used to support information on a row by row basis that does not make
        ///     sense in a columnar format.
        ///     <para/>
        ///     This method is not required to be called; it is perfectly acceptable
        ///     to not support details. This method may be called only zero (0) or one (1)
        ///     time(s) per instance.
        /// </summary>
        /// <param name="generator">
        ///     The function that should be used by the application to retrieve the details
        ///     for a given row.
        /// </param>
        /// <returns>
        ///     An <see cref="ITableBuilderWithRowCount"/> that may be used to continue
        ///     building the table.
        /// </returns>
        ITableBuilderWithRowCount SetTableRowDetailsGenerator(Func<int, IEnumerable<TableRowDetailEntry>> generator);
    }
}

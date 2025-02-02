// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.Registrar;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     This class is used to build tables. Tables are built by adding
    ///     columns, each consisting of the same number of rows. Each row in
    ///     the table is made up of projections, one per column, that take the
    ///     integral row number and project the data for the given column as
    ///     the appropriate type.
    ///     <para/>
    ///     Within the SDK, the runtime will pass your processor an
    ///     <see cref="ITableBuilder"/> via the <see cref="ICustomDataProcessor.BuildTable(TableDescriptor, ITableBuilder)"/>
    ///     method, so there is usually no need to interact with this class directly.
    /// </summary>
    public class TableBuilder
        : ITableBuilder,
          ITableBuilderWithRowCount
    {
        private readonly List<IDataColumn> columns;
        private readonly ReadOnlyCollection<IDataColumn> columnsRO;
        private readonly List<TableConfiguration> builtInTableConfigurations;
        private readonly List<TableCommand2> commands2;
        private readonly ColumnVariantsRegistrar variantsRegistrar;

        // Maps a row to a collection of row detail entry
        private Func<int, IEnumerable<TableRowDetailEntry>> tableDetailsGenerator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableBuilder"/>
        ///     class.
        /// </summary>
        public TableBuilder()
        {
            this.columns = new List<IDataColumn>();
            this.builtInTableConfigurations = new List<TableConfiguration>();
            this.columnsRO = new ReadOnlyCollection<IDataColumn>(this.columns);

            this.commands2 = new List<TableCommand2>();
            this.Commands = this.commands2;

            this.variantsRegistrar = new ColumnVariantsRegistrar();

            this.RowCount = 0;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IDataColumn> Columns => this.columnsRO;


        /// <inheritdoc />
        public IEnumerable<TableConfiguration> BuiltInTableConfigurations => this.builtInTableConfigurations.AsReadOnly();

        /// <inheritdoc />
        public TableConfiguration DefaultConfiguration { get; private set; }

        /// <inheritdoc />
        public int RowCount { get; private set; }

        public IReadOnlyList<TableCommand2> Commands { get; }

        /// <summary>
        ///     Gets the <see cref="IColumnVariantsRegistrar"/> that can be used to find
        ///     and discover registered column variants.
        /// </summary>
        public IColumnVariantsRegistrar ColumnVariantsRegistrar => this.variantsRegistrar;

        // TODO:
        // We currently set this to a default builder - build a TableDetails instance from the 
        // <row, func> generators. We might want to expose this in the future, so that table
        // authors can set this builder and be able to define how the table details are
        // structured among rows. Currently they can only define the info within each row.

        /// <inheritdoc />
        public TableDetailsGeneratorCallback TableDetailsBuilder => this.DefaultTableDetailsBuilder;

        public bool HasTableRowDetailsGenerator
        {
            get
            {
                return this.tableDetailsGenerator != null;
            }
        }

        /// <inheritdoc />
        public ITableBuilder AddTableCommand(string name, TableCommandCallback callback)
        {
            Guard.NotNull(callback, nameof(callback));

           return AddTableCommand2(
               name,
               (_) => true,
               (context) => callback(context.SelectedRows));
        }

        /// <inheritdoc />
        public ITableBuilder AddTableCommand2(string commandName, Predicate<TableCommandContext> canExecute, Action<TableCommandContext> onExecute)
        {
            Guard.NotNullOrWhiteSpace(commandName, nameof(commandName));
            Guard.NotNull(canExecute, nameof(canExecute));
            Guard.NotNull(onExecute, nameof(onExecute));

            var canonicalName = commandName.Trim();
            if (this.commands2.Any(x => StringComparer.CurrentCultureIgnoreCase.Equals(x.CommandName, canonicalName)))
            {
                throw new InvalidOperationException($"Duplicate command names are not allowed. Duplicate: {canonicalName}");
            }

            this.commands2.Add(new TableCommand2(canonicalName, canExecute, onExecute));

            return this;
        }

        /// <inheritdoc />
        public ITableBuilder AddTableConfiguration(TableConfiguration configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));

            this.builtInTableConfigurations.Add(configuration);

            return this;
        }

        /// <inheritdoc />
        public ITableBuilder SetDefaultTableConfiguration(TableConfiguration configuration)
        {
            Guard.NotNull(configuration, nameof(configuration));

            this.DefaultConfiguration = configuration;

            return this;
        }

        /// <inheritdoc />
        public ITableBuilderWithRowCount SetRowCount(int numberOfRows)
        {
            Guard.GreaterThan(numberOfRows, -1, nameof(numberOfRows));

            this.RowCount = numberOfRows;

            return this;
        }

        /// <inheritdoc />
        public ITableBuilderWithRowCount SetTableRowDetailsGenerator(Func<int, IEnumerable<TableRowDetailEntry>> generator)
        {
            Guard.NotNull(generator, nameof(generator));

            if (this.tableDetailsGenerator != null)
            {
                throw new InvalidOperationException($"Table details generator has already been set");
            }

            this.tableDetailsGenerator = generator;
            return this;
        }

        public ITableBuilderWithRowCount AddColumn(IDataColumn column)
        {
            return this.AddColumnWithVariants(column, null);
        }

        /// <inheritdoc />
        public ITableBuilderWithRowCount AddColumnWithVariants(
            IDataColumn column,
            Func<RootColumnBuilder, ColumnBuilder> options)
        {
            Guard.NotNull(column, nameof(column));

            this.columns.Add(column);

            if (options != null)
            {
                var processor = new BuiltColumnVariantsRegistrant(column, this.variantsRegistrar);
                var builder = new EmptyColumnBuilder(
                    processor,
                    column);

                options(builder).Commit();
            }

            return this;
        }

        /// <inheritdoc />
        public ITableBuilderWithRowCount ReplaceColumn(
            IDataColumn oldColumn,
            IDataColumn newColumn)
        {
            Guard.NotNull(oldColumn, nameof(oldColumn));
            Guard.NotNull(newColumn, nameof(newColumn));

            this.columns.Remove(oldColumn);
            this.columns.Add(newColumn);

            return this;
        }

        // Provides a default way of building TableDetails from the generators.
        private TableDetails DefaultTableDetailsBuilder(IEnumerable<int> rows)
        {
            Guard.NotNull(rows, nameof(rows));

            if (this.tableDetailsGenerator == null)
            {
                return null;
            }

            var tableRowDetailsCollection = rows.Select(row => new TableRowDetails(row, this.tableDetailsGenerator(row).ToList())).ToList();

            return new TableDetails(tableRowDetailsCollection);
        }
    }
}

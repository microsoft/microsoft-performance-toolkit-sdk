// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime
{
    /// <summary>
    ///     This class is used to build tables. Tables are built by adding
    ///     columns, each consisteing of the same number of rows. Each row in
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
        private readonly List<TableCommand> commands;
        private readonly IReadOnlyList<TableCommand> commandsRO;

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
            this.commands = new List<TableCommand>();
            this.commandsRO = new ReadOnlyCollection<TableCommand>(this.commands);

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

        /// <inheritdoc />
        public IReadOnlyList<TableCommand> Commands => this.commandsRO;

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
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            var canonicalName = name.Trim();
            if (this.commands.Any(x => StringComparer.CurrentCultureIgnoreCase.Equals(x.MenuName, canonicalName)))
            {
                throw new InvalidOperationException($"Duplicate command names are not allowed. Duplicate: {canonicalName}");
            }

            this.commands.Add(new TableCommand(canonicalName, callback));

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
            Action<IRootColumnBuilder> options)
        {
            Guard.NotNull(column, nameof(column));

            this.columns.Add(column);

            if (options != null)
            {
                var processor = new VariantsProcessor(column, this);
                var builder = new EmptyColumnBuilder(
                    processor,
                    column);

                options(builder);
            }

            return this;
        }

        private Dictionary<IDataColumn, IColumnVariantsTreeNode> columnVariants = new Dictionary<IDataColumn, IColumnVariantsTreeNode>();

        public bool TryGetVariantsRoot(IDataColumn column, out IColumnVariantsTreeNode variantsTreeNodes)
        {
            return this.columnVariants.TryGetValue(column, out variantsTreeNodes);
        }

        // TODO: expose the actual datacolumn variants
        // To do this, the variants with projections first need to expose an IDataColumn
        // Then, the below processor needs to use a visitor that extracts the datacolumns
        // associated with every variant identifier. These variants will be stored in a separate
        // dictionary, and exposed via a new method on this class.

        private class VariantsProcessor
            : IColumnVariantsProcessor
        {
            private readonly IDataColumn baseColumn;
            private readonly TableBuilder tableBuilder;

            public VariantsProcessor(IDataColumn baseColumn, TableBuilder tableBuilder)
            {
                this.baseColumn = baseColumn;
                this.tableBuilder = tableBuilder;
            }

            public void ProcessColumnVariants(IColumnVariantsTreeNode variantsTreeNodes)
            {
                tableBuilder.columnVariants[this.baseColumn] = variantsTreeNodes;
            }
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

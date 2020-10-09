// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     The core interface for defining a column. A column is a
    ///     projection of data with metadata.
    /// </summary>
    public interface IDataColumn
    {
        /// <summary>
        ///     Gets the configuration of this column.
        /// </summary>
        ColumnConfiguration Configuration { get; }

        /// <summary>
        ///     Gets the <see cref="Type"/> of data projected by this column.
        /// </summary>
        Type DataType { get; }

        /// <summary>
        ///     Gets the <see cref="Type"/> of the concrete projector that
        ///     projects the data for this column.
        /// </summary>
        Type ProjectorInterface { get; }

        /// <summary>
        ///     Projects the data in this column for the given row.
        /// </summary>
        /// <param name="index">
        ///     The row whose value is to be retrieved.
        /// </param>
        /// <returns>
        ///     The projected value at the given row.
        /// </returns>
        object Project(int index);
    }

    /// <summary>
    ///     A typed column.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of data projected by the
    ///     column.
    /// </typeparam>
    public interface IDataColumn<T>
        : IDataColumn,
          IProjector<int, T>
    {
        /// <summary>
        ///     Gets the projector that projects the data for this column.
        /// </summary>
        IProjection<int, T> Projector { get; }

        /// <summary>
        ///     Projects the data in this column for the given row.
        /// </summary>
        /// <param name="index">
        ///     The row whose value is to be retrieved.
        /// </param>
        /// <returns>
        ///     The projected value at the given row.
        /// </returns>
        T ProjectTyped(int index);
    }

    /// <summary>
    ///     Represents a data column that can display values in a
    ///     hierarchical format. For example, stacks are hierarchical.
    ///     <para/>
    ///     Expanding on the example, a stack is a collection of values
    ///     that should logically be treated as one value. As one expands
    ///     the stack, each call becomes its own row. This column type allows
    ///     for you to provide the means of displaying those expansions in
    ///     a meaningful way.
    /// </summary>
    /// <typeparam name="T">
    ///     The <see cref="Type"/> of data exposed by this column.
    /// </typeparam>
    public interface IHierarchicalDataColumn<T>
        : IDataColumn<T>
    {
        /// <summary>
        ///     Provides the mechanism by which hierarchical data is 
        ///     displayed. In the case of stacks, for example, it would
        ///     define how each value should have an additional indentation
        ///     level the deeper in the stack one goes.
        /// </summary>
        ICollectionInfoProvider<T> CollectionInfoProvider { get; }
    }
}

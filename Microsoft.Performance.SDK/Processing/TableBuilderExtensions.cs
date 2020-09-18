// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Processing
{
    /// <summary>
    ///     Contains static (Shared in Visual Basic) methods for interacting
    ///     with <see cref="ITableBuilder"/> and <see cref="ITableBuilderWithRowCount"/>
    ///     instances.
    /// </summary>
    public static class TableBuilderExtensions
    {
        /// <summary>
        ///     Adds a new column to the builder with the given
        ///     configuration and projection.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being projected.
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="column">
        ///     The column configuration the added column is to have.
        /// </param>
        /// <param name="projection">
        ///     The projection the added column is to have.
        /// </param>
        /// <returns>
        ///     The builder instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="column"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="projection"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static ITableBuilderWithRowCount AddColumn<T>(
            this ITableBuilderWithRowCount self,
            ColumnConfiguration column,
            IProjection<int, T> projection)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(projection, nameof(projection));

            return self.AddColumn(new BaseDataColumn<T>(column, projection));
        }

        /// <summary>
        ///     Adds a new hierarchical column to the builder with the given
        ///     configuration, projection, and info providers.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data being projected.
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="column">
        ///     The column configuration the added column is to have.
        /// </param>
        /// <param name="projection">
        ///     The projection the added column is to have.
        /// </param>
        /// <param name="collectionProvider">
        ///     The collection provider for the column.
        /// </param>
        /// <returns>
        ///     The builder instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="collectionProvider"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="column"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="projection"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static ITableBuilderWithRowCount AddHierarchicalColumn<T>(
            this ITableBuilderWithRowCount self,
            ColumnConfiguration column,
            IProjection<int, T> projection,
            ICollectionInfoProvider<T> collectionProvider)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(projection, nameof(projection));
            Guard.NotNull(collectionProvider, nameof(collectionProvider));

            return self.AddColumn(new HierarchicalDataColumn<T>(column, projection, collectionProvider));
        }

        /// <summary>
        ///     Tries to get the column with the given name from the builder,
        ///     if the builder has a column with the given name.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data projected by the column.
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="columnName">
        ///     The name of the column to retrieve.
        /// </param>
        /// <returns>
        ///     The column, if it exists; <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static IDataColumn<T> GetColumnOrNull<T>(
            this ITableBuilderWithRowCount self,
            string columnName)
        {
            Guard.NotNull(self, nameof(self));

            return self.GetColumnOrNull<T>(x => x.Configuration.Metadata.Name == columnName);
        }

        /// <summary>
        ///     Tries to get the column with the given <see cref="Guid"/> from the builder,
        ///     if the builder has a column with the given <see cref="Guid"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data projected by the column.
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="columnGuid">
        ///     The <see cref="Guid"/> of the column to retrieve.
        /// </param>
        /// <returns>
        ///     The column, if it exists; <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static IDataColumn<T> GetColumnOrNull<T>(
            this ITableBuilderWithRowCount self,
            Guid columnGuid)
        {
            Guard.NotNull(self, nameof(self));

            return self.GetColumnOrNull<T>(x => x.Configuration.Metadata.Guid == columnGuid);
        }

        /// <summary>
        ///     Tries to get the column that satisfies the given predicate from the builder.
        ///     This method will return the first column found that matches the predicate.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data projected by the column.
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="predicate">
        ///     The predicate to use to search for columns.
        /// </param>
        /// <returns>
        ///     The column, if it exists; <c>null</c> otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="self"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public static IDataColumn<T> GetColumnOrNull<T>(
            this ITableBuilderWithRowCount self,
            Func<IDataColumn<T>, bool> predicate)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(predicate, nameof(predicate));

            foreach (var column in self.Columns)
            {
                if (column is IDataColumn<T> castedColumn)
                {
                    if (predicate(castedColumn))
                    {
                        return castedColumn;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Replaces the column in the builder with a new column
        ///     using the given projection. The current columns configuration
        ///     is used on the new column. If the old column does not exist,
        ///     then a new column is simply added.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="Type"/> of data in the column
        /// </typeparam>
        /// <param name="self">
        ///     The builder instance.
        /// </param>
        /// <param name="old">
        ///     The column to replace.
        /// </param>
        /// <param name="newProjection">
        ///     The projection that the replacement column should have.
        /// </param>
        /// <returns>
        ///     The builder instance.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="newProjection"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="old"/> is <c>null</c>.
        ///     - or -
        ///     <paramref name="self"/> is <c>null</c>.
        /// </exception>
        public static ITableBuilderWithRowCount ReplaceColumn<T>(
            this ITableBuilderWithRowCount self,
            IDataColumn old,
            IProjection<int, T> newProjection)
        {
            Guard.NotNull(self, nameof(self));
            Guard.NotNull(old, nameof(old));
            Guard.NotNull(newProjection, nameof(newProjection));

            var newColumn = new BaseDataColumn<T>(old.Configuration, newProjection);
            return self.ReplaceColumn(old, newColumn);
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.Toolkit.Engine
{
    /// <summary>
    ///     Extensions for <see cref="ITableResult"/>.
    /// </summary>
    public static class ITableResultExtensions
    {
        /// <summary>
        ///     Attempts to get the column variants for the given column.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="ITableResult"/> to search.
        /// </param>
        /// <param name="baseColumn">
        ///     The column for which to get the variants.
        /// </param>
        /// <param name="foundColumns">
        ///     The column variants for the given column, if found.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the column variants were found; <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetColumnVariants(
            this ITableResult self,
            IDataColumn baseColumn,
            out IReadOnlyDictionary<ColumnVariantDescriptor, IDataColumn> foundColumns)
        {
            return self.ColumnVariants.TryGetValue(baseColumn, out foundColumns);
        }

        /// <summary>
        ///     Attempts to get the column variant for the given column and variant identifier.
        /// </summary>
        /// <param name="self">
        ///     The <see cref="ITableResult"/> to search.
        /// </param>
        /// <param name="columnGuid">
        ///     The guid of the column for which to get the variant.
        /// </param>
        /// <param name="variantGuid">
        ///     The guid of the variant to get.
        /// </param>
        /// <param name="columnVariant">
        ///     The variant of the given column with the given identifier, if found.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the variant was found; <c>false</c> otherwise.
        /// </returns>
        public static bool TryGetColumnVariant(
            this ITableResult self,
            Guid columnGuid,
            Guid variantGuid,
            out IDataColumn columnVariant)
        {
            IDataColumn column = self.Columns.FirstOrDefault(c => c.Configuration.Metadata.Guid == columnGuid);
            if (column == null)
            {
                columnVariant = null;
                return false;
            }

            if (!self.TryGetColumnVariants(column, out var variants))
            {
                columnVariant = null;
                return false;
            }

            foreach (var kvp in variants)
            {
                if (kvp.Key.Guid == variantGuid)
                {
                    columnVariant = kvp.Value;
                    return true;
                }
            }

            columnVariant = null;
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.Toolkit.Engine
{
    public static class ITableResultExtensions
    {
        public static bool TryGetColumnVariants(
            this ITableResult self,
            IDataColumn baseColumn,
            out IReadOnlyDictionary<ColumnVariantIdentifier, IDataColumn> foundColumns)
        {
            return self.ColumnVariants.TryGetValue(baseColumn, out foundColumns);
        }

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

            return variants
                .ToDictionary(kvp => kvp.Key.Guid, kvp => kvp.Value)
                .TryGetValue(variantGuid, out columnVariant);
        }
    }
}
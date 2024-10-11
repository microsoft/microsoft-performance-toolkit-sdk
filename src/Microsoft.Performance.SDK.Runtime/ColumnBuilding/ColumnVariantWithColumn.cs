using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding
{
    internal class ColumnVariantWithColumn
    {
        public ColumnVariantIdentifier Identifier { get; }
        public IDataColumn Column { get; }

        public ColumnVariantWithColumn(ColumnVariantIdentifier identifier, IDataColumn column)
        {
            Identifier = identifier;
            Column = column;
        }
    }
}
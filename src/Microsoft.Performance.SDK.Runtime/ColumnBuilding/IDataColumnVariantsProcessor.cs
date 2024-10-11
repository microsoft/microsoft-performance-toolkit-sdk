using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding
{
    public interface IDataColumnVariantsProcessor
    {
        void ProcessColumnVariants(IColumnVariant variants);
    }
}
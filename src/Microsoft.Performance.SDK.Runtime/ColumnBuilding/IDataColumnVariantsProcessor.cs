using Microsoft.Performance.SDK.Processing.DataColumns;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding
{
    public interface IDataColumnVariantsProcessor
    {
        void ProcessColumnVariants(IDataColumnVariants variants);
    }
}
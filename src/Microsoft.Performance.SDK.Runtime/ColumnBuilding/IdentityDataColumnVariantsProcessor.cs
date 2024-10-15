using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding;

internal class IdentityDataColumnVariantsProcessor
    : IDataColumnVariantsProcessor
{
    public IdentityDataColumnVariantsProcessor()
    {
        this.Output = NullColumnVariant.Instance;
    }

    public IColumnVariant Output { get; private set; }

    public void ProcessColumnVariants(IColumnVariant variants)
    {
        this.Output = variants;
    }
}
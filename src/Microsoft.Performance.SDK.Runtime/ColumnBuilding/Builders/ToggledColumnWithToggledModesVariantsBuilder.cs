using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

internal sealed class ToggledColumnWithToggledModesVariantsBuilder
    : ToggledColumnVariantsBuilder
{
    private readonly string toggleText;
    private readonly ICallbackInvoker modesCallbackActionInvoker;

    public ToggledColumnWithToggledModesVariantsBuilder(
        IReadOnlyCollection<AddedToggle> toggles,
        IDataColumn baseColumn,
        IDataColumnVariantsProcessor processor,
        ICallbackInvoker modesCallbackActionInvoker,
        string toggleText)
        : base(toggles, baseColumn, processor)
    {
        this.modesCallbackActionInvoker = modesCallbackActionInvoker;
        this.toggleText = toggleText;
    }

    protected override IColumnVariant GetRootVariant()
    {
        if (this.modesCallbackActionInvoker.TryGet(out var builtModesVariant))
        {
            return new ModesToggleColumnVariant(this.toggleText, builtModesVariant);
        }
        else
        {
            return NullColumnVariant.Instance;
        }
    }
}
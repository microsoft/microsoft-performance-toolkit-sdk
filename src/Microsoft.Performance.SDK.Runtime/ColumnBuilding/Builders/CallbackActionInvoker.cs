using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

internal interface ICallbackInvoker
{
    bool TryGet(out IColumnVariant builtVariant);
}

internal sealed class ModeCallbackInvoker
    : ICallbackInvoker
{
    private readonly Action<IToggleableColumnVariantsBuilder> callback;
    private readonly IDataColumn baseColumn;

    public ModeCallbackInvoker(
        Action<IToggleableColumnVariantsBuilder> callback,
        IDataColumn baseColumn)
    {
        this.callback = callback;
        this.baseColumn = baseColumn;
    }

    public bool TryGet(out IColumnVariant builtVariant)
    {
        var processor = new IdentityDataColumnVariantsProcessor();
        var builder = new EmptyColumnVariantsBuilder(processor, this.baseColumn);

        if (callback == null)
        {
            builtVariant = NullColumnVariant.Instance;
            return false;
        }

        this.callback(builder);
        builtVariant = processor.Output;
        return true;
    }
}

internal sealed class NestedModesCallbackInvoker
    : ICallbackInvoker
{
    private readonly Action<IColumnVariantsModesBuilder> callback;
    private readonly IDataColumn baseColumn;

    public NestedModesCallbackInvoker(
        Action<IColumnVariantsModesBuilder> callback,
        IDataColumn baseColumn)
    {
        this.callback = callback;
        this.baseColumn = baseColumn;
    }

    public bool TryGet(out IColumnVariant builtVariant)
    {
        var processor = new IdentityDataColumnVariantsProcessor();
        var builder = new ModesBuilder(
            processor,
            new List<ModesBuilder.AddedMode>(),
            this.baseColumn,
            null);

        if (callback == null)
        {
            builtVariant = NullColumnVariant.Instance;
            return false;
        }

        this.callback(builder);
        builtVariant = processor.Output;
        return true;
    }
}
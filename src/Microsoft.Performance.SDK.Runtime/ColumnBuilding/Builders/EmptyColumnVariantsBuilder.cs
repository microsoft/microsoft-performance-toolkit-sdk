using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

public sealed class EmptyColumnVariantsBuilder
    : IRootColumnVariantsBuilder
{
    private readonly IDataColumn baseColumn;
    private readonly IDataColumnVariantsProcessor processor;

    public EmptyColumnVariantsBuilder(
        IDataColumnVariantsProcessor processor,
        IDataColumn baseColumn)
    {
        this.processor = processor;
        this.baseColumn = baseColumn;
    }

    public void Build()
    {
        // NOOP since we are empty
    }

    public IToggleableColumnVariantsBuilder WithToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> column)
    {
        return ToggledColumnVariantsBuilder.New(
            baseColumn,
            processor,
            toggleIdentifier,
            column);
    }

    public IColumnVariantsBuilder WithToggledModes(
        string toggleText,
        Action<IColumnVariantsModesBuilder> builder)
    {
        return new ToggledColumnWithToggledModesVariantsBuilder(
            new List<ToggledColumnVariantsBuilder.AddedToggle>(),
            baseColumn,
            processor,
            new NestedModesCallbackInvoker(builder, baseColumn),
            toggleText);
    }

    public IColumnVariantsModesBuilder WithModes(
        string baseProjectionModeName)
    {
        return WithModes(baseProjectionModeName, null);
    }

    public IColumnVariantsModesBuilder WithModes(
        string baseProjectionModeName,
        Action<IToggleableColumnVariantsBuilder> builder)
    {
        return new ModesBuilder(
            processor,
            new List<ModesBuilder.AddedMode>()
            {
                new ModesBuilder.AddedMode(
                    new ColumnVariantIdentifier(this.baseColumn.Configuration.Metadata.Guid, baseProjectionModeName),
                    this.baseColumn,
                    builder),
            },
            baseColumn,
            null);
    }
}
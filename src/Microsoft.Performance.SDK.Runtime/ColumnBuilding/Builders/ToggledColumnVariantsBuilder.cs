using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders;

internal class ToggledColumnVariantsBuilder
    : IToggleableColumnVariantsBuilder
{
    protected internal record AddedToggle(
        ColumnVariantIdentifier ToggleIdentifier,
        IDataColumn column);

    private readonly IReadOnlyCollection<AddedToggle> toggles;
    private readonly IDataColumn baseColumn;
    private readonly IDataColumnVariantsProcessor processor;

    protected ToggledColumnVariantsBuilder(
        IReadOnlyCollection<AddedToggle> toggles,
        IDataColumn baseColumn,
        IDataColumnVariantsProcessor processor)
    {
        this.toggles = toggles;
        this.baseColumn = baseColumn;
        this.processor = processor;
    }

    public static ToggledColumnVariantsBuilder New<T>(
        IDataColumn baseColumn,
        IDataColumnVariantsProcessor processor,
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> projection)
    {
        return new ToggledColumnVariantsBuilder(
            new List<AddedToggle>
            {
                new(toggleIdentifier, new DataColumn<T>(baseColumn.Configuration, projection))
            },
            baseColumn,
            processor);
    }

    public void Build()
    {
        this.processor.ProcessColumnVariants(BuildVariant());
    }

    public IToggleableColumnVariantsBuilder WithToggle<T>(
        ColumnVariantIdentifier toggleIdentifier,
        IProjection<int, T> column)
    {
        return new ToggledColumnVariantsBuilder(
            this.toggles.Append(
                new AddedToggle(toggleIdentifier, new DataColumn<T>(this.baseColumn.Configuration, column))
            ).ToList(),
            this.baseColumn,
            this.processor);
    }

    public IColumnVariantsBuilder WithToggledModes(
        string toggleText,
        Action<IColumnVariantsModesBuilder> builder)
    {
        return new ToggledColumnWithToggledModesVariantsBuilder(
            this.toggles,
            this.baseColumn,
            this.processor,
            new NestedModesCallbackInvoker(builder, this.baseColumn),
            toggleText);
    }

    private IColumnVariant BuildVariant()
    {
        IColumnVariant variant = GetRootVariant();

        foreach (var toggle in this.toggles.Reverse())
        {
            variant = new ToggleableColumnVariant(toggle.ToggleIdentifier, toggle.column, variant);
        }

        return variant;
    }

    protected virtual IColumnVariant GetRootVariant()
    {
        return NullColumnVariant.Instance;
    }
}
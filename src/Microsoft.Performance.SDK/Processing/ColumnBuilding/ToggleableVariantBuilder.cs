// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for configuring a toggleable column variant, such as
///     associating <see cref="DataColumnCommands"/> with it.
/// </summary>
public abstract class ToggleableVariantBuilder
{
    private protected ToggleableVariantBuilder()
    {
        // Only internal implementations
    }

    /// <summary>
    ///     Associates the given <see cref="DataColumnCommands"/> with this variant.
    /// </summary>
    /// <param name="commands">
    ///     The commands supported by this variant.
    /// </param>
    /// <returns>
    ///     A <see cref="ToggleableVariantBuilder"/> that has been configured with the
    ///     given commands.
    /// </returns>
    public abstract ToggleableVariantBuilder WithCommands(DataColumnCommands commands);

    /// <summary>
    ///     Creates the <see cref="ToggleableVariant"/> represented by this builder.
    /// </summary>
    /// <param name="baseColumn">
    ///     The base column that the variant is built on top of.
    /// </param>
    /// <returns>
    ///     The <see cref="ToggleableVariant"/> configured by this builder.
    /// </returns>
    internal abstract ToggleableVariant CreateVariant(IDataColumn baseColumn);
}
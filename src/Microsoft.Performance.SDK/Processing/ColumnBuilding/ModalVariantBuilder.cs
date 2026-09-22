// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using Microsoft.Performance.SDK.ColumnCommands;
using System;

namespace Microsoft.Performance.SDK.Processing.ColumnBuilding;

/// <summary>
///     A builder for configuring a single mode (i.e. one of the mutually
///     exclusive variants) of a column that has been configured as modal.
/// </summary>
/// <remarks>
///     An instance of this builder represents one mode that has already been
///     given a <see cref="ColumnVariantDescriptor"/> and a projection. Use the
///     methods on this builder to further configure that mode, for example by
///     attaching the commands it supports or by nesting additional toggleable
///     variants underneath it. The methods return a builder so that calls can
///     be chained together.
/// </remarks>
public abstract class ModalVariantBuilder
{
    private protected ModalVariantBuilder()
    {
        // Only internal implementations
    }

    /// <summary>
    ///     Associates the given <see cref="DataColumnCommands"/> with this mode.
    /// </summary>
    /// <param name="commands">
    ///     The commands supported by this mode.
    /// </param>
    /// <returns>
    ///     A <see cref="ModalVariantBuilder"/> that has been configured with the
    ///     given commands.
    /// </returns>
    public abstract ModalVariantBuilder WithCommands(DataColumnCommands commands);

    /// <summary>
    ///     Nests additional toggleable variants underneath this mode by invoking
    ///     the given callback.
    /// </summary>
    /// <param name="builder">
    ///     A callback that builds toggleable sub-variants of this mode and returns
    ///     its final column configuration.
    /// </param>
    /// <returns>
    ///     A <see cref="ModalVariantBuilder"/> that has been configured with the
    ///     nested toggleable variants produced by <paramref name="builder"/>.
    /// </returns>
    public abstract ModalVariantBuilder WithBuilder(Func<ToggleableColumnBuilder, ColumnBuilder> builder);

    /// <summary>
    ///     Creates the <see cref="ModalVariant"/> represented by this builder for
    ///     the given base column.
    /// </summary>
    /// <param name="baseColumn">
    ///     The base <see cref="IDataColumn"/> that the mode is being built for.
    /// </param>
    /// <returns>
    ///     The <see cref="ModalVariant"/> described by this builder.
    /// </returns>
    internal abstract ModalVariant CreateVariant(IDataColumn baseColumn);
}

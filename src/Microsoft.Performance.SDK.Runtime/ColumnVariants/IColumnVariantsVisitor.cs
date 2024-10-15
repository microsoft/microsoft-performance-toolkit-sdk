// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.ColumnVariants;

/// <summary>
///     Represents a visitor for concrete <see cref="IColumnVariant"/> objects.
/// </summary>
public interface IColumnVariantsVisitor
{
    /// <summary>
    ///     Called when visiting a <see cref="NullColumnVariant"/>.
    /// </summary>
    /// <param name="nullColumnVariant">
    ///     The <see cref="NullColumnVariant"/> being visited.
    /// </param>
    void Visit(NullColumnVariant nullColumnVariant);

    /// <summary>
    ///     Called when visiting a <see cref="ToggleableColumnVariant"/>.
    /// </summary>
    /// <param name="toggleableColumnVariant">
    ///     The <see cref="ToggleableColumnVariant"/> being visited.
    /// </param>
    void Visit(ToggleableColumnVariant toggleableColumnVariant);

    /// <summary>
    ///     Called when visiting a <see cref="ModesToggleColumnVariant"/>.
    /// </summary>
    /// <param name="modesToggle">
    ///     The <see cref="ModesToggleColumnVariant"/> being visited.
    /// </param>
    void Visit(ModesToggleColumnVariant modesToggle);

    /// <summary>
    ///     Called when visiting a <see cref="ModesColumnVariant"/>.
    /// </summary>
    /// <param name="modesColumnVariant">
    ///     The <see cref="ModesColumnVariant"/> being visited.
    /// </param>
    void Visit(ModesColumnVariant modesColumnVariant);

    /// <summary>
    ///     Called when visiting a <see cref="ModeColumnVariant"/>.
    /// </summary>
    /// <param name="modeColumnVariant">
    ///     The <see cref="ModeColumnVariant"/> being visited.
    /// </param>
    void Visit(ModeColumnVariant modeColumnVariant);

}
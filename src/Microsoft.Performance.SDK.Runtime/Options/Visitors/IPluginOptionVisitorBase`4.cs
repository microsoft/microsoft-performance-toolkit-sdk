// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Options.Visitors;

/// <summary>
///     Base interface for a visitor that can visit variants of plugin options.
/// </summary>
/// <typeparam name="TBase">
///     The base type of each variant being visited.
/// </typeparam>
/// <typeparam name="TBool">
///     The type of the boolean variant.
/// </typeparam>
/// <typeparam name="TField">
///     The type of the field variant.
/// </typeparam>
/// <typeparam name="TFieldArray">
///     The type of the field array variant.
/// </typeparam>
public interface IPluginOptionVisitorBase<TBase, in TBool, in TField, in TFieldArray>
    where TBool : TBase
    where TField : TBase
    where TFieldArray : TBase
{
    /// <summary>
    ///     Visits the given <see cref="BooleanOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(TBool option);

    /// <summary>
    ///     Visits the given <see cref="FieldOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(TField option);

    /// <summary>
    ///     Visits the given <see cref="FieldArrayOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(TFieldArray option);
}

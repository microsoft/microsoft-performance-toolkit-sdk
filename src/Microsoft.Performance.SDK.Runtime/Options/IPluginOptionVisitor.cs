// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Represents a class that can visit <see cref="PluginOption"/> instances.
/// </summary>
public interface IPluginOptionVisitor
{
    /// <summary>
    ///     Visits the given <see cref="BooleanOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(BooleanOption option);

    /// <summary>
    ///     Visits the given <see cref="FieldOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(FieldOption option);

    /// <summary>
    ///     Visits the given <see cref="FieldArrayOption"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option to visit.
    /// </param>
    void Visit(FieldArrayOption option);
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;

/// <summary>
///     Responsible for invoking a callback that builds a single mode of a modal column variant.
/// </summary>
internal sealed class ModeBuilderCallbackInvoker
    : IBuilderCallbackInvoker
{
    private readonly Action<IToggleableColumnBuilder> callback;
    private readonly IDataColumn baseColumn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModeBuilderCallbackInvoker"/>
    /// </summary>
    /// <param name="callback">
    ///     The callback that builds the mode.
    /// </param>
    /// <param name="baseColumn">
    ///     The base column that the mode is being built for.
    /// </param>
    public ModeBuilderCallbackInvoker(
        Action<IToggleableColumnBuilder> callback,
        IDataColumn baseColumn)
    {
        this.callback = callback;
        this.baseColumn = baseColumn;
    }

    public bool TryGet(out IColumnVariantsTreeNode builtVariantsTreeNode)
    {
        if (callback == null)
        {
            builtVariantsTreeNode = NullColumnVariantsTreeNode.Instance;
            return false;
        }

        var processor = new BuiltColumnVariantReflector();
        var builder = new EmptyColumnBuilder(processor, baseColumn);

        callback(builder);
        builtVariantsTreeNode = processor.Output;
        return true;
    }
}
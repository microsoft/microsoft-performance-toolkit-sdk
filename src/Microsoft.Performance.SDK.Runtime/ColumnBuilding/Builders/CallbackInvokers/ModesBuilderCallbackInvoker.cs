// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Processing.ColumnBuilding;
using Microsoft.Performance.SDK.Runtime.ColumnBuilding.Processors;
using Microsoft.Performance.SDK.Runtime.ColumnVariants.TreeNodes;

namespace Microsoft.Performance.SDK.Runtime.ColumnBuilding.Builders.CallbackInvokers;

/// <summary>
///     Responsible for invoking a callback that builds modes of a modal column variant.
/// </summary>
internal sealed class ModesBuilderCallbackInvoker
    : IBuilderCallbackInvoker
{
    private readonly Action<IModalColumnBuilder> callback;
    private readonly IDataColumn baseColumn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModesBuilderCallbackInvoker"/>
    /// </summary>
    /// <param name="callback">
    ///     The callback to invoke to build the modes.
    /// </param>
    /// <param name="baseColumn">
    ///     The base <see cref="IDataColumn"/> that the modes are being built upon.
    /// </param>
    public ModesBuilderCallbackInvoker(
        Action<IModalColumnBuilder> callback,
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
        var builder = new ModalColumnBuilder(
            processor,
            new List<ModalColumnBuilder.AddedMode>(),
            this.baseColumn,
            null);

        this.callback(builder);
        builtVariantsTreeNode = processor.Output;
        return true;
    }
}
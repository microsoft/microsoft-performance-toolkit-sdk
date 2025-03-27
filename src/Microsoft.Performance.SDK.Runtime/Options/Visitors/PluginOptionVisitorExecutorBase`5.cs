// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Runtime.Options.Visitors;

/// <summary>
///     Base class for a <see cref="IPluginOptionVisitorBase{TBase,TBool,TField,TFieldArray}"/>
///     executor.
/// </summary>
/// <typeparam name="TVisitor">
///     The type of the visitor.
/// </typeparam>
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
public abstract class PluginOptionVisitorExecutorBase
    <TVisitor, TBase, TBool, TField, TFieldArray>
    where TBool : TBase
    where TField : TBase
    where TFieldArray : TBase
    where TVisitor : IPluginOptionVisitorBase<TBase, TBool, TField, TFieldArray>
{
    private readonly TVisitor visitor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionVisitorExecutorBase{TVisitor,TBase,TBool,TField,TFieldArray}"/>.
    /// </summary>
    /// <param name="visitor">
    ///     The visitor to use.
    /// </param>
    protected PluginOptionVisitorExecutorBase(TVisitor visitor)
    {
        this.visitor = visitor;
    }

    /// <summary>
    ///     Visits the given <see cref="PluginOption"/>s with the <see cref="IPluginOptionVisitor"/> provided to this instance.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="PluginOption"/>s to visit.
    /// </param>
    public void Visit(IEnumerable<TBase> options)
    {
        foreach (var option in options)
        {
            Visit(option);
        }
    }

    /// <summary>
    ///     Visits the given <see cref="PluginOption"/>s with the <see cref="IPluginOptionVisitor"/> provided to this instance.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="PluginOption"/>s to visit.
    /// </param>
    public void Visit(params TBase[] options)
    {
        Visit((IEnumerable<TBase>)options);
    }

    private void Visit(TBase option)
    {
        switch (option)
        {
            case TBool booleanOption:
                this.visitor.Visit(booleanOption);
                break;
            case TField fieldOption:
                this.visitor.Visit(fieldOption);
                break;
            case TFieldArray fieldArrayOption:
                this.visitor.Visit(fieldArrayOption);
                break;
        }
    }
}

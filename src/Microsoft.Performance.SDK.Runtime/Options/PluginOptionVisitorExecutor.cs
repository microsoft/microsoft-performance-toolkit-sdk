// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     This class is used to visit a collection of <see cref="PluginOption"/>s with a given <see cref="IPluginOptionVisitor"/>.
/// </summary>
public sealed class PluginOptionVisitorExecutor
{
    private readonly IPluginOptionVisitor visitor;

    /// <summary>
    ///     Creates a new instance of <see cref="PluginOptionVisitorExecutor"/> for the given <see cref="IPluginOptionVisitor"/>.
    /// </summary>
    /// <param name="visitor">
    ///     The <see cref="IPluginOptionVisitor"/> to use to visit each concrete option.
    /// </param>
    public PluginOptionVisitorExecutor(IPluginOptionVisitor visitor)
    {
        this.visitor = visitor;
    }

    /// <summary>
    ///     Visits the given <see cref="PluginOption"/>s with the <see cref="IPluginOptionVisitor"/> provided to this instance.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="PluginOption"/>s to visit.
    /// </param>
    public void Visit(IEnumerable<PluginOption> options)
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
    public void Visit(params PluginOption[] options)
    {
        Visit((IEnumerable<PluginOption>)options);
    }

    private void Visit(PluginOption option)
    {
        switch (option)
        {
            case BooleanOption booleanOption:
                this.visitor.Visit(booleanOption);
                break;
            case FieldOption fieldOption:
                this.visitor.Visit(fieldOption);
                break;
            case FieldArrayOption fieldArrayOption:
                this.visitor.Visit(fieldArrayOption);
                break;
        }
    }
}
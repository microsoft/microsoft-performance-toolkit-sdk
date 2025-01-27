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

    public PluginOptionVisitorExecutor(IPluginOptionVisitor visitor)
    {
        this.visitor = visitor;
    }

    public void Visit(IEnumerable<PluginOption> options)
    {
        foreach (var option in options)
        {
            Visit(option);
        }
    }

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
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     This class is used to visit <see cref="PluginOptionDefinition"/>s with a given <see cref="IPluginOptionDefinitionVisitor"/>.
/// </summary>
public sealed class PluginOptionDefinitionVisitorExecutor
{
    private readonly IPluginOptionDefinitionVisitor visitor;

    /// <summary>
    ///     Creates a new instance of <see cref="PluginOptionDefinitionVisitorExecutor"/> for the given <see cref="IPluginOptionVisitor"/>.
    /// </summary>
    /// <param name="visitor">
    ///     The <see cref="IPluginOptionDefinitionVisitor"/> to use to visit each concrete option.
    /// </param>
    public PluginOptionDefinitionVisitorExecutor(IPluginOptionDefinitionVisitor visitor)
    {
        this.visitor = visitor;
    }

    public void Visit(PluginOptionDefinition option)
    {
        switch (option)
        {
            case BooleanOptionDefinition booleanOption:
                this.visitor.Visit(booleanOption);
                break;
            case FieldOptionDefinition fieldOption:
                this.visitor.Visit(fieldOption);
                break;
            case FieldArrayOptionDefinition fieldArrayOption:
                this.visitor.Visit(fieldArrayOption);
                break;
        }
    }
}

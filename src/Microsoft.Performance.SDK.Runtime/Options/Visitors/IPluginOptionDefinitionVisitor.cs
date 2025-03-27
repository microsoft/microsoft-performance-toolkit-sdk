// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Performance.SDK.Options.Definitions;

namespace Microsoft.Performance.SDK.Runtime.Options.Visitors;

/// <summary>
///     Represents a class that can visit <see cref="PluginOptionDefinition"/> instances.
/// </summary>
public interface IPluginOptionDefinitionVisitor
    : IPluginOptionVisitorBase<PluginOptionDefinition, BooleanOptionDefinition, FieldOptionDefinition, FieldArrayOptionDefinition>
{
    /// <summary>
    ///     A <see cref="PluginOptionVisitorExecutorBase{TVisitor,TBase,TBool,TField,TFieldArray}"/>
    ///     for <see cref="IPluginOptionDefinitionVisitor"/> instances.
    /// </summary>
    public sealed class Executor
        : PluginOptionVisitorExecutorBase<
            IPluginOptionDefinitionVisitor,
            PluginOptionDefinition,
            BooleanOptionDefinition,
            FieldOptionDefinition,
            FieldArrayOptionDefinition>
    {
        public Executor(IPluginOptionDefinitionVisitor visitor) : base(visitor)
        {
        }
    }
}

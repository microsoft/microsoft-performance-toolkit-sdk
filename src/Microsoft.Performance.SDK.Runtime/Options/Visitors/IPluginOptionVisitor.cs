// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Runtime.Options.Visitors;

/// <summary>
///     Represents a class that can visit <see cref="PluginOption"/> instances.
/// </summary>
public interface IPluginOptionVisitor
    : IPluginOptionVisitorBase<PluginOption, BooleanOption, FieldOption, FieldArrayOption>
{
    /// <summary>
    ///     A <see cref="PluginOptionVisitorExecutorBase{TVisitor,TBase,TBool,TField,TFieldArray}"/>
    ///     for <see cref="IPluginOptionVisitor"/> instances.
    /// </summary>
    public sealed class Executor
        : PluginOptionVisitorExecutorBase<
            IPluginOptionVisitor,
            PluginOption,
            BooleanOption,
            FieldOption,
            FieldArrayOption>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Executor"/> class.
        /// </summary>
        /// <param name="visitor">
        ///     The <see cref="IPluginOptionVisitor"/> instance to use.
        /// </param>
        public Executor(IPluginOptionVisitor visitor)
            : base(visitor)
        {
        }
    }
}

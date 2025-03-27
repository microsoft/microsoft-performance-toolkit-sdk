// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Base class for plugin options.
/// </summary>
public abstract class PluginOption
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOption"/> class.
    /// </summary>
    private protected PluginOption()
    {
    }

    /// <summary>
    ///     Gets the <see cref="Guid"/> of the option.
    /// </summary>
    public abstract Guid Guid { get; }

    /// <summary>
    ///     Gets a value indicating whether this option is using its default value. This value will be <c>true</c>
    ///     if <see cref="PluginOption{T}.SetValue"/> is ever called, regardless of which
    ///     value it was set to. This means that this value will be <c>false</c> even if the option's value is
    ///     manually set to its default value.
    /// </summary>
    public bool IsUsingDefault { get; private protected set; }

    /// <summary>
    ///     Applies the default value to this option.
    /// </summary>
    public abstract void ApplyDefault();
}

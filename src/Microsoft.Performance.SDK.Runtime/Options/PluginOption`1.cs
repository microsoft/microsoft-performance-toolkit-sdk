// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Performance.SDK.Options.Definitions;
using Microsoft.Performance.SDK.Options.Values;

namespace Microsoft.Performance.SDK.Runtime.Options;

public abstract class PluginOption<T>
    : PluginOption
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOption{T}"/> class.
    /// </summary>
    private protected PluginOption()
    {
    }

    /// <summary>
    ///     Sets the current value of the option.
    /// </summary>
    /// <param name="value">
    ///     The value to set.
    /// </param>
    public abstract void SetValue(T value);
}

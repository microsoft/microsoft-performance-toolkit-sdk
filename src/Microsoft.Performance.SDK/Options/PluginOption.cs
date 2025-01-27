// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Options;

public abstract class PluginOption
    : ICloneable<PluginOption>
{
    public event EventHandler OptionChanged;

    internal PluginOption()
    {
    }

    public required Guid Guid { get; init; }

    public required string Category { get; init; }

    public required string Name { get; init;  }

    public required string Description { get; init; }

    internal bool IsUsingDefault { get; private protected set; }

    internal abstract void ApplyDefault();

    protected void RaiseOptionChanged()
    {
        this.OptionChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract PluginOption CloneT();

    public object Clone()
    {
        return CloneT();
    }
}
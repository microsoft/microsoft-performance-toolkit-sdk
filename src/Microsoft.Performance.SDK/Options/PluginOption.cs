using System;

namespace Microsoft.Performance.SDK.Options;

public abstract class PluginOption
    : ICloneable<PluginOption>
{
    public event EventHandler OptionChanged;

    internal event EventHandler OnChangedFromDefault;

    internal event EventHandler OnChangedToDefault;

    internal PluginOption()
    {
    }

    public required Guid Guid { get; init; }

    public required string Category { get; init; }

    public required string Name { get; init;  }

    public required string Description { get; init; }

    internal abstract void ApplyDefault();

    protected void RaiseOptionChanged()
    {
        this.OptionChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void RaiseOnChangedFromDefault()
    {
        this.OnChangedFromDefault?.Invoke(this, EventArgs.Empty);
    }

    protected void RaiseOnChangedToDefault()
    {
        this.OnChangedToDefault?.Invoke(this, EventArgs.Empty);
    }

    public abstract PluginOption CloneT();

    public object Clone()
    {
        return CloneT();
    }
}
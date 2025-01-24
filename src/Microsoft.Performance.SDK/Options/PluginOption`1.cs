using System;

namespace Microsoft.Performance.SDK.Options;

public abstract class PluginOption<T>
    : PluginOption
{
    private readonly T defaultValue;

    private T currentValue;

    internal PluginOption()
    {
    }

    public required T DefaultValue
    {
        get
        {
            return this.defaultValue;
        }
        init
        {
            this.defaultValue = value;
            this.currentValue = value;
        }
    }

    public T CurrentValue
    {
        get
        {
            return this.currentValue;
        }
        internal set
        {
            if (!this.currentValue.Equals(value))
            {
                this.currentValue = value;
                RaiseOptionChanged();
            }
        }
    }
}
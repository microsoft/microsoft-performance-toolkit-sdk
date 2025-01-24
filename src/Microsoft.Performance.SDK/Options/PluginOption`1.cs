using System;

namespace Microsoft.Performance.SDK.Options;

public abstract class PluginOption<T>
    : PluginOption
{
    private readonly T defaultValue;

    private bool isApplyingDefault;

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
            this.currentValue = value;
            RaiseOptionChanged();

            if (!isApplyingDefault)
            {
                RaiseOnChangedFromDefault();
            }
        }
    }

    internal override void ApplyDefault()
    {
        this.isApplyingDefault = true;
        this.CurrentValue = this.DefaultValue;
        RaiseOnChangedToDefault();
        this.isApplyingDefault = false;
    }
}
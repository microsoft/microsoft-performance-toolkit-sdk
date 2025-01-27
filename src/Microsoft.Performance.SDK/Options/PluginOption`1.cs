// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Options;

public abstract class PluginOption<T>
    : PluginOption
{
    private readonly T defaultValue;

    private bool isApplyingDefault;

    private T currentValue;

    internal PluginOption()
    {
        this.IsUsingDefault = true;
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
            this.IsUsingDefault = isApplyingDefault;

            this.currentValue = value;
            RaiseOptionChanged();
        }
    }

    internal override void ApplyDefault()
    {
        this.isApplyingDefault = true;
        this.CurrentValue = this.DefaultValue;
        this.isApplyingDefault = false;
    }
}
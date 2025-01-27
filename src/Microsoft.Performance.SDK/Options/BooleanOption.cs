// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Options;

public sealed class BooleanOption
    : PluginOption<bool>
{
    public override PluginOption CloneT()
    {
        return new BooleanOption
        {
            Guid = this.Guid,
            Category = this.Category,
            Name = this.Name,
            Description = this.Description,
            DefaultValue = this.DefaultValue,
            CurrentValue = this.CurrentValue,
            IsUsingDefault = this.IsUsingDefault,
        };
    }
}
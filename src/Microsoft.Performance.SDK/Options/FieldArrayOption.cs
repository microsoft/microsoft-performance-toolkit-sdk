// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Performance.SDK.Options;

public sealed class FieldArrayOption
    : PluginOption<IReadOnlyList<string>>
{
    public override PluginOption CloneT()
    {
        return new FieldArrayOption()
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
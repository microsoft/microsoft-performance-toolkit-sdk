// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Options;

/// <summary>
///     A <see cref="PluginOption{T}"/> for a single string field.
/// </summary>
public sealed class FieldOption
    : PluginOption<string>
{
    /// <inheritdoc />
    public override PluginOption CloneT()
    {
        return new FieldOption()
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

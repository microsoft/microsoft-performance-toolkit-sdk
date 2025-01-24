namespace Microsoft.Performance.SDK.Options;

public sealed class FieldOption
    : PluginOption<string>
{
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
        };
    }
}
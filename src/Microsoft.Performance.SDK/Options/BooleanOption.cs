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
            RequiresRestart = this.RequiresRestart,
            DefaultValue = this.DefaultValue,
            CurrentValue = this.CurrentValue,
        };
    }
}
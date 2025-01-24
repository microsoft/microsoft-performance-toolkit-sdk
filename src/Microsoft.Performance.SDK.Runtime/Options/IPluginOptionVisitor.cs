using System.Collections.Generic;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

public interface IPluginOptionVisitor
{
    void Visit(BooleanOption option);

    void Visit(FieldOption option);

    void Visit(FieldArrayOption option);
}

public interface IPluginOptionsLoader
{
    void Load();
}

public interface IPluginOptionsSystem
{
    IPluginOptionsLoader Loader { get; }

    IPluginOptionsSaver Saver { get; }
}

public sealed class PluginOptionVisitorExecutor
{
    private readonly IPluginOptionVisitor visitor;

    public PluginOptionVisitorExecutor(IPluginOptionVisitor visitor)
    {
        this.visitor = visitor;
    }

    public void Visit(IEnumerable<PluginOption> options)
    {
        foreach (var option in options)
        {
            Visit(option);
        }
    }

    public void Visit(PluginOption option)
    {
        switch (option)
        {
            case BooleanOption booleanOption:
                this.visitor.Visit(booleanOption);
                break;
            case FieldOption fieldOption:
                this.visitor.Visit(fieldOption);
                break;
            case FieldArrayOption fieldArrayOption:
                this.visitor.Visit(fieldArrayOption);
                break;
        }
    }
}
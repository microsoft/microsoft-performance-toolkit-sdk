using Microsoft.Performance.SDK.Options.Definitions;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Represents a class that can visit <see cref="PluginOptionDefinition"/> instances.
/// </summary>
public interface IPluginOptionDefinitionVisitor
{
    /// <summary>
    ///     Visits the given <see cref="BooleanOptionDefinition"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option definition to visit.
    /// </param>
    void Visit(BooleanOptionDefinition option);

    /// <summary>
    ///     Visits the given <see cref="FieldOptionDefinition"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option definition to visit.
    /// </param>
    void Visit(FieldOptionDefinition option);

    /// <summary>
    ///     Visits the given <see cref="FieldArrayOptionDefinition"/> instance.
    /// </summary>
    /// <param name="option">
    ///     The option definition to visit.
    /// </param>
    void Visit(FieldArrayOptionDefinition option);
}

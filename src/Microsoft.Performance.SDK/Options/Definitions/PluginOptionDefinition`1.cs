// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK.Options.Definitions;

/// <summary>
///     Base class for plugin option definitions.
/// </summary>
/// <typeparam name="T">
///     The type of the option value.
/// </typeparam>
public abstract class PluginOptionDefinition<T>
    : PluginOptionDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionDefinition{T}"/> class.
    /// </summary>
    private protected PluginOptionDefinition()
    {
    }

    /// <summary>
    ///     Gets or initializes the default value of the option. This value is required to be initialized.
    /// </summary>
    public required T DefaultValue
    {
        get;
        init;
    }
}

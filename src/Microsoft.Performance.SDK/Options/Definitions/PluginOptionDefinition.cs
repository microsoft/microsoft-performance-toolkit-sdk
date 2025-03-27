// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Options.Definitions;

/// <summary>
///     Base class for plugin option definitions.
/// </summary>
public abstract class PluginOptionDefinition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionDefinition"/> class.
    /// </summary>
    private protected  PluginOptionDefinition()
    {
    }

    /// <summary>
    ///     Gets or initializes the <see cref="Guid"/> of the option. This value is required to be
    ///     initialized.
    /// </summary>
    public required Guid Guid { get; init; }

    /// <summary>
    ///     Gets or initializes the human-readable category of the option. This value is required to be initialized.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    ///     Gets or initializes the human-readable name of the option. This value is required to be initialized.
    /// </summary>
    public required string Name { get; init;  }

    /// <summary>
    ///     Gets or initializes the human-readable description of the option. This value is required to be initialized.
    /// </summary>
    public required string Description { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.Name} ({this.Guid})";
    }
}

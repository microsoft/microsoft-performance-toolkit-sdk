# Abstract

This documents outlines how to define and consume plugin options within a plugin.

# Motiviation
A plugin may want to obtain information from a user that can be used during processing. For example, a plugin may want a user to define what server endpoints should be used when fetching information from the internet. Prior to SDK version `1.4`, there was not an API available for plugins to obtain this information.

SDK version `1.4` adds support for plugins to define "plugin options" that SDK drivers can persist between sessions. An API was created to allow plugins to fetch the value of any defined plugin option and receive updates whenever their values change.

# How to Define Plugin Options
Plugin options are defined by overriding the virtual `IEnumerable<PluginOptionDefinition> PluginOptions` property on `ProcessingSource`. By default, this property returns an empty collection.

This property should return concrete `PluginOptionDefinition` instances that each represent an option that is available. All valid concrete `PluginOptionDefinition` types are defined in the `Microsoft.Performance.SDK.Options` namespace; it is not possible to define "custom" plugin option types.

An example implementation may look like:

```cs
public override IEnumerable<PluginOptionDefinition> PluginOptions
{
    get
    {
        yield return new BooleanOptionDefinition()
        {
            Guid = Guid.Parse(...),
            Name = "Boolean Option",
            Category = "My Plugin Options",
            Description = "...",
            DefaultValue = false,
        };

        yield return new FieldOptionDefinition()
        {
            Guid = Guid.Parse(...),
            Name = "Field Option",
            Category = "My Plugin Options",
            Description = "...",
            DefaultValue = "data",
        };

        . . .
    }
}
```

Valid `PluginOptionDefinition` types are:
* `BooleanOptionDefinition`, which defines a `bool` option.
* `FieldOptionDefinition`, which defines a `string` option.
* `FieldArrayOptionDefinition`, which defines a `IReadOnlyList<string>` option.

# How to Use Plugin Option Values
The `PluginOptionDefinition` mentioned above do not expose the option values configured by the user. This information is exposed via `PluginOptionValue` instances, which are obtained by calling `IApplicationEnvironment.TryGetPluginOption`. This method has the following signature:

```cs
bool TryGetPluginOption<T>(Guid optionGuid, out T option) where T : PluginOptionValue;
```

A plugin must specify both
1. The concrete `PluginOptionValue` type that the plugin option is expected to have
2. The value of the `Guid` property on the `PluginOptionDefinition` that corresponds to the value being requested.

If a plugin option value with the specified type and `Guid` is found, this method returns `true` and provides the found `PluginOptionValue` as an `out` parameter.

Valid `PluginOptionValue` types are:
* `BooleanOptionValue`, wwhich correspond to a value for an option defined with a `BooleanOptionDefinition`.
* `FieldOptionValue`, which correspond to a value for an option defined with a `FieldOptionDefinition`.
* `FieldArrayOptionValue`, which correspond to a value for an option defined with a `FieldArrayOptionDefinition`.

Each concrete `PluginOptionValue` exposes a `CurrentValue` property of the appropritate type (e.g. `bool` for `BooleanOptionValue`, etc.). Additionally, each `PluginOptionValue` contains an `OptionChanged` event that is invoked whenever the value of `CurrentValue` changes.

An example of using this method may look like:

```cs
if (this.applicationEnvironment.TryGetPluginOption(
        MyPluginOptionIds.MyBooleanOptionId,
        out BooleanOptionValue booleanOption))
{
    bool currentValue = booleanOption.CurrentValue;
    booleanOption.OptionChanged += OnMyBooleanChanged;
}
```

# Setting Option Values
Option values can only be set via the SDK runtime, which is only accessible to SDK drivers such as Windows Performance Analyzer. Each SDK driver may implement setting option values differently.

To configure option values when using the SDK Engine, please refer to the "Defining Plugin Option Values" section of the [Using the Engine](../Using-the-engine.md#defining-plugin-option-values) documentation.
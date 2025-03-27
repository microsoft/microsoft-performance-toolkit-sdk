// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Options.Values;

/// <summary>
///     Base class for plugin option values.
/// </summary>
public abstract class PluginOptionValue
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginOptionValue"/> class.
    /// </summary>
    private protected PluginOptionValue()
    {
    }

    /// <summary>
    ///     Raised when this option's value changes.
    /// </summary>
    public event EventHandler OptionChanged;

    private protected void InvokeOptionChanged()
    {
        try
        {
            this.OptionChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // We catch all exceptions here because we don't want to crash the app just because
            // a plugin is throwing exceptions. The SDK's state is not impacted by an exception here,
            // since all that matters (the plugin's option value has been updated to the new value)
            // isn't affected. A badly designed plugin may not in an unknown state here, but that's not
            // the SDK's responsibility to address.

            return;
        }
    }
}

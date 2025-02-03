// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Runtime.Options;

namespace Microsoft.Performance.SDK.Runtime.Tests.Options;

/// <summary>
///     A test implementation of <see cref="PluginOptionsRegistry.Provider"/> that returns un-cloned
///     options provided by tests.
/// </summary>
public sealed class TestPluginOptionsRegistryProvider
    : PluginOptionsRegistry.Provider
{
    private readonly List<PluginOption> options;

    public TestPluginOptionsRegistryProvider(params PluginOption[] options)
    {
        this.options = new List<PluginOption>(options);
    }

    public IEnumerable<PluginOption> GetOptions()
    {
        return this.options;
    }
}

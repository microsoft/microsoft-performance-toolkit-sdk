// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     Represents a class that can save a <see cref="PluginOptionsDto"/> instance.
/// </summary>
public interface IPluginOptionsSaver
{
    Task<bool> TrySave(PluginOptionsDto optionsDto);
}
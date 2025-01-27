// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     Base class for saving a <see cref="PluginOptionDto"/> instance to a stream.
/// </summary>
public abstract class StreamPluginOptionsSaver
    : IPluginOptionsSaver
{
    private readonly bool closeStreamOnWrite;

    protected StreamPluginOptionsSaver(bool closeStreamOnWrite)
    {
        this.closeStreamOnWrite = closeStreamOnWrite;
    }

    public async Task<bool> TrySave(PluginOptionsDto optionsDto)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        var stream = GetStream();
        try
        {
            await JsonSerializer.SerializeAsync(stream, optionsDto, jsonSerializerOptions);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (this.closeStreamOnWrite)
            {
                stream.Dispose();
            }
        }
    }

    protected abstract Stream GetStream();
}
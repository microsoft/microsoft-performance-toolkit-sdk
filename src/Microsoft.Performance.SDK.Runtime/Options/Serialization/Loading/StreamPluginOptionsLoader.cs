// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     Base class for loading a <see cref="PluginOptionDto"/> instance from a stream.
/// </summary>
public abstract class StreamPluginOptionsLoader
    : IPluginOptionsLoader
{
    private readonly bool closeStreamOnWrite;

    protected StreamPluginOptionsLoader(bool closeStreamOnWrite)
    {
        this.closeStreamOnWrite = closeStreamOnWrite;
    }

    public async Task<PluginOptionsDto> TryLoadAsync()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        var stream = GetStream();
        try
        {
            return await JsonSerializer.DeserializeAsync<PluginOptionsDto>(stream, jsonSerializerOptions);
        }
        catch
        {
            return null;
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
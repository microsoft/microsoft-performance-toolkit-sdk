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

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamPluginOptionsSaver"/> class.
    /// </summary>
    /// <param name="closeStreamOnWrite">
    ///     Whether to dispose of the stream returned by <see cref="GetStream"/> at the end of a call to
    ///     <see cref="TrySaveAsync"/>.
    /// </param>
    protected StreamPluginOptionsSaver(bool closeStreamOnWrite)
    {
        this.closeStreamOnWrite = closeStreamOnWrite;
    }

    /// <inheritdoc />
    public async Task<bool> TrySaveAsync(PluginOptionsDto optionsDto)
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

    /// <summary>
    ///     Gets the <see cref="Stream"/> to save to.
    /// </summary>
    /// <returns>
    ///     The <see cref="Stream"/> to save to.
    /// </returns>
    protected abstract Stream GetStream();
}
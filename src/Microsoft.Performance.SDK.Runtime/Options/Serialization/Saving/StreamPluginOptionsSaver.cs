// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     Base class for saving a <see cref="PluginOptionDto"/> instance to a stream.
/// </summary>
public abstract class StreamPluginOptionsSaver
    : IPluginOptionsSaver
{
    private readonly bool closeStreamOnWrite;
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamPluginOptionsSaver"/> class.
    /// </summary>
    /// <param name="closeStreamOnWrite">
    ///     Whether to dispose of the stream returned by <see cref="GetStream"/> at the end of a call to
    ///     <see cref="TrySaveAsync"/>.
    /// </param>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    protected StreamPluginOptionsSaver(bool closeStreamOnWrite, ILogger logger)
    {
        this.closeStreamOnWrite = closeStreamOnWrite;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> TrySaveAsync(PluginOptionsDto optionsDto)
    {
        var jsonSerializerOptions = PluginOptionsDtoJsonSerialization.GetJsonSerializerOptions();

        var stream = GetStream();
        try
        {
            await JsonSerializer.SerializeAsync(stream, optionsDto, jsonSerializerOptions);
            return true;
        }
        catch (Exception e)
        {
            this.logger?.Error(e, GetSerializeErrorMessage(e));
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

    private protected virtual string GetSerializeErrorMessage(Exception exception)
    {
        return $"Failed to save plugin options to stream: {exception.Message}.";
    }

    /// <summary>
    ///     Gets the <see cref="Stream"/> to save to.
    /// </summary>
    /// <returns>
    ///     The <see cref="Stream"/> to save to.
    /// </returns>
    protected abstract Stream GetStream();
}

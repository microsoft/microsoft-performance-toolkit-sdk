// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     Base class for <see cref="IPluginOptionsLoader"/> classes which load from a stream.
/// </summary>
public abstract class StreamPluginOptionsLoader
    : IPluginOptionsLoader
{
    private readonly bool closeStreamOnRead;
    private readonly ILogger logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamPluginOptionsLoader"/> class.
    /// </summary>
    /// <param name="closeStreamOnRead">
    ///     Whether to dispose of the stream returned by <see cref="GetStream"/> at the end of a call to
    ///     <see cref="TryLoadAsync"/>.
    /// </param>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    protected StreamPluginOptionsLoader(
        bool closeStreamOnRead,
        ILogger logger)
    {
        this.closeStreamOnRead = closeStreamOnRead;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<PluginOptionsDto> TryLoadAsync()
    {
        var jsonSerializerOptions = PluginOptionsDtoJsonSerialization.GetJsonSerializerOptions();

        var stream = GetStream();
        try
        {
            return await JsonSerializer.DeserializeAsync<PluginOptionsDto>(stream, jsonSerializerOptions);
        }
        catch (Exception e)
        {
            this.logger?.Error(e, GetDeserializeErrorMessage(e));
            return null;
        }
        finally
        {
            if (this.closeStreamOnRead)
            {
                stream.Dispose();
            }
        }
    }

    private protected virtual string GetDeserializeErrorMessage(Exception exception)
    {
        return $"Failed to load plugin options from stream: {exception.Message}.";
    }

    /// <summary>
    ///     Gets the <see cref="Stream"/> from which to load the <see cref="PluginOptionsDto"/>.
    /// </summary>
    /// <returns>
    ///     The <see cref="Stream"/> from which to load the <see cref="PluginOptionsDto"/>.
    /// </returns>
    protected abstract Stream GetStream();
}

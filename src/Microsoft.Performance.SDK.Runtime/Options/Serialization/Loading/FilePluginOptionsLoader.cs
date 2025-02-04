// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     A <see cref="IPluginOptionsLoader"/> that can load a <see cref="PluginOptionsDto"/> instance from a file.
/// </summary>
public sealed class FilePluginOptionsLoader
    : StreamPluginOptionsLoader
{
    private readonly string filePath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilePluginOptionsLoader"/> class.
    /// </summary>
    /// <param name="filePath">
    ///     The path to the file from which to load options.
    /// </param>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    public FilePluginOptionsLoader(string filePath, ILogger logger)
        : base(true, logger)
    {
        this.filePath = filePath;
    }

    private protected override string GetDeserializeErrorMessage(Exception exception)
    {
        return $"Failed to load plugin options from '{this.filePath}': {exception.Message}.";
    }

    /// <inheritdoc />
    protected override Stream GetStream()
    {
        return File.Open(this.filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}

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
    : StreamPluginOptionsLoader<FileStream>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilePluginOptionsLoader"/> class.
    /// </summary>
    /// <param name="filePath">
    ///     The path to the file from which to load options.
    /// </param>
    /// <param name="logger">
    ///     The logger to use.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filePath"/> is or <paramref name="logger"/> <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="filePath"/> is empty or composed only of whitespace.
    /// </exception>
    public FilePluginOptionsLoader(string filePath, ILogger logger)
        : base(true, logger)
    {
        Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));
        Guard.NotNull(logger, nameof(logger));

        this.FilePath = filePath;
    }

    /// <summary>
    ///     Gets the path to the file from which to load options.
    /// </summary>
    public string FilePath { get; }

    private protected override string GetDeserializeErrorMessage(Exception exception)
    {
        return $"Failed to load plugin options from '{this.FilePath}': {exception.Message}.";
    }

    /// <inheritdoc />
    protected override FileStream GetStream()
    {
        return File.Open(this.FilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
    }

    /// <inheritdoc />
    protected override bool HasContent(FileStream stream)
    {
        return stream.Length > 0;
    }
}

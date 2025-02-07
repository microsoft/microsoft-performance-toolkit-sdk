// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     A <see cref="IPluginOptionsSaver"/> that can save a <see cref="PluginOptionsDto"/> instance to a file.
/// </summary>
public sealed class FilePluginOptionsSaver
    : StreamPluginOptionsSaver
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FilePluginOptionsSaver"/> class.
    /// </summary>
    /// <param name="filePath">
    ///     The path to the file to which options will be saved.
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
    public FilePluginOptionsSaver(string filePath, ILogger logger)
        : base(true, logger)
    {
        Guard.NotNullOrWhiteSpace(filePath, nameof(filePath));
        Guard.NotNull(logger, nameof(logger));

        this.FilePath = filePath;
    }

    /// <summary>
    ///     Gets the path to the file to which options will be saved.
    /// </summary>
    public string FilePath { get; }

    private protected override string GetSerializeErrorMessage(Exception exception)
    {
        return $"Failed to save plugin options to '{this.FilePath}': {exception.Message}.";
    }

    /// <inheritdoc />
    protected override Stream GetStream()
    {
        return File.Open(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}

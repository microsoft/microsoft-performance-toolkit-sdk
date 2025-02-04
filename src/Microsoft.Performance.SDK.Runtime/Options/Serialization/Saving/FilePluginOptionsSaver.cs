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
    private readonly string filePath;

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
        Guard.NotNull(filePath, nameof(filePath));
        Guard.NotNull(logger, nameof(logger));
        
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"The file path cannot be empty or whitespace.", nameof(filePath));
        }

        this.filePath = filePath;
    }

    private protected override string GetSerializeErrorMessage(Exception exception)
    {
        return $"Failed to save plugin options to '{this.filePath}': {exception.Message}.";
    }

    /// <inheritdoc />
    protected override Stream GetStream()
    {
        return File.Open(this.filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}

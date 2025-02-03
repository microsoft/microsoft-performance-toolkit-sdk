// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    public FilePluginOptionsSaver(string filePath, ILogger logger)
        : base(true, logger)
    {
        this.filePath = filePath;
    }

    /// <inheritdoc />
    protected override Stream GetStream()
    {
        return File.Open(this.filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}

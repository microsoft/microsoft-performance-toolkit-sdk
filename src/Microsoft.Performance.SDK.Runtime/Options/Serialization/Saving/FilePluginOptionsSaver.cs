// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Saving;

/// <summary>
///     Represents a class that can save a <see cref="PluginOptionsDto"/> instance to a file.
/// </summary>
public sealed class FilePluginOptionsSaver
    : StreamPluginOptionsSaver
{
    private readonly string filePath;

    public FilePluginOptionsSaver(string filePath)
        : base(true)
    {
        this.filePath = filePath;
    }

    protected override Stream GetStream()
    {
        return File.Open(this.filePath, FileMode.Create, FileAccess.Write, FileShare.None);
    }
}
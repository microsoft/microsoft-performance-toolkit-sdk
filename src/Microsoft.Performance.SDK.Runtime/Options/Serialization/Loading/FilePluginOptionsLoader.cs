// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Performance.SDK.Runtime.Options.Serialization.DTO;

namespace Microsoft.Performance.SDK.Runtime.Options.Serialization.Loading;

/// <summary>
///     Represents a class that can load a <see cref="PluginOptionsDto"/> instance from a file.
/// </summary>
public sealed class FilePluginOptionsLoader
    : StreamPluginOptionsLoader
{
    private readonly string filePath;

    public FilePluginOptionsLoader(string filePath)
        : base(true)
    {
        this.filePath = filePath;
    }

    protected override Stream GetStream()
    {
        return File.Open(this.filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
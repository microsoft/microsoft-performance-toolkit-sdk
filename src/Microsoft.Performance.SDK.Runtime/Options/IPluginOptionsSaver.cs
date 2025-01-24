using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

public interface IPluginOptionsSaver
{
    void Save();
}

public sealed class FilePluginOptionsSaver
    : IPluginOptionsSaver
{
    private readonly string filePath;
    private readonly IReadOnlyCollection<PluginOption> options;

    public FilePluginOptionsSaver(string filePath, IReadOnlyCollection<PluginOption> options)
    {
        this.filePath = filePath;
        this.options = options;
    }

    public void Save()
    {
    }
}
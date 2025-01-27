using System.Collections.Generic;
using Microsoft.Performance.SDK.Options;
using Microsoft.Performance.SDK.Processing;

namespace Microsoft.Performance.SDK.Runtime.Options;

/// <summary>
///     Provides plugin options for all <see cref="IProcessingSource"/> instances, where each
///     provided option is a clone of the original. This is to ensure that the original option instances returned
///     by the <see cref="IProcessingSource"/> instances are not modified or used by the implementing plugin.
/// </summary>
public sealed class ProcessingSourcePluginOptionsProvider
    : PluginOptionsRegistry.Provider
{
    private readonly IReadOnlyCollection<IProcessingSource> processingSources;

    public ProcessingSourcePluginOptionsProvider(IReadOnlyCollection<IProcessingSource> processingSources)
    {
        this.processingSources = processingSources;
    }

    public IEnumerable<PluginOption> GetOptions()
    {
        foreach (var processingSource in this.processingSources)
        {
            foreach (var option in processingSource.PluginOptions)
            {
                yield return option.CloneT();
            }
        }
    }
}
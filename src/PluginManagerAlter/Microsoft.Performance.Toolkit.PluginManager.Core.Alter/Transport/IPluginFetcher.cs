using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Performance.Toolkit.PluginManager.Core.Alter.Transport
{
    public interface IPluginFetcher : IPluginResource
    {
        /// <summary>
        ///     Checks if the given <paramref name="plugin"/> is supported by this fetcher.
        /// </summary>
        /// <param name="plugin">
        ///     The source this discover discovers plugins from.
        /// </param>
        /// <returns>
        ///     <c>true</c> if <paramref name="plugin"/> is supported by this fetcher. <c>false</c>
        ///     otherwise.
        /// </returns>
        bool CanFetch(AvailablePlugin plugin);

        Guid HostTypeGuid { get; }

        Task<Stream> GetPluginStreamAsync(
            AvailablePlugin plugin,
            CancellationToken cancellationToken,
            IProgress<int> progress);
    }
}

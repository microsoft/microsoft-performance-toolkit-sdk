using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Performance.SDK.Processing;
using Microsoft.Performance.Toolkit.Plugins.Core;
using Microsoft.Performance.Toolkit.Plugins.Core.Extensibility;
using Microsoft.Performance.Toolkit.Plugins.Core.Transport;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Exceptions;
using Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Transport
{
    public class PluginsFetchingManager
        : IPluginsFetchingManager
    {
        private readonly IPluginsManagerResourceRepository<IPluginFetcher> fetchersRepo;
        private readonly ILogger logger;

        public PluginsFetchingManager(
            IPluginsManagerResourceRepository<IPluginFetcher> fetcherRepo,
            ILogger logger)
        {
            this.fetchersRepo = fetcherRepo;
            this.logger = logger;
        }

        public async Task<Stream> FetchPluginStream(
            AvailablePlugin plugin,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            IPluginFetcher pluginFetcher = await TryGetPluginFetcher(plugin.AvailablePluginInfo);

            try
            {
                Stream stream = await pluginFetcher.GetPluginStreamAsync(
                    plugin.AvailablePluginInfo,
                    cancellationToken,
                    progress);

                return stream;
            }
            catch (OperationCanceledException)
            {
                this.logger.Info("Request to fetch plugin package is cancelled.");
                throw;
            }
            catch (Exception e)
            {
                string errorMsg = $"Fails to fetch plugin {plugin.AvailablePluginInfo.Identity} " +
                    $"from {plugin.AvailablePluginInfo.PluginPackageUri}";

                this.logger.Error(e, errorMsg);

                throw new PluginFetchingException(errorMsg, plugin.AvailablePluginInfo, e);
            }
        }

        private async Task<IPluginFetcher> TryGetPluginFetcher(AvailablePluginInfo availablePluginInfo)
        {
            IPluginFetcher fetcherToUse = this.fetchersRepo.Resources
                .SingleOrDefault(fetcher => fetcher.TryGetGuid() == availablePluginInfo.FetcherResourceId);

            if (fetcherToUse == null)
            {
                this.logger.Error($"Fetcher with ID {availablePluginInfo.FetcherResourceId} is not found.");
                return null;
            }

            // Validate that the found fetcher actually supports fetching the given plugin.
            Type fetcherType = fetcherToUse.GetType();
            try
            {
                bool isSupported = await fetcherToUse.IsSupportedAsync(availablePluginInfo);
                if (!isSupported)
                {
                    this.logger.Error($"Fetcher {fetcherType.Name} doesn't support fetching from {availablePluginInfo.PluginPackageUri}");
                    return null;
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"Error occurred when checking if plugin {availablePluginInfo.Identity} is supported by {fetcherType.Name}.", e);
                return null;
            }

            return fetcherToUse;
        }
    }
}

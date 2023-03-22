// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Runtime.Discovery;

namespace Microsoft.Performance.Toolkit.Plugins.Runtime.Extensibility
{
    /// <inheritdoc />
    public sealed class PluginsSystemResourceLoader
        : IPluginsSystemResourceDirectoryLoader
    {
        private readonly object mutex = new object();
        private readonly AssemblyExtensionDiscovery extensionDiscovery;

        private readonly HashSet<IPluginsSystemResourcesReferenceConsumer> subscribers;
        private readonly PluginsSystemResourceReflector resourcesReflector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginsSystemResourceLoader"/>
        /// </summary>
        /// <param name="assemblyLoader">
        ///     Loads assemblies.
        /// </param>
        /// <param name="validatorFactory">
        ///     Creates <see cref="IPreloadValidator"/> instances to make
        ///     sure candidate assemblies are valid to even try to load.
        ///     The function takes a collection of file names and returns
        ///     a new <see cref="IPreloadValidator"/> instance. This function
        ///     should never return <c>null</c>.
        /// </param>
        public PluginsSystemResourceLoader(
            IAssemblyLoader assemblyLoader,
            Func<IEnumerable<string>, IPreloadValidator> validatorFactory)
        {
            this.subscribers = new HashSet<IPluginsSystemResourcesReferenceConsumer>();
            this.extensionDiscovery = new AssemblyExtensionDiscovery(assemblyLoader, validatorFactory);
            this.resourcesReflector = new PluginsSystemResourceReflector(this.extensionDiscovery);
        }

        /// <inheritdoc />
        public bool TryLoad(string directory)
        {
            Guard.NotNull(directory, nameof(directory));

            lock (this.mutex)
            {
                var oldResources = new HashSet<PluginsSystemResourceReference>(this.resourcesReflector.AllResources);

                bool success = this.extensionDiscovery.ProcessAssemblies(directory, out ErrorInfo error);

                IEnumerable<PluginsSystemResourceReference> newResources = this.resourcesReflector.AllResources.Except(oldResources);
                NotifyResourceLoaded(newResources);

                return success;
            }
        }

        /// <inheritdoc />
        public bool Subscribe(IPluginsSystemResourcesReferenceConsumer consumer)
        {
            Guard.NotNull(consumer, nameof(consumer));

            lock (this.mutex)
            {
                if (this.subscribers.Contains(consumer))
                {
                    return false;
                }

                // Notify consumer of exisiting resources.
                consumer.OnNewResourcesLoaded(this.resourcesReflector.AllResources);

                this.subscribers.Add(consumer);

                return true;
            }
        }

        /// <inheritdoc />
        public bool Unsubscribe(IPluginsSystemResourcesReferenceConsumer consumer)
        {
            Guard.NotNull(consumer, nameof(consumer));

            lock (this.mutex)
            {
                return this.subscribers.Remove(consumer);
            }
        }

        private void NotifyResourceLoaded(IEnumerable<PluginsSystemResourceReference> resources)
        {
            foreach (IPluginsSystemResourcesReferenceConsumer subscriber in this.subscribers)
            {
                subscriber.OnNewResourcesLoaded(resources);
            }
        }
    }
}
